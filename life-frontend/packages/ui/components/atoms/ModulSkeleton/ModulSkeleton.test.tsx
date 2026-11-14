import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulSkeleton } from './ModulSkeleton';

describe('ModulSkeleton', () => {
  it('reserva el ancho y el alto indicados', async () => {
    const medidas = { width: 120, height: 24 };

    await renderWithTheme(<ModulSkeleton {...medidas} />);

    expect(screen.toJSON()).toMatchObject({ props: { style: medidas } });
  });

  it('es circular y toma el alto como lado', async () => {
    const forma = { circle: true, height: 40 };

    await renderWithTheme(<ModulSkeleton {...forma} />);

    expect(screen.toJSON()).toMatchObject({
      props: { style: { width: 40, height: 40 } },
    });
  });

  it('se oculta a las tecnologías de asistencia', async () => {
    const forma = {};

    await renderWithTheme(<ModulSkeleton {...forma} />);

    expect(screen.toJSON()).toMatchObject({ props: { accessibilityElementsHidden: true } });
  });
});
