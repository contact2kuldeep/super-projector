import { describe, expect, it } from 'vitest'
import {
  toProjectionRequest,
  validateProjectionForm,
  type ProjectionFormValues,
} from './validation'

// A known-good set of values. Each test changes just one field from this,
// so any error must have come from that field.
const valid: ProjectionFormValues = {
  age: '40',
  currentBalance: '150000',
  annualSalary: '110000',
  contributionRatePercent: '12',
}

describe('validateProjectionForm', () => {
  it('returns no errors for valid values', () => {
    expect(validateProjectionForm(valid)).toEqual({})
  })

  describe('age', () => {
    // it.each runs the same test once per row, like xUnit's [InlineData].
    it.each(['17', '67'])('rejects %s as outside 18-66', (age) => {
      expect(validateProjectionForm({ ...valid, age })).toEqual({
        age: 'Age must be between 18 and 66.',
      })
    })

    it.each(['18', '66'])('accepts boundary value %s', (age) => {
      expect(validateProjectionForm({ ...valid, age })).toEqual({})
    })

    it('rejects a non-whole number', () => {
      expect(validateProjectionForm({ ...valid, age: '40.5' })).toEqual({
        age: 'Age must be a whole number.',
      })
    })
  })

  describe('current balance', () => {
    it('rejects -1', () => {
      expect(validateProjectionForm({ ...valid, currentBalance: '-1' })).toEqual({
        currentBalance: 'Current balance cannot be negative.',
      })
    })

    it('accepts 0', () => {
      expect(validateProjectionForm({ ...valid, currentBalance: '0' })).toEqual({})
    })
  })

  describe('annual salary', () => {
    it('rejects -1', () => {
      expect(validateProjectionForm({ ...valid, annualSalary: '-1' })).toEqual({
        annualSalary: 'Annual salary cannot be negative.',
      })
    })

    it('accepts 0', () => {
      expect(validateProjectionForm({ ...valid, annualSalary: '0' })).toEqual({})
    })
  })

  describe('contribution rate', () => {
    it.each(['-1', '51'])('rejects %s as outside 0-50', (contributionRatePercent) => {
      expect(validateProjectionForm({ ...valid, contributionRatePercent })).toEqual({
        contributionRatePercent: 'Contribution rate must be between 0 and 50%.',
      })
    })

    it.each(['0', '50'])('accepts boundary value %s', (contributionRatePercent) => {
      expect(validateProjectionForm({ ...valid, contributionRatePercent })).toEqual({})
    })
  })

  describe('empty or non-numeric input', () => {
    // Number('') is 0, so without a specific check an empty field would
    // silently pass as zero. These tests guard against that bug.
    it.each(['', '   ', 'abc'])('reports every field as missing for %j', (text) => {
      const blank: ProjectionFormValues = {
        age: text,
        currentBalance: text,
        annualSalary: text,
        contributionRatePercent: text,
      }

      expect(validateProjectionForm(blank)).toEqual({
        age: 'Enter your age.',
        currentBalance: 'Enter your current balance.',
        annualSalary: 'Enter your annual salary.',
        contributionRatePercent: 'Enter a contribution rate.',
      })
    })
  })

  it('reports errors for several fields at once', () => {
    const errors = validateProjectionForm({
      age: '70',
      currentBalance: '-1',
      annualSalary: '-1',
      contributionRatePercent: '60',
    })

    expect(Object.keys(errors)).toEqual([
      'age',
      'currentBalance',
      'annualSalary',
      'contributionRatePercent',
    ])
  })
})

describe('toProjectionRequest', () => {
  it('converts form text to the numbers the API expects', () => {
    expect(
      toProjectionRequest({
        age: '40',
        currentBalance: '150000.50',
        annualSalary: '110000',
        contributionRatePercent: '11.5',
      }),
    ).toEqual({
      age: 40,
      currentBalance: 150000.5,
      annualSalary: 110000,
      contributionRatePercent: 11.5,
    })
  })
})
