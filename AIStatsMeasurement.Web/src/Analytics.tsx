import { useState } from 'react'
import { ResultCard } from './components/ResultCard'
import { Filters } from './components/Filters'
import { type ChartPoint } from './components/MetricsLineChart'
import './Analytics.css'
import { API_BASE_URL } from '../config'

const pageTheme = {
  background: '#f8fafc',
  cardBackground: '#ffffff',
  text: '#0f172a',
  mutedText: '#475569',
  border: '#e2e8f0',
  primary: '#0f172a',
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

  accuracyScoreTooltip: string
  findabilityScoreTooltip: string
  consistencyScoreTooltip: string
}

export type MetricsOverTime = {
  accuracy: ChartPoint[]
  consistency: ChartPoint[]
  findability: ChartPoint[]
}

type MetricsPerNsi = Record<string, MetricsOverTime>

function Analytics() {
  const [selectedNsi, setSelectedNsi] = useState('')
  const [selectedLlm, setSelectedLlm] = useState('')
  const [selectedTheme, setSelectedTheme] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')
  const [results, setResults] = useState<AnalyticsResponse[]>([])
  const [chartData, setChartData] = useState<MetricsPerNsi>({})
  const [groupBy, setGroupBy] = useState<'nsi' | 'model' | 'theme'>('nsi')

  const handleSend = async () => {
    setIsLoading(true)
    setError('')
    setResults([])
    setChartData({})

    try {
      const filterBody = {
        nsi: selectedNsi,
        llm: selectedLlm,
        theme: selectedTheme
      }

      const endpoint =
        groupBy === 'nsi'
          ? '/api/analytics/metrics-per-nsi'
          : groupBy === 'model'
            ? '/api/analytics/metrics-per-model'
            : '/api/analytics/metrics-per-theme'


      const metricsResponse = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(filterBody)
      })

      if (!metricsResponse.ok) {
        const errorText = await metricsResponse.text()
        throw new Error(errorText || 'Failed to load analytics')
      }

      const metricsData: AnalyticsResponse[] = await metricsResponse.json()

      const weeklyEndpoint = `/api/analytics/weekly/${groupBy}`

      const weeklyResponse = await fetch(`${API_BASE_URL}${weeklyEndpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(filterBody)
      })

      if (!weeklyResponse.ok) {
        const errorText = await weeklyResponse.text()
        throw new Error(errorText || 'Failed to load weekly analytics')
      }

      const weeklyData: MetricsPerNsi = await weeklyResponse.json()
      setChartData(weeklyData)

      setResults(metricsData)
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

      <div>
  <label>
    <input
      type="radio"
      checked={groupBy === 'nsi'}
      onChange={() => setGroupBy('nsi')}
    />
    NSI
  </label>

  <label>
    <input
      type="radio"
      checked={groupBy === 'model'}
      onChange={() => setGroupBy('model')}
    />
    Model
  </label>

  <label>
    <input
      type="radio"
      checked={groupBy === 'theme'}
      onChange={() => setGroupBy('theme')}
    />
    Theme
  </label>
</div>

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
          <ResultCard
            key={item.nsi}
            item={item}
            groupBy={groupBy}
            chartData={chartData[item.nsi]}
          />
        ))}
        </div>
      )}
    </div>
  )
}

export default Analytics