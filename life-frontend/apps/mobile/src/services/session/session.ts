import { createSecureSessionStorage, createSessionServices } from '@life/core';
import * as SecureStore from 'expo-secure-store';
import { env } from '../../config/env';
import { workspaceStore } from '../workspace/workspaceStore';

const options: SecureStore.SecureStoreOptions = {
  keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
};

const storage = createSecureSessionStorage({
  get: (key) => SecureStore.getItemAsync(key, options),
  set: (key, value) => SecureStore.setItemAsync(key, value, options),
  remove: (key) => SecureStore.deleteItemAsync(key, options),
});

export const { manager: sessionManager, apiClient } = createSessionServices({
  getWorkspaceId: () => workspaceStore.getState().activeId,
  baseUrl: env.apiUrl,
  storage,
  delivery: 'body',
});
