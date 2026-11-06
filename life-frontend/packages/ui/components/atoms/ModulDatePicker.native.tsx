import { useState } from 'react';
import { Platform, Pressable } from 'react-native';
import DateTimePicker from '@react-native-community/datetimepicker';
import { formatDate, formatTime, parseDateInput, parseTimeInput } from '../../utils/dateFormat';
import { useStyles } from '../../utils/useStyles';
import { ModulIcon } from './ModulIcon';
import { ModulModal } from './ModulModal';
import type { ModulDatePickerProps } from './ModulDatePicker.types';

export type { ModulDatePickerProps } from './ModulDatePicker.types';

// Móvil: abre el selector del sistema (diálogo en Android, ruedas dentro de una capa en iOS).
export function ModulDatePicker({ mode, value, onChange, accessibilityLabel, disabled = false }: ModulDatePickerProps) {
  const { view } = useStyles();
  const [open, setOpen] = useState(false);

  const current = (mode === 'date' ? parseDateInput(value) : parseTimeInput(value)) ?? new Date();

  const picker = (
    <DateTimePicker
      value={current}
      mode={mode}
      is24Hour
      display={Platform.OS === 'ios' ? 'spinner' : 'default'}
      onValueChange={(_, date) => {
        onChange(mode === 'date' ? formatDate(date) : formatTime(date));
        if (Platform.OS === 'android') setOpen(false);
      }}
      onDismiss={() => setOpen(false)}
    />
  );

  return (
    <>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={accessibilityLabel}
        disabled={disabled}
        onPress={() => setOpen(true)}
        style={[view.minW44, view.minH44, view.alignCenter, view.justifyCenter]}
      >
        <ModulIcon name={mode === 'date' ? 'CalendarIcon' : 'ClockCircleIcon'} color="textSecondary" />
      </Pressable>
      {open && Platform.OS === 'ios' ? (
        <ModulModal visible onClose={() => setOpen(false)} closeLabel={accessibilityLabel ?? mode}>
          {picker}
        </ModulModal>
      ) : null}
      {open && Platform.OS === 'android' ? picker : null}
    </>
  );
}
