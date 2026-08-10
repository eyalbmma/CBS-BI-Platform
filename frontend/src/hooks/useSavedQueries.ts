import { useCallback, useEffect, useRef, useState } from 'react'
import { getSavedQueries } from '../api/saved-queries/savedQueriesApi'
import { ApiError, isAbortError } from '../api/http/apiError'
import type { SavedAnalyticsQuery } from '../models/saved-queries/savedQueriesModels'

interface UseSavedQueriesState {
  savedQueries: SavedAnalyticsQuery[]
  isLoading: boolean
  error: unknown | null
  refresh: () => Promise<void>
}

export function useSavedQueries(): UseSavedQueriesState {
  const [savedQueries, setSavedQueries] = useState<SavedAnalyticsQuery[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<unknown | null>(null)
  const controllerRef = useRef<AbortController | null>(null)
  const mountedRef = useRef(true)

  const load = useCallback(async () => {
    controllerRef.current?.abort()
    const controller = new AbortController()
    controllerRef.current = controller

    setIsLoading(true)
    setError(null)

    try {
      const queries = await getSavedQueries(controller.signal)

      if (!mountedRef.current) {
        return
      }

      setSavedQueries(queries)
    } catch (caughtError) {
      if (!mountedRef.current || isAbortError(caughtError)) {
        return
      }

      setError(caughtError instanceof ApiError ? caughtError : caughtError ?? new Error('An unexpected error occurred.'))
    } finally {
      if (mountedRef.current && controllerRef.current === controller) {
        setIsLoading(false)
      }
    }
  }, [])

  useEffect(() => {
    void load()

    return () => {
      mountedRef.current = false
      controllerRef.current?.abort()
    }
  }, [load])

  return {
    savedQueries,
    isLoading,
    error,
    refresh: load,
  }
}
