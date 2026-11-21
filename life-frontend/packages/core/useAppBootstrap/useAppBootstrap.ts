import { useEffect } from 'react';

type AppBootstrapOptions = {
  signedIn: boolean;
  restoreSession: () => Promise<unknown>;
  startConnectivity: () => () => void;
  onSignedOut: () => void;
};

export function useAppBootstrap({
  signedIn,
  restoreSession,
  startConnectivity,
  onSignedOut,
}: AppBootstrapOptions) {
  useEffect(() => {
    void restoreSession();
  }, [restoreSession]);

  useEffect(() => {
    if (!signedIn) onSignedOut();
  }, [signedIn, onSignedOut]);

  useEffect(() => startConnectivity(), [startConnectivity]);
}
