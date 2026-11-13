import {
  connectivityStore,
  createConnectivityMonitor,
  createServerProbe,
  type ConnectivityMonitor,
} from '@life/core';
import { apiClient } from '../apiClient/apiClient';

export const connectivityMonitor: ConnectivityMonitor = createConnectivityMonitor({
  store: connectivityStore,
  probe: createServerProbe(apiClient),
  watchNetwork: (onChange) => {
    const notify = () => onChange(navigator.onLine);
    window.addEventListener('online', notify);
    window.addEventListener('offline', notify);
    notify();

    return () => {
      window.removeEventListener('online', notify);
      window.removeEventListener('offline', notify);
    };
  },
});
