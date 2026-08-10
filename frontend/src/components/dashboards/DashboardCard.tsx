import type { AnalyticsDashboard } from '../../models/dashboards/dashboardModels'

interface DashboardCardProps {
  dashboard: AnalyticsDashboard
  onOpen: (id: string) => void
  onDelete: (id: string) => void
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

export default function DashboardCard({ dashboard, onOpen, onDelete }: DashboardCardProps): JSX.Element {
  const createdAtText = formatCreatedAt(dashboard.createdAtUtc)
  const widgetCount = dashboard.widgets?.length ?? 0

  return (
    <article className="dashboard-card">
      <div className="dashboard-card__content">
        <div className="dashboard-card__heading-row">
          <h3 className="dashboard-card__title">{dashboard.name}</h3>
          <span className="dashboard-card__badge">{dashboard.isDynamic ? 'Dynamic dashboard' : 'Static dashboard'}</span>
        </div>
        <p className="dashboard-card__meta">{widgetCount} widgets</p>
        {createdAtText ? <p className="dashboard-card__meta">Created: {createdAtText}</p> : null}
      </div>

      <div className="dashboard-card__actions">
        <button className="secondary-button" type="button" onClick={() => onOpen(dashboard.id)}>
          Open
        </button>
        <button className="danger-button" type="button" onClick={() => onDelete(dashboard.id)}>
          Delete
        </button>
      </div>
    </article>
  )
}
