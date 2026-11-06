import { FlatList, type FlatListProps } from 'react-native';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulEmptyState } from '../ModulEmptyState/ModulEmptyState';
import { ModulErrorState } from '../ModulErrorState/ModulErrorState';
import { ModulLoadingState } from '../ModulLoadingState/ModulLoadingState';

export type ModulListProps<T> = Omit<
  FlatListProps<T>,
  'style' | 'contentContainerStyle' | 'ListEmptyComponent' | 'ItemSeparatorComponent'
> & {
  loading?: boolean;
  loadingMessage?: string;
  error?: string;
  retryLabel?: string;
  onRetry?: () => void;
  emptyTitle: string;
  emptyDescription?: string;
};

// Lista virtualizada: solo renderiza los elementos cercanos a la pantalla, con sus estados de carga, error y vacío.
export function ModulList<T>({
  loading = false,
  loadingMessage,
  error,
  retryLabel,
  onRetry,
  emptyTitle,
  emptyDescription,
  ...props
}: ModulListProps<T>) {
  const { view } = useStyles();

  if (loading) return <ModulLoadingState message={loadingMessage} />;
  if (error) return <ModulErrorState title={error} retryLabel={retryLabel} onRetry={onRetry} />;

  return (
    <FlatList
      initialNumToRender={12}
      maxToRenderPerBatch={12}
      windowSize={7}
      removeClippedSubviews
      {...props}
      contentContainerStyle={props.data?.length ? undefined : view.flex1}
      ItemSeparatorComponent={() => <ModulView divider />}
      ListEmptyComponent={<ModulEmptyState title={emptyTitle} description={emptyDescription} />}
    />
  );
}
