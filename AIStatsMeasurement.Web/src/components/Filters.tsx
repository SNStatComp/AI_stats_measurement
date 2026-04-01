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

export function Filters({
  selectedNsi,
  selectedLlm,
  selectedTheme,
  onNsiChange,
  onLlmChange,
  onThemeChange
}: FiltersProps) {
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
          <option value="">-- Select NSI --</option>
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
          <option value="">-- Select LLM --</option>
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
          <option value="">-- Select Theme --</option>
          {themeOptions.map((theme) => (
            <option key={theme} value={theme}>
              {theme}
            </option>
          ))}
        </select>
      </div>
    </div>
  )
}