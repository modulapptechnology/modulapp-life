import { createThemeColors, darkColors, lightColors } from './colors';

describe('createThemeColors', () => {
  it('con el acento Modul entrega los colores base del tema', () => {
    const colors = createThemeColors('light', 'modul');

    expect(colors).toEqual(lightColors);
  });

  it('cambia solo primary y secondary al elegir otro acento', () => {
    const colors = createThemeColors('dark', 'violet');

    expect(colors.primary).not.toBe(darkColors.primary);
    expect(colors.background).toBe(darkColors.background);
  });

  it('define los tokens de estado en el tema claro y en el oscuro', () => {
    const temas = [lightColors, darkColors];

    const tokens = temas.flatMap(({ hover, pressed, skeleton }) => [hover, pressed, skeleton]);

    expect(tokens.every(Boolean)).toBe(true);
    expect(lightColors.hover).not.toBe(darkColors.hover);
  });
});
