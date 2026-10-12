import { useEffect, useState } from 'react';
import { fetchCameras } from '../api/camerasApi';
import type { Camera } from '../types/camera';

const SEARCH_DEBOUNCE_MS = 250;

export interface CamerasState {
  cameras: Camera[];
  isLoading: boolean;
  error: string | null;
}

/**
 * Loads cameras for the given search term, re-fetching when it changes.
 *
 * All data fetching and request state lives here so that the components stay
 * presentational. Keystrokes are debounced and in-flight requests are aborted, which
 * stops a slow earlier response from overwriting a newer one.
 */
export function useCameras(searchTerm: string): CamerasState {
  const [state, setState] = useState<CamerasState>({
    cameras: [],
    isLoading: true,
    error: null,
  });

  useEffect(() => {
    const controller = new AbortController();

    setState((previous) => ({ ...previous, isLoading: true, error: null }));

    const timer = window.setTimeout(
      () => {
        fetchCameras(searchTerm, controller.signal)
          .then((cameras) => setState({ cameras, isLoading: false, error: null }))
          .catch((error: unknown) => {
            if (controller.signal.aborted) {
              return;
            }

            setState({ cameras: [], isLoading: false, error: toMessage(error) });
          });
      },
      searchTerm.length > 0 ? SEARCH_DEBOUNCE_MS : 0,
    );

    return () => {
      window.clearTimeout(timer);
      controller.abort();
    };
  }, [searchTerm]);

  return state;
}

function toMessage(error: unknown): string {
  if (error instanceof TypeError) {
    // fetch rejects with a TypeError when the server cannot be reached at all.
    return 'The API could not be reached.';
  }

  return error instanceof Error ? error.message : 'The cameras could not be loaded.';
}
