import { createApiClient } from '@life/api-client';
import type { ApiClient } from '@life/api-client';
import { session } from '../sessionStore/sessionStore';
import type { SessionStore } from '../sessionStore/sessionStore';

export type SessionApiClientOptions = {
  refreshSession?: () => Promise<boolean>;
  onSessionExpired?: () => void;
  fetchFn?: typeof fetch;
};

export function createSessionApiClient(
  baseUrl: string,
  store: SessionStore = session,
  options: SessionApiClientOptions = {},
): ApiClient {
  return createApiClient({
    baseUrl,
    getToken: () => store.getState().user?.token,
    refreshSession: options.refreshSession,
    onSessionExpired: options.onSessionExpired ?? store.expireSession,
    fetchFn: options.fetchFn,
  });
}
