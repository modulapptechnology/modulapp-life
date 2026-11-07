import type { SessionUser } from '@life/types';

export type SessionState = {
  user: SessionUser | null;
  pendingOperations: number;
};

export type SessionStore = {
  getState: () => SessionState;
  signIn: (user: SessionUser) => void;
  signOut: () => void;
  setPendingOperations: (count: number) => void;
  subscribe: (listener: () => void) => () => void;
};

export function createSessionStore(initial: Partial<SessionState> = {}): SessionStore {
  let state: SessionState = { user: null, pendingOperations: 0, ...initial };
  const listeners = new Set<() => void>();

  function update(next: Partial<SessionState>) {
    state = { ...state, ...next };
    listeners.forEach((listener) => listener());
  }

  return {
    getState: () => state,
    signIn: (user) => update({ user }),
    signOut: () => update({ user: null, pendingOperations: 0 }),
    setPendingOperations: (count) => update({ pendingOperations: Math.max(0, count) }),
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

export const session = createSessionStore({ user: { id: 'demo', name: 'Usuario Demo' } });
