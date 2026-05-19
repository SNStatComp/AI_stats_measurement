import type { AnalyticsResponse, MetricsOverTime } from '../Analytics'
import { MetricCard } from './MetricCard'
import { TopSources } from './TopSources'
import { getResultTheme } from '../utils/getResultTheme'
import cbsLogo from '../assets/cbs.png'
import oecdLogo from '../assets/oecd.png'
import statbankLogo from '../assets/StatBank Denmark.svg'
import { MetricsLineChart } from './MetricsLineChart'
  

const logos: Record<string, string> = {
  CBS: cbsLogo,
  OECD: oecdLogo,
  'StatBank Denmark': statbankLogo
}

type ResultCardProps = {
  item: AnalyticsResponse
  chartData?: MetricsOverTime
  groupBy: 'nsi' | 'model' | 'theme'
}

export function ResultCard({ item, chartData, groupBy }: ResultCardProps) {
  const label = item.nsi
  const theme = getResultTheme(label)

  const logo = logos[label]


   return (
    <div style={{
      position: 'relative',
      padding: '32px 24px 24px',
      borderRadius: '24px',
      background: theme.color,
      boxShadow: '0 10px 30px rgba(15, 23, 42, 0.08)',
    }}>
      <div style={{
        position: 'absolute',
        top: -18,
        left: 24,
        background: 'white',
        borderRadius: '16px',
        padding: '10px 18px',
        boxShadow: '0 8px 18px rgba(0,0,0,0.12)',
        fontWeight: 700
      }}>
        {logo ? (
          <img
            src={logo}
            alt={label}
            style={{ height: 44, width: 'auto', objectFit: 'contain' }}
          />
        ) : (
          label
        )}
      </div>

      <div style={{ height: 28 }} />

      <div style={{
        display: 'grid',
        gridTemplateColumns: '1fr 1fr',
        gap: '20px'
      }}>
        <MetricCard title="Accuracy" value={item.accuracyScore.toFixed(1)} color={theme.color} tooltip={item.accuracyScoreTooltip} />
        <MetricCard title="Findability" value={item.findabilityScore.toFixed(1)} color={theme.color} tooltip={item.findabilityScoreTooltip} />
        <MetricCard title="Consistency" value={item.consistencyScore.toFixed(1)} color={theme.color} tooltip={item.consistencyScoreTooltip} />

        <MetricCard
          title="Total measurements"
          value={item.totalMeasurements}
          color={theme.color}
          tooltip={`Total number of measurements for this ${groupBy}.`}
        />
      </div>

      <TopSources sources={item.topSources} color="white" />

      {chartData && (
        <div style={{ marginTop: '20px' }}>
          <MetricsLineChart data={chartData} />
        </div>
      )}
    </div>
  )
}