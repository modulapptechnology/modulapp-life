import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulChip } from './ModulChip';

describe('ModulChip', () => {
  it('llama a onPress al presionarlo', async () => {
    const onPress = jest.fn();
    await renderWithTheme(<ModulChip label="Hoy" onPress={onPress} />);

    await fireEvent.press(screen.getByRole('tab', { name: 'Hoy' }));

    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('marca el chip como seleccionado', async () => {
    const seleccionado = true;

    await renderWithTheme(<ModulChip label="Hoy" selected={seleccionado} />);

    expect(screen.getByRole('tab', { name: 'Hoy' })).toBeSelected();
  });

  it('muestra el botón de quitar cuando recibe onRemove y lo ejecuta', async () => {
    const onRemove = jest.fn();
    await renderWithTheme(<ModulChip label="Hoy" onRemove={onRemove} removeLabel="Quitar Hoy" />);

    await fireEvent.press(screen.getByRole('button', { name: 'Quitar Hoy' }));

    expect(onRemove).toHaveBeenCalledTimes(1);
  });
});
