import { APP_NAME } from '@life/constants';
import { ModulText, ModulView } from '@life/ui';

export default function HomeScreen() {
  return (
    <ModulView flex1 bgSurface alignCenter justifyCenter>
      <ModulText>{APP_NAME}</ModulText>
    </ModulView>
  );
}
