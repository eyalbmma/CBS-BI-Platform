export interface SavedAnalyticsQuery {
  id: string
  name: string
  question: string
  createdByUserId?: string
  createdAtUtc?: string
  updatedAtUtc?: string
}

export interface SaveAnalyticsQueryRequest {
  name: string
  question: string
}
