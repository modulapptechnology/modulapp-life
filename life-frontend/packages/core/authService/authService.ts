import type { ApiClient } from '@life/api-client';
import type { ApiResult, LoginCredentials } from '@life/types';

export const REGISTER_ROUTE = '/api/v1/auth/register';
export const VERIFY_EMAIL_ROUTE = '/api/v1/auth/verify-email';
export const FORGOT_PASSWORD_ROUTE = '/api/v1/auth/forgot-password';
export const RESET_PASSWORD_ROUTE = '/api/v1/auth/reset-password';

export type RegisterResponse = { id: string; email: string };

export type AuthService = {
  register: (credentials: LoginCredentials) => Promise<ApiResult<RegisterResponse>>;
  verifyEmail: (email: string, code: string) => Promise<ApiResult<void>>;
  forgotPassword: (email: string) => Promise<ApiResult<void>>;
  resetPassword: (token: string, password: string) => Promise<ApiResult<void>>;
};

export function createAuthService(client: ApiClient): AuthService {
  return {
    register: (credentials) =>
      client.post<RegisterResponse>(REGISTER_ROUTE, {
        body: { email: credentials.email.trim(), password: credentials.password },
      }),
    verifyEmail: (email, code) =>
      client.post<void>(VERIFY_EMAIL_ROUTE, { body: { email: email.trim(), code: code.trim() } }),
    forgotPassword: (email) =>
      client.post<void>(FORGOT_PASSWORD_ROUTE, { body: { email: email.trim() } }),
    resetPassword: (token, password) =>
      client.post<void>(RESET_PASSWORD_ROUTE, { body: { token, password } }),
  };
}
