import { darkColors, lightColors } from '../colors/colors';
import { layout } from '../dimension/dimension';
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

describe('estilos de la barra lateral y el panel deslizable', () => {
  it('da a la barra lateral el ancho del layout', () => {
    const estilos = createViewStyles(lightColors);

    const ancho = estilos.sidebar.width;

    expect(ancho).toBe(layout.sidebarWidth);
  });

  it('da al panel deslizable el ancho del layout y toda la altura', () => {
    const estilos = createViewStyles(lightColors);

    const panel = estilos.drawer;

    expect(panel).toEqual({ width: layout.drawerWidth, height: '100%' });
  });
});

describe('estilos de posición', () => {
  it('llena todo el contenedor con la posición absoluta pegada a los cuatro bordes', () => {
    const estilos = createViewStyles(lightColors);

    const relleno = estilos.absoluteFill;

    expect(relleno).toEqual({ position: 'absolute', top: 0, right: 0, bottom: 0, left: 0 });
  });
});
