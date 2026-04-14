import { useRef, useState } from 'react'
import { createPortal } from 'react-dom'

type Props = {
  title: string
  value: string | number
  color: string
  tooltip?: string
}

export function MetricCard({ title, value, color, tooltip }: Props) {
  const [cardHovered, setCardHovered] = useState(false)
  const [infoHovered, setInfoHovered] = useState(false)
  const buttonRef = useRef<HTMLButtonElement | null>(null)

  const buttonRect = buttonRef.current?.getBoundingClientRect()

  const tooltipTop = buttonRect ? buttonRect.bottom + 10 : 0
  const tooltipLeft = buttonRect ? buttonRect.right - 280 : 0

  return (
    <div
      style={{
        position: 'relative',
        padding: '22px',
        borderRadius: '18px',
        border: '1px solid #e2e8f0',
        background: '#ffffff',
        boxShadow: cardHovered
          ? '0 16px 32px rgba(15, 23, 42, 0.14)'
          : '0 8px 24px rgba(15, 23, 42, 0.08)',
        transform: cardHovered ? 'translateY(-3px)' : 'translateY(0)',
        transition: 'all 0.15s ease',
        cursor: 'default'
      }}
      onMouseEnter={() => setCardHovered(true)}
      onMouseLeave={() => setCardHovered(false)}
    >
      {tooltip && (
        <div
          style={{
            position: 'absolute',
            top: 14,
            right: 14
          }}
        >
          <button
            ref={buttonRef}
            type="button"
            aria-label={`More information about ${title}`}
            onMouseEnter={() => setInfoHovered(true)}
            onMouseLeave={() => setInfoHovered(false)}
            style={{
              width: 26,
              height: 26,
              borderRadius: '999px',
              border: '1px solid #cbd5e1',
              background: infoHovered ? '#eff6ff' : '#f8fafc',
              color: '#475569',
              fontSize: 14,
              fontWeight: 700,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              transition: 'all 0.15s ease',
              boxShadow: infoHovered
                ? '0 6px 16px rgba(37, 99, 235, 0.18)'
                : 'none'
            }}
          >
            i
          </button>

          {infoHovered &&
            buttonRect &&
            createPortal(
              <div
                onMouseEnter={() => setInfoHovered(true)}
                onMouseLeave={() => setInfoHovered(false)}
                style={{
                  position: 'fixed',
                  top: tooltipTop,
                  left: tooltipLeft,
                  width: 280,
                  padding: '12px 14px',
                  borderRadius: 14,
                  background: '#0f172a',
                  color: '#f8fafc',
                  fontSize: 13,
                  lineHeight: 1.5,
                  boxShadow: '0 18px 40px rgba(15, 23, 42, 0.22)',
                  zIndex: 9999
                }}
              >
                <div
                  style={{
                    position: 'absolute',
                    top: -6,
                    right: 16,
                    width: 12,
                    height: 12,
                    background: '#0f172a',
                    transform: 'rotate(45deg)'
                  }}
                />

                <div
                  style={{
                    fontWeight: 700,
                    marginBottom: 6
                  }}
                >
                  {title}
                </div>

                <div style={{ whiteSpace: 'pre-line' }}>{tooltip}</div>
              </div>,
              document.body
            )}
        </div>
      )}

      <h3
        style={{
          marginTop: 0,
          marginBottom: 12,
          color,
          paddingRight: tooltip ? 36 : 0
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