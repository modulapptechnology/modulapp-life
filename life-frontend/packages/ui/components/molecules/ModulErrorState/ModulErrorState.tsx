import { View } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulIcon } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';

export type ModulErrorStateProps = {
  title: string;
  description?: string;
  retryLabel?: string;
  onRetry?: () => void;
};

export function ModulErrorState({ title, description, retryLabel, onRetry }: ModulErrorStateProps) {
  const { view } = useStyles();

  return (
    <View
      accessibilityRole="alert"
      style={[view.flex1, view.alignCenter, view.justifyCenter, view.gap16, view.p16]}
    >
      <ModulIcon name="DangerCircleIcon" size="lg" color="danger" />
      <ModulText variant="sectionTitle" textDanger textCenter>
        {title}
      </ModulText>
      {description ? (
        <ModulText variant="bodySecondary" textSecondary textCenter>
          {description}
        </ModulText>
      ) : null}
      {retryLabel && onRetry ? <ModulButton label={retryLabel} onPress={onRetry} /> : null}
    </View>
  );
}
