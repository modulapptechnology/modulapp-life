import { fireEvent, screen } from '@testing-library/react-native';
import { collectRoles } from '../../../testing/accessibilityRoles/accessibilityRoles';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulToast } from './ModulToast';

describe('ModulToast', () => {
  it('muestra el mensaje dentro de una alerta', async () => {
    const mensaje = 'Guardado';

    await renderWithTheme(<ModulToast message={mensaje} closeLabel="Cerrar" onClose={jest.fn()} />);

    expect(collectRoles(screen.toJSON())).toContain('alert');
    expect(screen.getByText('Guardado')).toBeOnTheScreen();
  });

  it('usa el icono del tipo de aviso', async () => {
    const tipo = 'success';

    await renderWithTheme(
      <ModulToast message="Guardado" type={tipo} closeLabel="Cerrar" onClose={jest.fn()} />,
    );

    expect(screen.getByTestId('CheckCircleIcon')).toBeOnTheScreen();
  });

  it('llama a onClose al presionar el botón de cerrar', async () => {
    const onClose = jest.fn();
    await renderWithTheme(<ModulToast message="Guardado" closeLabel="Cerrar" onClose={onClose} />);

    await fireEvent.press(screen.getByRole('button', { name: 'Cerrar' }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
