import { useEffect, useRef, useState } from 'react';
import { Animated } from 'react-native';
import { animateTo } from '../animations/animations';

export function useTransition(visible: boolean, onExited?: () => void) {
  const [mounted, setMounted] = useState(visible);
  const [progress] = useState(() => new Animated.Value(0));
  const exited = useRef(onExited);

  if (visible && !mounted) setMounted(true);

  useEffect(() => {
    exited.current = onExited;
  });

  useEffect(() => {
    if (!visible && !mounted) return;

    animateTo(progress, visible ? 1 : 0, {
      onEnd: ({ finished }) => {
        if (visible || !finished) return;
        setMounted(false);
        exited.current?.();
      },
    });

    return () => progress.stopAnimation();
  }, [visible, mounted, progress]);

  return { mounted, progress };
}
