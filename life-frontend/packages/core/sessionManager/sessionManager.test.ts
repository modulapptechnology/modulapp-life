import type { ApiClient } from '@life/api-client';
import type { ApiResult, AuthResponse, SessionTokens } from '@life/types';
import { createSessionStore } from '../sessionStore/sessionStore';
import { createMemorySessionStorage } from '../sessionTokenStorage/sessionTokenStorage';
import { createFakeAccessToken } from './fakeAccessToken';
import { AUTH_ROUTES, createSessionManager } from './sessionManager';
import type { RefreshTokenDelivery } from './sessionManager';

const NOW = Date.parse('2026-11-07T12:00:00Z');

function createToken(): string {
  return createFakeAccessToken({ sub: 'u-1', email: 'ana@life.test' });
}

const authResponse: AuthResponse = {
  accessToken: createToken(),
  tokenType: 'Bearer',
  expiresAt: '2026-11-07T12:15:00Z',
  refreshToken: 'refresh-2',
  refreshTokenExpiresAt: '2027-01-06T12:00:00Z',
};

const storedTokens: SessionTokens = {
  accessToken: createToken(),
  accessTokenExpiresAt: '2026-11-07T11:00:00Z',
  refreshToken: 'refresh-1',
  refreshTokenExpiresAt: '2027-01-06T12:00:00Z',
};

function setup(delivery: RefreshTokenDelivery = 'body', post?: jest.Mock) {
  const success: ApiResult<AuthResponse> = { kind: 'success', status: 200, data: authResponse };
  const postMock = post ?? jest.fn().mockResolvedValue(success);
  const client = { post: postMock } as unknown as ApiClient;
  const storage = createMemorySessionStorage();
  const store = createSessionStore();
  const manager = createSessionManager({ client, storage, store, delivery, now: () => NOW });
  return { manager, storage, store, post: postMock };
}

describe('createSessionManager', () => {
  it('guarda los tokens y abre la sesión al iniciar sesión', async () => {
    const { manager, storage, store } = setup();

    await manager.signIn({ email: 'ana@life.test', password: 'Abcdefgh12' });

    expect([(await storage.load())?.refreshToken, store.getState().user?.id]).toEqual([
      'refresh-2',
      'u-1',
    ]);
  });

  it('no guarda el token de renovación cuando el servidor lo entrega por cookie', async () => {
    const { manager, storage, post } = setup('cookie');

    await manager.signIn({ email: 'ana@life.test', password: 'Abcdefgh12' });

    expect([
      (await storage.load())?.refreshToken,
      post.mock.calls[0][1].headers['X-Token-Delivery'],
    ]).toEqual([null, 'cookie']);
  });

  it('no guarda nada cuando el inicio de sesión es rechazado', async () => {
    const post = jest.fn().mockResolvedValue({ kind: 'unauthorized', status: 401 });
    const { manager, storage } = setup('body', post);

    const result = await manager.signIn({ email: 'ana@life.test', password: 'mala' });

    expect([result.kind, await storage.load()]).toEqual(['unauthorized', null]);
  });

  it('reabre la sesión al iniciar con un token de acceso vigente', async () => {
    const { manager, storage, store, post } = setup();
    await storage.save({ ...storedTokens, accessTokenExpiresAt: '2026-11-07T12:10:00Z' });

    const restored = await manager.restore();

    expect([restored, store.getState().user?.id, post.mock.calls.length]).toEqual([true, 'u-1', 0]);
  });

  it('renueva la sesión al iniciar cuando el token de acceso venció', async () => {
    const { manager, storage, store, post } = setup();
    await storage.save(storedTokens);

    const restored = await manager.restore();

    expect([restored, post.mock.calls[0][1].body, store.getState().user?.id]).toEqual([
      true,
      { refreshToken: 'refresh-1' },
      'u-1',
    ]);
  });

  it('renueva con la cookie al iniciar la web aunque no haya tokens en memoria', async () => {
    const { manager, post } = setup('cookie');

    const restored = await manager.restore();

    expect([restored, post.mock.calls[0][0]]).toEqual([true, AUTH_ROUTES.refresh]);
  });

  it('no abre sesión al iniciar sin tokens guardados', async () => {
    const { manager, store, post } = setup();

    const restored = await manager.restore();

    expect([restored, store.getState().user, post.mock.calls.length]).toEqual([false, null, 0]);
  });

  it('borra los tokens cuando el servidor rechaza la renovación', async () => {
    const post = jest.fn().mockResolvedValue({ kind: 'unauthorized', status: 401 });
    const { manager, storage } = setup('body', post);
    await storage.save(storedTokens);

    const restored = await manager.restore();

    expect([restored, await storage.load()]).toEqual([false, null]);
  });

  it('conserva los tokens cuando la renovación falla por falta de conexión', async () => {
    const post = jest.fn().mockResolvedValue({ kind: 'networkError' });
    const { manager, storage } = setup('body', post);
    await storage.save(storedTokens);

    const renewed = await manager.refresh();

    expect([renewed, (await storage.load())?.refreshToken]).toEqual([false, 'refresh-1']);
  });

  it('comparte una sola renovación entre llamadas simultáneas', async () => {
    const { manager, storage, post } = setup();
    await storage.save(storedTokens);

    await Promise.all([manager.refresh(), manager.refresh()]);

    expect(post).toHaveBeenCalledTimes(1);
  });

  it('borra los tokens, cierra la sesión y avisa al servidor al cerrar sesión', async () => {
    const { manager, storage, store, post } = setup();
    await manager.signIn({ email: 'ana@life.test', password: 'Abcdefgh12' });

    await manager.signOut();

    expect([
      await storage.load(),
      store.getState().user,
      post.mock.calls[1][0],
      post.mock.calls[1][1].body,
    ]).toEqual([null, null, AUTH_ROUTES.logout, { refreshToken: 'refresh-2' }]);
  });

  it('conserva los cambios pendientes cuando el servidor rechaza la sesión', async () => {
    const { manager, storage, store } = setup();
    await manager.signIn({ email: 'ana@life.test', password: 'Abcdefgh12' });
    store.setPendingOperations(3);

    await manager.expire();

    expect([await storage.load(), store.getState()]).toEqual([
      null,
      { user: null, pendingOperations: 3, restoring: false },
    ]);
  });
});

describe('createSessionManager restore y estado de carga', () => {
  function setupRestoring(post?: jest.Mock) {
    const postMock = post ?? jest.fn();
    const client = { post: postMock } as unknown as ApiClient;
    const storage = createMemorySessionStorage();
    const store = createSessionStore({ restoring: true });
    const manager = createSessionManager({
      client,
      storage,
      store,
      delivery: 'body',
      now: () => NOW,
    });
    return { manager, storage, store };
  }

  it('termina la carga sin usuario cuando no hay sesión guardada', async () => {
    const { manager, store } = setupRestoring();

    await manager.restore();

    expect([store.getState().user, store.getState().restoring]).toEqual([null, false]);
  });

  it('termina la carga con usuario cuando hay un token de acceso vigente', async () => {
    const { manager, storage, store } = setupRestoring();
    await storage.save({ ...storedTokens, accessTokenExpiresAt: '2026-11-07T12:10:00Z' });

    await manager.restore();

    expect([store.getState().user?.id, store.getState().restoring]).toEqual(['u-1', false]);
  });
});
