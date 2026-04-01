import type { AnalyticsResponse } from '../Analytics'
import { MetricCard } from './MetricCard'
import { TopSources } from './TopSources'
import { getResultTheme } from '../utils/getResultTheme'

type ResultCardProps = {
  item: AnalyticsResponse
}

export function ResultCard({ item }: ResultCardProps) {
  const theme = getResultTheme(item.nsi)

  return (
    <div
      style={{
        padding: '24px',
        borderRadius: '18px',
        border: `2px solid ${theme.color}`,
        background: theme.background,
        boxShadow: '0 6px 18px rgba(15, 23, 42, 0.06)'
      }}
    >
      <h2
        style={{
          marginTop: 0,
          marginBottom: '20px',
          color: theme.color
        }}
      >
        {item.nsi}
      </h2>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
          gap: '16px',
          marginBottom: '16px'
        }}
      >
        <MetricCard
          title="Accuracy"
          value={item.accuracyScore.toFixed(1)}
          color={theme.color}
        />

        <MetricCard
          title="Findability"
          value={item.findabilityScore.toFixed(1)}
          color={theme.color}
        />

        <MetricCard
          title="Consistency"
          value={item.consistencyScore.toFixed(1)}
          color={theme.color}
        />

        <MetricCard
          title="Total measurements"
          value={item.totalMeasurements}
          color={theme.color}
        />
      </div>

      <TopSources sources={item.topSources} color={theme.color} />
    </div>
  )
}