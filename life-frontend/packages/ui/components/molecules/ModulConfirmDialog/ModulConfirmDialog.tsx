import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulModal } from '../../atoms/ModulModal/ModulModal';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulConfirmDialogProps = {
  visible: boolean;
  title: string;
  message?: string;
  confirmLabel: string;
  cancelLabel: string;
  destructive?: boolean;
  // 'brand': icono arriba, texto centrado y los dos botones del mismo ancho, con los colores de marca.
  appearance?: 'default' | 'brand';
  icon?: IconName;
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
  appearance = 'default',
  icon,
  onConfirm,
  onCancel,
}: ModulConfirmDialogProps) {
  const brand = appearance === 'brand';

  return (
    <ModulModal visible={visible} onClose={onCancel} closeLabel={cancelLabel}>
      {brand && icon ? (
        <ModulView alignSelfCenter>
          <ModulIcon name={icon} size="lg" color="brand" />
        </ModulView>
      ) : null}
      <ModulText variant="sectionTitle" textCenter={brand}>
        {title}
      </ModulText>
      {message ? (
        <ModulText variant="body" textSecondary textCenter={brand}>
          {message}
        </ModulText>
      ) : null}
      <ModulView row justifyEnd={!brand} gap8={!brand} gap16={brand}>
        <ModulView flex1={brand}>
          <ModulButton
            variant={brand ? 'brandOutline' : 'secondary'}
            label={cancelLabel}
            onPress={onCancel}
          />
        </ModulView>
        <ModulView flex1={brand}>
          <ModulButton
            variant={brand ? 'brand' : destructive ? 'danger' : 'primary'}
            label={confirmLabel}
            onPress={onConfirm}
          />
        </ModulView>
      </ModulView>
    </ModulModal>
  );
}
