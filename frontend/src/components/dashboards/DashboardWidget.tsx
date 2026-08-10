import ErrorMessage from '../feedback/ErrorMessage'
import LoadingIndicator from '../feedback/LoadingIndicator'
import ResultTable from '../analytics/ResultTable'
import type { AnalyticsQuestionResponse } from '../../models/analytics/analyticsModels'
import type { AnalyticsDashboardWidget, VisualizationType } from '../../models/dashboards/dashboardModels'
import type { DashboardWidgetExecutionState } from '../../hooks/useDashboardDetails'

interface DashboardWidgetProps {
  executionState: DashboardWidgetExecutionState
}

function formatDisplayValue(value: unknown): string {
  if (value === null || value === undefined) {
    return 'No data'
  }

  if (typeof value === 'number') {
    return new Intl.NumberFormat().format(value)
  }

  if (typeof value === 'boolean') {
    return value ? 'True' : 'False'
  }

  if (typeof value === 'string') {
    const numericValue = Number(value)

    if (value.trim().length > 0 && Number.isFinite(numericValue)) {
      return new Intl.NumberFormat().format(numericValue)
    }

    return value
  }

  try {
    return JSON.stringify(value)
  } catch {
    return String(value)
  }
}

function getFirstDisplayValue(result: AnalyticsQuestionResponse): unknown {
  const firstColumn = result.result.columns[0]

  if (!firstColumn || result.result.rows.length === 0) {
    return null
  }

  return result.result.rows[0]?.[firstColumn] ?? null
}

function isNumericLikeValue(value: unknown): boolean {
  if (typeof value === 'number') {
    return Number.isFinite(value)
  }

  if (typeof value === 'string' && value.trim().length > 0) {
    return Number.isFinite(Number(value))
  }

  return false
}

function getBarChartData(result: AnalyticsQuestionResponse): { labelColumn: string; numericColumn: string; rows: Array<{ label: string; value: number }> } | null {
  if (result.result.columns.length < 2 || result.result.rows.length === 0) {
    return null
  }

  const numericColumn = result.result.columns.find((column) =>
    result.result.rows.some((row) => isNumericLikeValue(row[column])),
  )

  if (!numericColumn) {
    return null
  }

  const labelColumn = result.result.columns.find((column) => column !== numericColumn)

  if (!labelColumn) {
    return null
  }

  const rows = result.result.rows
    .map((row) => ({
      label: String(row[labelColumn] ?? ''),
      value: typeof row[numericColumn] === 'number' ? row[numericColumn] : Number(row[numericColumn]),
    }))
    .filter((row) => row.label.trim().length > 0 && Number.isFinite(row.value))

  return rows.length > 0 ? { labelColumn, numericColumn, rows } : null
}

function NumberWidget({ result }: { result: AnalyticsQuestionResponse }): JSX.Element {
  const firstValue = getFirstDisplayValue(result)
  const label = result.result.columns[0] ?? 'Value'

  return (
    <div className="dashboard-number-widget">
      <p className="dashboard-number-widget__label">{label}</p>
      <p className="dashboard-number-widget__value">{formatDisplayValue(firstValue)}</p>
    </div>
  )
}

function TableWidget({ result }: { result: AnalyticsQuestionResponse }): JSX.Element {
  return <ResultTable columns={result.result.columns} rows={result.result.rows} />
}

function BarChartWidget({ result }: { result: AnalyticsQuestionResponse }): JSX.Element {
  const chartData = getBarChartData(result)

  if (!chartData) {
    return <p className="dashboard-widget__inline-message">This result cannot be displayed as a bar chart.</p>
  }

  const maxValue = Math.max(...chartData.rows.map((row) => row.value), 0)

  return (
    <div className="dashboard-bar-chart" role="img" aria-label={`Bar chart with ${chartData.rows.length} items`}>
      {chartData.rows.map((row) => {
        const width = maxValue > 0 ? Math.max((row.value / maxValue) * 100, 4) : 0

        return (
          <div key={`${row.label}-${row.value}`} className="dashboard-bar-chart__row">
            <div className="dashboard-bar-chart__label">{row.label}</div>
            <div className="dashboard-bar-chart__track">
              <div className="dashboard-bar-chart__bar" style={{ width: `${width}%` }} />
            </div>
            <div className="dashboard-bar-chart__value">{formatDisplayValue(row.value)}</div>
          </div>
        )
      })}
    </div>
  )
}

function renderVisualization(widget: AnalyticsDashboardWidget, result: AnalyticsQuestionResponse): JSX.Element {
  switch (widget.visualizationType) {
    case 'Number':
      return <NumberWidget result={result} />
    case 'BarChart':
      return <BarChartWidget result={result} />
    case 'Table':
    default:
      return <TableWidget result={result} />
  }
}

export default function DashboardWidget({ executionState }: DashboardWidgetProps): JSX.Element {
  const { widget, savedQuery, status, result, error } = executionState

  return (
    <article className="dashboard-widget-card">
      <div className="dashboard-widget-card__header">
        <div>
          <h4 className="dashboard-widget-card__title">{widget.title}</h4>
          <p className="dashboard-widget-card__subtitle">
            {widget.visualizationType} visualization
            {savedQuery ? ` · ${savedQuery.name}` : ''}
          </p>
        </div>
      </div>

      <div className="dashboard-widget-card__body">
        {status === 'loading' ? <LoadingIndicator label="Loading widget..." /> : null}
        {status === 'missing' ? <p className="dashboard-widget__inline-message">Saved query is no longer available.</p> : null}
        {status === 'error' ? <ErrorMessage error={error} /> : null}
        {status === 'success' && result ? renderVisualization(widget, result) : null}
      </div>
    </article>
  )
}
