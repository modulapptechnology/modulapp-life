import { useState } from 'react';
import { translate, useAuthForm, validateVerifyForm } from '@life/core';
import { ModulAuthForm } from '@life/ui';
import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { authService } from '../services/auth/auth';

const send = (values: { email: string; code: string }) =>
  authService.verifyEmail(values.email, values.code);

export default function VerifyEmailScreen() {
  const { email = '' } = useLocalSearchParams<{ email?: string }>();
  const [code, setCode] = useState('');
  const { errors, serverErrors, submitting, done, submit } = useAuthForm({
    validate: validateVerifyForm,
    send,
  });

  if (!email) return <Redirect href="/login" />;

  return (
    <ModulAuthForm
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
              onAction: () => router.replace('/login'),
            }
          : undefined
      }
    />
  );
}
