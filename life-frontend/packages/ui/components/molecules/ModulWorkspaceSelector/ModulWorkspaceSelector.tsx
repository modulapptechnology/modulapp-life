import { useState } from 'react';
import { Pressable } from 'react-native';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulMenu } from '../ModulMenu/ModulMenu';

export type ModulWorkspaceSelectorProps = {
  workspaces: { id: string; name: string }[];
  activeId: string | null;
  onSelect: (id: string) => void;
  title: string;
  closeLabel: string;
};

export function ModulWorkspaceSelector({
  workspaces,
  activeId,
  onSelect,
  title,
  closeLabel,
}: ModulWorkspaceSelectorProps) {
  const [open, setOpen] = useState(false);
  const active = workspaces.find((workspace) => workspace.id === activeId);

  if (workspaces.length === 0) return null;

  return (
    <>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={title}
        onPress={() => setOpen(true)}
        hitSlop={8}
      >
        <ModulView row alignCenter gap8 minH44>
          <ModulText variant="body" fwBold>
            {active?.name ?? title}
          </ModulText>
          <ModulText variant="body">▾</ModulText>
        </ModulView>
      </Pressable>
      <ModulMenu
        visible={open}
        onClose={() => setOpen(false)}
        closeLabel={closeLabel}
        title={title}
        items={workspaces.map((workspace) => ({
          key: workspace.id,
          label: workspace.name,
          selected: workspace.id === activeId,
          onPress: () => onSelect(workspace.id),
        }))}
      />
    </>
  );
}
