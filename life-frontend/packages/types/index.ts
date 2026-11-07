import type * as Icons from '@life/icons';
import type { ThemeColors } from '@life/theme';

export type ModuleIconName = Extract<keyof typeof Icons, `${string}Icon`>;
export type ModuleAccent = Extract<
  keyof ThemeColors,
  'primary' | 'secondary' | 'success' | 'info' | 'warning'
>;

// Un módulo de la app: lo que necesita la navegación para mostrarlo y llevar a él.
export type ModuleDefinition = {
  id: string;
  path: string;
  nameKey: string;
  icon: ModuleIconName;
  accent: ModuleAccent;
};

export type SessionUser = {
  id: string;
  name: string;
  avatarUri?: string;
};
