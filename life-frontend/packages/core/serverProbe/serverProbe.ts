import type { ApiClient } from '@life/api-client';

export const SERVER_PROBE_PATH = 'api/v1/house';
export const SERVER_PROBE_TIMEOUT_MS = 5000;

const UNREACHABLE_KINDS = ['networkError', 'timeout', 'serverError'];

// Cualquier respuesta distinta de un 5xx prueba que el servidor está disponible.
export function createServerProbe(
  client: ApiClient,
  path: string = SERVER_PROBE_PATH,
  timeoutMs: number = SERVER_PROBE_TIMEOUT_MS,
): () => Promise<boolean> {
  return async () => {
    const result = await client.get<unknown>(path, { timeoutMs });
    return !UNREACHABLE_KINDS.includes(result.kind);
  };
}
