import { useEffect, useState, type ReactNode } from 'react';
import { Animated, ScrollView } from 'react-native';
import { layout } from '@life/theme';
import { animateTo } from '../../../utils/animations/animations';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulHeader } from '../../molecules/ModulHeader/ModulHeader';
import {
  ModulSidebar,
  type ModulSidebarProps,
  type ModulSidebarSection,
} from '../../molecules/ModulSidebar/ModulSidebar';

export type NavSection = ModulSidebarSection;

export type ModulLayoutWebProps = Pick<
  ModulSidebarProps,
  'sections' | 'activePath' | 'onNavigate' | 'user' | 'footerSections' | 'signOut'
> & {
  // Contenido del encabezado exclusivo de la web, se pasa tal cual a ModulHeader.
  headerContent?: ReactNode;
  headerEnd?: ReactNode;
  children: ReactNode;
};

// Layout de la web (y del escritorio, que muestra la web): encabezado, barra lateral y contenido.
export function ModulLayoutWeb({
  sections,
  activePath,
  onNavigate,
  user,
  footerSections,
  signOut,
  headerContent,
  headerEnd,
  children,
}: ModulLayoutWebProps) {
  const { view } = useStyles();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [mounted, setMounted] = useState(true);
  const [width] = useState(() => new Animated.Value(layout.sidebarWidth));

  if (sidebarOpen && !mounted) setMounted(true);

  // El ancho se anima de 0 al ancho completo; el contenido sigue montado hasta que termina de cerrarse.
  useEffect(() => {
    animateTo(width, sidebarOpen ? layout.sidebarWidth : 0, {
      nativeDriver: false,
      onEnd: ({ finished }) => {
        if (!sidebarOpen && finished) setMounted(false);
      },
    });

    return () => width.stopAnimation();
  }, [sidebarOpen, width]);

  return (
    <ModulView h100 flex1 bg overflowHidden>
      <ModulHeader
        sidebarOpen={sidebarOpen}
        onToggleSidebar={() => setSidebarOpen((open) => !open)}
        end={headerEnd}
      >
        {headerContent}
      </ModulHeader>
      <ModulView flex1 row>
        <Animated.View style={[view.bgSurface, view.overflowHidden, { width }]}>
          {mounted && (
            <ModulView testID="web-sidebar" flex1 style={view.sidebar}>
              <ModulSidebar
                sections={sections}
                activePath={activePath}
                onNavigate={onNavigate}
                user={user}
                footerSections={footerSections}
                signOut={signOut}
              />
            </ModulView>
          )}
        </Animated.View>
        <ModulView testID="web-main" flex1>
          <ScrollView contentContainerStyle={{ flexGrow: 1 }}>{children}</ScrollView>
        </ModulView>
      </ModulView>
    </ModulView>
  );
}
