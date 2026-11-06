import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulConfirmDialog } from './ModulConfirmDialog';

const textos = { title: 'Eliminar tarea', confirmLabel: 'Eliminar', cancelLabel: 'Cancelar' };

describe('ModulConfirmDialog', () => {
  it('muestra el título y el mensaje cuando está visible', async () => {
    const mensaje = 'No se puede deshacer';

    await renderWithTheme(
      <ModulConfirmDialog
        visible
        {...textos}
        message={mensaje}
        onConfirm={jest.fn()}
        onCancel={jest.fn()}
      />,
    );

    expect(screen.getByText('Eliminar tarea')).toBeOnTheScreen();
    expect(screen.getByText('No se puede deshacer')).toBeOnTheScreen();
  });

  it('llama a onConfirm al presionar el botón de confirmar', async () => {
    const onConfirm = jest.fn();
    await renderWithTheme(
      <ModulConfirmDialog visible {...textos} onConfirm={onConfirm} onCancel={jest.fn()} />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Eliminar' }));

    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it('llama a onCancel al presionar el botón de cancelar', async () => {
    const onCancel = jest.fn();
    await renderWithTheme(
      <ModulConfirmDialog visible {...textos} onConfirm={jest.fn()} onCancel={onCancel} />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Cancelar' }));

    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  it('no muestra nada cuando no está visible', async () => {
    const visible = false;

    await renderWithTheme(
      <ModulConfirmDialog
        visible={visible}
        {...textos}
        onConfirm={jest.fn()}
        onCancel={jest.fn()}
      />,
    );

    expect(screen.queryByText('Eliminar tarea')).toBeNull();
  });
});
