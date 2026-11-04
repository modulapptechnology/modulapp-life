import { useWindowDimensions } from 'react-native';
import { getSpacingScale } from '@life/theme';
import { getScreenRange } from './screenRange';

export { getScreenRange, type ScreenRange } from './screenRange';

export function useBreakpoint() {
  const { width } = useWindowDimensions();
  return { width, range: getScreenRange(width), scale: getSpacingScale(width) };
}
