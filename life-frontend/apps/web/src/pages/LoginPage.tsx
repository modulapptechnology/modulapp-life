import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { translate, useAuthForm, validateLoginForm } from '@life/core';
import { ModulAuthForm, ModulText, ModulView, useMessages } from '@life/ui';
import { sessionManager } from '../services/session/session';

const send = (values: { email: string; password: string }) =>
  sessionManager.signIn({ email: values.email.trim(), password: values.password });

export default function LoginPage() {
  const navigate = useNavigate();
  const { toast } = useMessages();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { errors, serverErrors, submitting, submit } = useAuthForm({
    validate: validateLoginForm,
    send,
  });

  return (
    <ModulAuthForm
      brand={{ name: translate('auth.brandName'), tagline: translate('auth.login.tagline') }}
      title={translate('auth.loginScreenTitle')}
      subtitle={translate('auth.login.description')}
      fields={[
        {
          name: 'email',
          label: translate('auth.username'),
          value: email,
          onChangeText: setEmail,
          error: errors.email ? translate(errors.email) : undefined,
          autoCapitalize: 'none',
          autoComplete: 'email',
          autoCorrect: false,
        },
        {
          name: 'password',
          type: 'password',
          label: translate('auth.password'),
          value: password,
          onChangeText: setPassword,
          error: errors.password ? translate(errors.password) : undefined,
          actionLabel: translate('auth.showPassword'),
          autoComplete: 'current-password',
        },
      ]}
      serverError={serverErrors[0]}
      submitLabel={translate('auth.loginAction')}
      submitting={submitting}
      onSubmit={() => void submit({ email, password })}
      aside={
        <ModulView alignEnd>
          <ModulText
            variant="bodySecondary"
            fontComfortaa
            textAccent
            onPress={() => navigate('/forgot-password')}
          >
            {translate('auth.forgotShort')}
          </ModulText>
        </ModulView>
      }
      links={[
        {
          text: translate('auth.noAccountShort'),
          actionLabel: translate('auth.createAccount'),
          onAction: () => navigate('/register', { replace: true }),
        },
      ]}
      providers={{
        label: translate('auth.signInWith'),
        items: [
          {
            label: translate('auth.googleShort'),
            onPress: () => toast(translate('auth.comingSoon'), { type: 'info' }),
          },
        ],
      }}
    />
  );
}
