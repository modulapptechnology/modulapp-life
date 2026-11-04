import { View, type StyleProp, type ViewProps, type ViewStyle } from 'react-native';
import { useStyles } from '../../utils/useStyles';
import { splitStyleProps, type ViewStyleProps } from '../../utils/styleProps';

export type ModulViewProps = ViewProps &
  ViewStyleProps & {
  style?: StyleProp<ViewStyle>;
};

export function ModulView({ style, ...props }: ModulViewProps) {
  const { view } = useStyles();
  const { style: flags, rest } = splitStyleProps(view, props);

  return <View {...(rest as ViewProps)} style={[...flags, style]} />;
}
