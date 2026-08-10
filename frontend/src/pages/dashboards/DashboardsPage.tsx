import { useState } from 'react'
import { createDashboard, deleteDashboard } from '../../api/dashboards/dashboardsApi'
import { useSavedQueries } from '../../hooks/useSavedQueries'
import { useDashboards } from '../../hooks/useDashboards'
import ErrorMessage from '../../components/feedback/ErrorMessage'
import LoadingIndicator from '../../components/feedback/LoadingIndicator'
import CreateDashboardPanel from '../../components/dashboards/CreateDashboardPanel'
import DashboardCard from '../../components/dashboards/DashboardCard'
import type { CreateAnalyticsDashboardRequest } from '../../models/dashboards/dashboardModels'

interface DashboardsPageProps {
  onOpenDashboard: (id: string) => void
}

export default function DashboardsPage({ onOpenDashboard }: DashboardsPageProps): JSX.Element {
  const { dashboards, isLoading, error, refresh } = useDashboards()
  const { savedQueries, isLoading: isSavedQueriesLoading } = useSavedQueries()
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [isCreating, setIsCreating] = useState(false)
  const [createError, setCreateError] = useState<unknown | null>(null)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [actionError, setActionError] = useState<unknown | null>(null)

  const handleCreateDashboard = async (request: CreateAnalyticsDashboardRequest) => {
    setIsCreating(true)
    setCreateError(null)

    try {
      const createdDashboard = await createDashboard(request)
      setIsCreateOpen(false)
      onOpenDashboard(createdDashboard.id)
      await refresh()
    } catch (caughtError) {
      setCreateError(caughtError)
    } finally {
      setIsCreating(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (deletingId) {
      return
    }

    const confirmed = window.confirm('Delete this dashboard?')

    if (!confirmed) {
      return
    }

    setDeletingId(id)
    setActionError(null)

    try {
      await deleteDashboard(id)
      await refresh()
    } catch (caughtError) {
      setActionError(caughtError)
    } finally {
      setDeletingId(null)
    }
  }

  const combinedError = actionError ?? error

  return (
    <div className="page-card">
      <div className="page-card__body stack">
        <header className="dashboard-list__header">
          <div>
            <h2 className="page-heading">Dashboards</h2>
            <p className="page-lead">Create dashboards from saved analytics questions and review their widget results.</p>
          </div>

          <button className="primary-button" type="button" onClick={() => setIsCreateOpen(true)}>
            Create Dashboard
          </button>
        </header>

        {isCreateOpen ? (
          <CreateDashboardPanel
            savedQueries={savedQueries}
            isSavedQueriesLoading={isSavedQueriesLoading}
            isSubmitting={isCreating}
            error={createError}
            onSubmit={handleCreateDashboard}
            onCancel={() => {
              setIsCreateOpen(false)
              setCreateError(null)
            }}
          />
        ) : null}

        {isLoading ? <LoadingIndicator label="Loading dashboards..." /> : null}
        {!isLoading ? <ErrorMessage error={combinedError} /> : null}

        {!isLoading && !combinedError && dashboards.length === 0 ? (
          <section className="saved-queries-empty-state" aria-labelledby="dashboards-empty-state-title">
            <h3 className="saved-queries-empty-state__title" id="dashboards-empty-state-title">
              No dashboards yet.
            </h3>
            <p className="saved-queries-empty-state__body">
              Create a dashboard from your saved analytics questions.
            </p>
            <button className="primary-button" type="button" onClick={() => setIsCreateOpen(true)}>
              Create Dashboard
            </button>
          </section>
        ) : null}

        {!isLoading && !combinedError && dashboards.length > 0 ? (
          <div className="dashboard-list">
            {dashboards.map((dashboard) => (
              <DashboardCard key={dashboard.id} dashboard={dashboard} onOpen={onOpenDashboard} onDelete={handleDelete} />
            ))}
          </div>
        ) : null}

        {deletingId ? <LoadingIndicator label="Deleting dashboard..." /> : null}
      </div>
    </div>
  )
}
