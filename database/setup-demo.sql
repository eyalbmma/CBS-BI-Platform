-- CBS BI Platform — Reproducible BigQuery Demo Dataset
-- Generated from the four CSV exports supplied from the working POC.
--
-- IMPORTANT:
-- 1. The data is synthetic demo data, not official CBS statistics.
-- 2. Run this script while the desired Google Cloud project is selected.
-- 3. The script creates/replaces the four POC tables in analytics_demo.
-- 4. Dataset location is me-west1 to match the current POC configuration.
-- 5. No credentials, tokens or service-account keys are included.

CREATE SCHEMA IF NOT EXISTS `analytics_demo`
OPTIONS (
  location = "me-west1",
  description = "CBS BI Platform synthetic analytics demo dataset"
);


-- ============================================================
-- city_population
-- Rows: 12
-- ============================================================


CREATE OR REPLACE TABLE `analytics_demo.city_population` (
  `City` STRING OPTIONS(description="City name."),
  `Population` INT64 OPTIONS(description="Total city population / number of residents."),
  `Year` INT64 OPTIONS(description="Reference year for the statistic."),
  `CityCode` STRING OPTIONS(description="Stable demo city code used as the semantic join key across city/year domains."),
  `District` STRING OPTIONS(description="Administrative district."),
  `AreaKm2` FLOAT64 OPTIONS(description="City area in square kilometers."),
  `DensityPerKm2` FLOAT64 OPTIONS(description="Population density per square kilometer."),
  `Households` INT64 OPTIONS(description="Number of households."),
  `AverageHouseholdSize` FLOAT64 OPTIONS(description="Average number of persons per household."),
  `MalePopulation` INT64 OPTIONS(description="Male population count."),
  `FemalePopulation` INT64 OPTIONS(description="Female population count."),
  `Age0To14Pct` FLOAT64 OPTIONS(description="Percentage of residents aged 0 to 14."),
  `Age15To64Pct` FLOAT64 OPTIONS(description="Percentage of residents aged 15 to 64."),
  `Age65PlusPct` FLOAT64 OPTIONS(description="Percentage of residents aged 65 and over."),
  `Births` INT64 OPTIONS(description="Number of births."),
  `Deaths` INT64 OPTIONS(description="Number of deaths."),
  `InternalMigrationBalance` INT64 OPTIONS(description="Internal migration balance."),
  `ExternalMigrationBalance` INT64 OPTIONS(description="External migration balance."),
  `AnnualGrowthPct` FLOAT64 OPTIONS(description="Annual population growth percentage."),
  `ReferenceDate` DATE OPTIONS(description="Reference date for the yearly/demo observation."),
  `DataQualityStatus` STRING OPTIONS(description="Demo data quality/status marker.")
)
OPTIONS (
  description="Gold-layer synthetic demo data for city population and demographic analytics in the CBS BI Platform POC."
);



INSERT INTO `analytics_demo.city_population` (`City`, `Population`, `Year`, `CityCode`, `District`, `AreaKm2`, `DensityPerKm2`, `Households`, `AverageHouseholdSize`, `MalePopulation`, `FemalePopulation`, `Age0To14Pct`, `Age15To64Pct`, `Age65PlusPct`, `Births`, `Deaths`, `InternalMigrationBalance`, `ExternalMigrationBalance`, `AnnualGrowthPct`, `ReferenceDate`, `DataQualityStatus`)
VALUES
  ('Beersheba', 218000, 2024, 'BSH', 'Southern', 117.5, 1855.3, 85000, 2.56, 107000, 111000, 24.0, 63.0, 13.0, 3600, 1900, 1200, 1800, 1.0, DATE '2024-12-31', 'FINAL'),
  ('Haifa', 292000, 2024, 'HFA', 'Haifa', 63.7, 4584.0, 127000, 2.3, 143000, 149000, 21.0, 61.0, 18.0, 4300, 3300, 900, 1500, 1.0, DATE '2024-12-31', 'FINAL'),
  ('Jerusalem', 990000, 2024, 'JER', 'Jerusalem', 125.2, 7907.3, 260000, 3.8, 490000, 500000, 31.0, 57.0, 12.0, 21000, 6200, 2800, 4500, 1.5, DATE '2024-12-31', 'FINAL'),
  ('Petah Tikva', 262000, 2024, 'PTK', 'Central', 35.9, 7298.1, 101000, 2.59, 128000, 134000, 23.0, 63.0, 14.0, 4000, 2200, 1700, 1300, 1.2, DATE '2024-12-31', 'FINAL'),
  ('Rishon LeZion', 272000, 2024, 'RLZ', 'Central', 58.7, 4633.7, 108000, 2.52, 133000, 139000, 22.0, 62.0, 16.0, 3900, 2400, 1600, 1100, 1.1, DATE '2024-12-31', 'FINAL'),
  ('Tel Aviv', 474000, 2024, 'TLV', 'Tel Aviv', 52.0, 9115.4, 220000, 2.15, 233000, 241000, 19.0, 66.0, 15.0, 7200, 4100, 5200, 3900, 1.3, DATE '2024-12-31', 'FINAL'),
  ('Beersheba', 220000, 2025, 'BSH', 'Southern', 117.5, 1872.3, 86000, 2.56, 108000, 112000, 24.1, 62.8, 13.1, 3700, 1950, 1350, 1950, 0.9, DATE '2025-12-31', 'FINAL'),
  ('Haifa', 295000, 2025, 'HFA', 'Haifa', 63.7, 4631.1, 129000, 2.29, 145000, 150000, 20.8, 61.0, 18.2, 4400, 3350, 1100, 1600, 1.0, DATE '2025-12-31', 'FINAL'),
  ('Jerusalem', 1000000, 2025, 'JER', 'Jerusalem', 125.2, 7987.2, 264000, 3.79, 495000, 505000, 31.2, 56.8, 12.0, 21500, 6300, 3100, 4900, 1.0, DATE '2025-12-31', 'FINAL'),
  ('Petah Tikva', 265000, 2025, 'PTK', 'Central', 35.9, 7381.6, 103000, 2.57, 129500, 135500, 22.9, 63.0, 14.1, 4100, 2250, 1800, 1400, 1.1, DATE '2025-12-31', 'FINAL'),
  ('Rishon LeZion', 275000, 2025, 'RLZ', 'Central', 58.7, 4684.8, 110000, 2.5, 134500, 140500, 21.8, 62.1, 16.1, 4000, 2450, 1750, 1200, 1.1, DATE '2025-12-31', 'FINAL'),
  ('Tel Aviv', 480000, 2025, 'TLV', 'Tel Aviv', 52.0, 9230.8, 224000, 2.14, 236000, 244000, 18.8, 66.1, 15.1, 7350, 4150, 5500, 4200, 1.3, DATE '2025-12-31', 'FINAL');


-- ============================================================
-- city_employment_yearly
-- Rows: 8
-- ============================================================


CREATE OR REPLACE TABLE `analytics_demo.city_employment_yearly` (
  `CityCode` STRING OPTIONS(description="Stable demo city code used as the semantic join key across city/year domains."),
  `City` STRING OPTIONS(description="City name."),
  `Year` INT64 OPTIONS(description="Reference year for the statistic."),
  `District` STRING OPTIONS(description="Administrative district."),
  `WorkingAgePopulation` INT64 OPTIONS(description="Working-age population."),
  `LaborForcePopulation` INT64 OPTIONS(description="Population participating in the labor force."),
  `EmployedPersons` INT64 OPTIONS(description="Number of employed persons."),
  `UnemployedPersons` INT64 OPTIONS(description="Number of unemployed persons."),
  `LaborForceParticipationPct` FLOAT64 OPTIONS(description="Labor force participation rate percentage."),
  `EmploymentRatePct` FLOAT64 OPTIONS(description="Employment rate percentage."),
  `UnemploymentRatePct` FLOAT64 OPTIONS(description="Unemployment rate percentage."),
  `AverageMonthlySalaryNis` FLOAT64 OPTIONS(description="Average monthly salary in NIS."),
  `MedianMonthlySalaryNis` FLOAT64 OPTIONS(description="Median monthly salary in NIS."),
  `SelfEmployedPct` FLOAT64 OPTIONS(description="Percentage of workers who are self-employed."),
  `HighTechEmploymentPct` FLOAT64 OPTIONS(description="Percentage of employment in high-tech."),
  `PublicSectorEmploymentPct` FLOAT64 OPTIONS(description="Percentage of employment in the public sector."),
  `PartTimeEmploymentPct` FLOAT64 OPTIONS(description="Percentage of employment that is part-time."),
  `JobsPer1000Residents` FLOAT64 OPTIONS(description="Number of jobs per 1,000 residents."),
  `NewJobSeekers` INT64 OPTIONS(description="Number of new job seekers."),
  `ReferenceDate` DATE OPTIONS(description="Reference date for the yearly/demo observation."),
  `DataQualityStatus` STRING OPTIONS(description="Demo data quality/status marker.")
)
PARTITION BY RANGE_BUCKET(Year, GENERATE_ARRAY(2020, 2031, 1))
CLUSTER BY CityCode
OPTIONS (
  description="Gold-layer synthetic yearly city employment demo data for the CBS BI Platform POC."
);



INSERT INTO `analytics_demo.city_employment_yearly` (`CityCode`, `City`, `Year`, `District`, `WorkingAgePopulation`, `LaborForcePopulation`, `EmployedPersons`, `UnemployedPersons`, `LaborForceParticipationPct`, `EmploymentRatePct`, `UnemploymentRatePct`, `AverageMonthlySalaryNis`, `MedianMonthlySalaryNis`, `SelfEmployedPct`, `HighTechEmploymentPct`, `PublicSectorEmploymentPct`, `PartTimeEmploymentPct`, `JobsPer1000Residents`, `NewJobSeekers`, `ReferenceDate`, `DataQualityStatus`)
VALUES
  ('BSH', 'Beersheba', 2024, 'Southern', 141000, 88000, 82800, 5200, 62.4, 58.7, 5.9, 11900.0, 10000.0, 9.5, 10.0, 23.0, 19.0, 720.0, 5200, DATE '2024-12-31', 'FINAL'),
  ('HFA', 'Haifa', 2024, 'Haifa', 185000, 125000, 119700, 5300, 67.6, 64.7, 4.2, 14200.0, 11900.0, 10.5, 15.0, 20.0, 16.0, 850.0, 4700, DATE '2024-12-31', 'FINAL'),
  ('JER', 'Jerusalem', 2024, 'Jerusalem', 560000, 340000, 321000, 19000, 60.7, 57.3, 5.6, 11800.0, 10100.0, 10.1, 8.0, 27.0, 18.0, 690.0, 12800, DATE '2024-12-31', 'FINAL'),
  ('TLV', 'Tel Aviv', 2024, 'Tel Aviv', 310000, 228000, 220500, 7500, 73.5, 71.1, 3.3, 17600.0, 14500.0, 14.0, 22.5, 15.0, 15.0, 1210.0, 6200, DATE '2024-12-31', 'FINAL'),
  ('BSH', 'Beersheba', 2025, 'Southern', 143000, 90000, 84780, 5220, 62.9, 59.3, 5.8, 12400.0, 10400.0, 9.7, 10.7, 23.4, 18.6, 735.0, 5000, DATE '2025-12-31', 'FINAL'),
  ('HFA', 'Haifa', 2025, 'Haifa', 187000, 127000, 121700, 5300, 67.9, 65.1, 4.2, 14800.0, 12300.0, 10.4, 15.8, 20.2, 15.8, 865.0, 4500, DATE '2025-12-31', 'FINAL'),
  ('JER', 'Jerusalem', 2025, 'Jerusalem', 565000, 346000, 327700, 18300, 61.2, 58.0, 5.3, 12300.0, 10500.0, 10.0, 8.5, 27.5, 17.5, 705.0, 12100, DATE '2025-12-31', 'FINAL'),
  ('TLV', 'Tel Aviv', 2025, 'Tel Aviv', 314000, 232000, 224800, 7200, 73.9, 71.6, 3.1, 18400.0, 15100.0, 14.2, 23.5, 15.2, 14.7, 1240.0, 5900, DATE '2025-12-31', 'FINAL');


-- ============================================================
-- city_housing_yearly
-- Rows: 8
-- ============================================================


CREATE OR REPLACE TABLE `analytics_demo.city_housing_yearly` (
  `CityCode` STRING OPTIONS(description="Stable demo city code used as the semantic join key across city/year domains."),
  `City` STRING OPTIONS(description="City name."),
  `Year` INT64 OPTIONS(description="Reference year for the statistic."),
  `District` STRING OPTIONS(description="Administrative district."),
  `AverageApartmentPriceNis` FLOAT64 OPTIONS(description="Average apartment price in NIS."),
  `MedianApartmentPriceNis` FLOAT64 OPTIONS(description="Median apartment price in NIS."),
  `AverageMonthlyRentNis` FLOAT64 OPTIONS(description="Average monthly rent in NIS."),
  `MedianMonthlyRentNis` FLOAT64 OPTIONS(description="Median monthly rent in NIS."),
  `TransactionsCount` INT64 OPTIONS(description="Number of housing transactions."),
  `NewApartmentsSold` INT64 OPTIONS(description="Number of new apartments sold."),
  `SecondHandApartmentsSold` INT64 OPTIONS(description="Number of second-hand apartments sold."),
  `BuildingStarts` INT64 OPTIONS(description="Number of housing/building starts."),
  `BuildingCompletions` INT64 OPTIONS(description="Number of housing/building completions."),
  `AverageApartmentSizeSqm` FLOAT64 OPTIONS(description="Average apartment size in square meters."),
  `AveragePricePerSqmNis` FLOAT64 OPTIONS(description="Average apartment price per square meter in NIS."),
  `AverageRentPerSqmNis` FLOAT64 OPTIONS(description="Average monthly rent per square meter in NIS."),
  `VacantHousingPct` FLOAT64 OPTIONS(description="Percentage of housing units that are vacant."),
  `OwnerOccupiedPct` FLOAT64 OPTIONS(description="Percentage of housing units that are owner occupied."),
  `RentalOccupiedPct` FLOAT64 OPTIONS(description="Percentage of housing units that are rental occupied."),
  `TotalHousingUnits` INT64 OPTIONS(description="Total number of housing units."),
  `ReferenceDate` DATE OPTIONS(description="Reference date for the yearly/demo observation."),
  `DataQualityStatus` STRING OPTIONS(description="Demo data quality/status marker.")
)
PARTITION BY RANGE_BUCKET(Year, GENERATE_ARRAY(2020, 2031, 1))
CLUSTER BY CityCode
OPTIONS (
  description="Gold-layer synthetic yearly city housing demo data for the CBS BI Platform POC."
);



INSERT INTO `analytics_demo.city_housing_yearly` (`CityCode`, `City`, `Year`, `District`, `AverageApartmentPriceNis`, `MedianApartmentPriceNis`, `AverageMonthlyRentNis`, `MedianMonthlyRentNis`, `TransactionsCount`, `NewApartmentsSold`, `SecondHandApartmentsSold`, `BuildingStarts`, `BuildingCompletions`, `AverageApartmentSizeSqm`, `AveragePricePerSqmNis`, `AverageRentPerSqmNis`, `VacantHousingPct`, `OwnerOccupiedPct`, `RentalOccupiedPct`, `TotalHousingUnits`, `ReferenceDate`, `DataQualityStatus`)
VALUES
  ('BSH', 'Beersheba', 2024, 'Southern', 1550000.0, 1450000.0, 3550.0, 3350.0, 3100, 1100, 2000, 2400, 2200, 94.0, 16500.0, 37.8, 9.0, 60.0, 35.0, 94000, DATE '2024-12-31', 'FINAL'),
  ('HFA', 'Haifa', 2024, 'Haifa', 1850000.0, 1720000.0, 4100.0, 3900.0, 3600, 900, 2700, 1900, 1700, 88.0, 21000.0, 46.5, 8.2, 67.0, 29.0, 135000, DATE '2024-12-31', 'FINAL'),
  ('JER', 'Jerusalem', 2024, 'Jerusalem', 2450000.0, 2300000.0, 5700.0, 5400.0, 5200, 1700, 3500, 4300, 3800, 92.0, 26600.0, 62.0, 6.5, 68.0, 28.0, 285000, DATE '2024-12-31', 'FINAL'),
  ('TLV', 'Tel Aviv', 2024, 'Tel Aviv', 4200000.0, 3900000.0, 7600.0, 7200.0, 6100, 2100, 4000, 3100, 2800, 82.0, 51200.0, 92.5, 7.8, 48.0, 48.0, 235000, DATE '2024-12-31', 'FINAL'),
  ('BSH', 'Beersheba', 2025, 'Southern', 1630000.0, 1520000.0, 3750.0, 3500.0, 3250, 1200, 2050, 2550, 2300, 95.0, 17150.0, 39.5, 8.7, 60.5, 34.5, 96000, DATE '2025-12-31', 'FINAL'),
  ('HFA', 'Haifa', 2025, 'Haifa', 1940000.0, 1800000.0, 4300.0, 4050.0, 3750, 950, 2800, 2000, 1800, 89.0, 21800.0, 48.3, 8.0, 67.2, 29.1, 137000, DATE '2025-12-31', 'FINAL'),
  ('JER', 'Jerusalem', 2025, 'Jerusalem', 2580000.0, 2410000.0, 5950.0, 5600.0, 5400, 1800, 3600, 4500, 4050, 93.0, 27700.0, 64.0, 6.3, 68.5, 27.5, 291000, DATE '2025-12-31', 'FINAL'),
  ('TLV', 'Tel Aviv', 2025, 'Tel Aviv', 4450000.0, 4120000.0, 7950.0, 7500.0, 6300, 2200, 4100, 3250, 2950, 83.0, 53600.0, 95.8, 7.5, 47.5, 48.5, 239000, DATE '2025-12-31', 'FINAL');


-- ============================================================
-- city_education_yearly
-- Rows: 8
-- ============================================================


CREATE OR REPLACE TABLE `analytics_demo.city_education_yearly` (
  `CityCode` STRING OPTIONS(description="Stable demo city code used as the semantic join key across city/year domains."),
  `City` STRING OPTIONS(description="City name."),
  `Year` INT64 OPTIONS(description="Reference year for the statistic."),
  `District` STRING OPTIONS(description="Administrative district."),
  `TotalStudents` INT64 OPTIONS(description="Total number of students."),
  `KindergartenStudents` INT64 OPTIONS(description="Number of kindergarten students."),
  `PrimaryStudents` INT64 OPTIONS(description="Number of primary school students."),
  `MiddleSchoolStudents` INT64 OPTIONS(description="Number of middle school students."),
  `HighSchoolStudents` INT64 OPTIONS(description="Number of high school students."),
  `SchoolsCount` INT64 OPTIONS(description="Number of schools."),
  `TeachersCount` INT64 OPTIONS(description="Number of teachers."),
  `AverageStudentsPerClass` FLOAT64 OPTIONS(description="Average number of students per class."),
  `MatriculationEligibilityPct` FLOAT64 OPTIONS(description="Percentage eligible for matriculation."),
  `FiveUnitMathPct` FLOAT64 OPTIONS(description="Percentage studying/qualifying in five-unit mathematics."),
  `FiveUnitEnglishPct` FLOAT64 OPTIONS(description="Percentage studying/qualifying in five-unit English."),
  `DropoutRatePct` FLOAT64 OPTIONS(description="Student dropout rate percentage."),
  `AcademicDegreePctAge25To64` FLOAT64 OPTIONS(description="Percentage of residents aged 25–64 with an academic degree."),
  `EducationBudgetPerStudentNis` FLOAT64 OPTIONS(description="Education budget per student in NIS."),
  `SpecialEducationStudents` INT64 OPTIONS(description="Number of special education students."),
  `DigitalLearningParticipationPct` FLOAT64 OPTIONS(description="Percentage participating in digital learning."),
  `ReferenceDate` DATE OPTIONS(description="Reference date for the yearly/demo observation."),
  `DataQualityStatus` STRING OPTIONS(description="Demo data quality/status marker.")
)
PARTITION BY RANGE_BUCKET(Year, GENERATE_ARRAY(2020, 2031, 1))
CLUSTER BY CityCode
OPTIONS (
  description="Gold-layer synthetic yearly city education demo data for the CBS BI Platform POC."
);



INSERT INTO `analytics_demo.city_education_yearly` (`CityCode`, `City`, `Year`, `District`, `TotalStudents`, `KindergartenStudents`, `PrimaryStudents`, `MiddleSchoolStudents`, `HighSchoolStudents`, `SchoolsCount`, `TeachersCount`, `AverageStudentsPerClass`, `MatriculationEligibilityPct`, `FiveUnitMathPct`, `FiveUnitEnglishPct`, `DropoutRatePct`, `AcademicDegreePctAge25To64`, `EducationBudgetPerStudentNis`, `SpecialEducationStudents`, `DigitalLearningParticipationPct`, `ReferenceDate`, `DataQualityStatus`)
VALUES
  ('BSH', 'Beersheba', 2024, 'Southern', 48000, 7500, 17000, 10000, 13500, 155, 3700, 25.3, 79.0, 20.0, 43.0, 2.1, 39.0, 18800.0, 5900, 79.0, DATE '2024-12-31', 'FINAL'),
  ('HFA', 'Haifa', 2024, 'Haifa', 62000, 9000, 22000, 13000, 18000, 190, 4800, 24.8, 84.0, 27.0, 52.0, 1.6, 49.0, 20100.0, 6900, 84.0, DATE '2024-12-31', 'FINAL'),
  ('JER', 'Jerusalem', 2024, 'Jerusalem', 285000, 48000, 102000, 55000, 80000, 640, 18500, 26.8, 72.0, 14.0, 35.0, 2.5, 34.0, 17100.0, 31000, 74.0, DATE '2024-12-31', 'FINAL'),
  ('TLV', 'Tel Aviv', 2024, 'Tel Aviv', 82000, 12000, 28500, 17000, 24500, 235, 6400, 24.1, 88.0, 32.0, 58.0, 1.3, 56.0, 21800.0, 8500, 88.0, DATE '2024-12-31', 'FINAL'),
  ('BSH', 'Beersheba', 2025, 'Southern', 49500, 7700, 17500, 10300, 14000, 160, 3820, 25.1, 80.5, 22.0, 45.0, 1.9, 41.0, 19500.0, 6050, 83.0, DATE '2025-12-31', 'FINAL'),
  ('HFA', 'Haifa', 2025, 'Haifa', 63500, 9200, 22500, 13200, 18600, 194, 4920, 24.6, 85.0, 29.0, 54.0, 1.5, 50.0, 20800.0, 7050, 87.0, DATE '2025-12-31', 'FINAL'),
  ('JER', 'Jerusalem', 2025, 'Jerusalem', 291000, 49000, 104000, 56000, 82000, 650, 19000, 26.6, 73.5, 15.5, 37.0, 2.3, 35.0, 17800.0, 31500, 78.0, DATE '2025-12-31', 'FINAL'),
  ('TLV', 'Tel Aviv', 2025, 'Tel Aviv', 84000, 12200, 29000, 17500, 25300, 240, 6600, 23.9, 89.0, 34.0, 60.0, 1.2, 58.0, 22600.0, 8700, 91.0, DATE '2025-12-31', 'FINAL');
