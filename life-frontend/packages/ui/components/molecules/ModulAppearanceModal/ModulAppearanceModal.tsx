import { Platform } from 'react-native';
import type { ThemeAccent, ThemePreference } from '@life/theme';
import { ThemeProvider, useTheme } from '../../../theme/ThemeProvider';
import { ModulCloseButton } from '../../atoms/ModulCloseButton/ModulCloseButton';
import { ModulModal } from '../../atoms/ModulModal/ModulModal';
import { ModulOptionCard } from '../../atoms/ModulOptionCard/ModulOptionCard';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';

export type ModulAppearanceProps = {
  title: string;
  themeTitle: string;
  accentTitle: string;
  themeOptions: { id: ThemePreference; label: string }[];
  accentOptions: { id: ThemeAccent; label: string }[];
  preference: ThemePreference;
  accent: ThemeAccent;
  onPreferenceChange: (preference: ThemePreference) => void;
  onAccentChange: (accent: ThemeAccent) => void;
};

type Device = 'desktop' | 'mobile';

type Props = ModulAppearanceProps & {
  visible: boolean;
  onClose: () => void;
  closeLabel: string;
  device: Device;
};

// Mini pantalla de ModulApp. Los colores salen del ThemeProvider que la envuelve.
function MiniScreen({ device }: { device: Device }) {
  const isDesktop = device === 'desktop';

  return (
    <ModulView
      bg
      overflowHidden
      row={isDesktop}
      previewDesktop={isDesktop}
      previewPhone={!isDesktop}
    >
      {isDesktop ? (
        <ModulView w22 bgSurface p4 gap4>
          <ModulView h8 radius3 bgPrimary />
          <ModulView h8 radius3 bgBorder />
          <ModulView h8 radius3 bgBorder />
        </ModulView>
      ) : null}
      <ModulView flex1 p5 gap4>
        <ModulView h6 w50 radius3 bgText />
        <ModulView h18={isDesktop} h22={!isDesktop} radius4 bgSurface borderSubtle />
        <ModulView h18={isDesktop} h22={!isDesktop} radius4 bgSurface borderSubtle />
        <ModulView h7 w40 radius4 bgPrimary />
      </ModulView>
      {!isDesktop ? (
        <ModulView h12 row alignCenter justifyAround bgSurface>
          <ModulView size6 roundedFull bgPrimary />
          <ModulView size6 roundedFull bgBorder />
          <ModulView size6 roundedFull bgBorder />
        </ModulView>
      ) : null}
    </ModulView>
  );
}

// Monitor en desktop, teléfono en mobile. "Sistema" lleva la mitad derecha oscura.
function DeviceFrame({
  device,
  preference,
  accent,
}: {
  device: Device;
  preference: ThemePreference;
  accent: ThemeAccent;
}) {
  const isDesktop = device === 'desktop';
  const isSplit = preference === 'system';

  return (
    <ModulView alignCenter>
      <ModulView
        overflowHidden
        borderDesktop={isDesktop}
        radius6={isDesktop}
        borderPhone={!isDesktop}
        roundedLg={!isDesktop}
      >
        <ThemeProvider preference={isSplit ? 'light' : preference} accent={accent}>
          <MiniScreen device={device} />
        </ThemeProvider>
        {isSplit ? (
          <ModulView
            absoluteRight
            overflowHidden
            previewDesktopHalf={isDesktop}
            previewPhoneHalf={!isDesktop}
          >
            <ModulView absoluteTopRight previewDesktop={isDesktop} previewPhone={!isDesktop}>
              <ThemeProvider preference="dark" accent={accent}>
                <MiniScreen device={device} />
              </ThemeProvider>
            </ModulView>
          </ModulView>
        ) : null}
      </ModulView>
      {isDesktop ? (
        <>
          <ModulView w10 h5 bgDevice />
          <ModulView w34 h3 radius2 bgDevice />
        </>
      ) : null}
    </ModulView>
  );
}

export function ModulAppearanceModal({
  visible,
  onClose,
  closeLabel,
  device,
  title,
  themeTitle,
  accentTitle,
  themeOptions,
  accentOptions,
  preference,
  accent,
  onPreferenceChange,
  onAccentChange,
}: Props) {
  const { scheme } = useTheme();

  return (
    <ModulModal
      visible={visible}
      onClose={onClose}
      closeLabel={closeLabel}
      placement={Platform.OS === 'web' ? 'center' : 'bottom'}
    >
      <ModulView row alignCenter justifyBetween>
        <ModulText variant="sectionTitle">{title}</ModulText>
        <ModulCloseButton label={closeLabel} onPress={onClose} />
      </ModulView>

      <ModulText variant="label">{themeTitle}</ModulText>
      <ModulView row gap8>
        {themeOptions.map((option) => {
          const selected = option.id === preference;
          return (
            <ModulOptionCard
              key={option.id}
              label={option.label}
              selected={selected}
              onPress={() => onPreferenceChange(option.id)}
            >
              <DeviceFrame device={device} preference={option.id} accent={accent} />
              <ModulText variant="label" fwBold={selected}>
                {option.label}
              </ModulText>
            </ModulOptionCard>
          );
        })}
      </ModulView>

      <ModulText variant="label">{accentTitle}</ModulText>
      <ModulView row gap8>
        {accentOptions.map((option) => {
          const selected = option.id === accent;
          return (
            <ThemeProvider key={option.id} preference={scheme} accent={option.id}>
              <ModulOptionCard
                row
                label={option.label}
                selected={selected}
                onPress={() => onAccentChange(option.id)}
              >
                <ModulView size16 roundedFull bgPrimary />
                <ModulText variant="label" fwBold={selected}>
                  {option.label}
                </ModulText>
              </ModulOptionCard>
            </ThemeProvider>
          );
        })}
      </ModulView>
    </ModulModal>
  );
}
