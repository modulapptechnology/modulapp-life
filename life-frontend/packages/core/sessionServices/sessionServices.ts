import { createApiClient } from '@life/api-client';
import type { ApiClient } from '@life/api-client';
import { createSessionApiClient } from '../apiClient/apiClient';
import { createSessionManager } from '../sessionManager/sessionManager';
import type { RefreshTokenDelivery, SessionManager } from '../sessionManager/sessionManager';
import { session } from '../sessionStore/sessionStore';
import type { SessionStore } from '../sessionStore/sessionStore';
import type { SessionTokenStorage } from '../sessionTokenStorage/sessionTokenStorage';

export type SessionServicesConfig = {
  baseUrl: string;
  storage: SessionTokenStorage;
  delivery: RefreshTokenDelivery;
  store?: SessionStore;
  fetchFn?: typeof fetch;
};

export type SessionServices = {
  manager: SessionManager;
  apiClient: ApiClient;
};

export function createSessionServices(config: SessionServicesConfig): SessionServices {
  const store = config.store ?? session;
  const manager = createSessionManager({
    client: createApiClient({ baseUrl: config.baseUrl, fetchFn: config.fetchFn }),
    storage: config.storage,
    store,
    delivery: config.delivery,
  });
  const apiClient = createSessionApiClient(config.baseUrl, store, {
    refreshSession: manager.refresh,
    onSessionExpired: () => {
      void manager.expire();
    },
    fetchFn: config.fetchFn,
  });
  return { manager, apiClient };
}
