import { fireEvent, renderHook, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulDragProvider, ModulDraggable, ModulDropZone } from './ModulDragDrop';

const tablero = (onDrop: (itemKey: string, zoneKey: string) => void) => (
  <ModulDragProvider onDrop={onDrop}>
    <ModulDraggable itemKey="tarea-1" label="Tarea 1">
      <Text>Tarea 1</Text>
    </ModulDraggable>
    <ModulDropZone zoneKey="hecho" label="Hecho">
      <Text>Hecho</Text>
    </ModulDropZone>
  </ModulDragProvider>
);

describe('ModulDragDrop', () => {
  it('suelta el elemento activado en la zona con onDrop(elemento, zona)', async () => {
    const onDrop = jest.fn();
    await renderWithTheme(tablero(onDrop));
    await fireEvent.press(screen.getByRole('button', { name: 'Tarea 1' }));

    await fireEvent.press(screen.getByRole('button', { name: 'Hecho' }));

    expect(onDrop).toHaveBeenCalledWith('tarea-1', 'hecho');
  });

  it('no suelta nada si se presiona la zona sin un elemento activado', async () => {
    const onDrop = jest.fn();
    await renderWithTheme(tablero(onDrop));

    await fireEvent.press(screen.getByRole('button', { name: 'Hecho' }));

    expect(onDrop).not.toHaveBeenCalled();
  });

  it('marca el elemento como seleccionado al activarlo y lo desactiva al presionarlo otra vez', async () => {
    await renderWithTheme(tablero(jest.fn()));
    await fireEvent.press(screen.getByRole('button', { name: 'Tarea 1' }));
    expect(screen.getByRole('button', { name: 'Tarea 1' })).toBeSelected();

    await fireEvent.press(screen.getByRole('button', { name: 'Tarea 1' }));

    expect(screen.getByRole('button', { name: 'Tarea 1' })).not.toBeSelected();
  });

  it('falla si un elemento arrastrable se usa fuera del ModulDragProvider', async () => {
    jest.spyOn(console, 'error').mockImplementation(() => undefined);

    const dibujar = renderHook(() => null, {
      wrapper: () => (
        <ModulDraggable itemKey="x" label="X">
          <Text>X</Text>
        </ModulDraggable>
      ),
    });

    await expect(dibujar).rejects.toThrow('deben estar dentro de <ModulDragProvider>');
  });
});
