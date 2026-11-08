import type { ReactNode } from 'react';
import { ModulEmptyState } from '../ModulEmptyState/ModulEmptyState';
import { ModulErrorState } from '../ModulErrorState/ModulErrorState';
import { ModulLoadingState } from '../ModulLoadingState/ModulLoadingState';

export type ModulStateContainerProps = {
  loading?: boolean;
  // Texto del error ya traducido; si viene, se muestra el estado de error.
  error?: string;
  // true cuando la consulta terminó sin datos.
  empty?: boolean;
  loadingMessage?: string;
  // Esqueleto que reemplaza al indicador de carga; el aviso a las tecnologías de asistencia lo sigue dando loadingMessage.
  loadingSkeleton?: ReactNode;
  errorDescription?: string;
  retryLabel?: string;
  onRetry?: () => void;
  emptyTitle: string;
  emptyDescription?: string;
  emptyActionLabel?: string;
  onEmptyAction?: () => void;
  children: ReactNode;
};

// Decide qué mostrar según el resultado de una consulta: carga, error con reintento, vacío o el contenido.
export function ModulStateContainer({
  loading = false,
  error,
  empty = false,
  loadingMessage,
  loadingSkeleton,
  errorDescription,
  retryLabel,
  onRetry,
  emptyTitle,
  emptyDescription,
  emptyActionLabel,
  onEmptyAction,
  children,
}: ModulStateContainerProps) {
  if (loading) return <ModulLoadingState message={loadingMessage} skeleton={loadingSkeleton} />;

  if (error)
    return (
      <ModulErrorState
        title={error}
        description={errorDescription}
        retryLabel={retryLabel}
        onRetry={onRetry}
      />
    );

  if (empty)
    return (
      <ModulEmptyState
        title={emptyTitle}
        description={emptyDescription}
        actionLabel={emptyActionLabel}
        onAction={onEmptyAction}
      />
    );

  return <>{children}</>;
}
