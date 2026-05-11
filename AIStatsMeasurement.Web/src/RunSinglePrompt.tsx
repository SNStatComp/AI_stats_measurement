import { type FormEvent, useState, useEffect } from 'react'
import './RunSinglePrompt.css'
import ExportRowCard from './components/ExportRowCard'
import { API_BASE_URL } from '../config'
import { apiFetch } from './apiFetch'
import { runLlmJob, type ResultWithSources } from './pages/llmJobRunner'

type Prompt = {
  id: number
  theme: string
  subject: string
  question: string
}

const modelOptions = [
  'gpt-4o-mini',
  'gemini-2.5-flash-lite',
  'grok-4-1-fast-non-reasoning',
  'gpt-5.4',
  //'gemini-3.1-pro-preview',
  'gemini-2.5-pro',
  'grok-4.20-reasoning'
]

function RunSinglePrompt() {
  const [prompts, setPrompts] = useState<Prompt[]>([])
  const [selectedPromptId, setSelectedPromptId] = useState<number | null>(null)
  const [selectedModels, setSelectedModels] = useState<string[]>([
    'gpt-4o-mini',
    'gemini-2.5-flash-lite',
    'grok-4-1-fast-non-reasoning'
  ])
  const [submittedQuestion, setSubmittedQuestion] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [results, setResults] = useState<ResultWithSources[]>([])
  const [error, setError] = useState('')
  const [jobStatus, setJobStatus] = useState('')

  useEffect(() => {
    apiFetch(`${API_BASE_URL}/api/prompts`)
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

  function handleModelToggle(modelName: string) {
    setSelectedModels((prev) =>
      prev.includes(modelName)
        ? prev.filter((m) => m !== modelName)
        : [...prev, modelName]
    )
  }

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (!selectedPromptId || isLoading) return

    if (!selectedModels.length) {
      alert('Select at least one model')
      return
    }

    setIsLoading(true)
    setError('')
    setResults([])
    setJobStatus('')

    try {
      const enrichedResults = await runLlmJob(
        [selectedPromptId],
        selectedModels,
        setJobStatus
      )

      setResults(enrichedResults)
      setJobStatus('Completed')
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
      <h1>Run Single Prompt</h1>

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

      <div className="prompt-select">
        <label>
          <strong>Select models</strong>
        </label>

        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
            gap: '10px',
            marginTop: '12px'
          }}
        >
          {modelOptions.map((model) => (
            <label
              key={model}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                background: 'white',
                padding: '10px 14px',
                borderRadius: '12px',
                border: '1px solid #e2e8f0'
              }}
            >
              <input
                type="checkbox"
                checked={selectedModels.includes(model)}
                onChange={() => handleModelToggle(model)}
              />
              <span>{model}</span>
            </label>
          ))}
        </div>
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
          disabled={!selectedPromptId || isLoading || selectedModels.length === 0}
          className="run-button"
        >
          {isLoading ? 'Running models...' : 'Compare AI Responses'}
        </button>
      </form>

      {jobStatus && (
        <div className="question-preview">
          <strong>{jobStatus}</strong>
        </div>
      )}


      {results.length > 0 && (
        <div className="results-section">
          <h2>Responses</h2>

          <div className="results-grid">
            {results.map((result, index) => (
              <ExportRowCard
                key={`${result.provider}-${index}`}
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

export default RunSinglePrompt
