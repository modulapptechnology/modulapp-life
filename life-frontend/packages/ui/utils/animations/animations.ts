import { useSyncExternalStore } from 'react';
import { AccessibilityInfo, Animated, Easing, Platform } from 'react-native';
import { layout } from '@life/theme';

export const animationDurations = {
  fast: 150,
  standard: layout.slideDuration,
  slow: 800,
} as const;

export const animationEasings = {
  standard: Easing.out(Easing.cubic),
  soft: Easing.out(Easing.ease),
} as const;

export const animationDistances = { enter: 16 } as const;
export const animationScales = { enter: 0.96 } as const;
export const animationOpacities = { pulseMin: 0.4 } as const;

let reduceMotionEnabled = false;
const reduceMotionListeners = new Set<() => void>();

const setReduceMotion = (enabled: boolean) => {
  reduceMotionEnabled = enabled;
  reduceMotionListeners.forEach((listener) => listener());
};

AccessibilityInfo.isReduceMotionEnabled()
  .then(setReduceMotion)
  .catch(() => undefined);
AccessibilityInfo.addEventListener('reduceMotionChanged', setReduceMotion);

export const isReduceMotionEnabled = () => reduceMotionEnabled;

const subscribeReduceMotion = (listener: () => void) => {
  reduceMotionListeners.add(listener);
  return () => {
    reduceMotionListeners.delete(listener);
  };
};

export const useReducedMotion = () =>
  useSyncExternalStore(subscribeReduceMotion, isReduceMotionEnabled, isReduceMotionEnabled);

export type TimingOptions = {
  duration?: number;
  easing?: (value: number) => number;
  nativeDriver?: boolean;
  onEnd?: (result: { finished: boolean }) => void;
};

export const animateTo = (value: Animated.Value, toValue: number, options: TimingOptions = {}) => {
  const {
    duration = animationDurations.standard,
    easing = animationEasings.standard,
    nativeDriver = Platform.OS !== 'web',
    onEnd,
  } = options;

  Animated.timing(value, {
    toValue,
    duration: reduceMotionEnabled ? 0 : duration,
    easing,
    useNativeDriver: nativeDriver,
  }).start(onEnd);
};

export const springTo = (value: Animated.ValueXY, toValue: { x: number; y: number }) => {
  if (reduceMotionEnabled) {
    value.setValue(toValue);
    return;
  }
  Animated.spring(value, { toValue, useNativeDriver: false }).start();
};

export const pulse = (value: Animated.Value) => {
  const step = (toValue: number) =>
    Animated.timing(value, {
      toValue,
      duration: animationDurations.slow,
      easing: animationEasings.soft,
      useNativeDriver: Platform.OS !== 'web',
    });

  return Animated.loop(Animated.sequence([step(animationOpacities.pulseMin), step(1)]));
};
