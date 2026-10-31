import { createTextStyles, createViewStyles, type ThemeColors } from '@life/theme';

export const createWebStyles = (colors: ThemeColors) => ({
  ...createViewStyles(colors),
  floating: { boxShadow: `0 4px 8px ${colors.black}1f` },
});

export const createWebTextStyles = (colors: ThemeColors) => createTextStyles(colors);
