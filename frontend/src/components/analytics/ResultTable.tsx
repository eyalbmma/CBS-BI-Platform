import type { AnalyticsResultRow } from '../../models/analytics/analyticsModels'

interface ResultTableProps {
  columns: string[]
  rows: AnalyticsResultRow[]
}

function formatCellValue(value: unknown): string {
  if (value === null || value === undefined) {
    return '—'
  }

  if (typeof value === 'string') {
    return value
  }

  if (typeof value === 'number' || typeof value === 'boolean') {
    return String(value)
  }

  try {
    return JSON.stringify(value)
  } catch {
    return String(value)
  }
}

export default function ResultTable({ columns, rows }: ResultTableProps): JSX.Element {
  return (
    <div className="result-table__wrapper" aria-label="Analytics result table">
      <table className="result-table">
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={column}>{column}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 ? (
            <tr>
              <td className="result-table__empty" colSpan={Math.max(columns.length, 1)}>
                No rows were returned.
              </td>
            </tr>
          ) : (
            rows.map((row, rowIndex) => (
              <tr key={rowIndex}>
                {columns.map((column) => (
                  <td key={column} dir="auto">
                    {formatCellValue(row[column])}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}
