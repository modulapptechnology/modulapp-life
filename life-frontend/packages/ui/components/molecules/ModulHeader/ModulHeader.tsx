import type { ReactNode } from 'react';
import { Pressable } from 'react-native';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulIcon } from '../../atoms/ModulIcon/ModulIcon';

export type ModulHeaderProps = {
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
  children?: ReactNode;
  end?: ReactNode;
};

export function ModulHeader({ sidebarOpen, onToggleSidebar, children, end }: ModulHeaderProps) {
  return (
    <ModulView accessibilityRole="header" row alignCenter justifyBetween gap8 px16 py4 bgSurface minH44>
      <ModulView row alignCenter gap8>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={sidebarOpen ? 'Cerrar barra lateral' : 'Abrir barra lateral'}
          accessibilityState={{ expanded: sidebarOpen }}
          onPress={onToggleSidebar}
          hitSlop={8}
        >
          <ModulIcon name="SidebarMinimalisticIcon" size="md" color="textSecondary" />
        </Pressable>
        {children}
      </ModulView>
      {end}
    </ModulView>
  );
}
