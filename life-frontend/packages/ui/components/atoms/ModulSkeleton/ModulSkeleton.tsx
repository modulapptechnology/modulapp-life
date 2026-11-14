import { useEffect, useState } from 'react';
import { Animated, type DimensionValue } from 'react-native';
import { pulse, useReducedMotion } from '../../../utils/animations/animations';
import { ModulView } from '../ModulView/ModulView';

export type ModulSkeletonProps = {
  width?: DimensionValue;
  height?: number;
  // Forma circular, para avatares.
  circle?: boolean;
};

// Esqueleto de carga: bloque estático que reserva el espacio del contenido mientras llega. Es decorativo,
// por eso lo oculta a las tecnologías de asistencia; la carga la anuncia el contenedor que lo usa.
export function ModulSkeleton({ width = '100%', height = 16, circle = false }: ModulSkeletonProps) {
  const reduceMotion = useReducedMotion();
  const [opacity] = useState(() => new Animated.Value(1));

  useEffect(() => {
    if (reduceMotion) {
      opacity.setValue(1);
      return;
    }

    const animation = pulse(opacity);
    animation.start();

    return () => animation.stop();
  }, [reduceMotion, opacity]);

  return (
    <Animated.View
      accessibilityElementsHidden
      importantForAccessibility="no-hide-descendants"
      style={{ width: circle ? height : width, height, opacity }}
    >
      <ModulView bgSkeleton rounded={!circle} roundedFull={circle} style={{ flex: 1 }} />
    </Animated.View>
  );
}
