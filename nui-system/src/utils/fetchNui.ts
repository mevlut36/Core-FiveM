/**
 * Fonction pour communiquer avec le client FiveM
 */
export async function fetchNui<T = any>(
  eventName: string,
  data?: any
): Promise<T> {
  const options = {
    method: 'post',
    headers: {
      'Content-Type': 'application/json; charset=UTF-8',
    },
    body: JSON.stringify(data),
  }

  if (import.meta.env.DEV) {
    console.log(`[DEV] NUI Event: ${eventName}`, data)
    return { ok: true } as T
  }

  const resourceName = (window as any).GetParentResourceName
    ? (window as any).GetParentResourceName()
    : 'nui-frame-app'

  const resp = await fetch(`https://${resourceName}/${eventName}`, options)

  return await resp.json()
}