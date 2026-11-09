import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// Determinism (docs/testing.md): fixed time zone for every test worker.
process.env.TZ = 'UTC'

export default defineConfig({
  plugins: [react()],
  server: {
    // Dev only: the API (launchSettings http profile) serves /health; same origin, no CORS.
    proxy: { '/health': 'http://localhost:5002' },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    // resource hygiene: one forked worker, no watch (check passes --run)
    pool: 'forks',
    maxWorkers: 1,
  },
})
