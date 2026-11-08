import type { ReactNode } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulText } from '../../atoms/ModulText/ModulText';

export type ModulLoadingStateProps = {
  // Texto ya traducido; también es el nombre que anuncian las tecnologías de asistencia.
  message?: string;
  // Esqueleto (ModulSkeleton) que reemplaza al indicador y deja ver la forma del contenido que llega.
  skeleton?: ReactNode;
};

export function ModulLoadingState({ message, skeleton }: ModulLoadingStateProps) {
  const { view, colors } = useStyles();

  return (
    <View
      accessible
      accessibilityRole="progressbar"
      accessibilityLabel={message}
      accessibilityState={{ busy: true }}
      accessibilityLiveRegion="polite"
      style={[view.flex1, view.alignCenter, view.justifyCenter, view.gap8, view.p16]}
    >
      {skeleton ?? <ActivityIndicator color={colors.primary} />}
      {message && !skeleton ? (
        <ModulText variant="bodySecondary" textSecondary>
          {message}
        </ModulText>
      ) : null}
    </View>
  );
}
