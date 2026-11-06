import { fireEvent, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulList } from './ModulList';

const tareas = [
  { id: '1', titulo: 'Comprar pan' },
  { id: '2', titulo: 'Llamar a Ana' },
];

const lista = (props: Partial<React.ComponentProps<typeof ModulList<(typeof tareas)[number]>>>) => (
  <ModulList
    data={tareas}
    keyExtractor={(tarea) => tarea.id}
    renderItem={({ item }) => <Text>{item.titulo}</Text>}
    emptyTitle="Sin tareas"
    {...props}
  />
);

describe('ModulList', () => {
  it('dibuja cada elemento con renderItem', async () => {
    const datos = tareas;

    await renderWithTheme(lista({ data: datos }));

    expect(screen.getByText('Comprar pan')).toBeOnTheScreen();
    expect(screen.getByText('Llamar a Ana')).toBeOnTheScreen();
  });

  it('muestra el estado vacío cuando no hay datos', async () => {
    const sinDatos: typeof tareas = [];

    await renderWithTheme(lista({ data: sinDatos }));

    expect(screen.getByText('Sin tareas')).toBeOnTheScreen();
  });

  it('muestra el estado de carga en lugar de la lista', async () => {
    const cargando = true;

    await renderWithTheme(lista({ loading: cargando, loadingMessage: 'Cargando' }));

    expect(screen.getByText('Cargando')).toBeOnTheScreen();
    expect(screen.queryByText('Comprar pan')).toBeNull();
  });

  it('muestra el error y reintenta con onRetry', async () => {
    const onRetry = jest.fn();
    await renderWithTheme(lista({ error: 'Sin conexión', retryLabel: 'Reintentar', onRetry }));

    await fireEvent.press(screen.getByRole('button', { name: 'Reintentar' }));

    expect(onRetry).toHaveBeenCalledTimes(1);
  });
});
