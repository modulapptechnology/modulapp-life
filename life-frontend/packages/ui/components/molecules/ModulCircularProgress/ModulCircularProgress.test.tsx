import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulCircularProgress } from './ModulCircularProgress';

describe('ModulCircularProgress', () => {
  it('muestra el porcentaje redondeado y la etiqueta', async () => {
    const valor = 49.6;

    await renderWithTheme(<ModulCircularProgress value={valor} label="Avance" />);

    expect(screen.getByText('50%')).toBeOnTheScreen();
    expect(screen.getByText('Avance')).toBeOnTheScreen();
  });

  it('limita el valor al 100% cuando lo supera', async () => {
    const valor = 150;

    await renderWithTheme(<ModulCircularProgress value={valor} label="Avance" />);

    expect(screen.getByLabelText('Avance').props.accessibilityValue.now).toBe(100);
  });

  it('muestra 0% cuando el valor es negativo', async () => {
    const valor = -5;

    await renderWithTheme(<ModulCircularProgress value={valor} label="Avance" />);

    expect(screen.getByText('0%')).toBeOnTheScreen();
  });
});
