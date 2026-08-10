import { useState } from 'react'
import { deleteDashboard } from '../../api/dashboards/dashboardsApi'
import ErrorMessage from '../../components/feedback/ErrorMessage'
import LoadingIndicator from '../../components/feedback/LoadingIndicator'
import DashboardWidget from '../../components/dashboards/DashboardWidget'
import { useDashboardDetails } from '../../hooks/useDashboardDetails'

interface DashboardDetailsPageProps {
  dashboardId: string
  onBackToDashboards: () => void
}

function formatCreatedAt(createdAtUtc?: string): string | null {
  if (!createdAtUtc) {
    return null
  }

  const date = new Date(createdAtUtc)

  if (Number.isNaN(date.getTime())) {
    return null
  }

  return date.toLocaleString()
}

export default function DashboardDetailsPage({ dashboardId, onBackToDashboards }: DashboardDetailsPageProps): JSX.Element {
  const { dashboard, widgets, isLoading, error } = useDashboardDetails(dashboardId)
  const [isDeleting, setIsDeleting] = useState(false)
  const [deleteError, setDeleteError] = useState<unknown | null>(null)

  const handleDelete = async () => {
    const confirmed = window.confirm('Delete this dashboard?')

    if (!confirmed) {
      return
    }

    setIsDeleting(true)
    setDeleteError(null)

    try {
      await deleteDashboard(dashboardId)
      onBackToDashboards()
    } catch (caughtError) {
      setDeleteError(caughtError)
    } finally {
      setIsDeleting(false)
    }
  }

  const createdAtText = formatCreatedAt(dashboard?.createdAtUtc)
  const widgetCount = dashboard?.widgets?.length ?? 0

  return (
    <div className="page-card">
      <div className="page-card__body stack">
        <header className="dashboard-details__header">
          <div className="stack">
            <div>
              <h2 className="page-heading">{dashboard?.name ?? 'Dashboard'}</h2>
              <p className="page-lead">
                {dashboard ? (dashboard.isDynamic ? 'Dynamic dashboard' : 'Static dashboard') : 'Dashboard details'}
                {dashboard ? ` · ${widgetCount} widgets` : ''}
              </p>
              {createdAtText ? <p className="page-lead">Created: {createdAtText}</p> : null}
            </div>
          </div>

          <div className="dashboard-details__actions">
            <button className="secondary-button" type="button" onClick={onBackToDashboards}>
              Back
            </button>
            <button className="danger-button" type="button" onClick={handleDelete} disabled={isLoading || isDeleting || !dashboard}>
              {isDeleting ? 'Deleting...' : 'Delete'}
            </button>
          </div>
        </header>

        {isLoading ? <LoadingIndicator label="Loading dashboard..." /> : null}

        {!isLoading ? <ErrorMessage error={deleteError ?? error} /> : null}

        {dashboard ? (
          <section className="dashboard-details__grid" aria-label="Dashboard widgets">
            {widgets.map((widgetState) => (
              <DashboardWidget key={widgetState.widget.id} executionState={widgetState} />
            ))}

            {widgets.length === 0 ? (
              <div className="saved-queries-empty-state">
                <h3 className="saved-queries-empty-state__title">No widgets yet.</h3>
                <p className="saved-queries-empty-state__body">
                  Add widgets when creating the dashboard.
                </p>
              </div>
            ) : null}
          </section>
        ) : null}
      </div>
    </div>
  )
}
