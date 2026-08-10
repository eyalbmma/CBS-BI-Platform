# CBS BI Platform — AI Architecture

> מסמך העומק הנבחר עבור דרישת ה־3–6 עמודים במטלת הבית.

## 1. מטרת שכבת ה־AI

מערכת CBS BI מיועדת גם למשתמשים עסקיים שאינם יודעים SQL. לכן אחד היכולות המרכזיות הוא לאפשר למשתמש לכתוב שאלה חופשית, לדוגמה:

> "איזו עיר בעלת שיעור האבטלה הנמוך ביותר?"

ולהפוך אותה באופן מבוקר לשאילתת BigQuery חוקית ומורשית.

האתגר אינו רק Natural Language → SQL. בסביבה ממשלתית יש ארבע בעיות נוספות:

1. המודל אינו מכיר באופן טבעי את מבנה הנתונים הפנימי.
2. אסור לאפשר לו להמציא tables/columns או להריץ DML/DDL.
3. שאילתת SQL יכולה להיות חוקית אך יקרה מאוד.
4. המשתמש רשאי לראות רק מידע שההרשאות שלו מאפשרות.

לכן שכבת ה־AI נבנית כ־**controlled pipeline** ולא כקריאה ישירה ל־LLM.

---

## 2. High-Level Flow

```text
Natural Language Question
        ↓
Authentication / CurrentUser
        ↓
Authorization
        ↓
Semantic Retrieval (RAG)
        ↓
Relevant Metadata + Business Dictionary
        ↓
Prompt Construction
        ↓
Vertex AI / Gemini
        ↓
Generated BigQuery SQL
        ↓
Read-only SQL Safety Validation
        ↓
BigQuery Dry Run
        ↓
Cost Guard
        ↓
MaximumBytesBilled
        ↓
Execution
        ↓
Structured Result
```

כל שלב משמש guardrail עצמאי. אין שלב יחיד שעליו נשענת כל האבטחה.

---

## 3. למה RAG נדרש

LLM כללי אינו יודע אילו tables קיימות בארגון, מה המשמעות העסקית של `UnemploymentRatePct`, כיצד מחברים בין domains או איזו שנה יש לבחור כאשר המשתמש אינו מציין שנה.

שליחת כל schema הארגוני בכל prompt אינה פתרון טוב:

- prompt גדול ויקר;
- latency גבוה יותר;
- signal-to-noise נמוך;
- חשיפה מיותרת של metadata שאינו רלוונטי;
- קשה יותר לשלוט בהרשאות לפי domain.

לכן משתמשים ב־RAG: קודם מאתרים את הידע הסמנטי הרלוונטי, ורק אותו מוסיפים ל־prompt.

---

## 4. Semantic Corpus

הקורפוס מורכב משני מקורות עיקריים.

### 4.1 Metadata

`BigQueryAnalyticsMetadataCatalog` קורא metadata אמיתי מ־BigQuery `INFORMATION_SCHEMA` ומייצר פריטי ידע עבור:

- tables;
- columns;
- data types;
- descriptions;
- fully-qualified references.

יתרון: schema changes יכולים להתגלות ללא hardcoding של כל table בתוך קוד ה־Application.

### 4.2 Business Dictionary

Metadata טכני לבדו אינו מספיק. משתמש אומר "אבטלה", לא `UnemploymentRatePct`. Business Dictionary מחבר בין vocabulary עסקי לבין fields טכניים.

דוגמאות:

```text
אוכלוסייה → city_population.Population
אבטלה → city_employment_yearly.UnemploymentRatePct
שכר → city_employment_yearly.AverageMonthlySalaryNis
מחיר דירה → city_housing_yearly.AverageApartmentPriceNis
זכאות לבגרות → city_education_yearly.MatriculationEligibilityPct
```

המילון כולל גם synonyms וכללי domain, למשל join בין domains לפי `CityCode` ו־`Year`.

---

## 5. Retrieval Strategy

### POC

ב־POC נבחר `LexicalAnalyticsSemanticRetriever`.

הוא מבצע:

- exact token match;
- controlled containment match;
- scoring דטרמיניסטי.

הבחירה מתאימה לקורפוס POC קטן מכיוון שהיא:

- פשוטה;
- זולה;
- ניתנת להסבר;
- אינה דורשת embeddings infrastructure.

דוגמה לבעיה שנחשפה בזמן runtime: המונח `עיר` לא הספיק לשאלה שהכילה `הערים`. במקום להכניס Hebrew-specific stemming ל־retriever, הורחב Business Dictionary עם `ערים`. כך ה־retriever נשאר generic והידע הלשוני נשאר בשכבת ה־semantic knowledge.

### Production

בעולם אמיתי שבו קיימים עשרות domains, מאות או אלפי tables ומילון עסקי גדול, lexical-only retrieval עלול לפספס ניסוחים סמנטיים.

לכן מוצע Hybrid Retrieval:

1. lexical matching למונחים מדויקים;
2. embeddings + vector similarity למשמעות;
3. metadata/domain filters;
4. authorization-aware filtering;
5. ranking של התוצאות לפני prompt construction.

ב־Google Cloud ניתן לשקול Vertex AI Vector Search / RAG capabilities. Vector database יתווסף **כאשר מדדי האיכות והיקף הקורפוס מצדיקים אותו**, ולא כדרישה מלאכותית.

---

## 6. Prompt Management

Prompt הוא חלק מה־application logic ולכן יש לנהל אותו כמו code/configuration ולא כטקסט אקראי בתוך controller.

ה־prompt צריך להכיל ארבעה חלקים:

1. **System rules** — BigQuery Standard SQL, read-only, החזרת SQL בלבד בפורמט מוגדר.
2. **Retrieved semantic context** — tables, columns, business terms וכללים שרלוונטיים לשאלה.
3. **Query rules** — latest year, minimum table set, join keys וכללי cost/safety.
4. **User question**.

עקרונות Prompt Management ב־Production:

- prompts versioned ב־Git;
- template ID/version נרשם ב־telemetry;
- regression tests מול set של שאלות benchmark;
- שינוי prompt עובר code review;
- אין secrets בתוך prompts;
- rollout הדרגתי של prompt/model changes כאשר נדרש.

### Minimum Table Set

כלל חשוב שהוסף ל־prompt הוא: אם כל הנתונים הדרושים נמצאים בטבלה אחת, אין לבצע JOIN מיותר.

למשל עבור "איזו עיר בעלת שיעור האבטלה הנמוך ביותר?" נדרש רק `city_employment_yearly`, ולכן אין סיבה לצרף `city_population` רק כדי לקבל `City`.

### Latest Year

אם המשתמש אינו מציין שנה, ה־SQL צריך לבחור `MAX(Year)` ולא hardcoded `2025`. כך ההתנהגות נשארת נכונה גם כשהנתונים החודשיים/שנתיים מתעדכנים.

---

## 7. Authorization Before Execution

AI אינו מחליף Authorization.

הזרימה המוצעת:

```text
Authenticated Identity
→ CurrentUser
→ Application Authorization
→ restrict semantic/data scope
→ generate query
→ validate target resources
→ BigQuery IAM / row/column controls
→ execute
```

ב־POC נבדק role בשם `AnalyticsQueryExecutor`. ללא user מתקבל `401`, וללא role מתאים מתקבל `403`.

ב־Production יש להרחיב את ההרשאה ל־data-level authorization:

- אילו domains המשתמש רשאי לשאול;
- אילו datasets/tables מותרים;
- row-level restrictions לפי ארגון/יחידה כאשר נדרש;
- column-level policy עבור שדות רגישים.

ה־RAG עצמו צריך להיות authorization-aware: אין טעם להעביר למודל metadata של table שהמשתמש ממילא אינו רשאי לקרוא.

---

## 8. AI Output is Untrusted

גם כאשר Gemini קיבל context נכון, ה־SQL המוחזר נחשב input לא מהימן.

לכן `ReadOnlyAnalyticsQuerySafetyValidator` מרשה רק query forms מתאימים (`SELECT`, `WITH`) וחוסם DML/DDL. ה־validator נדרש לזהות tokens אמיתיים ולא להיחסם ממילים שמופיעות בתוך strings/comments/backticks.

ב־Production מומלץ להוסיף גם:

- allowlist של datasets/projects;
- חסימת external tables/functions לא מאושרות;
- query plan/statement inspection לפי צורך;
- service account עם read-only permissions בלבד.

העיקרון: גם אם ה־AI טועה, identity של ה־service עצמו אינו מסוגל לבצע פעולה שאסור לו לבצע.

---

## 9. Preventing Expensive Queries

המטלה דורשת במפורש למנוע שאילתות יקרות ולהימנע משאילתות ארוכות ללא בקרה. הפתרון בנוי בכמה שכבות.

### 9.1 BigQuery Dry Run

לפני execution, ה־Backend שולח את ה־SQL ל־BigQuery Dry Run. BigQuery מאמת syntax ומחזיר estimate של bytes processed בלי להריץ את השאילתה בפועל.

### 9.2 Application Cost Guard

ה־estimate מושווה ל־`MaximumBytesProcessed` המוגדר ב־configuration. אם חורג, הבקשה נדחית לפני execution.

### 9.3 MaximumBytesBilled

גם בזמן execution מוגדר `MaximumBytesBilled`. זו שכבת הגנה נוספת במקרה שההערכה/plan משתנים.

### 9.4 Timeout

Analytics endpoints מוגבלים ל־55 שניות. הבחירה משאירה margin של חמש שניות מול דרישת "עד דקה" ומעבירה cancellation token לאורך ה־pipeline.

### 9.5 Query Design

- partition filters;
- clustering;
- Gold tables;
- minimum table set;
- limit בתרחישי Visual Query;
- בעתיד cache לשאלות נפוצות.

---

## 10. Fail-Closed Behavior

כאשר אין Semantic Context מספיק, המערכת אינה מבקשת מה־LLM "לנסות בכל זאת".

דוגמה שנבדקה:

> "מהי הטמפרטורה הממוצעת בישראל?"

אין knowledge רלוונטי בקורפוס ולכן הזרימה נעצרת ומחזירה `422` עם `analytics-query-generation-failed`.

זהו עיקרון חשוב במערכת ממשלתית:

```text
Unknown semantic domain
≠ best effort SQL

Unknown semantic domain
= controlled rejection
```

כך המערכת נמנעת מ־hallucinated tables ו־fabricated answers.

---

## 11. Model Choice and Routing

ב־POC נעשה שימוש ב־Gemini דרך Vertex AI. היתרון בסביבת Google Cloud הוא שילוב עם IAM, audit, networking ו־BigQuery ecosystem.

ב־Production לא נכון בהכרח לשלוח כל פעולה לאותו model. ניתן להשתמש ב־Model Routing:

- model מהיר/זול לשאלות פשוטות;
- model חזק יותר רק לשאלות מורכבות;
- Visual Query אינו משתמש ב־LLM כלל.

זה משפר גם עלות וגם latency.

חלופה שנשקלה: ספק LLM חיצוני. היא יכולה לספק models איכותיים, אך מוסיפה data-egress/integration/governance considerations. ההחלטה הסופית תלויה באישור אבטחת מידע, residency, contractual requirements ומדדי איכות.

---

## 12. Observability for AI

כדי לדעת אם המערכת באמת עומדת ביעדים, יש למדוד כל שלב בנפרד.

Metrics/Logs מומלצים:

- semantic retrieval latency;
- number of retrieved items;
- model name/version;
- prompt template version;
- Gemini latency;
- SQL validation failures;
- dry-run bytes estimate;
- BigQuery execution latency;
- 422 rate (unsupported semantic questions);
- timeout rate;
- total request latency.

אין לרשום raw tokens, credentials או מידע מסווג ב־logs. שאלות משתמש עשויות בעצמן להכיל מידע רגיש ולכן Production logging צריך להשתמש ב־redaction/metadata-only policy בהתאם למדיניות.

---

## 13. Cost Optimization in the AI Layer

1. **RAG context reduction** — שולחים רק knowledge רלוונטי ולא את כל schema.
2. **Model routing** — מודל זול/מהיר לשאלות פשוטות.
3. **No AI for Visual Query** — מסלול דטרמיניסטי חוסך tokens לחלוטין.
4. **Prompt compactness** — פורמט קבוע וקצר ל־metadata.
5. **Cache** — semantic retrieval / normalized repeated questions, תוך שמירה על authorization context.
6. **BigQuery cost guard** — מונע עלות downstream גם אם ה־AI ייצר SQL כבד.

---

## 14. Testing Strategy

AI system חייב להיבדק מעבר ל־unit tests רגילים.

### Unit Tests

- Authorization.
- Business Dictionary.
- Composite semantic source.
- Lexical Retriever.
- SQL safety.
- Cost guard.
- Visual Query mappings.

ה־Application test suite הגיע ל־118/118 tests passing בנקודת ה־POC הנוכחית.

### Semantic Regression Set

יש לתחזק benchmark של שאלות כגון:

- single-domain question;
- explicit year;
- latest year;
- cross-domain join;
- unsupported domain;
- synonyms/morphology;
- expensive query scenarios;
- authorization scenarios.

בכל שינוי prompt/model/retriever מריצים את ה־benchmark ומשווים accuracy, latency ו־cost.

---

## 15. POC Evidence

ה־POC הוכיח בפועל מספר תרחישים:

### Natural Language

"איזו עיר בעלת שיעור האבטלה הנמוך ביותר?"

→ single-table SQL
→ latest year
→ `Tel Aviv`.

### Housing

"באיזו עיר מחיר הדירה הממוצע הגבוה ביותר?"

→ `city_housing_yearly` בלבד.

### Education

"איזו עיר בעלת אחוז הזכאות לבגרות הגבוה ביותר?"

→ `city_education_yearly`.

### Cross-domain

שאלה המשלבת אוכלוסייה ואבטלה הובילה ל־JOIN על `CityCode` ו־`Year`.

### Unsupported Semantic Domain

שאלת טמפרטורה → `422` ללא SQL execution.

### Visual Query

Structured request:

```json
{
  "domain": "Population",
  "metric": "Population",
  "year": 2025,
  "sortDirection": "Descending",
  "limit": 5
}
```

→ deterministic SQL ללא Gemini
→ 5 הערים בעלות האוכלוסייה הגבוהה ביותר.

---

## 16. Production Evolution

המעבר מ־POC ל־Production אינו דורש החלפת הארכיטקטורה אלא חיזוק adapters ותשתיות:

```text
Development Authentication → Government Identity Provider
Lexical Retriever → Hybrid Retrieval / Vector when justified
In-memory Metadata Stores → Firestore/approved persistent store
Local ASP.NET → Cloud Run
Local config → Secret Manager
Basic ILogger → Cloud Logging/Monitoring/Audit
Synthetic Gold tables → Governed Bronze/Silver/Gold pipeline
```

ה־Application abstractions נשארות יציבות ככל האפשר.

---

## 17. Summary

הגישה המוצעת אינה "LLM שמקבל שאלה ומריץ SQL". היא pipeline מבוקר:

```text
Identity
→ Authorization
→ Retrieval
→ Controlled Prompt
→ AI
→ SQL Safety
→ Cost Estimation
→ Hard Billing Limit
→ Timed Execution
→ Result
```

RAG מקטין hallucinations ומחבר בין vocabulary עסקי ל־schema טכני. Authorization מתבצע מחוץ ל־LLM. SQL נחשב untrusted. Dry Run ו־MaximumBytesBilled מגנים על העלות. Timeout מגן על SLA. Visual Query עוקף AI לחלוטין כאשר המשתמש כבר מספק input מובנה.

כך מתקבלת AI Architecture שמתאימה יותר לסביבה ממשלתית: explainable, fail-closed, cost-aware וניתנת להרחבה.

## Official References

- Vertex AI RAG: https://docs.cloud.google.com/vertex-ai/generative-ai/docs/samples/generativeaionvertexai-rag-quickstart
- BigQuery cost controls: https://docs.cloud.google.com/bigquery/docs/best-practices-costs
- BigQuery dry run: https://docs.cloud.google.com/bigquery/docs/running-queries
- Cloud Run: https://docs.cloud.google.com/run/docs/overview/what-is-cloud-run
- IAM: https://docs.cloud.google.com/iam/docs/overview
- Cloud Audit Logs: https://docs.cloud.google.com/logging/docs/audit
