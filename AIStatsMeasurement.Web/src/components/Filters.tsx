import { useEffect, useState } from 'react'
import { API_BASE_URL } from '../../config'

type FiltersProps = {
  selectedNsi: string
  selectedLlm: string
  selectedTheme: string
  onNsiChange: (value: string) => void
  onLlmChange: (value: string) => void
  onThemeChange: (value: string) => void
}

const nsiOptions = ['CBS', 'OECD', 'StatBank Denmark']

const llmOptions = [
  'gemini-2.5-flash-lite',
  'gpt-4o-mini',
  'grok-4-1-fast-non-reasoning',
  'grok-4.20-0309-reasoning',
  'gemini-3.1-pro',
  'gemini-2.5-pro',
  'gpt-5.4',
  'grok-4.20-reasoning',
  'grok-4.3',
  'websearch enabled',
  'websearch disabled'
]

export function Filters({
  selectedNsi,
  selectedLlm,
  selectedTheme,
  onNsiChange,
  onLlmChange,
  onThemeChange
}: FiltersProps) {
  const [themes, setThemes] = useState<string[]>([])

  useEffect(() => {
    fetch(`${API_BASE_URL}/api/prompts/themes`)
      .then((res) => res.json())
      .then((data: string[]) => setThemes(data))
      .catch(() => console.log('Failed loading themes'))
  }, [])

  const cardStyle = {
    background: '#ffffff',
    border: '1px solid #e2e8f0',
    borderRadius: '16px',
    padding: '16px',
    boxShadow: '0 4px 14px rgba(15, 23, 42, 0.06)'
  }

  const selectStyle = {
    width: '100%',
    padding: '12px',
    borderRadius: '10px',
    border: '1px solid #e2e8f0',
    fontSize: '14px'
  }

  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))',
        gap: '16px',
        marginBottom: '20px'
      }}
    >
      <div style={cardStyle}>
        <label style={{ display: 'block', fontWeight: 700, marginBottom: '8px' }}>
          Select NSI
        </label>

        <select
          value={selectedNsi}
          onChange={(e) => onNsiChange(e.target.value)}
          style={selectStyle}
        >
          <option value="">All NSI's</option>
          {nsiOptions.map((nsi) => (
            <option key={nsi} value={nsi}>
              {nsi}
            </option>
          ))}
        </select>
      </div>

      <div style={cardStyle}>
        <label style={{ display: 'block', fontWeight: 700, marginBottom: '8px' }}>
          Select LLM
        </label>

        <select
          value={selectedLlm}
          onChange={(e) => onLlmChange(e.target.value)}
          style={selectStyle}
        >
          <option value="">All LLMs</option>
          {llmOptions.map((llm) => (
            <option key={llm} value={llm}>
              {llm}
            </option>
          ))}
        </select>
      </div>

      <div style={cardStyle}>
        <label style={{ display: 'block', fontWeight: 700, marginBottom: '8px' }}>
          Select Theme
        </label>

        <select
          value={selectedTheme}
          onChange={(e) => onThemeChange(e.target.value)}
          style={selectStyle}
        >
          <option value="">All themes</option>
          {themes.map((theme) => (
            <option key={theme} value={theme}>
              {theme}
            </option>
          ))}
        </select>
      </div>
    </div>
  )
}