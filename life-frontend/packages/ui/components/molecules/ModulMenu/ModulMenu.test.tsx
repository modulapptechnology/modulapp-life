import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulMenu } from './ModulMenu';

describe('ModulMenu', () => {
  it('muestra el título y las opciones cuando está visible', async () => {
    const opciones = [
      { key: 'editar', label: 'Editar', onPress: jest.fn() },
      { key: 'borrar', label: 'Borrar', onPress: jest.fn() },
    ];

    await renderWithTheme(
      <ModulMenu
        visible
        onClose={jest.fn()}
        closeLabel="Cerrar"
        title="Acciones"
        items={opciones}
      />,
    );

    expect(screen.getByText('Acciones')).toBeOnTheScreen();
    expect(screen.getByText('Editar')).toBeOnTheScreen();
    expect(screen.getByText('Borrar')).toBeOnTheScreen();
  });

  it('cierra el menú y ejecuta la acción al elegir una opción', async () => {
    const onClose = jest.fn();
    const onPress = jest.fn();
    const opciones = [{ key: 'editar', label: 'Editar', onPress }];
    await renderWithTheme(
      <ModulMenu visible onClose={onClose} closeLabel="Cerrar" items={opciones} />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Editar' }));

    expect(onClose).toHaveBeenCalledTimes(1);
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('no muestra las opciones cuando no está visible', async () => {
    const visible = false;
    const opciones = [{ key: 'editar', label: 'Editar', onPress: jest.fn() }];

    await renderWithTheme(
      <ModulMenu visible={visible} onClose={jest.fn()} closeLabel="Cerrar" items={opciones} />,
    );

    expect(screen.queryByText('Editar')).toBeNull();
  });
});
