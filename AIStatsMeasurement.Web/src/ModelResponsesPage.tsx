import { type FormEvent, useState, useEffect } from 'react'
import './RunSinglePrompt.css'
import ExportRowCard from './components/ExportRowCard'
import { API_BASE_URL } from '../config'
import { apiFetch } from './apiFetch'

type Prompt = {
  id: number
  theme: string
  subject: string
  question: string
}

type ExportRow = {
  id: number
  theme: string
  question: string
  expectedAnswer: number
  expectedSource: string
  actualAnswer: number
  actualSource: number[]
  provider: string
  rawText: string | null
  exception: string | null
  squareMeanRootError: number
  relativeError: number
  answerIsCorrect: boolean
  sourceIsCorrect: boolean
  createdUtc: string
}

type SourceDto = {
  id: number
  name: string | null
  url: string | null
  type: string | null
}

type ResultWithSources = ExportRow & {
  actualSourceDetails: SourceDto[]
}

const fetchSourcesByIds = async (ids: number[]): Promise<SourceDto[]> => {
  if (!ids.length) return []

  const response = await apiFetch(`${API_BASE_URL}/api/sources/getByIds`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(ids)
  })

  if (!response.ok) {
    throw new Error('Failed to fetch sources')
  }

  return response.json()
}

function ModelResponsesPage() {
  const [prompts, setPrompts] = useState<Prompt[]>([])
  const [selectedPromptId, setSelectedPromptId] = useState<number | null>(null)
  const [submittedQuestion, setSubmittedQuestion] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [results, setResults] = useState<ResultWithSources[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    apiFetch(`${API_BASE_URL}/api/prompts`)
      .then((res) => res.json())
      .then((data) => setPrompts(data))
      .catch(() => setError('Failed loading prompts'))
  }, [])

  const handlePromptSelect = (id: number) => {
    setSelectedPromptId(id)

    const prompt = prompts.find((p) => p.id === id)

    if (prompt) {
      setSubmittedQuestion(prompt.question)
    } else {
      setSubmittedQuestion('')
    }

    setResults([])
    setError('')
  }

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (!selectedPromptId || isLoading) return

    setIsLoading(true)
    setError('')
    setResults([])

    try {
      const response = await apiFetch(
        `${API_BASE_URL}/api/exportrows/byPrompt/${selectedPromptId}`
      )

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText || 'Request failed')
      }

      const data: ExportRow[] = await response.json()

      const allSourceIds = [
        ...new Set(data.flatMap((r) => r.actualSource ?? []))
      ]

      const sourceDtos = await fetchSourcesByIds(allSourceIds)

      const sourceMap = new Map<number, SourceDto>(
        sourceDtos.map((source) => [source.id, source])
      )

      const enrichedResults: ResultWithSources[] = data.map((result) => ({
        ...result,
        actualSourceDetails: (result.actualSource ?? [])
          .map((id) => sourceMap.get(id))
          .filter((source): source is SourceDto => Boolean(source))
      }))

      setResults(enrichedResults)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="app-container">
      <h1>Model Responses By Prompt</h1>

      <div className="prompt-select">
        <label>
          <strong>Select a Prompt</strong>
        </label>

        <select
          value={selectedPromptId ?? ''}
          onChange={(e) => handlePromptSelect(Number(e.target.value))}
          className="select-input"
        >
          <option value="">-- Select prompt --</option>

          {prompts.map((p) => (
            <option key={p.id} value={p.id}>
              {p.theme} — {p.subject}
            </option>
          ))}
        </select>
      </div>

      {submittedQuestion && (
        <div className="question-preview">
          <strong>Question:</strong> {submittedQuestion}
        </div>
      )}

      {error && (
        <div className="error-message">
          <strong>Error:</strong> {error}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <button
          type="submit"
          disabled={!selectedPromptId || isLoading}
          className="run-button"
        >
          {isLoading ? 'Loading responses...' : 'Show model responses'}
        </button>
      </form>

      {results.length > 0 && (
        <div className="results-section">
          <h2>Model Responses</h2>

          <div className="results-grid">
            {results.map((result, index) => (
              <ExportRowCard
                key={`${result.provider}-${result.createdUtc}-${index}`}
                result={result}
                index={index}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

export default ModelResponsesPage