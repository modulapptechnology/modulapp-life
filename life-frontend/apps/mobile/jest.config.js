module.exports = {
  preset: 'jest-expo',
  displayName: 'mobile',
  testMatch: ['<rootDir>/__tests__/**/*.test.ts?(x)'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  setupFiles: ['<rootDir>/jest.setup.ts'],
};
