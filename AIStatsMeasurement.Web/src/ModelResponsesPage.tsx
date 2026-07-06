import { type FormEvent, useState, useEffect } from 'react'
import './RunSinglePrompt.css'
import { API_BASE_URL } from '../config'
import { apiFetch } from './apiFetch'
import { Filters } from './components/Filters'

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

  const [selectedNsis, setSelectedNsis] = useState<string[]>([])
  const [selectedThemes, setSelectedThemes] = useState<string[]>([])
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  useEffect(() => {
    apiFetch(`${API_BASE_URL}/api/prompts`)
      .then((res) => res.json())
      .then((data) => setPrompts(data))
      .catch(() => setError('Failed loading prompts'))
  }, [])

  const handlePromptSelect = (id: number | null) => {
    setSelectedPromptId(id)

    if (id === null) {
      setSubmittedQuestion('')
      setResults([])
      setError('')
      return
    }

    const prompt = prompts.find((p) => p.id === id)
    setSubmittedQuestion(prompt?.question ?? '')
    setResults([])
    setError('')
  }

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (isLoading) return

    setIsLoading(true)
    setError('')
    setResults([])
    setIsExportOpen(false)

    try {
      const filterBody = {
        promptId: selectedPromptId,
        nsis: selectedNsis,
        themes: selectedThemes,
        startDate: startDate || null,
        endDate: endDate || null
      }

      const response = await apiFetch(`${API_BASE_URL}/api/exportrows/filter`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(filterBody)
      })

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
      <h1>Model Responses</h1>

      <div className="prompt-select">
        <label>
          <strong>Select a Prompt</strong>
        </label>

        <select
          value={selectedPromptId ?? ''}
          onChange={(e) => {
            const value = e.target.value
            handlePromptSelect(value ? Number(value) : null)
          }}
          className="select-input"
        >
          <option value="">-- All prompts --</option>

          {prompts.map((p) => (
            <option key={p.id} value={p.id}>
              {p.theme} — {p.subject}
            </option>
          ))}
        </select>
      </div>

      <div
        style={{
          display: 'flex',
          gap: '30px',
          alignItems: 'stretch',
          marginBottom: '26px',
          flexWrap: 'wrap'
        }}
      >
        <div
          style={{
            padding: '20px',
            background: '#ffffff',
            border: '1px solid #e2e8f0',
            borderRadius: '16px',
            boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)',
            minHeight: '158px',
            width: 'fit-content'
          }}
        >
          <h3
            style={{
              margin: '0 0 14px 0',
              fontSize: '18px',
              fontWeight: 700,
              color: '#0f172a'
            }}
          >
            Select period
          </h3>

          <div
            style={{
              display: 'flex',
              gap: '20px',
              alignItems: 'end'
            }}
          >
            <div
              style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}
            >
              <label style={{ fontWeight: 600, color: '#0f172a' }}>
                Start date
              </label>

              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                style={{
                  padding: '12px 14px',
                  borderRadius: '10px',
                  border: '1px solid #e2e8f0',
                  fontSize: '14px'
                }}
              />
            </div>

            <div
              style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}
            >
              <label style={{ fontWeight: 600, color: '#0f172a' }}>
                End date
              </label>

              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                style={{
                  padding: '12px 14px',
                  borderRadius: '10px',
                  border: '1px solid #e2e8f0',
                  fontSize: '14px'
                }}
              />
            </div>
          </div>
        </div>
      </div>

      <Filters
        selectedNsis={selectedNsis}
        selectedLlms={[]}
        selectedThemes={selectedThemes}
        onNsisChange={setSelectedNsis}
        onLlmsChange={() => {}}
        onThemesChange={setSelectedThemes}
      />

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
        <button type="submit" disabled={isLoading} className="run-button">
          {isLoading ? 'Loading responses...' : 'Show model responses'}
        </button>
      </form>

      {results.length > 0 && (
        <>
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
                      {result.actualSourceDetails.length > 0
                        ? result.actualSourceDetails
                            .map(
                              (source) =>
                                source.name ?? source.url ?? `Source ${source.id}`
                            )
                            .join(', ')
                        : '-'}
                    </td>
                    <td>{result.answerIsCorrect ? 'Yes' : 'No'}</td>
                    <td>{result.sourceIsCorrect ? 'Yes' : 'No'}</td>
                    <td>
                      {result.relativeError != null
                        ? result.relativeError.toFixed(2)
                        : '-'}
                    </td>
                    <td>{new Date(result.createdUtc).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}

export default ModelResponsesPage
