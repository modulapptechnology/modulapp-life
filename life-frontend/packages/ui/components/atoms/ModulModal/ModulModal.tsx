import type { ReactNode } from 'react';
import { Animated, Modal, Pressable, useWindowDimensions } from 'react-native';
import { animationScales } from '../../../utils/animations/animations';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { useTransition } from '../../../utils/useTransition/useTransition';
import { ModulView } from '../ModulView/ModulView';

export type ModulModalProps = {
  visible: boolean;
  onClose: () => void;
  closeLabel: string;
  // 'center': centrada (web). 'bottom': sube desde abajo (mobile).
  placement?: 'center' | 'bottom';
  children: ReactNode;
};

export function ModulModal({
  visible,
  onClose,
  closeLabel,
  placement = 'center',
  children,
}: ModulModalProps) {
  const { view } = useStyles();
  const { height } = useWindowDimensions();
  const { mounted, progress } = useTransition(visible);
  const isBottom = placement === 'bottom';

  const panelMotion = isBottom
    ? {
        transform: [
          { translateY: progress.interpolate({ inputRange: [0, 1], outputRange: [height, 0] }) },
        ],
      }
    : {
        opacity: progress,
        transform: [
          {
            scale: progress.interpolate({
              inputRange: [0, 1],
              outputRange: [animationScales.enter, 1],
            }),
          },
        ],
      };

  return (
    <Modal
      transparent
      visible={mounted}
      animationType="none"
      statusBarTranslucent
      onRequestClose={onClose}
    >
      <ModulView
        style={{
          flex: 1,
          alignItems: 'center',
          justifyContent: isBottom ? 'flex-end' : 'center',
          padding: isBottom ? 0 : 16,
        }}
      >
        <Animated.View style={[view.absoluteFill, { opacity: progress }]}>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel={closeLabel}
            onPress={onClose}
            style={view.overlay}
          />
        </Animated.View>
        <Animated.View
          accessibilityViewIsModal
          style={[{ width: '100%', alignItems: 'center' }, panelMotion]}
        >
          <ModulView
            w100
            maxW480
            maxH100
            bgSurface
            borderSubtle
            roundedLg
            p16
            gap16
            style={
              isBottom
                ? { maxWidth: '100%', borderBottomLeftRadius: 0, borderBottomRightRadius: 0 }
                : undefined
            }
          >
            {children}
          </ModulView>
        </Animated.View>
      </ModulView>
    </Modal>
  );
}
