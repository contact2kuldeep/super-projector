// Typed client for the Super Projector API.
// The types below mirror the C# records in api/src/SuperProjector.Api/Domain.
// ASP.NET serialises property names in camelCase, so `ProjectedBalance`
// in C# arrives as `projectedBalance` here.

/** What the member enters. Body of POST /api/projection. */
export type ProjectionRequest = {
  age: number
  currentBalance: number
  annualSalary: number
  contributionRatePercent: number
}

/** The economic assumptions the API used, echoed back with every result. */
export type ProjectionAssumptions = {
  netReturnPercent: number
  wageGrowthPercent: number
  contributionsTaxPercent: number
  retirementAge: number
}

/** The successful (200) response from POST /api/projection. */
export type ProjectionResponse = {
  yearsToRetirement: number
  retirementAge: number
  projectedBalance: number
  totalContributions: number
  totalInvestmentEarnings: number
  assumptions: ProjectionAssumptions
  disclaimer: string
}

/**
 * Every possible outcome of a call. Using a "discriminated union" (checked
 * via `kind`) instead of throwing makes callers handle each case explicitly.
 */
export type ProjectionOutcome =
  | { kind: 'success'; data: ProjectionResponse }
  | { kind: 'validation'; messages: string[] }
  | { kind: 'error'; message: string }

// Shape of ASP.NET's 400 "validation problem" body (only the parts we use).
type ValidationProblem = {
  errors?: Record<string, string[]>
}

// Set in .env.development (and per environment at build time). Falls back to
// the same origin, which is handy if the API is ever served behind one host.
const apiBaseUrl: string = import.meta.env.VITE_API_BASE_URL ?? ''

const friendlyErrorMessage =
  "Sorry, we couldn't reach the projection service. Please check your connection and try again."

/** Calls the API and returns a typed outcome. Never throws. */
export async function calculateProjection(
  request: ProjectionRequest,
): Promise<ProjectionOutcome> {
  let response: Response
  try {
    response = await fetch(`${apiBaseUrl}/api/projection`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    })
  } catch {
    // fetch only rejects on network failure (API down, offline, CORS blocked).
    return { kind: 'error', message: friendlyErrorMessage }
  }

  if (response.ok) {
    const data = (await response.json()) as ProjectionResponse
    return { kind: 'success', data }
  }

  if (response.status === 400) {
    // Flatten { field: [messages] } into a simple list for display.
    const problem = (await response.json().catch(() => ({}))) as ValidationProblem
    const messages = Object.values(problem.errors ?? {}).flat()
    return {
      kind: 'validation',
      messages: messages.length > 0 ? messages : ['Please check your details and try again.'],
    }
  }

  // Any other status (e.g. 500) is our fault, not the user's.
  return { kind: 'error', message: friendlyErrorMessage }
}
