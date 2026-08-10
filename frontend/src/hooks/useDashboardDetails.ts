import { useCallback, useEffect, useRef, useState } from 'react'
import { getDashboardById } from '../api/dashboards/dashboardsApi'
import { askQuestion } from '../api/analytics/analyticsApi'
import { getSavedQueries } from '../api/saved-queries/savedQueriesApi'
import { ApiError, isAbortError } from '../api/http/apiError'
import type { AnalyticsQuestionResponse } from '../models/analytics/analyticsModels'
import type { AnalyticsDashboard, AnalyticsDashboardWidget } from '../models/dashboards/dashboardModels'
import type { SavedAnalyticsQuery } from '../models/saved-queries/savedQueriesModels'

export type DashboardWidgetStatus = 'loading' | 'success' | 'error' | 'missing'

export interface DashboardWidgetExecutionState {
  widget: AnalyticsDashboardWidget
  savedQuery: SavedAnalyticsQuery | null
  status: DashboardWidgetStatus
  result: AnalyticsQuestionResponse | null
  error: unknown | null
}

interface UseDashboardDetailsState {
  dashboard: AnalyticsDashboard | null
  widgets: DashboardWidgetExecutionState[]
  isLoading: boolean
  error: unknown | null
  refresh: () => Promise<void>
}

export function useDashboardDetails(dashboardId: string): UseDashboardDetailsState {
  const [dashboard, setDashboard] = useState<AnalyticsDashboard | null>(null)
  const [widgets, setWidgets] = useState<DashboardWidgetExecutionState[]>([])
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
    setDashboard(null)
    setWidgets([])

    try {
      const [dashboardResponse, savedQueriesResponse] = await Promise.all([
        getDashboardById(dashboardId, controller.signal),
        getSavedQueries(controller.signal),
      ])

      if (!mountedRef.current) {
        return
      }

      setDashboard(dashboardResponse)

      const savedQueriesById = new Map(savedQueriesResponse.map((savedQuery) => [savedQuery.id, savedQuery]))
      const initialStates: DashboardWidgetExecutionState[] = dashboardResponse.widgets
        .slice()
        .sort((left, right) => left.displayOrder - right.displayOrder)
        .map((widget) => ({
          widget,
          savedQuery: savedQueriesById.get(widget.savedQueryId) ?? null,
          status: savedQueriesById.has(widget.savedQueryId) ? 'loading' : 'missing',
          result: null,
          error: null,
        }))

      setWidgets(initialStates)

      const executionPromises = initialStates
        .filter((state) => state.status === 'loading' && state.savedQuery)
        .map((state) => {
          const savedQuery = state.savedQuery as SavedAnalyticsQuery

          return askQuestion(savedQuery.question, controller.signal)
            .then((result) => {
              if (!mountedRef.current) {
                return
              }

              setWidgets((currentWidgets) =>
                currentWidgets.map((currentWidget) =>
                  currentWidget.widget.id === state.widget.id
                    ? {
                        ...currentWidget,
                        status: 'success',
                        result,
                        error: null,
                      }
                    : currentWidget,
                ),
              )
            })
            .catch((caughtError) => {
              if (!mountedRef.current || isAbortError(caughtError)) {
                return
              }

              setWidgets((currentWidgets) =>
                currentWidgets.map((currentWidget) =>
                  currentWidget.widget.id === state.widget.id
                    ? {
                        ...currentWidget,
                        status: 'error',
                        result: null,
                        error: caughtError instanceof ApiError ? caughtError : caughtError ?? new Error('An unexpected error occurred.'),
                      }
                    : currentWidget,
                ),
              )
            })
        })

      void Promise.allSettled(executionPromises)
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
  }, [dashboardId])

  useEffect(() => {
    void load()

    return () => {
      mountedRef.current = false
      controllerRef.current?.abort()
    }
  }, [load])

  return {
    dashboard,
    widgets,
    isLoading,
    error,
    refresh: load,
  }
}
