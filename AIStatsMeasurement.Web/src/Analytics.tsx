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
  label: string
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
  const [selectedNsis, setSelectedNsis] = useState<string[]>([])
  const [selectedLlms, setSelectedLlms] = useState<string[]>([])
  const [selectedThemes, setSelectedThemes] = useState<string[]>([])
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
        nsis: selectedNsis,
        llms: selectedLlms,
        themes: selectedThemes
      }

      const endpoint =
        groupBy === 'nsi'
          ? '/api/metrics/analytics/metrics-per-nsi'
          : groupBy === 'model'
            ? '/api/metrics/analytics/metrics-per-model'
            : '/api/metrics/analytics/metrics-per-theme'


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

      const weeklyEndpoint = `/api/metrics/analytics/weekly/${groupBy}`

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

      <div
  style={{
    display: 'flex',
    gap: '12px',
    padding: '12px',
    background: '#ffffff',
    border: '1px solid #e2e8f0',
    borderRadius: '16px',
    boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)',
    marginBottom: '20px',
    width: 'fit-content'
  }}
>
  {[
    { value: 'nsi', label: 'NSI' },
    { value: 'model', label: 'Model' },
    { value: 'theme', label: 'Theme' }
  ].map((option) => {
    const active = groupBy === option.value

    return (
      <label
        key={option.value}
        style={{
          cursor: 'pointer',
          padding: '10px 18px',
          borderRadius: '12px',
          fontWeight: 600,
          transition: 'all 0.2s ease',
          background: active ? '#22365a' : '#f8fafc',
          color: active ? 'white' : '#0f172a',
          border: active
            ? '1px solid #22365a'
            : '1px solid #e2e8f0',
          boxShadow: active
            ? '0 4px 10px rgba(16, 30, 62, 0.25)'
            : 'none'
        }}
      >
        <input
          type="radio"
          checked={active}
          onChange={() => setGroupBy(option.value as 'nsi' | 'model' | 'theme')}
          style={{ display: 'none' }}
        />

        {option.label}
      </label>
    )
  })}
</div>

      <Filters
        selectedNsis={selectedNsis}
        selectedLlms={selectedLlms}
        selectedThemes={selectedThemes}
        onNsisChange={setSelectedNsis}
        onLlmsChange={setSelectedLlms}
        onThemesChange={setSelectedThemes}
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
            key={item.label}
            item={item}
            groupBy={groupBy}
            chartData={chartData[item.label]}
          />
        ))}
        </div>
      )}
    </div>
  )
}

export default Analytics