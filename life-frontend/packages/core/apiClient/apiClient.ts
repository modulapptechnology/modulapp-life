import { createApiClient } from '@life/api-client';
import type { ApiClient } from '@life/api-client';
import { session } from '../sessionStore/sessionStore';
import type { SessionStore } from '../sessionStore/sessionStore';

export function createSessionApiClient(baseUrl: string, store: SessionStore = session): ApiClient {
  return createApiClient({
    baseUrl,
    getToken: () => store.getState().user?.token,
    onSessionExpired: store.signOut,
  });
}
