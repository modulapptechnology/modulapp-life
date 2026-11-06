import { ModulButton } from '../atoms/ModulButton';
import { ModulModal } from '../atoms/ModulModal';
import { ModulText } from '../atoms/ModulText';
import { ModulView } from '../atoms/ModulView';

export type ModulConfirmDialogProps = {
  visible: boolean;
  title: string;
  message?: string;
  confirmLabel: string;
  cancelLabel: string;
  destructive?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
};

// Las acciones destructivas usan el botón de peligro y no se confirman tocando fuera del diálogo.
export function ModulConfirmDialog({
                                     visible,
                                     title,
                                     message,
                                     confirmLabel,
                                     cancelLabel,
                                     destructive = false,
                                     onConfirm,
                                     onCancel,
                                   }: ModulConfirmDialogProps) {
  return (
    <ModulModal visible={visible} onClose={onCancel} closeLabel={cancelLabel}>
      <ModulText variant="sectionTitle">{title}</ModulText>
      {message ? (
        <ModulText variant="body" textSecondary>
          {message}
        </ModulText>
      ) : null}
      <ModulView row justifyEnd gap8>
        <ModulButton variant="secondary" label={cancelLabel} onPress={onCancel} />
        <ModulButton variant={destructive ? 'danger' : 'primary'} label={confirmLabel} onPress={onConfirm} />
      </ModulView>
    </ModulModal>
  );
}
