function normalizeBaseUrl(baseUrl: string): string {
  return baseUrl.replace(/\/+$/, '')
}

function parseEnableDevAuth(value?: string): boolean {
  return value === 'true'
}

export const appConfig = {
  apiBaseUrl: normalizeBaseUrl(import.meta.env.VITE_API_BASE_URL?.trim() || 'https://localhost:7116'),
  enableDevAuth: parseEnableDevAuth(import.meta.env.VITE_ENABLE_DEV_AUTH),
}
