import { act, renderHook } from '@testing-library/react-native';
import { createModuleStateStore } from '../moduleStateStore/moduleStateStore';
import { useModuleState } from './useModuleState';

describe('useModuleState', () => {
  it('devuelve el valor inicial cuando el módulo no tiene estado guardado', async () => {
    const store = createModuleStateStore();

    const { result } = await renderHook(() => useModuleState(store, 'p:tareas', 'vacio'));

    expect(result.current[0]).toBe('vacio');
  });

  it('muestra el valor nuevo después de guardarlo', async () => {
    const store = createModuleStateStore();
    const { result } = await renderHook(() => useModuleState(store, 'p:tareas', ''));

    await act(async () => result.current[1]('hoy'));

    expect(result.current[0]).toBe('hoy');
  });

  it('conserva el estado cuando la pantalla se desmonta y se vuelve a abrir', async () => {
    const store = createModuleStateStore();
    const primera = await renderHook(() => useModuleState(store, 'p:tareas', ''));
    await act(async () => primera.result.current[1]('hoy'));
    await primera.unmount();

    const segunda = await renderHook(() => useModuleState(store, 'p:tareas', ''));

    expect(segunda.result.current[0]).toBe('hoy');
  });

  it('no muestra el estado de otro espacio para el mismo módulo', async () => {
    const store = createModuleStateStore();
    store.set('p:tareas', 'hoy');

    const { result } = await renderHook(() => useModuleState(store, 'c:tareas', 'vacio'));

    expect(result.current[0]).toBe('vacio');
  });
});
