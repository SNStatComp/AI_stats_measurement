type Props = {
  sources: { hostname: string; count: number }[]
  color: string
}

export function TopSources({ sources, color }: Props) {
  return (
      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          gap: 10
        }}
      >
         <h3 style={{ color, marginTop: 0, marginBottom: 14 }}>
            Top Sources Cited
        </h3>
        {sources.map((s) => (
          <div
            key={s.hostname}
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              border: '1px solid #e2e8f0',
              padding: '12px 14px',
              borderRadius: 12,
              background: '#ffffff',
              boxShadow: '0 4px 12px rgba(15,23,42,0.05)',
              transition: 'all 0.15s ease'
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'translateY(-2px)'
              e.currentTarget.style.boxShadow =
                '0 10px 20px rgba(15,23,42,0.12)'
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'translateY(0)'
              e.currentTarget.style.boxShadow =
                '0 4px 12px rgba(15,23,42,0.05)'
            }}
          >
            <span>{s.hostname}</span>
            <strong>{s.count} times</strong>
          </div>
        ))}
      </div>
  )
}