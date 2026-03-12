import { type FormEvent, useState, useEffect } from 'react'
import './App.css'

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
  actualSource: string[]
  provider: string
  rawText: string | null
  exception: string | null
  squareMeanRootError: number
  relativeError: number
  answerIsCorrect: boolean
  sourceIsCorrect: boolean
  averageRelativeError: number
  averageAnswer: number
  averageAnswerCorrectness: number
  averageSourceCorrectness: number
  createdUtc: string
}

function App() {
  const [prompts, setPrompts] = useState<Prompt[]>([])
  const [selectedPromptId, setSelectedPromptId] = useState<number | null>(null)
  const [submittedQuestion, setSubmittedQuestion] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [results, setResults] = useState<ExportRow[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    fetch('http://localhost:5201/api/prompts')
      .then(res => res.json())
      .then(data => setPrompts(data))
      .catch(() => console.log('Failed loading prompts'))
  }, [])

  const handlePromptSelect = (id: number) => {
    setSelectedPromptId(id)

    const prompt = prompts.find(p => p.id === id)
    if (prompt) {
      setSubmittedQuestion(prompt.question)
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
          'Content-Type': 'application/json',
        },
        body: JSON.stringify([selectedPromptId]),
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText || 'Request failed')
      }

      const data: ExportRow[] = await response.json()
      setResults(data)
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message)
      } else {
        setError('Er ging iets mis.')
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
              const color =
                result.provider.includes('gpt')
                  ? '#2563eb'
                  : result.provider.includes('gemini')
                    ? '#16a34a'
                    : '#9333ea'

              const background =
                result.provider.includes('gpt')
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
                    <strong>Actual answer:</strong> {result.actualAnswer}
                  </p>

                  <p>
                    <strong>Expected answer:</strong> {result.expectedAnswer}
                  </p>

                  <div>
                    <strong>Actual source:</strong>
                    {result.actualSource && result.actualSource.length > 0 ? (
                      <ul className="source-list">
                        {result.actualSource.map((src, i) => (
                          <li key={i}>
                            <a href={src} target="_blank" rel="noopener noreferrer">
                              {src}
                            </a>
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p>no reference found</p>
                    )}
                  </div>

                  <p>
                    <strong>Expected source:</strong>{' '}
                    <a
                      href={result.expectedSource}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      {result.expectedSource}
                    </a>
                  </p>

                  <p>
                    <strong>Relative error:</strong> {(result.relativeError * 100).toFixed(1) + "%"}
                  </p>

                  <p>
                    <strong>Answer correct:</strong> {result.answerIsCorrect ? "yes" : "no"}
                  </p>

                  <p>
                    <strong>Source correct:</strong> {result.sourceIsCorrect ? "yes" : "no"}
                  </p>

                  <p>
                    <strong>Average answer:</strong> {result.averageAnswer}
                  </p>

                  <p>
                    <strong>Average relative error:</strong> {result.averageRelativeError}
                  </p>

                  <p>
                    <strong>Average answer correctness:</strong> {(result.averageAnswerCorrectness * 100).toFixed(1) + "%"}
                  </p>

                  <p>
                    <strong>Average source correctness:</strong> {(result.averageSourceCorrectness * 100).toFixed(1) + "%"}
                  </p>

                  <div className="raw-text-block">
                    <strong>Raw text:</strong>
                    <p className="raw-text">{result.rawText}</p>
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

export default App
