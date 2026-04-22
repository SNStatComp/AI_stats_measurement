import { useState, type FormEvent } from 'react'
import { API_BASE_URL } from '../config'
import { apiFetch } from './apiFetch';

type CreatePromptRequest = {
  provider: string
  theme: string
  periode: string
  subject: string
  question: string
  answer: number
  sourceName: string
  sourceType: string
  sourceUrl: string
  instruction: string
  answerLocation: string
  dimensions: Record<string, string>
}

const defaultInstruction =
  "Je bent een behulpzame en neutrale assistent voor algemene kennisvragen. " +
  "Beantwoord vragen kort en duidelijk, in correct Nederlands. " +
  "Vermeld welke bron je hebt gebruikt als link.";

function CreatePrompt() {
  const [form, setForm] = useState<CreatePromptRequest>({
    provider: '',
    theme: '',
    periode: '2020-02-24',
    subject: '',
    question: '',
    answer: 0,
    sourceName: '',
    sourceType: '',
    sourceUrl: '',
    instruction: defaultInstruction,
    answerLocation: '',
    dimensions: {}
  })

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target

    setForm((prev) => ({
      ...prev,
      [name]: name === 'answer' ? Number(value) : value
    }))
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()

    setIsSubmitting(true)
    setMessage('')
    setError('')

    try {
      const response = await apiFetch(`${API_BASE_URL}/api/prompts`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify([form])
        })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText || 'Failed to create prompt')
      }

      setMessage('Prompt created successfully.')

      setForm({
        provider: '',
        theme: '',
        periode: '',
        subject: '',
        question: '',
        answer: 0,
        sourceName: '',
        sourceType: 'NSI database',
        sourceUrl: '',
        instruction: '',
        answerLocation: '',
        dimensions: {}
      })
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message)
      } else {
        setError('Something went wrong.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  const cardStyle = {
    background: '#ffffff',
    border: '1px solid #e2e8f0',
    borderRadius: '18px',
    padding: '24px',
    boxShadow: '0 8px 24px rgba(15, 23, 42, 0.06)',
    maxWidth: '900px',
    margin: '0 auto'
  }

  const gridStyle = {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
    gap: '16px'
  }

  const fieldStyle = {
    display: 'flex',
    flexDirection: 'column' as const,
    gap: '8px'
  }

  const inputStyle = {
    padding: '12px',
    borderRadius: '10px',
    border: '1px solid #cbd5e1',
    fontSize: '14px'
  }

  const textAreaStyle = {
    ...inputStyle,
    minHeight: '110px',
    resize: 'vertical' as const
  }

  return (
    <div className="app-container">
      <h1>Create Prompt</h1>

      <form onSubmit={handleSubmit} style={cardStyle}>
        <div style={{ ...fieldStyle, marginTop: '16px' }}>
          <label>Question</label>
          <textarea
            name="question"
            value={form.question}
            onChange={handleChange}
            style={textAreaStyle}
            required
          />
        </div>

        <div style={{ ...fieldStyle, marginTop: '16px' }}>
          <label>LLM Instruction</label>
          <textarea
            name="instruction"
            value={form.instruction}
            onChange={handleChange}
            style={textAreaStyle}
          />
        </div>

        <div style={gridStyle}>
          <div style={fieldStyle}>
            <label>Data Provider</label>
            <select
              name="provider"
              value={form.provider}
              onChange={handleChange}
              style={inputStyle}
              required
            >
              <option value="">-- Select provider --</option>
              <option value="CBS">CBS</option>
              <option value="OECD">OECD</option>
              <option value="StatBank Denmark">StatBank Denmark</option>
            </select>
          </div>

          <div style={fieldStyle}>
            <label>NSI Theme</label>
            <input
              name="theme"
              value={form.theme}
              onChange={handleChange}
              style={inputStyle}
              required
            />
          </div>

          {/* <div style={fieldStyle}>
            <label>Periode</label>
            <input
              name="periode"
              value={form.periode}
              onChange={handleChange}
              placeholder="2020-02-24"
              style={inputStyle}
              required
            />
          </div> */}

          <div style={fieldStyle}>
            <label>Prompt Subject</label>
            <input
              name="subject"
              value={form.subject}
              onChange={handleChange}
              style={inputStyle}
              required
            />
          </div>

          <div style={fieldStyle}>
            <label>Prompt Answer</label>
            <input
              name="answer"
              type="number"
              value={form.answer}
              onChange={handleChange}
              style={inputStyle}
              required
            />
          </div>

          {/* <div style={fieldStyle}>
            <label>Source Type</label>
            <input
              name="sourceType"
              value={form.sourceType}
              onChange={handleChange}
              style={inputStyle}
            />
          </div> */}

          <div style={fieldStyle}>
            <label>Source Used Name</label>
            <input
              name="sourceName"
              value={form.sourceName}
              onChange={handleChange}
              style={inputStyle}
            />
          </div>

          <div style={fieldStyle}>
            <label>Source Used URL</label>
            <input
              name="sourceUrl"
              value={form.sourceUrl}
              onChange={handleChange}
              style={inputStyle}
            />
          </div>

          {/* <div style={fieldStyle}>
            <label>Answer Location</label>
            <input
              name="answerLocation"
              value={form.answerLocation}
              onChange={handleChange}
              style={inputStyle}
            />
          </div> */}

          {/* <div style={fieldStyle}>
            <label>Dimensions</label>
            <input
              name="dimensions"
              value={form.dimensions}
              onChange={handleChange}
              style={inputStyle}
              placeholder="{}"
            />
          </div> */}
        </div>

        <div style={{ marginTop: '24px', display: 'flex', gap: '12px', alignItems: 'center' }}>
          <button
            type="submit"
            disabled={isSubmitting}
            className="run-button"
          >
            {isSubmitting ? 'Creating...' : 'Create Prompt'}
          </button>

          {message && <span style={{ color: '#15803d', fontWeight: 600 }}>{message}</span>}
          {error && <span style={{ color: '#dc2626', fontWeight: 600 }}>{error}</span>}
        </div>
      </form>
    </div>
  )
}

export default CreatePrompt