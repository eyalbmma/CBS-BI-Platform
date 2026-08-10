import { apiRequest } from '../http/apiClient'
import type { SaveAnalyticsQueryRequest, SavedAnalyticsQuery } from '../../models/saved-queries/savedQueriesModels'

const SAVED_QUERIES_PATH = '/api/analytics/saved-queries'

export function getSavedQueries(signal?: AbortSignal): Promise<SavedAnalyticsQuery[]> {
  return apiRequest<SavedAnalyticsQuery[]>({
    method: 'GET',
    path: SAVED_QUERIES_PATH,
    signal,
  })
}

export function saveQuery(request: SaveAnalyticsQueryRequest, signal?: AbortSignal): Promise<SavedAnalyticsQuery> {
  return apiRequest<SavedAnalyticsQuery, SaveAnalyticsQueryRequest>({
    method: 'POST',
    path: SAVED_QUERIES_PATH,
    body: request,
    signal,
  })
}

export function deleteSavedQuery(id: string, signal?: AbortSignal): Promise<void> {
  return apiRequest<void>({
    method: 'DELETE',
    path: `${SAVED_QUERIES_PATH}/${encodeURIComponent(id)}`,
    signal,
  })
}
