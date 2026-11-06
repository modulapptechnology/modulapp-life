import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulButton } from './ModulButton';

describe('ModulButton', () => {
  it('llama a onPress al presionarlo', async () => {
    const onPress = jest.fn();
    await renderWithTheme(<ModulButton label="Guardar" onPress={onPress} />);

    await fireEvent.press(screen.getByRole('button', { name: 'Guardar' }));

    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('no llama a onPress cuando está deshabilitado', async () => {
    const onPress = jest.fn();
    await renderWithTheme(<ModulButton label="Guardar" disabled onPress={onPress} />);

    await fireEvent.press(screen.getByRole('button', { name: 'Guardar' }));

    expect(onPress).not.toHaveBeenCalled();
  });

  it('usa accessibilityLabel como nombre cuando es un botón solo de icono', async () => {
    const nombre = 'Cerrar';

    await renderWithTheme(<ModulButton icon="CloseCircleIcon" accessibilityLabel={nombre} />);

    expect(screen.getByRole('button', { name: 'Cerrar' })).toBeOnTheScreen();
  });
});
