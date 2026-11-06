import { resolveButtonKind } from './resolveButtonKind';

describe('resolveButtonKind', () => {
  it('usa la variante primaria cuando se pide primaria', () => {
    const variante = 'primary';

    const tipo = resolveButtonKind(variante, false);

    expect(tipo).toBe('primary');
  });

  it('usa la variante secundaria cuando se pide secundaria', () => {
    const variante = 'secondary';

    const tipo = resolveButtonKind(variante, false);

    expect(tipo).toBe('secondary');
  });

  it('usa la variante destructiva cuando se pide destructiva', () => {
    const variante = 'danger';

    const tipo = resolveButtonKind(variante, false);

    expect(tipo).toBe('danger');
  });

  it('mantiene la prop danger heredada por encima de la variante', () => {
    const variante = 'primary';

    const tipo = resolveButtonKind(variante, true);

    expect(tipo).toBe('danger');
  });
});
