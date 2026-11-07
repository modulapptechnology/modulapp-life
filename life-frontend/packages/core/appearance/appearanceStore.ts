import type { ThemeAccent, ThemePreference } from '@life/types';

export type AppearanceState = { preference: ThemePreference; accent: ThemeAccent };

// Lo mínimo que necesita el store de un almacenamiento (localStorage lo cumple tal cual).
export type AppearanceStorage = {
  getItem: (key: string) => string | null;
  setItem: (key: string, value: string) => void;
};

export type AppearanceStore = {
  getState: () => AppearanceState;
  setPreference: (preference: ThemePreference) => void;
  setAccent: (accent: ThemeAccent) => void;
  subscribe: (listener: () => void) => () => void;
};

const STORAGE_KEY = 'life.appearance';
const DEFAULT_STATE: AppearanceState = { preference: 'system', accent: 'modul' };
const PREFERENCES: ThemePreference[] = ['light', 'dark', 'system'];
const ACCENTS: ThemeAccent[] = ['modul', 'violet', 'ocean'];

function readState(storage?: AppearanceStorage): AppearanceState {
  try {
    const saved = JSON.parse(storage?.getItem(STORAGE_KEY) ?? 'null');
    return {
      preference: PREFERENCES.includes(saved?.preference)
        ? saved.preference
        : DEFAULT_STATE.preference,
      accent: ACCENTS.includes(saved?.accent) ? saved.accent : DEFAULT_STATE.accent,
    };
  } catch {
    return DEFAULT_STATE;
  }
}

export function createAppearanceStore(storage?: AppearanceStorage): AppearanceStore {
  let state = readState(storage);
  const listeners = new Set<() => void>();

  function update(next: Partial<AppearanceState>) {
    state = { ...state, ...next };
    try {
      storage?.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch {
      // Sin almacenamiento disponible: el cambio sigue valiendo en memoria.
    }
    listeners.forEach((listener) => listener());
  }

  return {
    getState: () => state,
    setPreference: (preference) => update({ preference }),
    setAccent: (accent) => update({ accent }),
    subscribe(listener) {
      listeners.add(listener);
      return () => listeners.delete(listener);
    },
  };
}

// Web: guarda en localStorage. En mobile queda en memoria hasta LIF-81.
export const appearanceStore = createAppearanceStore(
  typeof localStorage !== 'undefined' ? localStorage : undefined,
);
