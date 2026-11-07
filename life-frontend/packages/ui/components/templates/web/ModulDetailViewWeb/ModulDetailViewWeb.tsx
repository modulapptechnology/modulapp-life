import type { ReactNode } from 'react';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import { ModulErrorState } from '../../../molecules/ModulErrorState/ModulErrorState';
import { ModulLoadingState } from '../../../molecules/ModulLoadingState/ModulLoadingState';
import { ModulPageWeb } from '../ModulPageWeb/ModulPageWeb';

export type ModulDetailViewWebProps = {
  title: string;
  description?: string;
  status?: 'loading' | 'ready' | 'error';
  loadingMessage?: string;
  errorTitle?: string;
  retryLabel?: string;
  onRetry?: () => void;
  actions?: ReactNode;
  // Panel lateral con los datos secundarios (estado, fechas, responsable...).
  aside?: ReactNode;
  children: ReactNode;
};

// Plantilla de detalle para la web: contenido principal en una tarjeta y, a su derecha, un panel lateral de
// ancho fijo con los datos secundarios. Las acciones van en el encabezado.
export function ModulDetailViewWeb({
  title,
  description,
  status = 'ready',
  loadingMessage,
  errorTitle = '',
  retryLabel,
  onRetry,
  actions,
  aside,
  children,
}: ModulDetailViewWebProps) {
  return (
    <ModulPageWeb title={title} description={description} actions={actions}>
      {status === 'loading' ? <ModulLoadingState message={loadingMessage} /> : null}
      {status === 'error' ? (
        <ModulErrorState title={errorTitle} retryLabel={retryLabel} onRetry={onRetry} />
      ) : null}
      {status === 'ready' ? (
        <ModulView row gap16 style={{ alignItems: 'flex-start' }}>
          <ModulView flex1 bgSurface borderSubtle roundedLg p16 gap16>
            {children}
          </ModulView>
          {aside ? (
            <ModulView bgSurface borderSubtle roundedLg p16 gap8 style={{ width: 300 }}>
              {aside}
            </ModulView>
          ) : null}
        </ModulView>
      ) : null}
    </ModulPageWeb>
  );
}
