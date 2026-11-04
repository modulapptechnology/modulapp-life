import { useMemo } from 'react';
import { createTextStyles, createViewStyles } from '@life/theme';
import { useTheme } from '../theme/ThemeProvider';
import { useBreakpoint } from './useBreakpoint';
import { scaleSpacing } from './scaleSpacing';

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
      text: createTextStyles(colors),
    }),
    [colors, scheme, range, scale],
  );
}
