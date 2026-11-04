import { useStyles } from '../../utils/useStyles';
import { ModulText } from './ModulText';
import { ModulView } from './ModulView';

export type ModulBadgeTone = 'primary' | 'secondary' | 'danger' | 'success' | 'warning' | 'info';

export type ModulBadgeProps = {
  label: string | number;
  tone?: ModulBadgeTone;
};

const TONE_STYLE = {
  primary: 'bgPrimary',
  secondary: 'bgSecondary',
  danger: 'bgDanger',
  success: 'bgSuccess',
  warning: 'bgWarning',
  info: 'bgInfo',
} as const;

export function ModulBadge({ label, tone = 'danger' }: ModulBadgeProps) {
  const { view } = useStyles();

  return (
    <ModulView roundedFull px8 py4 alignCenter justifyCenter style={[view[TONE_STYLE[tone]], { alignSelf: 'flex-start' }]}>
      <ModulText variant="label" textOnPrimary>
        {label}
      </ModulText>
    </ModulView>
  );
}
