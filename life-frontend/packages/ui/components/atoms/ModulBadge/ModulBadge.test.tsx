import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulBadge } from './ModulBadge';

describe('ModulBadge', () => {
  it('muestra el texto de la etiqueta', async () => {
    const etiqueta = 'Nuevo';

    await renderWithTheme(<ModulBadge label={etiqueta} />);

    expect(screen.getByText('Nuevo')).toBeOnTheScreen();
  });

  it('muestra una etiqueta numérica, incluido el cero', async () => {
    const etiqueta = 0;

    await renderWithTheme(<ModulBadge label={etiqueta} tone="info" />);

    expect(screen.getByText('0')).toBeOnTheScreen();
  });
});
