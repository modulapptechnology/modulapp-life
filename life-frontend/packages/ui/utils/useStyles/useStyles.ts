import { useMemo } from 'react';
import { Platform } from 'react-native';
import { createTextStyles, createViewStyles } from '@life/theme';
import { useTheme } from '../../theme/ThemeProvider';
import { useBreakpoint } from '../useBreakpoint/useBreakpoint';
import { scaleSpacing } from '../scaleSpacing/scaleSpacing';

export function useStyles() {
  const { colors, scheme } = useTheme();
  const { scale, range } = useBreakpoint();

  return useMemo(
    () => ({
      colors,
      scheme,
      range,
      scale,
      view: scaleSpacing(createViewStyles(colors), scale),
      text: createTextStyles(colors, Platform.OS === 'web' ? 'web' : 'native'),
    }),
    [colors, scheme, range, scale],
  );
}
