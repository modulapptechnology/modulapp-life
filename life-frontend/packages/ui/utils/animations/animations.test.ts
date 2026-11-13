import { Animated } from 'react-native';
import { animateTo, animationDurations, springTo } from './animations';

// Las animaciones usan temporizadores: se simulan y se descartan para que no sigan tras terminar la prueba.
beforeEach(() => jest.useFakeTimers());
afterEach(() => {
  jest.clearAllTimers();
  jest.useRealTimers();
});

describe('animateTo', () => {
  it('anima el valor hasta el destino con la duración estándar', () => {
    const timing = jest.spyOn(Animated, 'timing');
    const value = new Animated.Value(0);

    animateTo(value, 1);

    expect(timing).toHaveBeenCalledWith(
      value,
      expect.objectContaining({ toValue: 1, duration: animationDurations.standard }),
    );
  });

  it('usa la duración, el driver y el callback que se le indican', () => {
    const timing = jest.spyOn(Animated, 'timing');
    const value = new Animated.Value(0);
    const onEnd = jest.fn();

    animateTo(value, 0, { duration: animationDurations.fast, nativeDriver: false, onEnd });

    expect(timing).toHaveBeenLastCalledWith(
      value,
      expect.objectContaining({ duration: 150, useNativeDriver: false }),
    );
  });
});

describe('springTo', () => {
  it('lleva el valor de arrastre al destino con un resorte', () => {
    const spring = jest.spyOn(Animated, 'spring');
    const value = new Animated.ValueXY();

    springTo(value, { x: 0, y: 0 });

    expect(spring).toHaveBeenCalledWith(
      value,
      expect.objectContaining({ toValue: { x: 0, y: 0 }, useNativeDriver: false }),
    );
  });
});
