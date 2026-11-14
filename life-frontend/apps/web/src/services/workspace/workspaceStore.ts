import { createWorkspaceStore } from '@life/core';

const storage = {
  get: (key: string) => Promise.resolve(localStorage.getItem(key)),
  set: (key: string, value: string) => Promise.resolve(localStorage.setItem(key, value)),
  remove: (key: string) => Promise.resolve(localStorage.removeItem(key)),
};

export const workspaceStore = createWorkspaceStore(storage);
