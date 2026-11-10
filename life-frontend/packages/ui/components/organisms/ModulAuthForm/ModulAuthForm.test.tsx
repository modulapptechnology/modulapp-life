import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulAuthForm, type ModulAuthFormField } from './ModulAuthForm';

const campos: ModulAuthFormField[] = [
  { name: 'email', label: 'Correo', value: '', onChangeText: jest.fn() },
  { name: 'password', type: 'password', label: 'Contraseña', value: '', onChangeText: jest.fn() },
];

describe('ModulAuthForm', () => {
  it('muestra el título, los campos y llama a onSubmit al enviar', async () => {
    const onSubmit = jest.fn();
    await renderWithTheme(
      <ModulAuthForm
        title="Iniciar sesión"
        fields={campos}
        submitLabel="Ingresar"
        onSubmit={onSubmit}
      />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Ingresar' }));

    expect(screen.getByText('Iniciar sesión')).toBeOnTheScreen();
    expect(screen.getByLabelText('Correo')).toBeOnTheScreen();
    expect(screen.getByLabelText('Contraseña')).toBeOnTheScreen();
    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  it('muestra el error del servidor con rol de alerta', async () => {
    const error = 'El correo o la contraseña no son correctos.';

    await renderWithTheme(
      <ModulAuthForm
        title="Iniciar sesión"
        fields={campos}
        submitLabel="Ingresar"
        onSubmit={jest.fn()}
        serverError={error}
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent(error);
  });

  it('bloquea el botón mientras se envía', async () => {
    const onSubmit = jest.fn();
    await renderWithTheme(
      <ModulAuthForm
        title="Iniciar sesión"
        fields={campos}
        submitLabel="Ingresar"
        onSubmit={onSubmit}
        submitting
      />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Ingresar' }));

    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('reemplaza el formulario por el mensaje de resultado y su acción', async () => {
    const onAction = jest.fn();
    const result = {
      message: 'Tu correo quedó confirmado.',
      actionLabel: 'Iniciar sesión',
      onAction,
    };
    await renderWithTheme(
      <ModulAuthForm
        title="Confirma tu correo"
        fields={campos}
        submitLabel="Confirmar"
        onSubmit={jest.fn()}
        result={result}
      />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Iniciar sesión' }));

    expect(screen.getByText('Tu correo quedó confirmado.')).toBeOnTheScreen();
    expect(screen.queryByLabelText('Correo')).toBeNull();
    expect(onAction).toHaveBeenCalledTimes(1);
  });
});

describe('ModulAuthForm con enlaces y proveedores', () => {
  it('muestra los enlaces y los proveedores y llama a sus acciones', async () => {
    const onAction = jest.fn();
    const onPress = jest.fn();
    await renderWithTheme(
      <ModulAuthForm
        title="Iniciar sesión"
        fields={campos}
        submitLabel="Ingresar"
        onSubmit={jest.fn()}
        links={[{ text: 'No tengo cuenta', actionLabel: 'Crearme cuenta', onAction }]}
        providers={{ label: 'Ingresar con', items: [{ label: 'Google', onPress }] }}
      />,
    );

    await fireEvent.press(screen.getByText('Crearme cuenta'));
    await fireEvent.press(screen.getByRole('button', { name: 'Google' }));

    expect(screen.getByText('No tengo cuenta')).toBeOnTheScreen();
    expect(screen.getByText('Ingresar con')).toBeOnTheScreen();
    expect(onAction).toHaveBeenCalledTimes(1);
    expect(onPress).toHaveBeenCalledTimes(1);
  });
});
