import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import App from './App'
import type { ProjectionResponse } from './api/projectionClient'

// A realistic API response (matches the API's known-value test).
const sampleResponse: ProjectionResponse = {
  yearsToRetirement: 27,
  retirementAge: 67,
  projectedBalance: 1932437.34,
  totalContributions: 456762.09,
  totalInvestmentEarnings: 1325675.25,
  assumptions: {
    netReturnPercent: 6.5,
    wageGrowthPercent: 3,
    contributionsTaxPercent: 15,
    retirementAge: 67,
  },
  disclaimer: 'Illustrative only. Not financial advice.',
}

describe('App', () => {
  it('renders the form with all four labelled inputs', () => {
    render(<App />)

    // getByLabelText only finds an input if a <label> is correctly linked to
    // it, so this also checks basic accessibility.
    expect(screen.getByLabelText('Age')).toHaveValue(40)
    expect(screen.getByLabelText('Current balance ($)')).toHaveValue(150000)
    expect(screen.getByLabelText('Annual salary ($)')).toHaveValue(110000)
    expect(screen.getByLabelText('Contribution rate (%)')).toHaveValue(12)
  })

  it('shows an inline error for an invalid age and does not call the API', async () => {
    const fetchMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()
    render(<App />)

    const ageInput = screen.getByLabelText('Age')
    await user.clear(ageInput)
    await user.type(ageInput, '17')
    await user.click(screen.getByRole('button', { name: 'Calculate' }))

    expect(screen.getByText('Age must be between 18 and 66.')).toBeInTheDocument()
    expect(ageInput).toHaveAttribute('aria-invalid', 'true')
    expect(ageInput).toHaveAccessibleDescription('Age must be between 18 and 66.')
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('shows the formatted projected balance after a successful API call', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify(sampleResponse), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }),
    )
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()
    render(<App />)

    await user.click(screen.getByRole('button', { name: 'Calculate' }))

    // findBy* waits for the async update to finish.
    expect(await screen.findByText('$1,932,437.34')).toBeInTheDocument()
    expect(screen.getByText('Illustrative only. Not financial advice.')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringMatching(/\/api\/projection$/),
      expect.objectContaining({ method: 'POST' }),
    )
  })
})
