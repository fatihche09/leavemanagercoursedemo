import { act, render, screen, waitFor } from '@testing-library/react'
import { delay, http, HttpResponse } from 'msw'
import { describe, expect, it, vi } from 'vitest'
import { App } from './App'
import { HEALTH_TIMEOUT_MS } from './ApiStatus'
import { server } from './test/setup'

const errorText = "API'ye şu anda ulaşılamıyor."

describe('ApiStatus', () => {
  it('shows loading text while checking', async () => {
    server.use(http.get('/health', () => delay('infinite')))

    render(<App />)

    expect(screen.getByRole('status')).toHaveTextContent('API durumu kontrol ediliyor…')
  })

  it('shows healthy text and app title when API is healthy', async () => {
    server.use(http.get('/health', () => HttpResponse.text('Healthy')))

    render(<App />)

    expect(await screen.findByText('API çalışıyor')).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('LeaveManager')
  })

  it('shows error on unhealthy response', async () => {
    server.use(http.get('/health', () => HttpResponse.text('Unhealthy', { status: 503 })))

    render(<App />)

    expect(await screen.findByText(errorText)).toBeInTheDocument()
    expect(screen.getByRole('status')).not.toHaveTextContent('Unhealthy')
  })

  it('shows error on network error', async () => {
    server.use(http.get('/health', () => HttpResponse.error()))

    render(<App />)

    expect(await screen.findByText(errorText)).toBeInTheDocument()
  })

  it('shows error after 5 s timeout', async () => {
    vi.useFakeTimers({ toFake: ['setTimeout', 'clearTimeout'] })
    server.use(http.get('/health', () => delay('infinite')))
    render(<App />)

    await act(() => vi.advanceTimersByTimeAsync(HEALTH_TIMEOUT_MS - 1))
    expect(screen.getByRole('status')).toHaveTextContent('API durumu kontrol ediliyor…')
    await act(() => vi.advanceTimersByTimeAsync(1))

    expect(screen.getByRole('status')).toHaveTextContent(errorText)
  })

  it('uses role=status for the API state', async () => {
    server.use(http.get('/health', () => HttpResponse.text('Healthy')))

    render(<App />)

    // The status element exists from the first render (loading); wait for its text to change.
    await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('API çalışıyor'))
  })
})
