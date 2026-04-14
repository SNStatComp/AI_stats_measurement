type SourceDto = {
  id: number
  name: string | null
  url: string | null
  type: string | null
}

type ExportRow = {
  id: number
  theme: string
  question: string
  expectedAnswer: number
  expectedSource: string
  actualAnswer: number
  actualSource: number[]
  provider: string
  rawText: string | null
  exception: string | null
  squareMeanRootError: number
  relativeError: number
  answerIsCorrect: boolean
  sourceIsCorrect: boolean
  createdUtc: string
}

type ResultWithSources = ExportRow & {
  actualSourceDetails: SourceDto[]
}

type Props = {
  result: ResultWithSources
  index: number
}

export default function ExportRowCard({ result, index }: Props) {
  return (
    <div
      key={`${result.provider}-${index}`}
      className="result-card"
      style={{
        position: 'relative',
        padding: '32px 24px 24px',
        borderRadius: '24px',
        background: '#16233b',
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
          boxShadow: '0 8px 18px rgba(0,0,0,0.12)',
          fontWeight: 700,
          fontSize: '1rem',
          color: '#16233b'
        }}
      >
        {result.provider}
      </div>

      <p>
        <strong>Expected answer:</strong> {result.expectedAnswer}
      </p>

      <p>
        <strong>Actual answer:</strong> {result.actualAnswer}
      </p>

      <div className="sources-section">
        <strong>Expected source:</strong>

        <div className="source-card-grid expected-grid">
          <div className="source-card">
            <div className="source-card-header">
              <span className="source-type-badge expected-badge">
                NSI Database
              </span>
            </div>

            <div className="source-card-body">
              <p className="source-name">{result.expectedSource}</p>
              <a
                href={result.expectedSource}
                target="_blank"
                rel="noopener noreferrer"
                className="source-link"
              >
                Open expected source
              </a>
            </div>
          </div>
        </div>
      </div>

      <div className="sources-section">
        <strong>Actual sources:</strong>

        {result.actualSourceDetails.length > 0 ? (
          <div className="source-card-grid">
            {result.actualSourceDetails.map((src) => (
              <div key={src.id} className="source-card">
                <div className="source-card-header">
                  <span className="source-type-badge">
                    {src.type || 'unknown'}
                  </span>
                </div>

                <div className="source-card-body">
                  <p className="source-name">
                    {src.name || `Source #${src.id}`}
                  </p>

                  {src.url ? (
                    <>
                      <p className="source-url">{src.url}</p>

                      <a
                        href={src.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="source-link"
                      >
                        Open source
                      </a>
                    </>
                  ) : (
                    <p className="source-no-link">No URL available</p>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="no-source-text">No reference found</p>
        )}
      </div>

      <p>
        <strong>Relative error:</strong>{' '}
        {(result.relativeError * 100).toFixed(1) + '%'}
      </p>

      <p>
        <strong>Answer correct:</strong>{' '}
        {result.answerIsCorrect ? 'yes' : 'no'}
      </p>

      <p>
        <strong>Source correct:</strong>{' '}
        {result.sourceIsCorrect ? 'yes' : 'no'}
      </p>

      <div className="raw-text-block">
        <strong>Raw text:</strong>
        <p className="raw-text">{result.rawText ?? 'No raw text available'}</p>
      </div>

      {result.exception && (
        <p className="exception-text">
          <strong>Exception:</strong> {result.exception}
        </p>
      )}
    </div>
  )
}