import { fireEvent, screen } from '@testing-library/react-native';
import { Text } from 'react-native';
import { renderWithTheme } from '../../../../testing/renderWithTheme/renderWithTheme';
import { ModulAgendaViewWeb } from '../ModulAgendaViewWeb/ModulAgendaViewWeb';
import { ModulDetailViewWeb } from '../ModulDetailViewWeb/ModulDetailViewWeb';
import { ModulFormViewWeb } from '../ModulFormViewWeb/ModulFormViewWeb';
import { ModulSettingsViewWeb } from '../ModulSettingsViewWeb/ModulSettingsViewWeb';
import { ModulListViewWeb } from './ModulListViewWeb';

describe('ModulListViewWeb', () => {
  it('muestra el título, las acciones, los filtros y los elementos', async () => {
    const datos = ['Comprar pan', 'Pagar luz'];

    await renderWithTheme(
      <ModulListViewWeb
        title="Tareas"
        actions={<Text>Nueva tarea</Text>}
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
    expect(screen.getByText('Nueva tarea')).toBeOnTheScreen();
    expect(screen.getByText('Todas')).toBeOnTheScreen();
    expect(screen.getByText('Comprar pan')).toBeOnTheScreen();
  });
});

describe('ModulFormViewWeb', () => {
  it('llama a onSubmit al presionar el botón de enviar', async () => {
    const onSubmit = jest.fn();
    await renderWithTheme(
      <ModulFormViewWeb title="Nueva tarea" submitLabel="Guardar" onSubmit={onSubmit}>
        <Text>Campo uno</Text>
        <Text>Campo dos</Text>
      </ModulFormViewWeb>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Guardar' }));

    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(screen.getByText('Campo dos')).toBeOnTheScreen();
  });
});

describe('ModulDetailViewWeb', () => {
  it('muestra el contenido principal y el panel lateral', async () => {
    const aside = <Text>Pendiente</Text>;

    await renderWithTheme(
      <ModulDetailViewWeb title="Comprar pan" aside={aside}>
        <Text>Descripción</Text>
      </ModulDetailViewWeb>,
    );

    expect(screen.getByText('Descripción')).toBeOnTheScreen();
    expect(screen.getByText('Pendiente')).toBeOnTheScreen();
  });
});

describe('ModulSettingsViewWeb', () => {
  it('muestra la primera sección y cambia de sección al elegirla', async () => {
    const sections = [
      { id: 'a', title: 'Notificaciones', content: <Text>Contenido A</Text> },
      { id: 'b', title: 'Privacidad', content: <Text>Contenido B</Text> },
    ];
    await renderWithTheme(<ModulSettingsViewWeb title="Ajustes" sections={sections} />);

    await fireEvent.press(screen.getByRole('tab', { name: 'Privacidad' }));

    expect(screen.getByText('Contenido B')).toBeOnTheScreen();
    expect(screen.queryByText('Contenido A')).toBeNull();
  });
});

describe('ModulAgendaViewWeb', () => {
  it('muestra el título, la barra de herramientas y el contenido', async () => {
    const toolbar = <Text>Semana</Text>;

    await renderWithTheme(
      <ModulAgendaViewWeb title="Agenda" toolbar={toolbar}>
        <Text>Calendario</Text>
      </ModulAgendaViewWeb>,
    );

    expect(screen.getByText('Agenda')).toBeOnTheScreen();
    expect(screen.getByText('Semana')).toBeOnTheScreen();
    expect(screen.getByText('Calendario')).toBeOnTheScreen();
  });
});
