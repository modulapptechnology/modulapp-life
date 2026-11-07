// @life/icons reexporta @solar-icons, que solo trae módulos ESM y Jest no resuelve.
// Cada icono se sustituye por una vista con su nombre como testID.
jest.mock('@life/icons', () => {
  const React = jest.requireActual<typeof import('react')>('react');
  const { View } = jest.requireActual<typeof import('react-native')>('react-native');

  return new Proxy({ __esModule: true } as Record<string, unknown>, {
    get: (target, name: string) =>
      name in target
        ? target[name]
        : (props: object) => React.createElement(View, { ...props, testID: name }),
  });
});
