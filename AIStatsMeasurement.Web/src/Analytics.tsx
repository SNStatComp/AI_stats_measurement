import { useState } from 'react'
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  LineChart,
  Line,
  CartesianGrid,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  Radar
} from 'recharts'

const nsiOptions = ['CBS', 'OECD', 'STATBANK DENMARK']

const llmOptions = [
  'gemini-2.5-flash-lite-preview-09-2025',
  'gpt-4o-mini',
  'grok-4-1-fast-non-reasoning',
  'gemini-3.1-pro-preview',
  'gpt-5.4',
  'grok-4.20-reasoning'
]

const themeOptions = [
  'Arbeid en sociale zekerheid',
  'Bedrijven',
  'Bevolking',
  'Bouwen en wonen',
  'Caribisch Nederland',
  'Energie',
  'Financiële en zakelijke diensten',
  'Gezondheid en welzijn',
  'Handel en horeca',
  'Industrie',
  'Inkomen en bestedingen',
  'Internationale handel',
  'Landbouw',
  'Macro-economie',
  'Natuur en milieu',
  'Nederland regionaal',
  'Onderwijs',
  'Overheid',
  'Prijzen',
  'Veiligheid en recht',
  'Verkeer en vervoer',
  'Vrije tijd en cultuur',
  'Agriculture and fisheries',
  'Development',
  'Economy',
  'Education and skills',
  'Environment and climate change',
  'Finance and investment',
  'Public governance',
  'Health',
  'Industry, business and entrepreneurship',
  'Science, technology and innovation',
  'Employment',
  'Society',
  'Regional, rural and urban development',
  'Trade',
  'Transport',
  'Taxation'
]

const pageTheme = {
  background: '#f8fafc',
  cardBackground: '#ffffff',
  text: '#0f172a',
  mutedText: '#475569',
  border: '#e2e8f0',
  grid: '#cbd5e1',
  primary: '#2563eb',
  secondary: '#10b981',
  accent: '#f59e0b',
  radarFill: '#2563eb',
  radarStroke: '#1d4ed8',
  tooltipBackground: '#ffffff'
}

type SummaryDto = {
  accuracy: number
  findability: number
  consistency: number
  totalmeasurements: number
}

type BarItemDto = {
  name: string
  score: number
}

type TimelineItemDto = {
  run: string
  accuracy: number
  findability: number
}

type RadarItemDto = {
  metric: string
  value: number
}

type AnalyticsResponse = {
  accuracyScore: number
  findabilityScore: number
  consistencyScore: number
  totalMeasurements: number
  barData?: BarItemDto[]
  timelineData?: TimelineItemDto[]
  radarData?: RadarItemDto[]
}

function ScoreCard({ title, value }: { title: string; value: number }) {
  return (
    <div
      style={{
        padding: '20px',
        borderRadius: '16px',
        background: pageTheme.cardBackground,
        boxShadow: '0 4px 14px rgba(15, 23, 42, 0.08)',
        width: '180px',
        border: `1px solid ${pageTheme.border}`
      }}
    >
      <div
        style={{
          fontSize: '14px',
          color: pageTheme.mutedText,
          marginBottom: '10px'
        }}
      >
        {title}
      </div>

      <div
        style={{
          fontSize: '42px',
          fontWeight: 700,
          color: pageTheme.text,
          lineHeight: 1
        }}
      >
        {value.toFixed(1)}
      </div>
    </div>
  )
}

function Analytics() {
  const [selectedNsi, setSelectedNsi] = useState('')
  const [selectedLlm, setSelectedLlm] = useState('')
  const [selectedTheme, setSelectedTheme] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState('')

  const [summary, setSummary] = useState<SummaryDto>({
    accuracy: 0,
    findability: 0,
    consistency: 0,
    totalmeasurements: 0
  })

  const [barData, setBarData] = useState<BarItemDto[]>([])
  const [timelineData, setTimelineData] = useState<TimelineItemDto[]>([])
  const [radarData, setRadarData] = useState<RadarItemDto[]>([])

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

  const data: AnalyticsResponse = await response.json()

  setSummary({
    accuracy: data.accuracyScore ?? 0,
    findability: data.findabilityScore ?? 0,
    consistency: data.consistencyScore ?? 0,
    totalmeasurements: data.totalMeasurements ?? 0
  })

  setBarData(
    data.barData ?? [
      { name: selectedLlm || 'Selected LLM', score: data.accuracyScore ?? 0 }
    ]
  )

  setTimelineData(
    data.timelineData ?? [
      {
        run: 'Current',
        accuracy: data.accuracyScore ?? 0,
        findability: data.findabilityScore ?? 0
      }
    ]
  )

  setRadarData(
    data.radarData ?? [
      { metric: 'Accuracy score', value: data.accuracyScore ?? 0 },
      { metric: 'Source score', value: data.findabilityScore ?? 0 },
      { metric: 'Consistency score', value: data.consistencyScore ?? 0 }
    ]
  )
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
    <div
      style={{
        background: pageTheme.background,
        minHeight: '100vh',
        padding: '24px',
        color: pageTheme.text,
        fontFamily: 'Arial, sans-serif'
      }}
    >
      <div style={{ maxWidth: '1400px', margin: '0 auto' }}>
        <h1 style={{ marginBottom: '24px' }}>Analytics</h1>

        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))',
            gap: '16px',
            marginBottom: '20px'
          }}
        >
          <div
            style={{
              background: pageTheme.cardBackground,
              border: `1px solid ${pageTheme.border}`,
              borderRadius: '16px',
              padding: '16px',
              boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
            }}
          >
            <label
              htmlFor="nsi-select"
              style={{
                display: 'block',
                fontWeight: 700,
                marginBottom: '8px'
              }}
            >
              Select NSI
            </label>

            <select
              id="nsi-select"
              value={selectedNsi}
              onChange={(e) => setSelectedNsi(e.target.value)}
              style={{
                width: '100%',
                padding: '12px',
                borderRadius: '10px',
                border: `1px solid ${pageTheme.border}`,
                fontSize: '14px'
              }}
            >
              <option value="">-- Select NSI --</option>
              {nsiOptions.map((nsi) => (
                <option key={nsi} value={nsi}>
                  {nsi}
                </option>
              ))}
            </select>
          </div>

          <div
            style={{
              background: pageTheme.cardBackground,
              border: `1px solid ${pageTheme.border}`,
              borderRadius: '16px',
              padding: '16px',
              boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
            }}
          >
            <label
              htmlFor="llm-select"
              style={{
                display: 'block',
                fontWeight: 700,
                marginBottom: '8px'
              }}
            >
              Select LLM
            </label>

            <select
              id="llm-select"
              value={selectedLlm}
              onChange={(e) => setSelectedLlm(e.target.value)}
              style={{
                width: '100%',
                padding: '12px',
                borderRadius: '10px',
                border: `1px solid ${pageTheme.border}`,
                fontSize: '14px'
              }}
            >
              <option value="">-- Select LLM --</option>
              {llmOptions.map((llm) => (
                <option key={llm} value={llm}>
                  {llm}
                </option>
              ))}
            </select>
          </div>

          <div
            style={{
              background: pageTheme.cardBackground,
              border: `1px solid ${pageTheme.border}`,
              borderRadius: '16px',
              padding: '16px',
              boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
            }}
          >
            <label
              htmlFor="theme-select"
              style={{
                display: 'block',
                fontWeight: 700,
                marginBottom: '8px'
              }}
            >
              Select Theme
            </label>

            <select
              id="theme-select"
              value={selectedTheme}
              onChange={(e) => setSelectedTheme(e.target.value)}
              style={{
                width: '100%',
                padding: '12px',
                borderRadius: '10px',
                border: `1px solid ${pageTheme.border}`,
                fontSize: '14px'
              }}
            >
              <option value="">-- Select Theme --</option>
              {themeOptions.map((themeOption) => (
                <option key={themeOption} value={themeOption}>
                  {themeOption}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div style={{ marginBottom: '32px' }}>
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

        <div
          style={{
            marginBottom: '32px',
            padding: '20px',
            borderRadius: '16px',
            background: pageTheme.cardBackground,
            border: `1px solid ${pageTheme.border}`,
            boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
          }}
        >
          <h2 style={{ marginTop: 0 }}>Selected filters</h2>
          <p>
            <strong>NSI:</strong> {selectedNsi || 'None'}
          </p>
          <p>
            <strong>LLM:</strong> {selectedLlm || 'None'}
          </p>
          <p>
            <strong>Theme:</strong> {selectedTheme || 'None'}
          </p>

          {error && (
            <p style={{ color: 'red', marginTop: '12px' }}>
              <strong>Error:</strong> {error}
            </p>
          )}
        </div>

        <div
          style={{
            display: 'flex',
            gap: '20px',
            flexWrap: 'wrap',
            marginBottom: '40px'
          }}
        >
          <ScoreCard title="Accuracy Score" value={summary.accuracy} />
          <ScoreCard title="Source Score" value={summary.findability} />
          <ScoreCard title="Consistency Score" value={summary.consistency} />
          <ScoreCard title="Total measurements" value={summary.totalmeasurements} />
        </div>

        <div
          style={{
            display: 'flex',
            gap: '24px',
            alignItems: 'flex-start',
            flexWrap: 'wrap'
          }}
        >
          <div
            style={{
              background: pageTheme.cardBackground,
              border: `1px solid ${pageTheme.border}`,
              borderRadius: '16px',
              padding: '20px',
              boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
            }}
          >
            <h3 style={{ marginTop: 0, marginBottom: '16px', color: pageTheme.text }}>
              Score per LLM
            </h3>

            <BarChart width={420} height={280} data={barData}>
              <CartesianGrid stroke={pageTheme.grid} strokeDasharray="3 3" />
              <XAxis dataKey="name" stroke={pageTheme.mutedText} />
              <YAxis domain={[0, 10]} stroke={pageTheme.mutedText} />
              <Tooltip
                contentStyle={{
                  backgroundColor: pageTheme.tooltipBackground,
                  border: `1px solid ${pageTheme.border}`,
                  borderRadius: '10px'
                }}
              />
              <Bar dataKey="score" fill={pageTheme.primary} radius={[8, 8, 0, 0]} />
            </BarChart>
          </div>

          <div
            style={{
              background: pageTheme.cardBackground,
              border: `1px solid ${pageTheme.border}`,
              borderRadius: '16px',
              padding: '20px',
              boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
            }}
          >
            <h3 style={{ marginTop: 0, marginBottom: '16px', color: pageTheme.text }}>
              Timeline
            </h3>

            <LineChart width={420} height={280} data={timelineData}>
              <CartesianGrid stroke={pageTheme.grid} strokeDasharray="3 3" />
              <XAxis dataKey="run" stroke={pageTheme.mutedText} />
              <YAxis domain={[0, 10]} stroke={pageTheme.mutedText} />
              <Tooltip
                contentStyle={{
                  backgroundColor: pageTheme.tooltipBackground,
                  border: `1px solid ${pageTheme.border}`,
                  borderRadius: '10px'
                }}
              />
              <Line
                type="monotone"
                dataKey="accuracy"
                stroke={pageTheme.primary}
                strokeWidth={3}
                dot={{ r: 5 }}
              />
              <Line
                type="monotone"
                dataKey="findability"
                stroke={pageTheme.secondary}
                strokeWidth={3}
                dot={{ r: 5 }}
              />
            </LineChart>
          </div>

          <div
            style={{
              background: pageTheme.cardBackground,
              border: `1px solid ${pageTheme.border}`,
              borderRadius: '16px',
              padding: '20px',
              boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
            }}
          >
            <h3 style={{ marginTop: 0, marginBottom: '16px', color: pageTheme.text }}>
              Metrics overview
            </h3>

            <RadarChart width={420} height={280} data={radarData}>
              <PolarGrid stroke={pageTheme.grid} />
              <PolarAngleAxis dataKey="metric" tick={{ fill: pageTheme.mutedText }} />
              <Radar
                dataKey="value"
                stroke={pageTheme.radarStroke}
                fill={pageTheme.radarFill}
                fillOpacity={0.5}
                strokeWidth={2}
              />
              <Tooltip
                contentStyle={{
                  backgroundColor: pageTheme.tooltipBackground,
                  border: `1px solid ${pageTheme.border}`,
                  borderRadius: '10px'
                }}
              />
            </RadarChart>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Analytics