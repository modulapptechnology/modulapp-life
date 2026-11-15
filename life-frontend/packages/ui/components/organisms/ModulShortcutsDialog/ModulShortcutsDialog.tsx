import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulDialog } from '../ModulDialog/ModulDialog';

export type ModulShortcutItem = { label: string; keys: string };

export type ModulShortcutsDialogProps = {
  visible: boolean;
  onClose: () => void;
  title: string;
  closeLabel: string;
  items: ModulShortcutItem[];
};

// Lista de atajos de teclado disponibles, para que se puedan descubrir desde la interfaz.
export function ModulShortcutsDialog({
  visible,
  onClose,
  title,
  closeLabel,
  items,
}: ModulShortcutsDialogProps) {
  return (
    <ModulDialog visible={visible} onClose={onClose} title={title} closeLabel={closeLabel}>
      <ModulView gap8>
        {items.map((item) => (
          <ModulView key={item.label} row alignCenter justifyBetween gap16>
            <ModulText variant="body">{item.label}</ModulText>
            <ModulView bgSurface rounded border px4 py4>
              <ModulText variant="body" fwBold>
                {item.keys}
              </ModulText>
            </ModulView>
          </ModulView>
        ))}
      </ModulView>
    </ModulDialog>
  );
}
