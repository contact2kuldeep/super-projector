import { useRef, useState, type FormEvent } from 'react'
import type { ProjectionRequest } from '../api/projectionClient'
import {
  toProjectionRequest,
  validateProjectionForm,
  type ProjectionFormErrors,
  type ProjectionFormValues,
} from '../validation'

type Props = {
  /** Called with valid, parsed values when the user submits. */
  onSubmit: (request: ProjectionRequest) => void
  /** True while the API call is in progress. */
  isLoading: boolean
}

// Field definitions, so the four inputs are rendered by one loop rather than
// four copies of the same markup.
type FieldName = keyof ProjectionFormValues
const fields: { name: FieldName; label: string; min: number; max?: number; step: string }[] = [
  { name: 'age', label: 'Age', min: 18, max: 66, step: '1' },
  { name: 'currentBalance', label: 'Current balance ($)', min: 0, step: 'any' },
  { name: 'annualSalary', label: 'Annual salary ($)', min: 0, step: 'any' },
  { name: 'contributionRatePercent', label: 'Contribution rate (%)', min: 0, max: 50, step: 'any' },
]

const defaultValues: ProjectionFormValues = {
  age: '40',
  currentBalance: '150000',
  annualSalary: '110000',
  contributionRatePercent: '12',
}

export function ProjectionForm({ onSubmit, isLoading }: Props) {
  const [values, setValues] = useState(defaultValues)
  const [errors, setErrors] = useState<ProjectionFormErrors>({})
  // Refs to each input, so we can move focus to the first invalid field.
  const inputRefs = useRef<Partial<Record<FieldName, HTMLInputElement | null>>>({})

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault() // stop the browser doing a full-page form post

    const newErrors = validateProjectionForm(values)
    setErrors(newErrors)

    const firstInvalid = fields.find((field) => newErrors[field.name])
    if (firstInvalid) {
      // Helps keyboard and screen reader users find the problem straight away.
      inputRefs.current[firstInvalid.name]?.focus()
      return
    }

    onSubmit(toProjectionRequest(values))
  }

  return (
    // noValidate turns off the browser's built-in pop-up validation so our
    // own inline messages (which match the API rules) are shown instead.
    <form className="card" onSubmit={handleSubmit} noValidate>
      {fields.map((field) => {
        const inputId = `field-${field.name}`
        const errorId = `${inputId}-error`
        const error = errors[field.name]

        return (
          <div className="field" key={field.name}>
            <label htmlFor={inputId}>{field.label}</label>
            <input
              ref={(element) => {
                inputRefs.current[field.name] = element
              }}
              id={inputId}
              name={field.name}
              type="number"
              inputMode={field.step === '1' ? 'numeric' : 'decimal'}
              min={field.min}
              max={field.max}
              step={field.step}
              value={values[field.name]}
              onChange={(event) => setValues({ ...values, [field.name]: event.target.value })}
              // Only point at the error element when there is one to read.
              aria-invalid={error ? true : undefined}
              aria-describedby={error ? errorId : undefined}
            />
            {error && (
              <p className="field-error" id={errorId}>
                {error}
              </p>
            )}
          </div>
        )
      })}

      <button type="submit" disabled={isLoading}>
        {isLoading ? 'Calculating...' : 'Calculate'}
      </button>
    </form>
  )
}
