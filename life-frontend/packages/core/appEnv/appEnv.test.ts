import { readAppEnv } from './appEnv';

describe('readAppEnv', () => {
  it('devuelve el ambiente y la URL de la API cuando ambas variables son válidas', () => {
    const source = { EXPO_PUBLIC_APP_ENV: 'test', EXPO_PUBLIC_API_URL: 'http://localhost:5042' };

    const env = readAppEnv(source);

    expect(env).toEqual({ appEnv: 'test', apiUrl: 'http://localhost:5042' });
  });

  it('falla nombrando cada variable obligatoria que falta', () => {
    const source = {};

    const leer = () => readAppEnv(source);

    expect(leer).toThrow(/EXPO_PUBLIC_APP_ENV es obligatoria/);
    expect(leer).toThrow(/EXPO_PUBLIC_API_URL es obligatoria/);
  });

  it('trata una variable con solo espacios como faltante', () => {
    const source = { EXPO_PUBLIC_APP_ENV: 'development', EXPO_PUBLIC_API_URL: '   ' };

    const leer = () => readAppEnv(source);

    expect(leer).toThrow(/EXPO_PUBLIC_API_URL es obligatoria/);
  });

  it('rechaza un ambiente que no es development, test ni production', () => {
    const source = { EXPO_PUBLIC_APP_ENV: 'staging', EXPO_PUBLIC_API_URL: 'http://localhost:5041' };

    const leer = () => readAppEnv(source);

    expect(leer).toThrow(/EXPO_PUBLIC_APP_ENV tiene el valor "staging"/);
  });

  it('rechaza una URL de API que no es http ni https', () => {
    const source = { EXPO_PUBLIC_APP_ENV: 'production', EXPO_PUBLIC_API_URL: 'api.life.com' };

    const leer = () => readAppEnv(source);

    expect(leer).toThrow(/EXPO_PUBLIC_API_URL tiene el valor "api.life.com"/);
  });
});
