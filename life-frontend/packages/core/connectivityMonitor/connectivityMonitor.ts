import type { ConnectivityStore } from '../connectivityStore/connectivityStore';

export const DEFAULT_CHECK_INTERVAL_MS = 15000;
// Un cambio de estado solo se publica si se mantiene este tiempo; evita el parpadeo de la interfaz.
export const DEFAULT_DEBOUNCE_MS = 2000;

export type ConnectivityMonitorOptions = {
  store: ConnectivityStore;
  // Responde si el servidor está alcanzable; no debe lanzar, pero si lo hace cuenta como no alcanzable.
  probe: () => Promise<boolean>;
  // Avisa de la pérdida y la recuperación de red del dispositivo; devuelve cómo dejar de escuchar.
  watchNetwork: (onChange: (available: boolean) => void) => () => void;
  intervalMs?: number;
  debounceMs?: number;
};

export type ConnectivityMonitor = {
  start: () => () => void;
};

export function createConnectivityMonitor({
  store,
  probe,
  watchNetwork,
  intervalMs = DEFAULT_CHECK_INTERVAL_MS,
  debounceMs = DEFAULT_DEBOUNCE_MS,
}: ConnectivityMonitorOptions): ConnectivityMonitor {
  return {
    start() {
      let networkAvailable = true;
      let checking = false;
      let pending: { value: boolean; timer: ReturnType<typeof setTimeout> } | null = null;

      function cancelPending() {
        if (pending) clearTimeout(pending.timer);
        pending = null;
      }

      function report(value: boolean) {
        if (value === store.getState().online) {
          cancelPending();
          return;
        }
        if (pending?.value === value) return;
        cancelPending();
        pending = {
          value,
          timer: setTimeout(() => {
            pending = null;
            store.setOnline(value);
          }, debounceMs),
        };
      }

      async function check() {
        if (!networkAvailable) {
          report(false);
          return;
        }
        if (checking) return;
        checking = true;
        try {
          const reachable = await probe();
          report(networkAvailable && reachable);
        } catch {
          report(false);
        } finally {
          checking = false;
        }
      }

      const stopWatching = watchNetwork((available) => {
        networkAvailable = available;
        void check();
      });
      const interval = setInterval(() => void check(), intervalMs);
      void check();

      return () => {
        stopWatching();
        clearInterval(interval);
        cancelPending();
      };
    },
  };
}
