import type { ApiClient } from '@life/api-client';
import {
  FORGOT_PASSWORD_ROUTE,
  RESET_PASSWORD_ROUTE,
  REGISTER_ROUTE,
  VERIFY_EMAIL_ROUTE,
  createAuthService,
} from './authService';

function createClient() {
  const ok = { kind: 'success', status: 200, data: undefined };
  const post = jest.fn().mockResolvedValue(ok);
  return { client: { post } as unknown as ApiClient, post };
}

describe('createAuthService', () => {
  it('registra con el correo sin espacios y la contraseña tal cual', async () => {
    const { client, post } = createClient();
    const servicio = createAuthService(client);

    await servicio.register({ email: ' ana@correo.com ', password: 'ClaveSegura1' });

    expect(post).toHaveBeenCalledWith(REGISTER_ROUTE, {
      body: { email: 'ana@correo.com', password: 'ClaveSegura1' },
    });
  });

  it('confirma el correo enviando el correo sin espacios y el código', async () => {
    const { client, post } = createClient();
    const servicio = createAuthService(client);

    await servicio.verifyEmail(' ana@correo.com ', ' 123456 ');

    expect(post).toHaveBeenCalledWith(VERIFY_EMAIL_ROUTE, {
      body: { email: 'ana@correo.com', code: '123456' },
    });
  });

  it('solicita el enlace de recuperación con el correo sin espacios', async () => {
    const { client, post } = createClient();
    const servicio = createAuthService(client);

    await servicio.forgotPassword(' ana@correo.com ');

    expect(post).toHaveBeenCalledWith(FORGOT_PASSWORD_ROUTE, { body: { email: 'ana@correo.com' } });
  });

  it('envía el token y la nueva contraseña al restablecer', async () => {
    const { client, post } = createClient();
    const servicio = createAuthService(client);

    await servicio.resetPassword('token-123', 'ClaveSegura1');

    expect(post).toHaveBeenCalledWith(RESET_PASSWORD_ROUTE, {
      body: { token: 'token-123', password: 'ClaveSegura1' },
    });
  });
});
