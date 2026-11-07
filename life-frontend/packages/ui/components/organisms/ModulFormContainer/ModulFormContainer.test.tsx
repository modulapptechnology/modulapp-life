import { fireEvent, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulFormContainer, toFieldErrors } from './ModulFormContainer';

describe('ModulFormContainer', () => {
  it('muestra los errores del servidor como alerta', async () => {
    const errores = ['El correo ya está registrado'];

    await renderWithTheme(
      <ModulFormContainer submitLabel="Guardar" onSubmit={jest.fn()} serverErrors={errores}>
        <Text>Campos</Text>
      </ModulFormContainer>,
    );

    expect(screen.getByText('El correo ya está registrado')).toBeOnTheScreen();
    expect(screen.getByRole('alert')).toBeOnTheScreen();
  });

  it('llama a onSubmit al presionar el botón de enviar', async () => {
    const onSubmit = jest.fn();
    await renderWithTheme(
      <ModulFormContainer submitLabel="Guardar" onSubmit={onSubmit}>
        <Text>Campos</Text>
      </ModulFormContainer>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Guardar' }));

    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  it('no llama a onSubmit mientras se está enviando', async () => {
    const onSubmit = jest.fn();
    await renderWithTheme(
      <ModulFormContainer submitLabel="Guardar" onSubmit={onSubmit} submitting>
        <Text>Campos</Text>
      </ModulFormContainer>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Guardar' }));

    expect(onSubmit).not.toHaveBeenCalled();
  });
});

describe('toFieldErrors', () => {
  it('devuelve el primer mensaje de cada campo', () => {
    const errores = { Email: ['Inválido', 'Requerido'], Name: [] };

    const resultado = toFieldErrors(errores);

    expect(resultado).toEqual({ Email: 'Inválido' });
  });
});
