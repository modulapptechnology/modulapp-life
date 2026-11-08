import type { SessionTokens } from '@life/types';
import {
  createInvokeKeyValueStore,
  createMemorySessionStorage,
  createSecureSessionStorage,
  SESSION_STORAGE_KEY,
} from './sessionTokenStorage';
import type { SecureKeyValueStore } from './sessionTokenStorage';

const tokens: SessionTokens = {
  accessToken: 'access',
  accessTokenExpiresAt: '2026-11-07T12:15:00Z',
  refreshToken: 'refresh',
  refreshTokenExpiresAt: '2027-01-06T12:00:00Z',
};

function createFakeStore(initial: Record<string, string> = {}) {
  const values = new Map(Object.entries(initial));
  const store: SecureKeyValueStore = {
    get: (key) => Promise.resolve(values.get(key) ?? null),
    set: (key, value) => {
      values.set(key, value);
      return Promise.resolve();
    },
    remove: (key) => {
      values.delete(key);
      return Promise.resolve();
    },
  };
  return { store, values };
}

describe('createSecureSessionStorage', () => {
  it('guarda los tokens en un solo valor del almacén seguro', async () => {
    const { store, values } = createFakeStore();
    const storage = createSecureSessionStorage(store);

    await storage.save(tokens);

    expect(JSON.parse(values.get(SESSION_STORAGE_KEY) ?? 'null')).toEqual(tokens);
  });

  it('devuelve los tokens guardados al leer', async () => {
    const { store } = createFakeStore({ [SESSION_STORAGE_KEY]: JSON.stringify(tokens) });
    const storage = createSecureSessionStorage(store);

    const loaded = await storage.load();

    expect(loaded).toEqual(tokens);
  });

  it('devuelve null cuando no hay nada guardado', async () => {
    const { store } = createFakeStore();
    const storage = createSecureSessionStorage(store);

    const loaded = await storage.load();

    expect(loaded).toBeNull();
  });

  it('descarta un valor ilegible o con otra forma', async () => {
    const { store, values } = createFakeStore({ [SESSION_STORAGE_KEY]: '{"accessToken":1}' });
    const storage = createSecureSessionStorage(store);

    const loaded = await storage.load();

    expect([loaded, values.has(SESSION_STORAGE_KEY)]).toEqual([null, false]);
  });

  it('elimina los tokens al limpiar', async () => {
    const { store, values } = createFakeStore({ [SESSION_STORAGE_KEY]: JSON.stringify(tokens) });
    const storage = createSecureSessionStorage(store);

    await storage.clear();

    expect(values.size).toBe(0);
  });
});

describe('createMemorySessionStorage', () => {
  it('conserva los tokens solo en memoria y los olvida al limpiar', async () => {
    const storage = createMemorySessionStorage();
    await storage.save(tokens);
    const saved = await storage.load();

    await storage.clear();

    expect([saved, await storage.load()]).toEqual([tokens, null]);
  });
});

describe('createInvokeKeyValueStore', () => {
  it('llama a los comandos nativos de escritorio con la clave y el valor', async () => {
    const invoke = jest.fn().mockResolvedValue(null);
    const store = createInvokeKeyValueStore(invoke);

    await store.set('k', 'v');

    expect(invoke).toHaveBeenCalledWith('secure_set', { key: 'k', value: 'v' });
  });

  it('devuelve null cuando el comando nativo no devuelve texto', async () => {
    const invoke = jest.fn().mockResolvedValue(null);
    const store = createInvokeKeyValueStore(invoke);

    const value = await store.get('k');

    expect(value).toBeNull();
  });
});
