import type { ReactNode } from 'react';
import { ScrollView } from 'react-native';
import { ModulText } from '../../../atoms/ModulText/ModulText';
import { ModulView } from '../../../atoms/ModulView/ModulView';
import { ModulErrorState } from '../../../molecules/ModulErrorState/ModulErrorState';
import { ModulLoadingState } from '../../../molecules/ModulLoadingState/ModulLoadingState';

export type ModulDetailViewProps = {
  title: string;
  status?: 'loading' | 'ready' | 'error';
  loadingMessage?: string;
  errorTitle?: string;
  retryLabel?: string;
  onRetry?: () => void;
  actions?: ReactNode;
  children: ReactNode;
};

// Plantilla de pantalla de detalle: título, acciones y contenido, con estados de carga y error.
export function ModulDetailView({
  title,
  status = 'ready',
  loadingMessage,
  errorTitle = '',
  retryLabel,
  onRetry,
  actions,
  children,
}: ModulDetailViewProps) {
  if (status === 'loading') return <ModulLoadingState message={loadingMessage} />;
  if (status === 'error') {
    return <ModulErrorState title={errorTitle} retryLabel={retryLabel} onRetry={onRetry} />;
  }

  return (
    <ScrollView>
      <ModulView p16 gap16>
        <ModulView row alignCenter justifyBetween gap8>
          <ModulText variant="sectionTitle" accessibilityRole="header">
            {title}
          </ModulText>
          {actions}
        </ModulView>
        {children}
      </ModulView>
    </ScrollView>
  );
}
