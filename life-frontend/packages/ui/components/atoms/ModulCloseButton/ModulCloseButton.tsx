import { Pressable } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';

export type ModulCloseButtonProps = {
  label: string;
  onPress: () => void;
};

export function ModulCloseButton({ label, onPress }: ModulCloseButtonProps) {
  const { view, colors } = useStyles();
  const bar = {
    position: 'absolute' as const,
    width: 14,
    height: 2,
    borderRadius: 1,
    backgroundColor: colors.textSecondary,
  };

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      onPress={onPress}
      hitSlop={8}
      style={[
        view.alignCenter,
        view.justifyCenter,
        view.roundedFull,
        view.bgBorder,
        { width: 32, height: 32 },
      ]}
    >
      <Pressable pointerEvents="none" style={[bar, { transform: [{ rotate: '45deg' }] }]} />
      <Pressable pointerEvents="none" style={[bar, { transform: [{ rotate: '-45deg' }] }]} />
    </Pressable>
  );
}
