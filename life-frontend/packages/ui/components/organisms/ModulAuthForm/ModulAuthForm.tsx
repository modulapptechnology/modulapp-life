import type { ReactNode } from 'react';
import { Platform } from 'react-native';
import { useBreakpoint } from '../../../utils/useBreakpoint/useBreakpoint';
import { ModulButton } from '../../atoms/ModulButton/ModulButton';
import { ModulFieldMessage } from '../../atoms/ModulFieldMessage/ModulFieldMessage';
import { ModulText } from '../../atoms/ModulText/ModulText';
import { ModulView } from '../../atoms/ModulView/ModulView';
import { ModulInput, type ModulInputProps } from '../../molecules/ModulInput/ModulInput';
import { ModulAuthView } from '../../templates/mobile/ModulAuthView/ModulAuthView';
import { ModulAuthViewWeb } from '../../templates/web/ModulAuthViewWeb/ModulAuthViewWeb';

export type ModulAuthFormField = Omit<ModulInputProps, 'variant' | 'disabled'> & {
  name: string;
};

export type ModulAuthFormResult = {
  message: string;
  actionLabel: string;
  onAction: () => void;
};

export type ModulAuthFormLink = {
  text?: string;
  actionLabel: string;
  onAction: () => void;
  muted?: boolean;
};

export type ModulAuthFormProviders = {
  label: string;
  items: { label: string; onPress: () => void }[];
};

export type ModulAuthFormProps = {
  title: string;
  subtitle?: string;
  brand?: { name: string; tagline: string };
  fields: ModulAuthFormField[];
  submitLabel: string;
  onSubmit: () => void;
  submitting?: boolean;
  serverError?: string;
  aside?: ReactNode;
  links?: ModulAuthFormLink[];
  providers?: ModulAuthFormProviders;
  result?: ModulAuthFormResult;
  children?: ReactNode;
};

export function ModulAuthForm({
  title,
  subtitle,
  brand,
  fields,
  submitLabel,
  onSubmit,
  submitting = false,
  serverError,
  aside,
  links = [],
  providers,
  result,
  children,
}: ModulAuthFormProps) {
  const { range } = useBreakpoint();
  const split = !!brand && Platform.OS === 'web' && range !== 'phone';

  const content = result ? (
    <ModulView gap24>
      <ModulText variant="body" textCenter={!split} accessibilityRole="alert">
        {result.message}
      </ModulText>
      <ModulButton
        variant="brand"
        large={split}
        label={result.actionLabel}
        onPress={result.onAction}
      />
    </ModulView>
  ) : (
    <>
      <ModulView gap24>
        <ModulFieldMessage error={serverError} />
        {fields.map(({ name, ...field }) => (
          <ModulInput key={name} variant="floating" disabled={submitting} {...field} />
        ))}
        {aside}
      </ModulView>
      <ModulView gap8>
        <ModulButton
          variant="brand"
          large={split}
          label={submitLabel}
          loading={submitting}
          onPress={onSubmit}
        />
        {links.map((link) => (
          <ModulView key={link.actionLabel} row wrap justifyCenter gap8>
            {link.text ? (
              <ModulText variant="bodySecondary" fontComfortaa>
                {link.text}
              </ModulText>
            ) : null}
            <ModulText
              variant="bodySecondary"
              fontComfortaa
              textAccent={!link.muted}
              textMuted={link.muted}
              onPress={link.onAction}
            >
              {link.actionLabel}
            </ModulText>
          </ModulView>
        ))}
      </ModulView>
    </>
  );

  const providersBlock = providers ? (
    <ModulView gap8>
      {split ? (
        <ModulView row alignCenter gap16>
          <ModulView flex1 divider />
          <ModulText variant="bodySecondary" fontComfortaa textSecondary>
            {providers.label}
          </ModulText>
          <ModulView flex1 divider />
        </ModulView>
      ) : (
        <ModulText variant="bodySecondary" fontComfortaa textMuted textCenter>
          {providers.label}
        </ModulText>
      )}
      {providers.items.map((item) => (
        <ModulButton
          key={item.label}
          variant="outline"
          large={split}
          label={item.label}
          onPress={item.onPress}
        />
      ))}
    </ModulView>
  ) : undefined;

  if (split) {
    return (
      <ModulAuthViewWeb
        layout="split"
        brandName={brand.name}
        tagline={brand.tagline}
        title={title}
        subtitle={subtitle}
        footer={providersBlock}
      >
        {content}
        {children}
      </ModulAuthViewWeb>
    );
  }

  return (
    <ModulAuthView variant="brand" title={title} subtitle={subtitle} footer={providersBlock}>
      {content}
      {children}
    </ModulAuthView>
  );
}
