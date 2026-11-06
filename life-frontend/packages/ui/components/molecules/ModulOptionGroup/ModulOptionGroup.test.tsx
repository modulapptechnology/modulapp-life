import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulOptionGroup } from './ModulOptionGroup';

const opciones = [
  { value: 'alta', label: 'Alta' },
  { value: 'baja', label: 'Baja' },
];

describe('ModulOptionGroup', () => {
  it('llama a onChange con el valor de la opción presionada', async () => {
    const onChange = jest.fn();
    await renderWithTheme(
      <ModulOptionGroup label="Prioridad" options={opciones} value="alta" onChange={onChange} />,
    );

    await fireEvent.press(screen.getByRole('radio', { name: 'Baja' }));

    expect(onChange).toHaveBeenCalledWith('baja');
  });

  it('marca como elegida solo la opción del valor actual', async () => {
    const valor = 'baja';

    await renderWithTheme(
      <ModulOptionGroup label="Prioridad" options={opciones} value={valor} onChange={jest.fn()} />,
    );

    expect(screen.getByRole('radio', { name: 'Baja' })).toBeChecked();
    expect(screen.getByRole('radio', { name: 'Alta' })).not.toBeChecked();
  });

  it('no llama a onChange cuando el grupo está deshabilitado', async () => {
    const onChange = jest.fn();
    await renderWithTheme(
      <ModulOptionGroup
        label="Prioridad"
        options={opciones}
        value="alta"
        disabled
        onChange={onChange}
      />,
    );

    await fireEvent.press(screen.getByRole('radio', { name: 'Baja' }));

    expect(onChange).not.toHaveBeenCalled();
  });

  it('muestra el error del grupo', async () => {
    const error = 'Elige una prioridad';

    await renderWithTheme(
      <ModulOptionGroup
        label="Prioridad"
        options={opciones}
        value=""
        error={error}
        onChange={jest.fn()}
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Elige una prioridad');
  });
});
