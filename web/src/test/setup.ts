// Runs before each test file.
import '@testing-library/jest-dom/vitest' // adds matchers like toBeInTheDocument()
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

afterEach(() => {
  cleanup() // remove anything rendered by the previous test
  vi.unstubAllGlobals() // undo any fetch mocks
})
