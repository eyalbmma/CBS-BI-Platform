import type { SavedAnalyticsQuery } from '../../models/saved-queries/savedQueriesModels'

interface SavedQueryCardProps {
  savedQuery: SavedAnalyticsQuery
  isDeleting: boolean
  onRunAgain: (question: string) => void
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

export default function SavedQueryCard({ savedQuery, isDeleting, onRunAgain, onDelete }: SavedQueryCardProps): JSX.Element {
  const createdAtText = formatCreatedAt(savedQuery.createdAtUtc)

  return (
    <article className="saved-query-card">
      <div className="saved-query-card__content">
        <h3 className="saved-query-card__title">{savedQuery.name}</h3>
        <p className="saved-query-card__question" dir="auto">{savedQuery.question}</p>
        {createdAtText ? <p className="saved-query-card__meta">Saved: {createdAtText}</p> : null}
      </div>

      <div className="saved-query-card__actions">
        <button className="secondary-button" type="button" onClick={() => onRunAgain(savedQuery.question)}>
          Run Again
        </button>
        <button className="danger-button" type="button" onClick={() => onDelete(savedQuery.id)} disabled={isDeleting}>
          {isDeleting ? 'Deleting...' : 'Delete'}
        </button>
      </div>
    </article>
  )
}
