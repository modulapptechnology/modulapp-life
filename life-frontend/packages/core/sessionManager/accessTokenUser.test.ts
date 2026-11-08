import { readUserFromAccessToken } from './accessTokenUser';
import { createFakeAccessToken } from './fakeAccessToken';

describe('readUserFromAccessToken', () => {
  it('lee el identificador y el correo de los claims', () => {
    const token = createFakeAccessToken({ sub: 'u-1', email: 'ana@life.test' });

    const user = readUserFromAccessToken(token);

    expect(user).toEqual({ id: 'u-1', name: 'ana@life.test', token });
  });

  it('devuelve null cuando falta un claim', () => {
    const token = createFakeAccessToken({ sub: 'u-1' });

    const user = readUserFromAccessToken(token);

    expect(user).toBeNull();
  });

  it('devuelve null cuando el token no es legible', () => {
    const token = 'no-es-un-token';

    const user = readUserFromAccessToken(token);

    expect(user).toBeNull();
  });
});
