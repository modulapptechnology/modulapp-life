import { fireEvent, screen } from '@testing-library/react-native';
import { lightColors } from '@life/theme';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulSidebar, type ModulSidebarProps, type ModulSidebarSection } from './ModulSidebar';

const SECTIONS: ModulSidebarSection[] = [
  { path: '/', label: 'Inicio', icon: 'Home2Icon' },
  { path: '/tareas', label: 'Tareas', icon: 'ChecklistMinimalisticIcon', accent: 'primary' },
  { path: '/agenda', label: 'Agenda', icon: 'CalendarIcon', accent: 'secondary' },
];

describe('ModulSidebar', () => {
  it('muestra una sección por módulo y marca como activa solo la ruta actual', async () => {
    const rutaActual = '/tareas/pendientes';

    await renderWithTheme(
      <ModulSidebar sections={SECTIONS} activePath={rutaActual} onNavigate={jest.fn()} />,
    );

    expect(screen.getAllByRole('tab')).toHaveLength(3);
    expect(screen.getByRole('tab', { name: 'Tareas' })).toBeSelected();
    expect(screen.getByRole('tab', { name: 'Inicio' })).not.toBeSelected();
  });

  it('pinta el icono con el color de acento del módulo', async () => {
    const acento = lightColors.secondary;

    await renderWithTheme(
      <ModulSidebar sections={SECTIONS} activePath="/" onNavigate={jest.fn()} />,
    );

    expect(screen.getByTestId('CalendarIcon').props.color).toBe(acento);
  });

  it('navega a la ruta de la sección que se toca', async () => {
    const onNavigate = jest.fn();
    await renderWithTheme(
      <ModulSidebar sections={SECTIONS} activePath="/" onNavigate={onNavigate} />,
    );

    await fireEvent.press(screen.getByRole('tab', { name: 'Agenda' }));

    expect(onNavigate).toHaveBeenCalledWith('/agenda');
  });

  it('no muestra secciones cuando no hay módulos', async () => {
    const sinSecciones: ModulSidebarSection[] = [];

    await renderWithTheme(
      <ModulSidebar sections={sinSecciones} activePath="/" onNavigate={jest.fn()} />,
    );

    expect(screen.queryAllByRole('tab')).toHaveLength(0);
  });

  describe('con sesión activa', () => {
    const USER = { name: 'Ana Pérez', path: '/profile' };
    const SETTINGS: ModulSidebarSection[] = [
      { path: '/settings', label: 'Ajustes', icon: 'SettingsIcon' },
    ];

    function buildSignOut(overrides: Partial<NonNullable<ModulSidebarProps['signOut']>> = {}) {
      return {
        label: 'Cerrar sesión',
        onSignOut: jest.fn(),
        confirmTitle: '¿Cerrar sesión?',
        confirmMessage: 'Hay operaciones pendientes',
        cancelLabel: 'Cancelar',
        ...overrides,
      };
    }

    function renderSignedIn(props: Partial<ModulSidebarProps> = {}) {
      return renderWithTheme(
        <ModulSidebar
          sections={SECTIONS}
          activePath="/"
          onNavigate={jest.fn()}
          user={USER}
          footerSections={SETTINGS}
          signOut={buildSignOut()}
          {...props}
        />,
      );
    }

    it('muestra el usuario de primero, los ajustes y el cierre de sesión al final', async () => {
      const expectedLabels = [
        'Ana Pérez',
        'Inicio',
        'Tareas',
        'Agenda',
        'Ajustes',
        'Cerrar sesión',
      ];

      await renderSignedIn();

      expect(screen.getAllByRole('tab').map((tab) => tab.props.accessibilityLabel)).toEqual(
        expectedLabels,
      );
    });

    it('lleva al perfil al tocar el usuario', async () => {
      const onNavigate = jest.fn();
      await renderSignedIn({ onNavigate });

      await fireEvent.press(screen.getByRole('tab', { name: 'Ana Pérez' }));

      expect(onNavigate).toHaveBeenCalledWith('/profile');
    });

    it('no muestra usuario, ajustes ni cierre de sesión sin sesión activa', async () => {
      const noSession = null;

      await renderSignedIn({ user: noSession });

      expect(screen.queryByRole('tab', { name: 'Ana Pérez' })).toBeNull();
      expect(screen.queryByRole('tab', { name: 'Ajustes' })).toBeNull();
      expect(screen.queryByRole('tab', { name: 'Cerrar sesión' })).toBeNull();
    });

    it('cierra la sesión sin confirmar cuando no hay operaciones pendientes', async () => {
      const signOut = buildSignOut({ pendingOperations: 0 });
      await renderSignedIn({ signOut });

      await fireEvent.press(screen.getByRole('tab', { name: 'Cerrar sesión' }));

      expect(signOut.onSignOut).toHaveBeenCalledTimes(1);
      expect(screen.queryByText('¿Cerrar sesión?')).toBeNull();
    });

    it('pide confirmación antes de cerrar la sesión con operaciones pendientes', async () => {
      const signOut = buildSignOut({ pendingOperations: 2 });
      await renderSignedIn({ signOut });

      await fireEvent.press(screen.getByRole('tab', { name: 'Cerrar sesión' }));

      expect(screen.getByText('¿Cerrar sesión?')).toBeOnTheScreen();
      expect(signOut.onSignOut).not.toHaveBeenCalled();
    });

    it('no cierra la sesión si se cancela la confirmación', async () => {
      const signOut = buildSignOut({ pendingOperations: 2 });
      await renderSignedIn({ signOut });
      await fireEvent.press(screen.getByRole('tab', { name: 'Cerrar sesión' }));

      await fireEvent.press(screen.getByRole('button', { name: 'Cancelar' }));

      expect(signOut.onSignOut).not.toHaveBeenCalled();
    });
  });
});
