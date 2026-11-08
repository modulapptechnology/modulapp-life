import { screen } from '@testing-library/react-native';
import { collectRoles } from '../../../testing/accessibilityRoles/accessibilityRoles';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulSkeleton } from '../../atoms/ModulSkeleton/ModulSkeleton';
import { ModulLoadingState } from './ModulLoadingState';

describe('ModulLoadingState', () => {
  it('muestra el indicador de progreso', async () => {
    const mensaje = undefined;

    await renderWithTheme(<ModulLoadingState message={mensaje} />);

    expect(collectRoles(screen.toJSON())).toContain('progressbar');
  });

  it('muestra el mensaje cuando se indica', async () => {
    const mensaje = 'Cargando tareas';

    await renderWithTheme(<ModulLoadingState message={mensaje} />);

    expect(screen.getByText('Cargando tareas')).toBeOnTheScreen();
  });

  it('se anuncia como ocupado con el mensaje como nombre', async () => {
    const mensaje = 'Cargando tareas';

    await renderWithTheme(<ModulLoadingState message={mensaje} />);

    expect(screen.getByRole('progressbar', { name: 'Cargando tareas' })).toBeBusy();
  });

  it('muestra el esqueleto en lugar del texto cuando se indica', async () => {
    const esqueleto = <ModulSkeleton height={20} />;

    await renderWithTheme(<ModulLoadingState message="Cargando" skeleton={esqueleto} />);

    expect(screen.queryByText('Cargando')).toBeNull();
  });
});
