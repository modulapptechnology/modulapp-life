import { fireEvent, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulHeader } from './ModulHeader';

describe('ModulHeader', () => {
  it('avisa cuando se toca el botón del sidebar', async () => {
    const onToggleSidebar = jest.fn();

    await renderWithTheme(<ModulHeader sidebarOpen onToggleSidebar={onToggleSidebar} />);
    await fireEvent.press(screen.getByRole('button', { name: 'Cerrar barra lateral' }));

    expect(onToggleSidebar).toHaveBeenCalledTimes(1);
  });

  it('cambia la etiqueta del botón según el sidebar esté abierto o cerrado', async () => {
    const sidebarCerrado = false;

    await renderWithTheme(<ModulHeader sidebarOpen={sidebarCerrado} onToggleSidebar={jest.fn()} />);

    expect(screen.getByRole('button', { name: 'Abrir barra lateral' })).toBeOnTheScreen();
  });

  it('muestra el contenido propio de cada plataforma', async () => {
    const extras = (
      <>
        <Text>Migas</Text>
      </>
    );

    await renderWithTheme(
      <ModulHeader sidebarOpen onToggleSidebar={jest.fn()} end={<Text>Acciones</Text>}>
        {extras}
      </ModulHeader>,
    );

    expect(screen.getByText('Migas')).toBeOnTheScreen();
    expect(screen.getByText('Acciones')).toBeOnTheScreen();
  });

  it('no muestra el botón de la barra lateral cuando no hay quien lo atienda', async () => {
    const sinBarraLateral = undefined;

    await renderWithTheme(<ModulHeader onToggleSidebar={sinBarraLateral} />);

    expect(screen.queryByRole('button')).toBeNull();
  });
});
