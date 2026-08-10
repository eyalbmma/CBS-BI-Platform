import { useEffect, useState } from 'react'
import ErrorMessage from '../feedback/ErrorMessage'

interface SaveQueryPanelProps {
  question: string
  isSaving: boolean
  error: unknown | null
  successMessage: string | null
  onSave: (name: string) => Promise<void>
  onCancel: () => void
  initialName?: string
}

export default function SaveQueryPanel({
  question,
  isSaving,
  error,
  successMessage,
  onSave,
  onCancel,
  initialName = '',
}: SaveQueryPanelProps): JSX.Element {
  const [name, setName] = useState(initialName)

  useEffect(() => {
    setName(initialName)
  }, [initialName])

  return (
    <section className="save-panel" aria-labelledby="save-query-title">
      <div className="save-panel__header">
        <h3 className="save-panel__title" id="save-query-title">Save query</h3>
        <p className="save-panel__subtitle">Store the successful analytics question for later use.</p>
      </div>

      <div className="field">
        <span className="field__label">Question</span>
        <p className="save-panel__question" dir="auto">{question}</p>
      </div>

      <form
        className="form"
        onSubmit={async (event) => {
          event.preventDefault()
          await onSave(name)
        }}
      >
        <label className="field" htmlFor="save-query-name">
          <span className="field__label">Name</span>
          <input
            id="save-query-name"
            className="save-panel__input"
            type="text"
            value={name}
            onChange={(event) => setName(event.target.value)}
            disabled={isSaving}
            autoComplete="off"
          />
        </label>

        <div className="form-actions">
          <button className="primary-button" type="submit" disabled={isSaving || name.trim().length === 0}>
            {isSaving ? 'Saving...' : 'Save'}
          </button>
          <button className="secondary-button" type="button" onClick={onCancel} disabled={isSaving}>
            Cancel
          </button>
        </div>
      </form>

      {successMessage ? <p className="status-inline status-inline--success">{successMessage}</p> : null}
      <ErrorMessage error={error} />
    </section>
  )
}
