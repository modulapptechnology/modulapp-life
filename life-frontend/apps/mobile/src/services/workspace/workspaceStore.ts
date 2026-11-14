import { createWorkspaceStore } from '@life/core';
import * as SecureStore from 'expo-secure-store';

export const workspaceStore = createWorkspaceStore({
  get: (key) => SecureStore.getItemAsync(key),
  set: (key, value) => SecureStore.setItemAsync(key, value),
  remove: (key) => SecureStore.deleteItemAsync(key),
});
