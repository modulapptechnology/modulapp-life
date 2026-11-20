import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulSplash } from './ModulSplash';

describe('ModulSplash', () => {
  it('muestra el nombre de la marca', async () => {
    const props = { brandName: 'LIFE', message: 'Cargando…' };

    await renderWithTheme(<ModulSplash {...props} />);

    expect(screen.getByText('LIFE')).toBeTruthy();
  });

  it('se anuncia como ocupado con el mensaje como nombre', async () => {
    const props = { brandName: 'LIFE', message: 'Cargando…' };

    await renderWithTheme(<ModulSplash {...props} />);

    expect(screen.getByRole('progressbar', { name: 'Cargando…' })).toBeTruthy();
  });
});
