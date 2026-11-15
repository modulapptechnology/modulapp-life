import type { SessionUser } from '@life/types';

export type SessionState = {
  user: SessionUser | null;
  pendingOperations: number;
  restoring: boolean;
  entering: boolean;
};

export type SessionStore = {
  getState: () => SessionState;
  signIn: (user: SessionUser) => void;
  signOut: () => void;
  expireSession: () => void;
  setPendingOperations: (count: number) => void;
  finishRestore: () => void;
  startEntering: () => void;
  finishEntering: () => void;
  subscribe: (listener: () => void) => () => void;
};

export function createSessionStore(initial: Partial<SessionState> = {}): SessionStore {
  let state: SessionState = {
    user: null,
    pendingOperations: 0,
    restoring: false,
    entering: false,
    ...initial,
  };
  const listeners = new Set<() => void>();

  function update(next: Partial<SessionState>) {
    state = { ...state, ...next };
    listeners.forEach((listener) => listener());
  }

  return {
    getState: () => state,
    signIn: (user) => update({ user }),
    signOut: () => update({ user: null, pendingOperations: 0 }),
    expireSession: () => update({ user: null }),
    setPendingOperations: (count) => update({ pendingOperations: Math.max(0, count) }),
    finishRestore: () => update({ restoring: false }),
    startEntering: () => update({ entering: true }),
    finishEntering: () => update({ entering: false }),
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
  };
}

export const session = createSessionStore({ restoring: true });
