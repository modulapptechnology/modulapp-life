import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulProgress } from './ModulProgress';

describe('ModulProgress', () => {
  it('limita el valor al 100% cuando lo supera', async () => {
    const valor = 250;

    await renderWithTheme(<ModulProgress value={valor} accessibilityLabel="Avance" />);

    expect(screen.getByLabelText('Avance').props.accessibilityValue).toEqual({
      min: 0,
      max: 100,
      now: 100,
    });
  });

  it('limita el valor al 0% cuando es negativo', async () => {
    const valor = -10;

    await renderWithTheme(<ModulProgress value={valor} accessibilityLabel="Avance" />);

    expect(screen.getByLabelText('Avance').props.accessibilityValue.now).toBe(0);
  });

  it('conserva el valor cuando está entre 0 y 100', async () => {
    const valor = 40;

    await renderWithTheme(<ModulProgress value={valor} accessibilityLabel="Avance" />);

    expect(screen.getByLabelText('Avance').props.accessibilityValue.now).toBe(40);
  });
});
