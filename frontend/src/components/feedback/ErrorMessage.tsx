import { getApiErrorMessage } from '../../api/http/apiError'

interface ErrorMessageProps {
  error: unknown | null
}

export default function ErrorMessage({ error }: ErrorMessageProps): JSX.Element | null {
  if (!error) {
    return null
  }

  return (
    <div className="error-message" role="alert">
      <p className="error-message__title">Unable to complete request</p>
      <p className="error-message__body">{getApiErrorMessage(error)}</p>
    </div>
  )
}
