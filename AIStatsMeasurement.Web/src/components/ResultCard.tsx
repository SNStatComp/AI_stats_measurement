import type { AnalyticsResponse } from '../Analytics'
import { MetricCard } from './MetricCard'
import { TopSources } from './TopSources'
import { getResultTheme } from '../utils/getResultTheme'
import cbsLogo from '../assets/cbs.png'
import oecdLogo from '../assets/oecd.png'
import statbankLogo from '../assets/StatBank Denmark.svg'

const logos: Record<string, string> = {
  CBS: cbsLogo,
  OECD: oecdLogo,
  'StatBank Denmark': statbankLogo
}

type ResultCardProps = {
  item: AnalyticsResponse
}

export function ResultCard({ item }: ResultCardProps) {
  const theme = getResultTheme(item.nsi)

  return (
    <div
      style={{
        position: 'relative',
        padding: '32px 24px 24px',
        borderRadius: '24px',
        background: theme.color,
        boxShadow: '0 10px 30px rgba(15, 23, 42, 0.08)'
      }}
    >
  <div
    style={{
      position: 'absolute',
      top: -18,
      left: 24,
      background: 'white',
      borderRadius: '16px',
      padding: '10px 18px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      boxShadow: '0 8px 18px rgba(0,0,0,0.12)'
    }}
  >
    <img
      src={logos[item.nsi]}
      alt={item.nsi}
      style={{
        height: 44,
        width: 'auto',
        display: 'block',
        objectFit: 'contain'
      }}
    />
  </div>

  <div style={{ height: 28 }} />

  <div
    style={{
      display: 'grid',
      gridTemplateColumns: '1fr 1fr',
      gap: '20px'
    }}
  >
        <MetricCard
          title="Accuracy"
          value={item.accuracyScore.toFixed(1)}
          color={theme.color}
          tooltip={item.accuracyScoreTooltip}
        />
        
        <MetricCard
          title="Findability"
          value={item.findabilityScore.toFixed(1)}
          color={theme.color}
          tooltip={item.findabilityScoreTooltip}
        />

        <MetricCard
          title="Consistency"
          value={item.consistencyScore.toFixed(1)}
          color={theme.color}
          tooltip={item.consistencyScoreTooltip}  
        />

        <MetricCard
          title="Total measurements"
          value={item.totalMeasurements}
          color={theme.color}
          tooltip={`Total number of measurements performed for this NSI and prompt combination.`}
        />
      </div>

      <TopSources sources={item.topSources} color="white" />
    </div>
  )
}