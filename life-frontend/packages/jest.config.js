module.exports = {
  preset: 'jest-expo/node',
  displayName: 'packages',
  testMatch: ['<rootDir>/**/*.test.ts'],
  testPathIgnorePatterns: ['/node_modules/', '<rootDir>/ui/'],
};
