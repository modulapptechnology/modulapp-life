import { useState } from 'react';
import { Navigate, useNavigate, useSearchParams } from 'react-router-dom';
import { translate, useAuthForm, validateVerifyForm } from '@life/core';
import { ModulAuthForm } from '@life/ui';
import { authService } from '../services/auth/auth';

const send = (values: { email: string; code: string }) =>
  authService.verifyEmail(values.email, values.code);

export default function VerifyEmailPage() {
  const navigate = useNavigate();
  const email = useSearchParams()[0].get('email') ?? '';
  const [code, setCode] = useState('');
  const { errors, serverErrors, submitting, done, submit } = useAuthForm({
    validate: validateVerifyForm,
    send,
  });

  if (!email) return <Navigate to="/login" replace />;

  return (
    <ModulAuthForm
      brand={{ name: translate('auth.brandName'), tagline: translate('auth.verify.tagline') }}
      title={translate('auth.verify.title')}
      subtitle={done ? undefined : translate('auth.verify.subtitle').replace('{email}', email)}
      fields={[
        {
          name: 'code',
          label: translate('auth.verify.code'),
          value: code,
          onChangeText: setCode,
          error: errors.code ? translate(errors.code) : undefined,
          maxLength: 6,
          autoComplete: 'one-time-code',
        },
      ]}
      serverError={serverErrors[0]}
      submitLabel={translate('auth.verify.submit')}
      submitting={submitting}
      onSubmit={() => void submit({ email, code })}
      result={
        done
          ? {
              message: translate('auth.verify.success'),
              actionLabel: translate('user.login'),
              onAction: () => navigate('/login', { replace: true }),
            }
          : undefined
      }
    />
  );
}
