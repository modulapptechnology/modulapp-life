import { useCallback, useMemo, useState } from 'react';
import { createAuthSubmitter, type AuthSubmitConfig } from '../authSubmitter/authSubmitter';
import type { AuthFormErrors } from '../authValidation/authValidation';
import { translate } from '../translate/translate';

export function useAuthForm<V>({ validate, send }: AuthSubmitConfig<V>) {
  const [errors, setErrors] = useState<AuthFormErrors>({});
  const [serverErrors, setServerErrors] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [done, setDone] = useState(false);
  const submitter = useMemo(() => createAuthSubmitter({ validate, send }), [validate, send]);

  const submit = useCallback(
    async (values: V) => {
      setServerErrors([]);
      setSubmitting(true);
      const outcome = await submitter(values);
      if (outcome.status === 'busy') return;
      setSubmitting(false);
      setErrors(outcome.status === 'invalid' ? outcome.errors : {});
      if (outcome.status === 'rejected') setServerErrors([translate(outcome.errorKey)]);
      if (outcome.status === 'done') setDone(true);
    },
    [submitter],
  );

  return { errors, serverErrors, submitting, done, submit };
}
