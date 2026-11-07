import { fireEvent, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulBottomSheet } from '../ModulBottomSheet/ModulBottomSheet';
import { ModulDialog } from './ModulDialog';

describe('ModulDialog', () => {
  it('muestra el título y el contenido cuando está visible', async () => {
    const visible = true;

    await renderWithTheme(
      <ModulDialog visible={visible} title="Nueva tarea" onClose={jest.fn()} closeLabel="Cerrar">
        <Text>Contenido</Text>
      </ModulDialog>,
    );

    expect(screen.getByText('Nueva tarea')).toBeOnTheScreen();
    expect(screen.getByText('Contenido')).toBeOnTheScreen();
  });

  it('llama a onClose al presionar el botón de cierre', async () => {
    const onClose = jest.fn();
    await renderWithTheme(
      <ModulDialog visible title="Nueva tarea" onClose={onClose} closeLabel="Cerrar">
        <Text>Contenido</Text>
      </ModulDialog>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Cerrar' }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('no muestra nada cuando no está visible', async () => {
    const visible = false;

    await renderWithTheme(
      <ModulDialog visible={visible} title="Nueva tarea" onClose={jest.fn()} closeLabel="Cerrar">
        <Text>Contenido</Text>
      </ModulDialog>,
    );

    expect(screen.queryByText('Contenido')).toBeNull();
  });
});

describe('ModulBottomSheet', () => {
  it('muestra el título y el contenido como hoja inferior', async () => {
    const visible = true;

    await renderWithTheme(
      <ModulBottomSheet visible={visible} title="Filtros" onClose={jest.fn()} closeLabel="Cerrar">
        <Text>Opciones</Text>
      </ModulBottomSheet>,
    );

    expect(screen.getByText('Filtros')).toBeOnTheScreen();
    expect(screen.getByText('Opciones')).toBeOnTheScreen();
  });
});
