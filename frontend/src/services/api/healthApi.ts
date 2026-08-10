import { HealthResponse } from './types'

const HEALTH_URL = 'https://localhost:7116/api/Health'

export async function getHealth(): Promise<HealthResponse> {
  const res = await fetch(HEALTH_URL, { method: 'GET' })
  if (!res.ok) {
    throw new Error(`Network response was not ok: ${res.status}`)
  }

  const data = await res.json()

  if (typeof data?.status !== 'string' || typeof data?.service !== 'string') {
    throw new Error('Invalid health response shape')
  }

  return data as HealthResponse
}
