import { createTextStyles, createViewStyles, type ThemeColors } from '@life/theme';
import { StyleSheet } from 'react-native';

export const createGlobalStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    ...createViewStyles(colors),
    floating: {
      shadowColor: colors.black,
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.12,
      shadowRadius: 8,
      elevation: 4,
    },
  });

// El tamaño, interlineado y peso base vienen de las variantes de typography.ts
export const createGlobalTextStyles = (colors: ThemeColors) =>
  StyleSheet.create(createTextStyles(colors));
