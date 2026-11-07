import type { ReactNode } from 'react';
import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulFormContainerProps = {
  children: ReactNode;
  submitLabel: string;
  onSubmit: () => void;
  cancelLabel?: string;
  onCancel?: () => void;
  submitting?: boolean;
  // Botones de 32 de alto, para las plantillas de la web.
  compact?: boolean;
  // Errores generales devueltos por el servidor; los de cada campo se pasan al `error` de cada ModulInput.
  serverErrors?: string[];
};

// Contenedor de formulario: campos, errores del servidor con un mismo aspecto y las acciones de enviar y cancelar.
export function ModulFormContainer({
  children,
  submitLabel,
  onSubmit,
  cancelLabel,
  onCancel,
  submitting = false,
  compact = false,
  serverErrors = [],
}: ModulFormContainerProps) {
  return (
    <ModulView gap16>
      {serverErrors.length ? (
        <ModulView gap4>
          {serverErrors.map((error) => (
            <ModulText key={error} variant="bodySecondary" textDanger accessibilityRole="alert">
              {error}
            </ModulText>
          ))}
        </ModulView>
      ) : null}
      {children}
      <ModulView row justifyEnd gap8>
        {cancelLabel && onCancel ? (
          <ModulButton
            variant="secondary"
            label={cancelLabel}
            onPress={onCancel}
            disabled={submitting}
            compact={compact}
          />
        ) : null}
        <ModulButton
          label={submitLabel}
          onPress={onSubmit}
          disabled={submitting}
          compact={compact}
        />
      </ModulView>
    </ModulView>
  );
}

// Convierte los errores por campo del servidor ({ campo: [mensajes] }) en un mapa campo → primer mensaje,
// listo para el `error` de cada ModulInput.
export function toFieldErrors(errors?: Record<string, string[]>): Record<string, string> {
  const result: Record<string, string> = {};
  for (const [field, messages] of Object.entries(errors ?? {})) {
    if (messages[0]) result[field] = messages[0];
  }
  return result;
}
