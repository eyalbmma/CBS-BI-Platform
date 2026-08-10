import { appConfig } from '../../config/appConfig'
import type { ProblemDetails } from '../../models/common/problemDetails'
import { ApiError, getApiErrorMessage } from './apiError'

const DEVELOPMENT_AUTH_HEADERS = {
  'X-Dev-UserId': 'dev-user',
  'X-Dev-Roles': 'AnalyticsQueryExecutor',
} as const

type ApiRequestOptions<TBody> = {
  method: string
  path: string
  body?: TBody
  signal?: AbortSignal
  headers?: HeadersInit
}

export async function apiRequest<TResponse, TBody = undefined>(options: ApiRequestOptions<TBody>): Promise<TResponse> {
  const response = await sendRequest(options)

  if (response.ok) {
    return (await readJsonBody<TResponse>(response)) as TResponse
  }

  const errorBody = await readErrorBody(response)
  const problemDetails = toProblemDetails(errorBody, response.status)

  throw new ApiError({
    message: getApiErrorMessage(
      new ApiError({
        message: 'Request failed.',
        kind: 'http',
        status: response.status,
        problemDetails,
      })
    ),
    kind: 'http',
    status: response.status,
    problemDetails,
    cause: errorBody,
  })
}

async function sendRequest<TBody>(options: ApiRequestOptions<TBody>): Promise<Response> {
  const url = buildUrl(options.path)
  const headers = new Headers(options.headers)

  headers.set('Accept', 'application/json')
  if (appConfig.enableDevAuth) {
    headers.set('X-Dev-UserId', DEVELOPMENT_AUTH_HEADERS['X-Dev-UserId'])
    headers.set('X-Dev-Roles', DEVELOPMENT_AUTH_HEADERS['X-Dev-Roles'])
  }

  const requestInit: RequestInit = {
    method: options.method,
    headers,
    signal: options.signal,
  }

  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json')
    requestInit.body = JSON.stringify(options.body)
  }

  try {
    return await fetch(url, requestInit)
  } catch (error) {
    if (isAbortLikeError(error)) {
      throw new ApiError({
        message: 'The request was cancelled.',
        kind: 'aborted',
        cause: error,
      })
    }

    throw new ApiError({
      message: 'The server could not be reached.',
      kind: 'network',
      cause: error,
    })
  }
}

async function readJsonBody<TResponse>(response: Response): Promise<TResponse | undefined> {
  const text = await response.text()

  if (text.trim().length === 0) {
    return undefined
  }

  try {
    return JSON.parse(text) as TResponse
  } catch (error) {
    throw new ApiError({
      message: 'The server returned an unexpected response.',
      kind: 'unexpected',
      status: response.status,
      cause: error,
    })
  }
}

async function readErrorBody(response: Response): Promise<unknown> {
  const text = await response.text()

  if (text.trim().length === 0) {
    return undefined
  }

  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
}

function toProblemDetails(body: unknown, status: number): ProblemDetails | null {
  if (typeof body === 'string') {
    return {
      status,
      detail: body,
    }
  }

  if (!body || typeof body !== 'object') {
    return null
  }

  const record = body as Record<string, unknown>
  const problemDetails: ProblemDetails = {}

  if (typeof record.type === 'string') {
    problemDetails.type = record.type
  }

  if (typeof record.title === 'string') {
    problemDetails.title = record.title
  }

  if (typeof record.detail === 'string') {
    problemDetails.detail = record.detail
  }

  if (typeof record.instance === 'string') {
    problemDetails.instance = record.instance
  }

  if (typeof record.status === 'number') {
    problemDetails.status = record.status
  } else {
    problemDetails.status = status
  }

  return Object.keys(problemDetails).length > 0 ? problemDetails : null
}

function buildUrl(path: string): string {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`
  return `${appConfig.apiBaseUrl}${normalizedPath}`
}

function isAbortLikeError(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'AbortError'
}
