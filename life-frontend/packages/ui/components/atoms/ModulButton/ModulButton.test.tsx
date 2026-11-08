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

  it('se bloquea y se anuncia como ocupado mientras carga', async () => {
    const onPress = jest.fn();
    await renderWithTheme(<ModulButton label="Guardar" loading onPress={onPress} />);

    await fireEvent.press(screen.getByRole('button', { name: 'Guardar' }));

    expect(onPress).not.toHaveBeenCalled();
    expect(screen.getByRole('button', { name: 'Guardar' })).toBeBusy();
  });

  it('sigue respondiendo a onPress en los estados de éxito y de error', async () => {
    const onPress = jest.fn();
    await renderWithTheme(
      <>
        <ModulButton label="Enviado" success onPress={onPress} />
        <ModulButton label="Fallido" error onPress={onPress} />
      </>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Enviado' }));
    await fireEvent.press(screen.getByRole('button', { name: 'Fallido' }));

    expect(onPress).toHaveBeenCalledTimes(2);
  });

  it('se dibuja en tema oscuro con cada estado visual', async () => {
    const estados = { loading: true, success: true, error: true };

    await renderWithTheme(<ModulButton label="Estado" {...estados} />, 'dark');

    expect(screen.getByRole('button', { name: 'Estado' })).toBeOnTheScreen();
  });
});
