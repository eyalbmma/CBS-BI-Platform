import { useState } from 'react'
import { deleteSavedQuery } from '../../api/saved-queries/savedQueriesApi'
import ErrorMessage from '../../components/feedback/ErrorMessage'
import LoadingIndicator from '../../components/feedback/LoadingIndicator'
import SavedQueryCard from '../../components/saved-queries/SavedQueryCard'
import { useSavedQueries } from '../../hooks/useSavedQueries'

interface SavedQueriesPageProps {
  onRunAgain: (question: string) => void
  onAskData: () => void
}

export default function SavedQueriesPage({ onRunAgain, onAskData }: SavedQueriesPageProps): JSX.Element {
  const { savedQueries, isLoading, error, refresh } = useSavedQueries()
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [actionError, setActionError] = useState<unknown | null>(null)

  const handleDelete = async (id: string) => {
    if (deletingId) {
      return
    }

    const confirmed = window.confirm('Delete this saved query?')

    if (!confirmed) {
      return
    }

    setDeletingId(id)
    setActionError(null)

    try {
      await deleteSavedQuery(id)
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
        <header className="stack">
          <div>
            <h2 className="page-heading">Saved Queries</h2>
            <p className="page-lead">
              Review saved business questions, run them again, or remove ones you no longer need.
            </p>
          </div>
        </header>

        {isLoading ? <LoadingIndicator label="Loading saved queries..." /> : null}

        {!isLoading ? <ErrorMessage error={combinedError} /> : null}

        {!isLoading && !combinedError && savedQueries.length === 0 ? (
          <section className="saved-queries-empty-state" aria-labelledby="saved-queries-empty-state-title">
            <h3 className="saved-queries-empty-state__title" id="saved-queries-empty-state-title">
              No saved queries yet.
            </h3>
            <p className="saved-queries-empty-state__body">
              Ask a question in Ask Data and save it for later use.
            </p>
            <button className="primary-button" type="button" onClick={onAskData}>
              Go to Ask Data
            </button>
          </section>
        ) : null}

        {!isLoading && !combinedError && savedQueries.length > 0 ? (
          <div className="saved-query-list">
            {savedQueries.map((savedQuery) => (
              <SavedQueryCard
                key={savedQuery.id}
                savedQuery={savedQuery}
                isDeleting={deletingId === savedQuery.id}
                onRunAgain={onRunAgain}
                onDelete={handleDelete}
              />
            ))}
          </div>
        ) : null}
      </div>
    </div>
  )
}
