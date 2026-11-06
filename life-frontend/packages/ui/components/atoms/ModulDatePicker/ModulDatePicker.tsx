import { useRef } from 'react';
import { Pressable } from 'react-native';
import { dateToIso, isoToDate } from '../../../utils/dateFormat/dateFormat';
import { useStyles } from '../../../utils/useStyles/useStyles';
import { ModulIcon } from '../ModulIcon/ModulIcon';
import type { ModulDatePickerProps } from './ModulDatePicker.types';

export type { ModulDatePickerProps } from './ModulDatePicker.types';

// Web: abre el calendario o el reloj del propio navegador (input nativo oculto).
// Móvil usa ModulDatePicker.native.tsx, que abre el selector del sistema.
export function ModulDatePicker({
  mode,
  value,
  onChange,
  accessibilityLabel,
  disabled = false,
}: ModulDatePickerProps) {
  const { view } = useStyles();
  const inputRef = useRef<HTMLInputElement>(null);

  const open = () => {
    const input = inputRef.current;
    if (!input) return;
    if (typeof input.showPicker === 'function') input.showPicker();
    else input.click();
  };

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={accessibilityLabel}
      disabled={disabled}
      onPress={open}
      style={[view.minW44, view.minH44, view.alignCenter, view.justifyCenter]}
    >
      <ModulIcon
        name={mode === 'date' ? 'CalendarIcon' : 'ClockCircleIcon'}
        color="textSecondary"
      />
      <input
        ref={inputRef}
        type={mode}
        tabIndex={-1}
        aria-hidden
        value={mode === 'date' ? dateToIso(value) : value}
        onChange={(event) =>
          onChange(mode === 'date' ? isoToDate(event.target.value) : event.target.value)
        }
        style={{
          position: 'absolute',
          bottom: 0,
          left: 0,
          width: 1,
          height: 1,
          opacity: 0,
          pointerEvents: 'none',
        }}
      />
    </Pressable>
  );
}
