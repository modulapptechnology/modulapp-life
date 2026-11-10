import { useState } from 'react';
import { translate, useAuthForm, validateForgotForm } from '@life/core';
import { ModulAuthForm } from '@life/ui';
import { router } from 'expo-router';
import { authService } from '../services/auth/auth';

const send = (values: { email: string }) => authService.forgotPassword(values.email);

export default function ForgotPasswordScreen() {
  const [email, setEmail] = useState('');
  const { errors, serverErrors, submitting, done, submit } = useAuthForm({
    validate: validateForgotForm,
    send,
  });

  return (
    <ModulAuthForm
      title={translate('auth.forgotTitle')}
      subtitle={done ? undefined : translate('auth.forgotSubtitle')}
      fields={[
        {
          name: 'email',
          label: translate('auth.emailShort'),
          value: email,
          onChangeText: setEmail,
          error: errors.email ? translate(errors.email) : undefined,
          autoCapitalize: 'none',
          autoComplete: 'email',
          autoCorrect: false,
        },
      ]}
      serverError={serverErrors[0]}
      submitLabel={translate('auth.forgotSend')}
      submitting={submitting}
      onSubmit={() => void submit({ email })}
      result={
        done
          ? {
              message: translate('auth.forgotSent'),
              actionLabel: translate('auth.backToLogin'),
              onAction: () => router.replace('/login'),
            }
          : undefined
      }
      links={[
        { actionLabel: translate('auth.backToLogin'), onAction: () => router.replace('/login') },
      ]}
    />
  );
}
