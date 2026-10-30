# life-frontend

Aplicación de ModulApp Life (React + TypeScript, React Native con Expo).

## Estructura

Las rutas usan Expo Router: cada archivo de `src/app/` es una pantalla y `_layout.tsx` define el navegador. El alias `@/` apunta a `src/`.

```
src/
  app/          rutas y layout raíz (Expo Router)
  components/   componentes compartidos
  hooks/        hooks compartidos
  services/     acceso a la API y otros servicios
  styles/       estilos globales reutilizables y tipos de sus props
  theme/        colores, tipografía y dimensiones
  types/        tipos de TypeScript
```

No se versionan `node_modules`, artefactos de compilación (`dist/`, `.expo/`, `web-build/`), archivos de entorno (`.env*`) ni claves o certificados (ver `.gitignore`).

## Sistema de diseño

Los valores visuales viven en `src/theme/` y `src/styles/`; los componentes no usan valores fijos propios.

| Archivo                     | Contenido                                                                      |
| --------------------------- | ------------------------------------------------------------------------------ |
| `src/theme/colors.ts`       | Paleta clara (`lightColors`) y oscura (`darkColors`), y el tipo `ThemeColors`  |
| `src/theme/typography.ts`   | Variantes de texto (tamaño, interlineado y peso) y límites de escala de fuente |
| `src/theme/dimension.ts`    | Layout, breakpoints y escalas responsivas (definidas, aún sin uso)             |
| `src/styles/global.ts`      | `createGlobalStyles` (vistas) y `createGlobalTextStyles` (texto)               |
| `src/styles/globalProps.ts` | Tipos de las props booleanas (`GlobalProps`, `GlobalTextProps`)                |

### Cambiar un valor

Cada valor se cambia en su archivo y se refleja en todo lo que lo usa. Un color en `colors.ts` cambia todos los estilos que lo leen (`bg`, `bgPrimary`, `textBrand`, etc.).

### Usar los estilos

Los componentes reciben los estilos como props booleanas, sin `style`, y el texto recibe una variante:

```tsx
<ModulView w100 p16 gap8 bgSurface rounded>
  <ModulText variant="sectionTitle" fwBold>
    Mis tareas
  </ModulText>
</ModulView>
```

Los estilos salen de `createGlobalStyles(colors)` y `createGlobalTextStyles(colors)`, con `lightColors` o `darkColors` según el tema del sistema.

### Convenciones

- Colores: siempre desde `ThemeColors`; nunca un hexadecimal dentro de un componente.
- Texto: el tamaño, interlineado y peso base salen de `typography`; el color y el peso extra, de las props (`textSecondary`, `textBrand`, `fwBold`, etc.).
- Para agregar un estilo nuevo: la entrada en `createGlobalStyles` o `createGlobalTextStyles`; el tipo de la prop se actualiza solo.
- Contraste: los pares de texto y fondo cumplen WCAG AA (4.5:1) en tema claro y oscuro; los bordes de interfaz, 3:1.
- Escalas responsivas: `breakpoints` y `spacingScale` están definidos en `dimension.ts`; los componentes aún no los aplican.

## Instalación y ejecución

Desde `life-frontend`:

```powershell
npm install
npx expo start   # Expo Go (QR), emulador (a Android, i iOS) o navegador (w)
npm run web      # navegador
npx tsc --noEmit # tipos
npx expo lint    # lint
```

Antes de usar APIs de Expo o React Native se consulta la documentación de la versión instalada (ver `AGENTS.md`).

## Formato y estilo

Las reglas están versionadas en el repositorio: `.prettierrc` y `.prettierignore` (Prettier: comillas simples, punto y coma, coma final, ancho de 100, finales de línea LF) y `.editorconfig` (indentación de 2 espacios, UTF-8, LF). El formato no depende de carpetas de IDE: `.idea/`, `.vs/` y `.vscode/` no se versionan (están en el `.gitignore` de la raíz). Para formatear al guardar basta activar Prettier en el editor, que lee `.prettierrc` y `.editorconfig` del proyecto.

Prettier se integra con ESLint mediante `eslint-config-prettier`, que debe añadirse al final de `extends` (o como `prettier` en `eslint.config.js`) cuando exista la configuración de ESLint de la app, para que las reglas de lint no contradigan el formato.

Desde `life-frontend`:

```powershell
npm install

# Aplicar el formato
npm run format

# Comprobar sin modificar archivos
npm run format:check
```

### Comentarios

No se agregan comentarios salvo que sean necesarios: solo cuando explican un porqué que el código no deja claro por sí mismo. No se comenta lo que el código ya expresa y no se deja código comentado.
