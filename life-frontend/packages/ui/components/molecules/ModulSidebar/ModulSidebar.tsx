import { useEffect, useState, type ReactNode } from 'react';
import { Animated, Pressable, ScrollView } from 'react-native';
import { animateTo, animationDurations } from '../../../utils/animations/animations';
import { useStyles } from '../../../utils/useStyles/useStyles';
import type { ThemeColors } from '@life/theme';
import { useTheme } from '../../../theme/ThemeProvider';
import { ModulAvatar } from '../../atoms/ModulAvatar/ModulAvatar';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulConfirmDialog } from '../ModulConfirmDialog/ModulConfirmDialog';

export type ModulSidebarSection = {
  path: string;
  label: string;
  icon: IconName;
  accent?: keyof ThemeColors;
};

export type ModulSidebarUser = {
  name: string;
  avatarUri?: string;
  path: string;
  caption?: string;
};

export type ModulSidebarSignOut = {
  label: string;
  onSignOut: () => void;
  pendingOperations?: number;
  confirmTitle: string;
  confirmMessage: string;
  cancelLabel: string;
};

export type ModulSidebarProps = {
  sections: ModulSidebarSection[];
  activePath: string;
  onNavigate: (path: string) => void;
  user?: ModulSidebarUser | null;
  footerSections?: ModulSidebarSection[];
  signOut?: ModulSidebarSignOut;
};

function isActive(sectionPath: string, activePath: string) {
  if (sectionPath === '/') return activePath === '/';
  return activePath === sectionPath || activePath.startsWith(`${sectionPath}/`);
}

type ItemProps = {
  label: string;
  active?: boolean;
  destructive?: boolean;
  caption?: string;
  onPress: () => void;
  children: ReactNode;
};

function SidebarItem({
  label,
  active = false,
  destructive = false,
  caption,
  onPress,
  children,
}: ItemProps) {
  const { colors } = useTheme();
  const { view } = useStyles();
  const [hovered, setHovered] = useState(false);
  const [hover] = useState(() => new Animated.Value(0));

  useEffect(() => {
    animateTo(hover, hovered ? 1 : 0, { duration: animationDurations.fast });

    return () => hover.stopAnimation();
  }, [hovered, hover]);

  return (
    <Pressable
      accessibilityRole="tab"
      accessibilityLabel={label}
      accessibilityState={{ selected: active }}
      onPress={onPress}
      onHoverIn={() => setHovered(true)}
      onHoverOut={() => setHovered(false)}
    >
      <ModulView rounded bg={active}>
        {!active ? (
          <Animated.View
            pointerEvents="none"
            style={[
              view.absoluteFill,
              view.rounded,
              { zIndex: 0 },
              { backgroundColor: colors.border, opacity: hover },
            ]}
          />
        ) : null}
        <ModulView row alignCenter gap8 p8 minH44 style={{ zIndex: 1 }}>
          {children}
          <ModulView row gap4 style={{ alignItems: 'flex-end' }}>
            <ModulText
              variant="label"
              textBrand={active && !destructive}
              textSecondary={!active && !destructive}
              textDanger={destructive}
            >
              {label}
            </ModulText>
            {caption ? (
              <Animated.View style={{ opacity: hover }}>
                <ModulText variant="label" textSecondary style={{ fontSize: 10, lineHeight: 14 }}>
                  {caption}
                </ModulText>
              </Animated.View>
            ) : null}
          </ModulView>
        </ModulView>
      </ModulView>
    </Pressable>
  );
}

function SectionItem({
  section,
  activePath,
  onNavigate,
}: {
  section: ModulSidebarSection;
  activePath: string;
  onNavigate: (path: string) => void;
}) {
  const active = isActive(section.path, activePath);

  return (
    <SidebarItem label={section.label} active={active} onPress={() => onNavigate(section.path)}>
      <ModulIcon
        name={section.icon}
        color={section.accent ?? (active ? 'primary' : 'textSecondary')}
      />
    </SidebarItem>
  );
}

export function ModulSidebar({
  sections,
  activePath,
  onNavigate,
  user,
  footerSections = [],
  signOut,
}: ModulSidebarProps) {
  const [confirmOpen, setConfirmOpen] = useState(false);

  function requestSignOut() {
    if (!signOut) return;
    if ((signOut.pendingOperations ?? 0) > 0) setConfirmOpen(true);
    else signOut.onSignOut();
  }

  return (
    <ModulView accessibilityRole="tablist" flex1 p8 gap8>
      {user ? (
        <ModulView gap4>
          <SidebarItem
            label={user.name}
            caption={user.caption}
            active={isActive(user.path, activePath)}
            onPress={() => onNavigate(user.path)}
          >
            <ModulAvatar name={user.name} uri={user.avatarUri} size="sm" />
          </SidebarItem>
          <ModulView divider />
        </ModulView>
      ) : null}
      <ScrollView style={{ flex: 1 }} contentContainerStyle={{ gap: 4 }}>
        {sections.map((section) => (
          <SectionItem
            key={section.path}
            section={section}
            activePath={activePath}
            onNavigate={onNavigate}
          />
        ))}
      </ScrollView>
      {user ? (
        <ModulView gap4>
          <ModulView divider />
          {footerSections.map((section) => (
            <SectionItem
              key={section.path}
              section={section}
              activePath={activePath}
              onNavigate={onNavigate}
            />
          ))}
          {signOut ? (
            <SidebarItem label={signOut.label} destructive onPress={requestSignOut}>
              <ModulIcon name="Logout2Icon" color="danger" />
            </SidebarItem>
          ) : null}
        </ModulView>
      ) : null}
      {signOut ? (
        <ModulConfirmDialog
          visible={confirmOpen}
          title={signOut.confirmTitle}
          message={signOut.confirmMessage}
          confirmLabel={signOut.label}
          cancelLabel={signOut.cancelLabel}
          destructive
          onConfirm={() => {
            setConfirmOpen(false);
            signOut.onSignOut();
          }}
          onCancel={() => setConfirmOpen(false)}
        />
      ) : null}
    </ModulView>
  );
}
