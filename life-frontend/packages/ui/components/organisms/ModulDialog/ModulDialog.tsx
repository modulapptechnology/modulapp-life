import { useEffect, useRef, type ElementRef, type ReactNode } from 'react';
import { AccessibilityInfo, findNodeHandle, Platform, View } from 'react-native';
import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulModal } from '../../atoms/ModulModal/ModulModal';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulDialogProps = {
  visible: boolean;
  title: string;
  onClose: () => void;
  closeLabel: string;
  // 'center': centrado (web). 'bottom': hoja inferior (mobile).
  placement?: 'center' | 'bottom';
  children: ReactNode;
};

type KeyListener = (event: { key: string }) => void;

// Diálogo con título y botón de cierre. Al abrirse mueve el foco al título; se cierra con el botón de retroceso
// de Android (onRequestClose de ModulModal), con la tecla Escape en web y con el botón de cierre.
export function ModulDialog({
  visible,
  title,
  onClose,
  closeLabel,
  placement = 'center',
  children,
}: ModulDialogProps) {
  const titleRef = useRef<ElementRef<typeof View>>(null);

  useEffect(() => {
    if (!visible) return;

    const handle = findNodeHandle(titleRef.current);
    if (handle) AccessibilityInfo.setAccessibilityFocus(handle);
  }, [visible]);

  useEffect(() => {
    if (!visible || Platform.OS !== 'web') return;

    const onKeyDown: KeyListener = (event) => {
      if (event.key === 'Escape') onClose();
    };
    const target = globalThis as unknown as {
      addEventListener: (type: string, listener: KeyListener) => void;
      removeEventListener: (type: string, listener: KeyListener) => void;
    };
    target.addEventListener('keydown', onKeyDown);

    return () => target.removeEventListener('keydown', onKeyDown);
  }, [visible, onClose]);

  return (
    <ModulModal visible={visible} onClose={onClose} closeLabel={closeLabel} placement={placement}>
      <ModulView row alignCenter justifyBetween gap8>
        <View ref={titleRef} accessible accessibilityRole="header" style={{ flex: 1 }}>
          <ModulText variant="sectionTitle">{title}</ModulText>
        </View>
        <ModulButton variant="secondary" label={closeLabel} onPress={onClose} />
      </ModulView>
      {children}
    </ModulModal>
  );
}
