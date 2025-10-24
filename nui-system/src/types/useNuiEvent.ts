import { useEffect } from 'react'

export function useNuiEvent<T = any>(eventName: string, handler: (data: T) => void) {
  useEffect(() => {
    const eventListener = (event: MessageEvent) => {
      const { action, ...data } = event.data

      if (action === eventName) {
        handler(data as T)
      }
    }

    window.addEventListener('message', eventListener)

    return () => {
      window.removeEventListener('message', eventListener)
    }
  }, [eventName, handler])
}