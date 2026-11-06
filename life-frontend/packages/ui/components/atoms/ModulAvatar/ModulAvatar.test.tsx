import { screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulAvatar } from './ModulAvatar';

describe('ModulAvatar', () => {
  it('muestra las iniciales del nombre cuando no hay imagen', async () => {
    const nombre = 'ana pérez gómez';

    await renderWithTheme(<ModulAvatar name={nombre} />);

    expect(screen.getByText('AP')).toBeOnTheScreen();
  });

  it('muestra la imagen y no las iniciales cuando hay uri', async () => {
    const uri = 'https://example.com/foto.png';

    await renderWithTheme(<ModulAvatar name="Ana Pérez" uri={uri} />);

    expect(screen.queryByText('AP')).toBeNull();
  });
});
