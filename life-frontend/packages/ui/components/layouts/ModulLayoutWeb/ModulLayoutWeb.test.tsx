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
    <ModulLayoutWeb
      title="ModulApp"
      sections={NAV_SECTIONS}
      activePath={activePath}
      onNavigate={onNavigate}
    >
      <></>
    </ModulLayoutWeb>,
  );
}

describe('ModulLayoutWeb', () => {
  it('muestra el encabezado, la barra lateral y el contenido', async () => {
    const titulo = 'ModulApp';

    await renderLayout();

    expect(screen.getByText(titulo)).toBeOnTheScreen();
    expect(screen.getByTestId('web-sidebar')).toBeOnTheScreen();
    expect(screen.getByTestId('web-main')).toBeOnTheScreen();
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
    fireEvent.press(screen.getByRole('tab', { name: 'Agenda' }));

    expect(onNavigate).toHaveBeenCalledWith('/agenda');
  });
});
