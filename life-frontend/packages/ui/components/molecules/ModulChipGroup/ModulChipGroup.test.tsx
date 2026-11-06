import { fireEvent, screen } from '@testing-library/react-native';
import { collectRoles } from '../../../testing/accessibilityRoles/accessibilityRoles';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulChipGroup } from './ModulChipGroup';

const opciones = [
  { value: 'hoy', label: 'Hoy' },
  { value: 'semana', label: 'Semana' },
];

describe('ModulChipGroup', () => {
  it('llama a onChange con el valor de la opción presionada', async () => {
    const onChange = jest.fn();
    await renderWithTheme(<ModulChipGroup options={opciones} value="hoy" onChange={onChange} />);

    await fireEvent.press(screen.getByRole('tab', { name: 'Semana' }));

    expect(onChange).toHaveBeenCalledWith('semana');
  });

  it('marca como seleccionada solo la opción del valor actual', async () => {
    const valor = 'semana';

    await renderWithTheme(<ModulChipGroup options={opciones} value={valor} onChange={jest.fn()} />);

    expect(screen.getByRole('tab', { name: 'Semana' })).toBeSelected();
    expect(screen.getByRole('tab', { name: 'Hoy' })).not.toBeSelected();
  });

  it('dibuja las opciones como control segmentado cuando segmented es verdadero', async () => {
    const segmentado = true;

    await renderWithTheme(
      <ModulChipGroup options={opciones} value="hoy" segmented={segmentado} onChange={jest.fn()} />,
    );

    expect(collectRoles(screen.toJSON())).toContain('tablist');
  });
});
