import { createApiClient, CORRELATION_HEADER } from './createApiClient';

type FetchMock = jest.Mock<ReturnType<typeof fetch>, Parameters<typeof fetch>>;

function respond(status: number, body?: unknown): Response {
  return new Response(body === undefined ? null : JSON.stringify(body), { status });
}

function createFetch(...responses: Response[]): FetchMock {
  const fetchMock: FetchMock = jest.fn();
  responses.forEach((response) => fetchMock.mockResolvedValueOnce(response));
  return fetchMock;
}

function headersOf(fetchMock: FetchMock, call = 0): Record<string, string> {
  return { ...fetchMock.mock.calls[call][1]?.headers } as Record<string, string>;
}

describe('createApiClient', () => {
  it('rechaza una URL base vacía', () => {
    const config = { baseUrl: '  ' };

    const create = () => createApiClient(config);

    expect(create).toThrow('The API base URL is not configured.');
  });

  it('envía la petición a la URL base del ambiente', async () => {
    const fetchMock = createFetch(respond(200, { ok: true }));
    const client = createApiClient({ baseUrl: 'http://localhost:5041/', fetchFn: fetchMock });

    await client.get('/api/v1/house');

    expect(fetchMock.mock.calls[0][0]).toBe('http://localhost:5041/api/v1/house');
  });

  it('agrega el token de la sesión y el identificador de correlación', async () => {
    const fetchMock = createFetch(respond(200, {}));
    const client = createApiClient({
      baseUrl: 'http://api',
      fetchFn: fetchMock,
      getToken: () => 'abc',
      createCorrelationId: () => 'corr-1',
    });

    await client.get('/x');

    expect(headersOf(fetchMock)).toMatchObject({
      Authorization: 'Bearer abc',
      [CORRELATION_HEADER]: 'corr-1',
    });
  });

  it('no envía la cabecera de autorización cuando no hay sesión', async () => {
    const fetchMock = createFetch(respond(200, {}));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    await client.get('/x');

    expect(headersOf(fetchMock)).not.toHaveProperty('Authorization');
  });

  it('envía el cuerpo como JSON', async () => {
    const fetchMock = createFetch(respond(201, { id: 1 }));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    await client.post('/tasks', { body: { name: 'Tarea' } });

    expect(fetchMock.mock.calls[0][1]).toMatchObject({
      method: 'POST',
      body: '{"name":"Tarea"}',
    });
    expect(headersOf(fetchMock)['Content-Type']).toBe('application/json');
  });

  it('devuelve los datos tipados en una respuesta 2xx', async () => {
    const fetchMock = createFetch(respond(200, { name: 'Casa' }));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    const result = await client.get<{ name: string }>('/house');

    expect(result).toEqual({ kind: 'success', status: 200, data: { name: 'Casa' } });
  });

  it('entrega null cuando la respuesta 2xx no tiene cuerpo', async () => {
    const fetchMock = createFetch(respond(204));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    const result = await client.delete('/tasks/1');

    expect(result).toEqual({ kind: 'success', status: 204, data: null });
  });

  it.each([
    [400, 'badRequest'],
    [401, 'unauthorized'],
    [403, 'forbidden'],
    [404, 'notFound'],
    [409, 'conflict'],
    [500, 'serverError'],
    [503, 'serverError'],
    [418, 'unexpected'],
  ])('traduce el código %i al resultado %s', async (status, kind) => {
    const fetchMock = createFetch(respond(status, {}));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    const result = await client.get('/x');

    expect(result.kind).toBe(kind);
  });

  it('incluye los errores por campo en un 400', async () => {
    const fetchMock = createFetch(respond(400, { errors: { name: ['Obligatorio.'] } }));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    const result = await client.post('/tasks', { body: {} });

    expect(result).toEqual({
      kind: 'badRequest',
      status: 400,
      problem: { errors: { name: ['Obligatorio.'] } },
    });
  });

  it('no expone el cuerpo de un error 5xx', async () => {
    const fetchMock = createFetch(respond(500, { detail: 'SqlException en Persistence' }));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    const result = await client.get('/x');

    expect(result).toEqual({ kind: 'serverError', status: 500 });
  });

  it('cierra la sesión cuando un 401 no se puede renovar', async () => {
    const fetchMock = createFetch(respond(401));
    const onSessionExpired = jest.fn();
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock, onSessionExpired });

    const result = await client.get('/x');

    expect(result.kind).toBe('unauthorized');
    expect(onSessionExpired).toHaveBeenCalledTimes(1);
  });

  it('renueva la sesión y repite la petición una vez ante un 401', async () => {
    const fetchMock = createFetch(respond(401), respond(200, { ok: true }));
    const refreshSession = jest.fn().mockResolvedValue(true);
    const onSessionExpired = jest.fn();
    const client = createApiClient({
      baseUrl: 'http://api',
      fetchFn: fetchMock,
      refreshSession,
      onSessionExpired,
    });

    const result = await client.get('/x');

    expect(result.kind).toBe('success');
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(onSessionExpired).not.toHaveBeenCalled();
  });

  it('cierra la sesión cuando la petición repetida vuelve a recibir 401', async () => {
    const fetchMock = createFetch(respond(401), respond(401));
    const onSessionExpired = jest.fn();
    const client = createApiClient({
      baseUrl: 'http://api',
      fetchFn: fetchMock,
      refreshSession: () => Promise.resolve(true),
      onSessionExpired,
    });

    const result = await client.get('/x');

    expect(result.kind).toBe('unauthorized');
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(onSessionExpired).toHaveBeenCalledTimes(1);
  });

  it('cierra la sesión cuando la renovación falla', async () => {
    const fetchMock = createFetch(respond(401));
    const onSessionExpired = jest.fn();
    const client = createApiClient({
      baseUrl: 'http://api',
      fetchFn: fetchMock,
      refreshSession: () => Promise.reject(new Error('sin red')),
      onSessionExpired,
    });

    await client.get('/x');

    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(onSessionExpired).toHaveBeenCalledTimes(1);
  });

  it('comparte una sola renovación entre peticiones simultáneas con 401', async () => {
    const fetchMock = createFetch(respond(401), respond(401), respond(200, {}), respond(200, {}));
    const refreshSession = jest.fn().mockResolvedValue(true);
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock, refreshSession });

    await Promise.all([client.get('/a'), client.get('/b')]);

    expect(refreshSession).toHaveBeenCalledTimes(1);
  });

  it('devuelve un error de red controlado cuando fetch falla', async () => {
    const fetchMock: FetchMock = jest.fn().mockRejectedValue(new TypeError('Failed to fetch'));
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    const result = await client.get('/x');

    expect(result).toEqual({ kind: 'networkError' });
  });

  it('cancela la petición y devuelve timeout al superar el tiempo de espera', async () => {
    const fetchMock: FetchMock = jest.fn(
      (_url, init) =>
        new Promise((_resolve, reject) => {
          init?.signal?.addEventListener('abort', () => reject(new Error('aborted')));
        }),
    );
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock, timeoutMs: 10 });

    const result = await client.get('/x');

    expect(result).toEqual({ kind: 'timeout' });
  });

  it('devuelve cancelled cuando quien llama cancela la petición', async () => {
    const fetchMock: FetchMock = jest.fn(
      (_url, init) =>
        new Promise((_resolve, reject) => {
          init?.signal?.addEventListener('abort', () => reject(new Error('aborted')));
        }),
    );
    const controller = new AbortController();
    const client = createApiClient({ baseUrl: 'http://api', fetchFn: fetchMock });

    const pending = client.get('/x', { signal: controller.signal });
    controller.abort();

    expect(await pending).toEqual({ kind: 'cancelled' });
  });
});
