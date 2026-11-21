import { renderHook } from '@testing-library/react-native';
import { useAppBootstrap } from './useAppBootstrap';

function createOptions(signedIn: boolean) {
  const stopConnectivity = jest.fn();

  return {
    stopConnectivity,
    options: {
      signedIn,
      restoreSession: jest.fn(() => Promise.resolve()),
      startConnectivity: jest.fn(() => stopConnectivity),
      onSignedOut: jest.fn(),
    },
  };
}

describe('useAppBootstrap', () => {
  it('restaura la sesión una sola vez al montar', async () => {
    const { options } = createOptions(true);

    await renderHook(() => useAppBootstrap(options));

    expect(options.restoreSession).toHaveBeenCalledTimes(1);
  });

  it('limpia los datos cuando no hay sesión iniciada', async () => {
    const { options } = createOptions(false);

    await renderHook(() => useAppBootstrap(options));

    expect(options.onSignedOut).toHaveBeenCalledTimes(1);
  });

  it('no limpia los datos mientras hay sesión iniciada', async () => {
    const { options } = createOptions(true);

    await renderHook(() => useAppBootstrap(options));

    expect(options.onSignedOut).not.toHaveBeenCalled();
  });

  it('limpia los datos al cerrar la sesión', async () => {
    const { options } = createOptions(true);
    const { rerender } = await renderHook((props: typeof options) => useAppBootstrap(props), {
      initialProps: options,
    });

    await rerender({ ...options, signedIn: false });

    expect(options.onSignedOut).toHaveBeenCalledTimes(1);
  });

  it('detiene el monitor de conectividad al desmontar', async () => {
    const { options, stopConnectivity } = createOptions(true);
    const { unmount } = await renderHook(() => useAppBootstrap(options));

    await unmount();

    expect(stopConnectivity).toHaveBeenCalledTimes(1);
  });
});
