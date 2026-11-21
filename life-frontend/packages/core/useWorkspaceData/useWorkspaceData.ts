import { useEffect, useState } from 'react';

type Result<T> = { workspaceId: string; data: T | null; failed: boolean };

export function useWorkspaceData<T>(
  workspaceId: string | null,
  load: (workspaceId: string) => Promise<T>,
) {
  const [result, setResult] = useState<Result<T> | null>(null);

  useEffect(() => {
    if (!workspaceId) return;
    let current = true;
    load(workspaceId).then(
      (data) => current && setResult({ workspaceId, data, failed: false }),
      () => current && setResult({ workspaceId, data: null, failed: true }),
    );
    return () => {
      current = false;
    };
  }, [workspaceId, load]);

  const ready = result?.workspaceId === workspaceId ? result : null;
  return { data: ready?.data ?? null, loading: !!workspaceId && !ready, error: !!ready?.failed };
}
