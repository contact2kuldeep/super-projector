import react from '@vitejs/plugin-react'
// Importing defineConfig from 'vitest/config' adds types for the `test` block.
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    // jsdom gives tests a simulated browser (document, window) to render into.
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
