import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulFloatingButton } from './ModulFloatingButton';

describe('ModulFloatingButton', () => {
  it('llama a onPress al presionarlo', async () => {
    const onPress = jest.fn();
    await renderWithTheme(
      <ModulFloatingButton icon="CloseCircleIcon" accessibilityLabel="Agregar" onPress={onPress} />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Agregar' }));

    expect(onPress).toHaveBeenCalledTimes(1);
  });
});
