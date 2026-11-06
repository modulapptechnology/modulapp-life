import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulEmptyState } from './ModulEmptyState';

describe('ModulEmptyState', () => {
  it('muestra el título y la descripción', async () => {
    const descripcion = 'Crea tu primera tarea';

    await renderWithTheme(<ModulEmptyState title="Sin tareas" description={descripcion} />);

    expect(screen.getByText('Sin tareas')).toBeOnTheScreen();
    expect(screen.getByText('Crea tu primera tarea')).toBeOnTheScreen();
  });

  it('no muestra el botón cuando falta la acción', async () => {
    const soloEtiqueta = { title: 'Sin tareas', actionLabel: 'Crear' };

    await renderWithTheme(<ModulEmptyState {...soloEtiqueta} />);

    expect(screen.queryByRole('button', { name: 'Crear' })).toBeNull();
  });

  it('llama a onAction al presionar el botón de acción', async () => {
    const onAction = jest.fn();
    await renderWithTheme(
      <ModulEmptyState title="Sin tareas" actionLabel="Crear" onAction={onAction} />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Crear' }));

    expect(onAction).toHaveBeenCalledTimes(1);
  });
});
