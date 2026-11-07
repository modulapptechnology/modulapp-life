import { fireEvent, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../../testing/renderWithTheme/renderWithTheme';
import { ModulDetailView } from '../ModulDetailView/ModulDetailView';
import { ModulListView } from './ModulListView';

describe('ModulListView', () => {
  it('muestra el título, los elementos y los filtros', async () => {
    const datos = ['Comprar pan', 'Pagar luz'];

    await renderWithTheme(
      <ModulListView
        title="Tareas"
        searchLabel="Buscar"
        searchValue=""
        onSearchChange={jest.fn()}
        filters={[{ value: 'all', label: 'Todas' }]}
        filterValue="all"
        onFilterChange={jest.fn()}
        data={datos}
        renderItem={({ item }) => <Text>{item}</Text>}
        emptyTitle="Sin tareas"
      />,
    );

    expect(screen.getByText('Tareas')).toBeOnTheScreen();
    expect(screen.getByText('Comprar pan')).toBeOnTheScreen();
    expect(screen.getByText('Todas')).toBeOnTheScreen();
  });

  it('muestra el estado vacío cuando no hay elementos', async () => {
    const datos: string[] = [];

    await renderWithTheme(
      <ModulListView
        title="Tareas"
        searchLabel="Buscar"
        searchValue=""
        onSearchChange={jest.fn()}
        data={datos}
        renderItem={({ item }) => <Text>{item}</Text>}
        emptyTitle="Sin tareas"
      />,
    );

    expect(screen.getByText('Sin tareas')).toBeOnTheScreen();
  });
});

describe('ModulDetailView', () => {
  it('muestra el contenido cuando está listo', async () => {
    const status = 'ready';

    await renderWithTheme(
      <ModulDetailView title="Detalle" status={status}>
        <Text>Descripción</Text>
      </ModulDetailView>,
    );

    expect(screen.getByText('Descripción')).toBeOnTheScreen();
  });

  it('llama a onRetry desde el estado de error', async () => {
    const onRetry = jest.fn();
    await renderWithTheme(
      <ModulDetailView
        title="Detalle"
        status="error"
        errorTitle="No se pudo cargar"
        retryLabel="Reintentar"
        onRetry={onRetry}
      >
        <Text>Descripción</Text>
      </ModulDetailView>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Reintentar' }));

    expect(onRetry).toHaveBeenCalledTimes(1);
  });
});
