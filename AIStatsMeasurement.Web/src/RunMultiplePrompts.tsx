import { type FormEvent, useState, useEffect } from 'react'
import './RunSinglePrompt.css'
import ExportRowCard from './components/ExportRowCard'
import { API_BASE_URL } from '../config'
import { apiFetch } from './apiFetch'
import { runLlmJob, type ResultWithSources } from './pages/llmJobRunner'

const nsiOptions = ['CBS', 'OECD', 'StatBank Denmark']

const modelOptions = [
  'gpt-4o-mini',
  'gemini-2.5-flash-lite',
  'grok-4-1-fast-non-reasoning',
  'grok-4.20-0309-reasoning',
  'gpt-5.4',
  //'gemini-3.1-pro-preview',
  'grok-4.20-reasoning',
  'grok-4.3',
  'gemini-2.5-pro'
]

type Prompt = {
  id: number
  theme: string
  subject: string
  question: string
}
const cardStyle = {
  background: '#ffffff',
  border: '1px solid #e2e8f0',
  borderRadius: '16px',
  padding: '16px',
  boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
}

const selectStyle = {
  width: '100%',
  padding: '12px',
  borderRadius: '10px',
  border: '1px solid #e2e8f0',
  fontSize: '14px'
}

function RunMultiplePrompts() {
  const [isLoading, setIsLoading] = useState(false)
  const [results, setResults] = useState<ResultWithSources[]>([])
  const [error, setError] = useState('')

  const [themes, setThemes] = useState<string[]>([])
  const [selectedNsi, setSelectedNsi] = useState('')
  const [selectedTheme, setSelectedTheme] = useState('')

  const [prompts, setPrompts] = useState<Prompt[]>([])
  const [filteredPrompts, setFilteredPrompts] = useState<Prompt[]>([])
  const [selectedPromptIds, setSelectedPromptIds] = useState<number[]>([])

  const [jobStatus, setJobStatus] = useState('')

  const [selectedModels, setSelectedModels] = useState<string[]>([
    'gpt-4o-mini',
    'gemini-2.5-flash-lite',
    'grok-4.3'
  ])

  function handleModelToggle(modelName: string) {
    setSelectedModels((prev) =>
      prev.includes(modelName)
        ? prev.filter((m) => m !== modelName)
        : [...prev, modelName]
    )
  }

  useEffect(() => {
    apiFetch(`${API_BASE_URL}/api/prompts`)
      .then((res) => res.json())
      .then((data: Prompt[]) => {
        setPrompts(data)
        setFilteredPrompts(data)
        setSelectedPromptIds(data.map((p) => p.id))
      })
      .catch(() => console.log('Failed loading prompts'))
  }, [])

  useEffect(() => {
    apiFetch(`${API_BASE_URL}/api/prompts/themes`)
      .then((res) => res.json())
      .then((data: string[]) => setThemes(data))
      .catch(() => console.log('Failed loading themes'))
  }, [])

  useEffect(() => {
    if (!selectedNsi) {
      let filtered = prompts

      if (selectedTheme) {
        filtered = filtered.filter((p) => p.theme === selectedTheme)
      }

      setFilteredPrompts(filtered)
      setSelectedPromptIds(filtered.map((p) => p.id))
      return
    }

    apiFetch(`${API_BASE_URL}/api/prompts/nsi?nsi=${selectedNsi}`)
      .then((res) => res.json())
      .then((ids: number[]) => {
        let filtered = prompts.filter((p) => ids.includes(p.id))

        if (selectedTheme) {
          filtered = filtered.filter((p) => p.theme === selectedTheme)
        }

        setFilteredPrompts(filtered)
        setSelectedPromptIds(filtered.map((p) => p.id))
      })
      .catch(() => console.log('Failed loading prompts by NSI'))
  }, [selectedNsi, selectedTheme, prompts])

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    if (!selectedPromptIds.length || isLoading) return

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
        selectedPromptIds,
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
      <h1>Run Multiple Prompts</h1>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))',
          gap: '16px',
          marginBottom: '20px'
        }}
      >
        <div style={cardStyle}>
          <label style={{ display: 'block', fontWeight: 700, marginBottom: '8px' }}>
            Select NSI
          </label>

          <select
            value={selectedNsi}
            onChange={(e) => setSelectedNsi(e.target.value)}
            style={selectStyle}
          >
            <option value="">All NSI's</option>
            {nsiOptions.map((nsi) => (
              <option key={nsi} value={nsi}>
                {nsi}
              </option>
            ))}
          </select>
        </div>

        <div style={cardStyle}>
          <label style={{ display: 'block', fontWeight: 700, marginBottom: '8px' }}>
            Select Theme
          </label>

          <select
            value={selectedTheme}
            onChange={(e) => setSelectedTheme(e.target.value)}
            style={selectStyle}
          >
            <option value="">All themes</option>
            {themes.map((theme) => (
              <option key={theme} value={theme}>
                {theme}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div style={{ ...cardStyle, marginBottom: '20px' }}>
        <label style={{ display: 'block', fontWeight: 700, marginBottom: '12px' }}>
          Select models
        </label>

        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
            gap: '10px'
          }}
        >
          {modelOptions.map((model) => (
            <label
              key={model}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                background: '#fff',
                padding: '10px 14px',
                borderRadius: '12px',
                border: '1px solid #e2e8f0',
                cursor: 'pointer'
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

      <div className="question-preview">
        <strong>Selected prompts:</strong> {selectedPromptIds.length}
      </div>

      <div className="question-preview">
        <strong>Matching prompt subjects:</strong>
        <ul style={{ marginTop: '8px' }}>
          {filteredPrompts.map((prompt) => (
            <li key={prompt.id}>
              {prompt.subject}
            </li>
          ))}
        </ul>
      </div>

      <form onSubmit={handleSubmit}>
        <button
          type="submit"
          disabled={!selectedPromptIds.length || !selectedModels.length || isLoading}
          className="run-button"
        >
          {isLoading ? 'Running job...' : 'Run All Matching Prompts'}
        </button>
      </form>

      {jobStatus && (
        <div className="question-preview">
          <strong>{jobStatus}</strong>
        </div>
      )}

      {error && <div className="error-message">{error}</div>}

      {results.length > 0 && (
        <div className="results-section">
          <h2>Responses</h2>

          <div className="results-grid">
            {results.map((result, index) => (
              <ExportRowCard
                key={`${result.provider}-${index}-${result.id}`}
                result={result}
                index={index}
              />
            ))}
          </div>
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: '16px'
            }}
          >
          </div>
        </div>
      )}
    </div>
  )
}

export default RunMultiplePrompts
