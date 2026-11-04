import { ModulText } from './ModulText';

export type ModulFieldMessageProps = {
  help?: string;
  error?: string;
};

export function ModulFieldMessage({ help, error }: ModulFieldMessageProps) {
  const message = error ?? help;
  if (!message) return null;

  return (
    <ModulText
      variant="bodySecondary"
      textSecondary={!error}
      textDanger={!!error}
      accessibilityRole={error ? 'alert' : undefined}
    >
      {message}
    </ModulText>
  );
}
