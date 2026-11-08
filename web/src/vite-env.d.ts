/// <reference types="vite/client" />

// Tells TypeScript which VITE_ environment variables exist and their types.
interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
