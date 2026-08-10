interface LoadingIndicatorProps {
  label?: string
}

export default function LoadingIndicator({ label = 'Loading...' }: LoadingIndicatorProps): JSX.Element {
  return (
    <div className="loading-indicator" role="status" aria-live="polite">
      <span className="loading-indicator__spinner" aria-hidden="true" />
      <span>{label}</span>
    </div>
  )
}
