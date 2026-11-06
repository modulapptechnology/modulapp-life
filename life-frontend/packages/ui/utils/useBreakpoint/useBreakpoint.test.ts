import { renderHook } from '@testing-library/react-native';
import { useWindowDimensions } from 'react-native';
import { useBreakpoint } from './useBreakpoint';

jest.mock('react-native/Libraries/Utilities/useWindowDimensions', () => ({
  __esModule: true,
  default: jest.fn(),
}));

const mockedDimensions = jest.mocked(useWindowDimensions);

describe('useBreakpoint', () => {
  it('devuelve el rango teléfono y la escala 1 con un ancho menor al de tablet', async () => {
    mockedDimensions.mockReturnValue({ width: 375, height: 812, scale: 2, fontScale: 1 });

    const { result } = await renderHook(() => useBreakpoint());

    expect(result.current).toEqual({ width: 375, range: 'phone', scale: 1 });
  });

  it('cambia a tablet justo en el ancho límite de 768', async () => {
    mockedDimensions.mockReturnValue({ width: 768, height: 1024, scale: 2, fontScale: 1 });

    const { result } = await renderHook(() => useBreakpoint());

    expect(result.current.range).toBe('tablet');
  });

  it('devuelve el rango escritorio y la escala 1.5 con un ancho de 1024', async () => {
    mockedDimensions.mockReturnValue({ width: 1024, height: 768, scale: 1, fontScale: 1 });

    const { result } = await renderHook(() => useBreakpoint());

    expect(result.current).toEqual({ width: 1024, range: 'desktop', scale: 1.5 });
  });
});
