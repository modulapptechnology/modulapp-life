import { Animated, Easing, Platform } from 'react-native';
import { layout } from '@life/theme';

//region Constantes
export const animationDurations = {
  fast: 150,
  standard: layout.slideDuration,
} as const;

export const animationEasings = {
  standard: Easing.out(Easing.cubic),
  soft: Easing.out(Easing.ease),
} as const;
//endregion Constantes

//region Timing
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

  Animated.timing(value, { toValue, duration, easing, useNativeDriver: nativeDriver }).start(onEnd);
};
//endregion Timing

//region Spring
export const springTo = (value: Animated.ValueXY, toValue: { x: number; y: number }) => {
  Animated.spring(value, { toValue, useNativeDriver: false }).start();
};
//endregion Spring
