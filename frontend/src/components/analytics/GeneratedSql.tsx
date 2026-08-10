interface GeneratedSqlProps {
  sql: string
}

export default function GeneratedSql({ sql }: GeneratedSqlProps): JSX.Element {
  return (
    <section className="generated-sql" aria-labelledby="generated-sql-title">
      <h3 className="generated-sql__title" id="generated-sql-title">Generated SQL</h3>
      <pre className="generated-sql__code" dir="ltr">
        <code>{sql}</code>
      </pre>
    </section>
  )
}
