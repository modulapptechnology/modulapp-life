import { createSessionStore } from '../sessionStore/sessionStore';
import { createSessionApiClient } from './apiClient';

describe('createSessionApiClient', () => {
  it('usa el token del usuario de la sesión en las peticiones', async () => {
    const store = createSessionStore({ user: { id: '1', name: 'Ana', token: 'abc' } });
    const fetchMock = jest.spyOn(globalThis, 'fetch').mockResolvedValue(new Response('{}'));
    const client = createSessionApiClient('http://api', store);

    await client.get('/x');

    const headers = fetchMock.mock.calls[0][1]?.headers as Record<string, string>;
    expect(headers.Authorization).toBe('Bearer abc');
    fetchMock.mockRestore();
  });

  it('cierra la sesión cuando la API responde 401', async () => {
    const store = createSessionStore({ user: { id: '1', name: 'Ana', token: 'vencido' } });
    const fetchMock = jest
      .spyOn(globalThis, 'fetch')
      .mockResolvedValue(new Response(null, { status: 401 }));
    const client = createSessionApiClient('http://api', store);

    await client.get('/x');

    expect(store.getState().user).toBeNull();
    fetchMock.mockRestore();
  });
});
