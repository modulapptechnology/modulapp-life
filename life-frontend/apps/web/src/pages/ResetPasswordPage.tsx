import { useCallback, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { translate, useAuthForm, validateResetForm } from '@life/core';
import { ModulAuthForm } from '@life/ui';
import { authService } from '../services/auth/auth';

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const token = useSearchParams()[0].get('token');
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
        onAction: () => navigate('/forgot-password', { replace: true }),
      }
    : done
      ? {
          message: translate('auth.resetDone'),
          actionLabel: translate('user.login'),
          onAction: () => navigate('/login', { replace: true }),
        }
      : undefined;

  return (
    <ModulAuthForm
      brand={{ name: translate('auth.brandName'), tagline: translate('auth.reset.tagline') }}
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
