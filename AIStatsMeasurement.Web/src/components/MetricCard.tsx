type Props = {
  title: string
  value: string | number
  color: string
}

export function MetricCard({ title, value, color }: Props) {
  return (
    <div
      style={{
        padding: '22px',
        borderRadius: '18px',
        border: '1px solid #e2e8f0',
        background: '#ffffff',
        boxShadow: '0 8px 24px rgba(15, 23, 42, 0.08)',
        transition: 'all 0.15s ease',
        cursor: 'default'
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = 'translateY(-3px)'
        e.currentTarget.style.boxShadow =
          '0 16px 32px rgba(15, 23, 42, 0.14)'
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = 'translateY(0)'
        e.currentTarget.style.boxShadow =
          '0 8px 24px rgba(15, 23, 42, 0.08)'
      }}
    >
      <h3
        style={{
          marginTop: 0,
          marginBottom: 12,
          color
        }}
      >
        {title}
      </h3>

      <p
        style={{
          fontSize: 32,
          fontWeight: 700,
          margin: 0,
          color: '#0f172a'
        }}
      >
        {value}
      </p>
    </div>
  )
}