import { useEffect, useState } from 'react';
import { Animated, StyleSheet, useWindowDimensions } from 'react-native';
import { sweep, useReducedMotion } from '../../../utils/animations/animations';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

const TRACK_WIDTH = 200;
const TRACK_HEIGHT = 8;
const BAR_WIDTH = 80;
const TRACK_OPACITY = 0.3;

export type ModulSplashProps = {
  // Nombre de la marca ya traducido, se muestra con la fuente de marca.
  brandName: string;
  // Texto ya traducido; es el nombre que anuncian las tecnologías de asistencia.
  message: string;
};

// Pantalla de carga con la marca: fondo violeta, nombre de la marca y una barra que recorre su pista en bucle.
export function ModulSplash({ brandName, message }: ModulSplashProps) {
  const { colors } = useStyles();
  const { height } = useWindowDimensions();
  const reduceMotion = useReducedMotion();
  const [progress] = useState(() => new Animated.Value(0));

  useEffect(() => {
    if (reduceMotion) {
      progress.setValue(0.5);
      return;
    }

    const animation = sweep(progress);
    animation.start();

    return () => animation.stop();
  }, [reduceMotion, progress]);

  const translateX = progress.interpolate({
    inputRange: [0, 1],
    outputRange: [-BAR_WIDTH, TRACK_WIDTH],
  });

  return (
    <ModulView
      flex1
      bgBrand
      alignCenter
      justifyCenter
      gap24
      accessible
      accessibilityRole="progressbar"
      accessibilityLabel={message}
      accessibilityState={{ busy: true }}
      style={{ minHeight: height }}
    >
      <ModulText variant="display" fontBrand textOnBrand>
        {brandName}
      </ModulText>
      <ModulView roundedFull overflowHidden style={{ width: TRACK_WIDTH, height: TRACK_HEIGHT }}>
        <ModulView
          style={[
            StyleSheet.absoluteFill,
            { backgroundColor: colors.white, opacity: TRACK_OPACITY },
          ]}
        />
        <Animated.View
          style={{
            width: BAR_WIDTH,
            height: '100%',
            backgroundColor: colors.white,
            transform: [{ translateX }],
          }}
        />
      </ModulView>
    </ModulView>
  );
}
