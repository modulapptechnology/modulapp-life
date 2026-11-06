import { fireEvent, screen } from '@testing-library/react-native';
import { renderWithTheme } from '../../../testing/renderWithTheme/renderWithTheme';
import { ModulDatePicker } from './ModulDatePicker.native';

type SelectorProps = { onValueChange: (event: object, date: Date) => void };

// El selector del sistema es nativo: se sustituye por un botón que elige el 25/12/2026 a las 09:30.
jest.mock('@react-native-community/datetimepicker', () => {
  const React = jest.requireActual<typeof import('react')>('react');
  const { Pressable } = jest.requireActual<typeof import('react-native')>('react-native');

  return {
    __esModule: true,
    default: ({ onValueChange }: SelectorProps) =>
      React.createElement(Pressable, {
        testID: 'selector-del-sistema',
        onPress: () => onValueChange({}, new Date(2026, 11, 25, 9, 30)),
      }),
  };
});

describe('ModulDatePicker', () => {
  it('no muestra el selector hasta presionar el botón', async () => {
    const valor = '';

    await renderWithTheme(
      <ModulDatePicker mode="date" value={valor} onChange={jest.fn()} accessibilityLabel="Fecha" />,
    );

    expect(screen.queryByTestId('selector-del-sistema')).toBeNull();
  });

  it('abre el selector al presionar el botón', async () => {
    await renderWithTheme(
      <ModulDatePicker mode="date" value="" onChange={jest.fn()} accessibilityLabel="Fecha" />,
    );

    await fireEvent.press(screen.getByRole('button', { name: 'Fecha' }));

    expect(screen.getByTestId('selector-del-sistema')).toBeOnTheScreen();
  });

  it('entrega la fecha elegida como DD/MM/AAAA', async () => {
    const onChange = jest.fn();
    await renderWithTheme(
      <ModulDatePicker mode="date" value="" onChange={onChange} accessibilityLabel="Fecha" />,
    );
    await fireEvent.press(screen.getByRole('button', { name: 'Fecha' }));

    await fireEvent.press(screen.getByTestId('selector-del-sistema'));

    expect(onChange).toHaveBeenCalledWith('25/12/2026');
  });

  it('entrega la hora elegida como HH:mm cuando el modo es time', async () => {
    const onChange = jest.fn();
    await renderWithTheme(
      <ModulDatePicker mode="time" value="" onChange={onChange} accessibilityLabel="Hora" />,
    );
    await fireEvent.press(screen.getByRole('button', { name: 'Hora' }));

    await fireEvent.press(screen.getByTestId('selector-del-sistema'));

    expect(onChange).toHaveBeenCalledWith('09:30');
  });

  it('no abre el selector cuando está deshabilitado', async () => {
    const deshabilitado = true;

    await renderWithTheme(
      <ModulDatePicker
        mode="date"
        value=""
        disabled={deshabilitado}
        onChange={jest.fn()}
        accessibilityLabel="Fecha"
      />,
    );
    await fireEvent.press(screen.getByRole('button', { name: 'Fecha' }));

    expect(screen.queryByTestId('selector-del-sistema')).toBeNull();
  });
});
