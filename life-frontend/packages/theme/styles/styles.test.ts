import { darkColors, lightColors } from '../colors/colors';
import { createTextStyles, createViewStyles } from './styles';

describe('estilos de los átomos', () => {
  it('toma los fondos de estado del color del tema claro y oscuro', () => {
    const claro = createViewStyles(lightColors);
    const oscuro = createViewStyles(darkColors);

    const fondos = [
      claro.bgSecondary.backgroundColor,
      claro.bgSuccess.backgroundColor,
      oscuro.bgWarning.backgroundColor,
      oscuro.bgInfo.backgroundColor,
    ];

    expect(fondos).toEqual([
      lightColors.secondary,
      lightColors.success,
      darkColors.warning,
      darkColors.info,
    ]);
  });

  it('toma los bordes primario y sutil del color del tema', () => {
    const estilos = createViewStyles(lightColors);

    const colores = [estilos.borderPrimary.borderColor, estilos.borderSubtle.borderColor];

    expect(colores).toEqual([lightColors.primary, lightColors.border]);
  });

  it('subraya el texto con el estilo underline', () => {
    const estilos = createTextStyles(lightColors);

    const decoracion = estilos.underline.textDecorationLine;

    expect(decoracion).toBe('underline');
  });
});

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
