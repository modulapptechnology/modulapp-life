import { ModulDialog, type ModulDialogProps } from '../ModulDialog/ModulDialog';

export type ModulBottomSheetProps = Omit<ModulDialogProps, 'placement'>;

// Hoja inferior: el mismo diálogo, anclado al borde inferior de la pantalla.
export function ModulBottomSheet(props: ModulBottomSheetProps) {
  return <ModulDialog {...props} placement="bottom" />;
}
