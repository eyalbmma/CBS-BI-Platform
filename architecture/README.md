# CBS BI Platform — Architecture Submission

תיקייה זו מכילה **רק את תוצרי הארכיטקטורה הנדרשים להגשה**, ללא פיצול מיותר למסמכים רבים.

## קבצים

1. **`cbs-bi-lld.png`** — Architecture Diagram / LLD מפורט של ארכיטקטורת ה־Production המוצעת על Google Cloud.
2. **`architecture-design.md`** — מסמך הארכיטקטורה הראשי: LLD explanation, Data Flow, Decision Justification, Cost Optimization, Security, Networking, Observability, Scalability, HA/DR והבדלה בין POC ל־Production.
3. **`ai-architecture.md`** — מסמך העומק הנבחר (3–6 עמודים): AI Architecture, כולל Natural Language → SQL, RAG, Prompt Management, Authorization ו־Cost/Safety controls.
4. **`README.md`** — אינדקס קצר זה.

## מיפוי ישיר לדרישות המטלה

| דרישת המטלה | היכן נענית |
|---|---|
| Architecture Diagram (LLD) | `cbs-bi-lld.png` |
| Frontend / Backend / Authentication / Networking / Load Balancing / API Layer | `cbs-bi-lld.png` + `architecture-design.md` |
| AI Layer / RAG / Vector Database לפי הצורך | `cbs-bi-lld.png` + `ai-architecture.md` |
| BigQuery / Database / Data Lake / Bronze-Silver-Gold / Storage | `cbs-bi-lld.png` + `architecture-design.md` |
| Logging / Monitoring / Alerting / Secret Management / Audit | `cbs-bi-lld.png` + `architecture-design.md` |
| CI/CD / IAM / VPC / Firewall | `cbs-bi-lld.png` + `architecture-design.md` |
| Backup / Disaster Recovery / High Availability | `architecture-design.md` |
| מסמך עומק 3–6 עמודים | `ai-architecture.md` |
| הצדקת החלטות + חלופה לכל רכיב מרכזי | `architecture-design.md` |
| לפחות 3 דרכי Cost Optimization | `architecture-design.md` |
| תרגום שפה חופשית לשאילתה / RAG / Prompts / Authorization / מניעת שאילתות יקרות | `ai-architecture.md` |
| Data Flow מקצה לקצה | `architecture-design.md` |

## POC מול Production

המסמכים מבדילים במפורש בין מה שמומש ונבדק בפועל לבין ארכיטקטורת ה־Production המוצעת.

**POC שנבדק בפועל:** React + TypeScript, .NET 8, Ask Data, RAG מבוסס Metadata + Business Dictionary + Lexical Retrieval, Gemini, SQL Safety, BigQuery Dry Run + MaximumBytesBilled, Authorization abstraction, Saved Queries, Dashboards, Visual Query דטרמיניסטי, timeout ו־118/118 Application tests.

**Production מוצע:** Government Identity Provider, External Application Load Balancer + Cloud Armor, Cloud Run, Firestore למטא־דאטה אפליקטיבי, Cloud Storage/Data Lake, Dataflow, BigQuery Gold layer, Vertex AI, Vector Search/RAG לפי היקף הקורפוס, Secret Manager, IAM, VPC controls, Logging/Monitoring/Alerting/Audit ו־CI/CD.
