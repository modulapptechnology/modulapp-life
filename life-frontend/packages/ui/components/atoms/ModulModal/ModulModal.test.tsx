import { fireEvent, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulModal } from './ModulModal';

describe('ModulModal', () => {
  it('muestra el contenido cuando está visible', async () => {
    const visible = true;

    await renderWithTheme(
      <ModulModal visible={visible} onClose={jest.fn()} closeLabel="Cerrar">
        <Text>Contenido</Text>
      </ModulModal>,
    );

    expect(screen.getByText('Contenido')).toBeOnTheScreen();
  });

  it('no muestra el contenido cuando no está visible', async () => {
    const visible = false;

    await renderWithTheme(
      <ModulModal visible={visible} onClose={jest.fn()} closeLabel="Cerrar">
        <Text>Contenido</Text>
      </ModulModal>,
    );

    expect(screen.queryByText('Contenido')).toBeNull();
  });

  it('llama a onClose al presionar la capa de cierre', async () => {
    const onClose = jest.fn();
    await renderWithTheme(
      <ModulModal visible onClose={onClose} closeLabel="Cerrar">
        <Text>Contenido</Text>
      </ModulModal>,
    );

    await fireEvent.press(screen.getByLabelText('Cerrar', { hidden: true }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
