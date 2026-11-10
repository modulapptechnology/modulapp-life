import { useEffect, useState, type ReactNode } from 'react';
import { Animated, BackHandler, Pressable } from 'react-native';
import { layout } from '@life/theme';
import { animateTo } from '../../../utils/animations/animations';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulView } from '../ModulView/ModulView';

export type ModulDrawerProps = {
  visible: boolean;
  onClose: () => void;
  closeLabel: string;
  children: ReactNode;
};

// Panel que se desliza desde la izquierda y cubre el contenedor donde se coloque, no la pantalla entera:
// en el móvil va debajo de ModulHeader, así el botón del encabezado sigue visible. Aloja al ModulSidebar.
export function ModulDrawer({ visible, onClose, closeLabel, children }: ModulDrawerProps) {
  const { view } = useStyles();
  const [mounted, setMounted] = useState(visible);
  const [offset] = useState(() => new Animated.Value(visible ? 0 : -layout.drawerWidth));

  // La capa oscura se aclara y oscurece al mismo ritmo que el panel entra y sale.
  const backdropOpacity = offset.interpolate({
    inputRange: [-layout.drawerWidth, 0],
    outputRange: [0, 1],
  });

  if (visible && !mounted) setMounted(true);

  useEffect(() => {
    animateTo(offset, visible ? 0 : -layout.drawerWidth, {
      onEnd: ({ finished }) => {
        if (!visible && finished) setMounted(false);
      },
    });

    return () => offset.stopAnimation();
  }, [visible, offset]);

  // El botón atrás de Android cierra el panel en lugar de salir de la pantalla.
  useEffect(() => {
    if (!visible) return;

    const subscription = BackHandler.addEventListener('hardwareBackPress', () => {
      onClose();
      return true;
    });

    return () => subscription.remove();
  }, [visible, onClose]);

  if (!mounted) return null;

  return (
    <ModulView testID="drawer" absoluteFill pointerEvents="box-none">
      <Animated.View style={[view.absoluteFill, { opacity: backdropOpacity }]}>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={closeLabel}
          onPress={onClose}
          style={view.overlay}
        />
      </Animated.View>
      <Animated.View style={[view.drawer, view.bgSurface, { transform: [{ translateX: offset }] }]}>
        {children}
      </Animated.View>
    </ModulView>
  );
}
