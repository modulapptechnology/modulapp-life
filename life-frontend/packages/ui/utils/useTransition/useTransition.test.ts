import { act, renderHook } from '@testing-library/react-native';
import { animateTo } from '../animations/animations';
import { useTransition } from './useTransition';

jest.mock('../animations/animations', () => ({
  animateTo: jest.fn(),
}));

const mockedAnimateTo = jest.mocked(animateTo);
const terminarAnimacion = async (finished: boolean) => {
  await act(async () => {
    mockedAnimateTo.mock.lastCall?.[2]?.onEnd?.({ finished });
  });
};

describe('useTransition', () => {
  beforeEach(() => {
    mockedAnimateTo.mockClear();
  });

  it('arranca montado cuando empieza visible', async () => {
    const visible = true;

    const { result } = await renderHook(() => useTransition(visible));

    expect(result.current.mounted).toBe(true);
  });

  it('arranca desmontado y sin animar cuando empieza oculto', async () => {
    const visible = false;

    const { result } = await renderHook(() => useTransition(visible));

    expect(result.current.mounted).toBe(false);
    expect(mockedAnimateTo).not.toHaveBeenCalled();
  });

  it('anima el progreso hacia 1 al mostrarse', async () => {
    const visible = true;

    const { result } = await renderHook(() => useTransition(visible));

    expect(mockedAnimateTo).toHaveBeenCalledWith(result.current.progress, 1, expect.any(Object));
  });

  it('monta al pasar de oculto a visible', async () => {
    const { result, rerender } = await renderHook(
      ({ visible }: { visible: boolean }) => useTransition(visible),
      {
        initialProps: { visible: false },
      },
    );

    await rerender({ visible: true });

    expect(result.current.mounted).toBe(true);
    expect(mockedAnimateTo).toHaveBeenLastCalledWith(
      result.current.progress,
      1,
      expect.any(Object),
    );
  });

  it('sigue montado mientras la animación de salida no termina', async () => {
    const { result, rerender } = await renderHook(
      ({ visible }: { visible: boolean }) => useTransition(visible),
      {
        initialProps: { visible: true },
      },
    );

    await rerender({ visible: false });

    expect(result.current.mounted).toBe(true);
    expect(mockedAnimateTo).toHaveBeenLastCalledWith(
      result.current.progress,
      0,
      expect.any(Object),
    );
  });

  it('se desmonta y llama a onExited cuando termina la salida', async () => {
    const onExited = jest.fn();
    const { result, rerender } = await renderHook(
      ({ visible }: { visible: boolean }) => useTransition(visible, onExited),
      { initialProps: { visible: true } },
    );
    await rerender({ visible: false });

    await terminarAnimacion(true);

    expect(result.current.mounted).toBe(false);
    expect(onExited).toHaveBeenCalledTimes(1);
  });

  it('no se desmonta ni llama a onExited si la salida se interrumpe', async () => {
    const onExited = jest.fn();
    const { result, rerender } = await renderHook(
      ({ visible }: { visible: boolean }) => useTransition(visible, onExited),
      { initialProps: { visible: true } },
    );
    await rerender({ visible: false });

    await terminarAnimacion(false);

    expect(result.current.mounted).toBe(true);
    expect(onExited).not.toHaveBeenCalled();
  });

  it('no llama a onExited al terminar la animación de entrada', async () => {
    const onExited = jest.fn();
    await renderHook(() => useTransition(true, onExited));

    await terminarAnimacion(true);

    expect(onExited).not.toHaveBeenCalled();
  });
});
