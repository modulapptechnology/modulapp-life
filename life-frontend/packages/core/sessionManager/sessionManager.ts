import type { ApiClient } from '@life/api-client';
import type { ApiResult, AuthResponse, LoginCredentials, SessionTokens } from '@life/types';
import type { SessionStore } from '../sessionStore/sessionStore';
import type { SessionTokenStorage } from '../sessionTokenStorage/sessionTokenStorage';
import { readUserFromAccessToken } from './accessTokenUser';

export const AUTH_ROUTES = {
  login: '/api/v1/auth/login',
  refresh: '/api/v1/auth/refresh',
  logout: '/api/v1/auth/logout',
} as const;

export type RefreshTokenDelivery = 'body' | 'cookie';

export type SessionManagerConfig = {
  client: ApiClient;
  storage: SessionTokenStorage;
  store: SessionStore;
  delivery: RefreshTokenDelivery;
  now?: () => number;
};

export type SessionManager = {
  signIn: (credentials: LoginCredentials) => Promise<ApiResult<AuthResponse>>;
  restore: () => Promise<boolean>;
  refresh: () => Promise<boolean>;
  signOut: () => Promise<void>;
  expire: () => Promise<void>;
};

export const DELIVERY_HEADER = 'X-Token-Delivery';

export function createSessionManager(config: SessionManagerConfig): SessionManager {
  const { client, storage, store, delivery } = config;
  const now = config.now ?? Date.now;
  const headers: Record<string, string> =
    delivery === 'cookie' ? { [DELIVERY_HEADER]: 'cookie' } : {};
  let refreshing: Promise<boolean> | null = null;

  function toTokens(response: AuthResponse): SessionTokens {
    return {
      accessToken: response.accessToken,
      accessTokenExpiresAt: response.expiresAt,
      refreshToken: delivery === 'cookie' ? null : response.refreshToken,
      refreshTokenExpiresAt: response.refreshTokenExpiresAt,
    };
  }

  async function start(tokens: SessionTokens): Promise<boolean> {
    const user = readUserFromAccessToken(tokens.accessToken);
    if (!user) {
      await storage.clear();
      return false;
    }
    await storage.save(tokens);
    store.signIn(user);
    return true;
  }

  async function loadStored(): Promise<SessionTokens | null> {
    try {
      return await storage.load();
    } catch {
      return null;
    }
  }

  async function doRefresh(): Promise<boolean> {
    const stored = await loadStored();
    if (delivery === 'body' && !stored?.refreshToken) {
      return false;
    }
    const result = await client.post<AuthResponse>(AUTH_ROUTES.refresh, {
      body: delivery === 'body' ? { refreshToken: stored?.refreshToken } : undefined,
      headers,
    });
    if (result.kind === 'success') {
      return start(toTokens(result.data));
    }
    if (result.kind === 'unauthorized' || result.kind === 'badRequest') {
      await storage.clear();
    }
    return false;
  }

  function refresh(): Promise<boolean> {
    if (!refreshing) {
      refreshing = doRefresh().finally(() => {
        refreshing = null;
      });
    }
    return refreshing;
  }

  return {
    async signIn(credentials) {
      const result = await client.post<AuthResponse>(AUTH_ROUTES.login, {
        body: credentials,
        headers,
      });
      if (result.kind === 'success' && !(await start(toTokens(result.data)))) {
        return { kind: 'unexpected', status: 200 };
      }
      return result;
    },

    async restore() {
      const stored = await loadStored();
      if (stored && Date.parse(stored.accessTokenExpiresAt) > now()) {
        const user = readUserFromAccessToken(stored.accessToken);
        if (user) {
          store.signIn(user);
          return true;
        }
      }
      return delivery === 'cookie' || stored?.refreshToken ? await refresh() : false;
    },

    refresh,

    async signOut() {
      const stored = await loadStored();
      await storage.clear();
      store.signOut();
      await client.post(AUTH_ROUTES.logout, {
        body: stored?.refreshToken ? { refreshToken: stored.refreshToken } : undefined,
        headers,
      });
    },

    async expire() {
      await storage.clear();
      store.expireSession();
    },
  };
}
