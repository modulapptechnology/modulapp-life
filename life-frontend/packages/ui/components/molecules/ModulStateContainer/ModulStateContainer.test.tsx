import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulStateContainer } from './ModulStateContainer';

const textos = { emptyTitle: 'Sin tareas', loadingMessage: 'Cargando tareas' };

describe('ModulStateContainer', () => {
  it('muestra el contenido cuando no hay carga, error ni vacío', async () => {
    const contenido = <ModulText>Mis tareas</ModulText>;

    await renderWithTheme(<ModulStateContainer {...textos}>{contenido}</ModulStateContainer>);

    expect(screen.getByText('Mis tareas')).toBeOnTheScreen();
  });

  it('muestra la carga antes que el error y el vacío', async () => {
    const estado = { loading: true, error: 'Falló', empty: true };

    await renderWithTheme(
      <ModulStateContainer {...textos} {...estado}>
        <ModulText>Mis tareas</ModulText>
      </ModulStateContainer>,
    );

    expect(screen.getByRole('progressbar', { name: 'Cargando tareas' })).toBeOnTheScreen();
    expect(screen.queryByText('Mis tareas')).toBeNull();
  });

  it('muestra el error y el reintento invoca la función recibida', async () => {
    const onRetry = jest.fn();
    const error = { error: 'No se pudo cargar', retryLabel: 'Reintentar', onRetry };
    await renderWithTheme(
      <ModulStateContainer {...textos} {...error}>
        <ModulText>Mis tareas</ModulText>
      </ModulStateContainer>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Reintentar' }));

    expect(onRetry).toHaveBeenCalledTimes(1);
  });

  it('muestra el vacío con su acción cuando la consulta no trae datos', async () => {
    const onEmptyAction = jest.fn();
    const vacio = { empty: true, emptyActionLabel: 'Nueva tarea', onEmptyAction };
    await renderWithTheme(
      <ModulStateContainer {...textos} {...vacio}>
        <ModulText>Mis tareas</ModulText>
      </ModulStateContainer>,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Nueva tarea' }));

    expect(screen.getByText('Sin tareas')).toBeOnTheScreen();
    expect(onEmptyAction).toHaveBeenCalledTimes(1);
  });
});
