import { useEffect, useState } from 'react'

type Status = 'loading' | 'healthy' | 'error'

export const HEALTH_TIMEOUT_MS = 5000

const messages: Record<Status, string> = {
  loading: 'API durumu kontrol ediliyor…',
  healthy: 'API çalışıyor',
  error: "API'ye şu anda ulaşılamıyor.",
}

export function ApiStatus() {
  const [status, setStatus] = useState<Status>('loading')

  useEffect(() => {
    // setTimeout + AbortController (not AbortSignal.timeout) so tests can drive the clock.
    const controller = new AbortController()
    const timer = setTimeout(() => controller.abort(), HEALTH_TIMEOUT_MS)
    let unmounted = false

    fetch('/health', { signal: controller.signal })
      .then((response) => {
        if (!unmounted) setStatus(response.ok ? 'healthy' : 'error')
      })
      .catch(() => {
        if (!unmounted) setStatus('error')
      })
      .finally(() => clearTimeout(timer))

    return () => {
      unmounted = true
      clearTimeout(timer)
      controller.abort()
    }
  }, [])

  return <p role="status">{messages[status]}</p>
}
