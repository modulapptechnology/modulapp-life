import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { translate, useAuthForm, validateForgotForm } from '@life/core';
import { ModulAuthForm } from '@life/ui';
import { authService } from '../services/auth/auth';

const send = (values: { email: string }) => authService.forgotPassword(values.email);

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const { errors, serverErrors, submitting, done, submit } = useAuthForm({
    validate: validateForgotForm,
    send,
  });

  return (
    <ModulAuthForm
      brand={{ name: translate('auth.brandName'), tagline: translate('auth.forgot.tagline') }}
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
              onAction: () => navigate('/login', { replace: true }),
            }
          : undefined
      }
      links={[
        {
          actionLabel: translate('auth.backToLogin'),
          onAction: () => navigate('/login', { replace: true }),
        },
      ]}
    />
  );
}
