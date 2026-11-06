import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulCheckbox } from './ModulCheckbox';

describe('ModulCheckbox', () => {
  it('invierte el valor al presionarlo cuando es casilla', async () => {
    const onChange = jest.fn();
    await renderWithTheme(<ModulCheckbox label="Acepto" checked={false} onChange={onChange} />);

    await fireEvent.press(screen.getByRole('checkbox', { name: 'Acepto' }));

    expect(onChange).toHaveBeenCalledWith(true);
  });

  it('entrega siempre true al presionar una opción de tipo radio', async () => {
    const onChange = jest.fn();
    await renderWithTheme(
      <ModulCheckbox type="radio" label="Opción A" checked onChange={onChange} />,
    );

    await fireEvent.press(screen.getByRole('radio', { name: 'Opción A' }));

    expect(onChange).toHaveBeenCalledWith(true);
  });

  it('no llama a onChange cuando está deshabilitado', async () => {
    const onChange = jest.fn();
    await renderWithTheme(
      <ModulCheckbox label="Acepto" checked={false} disabled onChange={onChange} />,
    );

    await fireEvent.press(screen.getByRole('checkbox', { name: 'Acepto' }));

    expect(onChange).not.toHaveBeenCalled();
  });

  it('muestra el mensaje de error', async () => {
    const error = 'Debes aceptar';

    await renderWithTheme(
      <ModulCheckbox label="Acepto" checked={false} error={error} onChange={jest.fn()} />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Debes aceptar');
  });
});
