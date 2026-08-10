import { useEffect, useState } from 'react'
import AnalyticsResult from '../../components/analytics/AnalyticsResult'
import { saveQuery } from '../../api/saved-queries/savedQueriesApi'
import { useAnalyticsQuestion } from '../../hooks/useAnalyticsQuestion'
import SaveQueryPanel from '../../components/saved-queries/SaveQueryPanel'
import ErrorMessage from '../../components/feedback/ErrorMessage'
import LoadingIndicator from '../../components/feedback/LoadingIndicator'

const QUESTION_PLACEHOLDER = 'איזו עיר בעלת שיעור האבטלה הנמוך ביותר?'

interface AskDataPageProps {
  initialQuestion?: string
}

function getSuggestedQueryName(question: string): string {
  const trimmedQuestion = question.trim()

  if (trimmedQuestion.length <= 48) {
    return trimmedQuestion
  }

  return `${trimmedQuestion.slice(0, 45).trimEnd()}...`
}

export default function AskDataPage({ initialQuestion }: AskDataPageProps): JSX.Element {
  const [question, setQuestion] = useState(initialQuestion ?? '')
  const [executedQuestion, setExecutedQuestion] = useState<string | null>(null)
  const [isSavePanelOpen, setIsSavePanelOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [saveError, setSaveError] = useState<unknown | null>(null)
  const [saveSuccessMessage, setSaveSuccessMessage] = useState<string | null>(null)
  const { result, error, isLoading, askAnalyticsQuestion } = useAnalyticsQuestion()

  useEffect(() => {
    setQuestion(initialQuestion ?? '')
  }, [initialQuestion])

  const canSubmit = question.trim().length > 0 && !isLoading

  const handleAsk = async () => {
    const trimmedQuestion = question.trim()
    const response = await askAnalyticsQuestion(trimmedQuestion)

    if (response) {
      setExecutedQuestion(trimmedQuestion)
      setIsSavePanelOpen(false)
      setIsSaving(false)
      setSaveError(null)
      setSaveSuccessMessage(null)
    }
  }

  const handleOpenSavePanel = () => {
    if (!executedQuestion) {
      return
    }

    setSaveError(null)
    setSaveSuccessMessage(null)
    setIsSavePanelOpen(true)
  }

  const handleSaveQuery = async (name: string) => {
    if (!executedQuestion || isSaving) {
      return
    }

    const trimmedName = name.trim()

    if (!trimmedName) {
      setSaveError(new Error('Please enter a name for the saved query.'))
      return
    }

    setIsSaving(true)
    setSaveError(null)
    setSaveSuccessMessage(null)

    try {
      await saveQuery({
        name: trimmedName,
        question: executedQuestion,
      })

      setIsSavePanelOpen(false)
      setSaveSuccessMessage('Query saved successfully.')
    } catch (caughtError) {
      setSaveError(caughtError)
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="page-card">
      <div className="page-card__body stack">
        <header className="stack">
          <div>
            <h2 className="page-heading">Ask CBS Data</h2>
            <p className="page-lead">
              Ask a natural-language analytics question, review the generated SQL, and inspect the returned result set.
            </p>
          </div>
        </header>

        <form
          className="form"
          onSubmit={async (event) => {
            event.preventDefault()
            await handleAsk()
          }}
        >
          <label className="field" htmlFor="analytics-question">
            <span className="field__label">Question</span>
            <textarea
              id="analytics-question"
              className="question-input"
              dir="auto"
              value={question}
              placeholder={QUESTION_PLACEHOLDER}
              onChange={(event) => setQuestion(event.target.value)}
              aria-describedby="question-hint"
            />
          </label>

          <p className="field__hint" id="question-hint">
            Example: {QUESTION_PLACEHOLDER}
          </p>

          <div className="form-actions">
            <button className="primary-button" type="submit" disabled={!canSubmit}>
              Ask
            </button>
            {isLoading ? <LoadingIndicator label="Submitting analytics request..." /> : null}
          </div>
        </form>

        <ErrorMessage error={error} />

        <AnalyticsResult result={result} />

        {result && executedQuestion ? (
          <section className="save-query-section">
            {!isSavePanelOpen ? (
              <div className="save-query-section__actions">
                {saveSuccessMessage ? <p className="status-inline status-inline--success">{saveSuccessMessage}</p> : null}
                <button className="secondary-button" type="button" onClick={handleOpenSavePanel}>
                  Save Query
                </button>
              </div>
            ) : (
              <SaveQueryPanel
                question={executedQuestion}
                initialName={getSuggestedQueryName(executedQuestion)}
                isSaving={isSaving}
                error={saveError}
                successMessage={saveSuccessMessage}
                onSave={handleSaveQuery}
                onCancel={() => {
                  setIsSavePanelOpen(false)
                  setSaveError(null)
                }}
              />
            )}
          </section>
        ) : null}
      </div>
    </div>
  )
}
