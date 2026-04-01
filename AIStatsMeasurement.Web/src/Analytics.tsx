import { useState } from 'react'
import { ResultCard } from './components/ResultCard'
import { Filters } from './components/Filters'
import './Analytics.css'

const pageTheme = {
  background: '#f8fafc',
  cardBackground: '#ffffff',
  text: '#0f172a',
  mutedText: '#475569',
  border: '#e2e8f0',
  primary: '#2563eb',
  danger: '#dc2626',
  dangerBackground: '#fef2f2'
}

export type SourceItem = {
  hostname: string
  count: number
}

export type AnalyticsResponse = {
  nsi: string
  accuracyScore: number
  findabilityScore: number
  consistencyScore: number
  totalMeasurements: number
  topSources: SourceItem[]
}

function Analytics() {
  const [selectedNsi, setSelectedNsi] = useState('')
  const [selectedLlm, setSelectedLlm] = useState('')
  const [selectedTheme, setSelectedTheme] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [results, setResults] = useState<AnalyticsResponse[]>([])

  const handleSend = async () => {
    setIsLoading(true)
    setError('')

    try {
      const response = await fetch('http://localhost:5201/api/metrics', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          nsi: selectedNsi,
          llm: selectedLlm,
          theme: selectedTheme
        })
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText || 'Failed to load analytics')
      }

      const data: AnalyticsResponse[] = await response.json()
      setResults(data)
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message)
      } else {
        setError('Something went wrong')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="page-content">
        <h1 style={{ marginBottom: '24px' }}>Analytics</h1>

        <Filters
          selectedNsi={selectedNsi}
          selectedLlm={selectedLlm}
          selectedTheme={selectedTheme}
          onNsiChange={setSelectedNsi}
          onLlmChange={setSelectedLlm}
          onThemeChange={setSelectedTheme}
        />

        <div style={{ marginBottom: '24px' }}>
          <button
            onClick={handleSend}
            disabled={isLoading}
            style={{
              padding: '12px 20px',
              borderRadius: '10px',
              border: 'none',
              background: pageTheme.primary,
              color: '#ffffff',
              fontSize: '14px',
              fontWeight: 700,
              cursor: isLoading ? 'not-allowed' : 'pointer',
              opacity: isLoading ? 0.7 : 1
            }}
          >
            {isLoading ? 'Loading...' : 'Send'}
          </button>
        </div>

        {error && (
          <div
            style={{
              marginBottom: '24px',
              padding: '14px 16px',
              borderRadius: '12px',
              border: `1px solid ${pageTheme.danger}`,
              background: pageTheme.dangerBackground,
              color: pageTheme.danger
            }}
          >
            {error}
          </div>
        )}

        {!isLoading && !error && results.length === 0 && (
          <div
            style={{
              background: pageTheme.cardBackground,
              border: `1px solid ${pageTheme.border}`,
              borderRadius: '16px',
              padding: '24px',
              color: pageTheme.mutedText
            }}
          >
            No analytics loaded yet. Choose filters and press Send.
          </div>
        )}

        {results.length > 0 && (
          
            <div className="results-grid">
              {results.map((item) => (
                <ResultCard key={item.nsi} item={item} />
              ))}
            </div>  
        )}
      </div>
  )
}

export default Analytics