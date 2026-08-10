import { useCallback, useEffect, useRef, useState } from 'react'
import { getDashboards } from '../api/dashboards/dashboardsApi'
import { ApiError, isAbortError } from '../api/http/apiError'
import type { AnalyticsDashboard } from '../models/dashboards/dashboardModels'

interface UseDashboardsState {
  dashboards: AnalyticsDashboard[]
  isLoading: boolean
  error: unknown | null
  refresh: () => Promise<void>
}

export function useDashboards(): UseDashboardsState {
  const [dashboards, setDashboards] = useState<AnalyticsDashboard[]>([])
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
      const response = await getDashboards(controller.signal)

      if (!mountedRef.current) {
        return
      }

      setDashboards(response)
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
    dashboards,
    isLoading,
    error,
    refresh: load,
  }
}
