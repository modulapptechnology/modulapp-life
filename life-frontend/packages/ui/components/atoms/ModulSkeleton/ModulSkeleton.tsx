import type { DimensionValue } from 'react-native';
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
  return (
    <ModulView
      bgSkeleton
      rounded={!circle}
      roundedFull={circle}
      accessibilityElementsHidden
      importantForAccessibility="no-hide-descendants"
      style={{ width: circle ? height : width, height }}
    />
  );
}
