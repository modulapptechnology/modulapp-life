// Mocks de módulos nativos que jest-expo no cubre. Los módulos de Expo ya los mockea el preset.
jest.mock(
  'react-native-safe-area-context',
  () => require('react-native-safe-area-context/jest/mock').default,
);
