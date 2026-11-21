import { act, renderHook, waitFor } from '@testing-library/react-native';
import { useWorkspaceData } from './useWorkspaceData';

function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (reason: unknown) => void;
  const promise = new Promise<T>((res, rej) => {
    resolve = res;
    reject = rej;
  });

  return { promise, resolve, reject };
}

describe('useWorkspaceData', () => {
  it('carga los datos del espacio activo', async () => {
    const load = jest.fn((id: string) => Promise.resolve(`datos-${id}`));

    const { result } = await renderHook(() => useWorkspaceData('p', load));

    await waitFor(() => expect(result.current.data).toBe('datos-p'));
  });

  it('no carga nada cuando no hay espacio activo', async () => {
    const load = jest.fn((id: string) => Promise.resolve(id));

    const { result } = await renderHook(() => useWorkspaceData(null, load));

    expect(load).not.toHaveBeenCalled();
    expect(result.current.loading).toBe(false);
  });

  it('recarga y no muestra los datos del espacio anterior al cambiar de espacio', async () => {
    const load = jest.fn((id: string) => Promise.resolve(`datos-${id}`));
    const { result, rerender } = await renderHook(
      ({ id }: { id: string }) => useWorkspaceData(id, load),
      { initialProps: { id: 'p' } },
    );
    await waitFor(() => expect(result.current.data).toBe('datos-p'));

    await rerender({ id: 'c' });

    await waitFor(() => expect(result.current.data).toBe('datos-c'));
    expect(load).toHaveBeenCalledTimes(2);
  });

  it('ignora la respuesta tardía del espacio anterior', async () => {
    const lenta = deferred<string>();
    const rapida = deferred<string>();
    const load = jest.fn((id: string) => (id === 'p' ? lenta.promise : rapida.promise));
    const { result, rerender } = await renderHook(
      ({ id }: { id: string }) => useWorkspaceData(id, load),
      { initialProps: { id: 'p' } },
    );
    await rerender({ id: 'c' });

    await act(async () => {
      rapida.resolve('datos-c');
      lenta.resolve('datos-p');
    });

    expect(result.current.data).toBe('datos-c');
  });

  it('marca el error cuando la carga falla', async () => {
    const load = jest.fn(() => Promise.reject(new Error('falla')));

    const { result } = await renderHook(() => useWorkspaceData('p', load));

    await waitFor(() => expect(result.current.error).toBe(true));
  });
});
