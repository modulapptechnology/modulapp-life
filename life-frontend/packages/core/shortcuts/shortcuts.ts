export type ShortcutId = 'newCapture' | 'openSettings' | 'showShortcuts';

export type ShortcutDefinition = { id: ShortcutId; key: string; labelKey: string };

// Teclas simples (sin Ctrl, Cmd ni Alt) para no chocar con los atajos del navegador ni del sistema.
export const SHORTCUTS: ShortcutDefinition[] = [
  { id: 'newCapture', key: 'n', labelKey: 'menu.newCapture' },
  { id: 'openSettings', key: ',', labelKey: 'user.settings' },
  { id: 'showShortcuts', key: '?', labelKey: 'shortcuts.title' },
];

export type ShortcutEvent = {
  key: string;
  ctrlKey?: boolean;
  metaKey?: boolean;
  altKey?: boolean;
  target?: { tagName?: string; isContentEditable?: boolean } | null;
};

const TYPING_TAGS = ['INPUT', 'TEXTAREA', 'SELECT'];

// Devuelve el atajo que corresponde a la tecla, o null si lleva modificadores o se está escribiendo en un campo.
export function matchShortcut(event: ShortcutEvent): ShortcutId | null {
  if (event.ctrlKey || event.metaKey || event.altKey) return null;
  const target = event.target;
  if (target?.isContentEditable || TYPING_TAGS.includes(target?.tagName ?? '')) return null;

  const key = event.key.toLowerCase();

  return SHORTCUTS.find((shortcut) => shortcut.key === key)?.id ?? null;
}
