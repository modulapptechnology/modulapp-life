import { Image } from 'react-native';
import { getInitials } from '../../utils/getInitials';
import { ModulText } from './ModulText';
import { ModulView } from './ModulView';

export type ModulAvatarSize = 'sm' | 'md' | 'lg';

export type ModulAvatarProps = {
  name: string;
  uri?: string;
  size?: ModulAvatarSize;
};

const SIZES = { sm: 32, md: 40, lg: 56 } as const;

export function ModulAvatar({ name, uri, size = 'md' }: ModulAvatarProps) {
  const dimension = SIZES[size];

  return (
    <ModulView
      bgSecondary
      roundedFull
      overflowHidden
      alignCenter
      justifyCenter
      accessibilityLabel={name}
      style={{ width: dimension, height: dimension }}
    >
      {uri ? (
        <Image source={{ uri }} accessibilityLabel={name} style={{ width: dimension, height: dimension }} />
      ) : (
        <ModulText variant="label" textOnPrimary>
          {getInitials(name)}
        </ModulText>
      )}
    </ModulView>
  );
}
