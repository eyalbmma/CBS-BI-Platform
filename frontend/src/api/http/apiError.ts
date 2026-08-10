import type { ProblemDetails } from '../../models/common/problemDetails'

export type ApiErrorKind = 'aborted' | 'http' | 'network' | 'unexpected'

export interface ApiErrorOptions {
  message: string
  kind: ApiErrorKind
  status?: number
  problemDetails?: ProblemDetails | null
  cause?: unknown
}

export class ApiError extends Error {
  readonly kind: ApiErrorKind

  readonly status?: number

  readonly type?: string

  readonly title?: string

  readonly detail?: string

  readonly problemDetails?: ProblemDetails | null

  readonly originalError?: unknown

  constructor(options: ApiErrorOptions) {
    super(options.message)
    this.name = 'ApiError'
    this.kind = options.kind
    this.status = options.status
    this.problemDetails = options.problemDetails ?? null
    this.type = options.problemDetails?.type
    this.title = options.problemDetails?.title
    this.detail = options.problemDetails?.detail
    this.originalError = options.cause
  }
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError
}

export function isAbortError(error: unknown): boolean {
  return isApiError(error) && error.kind === 'aborted'
}

export function getApiErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    if (error instanceof Error && error.message) {
      return error.message
    }

    return 'An unexpected error occurred.'
  }

  if (error.kind === 'aborted') {
    return 'The request was cancelled.'
  }

  if (error.kind === 'network') {
    return 'The server could not be reached.'
  }

  if (error.status === 401) {
    return 'Authentication is required to ask CBS data.'
  }

  if (error.status === 403) {
    return 'You are not authorized to perform this operation.'
  }

  if (error.status === 422) {
    return error.detail || error.title || 'The analytics question could not be processed.'
  }

  if (error.status === 504) {
    return error.detail || 'The analytics request exceeded the execution time limit.'
  }

  if (typeof error.status === 'number' && error.status >= 500) {
    if (error.detail) {
      return error.title ? `${error.title} ${error.detail}` : error.detail
    }

    return error.title || 'A server error occurred.'
  }

  return error.detail || error.title || error.message || 'An unexpected error occurred.'
}
