import { createConnectivityStore, resolveConnectivityStatus } from './connectivityStore';

describe('connectivityStore', () => {
  it('starts online so the interface does not flash offline on startup', () => {
    const store = createConnectivityStore();

    const state = store.getState();

    expect(state.online).toBe(true);
  });

  it('notifies subscribers only when the state really changes', () => {
    const store = createConnectivityStore();
    const listener = jest.fn();
    store.subscribe(listener);

    store.setOnline(true);
    store.setOnline(false);
    store.setOnline(false);

    expect(listener).toHaveBeenCalledTimes(1);
  });

  it('stops notifying after unsubscribing', () => {
    const store = createConnectivityStore();
    const listener = jest.fn();
    const unsubscribe = store.subscribe(listener);
    unsubscribe();

    store.setOnline(false);

    expect(listener).not.toHaveBeenCalled();
  });
});

describe('resolveConnectivityStatus', () => {
  it('is offline when there is no connection, even with pending operations', () => {
    const online = false;

    const status = resolveConnectivityStatus(online, 3);

    expect(status).toBe('offline');
  });

  it('is syncing when online with pending operations', () => {
    const online = true;

    const status = resolveConnectivityStatus(online, 1);

    expect(status).toBe('syncing');
  });

  it('is synced when online with no pending operations', () => {
    const online = true;

    const status = resolveConnectivityStatus(online, 0);

    expect(status).toBe('synced');
  });
});
