/** Un texto está en blanco si está vacío o solo tiene espacios. */
export function isBlank(value: string): boolean {
  return value.trim().length === 0;
}

/** Un texto es válido si no está en blanco y no supera el largo máximo (sin contar espacios de los extremos). */
export function isValidLength(value: string, maxLength: number): boolean {
  const length = value.trim().length;

  return length > 0 && length <= maxLength;
}
