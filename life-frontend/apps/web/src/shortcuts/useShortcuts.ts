import { useEffect, useRef } from 'react';
import { matchShortcut, type ShortcutEvent, type ShortcutId } from '@life/core';

// Solo web y escritorio: escucha las teclas del documento y avisa qué atajo se pulsó.
export function useShortcuts(onShortcut: (id: ShortcutId) => void, enabled: boolean) {
  const handlerRef = useRef(onShortcut);

  useEffect(() => {
    handlerRef.current = onShortcut;
  }, [onShortcut]);

  useEffect(() => {
    if (!enabled) return;

    const onKeyDown = (event: KeyboardEvent) => {
      const id = matchShortcut({
        key: event.key,
        ctrlKey: event.ctrlKey,
        metaKey: event.metaKey,
        altKey: event.altKey,
        target: event.target as ShortcutEvent['target'],
      });
      if (!id) return;

      event.preventDefault();
      handlerRef.current(id);
    };
    window.addEventListener('keydown', onKeyDown);

    return () => window.removeEventListener('keydown', onKeyDown);
  }, [enabled]);
}
