import { ModulModal } from '../../atoms/ModulModal/ModulModal';
import { ModulText } from '../../atoms/ModulText/ModulText';
import type { IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulList } from '../ModulList/ModulList';
import { ModulListItem } from '../ModulListItem/ModulListItem';

export type ModulMenuItem = {
  key: string;
  label: string;
  icon?: IconName;
  selected?: boolean;
  destructive?: boolean;
  disabled?: boolean;
  onPress: () => void;
};

export type ModulMenuProps = {
  visible: boolean;
  onClose: () => void;
  closeLabel: string;
  title?: string;
  items: ModulMenuItem[];
};

// Menú contextual: lista de acciones que se abre desde cualquier botón; el selector de ModulInput lo reutiliza.
export function ModulMenu({ visible, onClose, closeLabel, title, items }: ModulMenuProps) {
  return (
    <ModulModal visible={visible} onClose={onClose} closeLabel={closeLabel}>
      {title ? <ModulText variant="sectionTitle">{title}</ModulText> : null}
      <ModulList
        data={items}
        keyExtractor={(item) => item.key}
        emptyTitle={title ?? closeLabel}
        renderItem={({ item }) => (
          <ModulListItem
            title={item.label}
            icon={item.icon}
            selected={item.selected}
            destructive={item.destructive}
            disabled={item.disabled}
            onPress={() => {
              onClose();
              item.onPress();
            }}
          />
        )}
      />
    </ModulModal>
  );
}
