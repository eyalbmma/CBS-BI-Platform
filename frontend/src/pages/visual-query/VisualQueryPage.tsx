import { useEffect, useMemo, useState } from 'react'
import GeneratedSql from '../../components/analytics/GeneratedSql'
import ResultTable from '../../components/analytics/ResultTable'
import ErrorMessage from '../../components/feedback/ErrorMessage'
import LoadingIndicator from '../../components/feedback/LoadingIndicator'
import {
  DEFAULT_VISUAL_QUERY_DOMAIN,
  DEFAULT_VISUAL_QUERY_LIMIT,
  DEFAULT_VISUAL_QUERY_METRIC,
  DEFAULT_VISUAL_QUERY_SORT_OPTION_ID,
  DEFAULT_VISUAL_QUERY_YEAR_OPTION_ID,
  MAX_VISUAL_QUERY_LIMIT,
  MIN_VISUAL_QUERY_LIMIT,
  VISUAL_QUERY_DOMAIN_OPTIONS,
  VISUAL_QUERY_SORT_OPTIONS,
  VISUAL_QUERY_YEAR_OPTIONS,
  getMetricsForDomain,
  getSortDirectionByOptionId,
  getYearByOptionId,
  type VisualQuerySortOptionId,
  type VisualQueryYearOptionId,
} from '../../config/visualQueryOptions'
import { useVisualQuery } from '../../hooks/useVisualQuery'
import type { VisualQueryDomain, VisualQueryMetric } from '../../models/visual-query/visualQueryModels'

function parseLimitValue(value: string): number | null {
  if (!/^\d+$/.test(value.trim())) {
    return null
  }

  const numericValue = Number(value)

  if (!Number.isInteger(numericValue)) {
    return null
  }

  return numericValue
}

export default function VisualQueryPage(): JSX.Element {
  const [domain, setDomain] = useState<VisualQueryDomain>(DEFAULT_VISUAL_QUERY_DOMAIN)
  const [metric, setMetric] = useState<VisualQueryMetric>(DEFAULT_VISUAL_QUERY_METRIC)
  const [yearOptionId, setYearOptionId] = useState<VisualQueryYearOptionId>(DEFAULT_VISUAL_QUERY_YEAR_OPTION_ID)
  const [sortOptionId, setSortOptionId] = useState<VisualQuerySortOptionId>(DEFAULT_VISUAL_QUERY_SORT_OPTION_ID)
  const [limitInput, setLimitInput] = useState<string>(String(DEFAULT_VISUAL_QUERY_LIMIT))
  const [validationError, setValidationError] = useState<unknown | null>(null)
  const { result, error, isLoading, executeVisualQuery } = useVisualQuery()

  const metricOptions = useMemo(() => getMetricsForDomain(domain), [domain])

  useEffect(() => {
    if (metricOptions.length === 0) {
      return
    }

    if (!metricOptions.includes(metric)) {
      setMetric(metricOptions[0])
    }
  }, [domain, metric, metricOptions])

  const combinedError = validationError ?? error

  return (
    <div className="page-card">
      <div className="page-card__body stack">
        <header className="stack">
          <div>
            <h2 className="page-heading">Visual Query</h2>
            <p className="page-lead">
              Build a structured analytics request by selecting domain, metric, year, sort direction, and result limit.
            </p>
          </div>
        </header>

        <form
          className="visual-query-form"
          onSubmit={async (event) => {
            event.preventDefault()

            const parsedLimit = parseLimitValue(limitInput)

            if (parsedLimit === null || parsedLimit < MIN_VISUAL_QUERY_LIMIT || parsedLimit > MAX_VISUAL_QUERY_LIMIT) {
              setValidationError(new Error(`Limit must be an integer between ${MIN_VISUAL_QUERY_LIMIT} and ${MAX_VISUAL_QUERY_LIMIT}.`))
              return
            }

            setValidationError(null)

            await executeVisualQuery({
              domain,
              metric,
              year: getYearByOptionId(yearOptionId),
              sortDirection: getSortDirectionByOptionId(sortOptionId),
              limit: parsedLimit,
            })
          }}
        >
          <div className="visual-query-form__grid">
            <label className="field" htmlFor="visual-query-domain">
              <span className="field__label">Domain</span>
              <select
                id="visual-query-domain"
                className="save-panel__input"
                value={domain}
                onChange={(event) => setDomain(event.target.value as VisualQueryDomain)}
                disabled={isLoading}
              >
                {VISUAL_QUERY_DOMAIN_OPTIONS.map((option) => (
                  <option key={option.domain} value={option.domain}>
                    {option.domain}
                  </option>
                ))}
              </select>
            </label>

            <label className="field" htmlFor="visual-query-metric">
              <span className="field__label">Metric</span>
              <select
                id="visual-query-metric"
                className="save-panel__input"
                value={metric}
                onChange={(event) => setMetric(event.target.value as VisualQueryMetric)}
                disabled={isLoading}
              >
                {metricOptions.map((optionMetric) => (
                  <option key={optionMetric} value={optionMetric}>
                    {optionMetric}
                  </option>
                ))}
              </select>
            </label>

            <label className="field" htmlFor="visual-query-year">
              <span className="field__label">Year</span>
              <select
                id="visual-query-year"
                className="save-panel__input"
                value={yearOptionId}
                onChange={(event) => setYearOptionId(event.target.value as VisualQueryYearOptionId)}
                disabled={isLoading}
              >
                {VISUAL_QUERY_YEAR_OPTIONS.map((option) => (
                  <option key={option.id} value={option.id}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>

            <label className="field" htmlFor="visual-query-sort">
              <span className="field__label">Sort</span>
              <select
                id="visual-query-sort"
                className="save-panel__input"
                value={sortOptionId}
                onChange={(event) => setSortOptionId(event.target.value as VisualQuerySortOptionId)}
                disabled={isLoading}
              >
                {VISUAL_QUERY_SORT_OPTIONS.map((option) => (
                  <option key={option.id} value={option.id}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>

            <label className="field" htmlFor="visual-query-limit">
              <span className="field__label">Limit</span>
              <input
                id="visual-query-limit"
                className="save-panel__input"
                type="number"
                min={MIN_VISUAL_QUERY_LIMIT}
                max={MAX_VISUAL_QUERY_LIMIT}
                value={limitInput}
                onChange={(event) => setLimitInput(event.target.value)}
                disabled={isLoading}
                inputMode="numeric"
              />
            </label>
          </div>

          <div className="form-actions">
            <button className="primary-button" type="submit" disabled={isLoading}>
              Run Query
            </button>
            {isLoading ? <LoadingIndicator label="Running visual query..." /> : null}
          </div>
        </form>

        <ErrorMessage error={combinedError} />

        {result ? (
          <section className="result-section" aria-labelledby="visual-query-result-title">
            <h2 className="result-section__title" id="visual-query-result-title">Result</h2>
            <ResultTable columns={result.result.columns} rows={result.result.rows} />
            <GeneratedSql sql={result.sql} />
          </section>
        ) : null}
      </div>
    </div>
  )
}
