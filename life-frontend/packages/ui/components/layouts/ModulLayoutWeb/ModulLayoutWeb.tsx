import { useEffect, useState, type ReactNode } from 'react';
import { Animated, Easing } from 'react-native';
import { layout } from '@life/theme';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulHeader } from '../../molecules/ModulHeader/ModulHeader';
import { ModulSidebar, type ModulSidebarSection } from '../../molecules/ModulSidebar/ModulSidebar';

export type NavSection = ModulSidebarSection;

export type ModulLayoutWebProps = {
  sections: NavSection[];
  activePath: string;
  onNavigate: (path: string) => void;
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
    Animated.timing(width, {
      toValue: sidebarOpen ? layout.sidebarWidth : 0,
      duration: layout.slideDuration,
      easing: Easing.out(Easing.cubic),
      useNativeDriver: false,
    }).start(({ finished }) => {
      if (!sidebarOpen && finished) setMounted(false);
    });

    return () => width.stopAnimation();
  }, [sidebarOpen, width]);

  return (
    <ModulView flex1 bg>
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
            <ModulView testID="web-sidebar" style={view.sidebar}>
              <ModulSidebar sections={sections} activePath={activePath} onNavigate={onNavigate} />
            </ModulView>
          )}
        </Animated.View>
        <ModulView testID="web-main" flex1>
          {children}
        </ModulView>
      </ModulView>
    </ModulView>
  );
}
