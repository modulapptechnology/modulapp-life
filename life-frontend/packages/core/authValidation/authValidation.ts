export const EMAIL_MAX_LENGTH = 320;
export const PASSWORD_MIN_LENGTH = 10;
export const PASSWORD_MAX_LENGTH = 128;

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const VERIFICATION_CODE_LENGTH = 6;

export type AuthField = 'email' | 'password' | 'confirmPassword' | 'code';

export type AuthFormErrors = Partial<Record<AuthField, string>>;

function validateEmail(email: string): string | undefined {
  if (!email.trim()) return 'auth.validation.emailRequired';
  if (email.length > EMAIL_MAX_LENGTH) return 'auth.validation.emailTooLong';
  if (!EMAIL_PATTERN.test(email.trim())) return 'auth.validation.emailInvalid';
  return undefined;
}

function validateNewPassword(password: string): string | undefined {
  if (!password) return 'auth.validation.passwordRequired';
  if (password.length < PASSWORD_MIN_LENGTH) return 'auth.validation.passwordTooShort';
  if (password.length > PASSWORD_MAX_LENGTH) return 'auth.validation.passwordTooLong';
  if (!/\p{Lu}/u.test(password)) return 'auth.validation.passwordUpper';
  if (!/\p{Ll}/u.test(password)) return 'auth.validation.passwordLower';
  if (!/\d/.test(password)) return 'auth.validation.passwordDigit';
  return undefined;
}

export function validateLoginForm(values: { email: string; password: string }): AuthFormErrors {
  const errors: AuthFormErrors = {};
  const email = validateEmail(values.email);
  if (email) errors.email = email;
  if (!values.password) errors.password = 'auth.validation.passwordRequired';
  else if (values.password.length > PASSWORD_MAX_LENGTH)
    errors.password = 'auth.validation.passwordTooLong';
  return errors;
}

export function validateRegisterForm(values: {
  email: string;
  password: string;
  confirmPassword: string;
}): AuthFormErrors {
  const errors: AuthFormErrors = {};
  const email = validateEmail(values.email);
  if (email) errors.email = email;
  const password = validateNewPassword(values.password);
  if (password) errors.password = password;
  if (values.confirmPassword !== values.password)
    errors.confirmPassword = 'auth.validation.passwordMismatch';
  return errors;
}

export function validateForgotForm(values: { email: string }): AuthFormErrors {
  const errors: AuthFormErrors = {};
  const email = validateEmail(values.email);
  if (email) errors.email = email;
  return errors;
}

export function validateResetForm(values: {
  password: string;
  confirmPassword: string;
}): AuthFormErrors {
  const errors: AuthFormErrors = {};
  const password = validateNewPassword(values.password);
  if (password) errors.password = password;
  if (values.confirmPassword !== values.password)
    errors.confirmPassword = 'auth.validation.passwordMismatch';
  return errors;
}

export function validateVerifyForm(values: { email: string; code: string }): AuthFormErrors {
  const errors: AuthFormErrors = {};
  const email = validateEmail(values.email);
  if (email) errors.email = email;
  const code = values.code.trim();
  if (!code) errors.code = 'auth.validation.codeRequired';
  else if (!new RegExp(`^\\d{${VERIFICATION_CODE_LENGTH}}$`).test(code))
    errors.code = 'auth.validation.codeInvalid';
  return errors;
}

export function hasErrors(errors: AuthFormErrors): boolean {
  return Object.keys(errors).length > 0;
}
