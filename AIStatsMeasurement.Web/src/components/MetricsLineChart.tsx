import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend
} from 'recharts'

export type ChartPoint = {
  label: string
  value: number
}

type MetricsOverTime = {
  accuracy: ChartPoint[]
  consistency: ChartPoint[]
  findability: ChartPoint[]
}

type ChartRow = {
  label: string
  accuracy?: number
  consistency?: number
  findability?: number
}

type Props = {
  data: MetricsOverTime
}

function mergeChartData(data: MetricsOverTime): ChartRow[] {
  const map = new Map<string, ChartRow>()

  data.accuracy.forEach((point) => {
    map.set(point.label, {
      ...map.get(point.label),
      label: point.label,
      accuracy: point.value
    })
  })

  data.consistency.forEach((point) => {
    map.set(point.label, {
      ...map.get(point.label),
      label: point.label,
      consistency: point.value
    })
  })

  data.findability.forEach((point) => {
    map.set(point.label, {
      ...map.get(point.label),
      label: point.label,
      findability: point.value
    })
  })

  return Array.from(map.values()).sort((a, b) =>
    a.label.localeCompare(b.label)
  )
}

export function MetricsLineChart({ data }: Props) {
  const chartData = mergeChartData(data)

  return (
    <div
      style={{
        background: '#ffffff',
        border: '1px solid #e2e8f0',
        borderRadius: '16px',
        padding: '24px',
        marginBottom: '24px'
      }}
    >
      <h2 style={{ marginBottom: '16px' }}>Scores over time</h2>

      <LineChart
        width={900}
        height={360}
        data={chartData}
        margin={{
          top: 10,
          right: 30,
          left: 0,
          bottom: 10
        }}
      >
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="label" />
        <YAxis domain={[0, 10]} />
        <Tooltip />
        <Legend />

        <Line
          type="monotone"
          dataKey="accuracy"
          name="Accuracy"
          stroke="#2563eb"
          activeDot={{ r: 8 }}
        />

        <Line
          type="monotone"
          dataKey="consistency"
          name="Consistency"
          stroke="#16a34a"
          activeDot={{ r: 8 }}
        />

        <Line
          type="monotone"
          dataKey="findability"
          name="Findability"
          stroke="#dc2626"
          activeDot={{ r: 8 }}
        />
      </LineChart>
    </div>
  )
}