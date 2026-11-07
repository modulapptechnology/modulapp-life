# life-frontend

Aplicaciones móvil, web y de escritorio de ModulApp Life (React + TypeScript) y el código que comparten, en un monorepo con npm workspaces.

## Estructura

```
apps/
  mobile/     React Native con Expo y Expo Router
  web/        React, TypeScript, Vite y React Router
  desktop/    Tauri, que empaqueta la app web
packages/
  types/        tipos compartidos
  constants/    constantes compartidas
  utils/        funciones de utilidad
  validations/  validaciones
  api-client/   cliente de la API (aún vacío)
  core/         lógica común
package.json        workspaces y scripts de la raíz
tsconfig.base.json  opciones de TypeScript compartidas
```

Los paquetes se importan por su nombre (`@life/types`, `@life/constants`, `@life/utils`, `@life/validations`, `@life/api-client`, `@life/core`) y se declaran como dependencia (`"@life/constants": "*"`) en la app que los usa. Son TypeScript puro, sin imports específicos de plataforma: `packages/eslint.config.mjs` rechaza `react-native`, `expo*` y `react-dom`.

### Móvil

Las rutas usan Expo Router: cada archivo de `apps/mobile/src/app/` es una pantalla y `_layout.tsx` define el navegador. El alias `@/` apunta a `apps/mobile/src/`.

```
apps/mobile/src/
  app/          rutas y layout raíz (Expo Router)
  components/   componentes compartidos
  hooks/        hooks compartidos
  services/     acceso a la API y otros servicios
  styles/       estilos globales reutilizables y tipos de sus props
  theme/        colores, tipografía y dimensiones
  types/        tipos de TypeScript
```

### Web

`apps/web/src/main.tsx` monta el enrutador, `App.tsx` define las rutas y las páginas viven en `apps/web/src/pages/`.

### Escritorio

`apps/desktop/src-tauri/` contiene la configuración de Tauri (`tauri.conf.json`) y el código de Rust. La ventana carga la app web: en desarrollo desde `http://localhost:5173` y en producción desde `apps/web/dist`. Los permisos están limitados a `core:default` (`capabilities/default.json`).

No se versionan `node_modules`, artefactos de compilación (`dist/`, `.expo/`, `web-build/`), archivos de entorno (`.env*`) ni claves o certificados (ver `.gitignore`).

## Sistema de diseño

Los valores visuales viven en `packages/theme/` y los componentes en `packages/ui/` (las rutas de esta sección son relativas a `packages/`). Los componentes no usan valores fijos propios: consumen los estilos y colores del tema.

| Archivo               | Contenido                                                                      |
| --------------------- | ------------------------------------------------------------------------------ |
| `theme/colors/colors.ts`     | Paleta clara (`lightColors`) y oscura (`darkColors`), y el tipo `ThemeColors`  |
| `theme/typography/typography.ts` | Variantes de texto (tamaño, interlineado y peso) y límites de escala de fuente |
| `theme/dimension/dimension.ts`  | Layout, breakpoints y escalas responsivas                                      |
| `theme/styles/styles.ts`     | `createViewStyles` (vistas) y `createTextStyles` (texto)                       |

### Cambiar un valor

Cada valor se cambia en su archivo y se refleja en todo lo que lo usa. Un color en `colors.ts` cambia todos los estilos que lo leen (`bg`, `bgPrimary`, `textBrand`, etc.).

### Componentes compartidos (`@life/ui`)

`packages/ui` agrupa los componentes que usan la app móvil, la web y el escritorio (Tauri muestra la web). Se importan desde un único punto y reciben los estilos como props booleanas, sin `style`; el texto recibe una variante:

```tsx
import { ModulButton, ModulText, ModulView } from '@life/ui';

<ModulView flex1 alignCenter justifyCenter gap16 p16 bg>
  <ModulText variant="screenTitle" textBrand>
    Mis tareas
  </ModulText>
  <ModulButton label="Guardar" onPress={guardar} />
</ModulView>
```

| Ruta (en `ui/`)           | Contenido                                                        |
| ------------------------- | ---------------------------------------------------------------- |
| `components/atoms/`       | `ModulView`, `ModulText`, `ModulButton`, `ModulFloatingButton`, `ModulIcon`, `ModulFieldMessage`, `ModulBadge`, `ModulAvatar`, `ModulProgress`, `ModulChip`, `ModulModal` |
| `components/molecules/`   | `ModulInput`, `ModulSwitch`, `ModulCheckbox`, `ModulChipGroup`, `ModulLoadingState`, `ModulEmptyState`, `ModulErrorState`, `ModulOptionGroup`, `ModulListItem`, `ModulList`, `ModulMenu`, `ModulToast`, `ModulConfirmDialog`, `ModulMessagesProvider`, `ModulBarChart`, `ModulCircularProgress`, `ModulDragProvider`, `ModulDraggable`, `ModulDropZone` |
| `components/organisms/`   | Reservada: los organismos viven en cada app hasta que se repitan |
| `theme/ThemeProvider.tsx` | Tema claro, oscuro o del sistema (`ThemeProvider`, `useTheme`)   |
| `utils/`                  | `useStyles`, `useBreakpoint` y funciones auxiliares              |

#### Átomos y moléculas

- `ModulText`: `variant` define el tamaño; los títulos (`screenTitle`, `sectionTitle`, `headerTitle`) llevan rol de encabezado. Con `link` se muestra como enlace (color de marca, subrayado y rol de enlace) y se usa con `onPress`.
- `ModulButton`: `variant` es `primary`, `secondary` o `danger` (`danger` queda por compatibilidad). Con `icon` y sin `label` es un botón de solo icono y exige `accessibilityLabel`.
- `ModulFloatingButton`: botón de solo icono anclado abajo a la derecha; va dentro de un contenedor que ocupe la pantalla (`ModulView flex1`). Abre lo que defina la pantalla desde `onPress`.
- `ModulInput`: un solo campo con `type` (`text`, `password` con botón de mostrar y ocultar, `number`, `textarea`, `search` con botón de borrar, `date` y `time` con máscara `DD/MM/AAAA` y `HH:mm`, `select` con `options`), etiqueta, ayuda y error incluidos. `actionLabel` es el nombre accesible del botón del final del campo.
- `ModulSwitch` y `ModulCheckbox` (`type` `checkbox` o `radio`; `round` para la casilla redonda de tarea): controlados desde afuera con `checked` y `onChange`.
- `ModulBadge` (insignia y contador), `ModulAvatar` (imagen o iniciales), `ModulProgress` (valor de 0 a 100) y `ModulChip`. `ModulChipGroup` muestra los chips en un deslizador horizontal, o como control segmentado con `segmented`. `ModulChip` con `onRemove` es un chip de filtro activo.

```tsx
<ModulInput label="Correo" value={correo} onChangeText={setCorreo} error={errorCorreo} />
<ModulText link onPress={irARecuperar}>Olvidé mi contraseña</ModulText>
<ModulFloatingButton icon="AddCircleIcon" accessibilityLabel="Nueva tarea" onPress={abrirNueva} />
```

- `ModulList`: lista virtualizada (solo renderiza lo cercano a la pantalla) con estados de carga, error y vacío; sus filas son `ModulListItem` (icono, descripción, acciones secundarias, `compact`). `ModulMenu` es el menú contextual y el selector de `ModulInput` lo reutiliza.
- `ModulMessagesProvider` (una vez por app, dentro de `ThemeProvider`) habilita `useMessages()`: `toast('Guardado', { type: 'success' })` y `await confirm({ title: '¿Eliminar?', destructive: true })`.
- `ModulDragProvider`, `ModulDraggable` y `ModulDropZone`: se arrastra con ratón o dedo; con teclado o lector de pantalla se activa el elemento y luego la zona. `ModulBarChart` y `ModulCircularProgress` dibujan con vistas y colores del tema, sin SVG.

```tsx
const { toast, confirm } = useMessages();
if (await confirm({ title: '¿Eliminar la tarea?', destructive: true })) toast('Tarea eliminada', { type: 'success' });
```

Cada app envuelve su raíz con `<ThemeProvider>` y `<ModulMessagesProvider>`. La web resuelve `react-native` a `react-native-web` con un alias en `apps/web/vite.config.ts`.

### Convenciones

- Los componentes empiezan con `Modul` y tienen props tipadas.
- Colores: siempre desde `ThemeColors`; nunca un hexadecimal dentro de un componente.
- Texto: el tamaño, interlineado y peso base salen de `typography`; el color y el peso extra, de las props (`textSecondary`, `textBrand`, `fwBold`, etc.).
- Los textos llegan por props; los componentes no llevan textos fijos ni lógica de negocio.
- Para agregar un estilo nuevo: la entrada en `createViewStyles` o `createTextStyles`; el tipo de la prop se actualiza solo.
- Si un comportamiento difiere por plataforma (hover, foco, área táctil), se agrega un archivo `.web.tsx` o `.native.tsx` junto al componente.
- Contraste: los pares de texto y fondo cumplen WCAG AA (4.5:1) en tema claro y oscuro; los bordes de interfaz, 3:1.
- Escalas responsivas: `useStyles` aplica `spacingScale` según el ancho de pantalla (teléfono, tablet o escritorio) al `padding`, `margin` y `gap` de los estilos del tema.
- `@life/ui` es, junto con `@life/icons`, el único paquete que importa `react-native`; la excepción está en `packages/eslint.config.mjs`.

## Instalación y ejecución

Requisitos: Node.js 22 (`^22.13.0`) y npm 10 o superior. Para el escritorio, además, Rust (`rustup`) y las herramientas de C++ de Visual Studio con WebView2 (ver la [guía de requisitos de Tauri](https://tauri.app/start/prerequisites/)).

Desde `life-frontend`:

```powershell
npm ci             # instala las versiones exactas de package-lock.json (usar npm install solo para agregar o actualizar dependencias)
npm run mobile     # Expo: Expo Go (QR), emulador (a Android, i iOS) o navegador (w)
npm run web        # web en http://localhost:5173
npm run desktop    # ventana nativa con la app web (la primera vez compila Rust)
npm run typecheck  # tsc en las apps y los paquetes
npm run lint       # lint en las apps y los paquetes
npm test           # pruebas unitarias (Jest) de la app móvil
```

Para compilar la web: `npm run build -w @life/web`. Para detener `npm run desktop`, `Ctrl+C` en la terminal; el aviso `STATUS_CONTROL_C_EXIT` de `cargo` es normal.

Antes de usar APIs de Expo o React Native se consulta la documentación de la versión instalada (ver `AGENTS.md`).

## Pruebas unitarias

Las pruebas verifican la lógica de servicios, hooks y utilidades con Jest y el preset `jest-expo`, configurados en `jest.config.js` (un proyecto por zona). Cubren el caso exitoso, el error y los valores límite. Los componentes de `@life/ui` también se prueban (lo que muestran y cómo responden a la interacción); no hay pruebas de integración ni de extremo a extremo. Las versiones de `jest` y `jest-expo` deben ser las que `npx expo install` resuelve para la versión de Expo instalada. Los hooks se prueban con `renderHook` y los componentes con `renderWithTheme` (`packages/ui/testing/`), ambos de `@testing-library/react-native`. La configuración y las dependencias de pruebas del frontend no se comparten con las del backend.

Cada archivo vive en su propia carpeta (por ejemplo `utils/clampPercent/clampPercent.ts`) y su test va en esa misma carpeta, con el nombre `<archivo>.test.ts` (o `.test.tsx`); no hay una carpeta `tests/` centralizada. La lógica compartida vive en `packages/*` y la usan las tres plataformas: se prueba una sola vez ahí. Las adaptaciones propias de una plataforma se prueban junto a su implementación en `apps/<plataforma>/`.

| Proyecto   | Dónde está el test                                   | Entorno          |
| ---------- | ---------------------------------------------------- | ---------------- |
| `mobile`   | `apps/mobile/src/**/<archivo>.test.ts`               | `jest-expo`      |
| `web`      | `apps/web/**/<archivo>.test.ts`                      | `jest-expo/node` |
| `desktop`  | `apps/desktop/**/<archivo>.test.ts`                  | `jest-expo/node` |
| `packages` | `packages/<paquete>/**/<archivo>.test.ts`            | `jest-expo/node` |
| `ui`       | `packages/ui/**/<archivo>.test.ts` (hooks incluidos) | `jest-expo`      |

| Qué                      | Dónde                                                                  |
| ------------------------ | ---------------------------------------------------------------------- |
| Mocks de módulos nativos | `apps/mobile/jest.setup.ts` y `packages/ui/jest.setup.ts` (iconos); los módulos de Expo los mockea el preset |
| Alias `@/` (móvil)       | Apunta a `apps/mobile/src/`, igual que en la app                       |

Desde `life-frontend`:

```powershell
npm test                              # todas las pruebas
npm test -- -t "tamaño mínimo"        # solo las pruebas cuyo nombre coincide
```

### Regla del desarrollador

Los tests son obligatorios en los cambios: todo cambio de lógica (servicios, hooks, utilidades, validaciones) incluye su test junto al archivo que cambia, y `npm test` debe pasar antes de pedir revisión. Cada test tiene tres partes (preparación, qué se prueba, la prueba), como se explica en la convención. Un cambio sin test no se da por terminado. Si el cambio no es de lógica (solo documentación, estilos o configuración), se explica en la revisión por qué no lleva test.

### Convención

Cada prueba tiene tres partes separadas por una línea en blanco: preparación, qué se prueba y la prueba (las aserciones). El nombre dice el comportamiento esperado.

```ts
import { MIN_FONT_SIZE, typography } from '../typography';

describe('typography', () => {
  it('ninguna variante de texto baja del tamaño mínimo de fuente', () => {
    const tamanos = Object.values(typography).map((variante) => variante.fontSize);

    const menorTamano = Math.min(...tamanos);

    expect(menorTamano).toBeGreaterThanOrEqual(MIN_FONT_SIZE);
  });
});
```

## Versiones requeridas

| Herramienta | Versión | Dónde se fija |
|---|---|---|
| Node.js | 22 (`^22.13.0`) | `engines` de `package.json` y `.nvmrc` |
| npm | 10 o superior | `engines` de `package.json` |
| Dependencias | Versiones exactas resueltas | `package-lock.json` (se versiona) |
| Expo, React Native, React | Expo `~58`, React Native `0.88.1`, React `19.3.0` | `apps/mobile/package.json` |
| TypeScript | `~6.0.3` | `package.json` raíz y de cada app |

Con `nvm`, `nvm use` toma la versión de `.nvmrc`. `npm ci` instala exactamente lo que dice `package-lock.json`, así que da las mismas versiones en cualquier equipo.

### Política de actualización

- Las dependencias se actualizan en un ticket propio, nunca como parte de otro cambio, y el `package-lock.json` se versiona en el mismo commit.
- Expo, React Native y las librerías `expo-*` se actualizan juntas y se validan con `npx expo install --check` (desde `apps/mobile`); no se sube una de forma aislada.
- Las dependencias de Expo y React Native llevan `~` o versión exacta; solo las herramientas ajenas a Expo (por ejemplo la web con Vite) pueden usar `^`.
- Un cambio de versión mayor de Node actualiza `engines`, `.nvmrc` y esta sección en el mismo cambio.
- Tras actualizar, `npm run typecheck` y `npm run lint` deben terminar sin errores.

## Integración continua

El flujo `.github/workflows/frontend.yml` tiene dos jobs en GitHub Actions: `Compilar frontend` (instala con `npm ci` y ejecuta `npm run typecheck`) y `Validar código del frontend` (ejecuta, en pasos separados, `npm run typecheck`, `npm run lint` y `npm run format:check`). Se ejecutan en los pull requests hacia `review` y `main`, y en los push a `review`, `main` y `release/**`, cuando cambia algo en `life-frontend`. La versión de Node sale de `.nvmrc` y la caché de npm se basa en `package-lock.json`. No necesitan secretos. Las reglas que se aplican son las versionadas en el repositorio (ver "Formato y estilo").

Para que un error de validación bloquee el pull request, en GitHub (Settings > Branches > regla de protección de `review` y `main`) se activa "Require status checks to pass before merging" y se marcan los checks `Compilar frontend` y `Validar código del frontend`.

Para reproducir los mismos pasos en local, desde `life-frontend`:

```powershell
npm ci
npm run typecheck
npm run lint
npm run format:check
```

## Entornos

La app tiene tres entornos. Cada uno tiene un archivo versionado con sus variables `EXPO_PUBLIC_*`, que apuntan a la URL de la API de ese entorno (la API usa su propia base de datos, ver el README del backend):

| Entorno | Archivo | `EXPO_PUBLIC_API_URL` |
|---|---|---|
| Desarrollo | `.env.development` | `http://localhost:5041` |
| Pruebas | `.env.test` | `http://localhost:5042` |
| Producción | `.env.production` | `https://api.life.example.com` (marcador: se reemplaza por la URL real al aprovisionar producción) |

Las variables `EXPO_PUBLIC_*` se incrustan en el código de la app y son visibles para cualquiera que la instale, por eso solo llevan valores públicos como la URL de la API. Los secretos no van en estos archivos. Para valores locales propios (por ejemplo, la IP de tu equipo al probar en un dispositivo físico) se usa `.env.local`, que no se versiona.

En el emulador de Android `localhost` es el propio emulador: allí se usa `http://10.0.2.2:5041` en `.env.local`.

### Cambiar de entorno

Expo elige el archivo según `NODE_ENV`. Desde `life-frontend`, una vez creada la app:

```powershell
# Desarrollo (por defecto con expo start)
npx expo start

# Pruebas
$env:NODE_ENV = "test"
npx expo start

# Producción
$env:NODE_ENV = "production"
npx expo export
```

El código lee la URL con `process.env.EXPO_PUBLIC_API_URL` y nunca la escribe directamente.

## Variables de entorno y plantilla

`.env.example` es la plantilla de variables y se versiona sin valores reales. Para configurar un equipo local se copia a `.env.local` (ignorado por Git), que tiene prioridad sobre el archivo del entorno. Desde `life-frontend`:

```powershell
Copy-Item .env.example .env.local
```

| Variable | Qué es |
|---|---|
| `EXPO_PUBLIC_APP_ENV` | Nombre del entorno (`development`, `test` o `production`) |
| `EXPO_PUBLIC_API_URL` | URL de la API del entorno |

Solo las variables `EXPO_PUBLIC_*` llegan a la app, y por eso nunca llevan contraseñas, claves ni tokens. Cualquier valor sensible vive en el backend o en los secretos de CI de GitHub Actions (ver el README del backend).

## Formato y estilo

Las reglas están versionadas en el repositorio: `.prettierrc` y `.prettierignore` (Prettier: comillas simples, punto y coma, coma final, ancho de 100, finales de línea LF) y `.editorconfig` (indentación de 2 espacios, UTF-8, LF). El formato no depende de carpetas de IDE: `.idea/`, `.vs/` y `.vscode/` no se versionan (están en el `.gitignore` de la raíz). Para formatear al guardar basta activar Prettier en el editor, que lee `.prettierrc` y `.editorconfig` del proyecto.

Los finales de línea LF se fuerzan con el `.gitattributes` de la raíz del repositorio, para que `format:check` dé el mismo resultado en Windows y en el CI. El lint corre en las tres aplicaciones (`mobile`, `web` y `desktop`) y en todos los paquetes con `npm run lint`; cada uno tiene su `eslint.config.*`, y todos comparten la regla de `eslint.english.cjs` (ver "Idioma del código").

Prettier se integra con ESLint mediante `eslint-config-prettier`, que debe añadirse al final de `extends` (o como `prettier` en `eslint.config.js`) cuando exista la configuración de ESLint de la app, para que las reglas de lint no contradigan el formato.

Desde `life-frontend`:

```powershell
npm install

# Aplicar el formato
npm run format

# Comprobar sin modificar archivos
npm run format:check
```

### Idioma del código

Todo el código se escribe en inglés, igual que en el backend: nombres de componentes, funciones, hooks, variables, propiedades, tipos, carpetas y archivos, rutas y nombres de las pruebas. La documentación (README, `docs/`) y los textos que ve el usuario final siguen en español. ESLint lo comprueba de forma parcial con la regla `id-match` de `eslint.english.cjs`: los identificadores solo pueden llevar letras sin tilde, dígitos, `_` y `$`, de modo que `número` o `año` hacen fallar `npm run lint`. La regla no detecta palabras en español sin tilde (`lista`, `tarea`); eso se revisa en la revisión de código.

### Comentarios

No se agregan comentarios salvo que sean necesarios: solo cuando explican un porqué que el código no deja claro por sí mismo. No se comenta lo que el código ya expresa y no se deja código comentado.

## Reglas y estándares de desarrollo

Referencia para desarrollar, mantener y modificar el frontend (React Native, Expo, React y TypeScript). Las reglas de formato, idioma y comentarios de las secciones anteriores siguen vigentes; aquí se completan. Las reglas generales (puntos 1 a 4 y 7 a 12) son las mismas que las del README del backend, y las específicas de cada tecnología están en el punto 5.

### 1. Formato y análisis estático

- El código se formatea con las configuraciones versionadas: Prettier (`.prettierrc`), ESLint (`eslint.config.*`), TypeScript (`tsconfig.base.json` y el `tsconfig.json` de cada proyecto) y EditorConfig (`.editorconfig`).
- No se introducen configuraciones nuevas ni se modifican las existentes sin una justificación técnica, y ninguna regla nueva contradice las actuales (por ejemplo, ESLint no impone un formato distinto al de Prettier; ver `eslint-config-prettier`).
- Las mismas convenciones se aplican a todos los archivos equivalentes: un componente, un hook o una prueba nueva se formatea igual que los existentes, en las tres plataformas.

### 2. Idioma y nomenclatura

- Nombres de archivos, carpetas, variables, constantes, funciones, componentes, clases, interfaces, tipos y propiedades, en inglés (ver "Idioma del código").
- `PascalCase` para componentes, clases, interfaces y tipos; `camelCase` para variables, funciones, métodos y propiedades; `UPPER_SNAKE_CASE` para constantes globales (como `MIN_FONT_SIZE`). Los archivos de un componente o de una unidad de lógica llevan el nombre de lo que exportan.
- Un identificador existente en español se corrige solo cuando el cambio lo toca o lo requiere, y en el mismo cambio se actualizan todas sus referencias. Los ejemplos de código de este README que usan nombres en español son ilustrativos y no son un modelo a seguir.
- Los términos son los mismos que en el backend (por ejemplo `Workspace`, `Task`), para que un concepto tenga un único nombre en todo el proyecto.
- No se traducen los textos que ve el usuario ni el contenido de negocio que debe seguir en español.

### 3. Comentarios y limpieza

- En los archivos que se crean o modifican se eliminan los comentarios innecesarios, redundantes u obsoletos; no se explica código evidente. Se conservan los que necesitan las herramientas (`eslint-disable` justificado, directivas de TypeScript, `/** */` que alimenta el autocompletado del IDE) y la documentación externa y técnica existente.
- Se eliminan los imports, variables, funciones y código muerto sin uso, y las duplicaciones innecesarias. No se agregan abstracciones que no aporten valor.
- Los archivos y carpetas se organizan por responsabilidad, sin dependencias circulares entre módulos ni entre paquetes.

### 4. Tipado y seguridad de tipos

- TypeScript en modo estricto: las props, estados, eventos, respuestas de la API, parámetros y retornos de funciones públicas están tipados.
- No se usa `any`, `@ts-ignore` ni conversiones (`as`) que no sean necesarias. Si el tipo no se conoce, se usa `unknown` y se acota antes de usarlo.
- Para representar los datos se usan interfaces o tipos concretos (los compartidos viven en `@life/types`); se evitan los tipos genéricos cuando se conoce el concreto.
- Se aprovecha la inferencia de tipos cuando no resta claridad.

### 5. Reglas específicas del frontend

- Componentes funcionales y Hooks; no se usan componentes de clase.
- Un componente no crece de más: cuando mezcla presentación y lógica, la lógica se separa en un hook o una utilidad con su prueba.
- La lógica de negocio no va en los componentes de presentación (ver "Convenciones" de `@life/ui`).
- Las llamadas HTTP se centralizan en los servicios (`services/` en móvil y `@life/api-client`); los componentes y las pantallas no llaman a la API directamente. La URL sale de `EXPO_PUBLIC_API_URL`.
- Los estilos se mantienen organizados y reutilizables en el tema y en las props de `@life/ui`; no se repiten valores en cada pantalla.
- Mobile, web y escritorio comparten lo común en `packages/*`. No se duplica lógica entre plataformas; lo que difiere se separa con archivos `.web.tsx` y `.native.tsx`, y el cambio se comprueba en las plataformas afectadas.
- Se reutiliza el layout principal y los componentes compartidos (`ModulLayoutWeb`, `ModulHeader`, `ModulSidebar`, etc.). Los módulos funcionales entran como vistas dentro de ese layout y no crean uno propio.
- Los textos visibles pasan por `translate` (`@life/core`), que hoy es una implementación temporal y se reemplazará por ModulApp Translation: el resto del código solo depende de su firma, y no se crean mecanismos de traducción paralelos.
- Se evitan las dependencias circulares y el acoplamiento innecesario entre apps y paquetes (los paquetes no importan de las apps).

### 6. Servicios, dependencias y autocompletado

- Los servicios, hooks, contextos y proveedores (`useTheme`, `useMessages`, `useSession`) se usan con sus tipos concretos, sin perder la información de tipos con `any`, `object` ni conversiones.
- Se aprovechan el autocompletado, la navegación y el análisis estático del IDE; por eso los tipos se exportan y se importan por el paquete (`@life/types`, `@life/ui`) y no por rutas relativas entre paquetes.
- No se agregan mecanismos de resolución o de acceso alternativos cuando el proyecto ya tiene uno establecido (proveedores y hooks para el tema y los mensajes, y el cliente de la API).

### 7. Seguridad y dependencias

- No se versionan credenciales, tokens, secretos ni datos sensibles; las variables `EXPO_PUBLIC_*` solo llevan valores públicos (ver "Variables de entorno y plantilla").
- No se registra información sensible en la consola ni en logs.
- Se validan los datos de entrada (con `@life/validations`) y se respetan la autenticación, la autorización y los mecanismos de protección existentes.
- No se agregan dependencias innecesarias: se reutilizan las del proyecto, y las versiones se actualizan según la "Política de actualización", nunca por motivos estéticos.
- La configuración de entorno solo cambia con una justificación técnica.

### 8. Arquitectura y reutilización

- Se respeta la arquitectura actual (apps, paquetes compartidos y sistema de diseño) y la separación de responsabilidades.
- No se duplican componentes, hooks, servicios, utilidades ni lógica de negocio; no se crean sistemas independientes para lo que ya está centralizado (tema, mensajes, traducción, sesión).
- Se mantiene un único layout principal reutilizable, y los módulos reutilizan el layout y los componentes de `@life/ui`.

### 9. Reutilizar antes de crear y control del alcance

Esta regla tiene prioridad sobre las demás al implementar o modificar algo: reutilizar antes que crear, extender antes que duplicar y modificar solo lo necesario.

- Antes de implementar, se inspecciona el código para identificar componentes, hooks, servicios, utilidades, tipos y patrones reutilizables, y se usan de preferencia. No se crea una implementación nueva si ya existe una que cubra la necesidad; si hay que ampliarla, se modifica o se extiende la existente.
- Se implementa únicamente lo necesario para el objetivo. No se hacen refactorizaciones, mejoras ni cambios de comportamiento, de contratos o de componentes fuera del alcance.
- Se revisan las referencias de lo modificado para detectar efectos secundarios y se comprueba que no cambie nada fuera del alcance.
- Una mejora que exceda el alcance se indica en el informe final, sin implementarla.

### 10. Restricciones de modificación

- No se altera la funcionalidad existente por motivos de estilo.
- No se cambian contratos de la API, estructuras de datos ni reglas de negocio si el objetivo no lo requiere.
- No se eliminan pruebas existentes.
- No se reorganizan la arquitectura ni las carpetas sin necesidad, y no se agregan funcionalidades ajenas al objetivo.
- Si una corrección exige tocar funcionalidad o arquitectura fuera del alcance, se documenta el problema en lugar de cambiarlo.

### 11. Validación final

Cuando se modifica código, antes de pedir revisión se ejecuta, desde `life-frontend`:

```powershell
npm run format:check
npm run lint
npm run typecheck
npm test
npm run build -w @life/web
```

- El formato, el lint y el tipado no reportan errores, ni imports o referencias sin uso.
- Las pruebas existentes y las de lo modificado pasan.
- No cambiaron contratos ni comportamientos funcionales, y se revisaron los efectos secundarios en las funcionalidades relacionadas.
- Los errores que introdujo el cambio se corrigen; los problemas que ya existían y no se pueden corregir sin ampliar el alcance se identifican y se reportan.
- No se afirma que una validación fue exitosa si no se ejecutó.

### 12. Informe de resultados

Al terminar un cambio de código, el resumen incluye: (1) archivos creados, modificados y eliminados; (2) reglas de formato y estilo aplicadas; (3) identificadores corregidos al inglés; (4) comentarios eliminados y anotaciones técnicas conservadas; (5) limpieza realizada; (6) problemas de tipado detectados y corregidos; (7) resultado de la compilación; (8) resultado de las pruebas; (9) problemas preexistentes o pendientes; (10) posibles mejoras fuera del alcance.
