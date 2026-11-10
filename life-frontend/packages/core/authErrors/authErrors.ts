import type { ApiResult } from '@life/types';

export function authErrorKey(result: ApiResult<unknown>): string | undefined {
  switch (result.kind) {
    case 'success':
    case 'cancelled':
      return undefined;
    case 'unauthorized':
      return 'auth.error.credentials';
    case 'conflict':
      return 'auth.error.register';
    case 'badRequest':
      return 'auth.error.invalid';
    case 'networkError':
    case 'timeout':
      return 'auth.error.network';
    default:
      return 'auth.error.generic';
  }
}
