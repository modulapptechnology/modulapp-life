import { createViewStyles, lightColors } from '@life/theme';

describe('estilos de capas y listas', () => {
  it('pinta la capa oscura con el negro del tema', () => {
    const estilos = createViewStyles(lightColors);

    const fondo = estilos.overlay.backgroundColor;

    expect(fondo).toBe(lightColors.black);
  });

  it('pinta el separador de lista con el color de borde del tema', () => {
    const estilos = createViewStyles(lightColors);

    const color = estilos.divider.backgroundColor;

    expect(color).toBe(lightColors.border);
  });
});
