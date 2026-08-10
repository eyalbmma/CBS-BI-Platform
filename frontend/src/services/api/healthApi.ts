import { apiRequest } from '../../api/http/apiClient'
import { HealthResponse } from './types'

export async function getHealth(): Promise<HealthResponse> {
  const data = await apiRequest<HealthResponse>({
    method: 'GET',
    path: '/api/Health',
  })

  if (typeof data?.status !== 'string' || typeof data?.service !== 'string') {
    throw new Error('Invalid health response shape')
  }

  return data as HealthResponse
}
