import { createElement, type ReactNode } from 'react';
import { renderHook } from '@testing-library/react-native';
import { useWindowDimensions } from 'react-native';
import { darkColors, lightColors } from '@life/theme';
import { ThemeProvider } from '../../theme/ThemeProvider';
import { useStyles } from './useStyles';

jest.mock('react-native/Libraries/Utilities/useWindowDimensions', () => ({
  __esModule: true,
  default: jest.fn(),
}));

const mockedDimensions = jest.mocked(useWindowDimensions);

const conTema = (preference: 'light' | 'dark') =>
  function Wrapper({ children }: { children: ReactNode }) {
    return createElement(ThemeProvider, { preference, children });
  };

describe('useStyles', () => {
  it('entrega los colores y el rango del tema y la pantalla actuales', async () => {
    mockedDimensions.mockReturnValue({ width: 375, height: 812, scale: 2, fontScale: 1 });

    const { result } = await renderHook(() => useStyles(), { wrapper: conTema('dark') });

    expect(result.current).toMatchObject({ colors: darkColors, scheme: 'dark', range: 'phone' });
  });

  it('usa los colores del tema claro cuando la preferencia es light', async () => {
    mockedDimensions.mockReturnValue({ width: 375, height: 812, scale: 2, fontScale: 1 });

    const { result } = await renderHook(() => useStyles(), { wrapper: conTema('light') });

    expect(result.current.colors).toEqual(lightColors);
  });

  it('falla con un mensaje claro si se usa fuera del ThemeProvider', async () => {
    mockedDimensions.mockReturnValue({ width: 375, height: 812, scale: 2, fontScale: 1 });
    jest.spyOn(console, 'error').mockImplementation(() => undefined);

    const render = renderHook(() => useStyles());

    await expect(render).rejects.toThrow('useTheme debe usarse dentro de <ThemeProvider>');
  });
});
