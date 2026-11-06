export type ModulDatePickerProps = {
  mode: 'date' | 'time';
  // Fecha como DD/MM/AAAA u hora como HH:mm, igual que el texto del campo.
  value: string;
  onChange: (value: string) => void;
  accessibilityLabel?: string;
  disabled?: boolean;
};
