import { ModulView } from '@life/ui';
import { Tabs } from 'expo-router/js-tabs';
import { MobileHeader } from '../../components/MobileHeader';
import { MobileTabBar } from '../../components/MobileTabBar';

export default function TabsLayout() {
  return (
    <ModulView flex1>
      <MobileHeader />
      <ModulView flex1>
        <Tabs screenOptions={{ headerShown: false }} tabBar={() => <MobileTabBar />}>
          <Tabs.Screen name="index" />
          <Tabs.Screen name="inbox" />
          <Tabs.Screen name="planner" />
          <Tabs.Screen name="modules" />
        </Tabs>
      </ModulView>
    </ModulView>
  );
}
