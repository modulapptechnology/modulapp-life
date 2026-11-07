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
});
