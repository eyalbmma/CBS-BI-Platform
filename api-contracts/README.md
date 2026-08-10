# CBS BI Platform — API Contracts

תיקייה זו מרכזת רק את ה־API contracts הרלוונטיים ל־POC ולהדגמה.

## עקרון חשוב

מקור האמת בזמן ריצה הוא ה־ASP.NET Core controllers וה־Swagger/OpenAPI שמופק מה־Backend.

הקובץ `poc-api-contracts.yaml` הוא **תיעוד מרוכז של ה־public POC API** לצורך GitHub, review והגשת המטלה. הוא אינו מחליף את Swagger שנוצר מהקוד.

## מה כלול

- Ask Data — Natural Language → RAG/Gemini → SQL → BigQuery
- Visual Query — Structured request → deterministic SQL → BigQuery
- Saved Queries — Save / List / Delete
- Dashboards — Create / List / Get / Delete

לא נכללו כאן development-only endpoints כגון:

- `/api/development/analytics/generate-sql`
- `/api/development/analytics/metadata`
- `/api/development/analytics/retrieve-semantic`

הם כלי POC/diagnostics ולא חלק מה־public product contract.

## Authentication

ב־POC המקומי ה־frontend מוסיף development headers:

```text
X-Dev-UserId: dev-user
X-Dev-Roles: AnalyticsQueryExecutor
```

אלה אינם חלק מארכיטקטורת Production.

ב־Production ה־Backend מיועד לקבל identity מאומת דרך אינטגרציה עם מנגנון ההזדהות הממשלתי, לבנות `CurrentUser`, ואז לבצע Authorization.

## קבצים

```text
api-contracts/
├── README.md
├── poc-api-contracts.yaml
└── examples/
    ├── ask-data.json
    ├── visual-query.json
    ├── saved-query.json
    └── dashboard.json
```

## הערה להגשה

אין צורך ליצור API contract נפרד לכל class פנימי במערכת. התיקייה מתעדת רק את ה־HTTP surface שה־Frontend משתמש בו בפועל.
