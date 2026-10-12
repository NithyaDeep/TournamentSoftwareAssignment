import type { Camera } from '../types/camera';

/**
 * The only module that knows the API's URL shape. Components and hooks call the
 * functions here, so a change to an endpoint touches one file.
 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5141';

export async function fetchCameras(name: string, signal?: AbortSignal): Promise<Camera[]> {
  const url = new URL('/api/cameras', API_BASE_URL);

  const trimmed = name.trim();
  if (trimmed.length > 0) {
    url.searchParams.set('name', trimmed);
  }

  const response = await fetch(url, { signal, headers: { Accept: 'application/json' } });

  if (!response.ok) {
    throw new Error(`The API responded with ${response.status} ${response.statusText}.`);
  }

  return (await response.json()) as Camera[];
}
