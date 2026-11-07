import type { ReactNode } from 'react';
import { Modal, Pressable } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
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
  const isBottom = placement === 'bottom';

  return (
    <Modal
      transparent
      visible={visible}
      animationType={isBottom ? 'slide' : 'fade'}
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
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={closeLabel}
          onPress={onClose}
          style={view.overlay}
        />
        <ModulView
          accessibilityViewIsModal
          w100
          maxW480
          maxH70
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
      </ModulView>
    </Modal>
  );
}
