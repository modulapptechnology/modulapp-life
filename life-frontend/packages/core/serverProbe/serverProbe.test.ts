import type { ApiClient } from '@life/api-client';
import { createServerProbe, SERVER_PROBE_PATH } from './serverProbe';

function clientReturning(result: unknown): ApiClient {
  return { get: jest.fn().mockResolvedValue(result) } as unknown as ApiClient;
}

describe('createServerProbe', () => {
  it('is reachable when the server answers successfully', async () => {
    const client = clientReturning({ kind: 'success', status: 200, data: { status: 'ok' } });

    const reachable = await createServerProbe(client)();

    expect(reachable).toBe(true);
  });

  it('is reachable when the server answers with a client error', async () => {
    const client = clientReturning({ kind: 'notFound', status: 404 });

    const reachable = await createServerProbe(client)();

    expect(reachable).toBe(true);
  });

  it.each(['networkError', 'timeout', 'serverError'])('is unreachable on %s', async (kind) => {
    const client = clientReturning({ kind });

    const reachable = await createServerProbe(client)();

    expect(reachable).toBe(false);
  });

  it('asks the health path with a short timeout', async () => {
    const client = clientReturning({ kind: 'success', status: 200, data: null });

    await createServerProbe(client, SERVER_PROBE_PATH, 1234)();

    expect(client.get).toHaveBeenCalledWith(SERVER_PROBE_PATH, { timeoutMs: 1234 });
  });
});
