import { API_BASE_URL } from '../../config'
import { apiFetch } from '../apiFetch'

export type ExportRow = {
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

export type SourceDto = {
  id: number
  name: string | null
  url: string | null
  type: string | null
}

export type ResultWithSources = ExportRow & {
  actualSourceDetails: SourceDto[]
}

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms))

const fetchSourcesByIds = async (ids: number[]): Promise<SourceDto[]> => {
  if (!ids.length) return []

  const response = await apiFetch(`/api/sources/getByIds`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(ids)
  })

  if (!response.ok) {
    throw new Error('Failed to fetch sources')
  }

  return response.json()
}

const enrichResultsWithSources = async (
  data: ExportRow[]
): Promise<ResultWithSources[]> => {
  const allSourceIds = [...new Set(data.flatMap((r) => r.actualSource ?? []))]
  const sourceDtos = await fetchSourcesByIds(allSourceIds)

  const sourceMap = new Map<number, SourceDto>(
    sourceDtos.map((source) => [source.id, source])
  )

  return data.map((result) => ({
    ...result,
    actualSourceDetails: (result.actualSource ?? [])
      .map((id) => sourceMap.get(id))
      .filter((source): source is SourceDto => Boolean(source))
  }))
}

export const runLlmJob = async (
  promptIds: number[],
  modelNames: string[],
  onStatusChange?: (status: string) => void
): Promise<ResultWithSources[]> => {
  onStatusChange?.('Starting job...')

  const startResponse = await apiFetch(`${API_BASE_URL}/api/llm/run`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      promptIds,
      modelNames
    })
  })

  if (!startResponse.ok) {
    const errorText = await startResponse.text()
    throw new Error(errorText || 'Failed to start job')
  }

  const startData: { jobId: string } = await startResponse.json()
  const jobId = startData.jobId

  let status = 'Queued'

  while (status !== 'Completed' && status !== 'Failed') {
    await sleep(3000)

    const statusResponse = await apiFetch(`${API_BASE_URL}/api/llmjobs/jobs/${jobId}`)

    if (!statusResponse.ok) {
      const errorText = await statusResponse.text()
      throw new Error(errorText || 'Failed to check job status')
    }

    const job: {
      status: string
      error?: string | null
    } = await statusResponse.json()

    status = job.status
    onStatusChange?.(`Job status: ${status}`)

    if (status === 'Failed') {
      throw new Error(job.error ?? 'Job failed')
    }
  }

  const resultResponse = await apiFetch(`${API_BASE_URL}/api/llmjobs/jobs/${jobId}/result`)

  if (!resultResponse.ok) {
    const errorText = await resultResponse.text()
    throw new Error(errorText || 'Failed to fetch job result')
  }

  const data: ExportRow[] = await resultResponse.json()
  return enrichResultsWithSources(data)
}