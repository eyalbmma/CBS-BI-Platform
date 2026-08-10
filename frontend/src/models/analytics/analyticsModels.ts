export interface AnalyticsQuestionRequest {
  question: string
}

export type AnalyticsCellValue = string | number | boolean | null

export interface AnalyticsResultRow {
  [columnName: string]: AnalyticsCellValue
}

export interface AnalyticsResultSet {
  columns: string[]
  rows: AnalyticsResultRow[]
}

export interface AnalyticsQuestionResponse {
  sql: string
  result: AnalyticsResultSet
}
