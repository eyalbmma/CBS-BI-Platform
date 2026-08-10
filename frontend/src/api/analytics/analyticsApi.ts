import { apiRequest } from '../http/apiClient'
import type { AnalyticsQuestionRequest, AnalyticsQuestionResponse } from '../../models/analytics/analyticsModels'

export function askQuestion(question: string, signal?: AbortSignal): Promise<AnalyticsQuestionResponse> {
  return apiRequest<AnalyticsQuestionResponse, AnalyticsQuestionRequest>({
    method: 'POST',
    path: '/api/Analytics/ask',
    body: {
      question,
    },
    signal,
  })
}
