import { ModulIcon } from '../ModulIcon/ModulIcon';
import { ModulText } from '../ModulText/ModulText';
import { ModulView } from '../ModulView/ModulView';

export type ModulFieldMessageProps = {
  help?: string;
  error?: string;
};

// El error lleva un icono además del color, para que no dependa solo del color.
export function ModulFieldMessage({ help, error }: ModulFieldMessageProps) {
  const message = error ?? help;
  if (!message) return null;

  return (
    <ModulView row alignCenter gap6>
      {error ? <ModulIcon name="DangerCircleIcon" size="sm" color="danger" /> : null}
      <ModulText
        variant="bodySecondary"
        textSecondary={!error}
        textDanger={!!error}
        accessibilityRole={error ? 'alert' : undefined}
        style={{ flexShrink: 1 }}
      >
        {message}
      </ModulText>
    </ModulView>
  );
}
