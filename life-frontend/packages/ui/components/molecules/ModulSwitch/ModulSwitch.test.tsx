import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulSwitch } from './ModulSwitch';

describe('ModulSwitch', () => {
  it('entrega el nuevo valor al cambiar el interruptor', async () => {
    const onChange = jest.fn();
    await renderWithTheme(
      <ModulSwitch label="Recordatorios" checked={false} onChange={onChange} />,
    );

    await fireEvent(screen.getByLabelText('Recordatorios'), 'valueChange', true);

    expect(onChange).toHaveBeenCalledWith(true);
  });

  it('muestra el interruptor encendido cuando checked es verdadero', async () => {
    const encendido = true;

    await renderWithTheme(
      <ModulSwitch label="Recordatorios" checked={encendido} onChange={jest.fn()} />,
    );

    expect(screen.getByLabelText('Recordatorios').props.value).toBe(true);
  });

  it('muestra el error con rol de alerta', async () => {
    const error = 'Requerido';

    await renderWithTheme(
      <ModulSwitch label="Recordatorios" checked={false} error={error} onChange={jest.fn()} />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Requerido');
  });
});
