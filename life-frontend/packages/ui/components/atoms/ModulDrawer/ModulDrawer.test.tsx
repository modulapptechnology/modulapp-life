import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulText } from '../ModulText/ModulText';
import { ModulDrawer } from './ModulDrawer';

describe('ModulDrawer', () => {
  it('muestra su contenido cuando está visible', async () => {
    const visible = true;

    await renderWithTheme(
      <ModulDrawer visible={visible} onClose={jest.fn()} closeLabel="Cerrar menú">
        <ModulText>Contenido</ModulText>
      </ModulDrawer>,
    );

    expect(screen.getByText('Contenido')).toBeOnTheScreen();
  });

  it('no muestra su contenido cuando no está visible', async () => {
    const visible = false;

    await renderWithTheme(
      <ModulDrawer visible={visible} onClose={jest.fn()} closeLabel="Cerrar menú">
        <ModulText>Contenido</ModulText>
      </ModulDrawer>,
    );

    expect(screen.queryByText('Contenido')).toBeNull();
  });

  it('se cierra al tocar la capa oscura', async () => {
    const onClose = jest.fn();
    await renderWithTheme(
      <ModulDrawer visible onClose={onClose} closeLabel="Cerrar menú">
        <ModulText>Contenido</ModulText>
      </ModulDrawer>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Cerrar menú' }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
