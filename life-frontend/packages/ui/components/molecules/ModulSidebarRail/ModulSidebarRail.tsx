import { useEffect, useState, type ReactNode } from 'react';
import { Animated, Pressable, ScrollView } from 'react-native';
import { layout } from '@life/theme';
import { animateTo, animationDurations } from '../../../utils/animations/animations';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { useTheme } from '../../../theme/ThemeProvider';
import { ModulAvatar } from '../../atoms/ModulAvatar/ModulAvatar';
import { ModulIcon, type IconName } from '../../atoms/ModulIcon/ModulIcon';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulConfirmDialog } from '../ModulConfirmDialog/ModulConfirmDialog';
import {
  isActive,
  type ModulSidebarProps,
  type ModulSidebarSection,
  type ModulSidebarUser,
} from '../ModulSidebar/ModulSidebar';

export type ModulSidebarRailLink = {
  path: string;
  label: string;
  icon: IconName;
};

export type ModulSidebarRailGroup = {
  id: string;
  title: string;
  links: ModulSidebarRailLink[];
};

export type ModulSidebarRailSection = ModulSidebarSection & {
  groups?: ModulSidebarRailGroup[];
};

export type ModulSidebarRailUser = ModulSidebarUser & { groups?: ModulSidebarRailGroup[] };

export type ModulSidebarRailProps = Omit<
  ModulSidebarProps,
  'sections' | 'user' | 'footerSections'
> & {
  sections: ModulSidebarRailSection[];
  user?: ModulSidebarRailUser | null;
  footerSections?: ModulSidebarRailSection[];
  open?: boolean;
};

export function findActiveRailSection(sections: ModulSidebarRailSection[], activePath: string) {
  return sections.find(
    (section) =>
      isActive(section.path, activePath) ||
      section.groups?.some((group) => group.links.some((link) => isActive(link.path, activePath))),
  );
}

type RailItemProps = {
  label: string;
  active?: boolean;
  onPress: () => void;
  onHoverChange?: (hovering: boolean) => void;
  children: ReactNode;
};

function RailItem({ label, active = false, onPress, onHoverChange, children }: RailItemProps) {
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
      onHoverIn={() => {
        setHovered(true);
        onHoverChange?.(true);
      }}
      onHoverOut={() => {
        setHovered(false);
        onHoverChange?.(false);
      }}
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
        <ModulView alignCenter px4 py8 style={{ zIndex: 1 }}>
          {children}
        </ModulView>
      </ModulView>
    </Pressable>
  );
}

function RailSection({
  section,
  active,
  onNavigate,
  onHoverChange,
}: {
  section: ModulSidebarRailSection;
  active: boolean;
  onNavigate: (path: string) => void;
  onHoverChange?: (hovering: boolean) => void;
}) {
  return (
    <RailItem
      label={section.label}
      active={active}
      onPress={() => onNavigate(section.path)}
      onHoverChange={onHoverChange}
    >
      <ModulIcon name={section.icon} color={active ? 'primary' : 'text'} />
    </RailItem>
  );
}

function PanelLink({
  link,
  active,
  onNavigate,
}: {
  link: ModulSidebarRailLink;
  active: boolean;
  onNavigate: (path: string) => void;
}) {
  return (
    <Pressable
      accessibilityRole="link"
      accessibilityLabel={link.label}
      accessibilityState={{ selected: active }}
      onPress={() => onNavigate(link.path)}
    >
      <ModulView row alignCenter gap8 p8 rounded bg={active}>
        <ModulIcon name={link.icon} color={active ? 'primary' : 'text'} />
        <ModulText variant="label" textBrand={active}>
          {link.label}
        </ModulText>
      </ModulView>
    </Pressable>
  );
}

function PanelGroup({
  group,
  activePath,
  expanded,
  onToggle,
  onNavigate,
}: {
  group: ModulSidebarRailGroup;
  activePath: string;
  expanded: boolean;
  onToggle: () => void;
  onNavigate: (path: string) => void;
}) {
  return (
    <ModulView gap4>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={group.title}
        accessibilityState={{ expanded }}
        onPress={onToggle}
      >
        <ModulView row alignCenter justifyBetween px8 py4>
          <ModulText variant="label">{group.title}</ModulText>
          <ModulIcon
            name={expanded ? 'AltArrowDownIcon' : 'AltArrowRightIcon'}
            size="sm"
            color="text"
          />
        </ModulView>
      </Pressable>
      {expanded
        ? group.links.map((link) => (
            <PanelLink
              key={link.path}
              link={link}
              active={isActive(link.path, activePath)}
              onNavigate={onNavigate}
            />
          ))
        : null}
    </ModulView>
  );
}

export function ModulSidebarRail({
  sections,
  activePath,
  onNavigate,
  user,
  footerSections = [],
  signOut,
  open = true,
}: ModulSidebarRailProps) {
  const { view } = useStyles();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});
  const [flyoutPath, setFlyoutPath] = useState<string | null>(null);
  const userSection: ModulSidebarRailSection | undefined = user
    ? { path: user.path, label: user.caption ?? user.name, icon: 'UserIcon', groups: user.groups }
    : undefined;
  const allSections = [
    ...(userSection ? [userSection] : []),
    ...sections,
    ...(user ? footerSections : []),
  ];
  const activeSection = findActiveRailSection(allSections, activePath);
  const [lastPath, setLastPath] = useState(activeSection?.path);
  if (activeSection && activeSection.path !== lastPath) setLastPath(activeSection.path);
  const panelSection =
    activeSection ?? allSections.find((section) => section.path === lastPath) ?? sections[0];
  const groups = panelSection?.groups ?? [];
  const panelTarget = open && groups.length > 0 ? layout.sidebarWidth : 0;
  const [panelWidth] = useState(() => new Animated.Value(panelTarget));
  const [panelMounted, setPanelMounted] = useState(panelTarget > 0);

  if (panelTarget > 0 && !panelMounted) setPanelMounted(true);

  useEffect(() => {
    animateTo(panelWidth, panelTarget, {
      nativeDriver: false,
      onEnd: ({ finished }) => {
        if (panelTarget === 0 && finished) setPanelMounted(false);
      },
    });

    return () => panelWidth.stopAnimation();
  }, [panelTarget, panelWidth]);

  const leaveProps = { onMouseLeave: () => setFlyoutPath(null) } as object;

  function hoverSection(section: ModulSidebarRailSection | null, hovering: boolean) {
    if (hovering) setFlyoutPath(section?.path ?? null);
  }

  const flyoutSection =
    panelTarget > 0 ? undefined : allSections.find((section) => section.path === flyoutPath);
  const flyoutGroups = flyoutSection?.groups ?? [];

  function requestSignOut() {
    if (!signOut) return;
    if ((signOut.pendingOperations ?? 0) > 0) setConfirmOpen(true);
    else signOut.onSignOut();
  }

  return (
    <ModulView row flex1>
      <ModulView testID="web-sidebar-rail" style={view.rail} {...leaveProps}>
        <ModulView accessibilityRole="tablist" flex1 p4 gap8>
          {user ? (
            <ModulView gap4>
              <RailItem
                label={user.name}
                active={userSection === activeSection}
                onPress={() => onNavigate(user.path)}
                onHoverChange={(hovering) => hoverSection(userSection ?? null, hovering)}
              >
                <ModulAvatar name={user.name} uri={user.avatarUri} size="xs" />
              </RailItem>
              <ModulView divider />
            </ModulView>
          ) : null}
          <ScrollView style={{ flex: 1 }} contentContainerStyle={{ gap: 4 }}>
            {sections.map((section) => (
              <RailSection
                key={section.path}
                section={section}
                active={section === activeSection}
                onNavigate={onNavigate}
                onHoverChange={(hovering) => hoverSection(section, hovering)}
              />
            ))}
          </ScrollView>
          {user ? (
            <ModulView gap4>
              <ModulView divider />
              {footerSections.map((section) => (
                <RailSection
                  key={section.path}
                  section={section}
                  active={section === activeSection}
                  onNavigate={onNavigate}
                  onHoverChange={(hovering) => hoverSection(section, hovering)}
                />
              ))}
              {signOut ? (
                <RailItem
                  label={signOut.label}
                  onPress={requestSignOut}
                  onHoverChange={(hovering) => hoverSection(null, hovering)}
                >
                  <ModulIcon name="Logout2Icon" color="danger" />
                </RailItem>
              ) : null}
            </ModulView>
          ) : null}
        </ModulView>
        {flyoutSection && flyoutGroups.length > 0 ? (
          <ModulView
            style={[view.absolute, { top: 0, bottom: 0, left: layout.railWidth, zIndex: 10 }]}
          >
            <ModulView
              testID="web-sidebar-flyout"
              flex1
              bgSurface
              borderSubtle
              roundedLg
              p8
              gap8
              style={view.sidebar}
            >
              <ModulText variant="label" fwSemibold>
                {flyoutSection.label}
              </ModulText>
              {flyoutGroups.map((group) => (
                <ModulView key={group.id} gap4>
                  <ModulView px8>
                    <ModulText variant="label">{group.title}</ModulText>
                  </ModulView>
                  {group.links.map((link) => (
                    <PanelLink
                      key={link.path}
                      link={link}
                      active={isActive(link.path, activePath)}
                      onNavigate={(path) => {
                        setFlyoutPath(null);
                        onNavigate(path);
                      }}
                    />
                  ))}
                </ModulView>
              ))}
            </ModulView>
          </ModulView>
        ) : null}
      </ModulView>
      <Animated.View style={[view.overflowHidden, { width: panelWidth }]}>
        {panelMounted && groups.length > 0 && panelSection ? (
          <ModulView testID="web-sidebar-panel" flex1 borderLeftSubtle p8 gap8 style={view.sidebar}>
            <ModulText variant="sectionTitle" fwSemibold>
              {panelSection.label}
            </ModulText>
            <ScrollView style={{ flex: 1 }} contentContainerStyle={{ gap: 8 }}>
              {groups.map((group) => (
                <PanelGroup
                  key={group.id}
                  group={group}
                  activePath={activePath}
                  expanded={!collapsed[group.id]}
                  onToggle={() =>
                    setCollapsed((state) => ({ ...state, [group.id]: !state[group.id] }))
                  }
                  onNavigate={onNavigate}
                />
              ))}
            </ScrollView>
          </ModulView>
        ) : null}
      </Animated.View>
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
