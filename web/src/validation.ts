// Client-side validation. These rules match ProjectionRequestValidator in the
// API, so users get instant feedback. The API still validates too: never
// trust the browser alone.

import type { ProjectionRequest } from './api/projectionClient'

/** Raw form values. Kept as strings so a field can be empty while typing. */
export type ProjectionFormValues = Record<keyof ProjectionRequest, string>

/** One optional error message per field. */
export type ProjectionFormErrors = Partial<Record<keyof ProjectionRequest, string>>

/** Checks every field and returns any errors (an empty object means valid). */
export function validateProjectionForm(values: ProjectionFormValues): ProjectionFormErrors {
  const errors: ProjectionFormErrors = {}

  const age = parse(values.age)
  if (age === null) errors.age = 'Enter your age.'
  else if (!Number.isInteger(age)) errors.age = 'Age must be a whole number.'
  else if (age < 18 || age > 66) errors.age = 'Age must be between 18 and 66.'

  const balance = parse(values.currentBalance)
  if (balance === null) errors.currentBalance = 'Enter your current balance.'
  else if (balance < 0) errors.currentBalance = 'Current balance cannot be negative.'

  const salary = parse(values.annualSalary)
  if (salary === null) errors.annualSalary = 'Enter your annual salary.'
  else if (salary < 0) errors.annualSalary = 'Annual salary cannot be negative.'

  const rate = parse(values.contributionRatePercent)
  if (rate === null) errors.contributionRatePercent = 'Enter a contribution rate.'
  else if (rate < 0 || rate > 50)
    errors.contributionRatePercent = 'Contribution rate must be between 0 and 50%.'

  return errors
}

/** Converts valid form values to the numbers the API expects. */
export function toProjectionRequest(values: ProjectionFormValues): ProjectionRequest {
  return {
    age: Number(values.age),
    currentBalance: Number(values.currentBalance),
    annualSalary: Number(values.annualSalary),
    contributionRatePercent: Number(values.contributionRatePercent),
  }
}

// Returns the number, or null for empty/non-numeric input.
// (Number('') is 0, so we must check for empty text first.)
function parse(text: string): number | null {
  if (text.trim() === '') return null
  const value = Number(text)
  return Number.isFinite(value) ? value : null
}
