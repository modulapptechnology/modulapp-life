const pad = (value: number) => String(value).padStart(2, '0');

// '25/12/2026' -> Date, o null si el texto está incompleto o es una fecha que no existe (31/02/2026).
export function parseDateInput(text: string) {
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(text);
  if (!match) return null;

  const [, day, month, year] = match.map(Number);
  const date = new Date(year, month - 1, day);
  const valid = date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;

  return valid ? date : null;
}

// '09:30' -> Date de hoy con esa hora, o null si el texto está incompleto o fuera de rango.
export function parseTimeInput(text: string) {
  const match = /^(\d{2}):(\d{2})$/.exec(text);
  if (!match) return null;

  const [, hours, minutes] = match.map(Number);
  if (hours > 23 || minutes > 59) return null;

  const date = new Date();
  date.setHours(hours, minutes, 0, 0);

  return date;
}

export const formatDate = (date: Date) => `${pad(date.getDate())}/${pad(date.getMonth() + 1)}/${date.getFullYear()}`;

export const formatTime = (date: Date) => `${pad(date.getHours())}:${pad(date.getMinutes())}`;

// '25/12/2026' <-> '2026-12-25': el formato que usa el calendario del navegador.
export function dateToIso(text: string) {
  const date = parseDateInput(text);
  return date ? `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` : '';
}

export function isoToDate(iso: string) {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso);
  return match ? `${match[3]}/${match[2]}/${match[1]}` : '';
}
