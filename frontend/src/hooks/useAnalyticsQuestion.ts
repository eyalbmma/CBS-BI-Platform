import { useCallback, useEffect, useRef, useState } from 'react'
import { askQuestion } from '../api/analytics/analyticsApi'
import { ApiError, isAbortError } from '../api/http/apiError'
import type { AnalyticsQuestionResponse } from '../models/analytics/analyticsModels'

interface UseAnalyticsQuestionState {
  result: AnalyticsQuestionResponse | null
  error: unknown | null
  isLoading: boolean
  askAnalyticsQuestion: (question: string) => Promise<AnalyticsQuestionResponse | null>
  reset: () => void
}

export function useAnalyticsQuestion(): UseAnalyticsQuestionState {
  const [result, setResult] = useState<AnalyticsQuestionResponse | null>(null)
  const [error, setError] = useState<unknown | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const controllerRef = useRef<AbortController | null>(null)
  const mountedRef = useRef(true)

  useEffect(() => {
    return () => {
      mountedRef.current = false
      controllerRef.current?.abort()
    }
  }, [])

  const reset = useCallback(() => {
    setResult(null)
    setError(null)
    setIsLoading(false)
  }, [])

  const askAnalyticsQuestion = useCallback(async (question: string): Promise<AnalyticsQuestionResponse | null> => {
    const trimmedQuestion = question.trim()

    if (!trimmedQuestion) {
      setError(new Error('Please enter a question.'))
      return null
    }

    if (isLoading) {
      return null
    }

    controllerRef.current?.abort()

    const controller = new AbortController()
    controllerRef.current = controller

    setIsLoading(true)
    setError(null)
    setResult(null)

    try {
      const response = await askQuestion(trimmedQuestion, controller.signal)

      if (!mountedRef.current) {
        return null
      }

      setResult(response)
      return response
    } catch (caughtError) {
      if (!mountedRef.current || isAbortError(caughtError)) {
        return null
      }

      setError(caughtError instanceof ApiError ? caughtError : caughtError ?? new Error('An unexpected error occurred.'))
      return null
    } finally {
      if (mountedRef.current && controllerRef.current === controller) {
        setIsLoading(false)
      }
    }
  }, [isLoading])

  return {
    result,
    error,
    isLoading,
    askAnalyticsQuestion,
    reset,
  }
}
