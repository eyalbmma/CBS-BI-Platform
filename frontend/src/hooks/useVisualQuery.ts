import { useCallback, useEffect, useRef, useState } from 'react'
import { runVisualQuery } from '../api/visual-query/visualQueryApi'
import { ApiError, isAbortError } from '../api/http/apiError'
import type { VisualQueryRequest, VisualQueryResponse } from '../models/visual-query/visualQueryModels'

interface UseVisualQueryState {
  result: VisualQueryResponse | null
  error: unknown | null
  isLoading: boolean
  executeVisualQuery: (request: VisualQueryRequest) => Promise<VisualQueryResponse | null>
  reset: () => void
}

export function useVisualQuery(): UseVisualQueryState {
  const [result, setResult] = useState<VisualQueryResponse | null>(null)
  const [error, setError] = useState<unknown | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const controllerRef = useRef<AbortController | null>(null)
  const mountedRef = useRef(true)

  useEffect(() => {
    return () => {
      mountedRef.current = false
      controllerRef.current?.abort()
    }
  }, [])

  const reset = useCallback(() => {
    setResult(null)
    setError(null)
    setIsLoading(false)
  }, [])

  const executeVisualQuery = useCallback(async (request: VisualQueryRequest): Promise<VisualQueryResponse | null> => {
    if (isLoading) {
      return null
    }

    controllerRef.current?.abort()

    const controller = new AbortController()
    controllerRef.current = controller

    setIsLoading(true)
    setError(null)
    setResult(null)

    try {
      const response = await runVisualQuery(request, controller.signal)

      if (!mountedRef.current) {
        return null
      }

      setResult(response)
      return response
    } catch (caughtError) {
      if (!mountedRef.current || isAbortError(caughtError)) {
        return null
      }

      setError(caughtError instanceof ApiError ? caughtError : caughtError ?? new Error('An unexpected error occurred.'))
      return null
    } finally {
      if (mountedRef.current && controllerRef.current === controller) {
        setIsLoading(false)
      }
    }
  }, [isLoading])

  return {
    result,
    error,
    isLoading,
    executeVisualQuery,
    reset,
  }
}
