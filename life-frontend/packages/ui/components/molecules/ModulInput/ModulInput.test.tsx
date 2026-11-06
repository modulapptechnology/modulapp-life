import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulInput } from './ModulInput';

describe('ModulInput', () => {
  it('muestra la etiqueta y entrega lo escrito con onChangeText', async () => {
    const onChangeText = jest.fn();
    await renderWithTheme(<ModulInput label="Nombre" value="" onChangeText={onChangeText} />);

    await fireEvent.changeText(screen.getByLabelText('Nombre'), 'Ana');

    expect(onChangeText).toHaveBeenCalledWith('Ana');
  });

  it('muestra el error del campo con rol de alerta', async () => {
    const error = 'Campo obligatorio';

    await renderWithTheme(<ModulInput label="Nombre" error={error} />);

    expect(screen.getByRole('alert')).toHaveTextContent('Campo obligatorio');
  });

  it('no permite escribir cuando está deshabilitado', async () => {
    const deshabilitado = true;

    await renderWithTheme(<ModulInput label="Nombre" disabled={deshabilitado} />);

    expect(screen.getByLabelText('Nombre').props.editable).toBe(false);
  });

  it('oculta la contraseña y la muestra al presionar el botón del campo', async () => {
    await renderWithTheme(<ModulInput type="password" label="Clave" actionLabel="Mostrar clave" />);
    expect(screen.getByLabelText('Clave').props.secureTextEntry).toBe(true);

    await fireEvent.press(screen.getByRole('button', { name: 'Mostrar clave' }));

    expect(screen.getByLabelText('Clave').props.secureTextEntry).toBe(false);
  });

  it('borra la búsqueda con el botón del campo', async () => {
    const onChangeText = jest.fn();
    await renderWithTheme(
      <ModulInput
        type="search"
        label="Buscar"
        value="tareas"
        actionLabel="Borrar búsqueda"
        onChangeText={onChangeText}
      />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Borrar búsqueda' }));

    expect(onChangeText).toHaveBeenCalledWith('');
  });

  it('muestra la opción elegida y entrega el valor de la opción que se selecciona', async () => {
    const onChangeText = jest.fn();
    const opciones = [
      { value: 'alta', label: 'Alta' },
      { value: 'baja', label: 'Baja' },
    ];
    await renderWithTheme(
      <ModulInput
        type="select"
        label="Prioridad"
        value="alta"
        options={opciones}
        onChangeText={onChangeText}
      />,
    );
    expect(screen.getByRole('combobox', { name: 'Prioridad' })).toHaveTextContent('Alta');

    await fireEvent.press(screen.getByRole('combobox', { name: 'Prioridad' }));
    await fireEvent.press(screen.getByRole('button', { name: 'Baja' }));

    expect(onChangeText).toHaveBeenCalledWith('baja');
  });
});
