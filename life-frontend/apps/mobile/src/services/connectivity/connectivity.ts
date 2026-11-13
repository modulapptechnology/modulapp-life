import {
  connectivityStore,
  createConnectivityMonitor,
  createServerProbe,
  type ConnectivityMonitor,
} from '@life/core';
import { addNetworkStateListener, getNetworkStateAsync } from 'expo-network';
import { apiClient } from '../apiClient/apiClient';

// Si el sistema no sabe el estado de la red, se asume disponible y decide la comprobación al servidor.
export const connectivityMonitor: ConnectivityMonitor = createConnectivityMonitor({
  store: connectivityStore,
  probe: createServerProbe(apiClient),
  watchNetwork: (onChange) => {
    const subscription = addNetworkStateListener((state) => onChange(state.isConnected !== false));
    getNetworkStateAsync()
      .then((state) => onChange(state.isConnected !== false))
      .catch(() => undefined);

    return () => subscription.remove();
  },
});
