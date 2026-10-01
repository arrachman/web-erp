import { defineConfig, globalIgnores } from 'eslint/config';
import nextCoreWebVitals from 'eslint-config-next/core-web-vitals';
import nextTypescript from 'eslint-config-next/typescript';

/**
 * Patch nextCoreWebVitals/nextTypescript rule severities in-place.
 * Flat-config plugins are scoped per config-object, so override severity
 * di config object yang sama dimana plugin di-load (config blocks dari
 * preset Next). Walk hasil spread, cari config object yang punya plugin
 * target, lalu downgrade rule severity.
 */
function patchSeverity(config, ruleName, level) {
  for (const block of config) {
    if (block?.plugins) {
      const ns = ruleName.split('/')[0];
      if (block.plugins[ns]) {
        block.rules = { ...(block.rules ?? {}), [ruleName]: level };
      }
    }
  }
  return config;
}

const patchedNextCore = patchSeverity(
  patchSeverity([...nextCoreWebVitals], 'react-hooks/set-state-in-effect', 'warn'),
  'react-hooks/exhaustive-deps',
  'warn',
);
const patchedNextTs = patchSeverity(
  [...nextTypescript],
  '@typescript-eslint/no-unused-vars',
  ['warn', { argsIgnorePattern: '^_|^err$', varsIgnorePattern: '^_' }],
);

export default defineConfig([
  ...patchedNextCore,
  ...patchedNextTs,
  {
    rules: {
      'react/react-in-jsx-scope': 'off',
      'react/no-unescaped-entities': 'off',
      '@next/next/no-img-element': 'off',
    },
  },
  {
    files: ['app/**/*.{ts,tsx,js,jsx}', 'components/**/*.{ts,tsx,js,jsx}', 'lib/**/*.{ts,tsx,js,jsx}', 'shared/**/*.{ts,tsx,js,jsx}'],
    ignores: ['**/*.test.{ts,tsx}', '**/*.spec.{ts,tsx}', '**/*.seed.{ts,tsx}', '**/*.mock.{ts,tsx}', '**/__fixtures__/**', '**/__mocks__/**'],
    rules: {
      'max-lines': ['error', { max: 400, skipBlankLines: true, skipComments: true }],
    },
  },
  globalIgnores([
    '.next/**',
    'node_modules/**',
    'public/**',
    'prototype/**',
    'preferensi/**',
    'db-design/**',
  ]),
]);
