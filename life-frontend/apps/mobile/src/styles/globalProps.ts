import type { ViewProps, ViewStyle, TextProps, TextStyle, StyleProp } from 'react-native';
import type { createGlobalStyles, createGlobalTextStyles } from './global';
import type { TextVariant } from '@life/theme';

export type GlobalStyleProps = {
  [K in keyof ReturnType<typeof createGlobalStyles>]?: boolean;
};

export type GlobalProps = ViewProps &
  GlobalStyleProps & {
  style?: StyleProp<ViewStyle>;
};

export type GlobalTextStyleProps = {
  [K in keyof ReturnType<typeof createGlobalTextStyles>]?: boolean;
};

export type GlobalTextProps = TextProps &
  GlobalTextStyleProps & {
  variant?: TextVariant;
  style?: StyleProp<TextStyle>;
};
