import { ActivityIndicator, View } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulText } from '../../atoms/ModulText/ModulText';

export type ModulLoadingStateProps = {
  message?: string;
};

export function ModulLoadingState({ message }: ModulLoadingStateProps) {
  const { view, colors } = useStyles();

  return (
    <View
      accessibilityRole="progressbar"
      style={[view.flex1, view.alignCenter, view.justifyCenter, view.gap8, view.p16]}
    >
      <ActivityIndicator color={colors.primary} />
      {message ? (
        <ModulText variant="bodySecondary" textSecondary>
          {message}
        </ModulText>
      ) : null}
    </View>
  );
}
