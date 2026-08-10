import type { AnalyticsResultSet } from '../analytics/analyticsModels'

export type VisualQueryDomain = 'Population' | 'Employment' | 'Housing' | 'Education'

export type VisualQueryMetric =
  | 'Population'
  | 'UnemploymentRatePct'
  | 'EmployedPersons'
  | 'AverageMonthlySalaryNis'
  | 'AverageApartmentPriceNis'
  | 'AverageMonthlyRentNis'
  | 'TotalStudents'
  | 'MatriculationEligibilityPct'
  | 'TeachersCount'

export type VisualQuerySortDirection = 'Ascending' | 'Descending'

export interface VisualQueryRequest {
  domain: VisualQueryDomain
  metric: VisualQueryMetric
  year: number | null
  sortDirection: VisualQuerySortDirection
  limit: number
}

export interface VisualQueryResponse {
  sql: string
  result: AnalyticsResultSet
}
