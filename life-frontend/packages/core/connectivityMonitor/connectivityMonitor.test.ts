import { createConnectivityStore } from '../connectivityStore/connectivityStore';
import { createConnectivityMonitor } from './connectivityMonitor';

const INTERVAL = 10000;
const DEBOUNCE = 2000;

function setup(reachable = true) {
  const store = createConnectivityStore();
  const probe = jest.fn().mockResolvedValue(reachable);
  let emit: (available: boolean) => void = () => undefined;
  const stopWatching = jest.fn();
  const watchNetwork = jest.fn((onChange: (available: boolean) => void) => {
    emit = onChange;
    return stopWatching;
  });
  const monitor = createConnectivityMonitor({
    store,
    probe,
    watchNetwork,
    intervalMs: INTERVAL,
    debounceMs: DEBOUNCE,
  });

  return { store, probe, stopWatching, monitor, emit: (value: boolean) => emit(value) };
}

describe('connectivityMonitor', () => {
  beforeEach(() => jest.useFakeTimers());
  afterEach(() => jest.useRealTimers());

  it('detects the loss of the network after the debounce time', async () => {
    const { store, monitor, emit } = setup();
    monitor.start();

    emit(false);
    await jest.advanceTimersByTimeAsync(DEBOUNCE);

    expect(store.getState().online).toBe(false);
  });

  it('does not change the state when the loss is shorter than the debounce time', async () => {
    const { store, monitor, emit } = setup();
    monitor.start();

    emit(false);
    await jest.advanceTimersByTimeAsync(DEBOUNCE - 1);
    emit(true);
    await jest.advanceTimersByTimeAsync(DEBOUNCE * 2);

    expect(store.getState().online).toBe(true);
  });

  it('detects the recovery once the server answers again', async () => {
    const { store, probe, monitor, emit } = setup();
    monitor.start();
    emit(false);
    await jest.advanceTimersByTimeAsync(DEBOUNCE);

    emit(true);
    await jest.advanceTimersByTimeAsync(DEBOUNCE);

    expect(probe).toHaveBeenCalled();
    expect(store.getState().online).toBe(true);
  });

  it('is offline when there is network but the server is unreachable', async () => {
    const { store, monitor } = setup(false);

    monitor.start();
    await jest.advanceTimersByTimeAsync(DEBOUNCE);

    expect(store.getState().online).toBe(false);
  });

  it('treats a failing probe as unreachable', async () => {
    const { store, probe, monitor } = setup();
    probe.mockRejectedValue(new Error('boom'));

    monitor.start();
    await jest.advanceTimersByTimeAsync(DEBOUNCE);

    expect(store.getState().online).toBe(false);
  });

  it('checks the server again on every interval', async () => {
    const { probe, monitor } = setup();
    monitor.start();

    await jest.advanceTimersByTimeAsync(INTERVAL * 2);

    expect(probe).toHaveBeenCalledTimes(3);
  });

  it('stops watching and checking after stopping', async () => {
    const { probe, stopWatching, monitor } = setup();
    const stop = monitor.start();
    await jest.advanceTimersByTimeAsync(0);
    probe.mockClear();

    stop();
    await jest.advanceTimersByTimeAsync(INTERVAL * 2);

    expect(stopWatching).toHaveBeenCalledTimes(1);
    expect(probe).not.toHaveBeenCalled();
  });
});
