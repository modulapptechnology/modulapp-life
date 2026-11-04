import type { ComponentType } from 'react';
import * as Icons from '@life/icons';
import type { ThemeColors } from '@life/theme';
import { useTheme } from '../../theme/ThemeProvider';

const ICON_SIZES = { sm: 16, md: 24, lg: 32, xl: 48 } as const;

export type IconName = Extract<keyof typeof Icons, `${string}Icon`>;
export type IconSize = keyof typeof ICON_SIZES;

export type ModulIconProps = {
  name: IconName;
  size?: IconSize;
  color?: keyof ThemeColors;
};

type IconComponent = ComponentType<{ size?: number; color?: string }>;

export function ModulIcon({ name, size = 'md', color = 'text' }: ModulIconProps) {
  const { colors } = useTheme();
  const Icon = Icons[name] as IconComponent;

  return <Icon size={ICON_SIZES[size]} color={colors[color]} />;
}
