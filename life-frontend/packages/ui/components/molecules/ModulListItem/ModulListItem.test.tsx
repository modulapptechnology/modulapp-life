import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulListItem } from './ModulListItem';

describe('ModulListItem', () => {
  it('muestra el título y la descripción', async () => {
    const descripcion = 'Vence hoy';

    await renderWithTheme(<ModulListItem title="Comprar pan" description={descripcion} />);

    expect(screen.getByText('Comprar pan')).toBeOnTheScreen();
    expect(screen.getByText('Vence hoy')).toBeOnTheScreen();
  });

  it('llama a onPress al presionarlo', async () => {
    const onPress = jest.fn();
    await renderWithTheme(<ModulListItem title="Comprar pan" onPress={onPress} />);

    await fireEvent.press(screen.getByRole('button', { name: 'Comprar pan' }));

    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('no es un botón cuando no recibe onPress', async () => {
    const sinAccion = { title: 'Comprar pan' };

    await renderWithTheme(<ModulListItem {...sinAccion} />);

    expect(screen.queryByRole('button', { name: 'Comprar pan' })).toBeNull();
  });

  it('no llama a onPress cuando está deshabilitado', async () => {
    const onPress = jest.fn();
    await renderWithTheme(<ModulListItem title="Comprar pan" disabled onPress={onPress} />);

    await fireEvent.press(screen.getByLabelText('Comprar pan'));

    expect(onPress).not.toHaveBeenCalled();
  });

  it('ejecuta la acción asociada al presionar su botón', async () => {
    const onPress = jest.fn();
    const acciones = [{ icon: 'CloseCircleIcon' as const, label: 'Eliminar', onPress }];
    await renderWithTheme(<ModulListItem title="Comprar pan" actions={acciones} />);

    await fireEvent.press(screen.getByRole('button', { name: 'Eliminar' }));

    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('marca el elemento como seleccionado', async () => {
    const seleccionado = true;

    await renderWithTheme(
      <ModulListItem title="Comprar pan" selected={seleccionado} onPress={jest.fn()} />,
    );

    expect(screen.getByRole('button', { name: 'Comprar pan' })).toBeSelected();
  });
});
