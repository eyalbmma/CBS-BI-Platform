# CBS BI Platform — Demo Database

תיקייה זו מאפשרת לשחזר את שכבת ה־BigQuery של ה־POC **בלי לקבל גישה לפרויקט Google Cloud הפרטי של המפתח**.

## קבצים

```text
database/
├── README.md
└── setup-demo.sql
```

## מקור הנתונים

`setup-demo.sql` נוצר מארבעת קובצי ה־CSV שיוצאו מהטבלאות הפעילות של ה־POC:

- `city_population` — 12 rows, 21 columns
- `city_employment_yearly` — 8 rows, 21 columns
- `city_housing_yearly` — 8 rows, 22 columns
- `city_education_yearly` — 8 rows, 22 columns

הנתונים הם **Synthetic Demo Data בלבד** ואינם נתוני למ״ס רשמיים.

## Dataset

ה־POC משתמש ב:

```text
Dataset: analytics_demo
Location: me-west1
```

הסקריפט יוצר את ה־dataset אם הוא אינו קיים, ולאחר מכן יוצר מחדש את ארבע הטבלאות ומכניס את נתוני הדמו.

## Tables

### city_population

Demographic/population Gold-layer demo table.

כוללת בין היתר:

- City / CityCode
- Population
- Year
- District
- Area / Density
- Households
- Age distribution
- Births / Deaths
- Migration
- Annual growth

### city_employment_yearly

Employment Gold-layer yearly demo table.

כוללת בין היתר:

- EmployedPersons
- UnemployedPersons
- UnemploymentRatePct
- EmploymentRatePct
- AverageMonthlySalaryNis
- MedianMonthlySalaryNis
- HighTechEmploymentPct

מוגדרת ב־setup script כ־integer-range partitioned by `Year` ו־clustered by `CityCode`.

### city_housing_yearly

Housing Gold-layer yearly demo table.

כוללת בין היתר:

- AverageApartmentPriceNis
- MedianApartmentPriceNis
- AverageMonthlyRentNis
- TransactionsCount
- BuildingStarts / BuildingCompletions
- TotalHousingUnits

מוגדרת כ־partitioned by `Year` ו־clustered by `CityCode`.

### city_education_yearly

Education Gold-layer yearly demo table.

כוללת בין היתר:

- TotalStudents
- TeachersCount
- MatriculationEligibilityPct
- FiveUnitMathPct
- FiveUnitEnglishPct
- DropoutRatePct
- EducationBudgetPerStudentNis

מוגדרת כ־partitioned by `Year` ו־clustered by `CityCode`.

## איך משחזרים את ה־Database

1. צור/בחר Google Cloud project.
2. ודא ש־BigQuery API פעיל.
3. פתח BigQuery Studio.
4. בחר את הפרויקט שבו אתה רוצה להקים את סביבת הדמו.
5. פתח את `setup-demo.sql`.
6. הרץ את כל הסקריפט.
7. ודא שנוצר dataset בשם `analytics_demo` ובו ארבע הטבלאות.

אחרי ההקמה ניתן לבדוק:

```sql
SELECT *
FROM `analytics_demo.city_population`
ORDER BY Year, City;
```

## חיבור ה־Backend

יש להגדיר ב־Backend את ה־Google Cloud project שבו הוקם ה־dataset.

ה־POC המקורי פותח מול project פרטי בשם:

```text
cbs-bi-poc
```

השם הזה מופיע ב־semantic metadata ובדוגמאות SQL של ה־POC. בסביבה אחרת יש להתאים את Project ID/configuration בהתאם לדרך שבה ה־Backend מרכיב fully-qualified BigQuery references.

אין להעלות ל־GitHub:

```text
service-account.json
access tokens
API keys
private credentials
```

בפיתוח מקומי יש להשתמש בדרך authentication מאושרת כגון Application Default Credentials או Service Account ייעודי שאינו נשמר ב־repository.

## Join Semantics

הטבלאות מייצגות city/year statistics.

כאשר משלבים domains, ה־semantic rule של ה־POC הוא:

```text
JOIN ON CityCode AND Year
```

ולא join לפי שם העיר בלבד.

## Latest Year

כאשר המשתמש אינו מציין שנה, ה־POC משתמש ב־:

```sql
MAX(Year)
```

ולא בשנה hardcoded.

## Reproducibility Note

ה־CSV exports סיפקו את **תוכן הטבלאות**. ה־setup script מוסיף schema descriptions שמתאימים ל־semantic layer של ה־POC, וכן משמר את תכנון ה־partitioning/clustering של שלוש טבלאות ה־yearly domains.

`city_population` נשארת ללא partitioning בסקריפט, בהתאם ל־POC baseline המתועד.
