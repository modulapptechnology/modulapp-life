import { act, fireEvent, screen } from '@testing-library/react-native';
import { ThemeProvider } from '../../../theme/ThemeProvider';
import { darkColors, lightColors } from '@life/theme';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import {
  findActiveRailSection,
  ModulSidebarRail,
  type ModulSidebarRailSection,
} from './ModulSidebarRail';

const SECTIONS: ModulSidebarRailSection[] = [
  { path: '/', label: 'Inicio', icon: 'Home2Icon' },
  {
    path: '/tareas',
    label: 'Tareas',
    icon: 'ChecklistMinimalisticIcon',
    groups: [
      {
        id: 'mias',
        title: 'Mis tareas',
        links: [
          { path: '/tareas/hoy', label: 'Hoy y vencido', icon: 'ClockCircleIcon' },
          { path: '/tareas/mas', label: 'Ver más', icon: 'MenuDotsIcon' },
        ],
      },
      {
        id: 'listas',
        title: 'Listas',
        links: [{ path: '/tareas/personal', label: 'Lista personal', icon: 'ListIcon' }],
      },
    ],
  },
];

function renderRail(activePath: string, onNavigate = jest.fn(), open = true) {
  return renderWithTheme(
    <ModulSidebarRail
      sections={SECTIONS}
      activePath={activePath}
      onNavigate={onNavigate}
      open={open}
    />,
  );
}

describe('ModulSidebarRail', () => {
  // La animación del panel avanza con temporizadores: se controlan para que sus cambios ocurran dentro de act.
  beforeEach(() => jest.useFakeTimers());
  afterEach(async () => {
    await act(async () => {
      jest.advanceTimersByTime(1000);
    });
    jest.useRealTimers();
  });

  it('muestra una opción por sección y marca como activa solo la de la ruta actual', async () => {
    const rutaActual = '/tareas';

    await renderRail(rutaActual);

    expect(screen.getAllByRole('tab')).toHaveLength(2);
    expect(screen.getByRole('tab', { name: 'Tareas' })).toBeSelected();
    expect(screen.getByRole('tab', { name: 'Inicio' })).not.toBeSelected();
  });

  it('abre el panel con las sub-opciones de la opción activa', async () => {
    const rutaActual = '/tareas';

    await renderRail(rutaActual);

    expect(screen.getByTestId('web-sidebar-panel')).toBeOnTheScreen();
    expect(screen.getByRole('link', { name: 'Hoy y vencido' })).toBeOnTheScreen();
    expect(screen.getByRole('link', { name: 'Lista personal' })).toBeOnTheScreen();
  });

  it('no muestra el panel cuando la opción activa no tiene sub-opciones', async () => {
    const rutaActual = '/';

    await renderRail(rutaActual);

    expect(screen.queryByTestId('web-sidebar-panel')).toBeNull();
  });

  it('mantiene activa la opción cuando la ruta es una de sus sub-opciones', async () => {
    const rutaDeSubOpcion = '/tareas/hoy';

    await renderRail(rutaDeSubOpcion);

    expect(screen.getByRole('tab', { name: 'Tareas' })).toBeSelected();
    expect(screen.getByRole('link', { name: 'Hoy y vencido' })).toBeSelected();
  });

  it('navega a la ruta de la sub-opción que se toca', async () => {
    const onNavigate = jest.fn();
    await renderRail('/tareas', onNavigate);

    await fireEvent.press(screen.getByRole('link', { name: 'Hoy y vencido' }));

    expect(onNavigate).toHaveBeenCalledWith('/tareas/hoy');
  });

  it('colapsa un grupo al tocar su título', async () => {
    await renderRail('/tareas');

    await fireEvent.press(screen.getByRole('button', { name: 'Listas' }));

    expect(screen.queryByRole('link', { name: 'Lista personal' })).toBeNull();
  });

  it('expande de nuevo un grupo colapsado al tocar su título otra vez', async () => {
    await renderRail('/tareas');
    await fireEvent.press(screen.getByRole('button', { name: 'Listas' }));

    await fireEvent.press(screen.getByRole('button', { name: 'Listas' }));

    expect(screen.getByRole('link', { name: 'Lista personal' })).toBeOnTheScreen();
  });

  it('encuentra la opción activa por su ruta o por la de una sub-opción', () => {
    const rutaDeSubOpcion = '/tareas/personal';

    const activa = findActiveRailSection(SECTIONS, rutaDeSubOpcion);

    expect(activa?.path).toBe('/tareas');
  });

  describe('en vistas sin opción propia (perfil, ajustes)', () => {
    it('conserva el panel de la última opción elegida', async () => {
      const { rerender } = await renderRail('/tareas');

      await rerender(
        <ThemeProvider preference="light">
          <ModulSidebarRail sections={SECTIONS} activePath="/settings" onNavigate={jest.fn()} />
        </ThemeProvider>,
      );

      expect(screen.getByTestId('web-sidebar-panel')).toBeOnTheScreen();
      expect(screen.getByRole('link', { name: 'Hoy y vencido' })).toBeOnTheScreen();
    });

    it('muestra el panel de la primera opción si todavía no se eligió ninguna', async () => {
      const tareasPrimero = [SECTIONS[1], SECTIONS[0]];

      await renderWithTheme(
        <ModulSidebarRail sections={tareasPrimero} activePath="/settings" onNavigate={jest.fn()} />,
      );

      expect(screen.getByRole('link', { name: 'Hoy y vencido' })).toBeOnTheScreen();
    });

    it('muestra el menú flotante al pasar el cursor con el sidebar cerrado', async () => {
      const vistaSinOpcion = '/settings';
      const panelCerrado = false;
      await renderRail(vistaSinOpcion, jest.fn(), panelCerrado);

      await fireEvent(screen.getByRole('tab', { name: 'Tareas' }), 'hoverIn');

      expect(screen.getByTestId('web-sidebar-flyout')).toBeOnTheScreen();
    });
  });

  describe('colores según el tema', () => {
    it('pinta el ícono inactivo con el color de texto del tema claro', async () => {
      const rutaDeInicio = '/';

      await renderRail(rutaDeInicio);

      expect(screen.getByTestId('ChecklistMinimalisticIcon').props.color).toBe(lightColors.text);
    });

    it('pinta el ícono inactivo con el color de texto del tema oscuro', async () => {
      const temaOscuro = 'dark';

      await renderWithTheme(
        <ModulSidebarRail sections={SECTIONS} activePath="/" onNavigate={jest.fn()} />,
        temaOscuro,
      );

      expect(screen.getByTestId('ChecklistMinimalisticIcon').props.color).toBe(darkColors.text);
    });

    it('pinta el ícono activo con el color primario', async () => {
      const rutaDeInicio = '/';

      await renderRail(rutaDeInicio);

      expect(screen.getByTestId('Home2Icon').props.color).toBe(lightColors.primary);
    });
  });

  describe('menú flotante al pasar el cursor', () => {
    it('muestra las sub-opciones de la opción bajo el cursor cuando el panel está cerrado', async () => {
      const panelCerrado = false;
      await renderRail('/', jest.fn(), panelCerrado);

      await fireEvent(screen.getByRole('tab', { name: 'Tareas' }), 'hoverIn');

      expect(screen.getByTestId('web-sidebar-flyout')).toBeOnTheScreen();
      expect(screen.getByRole('link', { name: 'Hoy y vencido' })).toBeOnTheScreen();
    });

    it('no lo muestra mientras el panel de esa opción está abierto', async () => {
      const rutaConPanel = '/tareas';
      await renderRail(rutaConPanel);

      await fireEvent(screen.getByRole('tab', { name: 'Tareas' }), 'hoverIn');

      expect(screen.queryByTestId('web-sidebar-flyout')).toBeNull();
    });

    it('no lo muestra para una opción sin sub-opciones', async () => {
      const panelCerrado = false;
      await renderRail('/tareas', jest.fn(), panelCerrado);

      await fireEvent(screen.getByRole('tab', { name: 'Inicio' }), 'hoverIn');

      expect(screen.queryByTestId('web-sidebar-flyout')).toBeNull();
    });

    it('se cierra cuando el cursor sale de la barra y del menú', async () => {
      const panelCerrado = false;
      await renderRail('/', jest.fn(), panelCerrado);
      await fireEvent(screen.getByRole('tab', { name: 'Tareas' }), 'hoverIn');

      await fireEvent(screen.getByTestId('web-sidebar-rail'), 'mouseLeave');

      expect(screen.queryByTestId('web-sidebar-flyout')).toBeNull();
    });

    it('cambia al menú de otra opción al pasar el cursor por ella', async () => {
      const panelCerrado = false;
      await renderRail('/', jest.fn(), panelCerrado);
      await fireEvent(screen.getByRole('tab', { name: 'Tareas' }), 'hoverIn');

      await fireEvent(screen.getByRole('tab', { name: 'Inicio' }), 'hoverIn');

      expect(screen.queryByTestId('web-sidebar-flyout')).toBeNull();
    });

    it('navega y se cierra al tocar una sub-opción', async () => {
      const onNavigate = jest.fn();
      const panelCerrado = false;
      await renderRail('/', onNavigate, panelCerrado);
      await fireEvent(screen.getByRole('tab', { name: 'Tareas' }), 'hoverIn');

      await fireEvent.press(screen.getByRole('link', { name: 'Hoy y vencido' }));

      expect(onNavigate).toHaveBeenCalledWith('/tareas/hoy');
      expect(screen.queryByTestId('web-sidebar-flyout')).toBeNull();
    });
  });

  describe('perfil y ajustes con sub-opciones', () => {
    const PERFIL_GRUPOS = [
      {
        id: 'cuenta',
        title: 'Cuenta',
        links: [{ path: '/profile/seguridad', label: 'Seguridad', icon: 'ShieldIcon' as const }],
      },
    ];
    const AJUSTES: ModulSidebarRailSection = {
      path: '/settings',
      label: 'Ajustes',
      icon: 'SettingsIcon',
      groups: [
        {
          id: 'preferencias',
          title: 'Preferencias',
          links: [{ path: '/settings/idioma', label: 'Idioma', icon: 'GlobalIcon' as const }],
        },
      ],
    };

    function renderSignedIn(activePath: string, open = true) {
      return renderWithTheme(
        <ModulSidebarRail
          sections={SECTIONS}
          activePath={activePath}
          onNavigate={jest.fn()}
          user={{ name: 'Ana Pérez', path: '/profile', caption: 'Perfil', groups: PERFIL_GRUPOS }}
          footerSections={[AJUSTES]}
          open={open}
        />,
      );
    }

    it('abre el panel del perfil al estar en la vista del perfil', async () => {
      const rutaDelPerfil = '/profile';

      await renderSignedIn(rutaDelPerfil);

      expect(screen.getByRole('tab', { name: 'Ana Pérez' })).toBeSelected();
      expect(screen.getByRole('link', { name: 'Seguridad' })).toBeOnTheScreen();
    });

    it('abre el panel de los ajustes al estar en la vista de ajustes', async () => {
      const rutaDeAjustes = '/settings';

      await renderSignedIn(rutaDeAjustes);

      expect(screen.getByRole('tab', { name: 'Ajustes' })).toBeSelected();
      expect(screen.getByRole('link', { name: 'Idioma' })).toBeOnTheScreen();
    });

    it('muestra el menú flotante del perfil al pasar el cursor con el sidebar cerrado', async () => {
      const panelCerrado = false;
      await renderSignedIn('/tareas', panelCerrado);

      await fireEvent(screen.getByRole('tab', { name: 'Ana Pérez' }), 'hoverIn');

      expect(screen.getByRole('link', { name: 'Seguridad' })).toBeOnTheScreen();
    });

    it('muestra el menú flotante de los ajustes al pasar el cursor con el sidebar cerrado', async () => {
      const panelCerrado = false;
      await renderSignedIn('/tareas', panelCerrado);

      await fireEvent(screen.getByRole('tab', { name: 'Ajustes' }), 'hoverIn');

      expect(screen.getByRole('link', { name: 'Idioma' })).toBeOnTheScreen();
    });
  });

  describe('con sesión activa', () => {
    it('muestra el usuario, los ajustes y el cierre de sesión', async () => {
      const onSignOut = jest.fn();
      await renderWithTheme(
        <ModulSidebarRail
          sections={SECTIONS}
          activePath="/"
          onNavigate={jest.fn()}
          user={{ name: 'Ana Pérez', path: '/profile' }}
          footerSections={[{ path: '/settings', label: 'Ajustes', icon: 'SettingsIcon' }]}
          signOut={{
            label: 'Cerrar sesión',
            onSignOut,
            confirmTitle: '¿Cerrar sesión?',
            confirmMessage: 'Hay operaciones pendientes',
            cancelLabel: 'Cancelar',
          }}
        />,
      );

      await fireEvent.press(screen.getByRole('tab', { name: 'Cerrar sesión' }));

      expect(screen.getByRole('tab', { name: 'Ana Pérez' })).toBeOnTheScreen();
      expect(screen.getByRole('tab', { name: 'Ajustes' })).toBeOnTheScreen();
      expect(onSignOut).toHaveBeenCalledTimes(1);
    });
  });
});
