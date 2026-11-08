import { useState } from 'react'
import {
  calculateProjection,
  type ProjectionOutcome,
  type ProjectionRequest,
} from './api/projectionClient'
import { ProjectionForm } from './components/ProjectionForm'
import { ProjectionResult } from './components/ProjectionResult'

/** The whole page: the form on top, the outcome underneath. */
export default function App() {
  const [isLoading, setIsLoading] = useState(false)
  const [outcome, setOutcome] = useState<ProjectionOutcome | null>(null)

  async function handleSubmit(request: ProjectionRequest) {
    setIsLoading(true)
    setOutcome(await calculateProjection(request))
    setIsLoading(false)
  }

  return (
    <main className="page">
      <h1>Super Projector</h1>
      <p className="intro">
        Estimate what your superannuation balance could be when you reach retirement age.
      </p>

      <ProjectionForm onSubmit={handleSubmit} isLoading={isLoading} />

      {/*
        aria-live="polite" makes screen readers announce whatever appears in
        here once they finish speaking. The region must already be on the
        page before its content changes, so it is always rendered.
      */}
      <div aria-live="polite">
        {outcome?.kind === 'success' && <ProjectionResult result={outcome.data} />}

        {outcome?.kind === 'validation' && (
          <div className="card message message-error">
            <h2>Please check your details</h2>
            <ul>
              {outcome.messages.map((message) => (
                <li key={message}>{message}</li>
              ))}
            </ul>
          </div>
        )}

        {outcome?.kind === 'error' && (
          <div className="card message message-error">
            <p>{outcome.message}</p>
          </div>
        )}
      </div>
    </main>
  )
}
