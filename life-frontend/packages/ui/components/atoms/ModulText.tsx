import { Text as RNText, type StyleProp, type TextProps as RNTextProps, type TextStyle } from 'react-native';
import { MAX_FONT_SIZE_MULTIPLIER, typography, type TextVariant } from '@life/theme';
import { resolveTextRole } from '../../utils/resolveTextRole';
import { useStyles } from '../../utils/useStyles';
import { splitStyleProps, type TextStyleProps } from '../../utils/styleProps';

export type ModulTextProps = RNTextProps &
  TextStyleProps & {
  variant?: TextVariant;
  link?: boolean;
  style?: StyleProp<TextStyle>;
};

export function ModulText({ variant = 'body', link = false, style, ...props }: ModulTextProps) {
  const { text } = useStyles();
  const { style: flags, rest } = splitStyleProps(text, props);

  return (
    <RNText
      maxFontSizeMultiplier={MAX_FONT_SIZE_MULTIPLIER}
      accessibilityRole={resolveTextRole(variant, link)}
      {...(rest as RNTextProps)}
      style={[typography[variant], text.textPrimary, link && text.textBrand, link && text.underline, ...flags, style]}
    />
  );
}
