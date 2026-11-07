import { fireEvent, screen } from '@testing-library/react-native';
import { lightColors } from '@life/theme';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulSidebar, type ModulSidebarSection } from './ModulSidebar';

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
});
