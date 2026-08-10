import { useEffect, useState } from 'react'
import AppLayout, { type AppRoutePath } from '../components/layout/AppLayout'
import AskDataPage from '../pages/ask-data/AskDataPage'
import DashboardsPage from '../pages/dashboards/DashboardsPage'
import DashboardDetailsPage from '../pages/dashboards/DashboardDetailsPage'
import SavedQueriesPage from '../pages/saved-queries/SavedQueriesPage'
import VisualQueryPage from '../pages/visual-query/VisualQueryPage'

const DEFAULT_ROUTE: AppRoutePath = '/ask-data'
const DASHBOARD_DETAILS_PREFIX = '/dashboards/'

interface AppHistoryState {
  prefillQuestion?: string
}

function isAppRoute(pathname: string): pathname is AppRoutePath {
  return pathname === '/ask-data' || pathname === '/saved-queries' || pathname === '/dashboards' || pathname === '/visual-query'
}

function isDashboardDetailsRoute(pathname: string): pathname is `${typeof DASHBOARD_DETAILS_PREFIX}${string}` {
  return pathname.startsWith(DASHBOARD_DETAILS_PREFIX) && pathname.length > DASHBOARD_DETAILS_PREFIX.length
}

function resolveRoute(pathname: string): string {
  if (isAppRoute(pathname) || isDashboardDetailsRoute(pathname)) {
    return pathname
  }

  return DEFAULT_ROUTE
}

function readHistoryState(): AppHistoryState | null {
  const state = window.history.state

  if (!state || typeof state !== 'object') {
    return null
  }

  const record = state as Record<string, unknown>

  return typeof record.prefillQuestion === 'string' ? { prefillQuestion: record.prefillQuestion } : null
}

function getDashboardIdFromPath(pathname: string): string | null {
  if (!isDashboardDetailsRoute(pathname)) {
    return null
  }

  const id = pathname.slice(DASHBOARD_DETAILS_PREFIX.length)

  return id.length > 0 ? decodeURIComponent(id) : null
}

function renderPage(
  path: string,
  historyState: AppHistoryState | null,
  navigate: (path: AppRoutePath, state?: AppHistoryState | null) => void,
): JSX.Element {
  if (isDashboardDetailsRoute(path)) {
    const dashboardId = getDashboardIdFromPath(path)

    if (!dashboardId) {
      return <DashboardsPage onOpenDashboard={(id) => navigate(`/dashboards/${encodeURIComponent(id)}`)} />
    }

    return (
      <DashboardDetailsPage
        dashboardId={dashboardId}
        onBackToDashboards={() => navigate('/dashboards')}
      />
    )
  }

  switch (path as AppRoutePath) {
    case '/saved-queries':
      return (
        <SavedQueriesPage
          onRunAgain={(question) => navigate('/ask-data', { prefillQuestion: question })}
          onAskData={() => navigate('/ask-data')}
        />
      )
    case '/dashboards':
      return <DashboardsPage onOpenDashboard={(id) => navigate(`/dashboards/${encodeURIComponent(id)}`)} />
    case '/visual-query':
      return <VisualQueryPage />
    case '/ask-data':
    default:
      return <AskDataPage initialQuestion={historyState?.prefillQuestion} />
  }
}

export default function AppRoutes(): JSX.Element {
  const [currentPath, setCurrentPath] = useState<string>(() => resolveRoute(window.location.pathname))
  const [historyState, setHistoryState] = useState<AppHistoryState | null>(() => readHistoryState())

  useEffect(() => {
    const handlePopState = () => {
      setCurrentPath(resolveRoute(window.location.pathname))
      setHistoryState(readHistoryState())
    }

    window.addEventListener('popstate', handlePopState)

    return () => {
      window.removeEventListener('popstate', handlePopState)
    }
  }, [])

  useEffect(() => {
    const resolvedPath = resolveRoute(window.location.pathname)

    if (resolvedPath !== currentPath) {
      window.history.replaceState(historyState, '', resolvedPath)
      setCurrentPath(resolvedPath)
    }
  }, [currentPath, historyState])

  const navigate = (path: AppRoutePath, state: AppHistoryState | null = null) => {
    if (path === currentPath) {
      window.history.replaceState(state, '', path)
      setHistoryState(state)
      return
    }

    window.history.pushState(state, '', path)
    setHistoryState(state)
    setCurrentPath(path)
  }

  return <AppLayout currentPath={currentPath} onNavigate={(path) => navigate(path)}>{renderPage(currentPath, historyState, navigate)}</AppLayout>
}
