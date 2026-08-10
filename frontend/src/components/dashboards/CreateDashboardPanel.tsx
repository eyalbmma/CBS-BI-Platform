import { useEffect, useMemo, useState, type FormEvent } from 'react'
import ErrorMessage from '../feedback/ErrorMessage'
import LoadingIndicator from '../feedback/LoadingIndicator'
import type { SavedAnalyticsQuery } from '../../models/saved-queries/savedQueriesModels'
import type {
  CreateAnalyticsDashboardRequest,
  CreateAnalyticsDashboardWidgetRequest,
  VisualizationType,
} from '../../models/dashboards/dashboardModels'

interface WidgetDraft {
  clientId: string
  savedQueryId: string
  title: string
  visualizationType: VisualizationType
  titleTouched: boolean
}

interface CreateDashboardPanelProps {
  savedQueries: SavedAnalyticsQuery[]
  isSavedQueriesLoading: boolean
  isSubmitting: boolean
  error: unknown | null
  onSubmit: (request: CreateAnalyticsDashboardRequest) => Promise<void>
  onCancel: () => void
}

const visualizationOptions: VisualizationType[] = ['Number', 'Table', 'BarChart']

function createClientId(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    return crypto.randomUUID()
  }

  return `${Date.now()}-${Math.random().toString(16).slice(2)}`
}

function createDefaultWidgetDraft(savedQueries: SavedAnalyticsQuery[]): WidgetDraft {
  const firstSavedQuery = savedQueries[0]

  return {
    clientId: createClientId(),
    savedQueryId: firstSavedQuery?.id ?? '',
    title: firstSavedQuery?.name ?? '',
    visualizationType: 'Number',
    titleTouched: false,
  }
}

function toDashboardWidgetRequest(widget: WidgetDraft, displayOrder: number): CreateAnalyticsDashboardWidgetRequest {
  return {
    title: widget.title.trim(),
    savedQueryId: widget.savedQueryId,
    visualizationType: widget.visualizationType,
    displayOrder,
  }
}

export default function CreateDashboardPanel({
  savedQueries,
  isSavedQueriesLoading,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: CreateDashboardPanelProps): JSX.Element {
  const [dashboardName, setDashboardName] = useState('')
  const [isDynamic, setIsDynamic] = useState(false)
  const [widgetDrafts, setWidgetDrafts] = useState<WidgetDraft[]>(() => [createDefaultWidgetDraft(savedQueries)])
  const [validationError, setValidationError] = useState<string | null>(null)

  const canEditWidgets = !isSubmitting && !isSavedQueriesLoading && savedQueries.length > 0

  const savedQueryNameById = useMemo(() => new Map(savedQueries.map((savedQuery) => [savedQuery.id, savedQuery.name])), [savedQueries])

  useEffect(() => {
    if (savedQueries.length === 0) {
      return
    }

    setWidgetDrafts((currentDrafts) =>
      currentDrafts.map((draft) => {
        if (draft.savedQueryId || draft.titleTouched) {
          return draft
        }

        const firstSavedQuery = savedQueries[0]

        return {
          ...draft,
          savedQueryId: firstSavedQuery.id,
          title: firstSavedQuery.name,
        }
      }),
    )
  }, [savedQueries])

  const updateWidget = (clientId: string, updater: (current: WidgetDraft) => WidgetDraft) => {
    setWidgetDrafts((currentDrafts) => currentDrafts.map((draft) => (draft.clientId === clientId ? updater(draft) : draft)))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmedDashboardName = dashboardName.trim()
    const validWidgets = widgetDrafts.filter((widget) => widget.savedQueryId && widget.title.trim())

    if (!trimmedDashboardName) {
      setValidationError('Dashboard name is required.')
      return
    }

    if (validWidgets.length === 0) {
      setValidationError('Add at least one widget and choose a saved query.')
      return
    }

    if (validWidgets.length !== widgetDrafts.length) {
      setValidationError('Each widget must have a title and a saved query.')
      return
    }

    setValidationError(null)

    await onSubmit({
      name: trimmedDashboardName,
      isDynamic,
      widgets: validWidgets.map(toDashboardWidgetRequest),
    })
  }

  return (
    <section className="dashboard-create-panel" aria-labelledby="create-dashboard-title">
      <div className="dashboard-create-panel__header">
        <div>
          <h3 className="dashboard-create-panel__title" id="create-dashboard-title">Create Dashboard</h3>
          <p className="dashboard-create-panel__subtitle">
            Build a dashboard from your saved analytics questions.
          </p>
        </div>
        <button className="secondary-button" type="button" onClick={onCancel} disabled={isSubmitting}>
          Close
        </button>
      </div>

      <form className="form" onSubmit={handleSubmit}>
        <label className="field" htmlFor="dashboard-name">
          <span className="field__label">Dashboard Name</span>
          <input
            id="dashboard-name"
            className="save-panel__input"
            type="text"
            value={dashboardName}
            onChange={(event) => setDashboardName(event.target.value)}
            disabled={isSubmitting}
            autoComplete="off"
          />
        </label>

        <label className="dashboard-create-panel__toggle">
          <input
            type="checkbox"
            checked={isDynamic}
            onChange={(event) => setIsDynamic(event.target.checked)}
            disabled={isSubmitting}
          />
          <span>Dynamic dashboard</span>
        </label>

        {isDynamic ? (
          <p className="dashboard-create-panel__info">
            Dynamic dashboard filters are planned for a later POC phase.
          </p>
        ) : null}

        {isSavedQueriesLoading ? <LoadingIndicator label="Loading saved queries..." /> : null}

        {!isSavedQueriesLoading && savedQueries.length === 0 ? (
          <p className="dashboard-create-panel__info">
            You need at least one saved query before creating a dashboard.
          </p>
        ) : null}

        <div className="dashboard-widget-editor-list">
          {widgetDrafts.map((widgetDraft, index) => {
            const selectedQueryName = savedQueryNameById.get(widgetDraft.savedQueryId) ?? ''

            return (
              <section key={widgetDraft.clientId} className="dashboard-widget-editor">
                <div className="dashboard-widget-editor__header">
                  <h4 className="dashboard-widget-editor__title">Widget {index + 1}</h4>
                  <button
                    className="danger-button"
                    type="button"
                    onClick={() => {
                      setWidgetDrafts((currentDrafts) => currentDrafts.length > 1 ? currentDrafts.filter((draft) => draft.clientId !== widgetDraft.clientId) : currentDrafts)
                    }}
                    disabled={isSubmitting || widgetDrafts.length === 1}
                  >
                    Remove
                  </button>
                </div>

                <label className="field" htmlFor={`saved-query-${widgetDraft.clientId}`}>
                  <span className="field__label">Saved Query</span>
                  <select
                    id={`saved-query-${widgetDraft.clientId}`}
                    className="save-panel__input"
                    value={widgetDraft.savedQueryId}
                    disabled={!canEditWidgets}
                    onChange={(event) => {
                      const savedQueryId = event.target.value
                      const nextQueryName = savedQueryNameById.get(savedQueryId) ?? ''

                      updateWidget(widgetDraft.clientId, (current) => ({
                        ...current,
                        savedQueryId,
                        title: current.titleTouched ? current.title : nextQueryName,
                      }))
                    }}
                  >
                    <option value="">Select a saved query</option>
                    {savedQueries.map((savedQuery) => (
                      <option key={savedQuery.id} value={savedQuery.id}>
                        {savedQuery.name}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="field" htmlFor={`widget-title-${widgetDraft.clientId}`}>
                  <span className="field__label">Widget Title</span>
                  <input
                    id={`widget-title-${widgetDraft.clientId}`}
                    className="save-panel__input"
                    type="text"
                    value={widgetDraft.title}
                    onChange={(event) => {
                      const title = event.target.value
                      updateWidget(widgetDraft.clientId, (current) => ({
                        ...current,
                        title,
                        titleTouched: true,
                      }))
                    }}
                    disabled={!canEditWidgets}
                    autoComplete="off"
                  />
                </label>

                <label className="field" htmlFor={`widget-visualization-${widgetDraft.clientId}`}>
                  <span className="field__label">Visualization</span>
                  <select
                    id={`widget-visualization-${widgetDraft.clientId}`}
                    className="save-panel__input"
                    value={widgetDraft.visualizationType}
                    disabled={!canEditWidgets}
                    onChange={(event) => {
                      updateWidget(widgetDraft.clientId, (current) => ({
                        ...current,
                        visualizationType: event.target.value as VisualizationType,
                      }))
                    }}
                  >
                    {visualizationOptions.map((visualizationType) => (
                      <option key={visualizationType} value={visualizationType}>
                        {visualizationType}
                      </option>
                    ))}
                  </select>
                </label>

                {selectedQueryName ? <p className="dashboard-widget-editor__hint">Selected saved query: {selectedQueryName}</p> : null}
              </section>
            )
          })}
        </div>

        <div className="form-actions">
          <button
            className="secondary-button"
            type="button"
            onClick={() => {
              setWidgetDrafts((currentDrafts) => [...currentDrafts, createDefaultWidgetDraft(savedQueries)])
            }}
            disabled={isSubmitting || isSavedQueriesLoading || savedQueries.length === 0}
          >
            Add Widget
          </button>

          <button className="primary-button" type="submit" disabled={isSubmitting || isSavedQueriesLoading}>
            {isSubmitting ? 'Creating...' : 'Create Dashboard'}
          </button>
        </div>

        <ErrorMessage error={error} />

        {validationError ? <p className="dashboard-create-panel__validation">{validationError}</p> : null}
      </form>
    </section>
  )
}
