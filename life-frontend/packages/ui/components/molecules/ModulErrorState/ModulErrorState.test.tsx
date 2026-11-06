import { fireEvent, screen } from '@testing-library/react-native';
import { collectRoles } from '../../../testing/accessibilityRoles/accessibilityRoles';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulErrorState } from './ModulErrorState';

describe('ModulErrorState', () => {
  it('muestra el título dentro de una alerta', async () => {
    const titulo = 'No se pudo cargar';

    await renderWithTheme(<ModulErrorState title={titulo} />);

    expect(collectRoles(screen.toJSON())).toContain('alert');
    expect(screen.getByText('No se pudo cargar')).toBeOnTheScreen();
  });

  it('no muestra el botón de reintento cuando falta onRetry', async () => {
    const soloEtiqueta = { title: 'Error', retryLabel: 'Reintentar' };

    await renderWithTheme(<ModulErrorState {...soloEtiqueta} />);

    expect(screen.queryByRole('button', { name: 'Reintentar' })).toBeNull();
  });

  it('llama a onRetry al presionar el botón de reintento', async () => {
    const onRetry = jest.fn();
    await renderWithTheme(
      <ModulErrorState title="Error" retryLabel="Reintentar" onRetry={onRetry} />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Reintentar' }));

    expect(onRetry).toHaveBeenCalledTimes(1);
  });
});
