import { screen } from '@testing-library/react-native';
import { collectRoles } from '../../../testing/accessibilityRoles/accessibilityRoles';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
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
});
