import { type FormEvent, useState, useEffect } from 'react'
import './RunSinglePrompt.css'
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
  const [isExportOpen, setIsExportOpen] = useState(false)

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

  const downloadFile = (
    content: string,
    fileName: string,
    contentType: string
  ) => {
    const blob = new Blob([content], { type: contentType })
    const url = URL.createObjectURL(blob)

    const link = document.createElement('a')
    link.href = url
    link.download = fileName
    link.click()

    URL.revokeObjectURL(url)
  }

  const exportToJson = (results: ResultWithSources[]) => {
    const json = JSON.stringify(results, null, 2)
    downloadFile(json, 'llm-results.json', 'application/json')
  }

  const exportToCsv = (results: ResultWithSources[]) => {
    if (results.length === 0) return

    const rows = results.map((r) => ({
      id: r.id,
      theme: r.theme,
      question: r.question,
      expectedAnswer: r.expectedAnswer,
      expectedSource: r.expectedSource,
      actualAnswer: r.actualAnswer,
      actualSources: r.actualSourceDetails
        .map((source) => source.name ?? source.url ?? `Source ${source.id}`)
        .join(', '),
      provider: r.provider,
      rawText: r.rawText ?? '',
      exception: r.exception ?? '',
      squareMeanRootError: r.squareMeanRootError,
      relativeError: r.relativeError,
      answerIsCorrect: r.answerIsCorrect,
      sourceIsCorrect: r.sourceIsCorrect,
      createdUtc: r.createdUtc
    }))

    const headers = Object.keys(rows[0])

    const escape = (value: unknown) => {
      if (value == null) return '""'
      return `"${String(value).replace(/"/g, '""')}"`
    }

    const csv = [
      headers.join(';'),
      ...rows.map((row) =>
        headers
          .map((header) => escape(row[header as keyof typeof row]))
          .join(';')
      )
    ].join('\n')

    downloadFile(csv, 'llm-results.csv', 'text/csv;charset=utf-8;')
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

      <div
        style={{
          position: 'relative',
          display: 'flex',
          justifyContent: 'flex-end',
          marginBottom: '16px'
        }}
      >
        <button
          type="button"
          className="run-button"
          onClick={() => setIsExportOpen((prev) => !prev)}
          style={{ minWidth: '140px' }}
        >
          Export
        </button>

        {isExportOpen && (
          <div
            style={{
              position: 'absolute',
              top: '110%',
              right: 0,
              background: '#ffffff',
              border: '1px solid #e2e8f0',
              borderRadius: '12px',
              boxShadow: '0 10px 24px rgba(15, 23, 42, 0.12)',
              overflow: 'hidden',
              zIndex: 20,
              minWidth: '160px'
            }}
          >
            <button
              type="button"
              onClick={() => {
                exportToCsv(results)
                setIsExportOpen(false)
              }}
              style={{
                width: '100%',
                padding: '12px 16px',
                border: 'none',
                background: '#fff',
                textAlign: 'left',
                cursor: 'pointer'
              }}
            >
              Export as CSV
            </button>

            <button
              type="button"
              onClick={() => {
                exportToJson(results)
                setIsExportOpen(false)
              }}
              style={{
                width: '100%',
                padding: '12px 16px',
                border: 'none',
                background: '#fff',
                textAlign: 'left',
                cursor: 'pointer',
                borderTop: '1px solid #e2e8f0'
              }}
            >
              Export as JSON
            </button>
          </div>
        )}
      </div>

      <div className="table-wrapper">
        <table className="results-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Provider</th>
              <th>Theme</th>
              <th>Question</th>
              <th>Expected Answer</th>
              <th>Actual Answer</th>
              <th>Expected Source</th>
              <th>Actual Sources</th>
              <th>Answer Correct</th>
              <th>Source Correct</th>
              <th>Relative Error</th>
              <th>Created</th>
            </tr>
          </thead>

          <tbody>
            {results.map((result, index) => (
              <tr key={`${result.provider}-${result.createdUtc}-${index}`}>
                <td>{index + 1}</td>
                <td>{result.provider}</td>
                <td>{result.theme}</td>
                <td>{result.question}</td>
                <td>{result.expectedAnswer}</td>
                <td>{result.actualAnswer}</td>
                <td>{result.expectedSource}</td>

                <td>
                  {result.actualSourceDetails.length > 0 ? (
                    result.actualSourceDetails.map((source) => (
                      <div key={source.id}>
                        {source.url ? (
                          <a href={source.url} target="_blank" rel="noreferrer">
                            {source.name ?? source.url}
                          </a>
                        ) : (
                          source.name ?? `Source ${source.id}`
                        )}
                      </div>
                    ))
                  ) : (
                    <span>-</span>
                  )}
                </td>

                <td>{result.answerIsCorrect ? 'Yes' : 'No'}</td>
                <td>{result.sourceIsCorrect ? 'Yes' : 'No'}</td>
                <td>{result.relativeError}</td>
                <td>{new Date(result.createdUtc).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default ModelResponsesPage