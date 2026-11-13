import { Pressable } from 'react-native';
import { ModulFloatingButton } from '../../atoms/ModulFloatingButton/ModulFloatingButton';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { isActive } from '../ModulSidebar/ModulSidebar';

export type ModulTabBarItem = {
  path: string;
  label: string;
  icon: IconName;
};

export type ModulTabBarAction = {
  icon: IconName;
  label: string;
  onPress: () => void;
};

export type ModulTabBarProps = {
  items: ModulTabBarItem[];
  activePath: string;
  onNavigate: (path: string) => void;
  action?: ModulTabBarAction;
};

export function ModulTabBar({ items, activePath, onNavigate, action }: ModulTabBarProps) {
  return (
    <ModulView>
      {action ? (
        <ModulView
          absolute
          pointerEvents="box-none"
          style={{ bottom: '100%', left: 0, right: 0, height: 72 }}
        >
          <ModulFloatingButton
            icon={action.icon}
            accessibilityLabel={action.label}
            onPress={action.onPress}
          />
        </ModulView>
      ) : null}
      <ModulView accessibilityRole="tablist" row justifyAround alignCenter bg py4>
        {items.map((item) => {
          const active = isActive(item.path, activePath);

          return (
            <Pressable
              key={item.path}
              accessibilityRole="tab"
              accessibilityLabel={item.label}
              accessibilityState={{ selected: active }}
              onPress={() => onNavigate(item.path)}
              hitSlop={8}
            >
              <ModulView alignCenter justifyCenter gap4 minH44 minW44>
                <ModulIcon name={item.icon} size="lg" color={active ? 'brand' : 'text'} />
                <ModulView
                  roundedFull
                  bgBrand
                  style={{ width: 6, height: 6, opacity: active ? 1 : 0 }}
                />
              </ModulView>
            </Pressable>
          );
        })}
      </ModulView>
    </ModulView>
  );
}
