import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulEmptyStateProps = {
  title: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
};

export function ModulEmptyState({
  title,
  description,
  actionLabel,
  onAction,
}: ModulEmptyStateProps) {
  return (
    <ModulView flex1 alignCenter justifyCenter gap16 p16>
      <ModulText variant="sectionTitle" textCenter>
        {title}
      </ModulText>
      {description ? (
        <ModulText variant="bodySecondary" textSecondary textCenter>
          {description}
        </ModulText>
      ) : null}
      {actionLabel && onAction ? <ModulButton label={actionLabel} onPress={onAction} /> : null}
    </ModulView>
  );
}
