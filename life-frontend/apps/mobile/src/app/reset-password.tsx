import { useCallback, useState } from 'react';
import { translate, useAuthForm, validateResetForm } from '@life/core';
import { ModulAuthForm } from '@life/ui';
import { router, useLocalSearchParams } from 'expo-router';
import { authService } from '../services/auth/auth';

export default function ResetPasswordScreen() {
  const { token } = useLocalSearchParams<{ token?: string }>();
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const send = useCallback(
    (values: { password: string; confirmPassword: string }) =>
      authService.resetPassword(token ?? '', values.password),
    [token],
  );
  const { errors, serverErrors, submitting, done, submit } = useAuthForm({
    validate: validateResetForm,
    send,
  });

  const result = !token
    ? {
        message: translate('auth.resetInvalidLink'),
        actionLabel: translate('auth.requestNewLink'),
        onAction: () => router.replace('/forgot-password'),
      }
    : done
      ? {
          message: translate('auth.resetDone'),
          actionLabel: translate('user.login'),
          onAction: () => router.replace('/login'),
        }
      : undefined;

  return (
    <ModulAuthForm
      title={translate('auth.resetTitle')}
      fields={[
        {
          name: 'password',
          type: 'password',
          label: translate('auth.newPassword'),
          value: password,
          onChangeText: setPassword,
          error: errors.password ? translate(errors.password) : undefined,
          actionLabel: translate('auth.showPassword'),
          autoComplete: 'new-password',
        },
        {
          name: 'confirmPassword',
          type: 'password',
          label: translate('auth.confirmPasswordShort'),
          value: confirmPassword,
          onChangeText: setConfirmPassword,
          error: errors.confirmPassword ? translate(errors.confirmPassword) : undefined,
          actionLabel: translate('auth.showPassword'),
          autoComplete: 'new-password',
        },
      ]}
      serverError={serverErrors[0]}
      submitLabel={translate('auth.resetSave')}
      submitting={submitting}
      onSubmit={() => void submit({ password, confirmPassword })}
      result={result}
    />
  );
}
