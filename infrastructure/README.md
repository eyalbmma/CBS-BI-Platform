# CBS BI Platform — Infrastructure

תיקייה זו מתארת את **תשתית ה-Production המוצעת על Google Cloud** עבור CBS BI Platform.

> חשוב: מסמכים אלה הם תכנון Infrastructure/Deployment להגשה. הם אינם טוענים שכל רכיבי ה-Production כבר נפרסו בפועל.  
> ה-POC הקיים רץ מקומית ב-React + .NET 8 ומתחבר ל-BigQuery ול-Gemini; רכיבי edge, Government Authentication, persistent application storage, CI/CD מלא, VPC controls ו-DR הם יעד Production.

## למה קיימת התיקייה הזאת?

מטלת הבית דורשת Architecture מלאה הכוללת, בין היתר:

- Networking
- Load Balancing
- VPC
- Firewall
- IAM
- Secret Management
- Logging / Monitoring / Alerting
- Audit
- CI/CD
- Backup
- Disaster Recovery
- High Availability

התרשים והנימוקים המלאים נמצאים ב-`architecture/`.  
התיקייה `infrastructure/` משלימה אותם מנקודת מבט תפעולית: **איך המערכת אמורה להיפרס ולהתנהל ב-Google Cloud**.

## קבצים

| קובץ | מטרה |
|---|---|
| `README.md` | גבולות התיקייה, עקרונות Infrastructure וסטטוס POC מול Production |
| `deployment.md` | Deployment topology, networking, IAM, secrets, observability, CI/CD, HA/DR ותהליך release |

## Production Target — High Level

```text
Government User
       |
       v
Government Identity Provider
       |
       v
External HTTPS Application Load Balancer
       |
       +--> Cloud Armor
       |
       +--> Frontend static assets
       |      Cloud Storage backend bucket + Cloud CDN
       |
       v
.NET 8 Backend — Cloud Run
       |
       +--> IAM / service account
       +--> Secret Manager
       +--> Cloud Logging / Monitoring
       |
       +--> Vertex AI / Gemini
       +--> BigQuery Gold layer
       +--> Persistent app metadata store
       |
       v
Data Platform
Cloud Storage Data Lake
Bronze -> Silver -> Gold -> BigQuery
```

## עקרונות Infrastructure

### 1. Least Privilege

אין שימוש ב-credentials קשיחים בקוד.  
לכל workload מוגדר Service Account ייעודי עם הרשאות מינימליות בלבד.

לדוגמה, Backend runtime צריך רק את ההרשאות הנדרשות עבור:

- הרצת BigQuery jobs וקריאת datasets מורשים;
- גישה ל-Gemini/Vertex AI;
- קריאת secrets ספציפיים;
- כתיבת logs/metrics לפי השירותים המנוהלים.

### 2. Public Edge, Controlled Backend

נקודת הכניסה הציבורית היא HTTPS Load Balancer.

Cloud Armor מיועד ל:

- WAF / application protection;
- rate controls;
- הגנת DDoS;
- חסימת traffic לא מורשה לפי מדיניות.

ה-Backend אינו אמור להיחשף כ-service URL ציבורי עוקף-LB כאשר מדיניות ה-Production דורשת ingress מבוקר.

### 3. Government Authentication Is External

המערכת אינה מנהלת Username/Password.

ב-Production:

```text
Government Identity Provider
          ↓
validated identity/session/token
          ↓
.NET authentication adapter
          ↓
ICurrentUserContext
          ↓
CurrentUser
          ↓
Authorization
```

ב-POC בלבד משתמשים ב-Development Authentication headers.

### 4. VPC and Firewall

VPC משמש לשליטה בתקשורת פרטית וב-egress לפי הצורך.

עבור Cloud Run ניתן לחבר serverless workloads ל-VPC כאשר נדרשת גישה למשאבים פרטיים.  
Firewall rules יהיו deny-by-default ככל שהארכיטקטורה מאפשרת, עם allow rules ממוקדים.

עבור שירותי Google מנוהלים ונתונים רגישים, ניתן להוסיף controls כגון Private Service Connect / VPC Service Controls בהתאם לדרישות אבטחת המידע הממשלתיות.

### 5. Secrets

Production secrets נשמרים ב-Secret Manager.

לא נכנסים ל-Git:

- service account keys;
- API keys;
- access tokens;
- passwords;
- certificates פרטיים.

### 6. Observability

נאספים לפחות:

- HTTP request latency;
- 4xx/5xx rates;
- analytics request duration;
- semantic retrieval duration;
- Gemini generation duration;
- BigQuery dry-run duration;
- BigQuery execution duration;
- timeout count;
- authorization failures;
- cost-guard rejections.

Cloud Logging מרכז logs.  
Cloud Monitoring מספק metrics/dashboards ו-Alerting.

Cloud Audit Logs משמשים לאירועי admin/access בהתאם למדיניות.

### 7. Cost and Safety Before Execution

גם ב-Production נשמר ה-flow שכבר הוכח ב-POC:

```text
SQL
 ↓
Read-only Safety
 ↓
BigQuery Dry Run
 ↓
Estimated bytes
 ↓
MaximumBytesProcessed policy
 ↓
MaximumBytesBilled
 ↓
Execution
```

### 8. POC vs Production

| נושא | POC קיים | Production יעד |
|---|---|---|
| Frontend hosting | Vite local | Cloud Storage + Cloud CDN + HTTPS LB |
| Backend hosting | ASP.NET local | Cloud Run |
| Authentication | Development handler/headers | Government Identity Provider integration |
| Saved Queries / Dashboards | In-memory | Approved persistent metadata store |
| Data | Synthetic BigQuery demo | Governed Data Lake + Bronze/Silver/Gold |
| RAG | Lexical + Business Dictionary | Hybrid/vector לפי צורך |
| Secrets | Development config | Secret Manager |
| IAM | Development credentials | Dedicated service accounts + least privilege |
| Edge security | Localhost | External ALB + Cloud Armor |
| Observability | ILogger + timings | Cloud Logging + Monitoring + Alerting + Audit |
| CI/CD | Manual local | Cloud Build + Artifact Registry + controlled deployment |
| DR | לא ממומש | RPO/RTO + backup/restore + approved failover strategy |

## מה לא נדרש למטלה

המטלה היא מטלת Architecture. לכן אין חובה לממש Terraform מלא, GKE cluster, production Government IdP או DR environment בפועל רק כדי "למלא" את repository.

אם יוחלט להוסיף Infrastructure as Code בהמשך, מומלץ להוסיף:

```text
infrastructure/
  terraform/
    modules/
    environments/
      dev/
      prod/
```

אבל רק אם הוא באמת נבדק ומתוחזק.

## מקורות רשמיים

- Cloud Run: https://docs.cloud.google.com/run/docs/overview/what-is-cloud-run
- Cloud Load Balancing: https://docs.cloud.google.com/load-balancing/docs/load-balancing-overview
- Cloud Armor: https://docs.cloud.google.com/armor/docs/cloud-armor-overview
- VPC: https://docs.cloud.google.com/vpc/docs/overview
- IAM: https://docs.cloud.google.com/iam/docs/overview
- Secret Manager: https://docs.cloud.google.com/secret-manager/docs/overview
- Cloud Logging: https://docs.cloud.google.com/logging/docs/overview
- Cloud Audit Logs: https://docs.cloud.google.com/logging/docs/audit
- Cloud Build: https://docs.cloud.google.com/build/docs/overview
