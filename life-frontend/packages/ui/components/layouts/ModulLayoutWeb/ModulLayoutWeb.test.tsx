import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulLayoutWeb, type NavSection } from './ModulLayoutWeb';

const NAV_SECTIONS: NavSection[] = [
  { path: '/', label: 'Inicio', icon: 'Home2Icon' },
  { path: '/tareas', label: 'Tareas', icon: 'ChecklistMinimalisticIcon' },
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
  it('muestra el encabezado, la barra lateral y el contenido', async () => {
    const botonDelSidebar = 'Cerrar barra lateral';

    await renderLayout();

    expect(screen.getByRole('button', { name: botonDelSidebar })).toBeOnTheScreen();
    expect(screen.getByTestId('web-sidebar')).toBeOnTheScreen();
    expect(screen.getByTestId('web-main')).toBeOnTheScreen();
  });

  it('oculta la barra lateral con el botón del encabezado', async () => {
    const botonCerrar = 'Cerrar barra lateral';

    await renderLayout();
    await fireEvent.press(screen.getByRole('button', { name: botonCerrar }));

    expect(screen.queryByTestId('web-sidebar')).toBeNull();
  });

  it('vuelve a mostrar la barra lateral al tocar el botón por segunda vez', async () => {
    const botonDelSidebar = { name: /barra lateral/ };

    await renderLayout();
    await fireEvent.press(screen.getByRole('button', botonDelSidebar));
    await fireEvent.press(screen.getByRole('button', botonDelSidebar));

    expect(screen.getByTestId('web-sidebar')).toBeOnTheScreen();
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
