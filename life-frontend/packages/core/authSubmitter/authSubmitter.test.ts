import type { ApiResult } from '@life/types';
import { createAuthSubmitter } from './authSubmitter';

const sinErrores = () => ({});

describe('createAuthSubmitter', () => {
  it('no envía mientras la validación del cliente falla', async () => {
    const send = jest.fn();
    const enviar = createAuthSubmitter({
      validate: () => ({ email: 'auth.validation.emailRequired' }),
      send,
    });

    const resultado = await enviar({});

    expect(send).not.toHaveBeenCalled();
    expect(resultado).toEqual({
      status: 'invalid',
      errors: { email: 'auth.validation.emailRequired' },
    });
  });

  it('ignora un segundo envío mientras el primero se procesa', async () => {
    let terminar: (r: ApiResult<void>) => void = () => undefined;
    const send = jest.fn(() => new Promise<ApiResult<void>>((resolve) => (terminar = resolve)));
    const enviar = createAuthSubmitter({ validate: sinErrores, send });

    const primero = enviar({});
    const segundo = await enviar({});
    terminar({ kind: 'success', status: 200, data: undefined });

    expect(segundo).toEqual({ status: 'busy' });
    expect(await primero).toEqual({ status: 'done' });
    expect(send).toHaveBeenCalledTimes(1);
  });

  it('devuelve una clave genérica cuando el servidor rechaza y permite reintentar', async () => {
    const send = jest.fn().mockResolvedValue({ kind: 'unauthorized', status: 401 });
    const enviar = createAuthSubmitter({ validate: sinErrores, send });

    const intentos = [await enviar({}), await enviar({})];

    expect(intentos).toEqual([
      { status: 'rejected', errorKey: 'auth.error.credentials' },
      { status: 'rejected', errorKey: 'auth.error.credentials' },
    ]);
    expect(send).toHaveBeenCalledTimes(2);
  });
});
