import { resolveTextRole } from './resolveTextRole';

describe('resolveTextRole', () => {
  it('devuelve el rol de enlace cuando el texto es un enlace', () => {
    const variante = 'body';

    const rol = resolveTextRole(variante, true);

    expect(rol).toBe('link');
  });

  it('devuelve el rol de encabezado en las variantes de título', () => {
    const variantes = ['screenTitle', 'sectionTitle', 'headerTitle'] as const;

    const roles = variantes.map((variante) => resolveTextRole(variante, false));

    expect(roles).toEqual(['header', 'header', 'header']);
  });

  it('no devuelve rol en las variantes de texto normal', () => {
    const variantes = ['body', 'bodySecondary', 'label', 'button'] as const;

    const roles = variantes.map((variante) => resolveTextRole(variante, false));

    expect(roles).toEqual([undefined, undefined, undefined, undefined]);
  });

  it('prioriza el rol de enlace sobre el de encabezado', () => {
    const variante = 'screenTitle';

    const rol = resolveTextRole(variante, true);

    expect(rol).toBe('link');
  });
});
