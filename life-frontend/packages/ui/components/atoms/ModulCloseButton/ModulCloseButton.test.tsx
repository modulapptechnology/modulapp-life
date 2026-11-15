import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulCloseButton } from './ModulCloseButton';

describe('ModulCloseButton', () => {
  it('expone rol de botón con su nombre accesible y llama a onPress', async () => {
    const onPress = jest.fn();
    await renderWithTheme(<ModulCloseButton label="Cerrar" onPress={onPress} />);

    await fireEvent.press(screen.getByRole('button', { name: 'Cerrar' }));

    expect(onPress).toHaveBeenCalledTimes(1);
  });
});
