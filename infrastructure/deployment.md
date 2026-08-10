# CBS BI Platform — Production Deployment Design

## 1. Scope

מסמך זה מתאר את דרך הפריסה המוצעת ל-CBS BI Platform ב-Google Cloud.

הוא משלים את מסמכי `architecture/` ומתמקד ב:

- deployment topology;
- network path;
- frontend/backend hosting;
- identity boundary;
- IAM;
- secrets;
- CI/CD;
- logging/monitoring;
- rollout;
- high availability;
- backup/disaster recovery.

המסמך מבדיל בין **POC שמומש בפועל** לבין **Production target**.

---

## 2. Environment Strategy

מומלץ להפריד environments לפחות ל:

```text
Development
Test / Staging
Production
```

בארגון ממשלתי ניתן להפריד גם ברמת Google Cloud projects:

```text
cbs-bi-dev
cbs-bi-staging
cbs-bi-prod
```

היתרונות:

- blast radius קטן יותר;
- IAM נפרד;
- quotas/budgets נפרדים;
- secrets נפרדים;
- logging/audit ברורים;
- אין ערבוב demo/test data עם Production.

ה-POC הנוכחי משתמש בפרויקט:

```text
cbs-bi-poc
```

וב-dataset:

```text
analytics_demo
```

זה אינו שם Production.

---

## 3. Request Path

### 3.1 User Entry

```text
Browser
  ↓ HTTPS
External Application Load Balancer
  ↓
Cloud Armor
  ↓
Frontend / API routing
```

ה-Load Balancer הוא נקודת הכניסה ל-HTTP(S).

Cloud Armor מוסיף שכבת הגנה ב-edge:

- DDoS protection;
- WAF policies;
- rate limiting;
- allow/deny rules לפי policy.

### 3.2 Authentication

המערכת אינה מייצרת משתמשים מקומיים.

```text
Unauthenticated user
      ↓
Government Identity Provider
      ↓
Successful authentication
      ↓
identity/session/token according to approved integration
      ↓
CBS BI
```

הפרוטוקול המדויק (למשל OIDC/SAML או ממשק ממשלתי ייעודי) ייקבע לפי ה-Identity Provider בפועל ולא מומצא במסגרת ה-POC.

Backend flow:

```text
Authenticated principal
      ↓
.NET Authentication adapter
      ↓
ICurrentUserContext
      ↓
CurrentUser
      ↓
IAnalyticsAuthorizationService
```

ב-Development בלבד קיימים `X-Dev-UserId` ו-`X-Dev-Roles`.

---

## 4. Frontend Deployment

### Recommended target

```text
React production build
      ↓
Cloud Storage
      ↓
Backend bucket
      ↓
Cloud CDN
      ↓
External HTTPS Application Load Balancer
```

ה-Frontend הוא static bundle ולכן אינו דורש container runtime רק לצורך הגשת HTML/CSS/JS.

יתרונות:

- managed hosting;
- CDN caching;
- deployment פשוט;
- פחות compute cost;
- הפרדה בין static frontend לבין Backend API.

Alternative:

- Firebase Hosting יכול להיות חלופה נוחה;
- Cloud Run frontend אפשרי, אך מוסיף runtime שאין בו צורך עבור SPA סטטי.

---

## 5. Backend Deployment

### Recommended target

```text
.NET 8 application
     ↓
container image
     ↓
Artifact Registry
     ↓
Cloud Run service
```

Cloud Run נבחר עבור Backend HTTP stateless בגלל:

- autoscaling;
- managed runtime;
- scale-down כאשר אין עומס;
- אין צורך לנהל Kubernetes cluster;
- מתאים לעשרות משתמשים במקביל ולגידול עתידי.

Production configuration יכלול:

- request concurrency בהתאם למדידות;
- min instances רק אם latency/cold-start מצדיקים עלות;
- max instances כדי להגן על downstream services ועל budget;
- request timeout התואם SLA עסקי;
- resource limits לפי profiling.

ב-Application כבר קיים analytics timeout של 55 שניות כדי להשאיר margin מתחת לדרישת דקה.

---

## 6. Backend Ingress

Production policy מומלצת:

```text
Internet
   ↓
External HTTPS Application Load Balancer
   ↓
Cloud Run
```

אין להסתמך על Cloud Run public URL כמסלול עוקף.

ב-Production יש להגביל ingress בהתאם למודל ה-deployment המאושר כדי שהגישה תעבור דרך שכבת ה-edge המוגנת.

---

## 7. Networking / VPC / Firewall

### VPC

VPC משמש ל:

- private connectivity;
- egress control;
- segmentation;
- connectivity לשירותים פרטיים/ארגוניים;
- integration עתידי עם government/on-prem networks.

Cloud Run מתחבר ל-VPC כאשר נדרשת גישה פרטית למשאב VPC.

### Firewall

עקרונות:

1. default deny כאשר אפשר;
2. allow רק source/destination/port נדרשים;
3. אין פתיחת management ports לציבור;
4. אין גישה ישירה ל-databases פרטיים מה-Internet;
5. logging של firewall rules רלוונטיים לפי מדיניות.

### Managed Google Services

BigQuery, Cloud Storage ו-Vertex AI הם managed services.  
כאשר רמת הסיווג מחייבת data-exfiltration perimeter, יש לבחון controls נוספים כגון VPC Service Controls ו-private connectivity patterns בהתאם לשירות ולמדיניות.

---

## 8. Data Platform Connectivity

Analytics path:

```text
Cloud Run Backend
       ↓
authorized Google Cloud identity
       ↓
BigQuery Gold datasets
```

Data ingestion path:

```text
Source Systems
     ↓
Cloud Storage Data Lake
     ↓
Bronze
     ↓
processing / validation
     ↓
Silver
     ↓
business transformations
     ↓
Gold
     ↓
BigQuery analytics tables
```

ה-POC הנוכחי משתמש ב-BigQuery tables סינתטיות ואינו מממש pipeline מלא.

Production location/data residency חייבים להיקבע לפי מדיניות המידע.  
העובדה שה-POC משתמש ב-`me-west1` אינה מחליפה החלטת residency רשמית.

---

## 9. IAM Design

### Runtime identities

מומלץ Service Account נפרד לכל workload.

לדוגמה:

```text
cbs-bi-backend-runtime
cbs-bi-build
cbs-bi-deployer
```

אין להשתמש ב-Service Account אחד עם Owner/Editor לכל המערכת.

### Backend permissions

ה-Backend יקבל רק הרשאות נדרשות, כגון:

- execute BigQuery jobs;
- read approved datasets/views;
- invoke required Vertex AI capabilities;
- read specific Secret Manager secrets.

Production data access צריך להיות מצומצם לפי domain/role וניתן לשלב:

- dataset/table IAM;
- authorized views;
- row-level policies;
- column/policy-tag controls;

בהתאם למודל ההרשאות העסקי.

---

## 10. Secret Management

Secrets נשמרים ב-Secret Manager.

Pipeline/runtime יקבלו secrets באמצעות IAM ולא באמצעות קובץ credentials שמועלה ל-repository.

לא שומרים ב-Git:

```text
service-account.json
API keys
access tokens
passwords
private certificates
connection credentials
```

יש להגדיר rotation בהתאם לסוג secret ולמדיניות.

---

## 11. CI/CD

### Backend pipeline

```text
Git push / pull request
      ↓
Cloud Build
      ↓
restore + build
      ↓
unit tests
      ↓
container build
      ↓
Artifact Registry
      ↓
security/policy gates
      ↓
deploy Cloud Run
      ↓
smoke test
```

### Frontend pipeline

```text
Git push / pull request
      ↓
Cloud Build
      ↓
npm clean install
      ↓
lint / test / build
      ↓
static artifact
      ↓
Cloud Storage deployment
      ↓
CDN cache handling
      ↓
smoke test
```

### Production promotion

Production deployment צריך להיות מבוקר.

אפשר להשתמש ב:

- separate production trigger;
- manual approval;
- protected Git branch;
- deployment service account נפרד.

אין לתת ל-build identity הרשאות רחבות מעבר לנדרש.

---

## 12. Configuration

Configuration מתחלקת ל:

### Non-secret config

לדוגמה:

```text
BigQuery dataset ID
location
timeout seconds
maximum bytes processed
allowed frontend origin
model identifier
```

מגיעה מ-environment configuration.

### Secrets

מגיעים מ-Secret Manager בלבד.

אין לקודד environment-specific values בתוך source code כאשר ניתן להגדירם בפריסה.

---

## 13. Observability

### Application telemetry

ה-POC כבר מודד:

```text
SemanticRetrieval
GeminiSqlGeneration
BigQueryDryRun
BigQueryExecution
AskQuestion Total
```

ב-Production מדדים אלה מועברים ל-observability platform.

### Logs

Cloud Logging:

- structured application logs;
- request correlation ID;
- severity;
- component;
- duration;
- controlled error metadata.

אין לרשום:

- raw secrets;
- credentials;
- full access tokens;
- sensitive personal content ללא הצדקה ומדיניות.

### Metrics

Cloud Monitoring:

- request count;
- p50/p95/p99 latency;
- error rate;
- 401/403/422/504 counts;
- Cloud Run instance/concurrency metrics;
- BigQuery failures;
- analytics timeout rate;
- AI latency;
- cost-guard rejection count.

### Alerts

דוגמאות:

```text
5xx rate above threshold
p95 latency approaching 60 seconds
repeated 504 timeouts
BigQuery execution failures
Cloud Run unhealthy/deployment failure
unusual authorization failures
budget/cost anomaly
```

### Audit

Cloud Audit Logs ישמשו לאירועי control-plane/data-access בהתאם למדיניות.

Application audit trail צריך לתעד אירועים עסקיים רלוונטיים כגון:

- מי הריץ פעולה;
- איזה domain;
- מתי;
- האם אושרה/נדחתה;
- מזהה correlation;

בלי לרשום מידע רגיש מעבר לנדרש.

---

## 14. Availability and Scalability

### Cloud Run

Cloud Run מספק autoscaling ל-Backend stateless.

מומלץ להגדיר:

- max instances כדי להגן על Gemini/BigQuery/quota/cost;
- min instances רק אם נדרש latency יציב;
- concurrency לאחר load test.

### BigQuery

BigQuery הוא managed analytics platform ולכן אין צורך לנהל DB servers.

Query governance נשאר חובה:

- partition filtering;
- clustering;
- Gold-layer models;
- dry run;
- MaximumBytesBilled;
- query timeout;
- caching לפי צורך.

### Frontend

Static assets מופצים דרך CDN ולכן scale של frontend מופרד מ-scale של API.

---

## 15. Backup

### Source code

Git repository הוא מקור הקוד, אך אינו תחליף ל-backup policy ארגונית.

### Data Lake

Cloud Storage:

- retention policies לפי classification;
- object versioning כאשר מתאים;
- lifecycle policies;
- replicated/dual-region strategy רק אם residency מאפשרת.

### BigQuery

בהתאם ל-RPO:

- time travel / snapshots;
- table copies;
- export critical datasets;
- tested restore procedure.

### Application metadata

Saved Queries / Dashboards עוברים ב-Production מ-In-Memory ל-persistent store.

ל-store הנבחר יש להגדיר:

- backup;
- PITR כאשר נתמך ונדרש;
- retention;
- restore test.

---

## 16. Disaster Recovery

DR אינו "עוד region" אוטומטי.

קודם מגדירים:

```text
RPO = כמה מידע מותר לאבד
RTO = כמה זמן השירות יכול להיות לא זמין
```

אחר כך בוחרים strategy המתאימה למדיניות מידע ממשלתית.

אפשרות יעד:

```text
Primary environment
      ↓ failure
detect / alert
      ↓
deploy/activate secondary approved environment
      ↓
restore/attach approved data copies
      ↓
traffic failover
      ↓
validation
```

אין להעתיק מידע מסווג לאזור/מדינה אחרים בלי אישור residency/security.

Infrastructure/configuration צריכים להיות reproducible כדי לאפשר redeployment מהיר.

---

## 17. High Availability

HA מתקבלת משילוב:

- External Application Load Balancer;
- stateless Cloud Run service עם מספר instances לפי עומס;
- Cloud Storage/CDN עבור frontend;
- managed BigQuery;
- managed Secret Manager;
- monitored deployment;
- no local process state as Production source of truth.

זו גם הסיבה שב-Production אין להשתמש ב-InMemory stores עבור Saved Queries/Dashboards.

---

## 18. Security Gates Before Analytics Execution

Production deployment אינו מבטל את controls שכבר קיימים ב-Application:

### Ask Data

```text
Authentication
 ↓
Authorization
 ↓
Semantic retrieval
 ↓
Gemini
 ↓
Read-only SQL validation
 ↓
BigQuery Dry Run
 ↓
Cost policy
 ↓
MaximumBytesBilled
 ↓
Execution
```

### Visual Query

```text
Authentication
 ↓
Authorization
 ↓
structured request validation
 ↓
server-side allowlist
 ↓
deterministic SQL
 ↓
Read-only SQL validation
 ↓
Dry Run / Cost Guard
 ↓
Execution
```

---

## 19. Release Strategy

מומלץ:

1. deploy לגרסת Staging;
2. run smoke tests;
3. validate authentication/authorization;
4. test representative Ask Data queries;
5. test Visual Query;
6. verify 422/504 flows;
7. review logs/metrics;
8. approve Production;
9. deploy new Cloud Run revision;
10. monitor after release;
11. rollback revision if necessary.

---

## 20. What Is Actually Implemented Today

### Runtime-verified POC

- React + TypeScript frontend;
- .NET 8 backend;
- Development Authentication;
- authorization abstraction;
- Ask Data;
- RAG — Metadata + Business Dictionary + lexical retrieval;
- Gemini SQL generation;
- read-only SQL safety;
- BigQuery Dry Run;
- MaximumBytesBilled;
- timeout;
- Saved Queries;
- Dashboards;
- Visual Query deterministic backend;
- BigQuery execution;
- 118/118 Application tests passing.

### Production design only

- Government Authentication integration;
- External Application Load Balancer;
- Cloud Armor;
- Cloud Run deployment;
- Cloud Storage/CDN frontend hosting;
- persistent Saved Query/Dashboard storage;
- production VPC/firewall controls;
- Secret Manager integration;
- Cloud Build/Artifact Registry pipeline;
- full Cloud Logging/Monitoring/Alerting/Audit;
- production backup/DR environment.

ההפרדה הזאת מכוונת: ההגשה מציגה POC אמיתי לצד Architecture מלאה, בלי לטעון שנפרסו רכיבים שלא מומשו.

---

## 21. Official Google Cloud References

- Cloud Run: https://docs.cloud.google.com/run/docs/overview/what-is-cloud-run
- Cloud Run autoscaling: https://docs.cloud.google.com/run/docs/about-instance-autoscaling
- Cloud Load Balancing: https://docs.cloud.google.com/load-balancing/docs/load-balancing-overview
- External Application Load Balancer: https://docs.cloud.google.com/load-balancing/docs/https
- Cloud Armor: https://docs.cloud.google.com/armor/docs/cloud-armor-overview
- Cloud Armor security policies: https://docs.cloud.google.com/armor/docs/security-policy-overview
- VPC: https://docs.cloud.google.com/vpc/docs/overview
- IAM: https://docs.cloud.google.com/iam/docs/overview
- Secret Manager: https://docs.cloud.google.com/secret-manager/docs/overview
- Cloud Logging: https://docs.cloud.google.com/logging/docs/overview
- Cloud Audit Logs: https://docs.cloud.google.com/logging/docs/audit
- Cloud Build: https://docs.cloud.google.com/build/docs/overview
