import { useEffect, useState } from 'react'
import { API_BASE_URL } from '../../config'

type FiltersProps = {
  selectedNsis: string[]
  selectedLlms: string[]
  selectedThemes: string[]
  onNsisChange: (value: string[]) => void
  onLlmsChange: (value: string[]) => void
  onThemesChange: (value: string[]) => void
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
  selectedNsis,
  selectedLlms,
  selectedThemes,
  onNsisChange,
  onLlmsChange,
  onThemesChange
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

  function getSelectedValues(e: React.ChangeEvent<HTMLSelectElement>) {
  return Array.from(e.target.selectedOptions).map(option => option.value)
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
  multiple
  value={selectedNsis}
  onChange={(e) => onNsisChange(getSelectedValues(e))}
  style={selectStyle}
>
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
  multiple
  value={selectedLlms}
  onChange={(e) => onLlmsChange(getSelectedValues(e))}
  style={selectStyle}
>
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
  multiple
  value={selectedThemes}
  onChange={(e) => onThemesChange(getSelectedValues(e))}
  style={selectStyle}
>
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