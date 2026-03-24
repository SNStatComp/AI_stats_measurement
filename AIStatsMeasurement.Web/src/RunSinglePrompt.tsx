import { type FormEvent, useState, useEffect } from 'react'

import './RunSinglePrompt.css'

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

  const response = await fetch('http://localhost:5201/api/sources/getByIds', {
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

function RunSinglePrompt() {
  const [prompts, setPrompts] = useState<Prompt[]>([])
  const [selectedPromptId, setSelectedPromptId] = useState<number | null>(null)
  const [submittedQuestion, setSubmittedQuestion] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [results, setResults] = useState<ResultWithSources[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    fetch('http://localhost:5201/api/prompts')
      .then((res) => res.json())
      .then((data) => setPrompts(data))
      .catch(() => console.log('Failed loading prompts'))
  }, [])

  const handlePromptSelect = (id: number) => {
    setSelectedPromptId(id)

    const prompt = prompts.find((p) => p.id === id)
    if (prompt) {
      setSubmittedQuestion(prompt.question)
    } else {
      setSubmittedQuestion('')
    }
  }

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (!selectedPromptId || isLoading) return

    setIsLoading(true)
    setError('')
    setResults([])

    try {
      const response = await fetch('http://localhost:5201/api/llm/run', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify([selectedPromptId])
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText || 'Request failed')
      }

      const data: ExportRow[] = await response.json()

      const allSourceIds = [...new Set(data.flatMap((r) => r.actualSource ?? []))]

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
      if (err instanceof Error) {
        setError(err.message)
      } else {
        setError('Something went wrong.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="app-container">
      <h1>LLM Statistics Monitoring Tool</h1>

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

      <form onSubmit={handleSubmit}>
        <button
          type="submit"
          disabled={!selectedPromptId || isLoading}
          className="run-button"
        >
          {isLoading ? 'Running models...' : 'Compare AI Responses'}
        </button>
      </form>

      {error && <div className="error-message">{error}</div>}

      {results.length > 0 && (
        <div className="results-section">
          <h2>Responses</h2>

          <div className="results-grid">
            {results.map((result, index) => {
              const color = result.provider.includes('gpt')
                ? '#2563eb'
                : result.provider.includes('gemini')
                  ? '#16a34a'
                  : '#9333ea'

              const background = result.provider.includes('gpt')
                ? '#eff6ff'
                : result.provider.includes('gemini')
                  ? '#f0fdf4'
                  : '#faf5ff'

              return (
                <div
                  key={`${result.provider}-${index}`}
                  className="result-card"
                  style={{
                    border: `2px solid ${color}`,
                    background
                  }}
                >
                  <h3 style={{ color }}>{result.provider}</h3>

                  <p>
                    <strong>Expected answer:</strong> {result.expectedAnswer}
                  </p>

                  <p>
                    <strong>Actual answer:</strong> {result.actualAnswer}
                  </p>               

                  <div className="sources-section">
                    <strong>Expected source:</strong>

                    <div className="source-card-grid expected-grid">
                      <div className="source-card">
                        <div className="source-card-header">
                          <span className="source-type-badge expected-badge">
                            NSI Database
                          </span>
                        </div>

                        <div className="source-card-body">
                          <p className="source-name">{result.expectedSource}</p>
                          <a
                            href={result.expectedSource}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="source-link"
                          >
                            Open expected source
                          </a>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="sources-section">
                    <strong>Actual sources:</strong>

                    {result.actualSourceDetails.length > 0 ? (
                      <div className="source-card-grid">
                        {result.actualSourceDetails.map((src) => (
                          <div key={src.id} className="source-card">
                            <div className="source-card-header">
                              <span className="source-type-badge">
                                {src.type || 'unknown'}
                              </span>
                            </div>

                            <div className="source-card-body">
                              <p className="source-name">
                                {src.name || `Source #${src.id}`}
                              </p>

                              {src.url ? (
                              <>
                                <p className="source-url">{src.url}</p>

                                <a
                                  href={src.url}
                                  target="_blank"
                                  rel="noopener noreferrer"
                                  className="source-link"
                                >
                                  Open source
                                </a>
                              </>
                            ) : (
                              <p className="source-no-link">No URL available</p>
                            )}
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <p className="no-source-text">No reference found</p>
                    )}
                  </div>

                  <p>
                    <strong>Relative error:</strong>{' '}
                    {(result.relativeError * 100).toFixed(1) + '%'}
                  </p>

                  <p>
                    <strong>Answer correct:</strong>{' '}
                    {result.answerIsCorrect ? 'yes' : 'no'}
                  </p>

                  <p>
                    <strong>Source correct:</strong>{' '}
                    {result.sourceIsCorrect ? 'yes' : 'no'}
                  </p>

                  <div className="raw-text-block">
                    <strong>Raw text:</strong>
                    <p className="raw-text">{result.rawText ?? 'No raw text available'}</p>
                  </div>

                  {result.exception && (
                    <p className="exception-text">
                      <strong>Exception:</strong> {result.exception}
                    </p>
                  )}
                </div>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}

export default RunSinglePrompt
