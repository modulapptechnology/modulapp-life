import type { ApiResult } from '@life/types';
import { authErrorKey } from '../authErrors/authErrors';
import { hasErrors, type AuthFormErrors } from '../authValidation/authValidation';

export type AuthSubmitConfig<V> = {
  validate: (values: V) => AuthFormErrors;
  send: (values: V) => Promise<ApiResult<unknown>>;
};

export type AuthSubmitOutcome =
  | { status: 'busy' }
  | { status: 'invalid'; errors: AuthFormErrors }
  | { status: 'rejected'; errorKey: string }
  | { status: 'done' };

export function createAuthSubmitter<V>({ validate, send }: AuthSubmitConfig<V>) {
  let busy = false;

  return async (values: V): Promise<AuthSubmitOutcome> => {
    if (busy) return { status: 'busy' };
    const errors = validate(values);
    if (hasErrors(errors)) return { status: 'invalid', errors };
    busy = true;
    try {
      const errorKey = authErrorKey(await send(values));
      return errorKey ? { status: 'rejected', errorKey } : { status: 'done' };
    } finally {
      busy = false;
    }
  };
}
