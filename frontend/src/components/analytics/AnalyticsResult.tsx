import type { AnalyticsQuestionResponse } from '../../models/analytics/analyticsModels'
import GeneratedSql from './GeneratedSql'
import ResultTable from './ResultTable'

interface AnalyticsResultProps {
  result: AnalyticsQuestionResponse | null
}

export default function AnalyticsResult({ result }: AnalyticsResultProps): JSX.Element | null {
  if (!result) {
    return null
  }

  return (
    <section className="result-section" aria-labelledby="analytics-result-title">
      <h2 className="result-section__title" id="analytics-result-title">Result</h2>
      <ResultTable columns={result.result.columns} rows={result.result.rows} />
      <GeneratedSql sql={result.sql} />
    </section>
  )
}
