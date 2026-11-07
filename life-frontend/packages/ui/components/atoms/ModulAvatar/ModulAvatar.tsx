import { Image } from 'react-native';
import { getInitials } from '../../../utils/getInitials/getInitials';
import { ModulText } from '../ModulText/ModulText';
import { ModulView } from '../ModulView/ModulView';

export type ModulAvatarSize = 'xs' | 'sm' | 'md' | 'lg';

export type ModulAvatarProps = {
  name: string;
  uri?: string;
  size?: ModulAvatarSize;
};

const SIZES = { xs: 24, sm: 32, md: 40, lg: 56 } as const;

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
        <Image
          source={{ uri }}
          accessibilityLabel={name}
          style={{ width: dimension, height: dimension }}
        />
      ) : (
        <ModulText
          variant="label"
          textOnPrimary
          style={size === 'xs' ? { fontSize: 10, lineHeight: 14 } : undefined}
        >
          {getInitials(name)}
        </ModulText>
      )}
    </ModulView>
  );
}
