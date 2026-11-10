import type { ApiResult } from '@life/types';
import { authErrorKey } from './authErrors';

describe('authErrorKey', () => {
  it('no devuelve error cuando la operación fue exitosa', () => {
    const resultado: ApiResult<void> = { kind: 'success', status: 204, data: undefined };

    const clave = authErrorKey(resultado);

    expect(clave).toBeUndefined();
  });

  it('usa mensajes genéricos que no revelan si el correo existe', () => {
    const resultados: ApiResult<void>[] = [
      { kind: 'unauthorized', status: 401 },
      { kind: 'conflict', status: 409, problem: { title: 'El correo ya está registrado' } },
    ];

    const claves = resultados.map((resultado) => authErrorKey(resultado));

    expect(claves).toEqual(['auth.error.credentials', 'auth.error.register']);
  });

  it('distingue datos inválidos, falta de conexión y error inesperado', () => {
    const resultados: ApiResult<void>[] = [
      { kind: 'badRequest', status: 400, problem: {} },
      { kind: 'networkError' },
      { kind: 'serverError', status: 500 },
    ];

    const claves = resultados.map((resultado) => authErrorKey(resultado));

    expect(claves).toEqual(['auth.error.invalid', 'auth.error.network', 'auth.error.generic']);
  });
});
