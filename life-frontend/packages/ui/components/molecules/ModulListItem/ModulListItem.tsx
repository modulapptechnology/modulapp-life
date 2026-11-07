import { useState } from 'react';
import { Pressable } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { useTheme } from '../../../theme/ThemeProvider';

export type ModulListItemAction = {
  icon: IconName;
  label: string;
  onPress: () => void;
};

export type ModulListItemProps = {
  title: string;
  description?: string;
  icon?: IconName;
  actions?: ModulListItemAction[];
  selected?: boolean;
  destructive?: boolean;
  compact?: boolean;
  disabled?: boolean;
  onPress?: () => void;
};

export function ModulListItem({
  title,
  description,
  icon,
  actions,
  selected = false,
  destructive = false,
  compact = false,
  disabled = false,
  onPress,
}: ModulListItemProps) {
  const { view } = useStyles();
  const [hovered, setHovered] = useState(false);
  const { colors } = useTheme();

  return (
    <ModulView row alignCenter gap8 px16 style={disabled ? view.opacityDisabled : undefined}>
      <Pressable
        accessibilityRole={onPress ? 'button' : undefined}
        accessibilityLabel={title}
        accessibilityState={{ selected, disabled }}
        disabled={disabled || !onPress}
        onPress={onPress}
        onHoverIn={() => setHovered(true)}
        onHoverOut={() => setHovered(false)}
        style={[
          view.flex1,
          view.row,
          view.alignCenter,
          view.gap8,
          view.minH44,
          view.rounded,
          compact ? view.py4 : view.py8,
          onPress && hovered && !disabled ? { backgroundColor: colors.border } : null,
        ]}
      >
        {icon ? <ModulIcon name={icon} color={destructive ? 'danger' : 'text'} /> : null}
        <ModulView flex1>
          <ModulText variant="body" textDanger={destructive}>
            {title}
          </ModulText>
          {description ? (
            <ModulText variant="bodySecondary" textSecondary>
              {description}
            </ModulText>
          ) : null}
        </ModulView>
        {selected ? <ModulIcon name="CheckIcon" color="primary" /> : null}
      </Pressable>
      {actions?.map((action) => (
        <ModulButton
          key={action.label}
          variant="secondary"
          icon={action.icon}
          accessibilityLabel={action.label}
          disabled={disabled}
          onPress={action.onPress}
        />
      ))}
    </ModulView>
  );
}
