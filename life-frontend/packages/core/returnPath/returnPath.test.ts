import { resolveReturnPath } from './returnPath';

describe('resolveReturnPath', () => {
  it('acepta una ruta interna con parámetros', () => {
    const value = '/profile?tab=1';

    const result = resolveReturnPath(value);

    expect(result).toBe('/profile?tab=1');
  });

  it.each(['//evil.com', 'https://evil.com', '', undefined, null])(
    'rechaza %p y usa la ruta por defecto',
    (value) => {
      const fallback = '/';

      const result = resolveReturnPath(value, fallback);

      expect(result).toBe('/');
    },
  );
});
