import { breakpoints } from '@life/theme';

export type ScreenRange = 'phone' | 'tablet' | 'desktop';

export const getScreenRange = (width: number): ScreenRange => {
  if (width >= breakpoints.desktop) return 'desktop';
  if (width >= breakpoints.tablet) return 'tablet';
  return 'phone';
};
