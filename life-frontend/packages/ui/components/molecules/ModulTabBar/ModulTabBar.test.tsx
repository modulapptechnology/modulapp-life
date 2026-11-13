import { fireEvent, screen } from '@testing-library/react-native';
import { lightColors } from '@life/theme';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulTabBar, type ModulTabBarItem } from './ModulTabBar';

const ITEMS: ModulTabBarItem[] = [
  { path: '/', label: 'Inicio', icon: 'Home2Icon' },
  { path: '/agenda', label: 'Agenda', icon: 'CalendarIcon' },
  { path: '/profile', label: 'Perfil', icon: 'UserIcon' },
];

describe('ModulTabBar', () => {
  it('muestra una pestaña por elemento y marca como activa solo la ruta actual', async () => {
    const rutaActual = '/agenda/proximos';

    await renderWithTheme(
      <ModulTabBar items={ITEMS} activePath={rutaActual} onNavigate={jest.fn()} />,
    );

    expect(screen.getAllByRole('tab')).toHaveLength(3);
    expect(screen.getByRole('tab', { name: 'Agenda' })).toBeSelected();
    expect(screen.getByRole('tab', { name: 'Inicio' })).not.toBeSelected();
  });

  it('pinta con el color de marca el icono activo y con el de texto los demás', async () => {
    const colorActivo = lightColors.brand;
    const colorInactivo = lightColors.text;

    await renderWithTheme(<ModulTabBar items={ITEMS} activePath="/" onNavigate={jest.fn()} />);

    expect(screen.getByTestId('Home2Icon').props.color).toBe(colorActivo);
    expect(screen.getByTestId('CalendarIcon').props.color).toBe(colorInactivo);
  });

  it('navega a la ruta de la pestaña que se toca', async () => {
    const onNavigate = jest.fn();
    await renderWithTheme(<ModulTabBar items={ITEMS} activePath="/" onNavigate={onNavigate} />);

    await fireEvent.press(screen.getByRole('tab', { name: 'Perfil' }));

    expect(onNavigate).toHaveBeenCalledWith('/profile');
  });

  it('no muestra el botón de acción cuando no se indica', async () => {
    const sinAccion = undefined;

    await renderWithTheme(
      <ModulTabBar items={ITEMS} activePath="/" onNavigate={jest.fn()} action={sinAccion} />,
    );

    expect(screen.queryByRole('button')).toBeNull();
  });

  it('ejecuta la acción al tocar el botón flotante', async () => {
    const onPress = jest.fn();
    const action = { icon: 'AddCircleIcon' as const, label: 'Nueva captura', onPress };
    await renderWithTheme(
      <ModulTabBar items={ITEMS} activePath="/" onNavigate={jest.fn()} action={action} />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Nueva captura' }));

    expect(onPress).toHaveBeenCalledTimes(1);
  });
});
