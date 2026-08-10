import { apiRequest } from '../http/apiClient'
import type { VisualQueryRequest, VisualQueryResponse } from '../../models/visual-query/visualQueryModels'

const VISUAL_QUERY_PATH = '/api/analytics/visual-query'

export function runVisualQuery(request: VisualQueryRequest, signal?: AbortSignal): Promise<VisualQueryResponse> {
  return apiRequest<VisualQueryResponse, VisualQueryRequest>({
    method: 'POST',
    path: VISUAL_QUERY_PATH,
    body: request,
    signal,
  })
}
