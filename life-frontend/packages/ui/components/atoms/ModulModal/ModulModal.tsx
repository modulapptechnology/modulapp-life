import type { ReactNode } from 'react';
import { Modal, Pressable } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulView } from '../ModulView/ModulView';

export type ModulModalProps = {
  visible: boolean;
  onClose: () => void;
  closeLabel: string;
  children: ReactNode;
};

// Capa centrada sobre la pantalla: la usan el menú y el diálogo de confirmación.
export function ModulModal({ visible, onClose, closeLabel, children }: ModulModalProps) {
  const { view } = useStyles();

  return (
    <Modal
      transparent
      visible={visible}
      animationType="fade"
      statusBarTranslucent
      onRequestClose={onClose}
    >
      <ModulView flex1 alignCenter justifyCenter p16>
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
        >
          {children}
        </ModulView>
      </ModulView>
    </Modal>
  );
}
