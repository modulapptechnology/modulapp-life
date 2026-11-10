import { ScrollView } from 'react-native';
import { translate } from '@life/core';
import { ModulButton, ModulText, ModulView } from '@life/ui';
import { router } from 'expo-router';

export default function WelcomeScreen() {
  return (
    <ModulView flex1 bg>
      <ScrollView contentContainerStyle={{ flexGrow: 1 }}>
        <ModulView px24 pt24 pb24 w100 maxW480 alignSelfCenter flexGrow justifyBetween gap24>
          <ModulView flex1 alignCenter justifyCenter gap16>
            <ModulText
              variant="sectionTitle"
              fontComfortaaBold
              textAccent
              textCenter
              style={{ maxWidth: 260 }}
            >
              {translate('auth.welcome.tagline')}
            </ModulText>
            <ModulText variant="bodySecondary" fontComfortaa textMuted>
              {translate('auth.welcome.modules')}
            </ModulText>
            <ModulText
              variant="watermark"
              fontBrand
              textWatermark
              textCenter
              trackingWide
              importantForAccessibility="no-hide-descendants"
              accessibilityElementsHidden
            >
              LIFE
            </ModulText>
          </ModulView>
          <ModulView gap16>
            <ModulButton
              variant="brand"
              label={translate('user.login')}
              onPress={() => router.push('/login')}
            />
            <ModulButton
              variant="outline"
              label={translate('auth.register')}
              onPress={() => router.push('/register')}
            />
          </ModulView>
        </ModulView>
      </ScrollView>
    </ModulView>
  );
}
