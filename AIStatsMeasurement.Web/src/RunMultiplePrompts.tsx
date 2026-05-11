import { type FormEvent, useState, useEffect } from 'react'
import './RunSinglePrompt.css'
import ExportRowCard from './components/ExportRowCard'
import { API_BASE_URL } from '../config'
import { apiFetch } from './apiFetch'

const nsiOptions = ['CBS', 'OECD', 'StatBank Denmark']

const modelOptions = [
  'gpt-4o-mini',
  'gemini-2.5-flash-lite',
  'grok-4-1-fast-non-reasoning',
  'gpt-5.4',
  //'gemini-3.1-pro-preview',
  'grok-4.20-reasoning',
  'gemini-2.5-pro'
]

const downloadFile = (content: string, fileName: string, contentType: string) => {
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
  const rows = results.map((r) => ({
    id: r.id,
    theme: r.theme,
    question: r.question,
    expectedAnswer: r.expectedAnswer,
    expectedSource: r.expectedSource,
    actualAnswer: r.actualAnswer,
    provider: r.provider,
    rawText: r.rawText ?? ''
  }))

  const headers = Object.keys(rows[0])

  const escape = (value: unknown) => {
    if (value == null) return '""'
    return `"${String(value).replace(/"/g, '""')}"`
  }

  const csv = [
    headers.join(';'),
    ...rows.map(row =>
      headers.map(h => escape(row[h as keyof typeof row])).join(';')
    )
  ].join('\n')

  downloadFile(csv, 'results.csv', 'text/csv;charset=utf-8;')
}

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

const fetchSourcesByIds = async (ids: number[]): Promise<SourceDto[]> => {
  if (!ids.length) return []

  const response = await apiFetch(`/api/sources/getByIds`, {
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

  const [selectedModels, setSelectedModels] = useState<string[]>([
    'gpt-4o-mini',
    'gemini-2.5-flash-lite',
    'grok-4-1-fast-non-reasoning'
  ])

  const [isExportOpen, setIsExportOpen] = useState(false)

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

  const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms))

const chunkArray = <T,>(items: T[], size: number): T[][] => {
  const chunks: T[][] = []

  for (let i = 0; i < items.length; i += size) {
    chunks.push(items.slice(i, i + size))
  }

  return chunks
}

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

  const batches = chunkArray(selectedPromptIds, 25)

  try {
    for (let i = 0; i < batches.length; i++) {
      const batch = batches[i]

      const response = await apiFetch(`${API_BASE_URL}/api/llm/run`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          promptIds: batch,
          modelNames: selectedModels
        })
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

      setResults((prev) => [...prev, ...enrichedResults])

      if (i < batches.length - 1) {
        await sleep(60_000)
      }
    }
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
      {isLoading ? 'Running prompts in batches...' : 'Run All Matching Prompts'}
    </button>
  </form>

      <div> 
          <div
  style={{
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

      </div>

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
