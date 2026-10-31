/** Quita los espacios de los extremos y reduce los espacios repetidos a uno solo. */
export function normalizeText(value: string): string {
  return value.trim().replace(/\s+/g, ' ');
}

/** Arma el título de una pantalla o ventana: "Sección · Aplicación", o solo el nombre si no hay sección. */
export function buildTitle(appName: string, section?: string): string {
  const name = section ? normalizeText(section) : '';

  return name ? `${name} · ${appName}` : appName;
}
