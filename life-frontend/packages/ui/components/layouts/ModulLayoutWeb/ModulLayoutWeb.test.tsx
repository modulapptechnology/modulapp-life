import { act, fireEvent, screen, waitFor } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulLayoutWeb, type NavSection } from './ModulLayoutWeb';

const NAV_SECTIONS: NavSection[] = [
  { path: '/', label: 'Inicio', icon: 'Home2Icon' },
  {
    path: '/tareas',
    label: 'Tareas',
    icon: 'ChecklistMinimalisticIcon',
    groups: [
      {
        id: 'mias',
        title: 'Mis tareas',
        links: [{ path: '/tareas/hoy', label: 'Hoy y vencido', icon: 'ClockCircleIcon' }],
      },
    ],
  },
  { path: '/agenda', label: 'Agenda', icon: 'CalendarIcon' },
];

function renderLayout(activePath = '/', onNavigate = jest.fn()) {
  return renderWithTheme(
    <ModulLayoutWeb sections={NAV_SECTIONS} activePath={activePath} onNavigate={onNavigate}>
      <></>
    </ModulLayoutWeb>,
  );
}

describe('ModulLayoutWeb', () => {
  // La animación del panel avanza con temporizadores: se controlan para que sus cambios ocurran dentro de act.
  beforeEach(() => jest.useFakeTimers());
  afterEach(async () => {
    await act(async () => {
      jest.advanceTimersByTime(1000);
    });
    jest.useRealTimers();
  });

  it('muestra el encabezado, la barra lateral y el contenido', async () => {
    const botonDelSidebar = 'Cerrar barra lateral';

    await renderLayout();

    expect(screen.getByRole('button', { name: botonDelSidebar })).toBeOnTheScreen();
    expect(screen.getByTestId('web-sidebar')).toBeOnTheScreen();
    expect(screen.getByTestId('web-main')).toBeOnTheScreen();
  });

  it('deja solo la barra de íconos al cerrar con el botón del encabezado', async () => {
    const botonCerrar = 'Cerrar barra lateral';
    await renderLayout('/tareas');

    await fireEvent.press(screen.getByRole('button', { name: botonCerrar }));

    await waitFor(() => expect(screen.queryByTestId('web-sidebar-panel')).toBeNull());
    expect(screen.getByRole('tab', { name: 'Tareas' })).toBeOnTheScreen();
  });

  it('vuelve a mostrar el panel al tocar el botón por segunda vez', async () => {
    const botonDelSidebar = { name: /barra lateral/ };
    await renderLayout('/tareas');
    await fireEvent.press(screen.getByRole('button', botonDelSidebar));
    await waitFor(() => expect(screen.queryByTestId('web-sidebar-panel')).toBeNull());

    await fireEvent.press(screen.getByRole('button', botonDelSidebar));

    expect(screen.getByTestId('web-sidebar-panel')).toBeOnTheScreen();
  });

  it('marca como activa solo la sección de la ruta actual', async () => {
    const rutaActual = '/tareas';

    await renderLayout(rutaActual);

    expect(screen.getByRole('tab', { name: 'Tareas' })).toBeSelected();
    expect(screen.getByRole('tab', { name: 'Inicio' })).not.toBeSelected();
  });

  it('navega a la ruta de la sección que se toca', async () => {
    const onNavigate = jest.fn();

    await renderLayout('/', onNavigate);
    await fireEvent.press(screen.getByRole('tab', { name: 'Agenda' }));

    expect(onNavigate).toHaveBeenCalledWith('/agenda');
  });
});
