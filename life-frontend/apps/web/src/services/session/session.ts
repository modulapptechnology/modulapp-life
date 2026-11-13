import {
  createInvokeKeyValueStore,
  createMemorySessionStorage,
  createSecureSessionStorage,
  createSessionServices,
} from '@life/core';
import { invoke, isTauri } from '@tauri-apps/api/core';
import { env } from '../../config/env';

const desktop = isTauri();

export const { manager: sessionManager, apiClient } = createSessionServices({
  baseUrl: env.apiUrl,
  delivery: desktop ? 'body' : 'cookie',
  storage: desktop
    ? createSecureSessionStorage(createInvokeKeyValueStore(invoke))
    : createMemorySessionStorage(),
  fetchFn: desktop ? undefined : (input, init) => fetch(input, { ...init, credentials: 'include' }),
});
