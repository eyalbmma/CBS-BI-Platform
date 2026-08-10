# CBS BI Platform — Architecture Design

## 1. מטרת המערכת

המערכת מיועדת לאנליסטים, חוקרים, מנהלים ומשתמשים עסקיים שאינם נדרשים לדעת SQL. היא מאפשרת תשאול נתונים בשפה חופשית, בניית שאילתות בצורה ויזואלית, שמירת שאילתות ויצירת Dashboards. סביבת היעד היא Google Cloud, והארכיטקטורה נדרשת להתמודד עם מידע ממשלתי, הרשאות, עשרות משתמשים במקביל, טבלאות גדולות, זמן תגובה של עד דקה ובקרת עלויות.

העיקרון המרכזי הוא הפרדה בין שני מסלולי תשאול:

- **Ask Data** — שאלה בשפה חופשית עוברת RAG ו־Gemini ומומרת ל־SQL.
- **Visual Query** — בקשה מובנית (`Domain`, `Metric`, `Year`, `Sort`, `Limit`) מתורגמת דטרמיניסטית ל־SQL ללא AI.

כך AI מופעל רק במקום שבו הוא מוסיף ערך, בעוד שתרחיש מובנה נשאר צפוי, זול וקל לבקרה.

---

## 2. Architecture Overview

ה־Production המוצע מחולק לחמש שכבות:

1. **User & Edge** — משתמש, הזדהות ממשלתית, HTTPS Load Balancer, Cloud Armor.
2. **Application** — React Frontend ו־.NET 8 Backend על Cloud Run.
3. **AI & Semantic Layer** — Metadata, Business Dictionary, RAG, Vertex AI / Gemini, Vector Search אופציונלי.
4. **Data Platform** — Cloud Storage Data Lake, Bronze/Silver/Gold, Dataflow, BigQuery.
5. **Cross-cutting Operations & Security** — IAM, Secret Manager, VPC controls, Logging, Monitoring, Alerting, Audit, CI/CD, Backup/DR.

התרשים המלא נמצא ב־`cbs-bi-lld.png`.

---

## 3. User, Authentication and Authorization

### Production

המשתמש נכנס ל־CBS BI Portal ומזדהה באמצעות **מנגנון ההזדהות הממשלתי**. ההנחה הארכיטקטונית היא שהמנגנון מספק זהות וטענות הרשאה באמצעות פרוטוקול סטנדרטי מתאים (למשל OIDC/SAML, בהתאם לממשק שיסופק בפועל). ה־Backend אינו מנהל Username/Password מקומיים.

לאחר ההזדהות:

1. ה־Frontend מקבל session/token בהתאם למנגנון המאושר.
2. ה־Backend מאמת את זהות המשתמש.
3. `ICurrentUserContext` מתרגם את הזהות ל־`CurrentUser` אפליקטיבי.
4. `IAnalyticsAuthorizationService` בודק האם למשתמש מותר לבצע את הפעולה.
5. ב־Production, הרשאות מידע צריכות להיאכף גם ברמת BigQuery לפי הצורך באמצעות IAM, Authorized Views, Row Access Policies ו/או Policy Tags.

### POC

האינטגרציה הממשלתית עצמה אינה ממומשת. במקום זאת קיים `DevelopmentAuthenticationHandler`, וה־Frontend שולח headers כגון `X-Dev-UserId` ו־`X-Dev-Roles`. מנגנון זה נועד רק לדמות identity ב־Development.

ה־POC כן הוכיח את גבול ההרשאה:

- ללא משתמש מאומת → `401`.
- משתמש ללא role מתאים → `403`.
- משתמש עם `AnalyticsQueryExecutor` → הפעולה מורשית.

המשמעות היא שב־Production ניתן להחליף את Authentication Adapter בלי לשנות את ה־Application use cases.

---

## 4. Frontend

### בחירה

**React + TypeScript**, כ־SPA סטטי. ב־Production ניתן להגיש את ה־build מ־Cloud Storage מאחורי External Application Load Balancer ו־Cloud CDN לפי הצורך.

### אחריות

- Ask Data.
- Saved Queries.
- Dashboards.
- Visual Query.
- הצגת Result Table / KPI / Bar Chart.
- טיפול ב־401/403/422/504/Network errors.

ה־Frontend אינו מחזיק SQL generation logic של Ask Data ואינו מקבל החלטות הרשאה. כל החלטה רגישה מתבצעת ב־Backend.

### חלופה שנשקלה

**Server-side rendered application**. נפסלה ל־POC ולמערכת BI אינטראקטיבית זו משום ש־SPA מתאים יותר לאינטראקציה עשירה, וה־Backend כבר מוגדר כ־REST API נפרד.

---

## 5. API / Backend

### בחירה

**ASP.NET Core .NET 8** בתוך container על **Cloud Run**.

Cloud Run מתאים לעשרות משתמשים במקביל ומספק scaling מנוהל ללא צורך בניהול cluster. ניתן להגדיר minimum instances כדי לצמצם cold starts ו־maximum instances כדי לשלוט בעלויות ובהשפעה על מערכות downstream.

### מבנה לוגי

```text
CBS.BI.Api
    ↓
CBS.BI.Application
    ↑
CBS.BI.Infrastructure

CBS.BI.Domain
```

עקרונות:

- Controllers דקים.
- Business logic ב־Application.
- External services מאחורי abstractions.
- Infrastructure מממש interfaces של Application.
- Application אינו תלוי ב־Infrastructure.

### חלופה שנשקלה

**GKE**. הוא מספק שליטה מלאה ב־Kubernetes, אך עבור עומס של עשרות משתמשים ומערכת API אחת הוא מוסיף עלות תפעולית, cluster management ומורכבות ללא ערך מספק. אם בעתיד המערכת תהפוך לפלטפורמה של עשרות microservices עם דרישות networking מיוחדות, ניתן לשקול GKE מחדש.

---

## 6. Edge, Load Balancing and Firewall

הכניסה למערכת מתבצעת דרך **Global/External Application Load Balancer** ב־HTTPS בלבד. לפני השירותים מוצב **Cloud Armor** לצורך WAF, הגנת DDoS, rate limiting וחסימת דפוסים זדוניים.

ה־Load Balancer מנתב traffic ל־Frontend ול־Backend. ה־Backend אינו נדרש להיות חשוף ישירות ל־Internet מעבר לנתיב המאושר.

ב־VPC מוגדרים כללי egress/ingress מינימליים. גישה לשירותי Google הרגישים מוגבלת באמצעות IAM וב־Production מומלץ לשקול **VPC Service Controls** סביב BigQuery, Cloud Storage ו־Vertex AI בהתאם למדיניות אבטחת המידע הממשלתית.

### חלופה

חשיפה ישירה של Cloud Run ללא Load Balancer. זולה ופשוטה יותר, אך מפחיתה שליטה אחידה על WAF, custom domain, rate limiting ו־edge security ולכן אינה הבחירה המועדפת לסביבה ממשלתית.

---

## 7. Ask Data — Natural Language Query Flow

זהו המסלול שבו המשתמש כותב שאלה חופשית.

```text
Question
→ Authentication / CurrentUser
→ Authorization
→ Semantic Retrieval
→ Metadata + Business Dictionary
→ Relevant Context
→ Gemini
→ Generated SQL
→ Read-only SQL Safety
→ BigQuery Dry Run
→ Cost Guard
→ MaximumBytesBilled
→ BigQuery Execution
→ Result
→ React
```

עקרונות בטיחות:

- אין semantic context מספק → `422`, ללא קריאה ל־BigQuery.
- SQL של Gemini נחשב input לא מהימן.
- מותרות רק שאילתות קריאה (`SELECT` / `WITH`).
- Dry Run מתבצע לפני execution.
- אם estimated bytes חורגים מהסף → השאילתה נדחית.
- גם execution עצמו מוגבל באמצעות `MaximumBytesBilled`.
- Request timeout מוגדר ל־55 שניות כדי להשאיר margin מול דרישת הדקה.

---

## 8. Visual Query Flow

Visual Query עונה ישירות על הדרישה "יצירת שאילתות בצורה ויזואלית ללא SQL".

המשתמש בוחר לדוגמה:

```text
Domain: Population
Metric: Population
Year: 2025
Sort: Highest
Limit: 5
```

ה־Frontend שולח DTO מובנה:

```json
{
  "domain": "Population",
  "metric": "Population",
  "year": 2025,
  "sortDirection": "Descending",
  "limit": 5
}
```

ה־Backend ממפה את הערכים דרך allowlist סגור של domains, tables ו־columns. אין הכנסת table/column names חופשיים מהלקוח. `Descending` ממופה ל־`DESC`, ו־limit/year הם integers שעוברים validation.

```text
Structured Request
→ Validation
→ Authorization
→ Server-side Allowlist Mapping
→ Deterministic SQL
→ SQL Safety
→ Dry Run / Cost Guard
→ BigQuery
→ Result
```

אין שימוש ב־Gemini במסלול זה, ולכן הוא צפוי וזול יותר.

---

## 9. RAG and Semantic Layer

### POC

ה־POC כולל:

- Metadata Knowledge Source — Metadata אמיתי מ־BigQuery `INFORMATION_SCHEMA`.
- Business Dictionary — מונחים עסקיים, synonyms ו־mapping לשדות.
- Composite Semantic Corpus.
- Lexical Retriever עם exact ו־controlled containment matching.

דוגמאות למונחים: אוכלוסייה, עיר, אבטלה, מועסקים, שכר, מחיר דירה, שכירות, תלמידים, זכאות לבגרות, מורים ושנה.

### Production

כאשר הקורפוס גדל לעשרות domains, מאות/אלפי tables ומילון עסקי גדול, מומלץ לעבור ל־**Hybrid Retrieval**:

- keyword/lexical retrieval למונחים מדויקים;
- embeddings + vector retrieval למשמעות סמנטית;
- metadata filters לפי domain והרשאות.

**Vector Database אינו נדרש רק כדי לסמן checkbox.** הוא יתווסף כאשר מדדי recall/precision וגודל הקורפוס מצדיקים זאת. ב־Google Cloud ניתן לשקול Vertex AI Vector Search / RAG capabilities.

---

## 10. Data Platform: Data Lake + Bronze / Silver / Gold

### Bronze

**Cloud Storage** שומר raw immutable data מהמקורות, עם versioning/retention לפי מדיניות. המידע נשמר כפי שהתקבל לצורכי traceability ו־reprocessing.

### Silver

**Dataflow** מבצע validation, normalization, deduplication, schema alignment והתממה/בדיקות איכות. תוצרי Silver נשמרים במבנה נקי ומבוקר, ב־Cloud Storage ו/או BigQuery לפי סוג הנתונים.

### Gold

**BigQuery** מכיל טבלאות curated המותאמות ל־BI ולשאילתות. ב־POC קיימים domains לדוגמה:

- `city_population`
- `city_employment_yearly`
- `city_housing_yearly`
- `city_education_yearly`

Production ישתמש ב־partitioning, clustering, curated views ו־data governance כדי לצמצם bytes scanned ולבודד את ה־analytics layer ממבני raw ingestion.

### חלופה

Cloud SQL מתאים ל־OLTP וליחסים טרנזקציוניים אך אינו הבחירה המרכזית לסריקות אנליטיות של עשרות מיליוני רשומות. BigQuery מתאים יותר ל־analytical workloads, columnar scanning ו־elastic compute.

---

## 11. Saved Queries and Dashboards Metadata

ב־POC המידע נשמר ב־in-memory stores. זה נעשה בכוונה כדי להוכיח abstraction ו־user isolation בלי להוסיף persistence שאינו מרכז המטלה.

ב־Production מומלץ להשתמש ב־**Firestore** עבור metadata אפליקטיבי כגון Saved Queries, Dashboard definitions ו־Widget configuration:

- serverless;
- low operational overhead;
- מתאים למסמכי configuration קטנים;
- scale לפי שימוש.

חלופה: **Cloud SQL PostgreSQL**, שתיבחר אם נדרשות טרנזקציות מורכבות, relational joins או constraints משמעותיים בין entities.

Dashboard widgets מפנים ל־Saved Query. ב־POC ה־Frontend מריץ widgets במקביל באמצעות ה־analytics API ומבודד כישלון של widget אחד משאר ה־Dashboard.

---

## 12. Security and IAM

עקרון מרכזי: **Least Privilege + Defense in Depth**.

- Government Identity Provider לזיהוי משתמשים.
- Application authorization לפני analytics execution.
- Service Account נפרד ל־Cloud Run.
- הרשאות BigQuery מינימליות ל־datasets הנדרשים בלבד.
- Secret Manager לסודות/configuration רגישים.
- Cloud Armor להגנת edge.
- TLS בלבד.
- BigQuery IAM / Row-Level / Column-Level controls לפי עולם התוכן.
- Cloud Audit Logs לתיעוד פעולות ניהול וגישה בהתאם ליכולות השירותים.
- אין logging של tokens, credentials או raw classified data.
- VPC Service Controls מומלץ סביב data services כאשר נדרש perimeter למידע רגיש.

---

## 13. Logging, Monitoring, Alerting and Audit

### Logging

Cloud Logging מרכז structured logs מה־Backend. ב־POC כבר נמדדים שלבים כגון:

- Semantic Retrieval latency.
- Gemini SQL Generation latency.
- BigQuery Dry Run latency.
- BigQuery Execution latency.
- Total Ask latency.

### Monitoring

Cloud Monitoring יקבל metrics כגון:

- p50/p95 request latency.
- שיעורי 401/403/422/5xx/504.
- BigQuery estimated/processed bytes.
- Gemini latency/error rate.
- Cloud Run instance count/concurrency.

### Alerts

Alerts לדוגמה:

- p95 מתקרב ל־60 שניות.
- spike ב־5xx/504.
- עלייה חריגה ב־bytes processed או בעלות.
- failures ב־data pipeline.
- unauthorized access anomalies.

### Audit

Cloud Audit Logs נשמרים בנפרד מ־application logs. לצורכי business audit ניתן לשמור מי הריץ פעולה, מתי, באיזה domain ומה סטטוס הפעולה — ללא שמירת מידע רגיש שאינו נדרש.

---

## 14. CI/CD and Secret Management

Pipeline מוצע:

```text
GitHub
→ Cloud Build trigger
→ Unit Tests
→ Build Frontend / Backend
→ Container image
→ Artifact Registry
→ Security checks
→ Deploy Cloud Run revision
→ Smoke Test
→ Traffic promotion
```

Infrastructure מנוהל באמצעות Terraform כדי למנוע drift ולאפשר review של שינויים.

Secrets אינם נשמרים ב־Git. Cloud Run מקבל secrets מ־Secret Manager באמצעות Service Account ייעודי.

חלופה: GitHub Actions ישירות. היא אפשרית, אך Cloud Build משתלב באופן טבעי עם Google Cloud IAM, Artifact Registry ו־deployment. אם משתמשים ב־GitHub Actions, יש להעדיף Workload Identity Federation על פני service-account keys קבועים.

---

## 15. Performance, Scalability and Query Control

דרישת המטלה היא זמן תשובה של עד דקה ואי־אפשרות להשאיר שאילתות ארוכות ללא בקרה.

המענה הארכיטקטוני:

1. Cloud Run autoscaling לעומס HTTP.
2. Request timeout של 55 שניות ב־analytics endpoints.
3. BigQuery Dry Run לפני execution.
4. MaximumBytesBilled.
5. partitioning/clustering.
6. minimum table set ב־AI prompt כדי למנוע joins מיותרים.
7. הגבלת Visual Query ל־allowlisted metrics ו־limit מקסימלי.
8. בעתיד: result/query cache לשאלות נפוצות.

Runtime POC שנמדד עבור Ask Data היה בסדר גודל של שניות בודדות, מתחת משמעותית למגבלת הדקה; המדידה עצמה אינה SLA של Production אלא הוכחת feasibility.

---

## 16. High Availability, Backup and Disaster Recovery

### High Availability

- Cloud Run הוא managed service ומסוגל להריץ מספר instances בהתאם לעומס.
- BigQuery ו־Cloud Storage הם managed services; היישום אינו מנהל database servers בעצמו.
- Frontend סטטי יכול להיות מוגש דרך Load Balancer/CDN.

### Backup

- Cloud Storage: versioning/retention policy לפי classification.
- Firestore/metadata store: scheduled backup / PITR בהתאם ל־RPO שנקבע.
- Source code + Terraform ב־Git.
- BigQuery: snapshots/copies/exports בהתאם למדיניות retention.

### Disaster Recovery

Primary deployment מומלץ באזור המתאים לדרישות residency, למשל `me-west1` כאשר המדיניות מאפשרת. DR בין אזורים/פרויקטים יתוכנן רק בהתאם למדיניות מידע ממשלתית: אין להעתיק classified data לאזור אחר ללא אישור.

יש להגדיר עסקית:

- **RPO** — כמה נתונים מותר לאבד.
- **RTO** — תוך כמה זמן השירות חייב לחזור.

בתרחיש DR: redeploy באמצעות Terraform/CI, restore metadata, activate replicated/backed-up datasets בהתאם למדיניות, ואז DNS/traffic failover.

---

## 17. Decision Justification

| רכיב | בחירה | חלופה | למה הבחירה מתאימה | חסרון / Trade-off |
|---|---|---|---|---|
| Compute | Cloud Run | GKE | Managed, autoscaling, נמוך בתפעול | פחות שליטה ב־orchestration |
| Analytics DB | BigQuery | Cloud SQL | מותאם ל־analytics ולסריקות גדולות | עלות לפי bytes/slots מחייבת governance |
| Data Lake | Cloud Storage | BigQuery בלבד | Raw immutable, זול, מתאים Bronze | דורש pipeline ו־governance |
| AI | Vertex AI / Gemini | LLM חיצוני | אינטגרציה עם GCP/IAM ו־data platform | model dependency ועלות tokens |
| RAG POC | Lexical + Dictionary | Vector DB | פשוט, שקוף ומתאים לקורפוס קטן | recall יורד בקורפוס גדול |
| RAG Production | Hybrid + Vector לפי צורך | Lexical בלבד | semantic recall בקנה מידה גדול | מורכבות ועלות embeddings/index |
| App metadata | Firestore | Cloud SQL | serverless, פשוט למסמכי config | relational queries מוגבלות |
| Edge | External ALB + Cloud Armor | Cloud Run public URL | WAF, routing, rate controls | עלות/קונפיגורציה נוספת |
| CI/CD | Cloud Build + Artifact Registry | GitHub Actions בלבד | native GCP integration | coupling ל־GCP |
| Visual Query | Deterministic allowlist | Gemini | צפוי, מאובטח וזול יותר | פחות גמיש משפה חופשית |

---

## 18. Cost Optimization

המטלה דורשת לפחות שלוש דרכים; הארכיטקטורה מציעה שש:

### 1. BigQuery Dry Run + MaximumBytesBilled

לפני execution מתקבל estimate של bytes scanned. אם ההערכה חורגת מהסף, הבקשה נדחית. בנוסף `MaximumBytesBilled` הוא guard נוסף בזמן ההרצה.

### 2. Partitioning + Clustering

טבלאות yearly מחולקות לפי `Year` ומקובצות לפי `CityCode` כאשר מתאים. שאילתות מסננות partitions במקום לסרוק dataset מלא.

### 3. Gold Layer + Minimum Table Set

ה־AI מודרך להשתמש במספר הטבלאות המינימלי. Gold tables מותאמות לשימוש עסקי ומקטינות joins וסריקות מיותרות.

### 4. Limit Semantic Context

RAG שולח ל־Gemini רק metadata/business terms רלוונטיים ולא את כל הקטלוג. זה מקטין tokens, latency ועלות.

### 5. Model Routing

ב־Production ניתן להפנות פעולות פשוטות למודל זול/מהיר יותר ולהשתמש במודל חזק יותר רק עבור שאלות מורכבות.

### 6. Cache

Cache לתוצאות של שאלות נפוצות או semantic retrieval יכול לצמצם קריאות חוזרות ל־Gemini ול־BigQuery. יש להגדיר TTL ו־cache key שכולל user/data-access context כדי לא לדלוף מידע בין משתמשים.

---

## 19. End-to-End Data Flow

### 19.1 Ingestion Flow

```text
Government / Source Systems
→ Secure ingestion
→ Cloud Storage Bronze
→ Dataflow validation/transformation
→ Silver
→ BigQuery Gold
→ INFORMATION_SCHEMA / Business Dictionary
→ Semantic Corpus
```

### 19.2 Ask Data Flow

```text
User
→ Government Authentication
→ React
→ Load Balancer / Cloud Armor
→ .NET API on Cloud Run
→ CurrentUser + Authorization
→ RAG
→ Gemini
→ SQL Safety
→ Dry Run + Cost Guard
→ BigQuery Gold
→ Result
→ React
```

### 19.3 Visual Query Flow

```text
User selections
→ React
→ .NET API
→ Authorization
→ Allowlisted deterministic query builder
→ SQL Safety
→ Dry Run + Cost Guard
→ BigQuery Gold
→ Result
```

### 19.4 Dashboard Flow

```text
User opens Dashboard
→ Dashboard definition
→ Saved Query references
→ analytics executions in parallel
→ per-widget result/error isolation
→ Number / Table / BarChart
```

---

## 20. POC vs Production

| נושא | POC קיים | Production מוצע |
|---|---|---|
| Authentication | Development headers/handler | Government Identity Provider |
| Saved Queries/Dashboards | In-memory | Firestore / approved persistent store |
| RAG | Lexical | Hybrid + embeddings/vector לפי scale |
| Data | Synthetic demo tables | governed Bronze/Silver/Gold pipelines |
| Frontend hosting | Local Vite | Cloud Storage/CDN/Load Balancer |
| Backend hosting | Local ASP.NET | Cloud Run |
| Secrets | Local development config | Secret Manager |
| Observability | ILogger + timings | Cloud Logging/Monitoring/Alerting/Audit |
| DR | לא ממומש | RPO/RTO + backup/restore + approved cross-region strategy |

הפרדה זו חשובה: ההגשה אינה טוענת שמומשו רכיבי Production שלא נבנו בפועל; היא מציגה POC עובד לצד יעד ארכיטקטוני מלא.

---

## 21. Official Google Cloud References

- Cloud Run: https://docs.cloud.google.com/run/docs/overview/what-is-cloud-run
- Cloud Run autoscaling: https://docs.cloud.google.com/run/docs/about-instance-autoscaling
- External Application Load Balancer: https://docs.cloud.google.com/load-balancing/docs/https
- Cloud Armor: https://cloud.google.com/security/products/armor
- BigQuery cost controls: https://docs.cloud.google.com/bigquery/docs/best-practices-costs
- BigQuery dry run: https://docs.cloud.google.com/bigquery/docs/running-queries
- Vertex AI RAG: https://docs.cloud.google.com/vertex-ai/generative-ai/docs/samples/generativeaionvertexai-rag-quickstart
- Cloud Storage: https://docs.cloud.google.com/storage/docs/introduction
- IAM: https://docs.cloud.google.com/iam/docs/overview
- Secret Manager: https://docs.cloud.google.com/secret-manager/docs/overview
- Cloud Logging: https://docs.cloud.google.com/logging/docs/overview
- Cloud Monitoring: https://docs.cloud.google.com/monitoring/docs/monitoring-overview
- Cloud Audit Logs: https://docs.cloud.google.com/logging/docs/audit
