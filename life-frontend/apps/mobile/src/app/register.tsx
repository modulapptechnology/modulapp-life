import { useState } from 'react';
import { translate, useAuthForm, validateRegisterForm } from '@life/core';
import { ModulAuthForm, ModulConfirmDialog, useMessages } from '@life/ui';
import { router } from 'expo-router';
import { authService } from '../services/auth/auth';

const send = (values: { email: string; password: string; confirmPassword: string }) =>
  authService.register({ email: values.email, password: values.password });

export default function RegisterScreen() {
  const { toast } = useMessages();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const { errors, serverErrors, submitting, done, submit } = useAuthForm({
    validate: validateRegisterForm,
    send,
  });

  return (
    <ModulAuthForm
      title={translate('auth.registerScreenTitle')}
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
        {
          name: 'password',
          type: 'password',
          label: translate('auth.password'),
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
      submitLabel={translate('auth.register')}
      submitting={submitting}
      onSubmit={() => void submit({ email, password, confirmPassword })}
      links={[
        {
          text: translate('auth.haveAccountShort'),
          actionLabel: translate('user.login'),
          onAction: () => router.replace('/login'),
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
    >
      <ModulConfirmDialog
        appearance="brand"
        icon="CheckCircleIcon"
        visible={done}
        title={translate('auth.registerSuccess.title')}
        message={translate('auth.registerSuccess.message')}
        cancelLabel={translate('common.cancel')}
        confirmLabel={translate('auth.accept')}
        onCancel={() => router.replace('/login')}
        onConfirm={() =>
          router.replace({ pathname: '/verify-email', params: { email: email.trim() } })
        }
      />
    </ModulAuthForm>
  );
}
