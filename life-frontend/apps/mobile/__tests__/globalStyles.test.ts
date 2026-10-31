import { darkColors, lightColors } from '@life/theme';
import { createGlobalStyles } from '@/styles/global';

describe('createGlobalStyles', () => {
  it('la sombra flotante usa el color negro de la paleta', () => {
    const paletas = [lightColors, darkColors];

    const sombras = paletas.map((colores) => createGlobalStyles(colores).floating.shadowColor);

    expect(sombras).toEqual([lightColors.black, darkColors.black]);
  });
});
