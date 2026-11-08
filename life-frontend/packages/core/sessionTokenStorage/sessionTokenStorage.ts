import type { SessionTokens } from '@life/types';

export const SESSION_STORAGE_KEY = 'life.session';

export type SecureKeyValueStore = {
  get: (key: string) => Promise<string | null>;
  set: (key: string, value: string) => Promise<void>;
  remove: (key: string) => Promise<void>;
};

export type SessionTokenStorage = {
  load: () => Promise<SessionTokens | null>;
  save: (tokens: SessionTokens) => Promise<void>;
  clear: () => Promise<void>;
};

export function isSessionTokens(value: unknown): value is SessionTokens {
  if (typeof value !== 'object' || value === null) {
    return false;
  }
  const candidate = value as Record<string, unknown>;
  return (
    typeof candidate.accessToken === 'string' &&
    typeof candidate.accessTokenExpiresAt === 'string' &&
    (typeof candidate.refreshToken === 'string' || candidate.refreshToken === null) &&
    typeof candidate.refreshTokenExpiresAt === 'string'
  );
}

function parseTokens(raw: string): SessionTokens | null {
  try {
    const parsed: unknown = JSON.parse(raw);
    return isSessionTokens(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

export function createSecureSessionStorage(
  store: SecureKeyValueStore,
  key: string = SESSION_STORAGE_KEY,
): SessionTokenStorage {
  return {
    async load() {
      const raw = await store.get(key);
      if (raw === null) {
        return null;
      }
      const parsed = parseTokens(raw);
      if (parsed) {
        return parsed;
      }
      await store.remove(key);
      return null;
    },
    save: (tokens) => store.set(key, JSON.stringify(tokens)),
    clear: () => store.remove(key),
  };
}

export function createMemorySessionStorage(): SessionTokenStorage {
  let current: SessionTokens | null = null;
  return {
    load: () => Promise.resolve(current),
    save: (tokens) => {
      current = tokens;
      return Promise.resolve();
    },
    clear: () => {
      current = null;
      return Promise.resolve();
    },
  };
}

export type NativeInvoke = (command: string, args?: Record<string, unknown>) => Promise<unknown>;

export function createInvokeKeyValueStore(invoke: NativeInvoke): SecureKeyValueStore {
  return {
    async get(key) {
      const value = await invoke('secure_get', { key });
      return typeof value === 'string' ? value : null;
    },
    async set(key, value) {
      await invoke('secure_set', { key, value });
    },
    async remove(key) {
      await invoke('secure_delete', { key });
    },
  };
}
