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
  api-client/   cliente HTTP compartido de la API
  core/         lógica común (sesión, traducción, cliente con sesión, catálogo de módulos)
  theme/        colores, tipografía, dimensiones y estilos del sistema de diseño
  ui/           componentes compartidos (@life/ui)
  icons/        iconos por plataforma (@life/icons)
package.json        workspaces y scripts de la raíz
tsconfig.base.json  opciones de TypeScript compartidas (modo estricto)
```

Los paquetes se importan por su nombre (`@life/types`, `@life/constants`, `@life/utils`, `@life/validations`, `@life/api-client`, `@life/core`, `@life/theme`, `@life/ui`, `@life/icons`) y se declaran como dependencia (`"@life/constants": "*"`) en la app que los usa. Son TypeScript puro, sin imports específicos de plataforma: `packages/eslint.config.mjs` rechaza `react-native`, `expo*` y `react-dom`. Las únicas excepciones son `@life/ui` y `@life/icons`.

### Móvil

Las rutas usan Expo Router: cada archivo de `apps/mobile/src/app/` es una pantalla y `_layout.tsx` define el navegador. El alias `@/` apunta a `apps/mobile/src/`.

```
apps/mobile/src/
  app/          rutas y layout raíz (Expo Router)
  components/   componentes propios de la app
  hooks/        hooks compartidos
  services/     acceso a la API y otros servicios
  styles/       estilos globales reutilizables y tipos de sus props
  types/        tipos de TypeScript
  test/         utilidades de prueba
```

La navegación principal es una barra de pestañas inferior. Las pantallas con pestañas (Inicio, Bandeja, Planner y Módulos) viven en el grupo de rutas `app/(tabs)/`, cuyos paréntesis evitan que `tabs` aparezca en la URL (siguen siendo `/`, `/inbox`, `/planner` y `/modules`). `(tabs)/_layout.tsx` muestra `MobileHeader` (usuario a la izquierda y campana de notificaciones a la derecha) sobre las pestañas, y `MobileTabBar` dibuja la barra con `ModulTabBar` de `@life/ui`. Las pestañas salen de `MOBILE_TABS` en `@life/core`, y el texto de cada una es solo la etiqueta de accesibilidad. El botón flotante `+` abre la hoja inferior de nueva captura (`ModulBottomSheet`), que sube desde abajo y no es una ruta. Perfil y Ajustes quedan fuera de las pestañas; `MobileHeader` recibe `title` para mostrar la flecha de volver en esas pantallas, y Perfil enlaza a Ajustes. La campana y los contenidos de la hoja de captura, Bandeja, Planner, Módulos y Perfil son por ahora marcadores de posición.

### Web

`apps/web/src/main.tsx` monta el enrutador, `App.tsx` define las rutas, las páginas viven en `apps/web/src/pages/` y el layout en `apps/web/src/layout/`.

### Escritorio

`apps/desktop/src-tauri/` contiene la configuración de Tauri (`tauri.conf.json`) y el código de Rust. La ventana carga la app web: en desarrollo desde `http://localhost:5173` y en producción desde `apps/web/dist`. Los permisos están limitados a `core:default`, `core:window:allow-set-title` y `core:window:allow-close` (`capabilities/default.json`).

**Ventana nativa.** La ventana usa la barra de título del sistema operativo, abre en 1100×720 y no baja de 800×600 (`tauri.conf.json`). El plugin `tauri-plugin-window-state` (`src-tauri/src/lib.rs`) recuerda su tamaño y posición entre sesiones.

- **Título:** `ModulApp Life — <sección actual>`, calculado en `buildWindowTitle` (`packages/core/desktopMenu`) y aplicado por el hook `useDesktopWindow` (`apps/web/src/desktop`), que solo actúa dentro de Tauri (`isTauri()`).
- **Menú nativo:** `buildDesktopMenu` define su estructura y los textos salen de `translate`; el hook lo instala con `@tauri-apps/api/menu`. En Windows y Linux, Archivo incluye nueva captura, ajustes, recargar y salir; en macOS, ajustes y salir pasan al menú de la aplicación. «Ir a» lista Inicio y los módulos habilitados. Las acciones de navegación usan las mismas rutas que la interfaz. En Windows la barra del menú queda oculta (comando `hide_menu` de `src-tauri/src/lib.rs`, que el hook invoca tras instalarlo); el menú sigue definido.
- **Atajos:** `Ctrl/Cmd+N` nueva captura, `Ctrl/Cmd+,` ajustes, `Ctrl/Cmd+R` recargar, `Ctrl/Cmd+Q` salir y `Ctrl/Cmd+1…9` para Inicio y los módulos.
- **Captura:** la ruta `/captura` (`CAPTURE_SECTION`) es un marcador de posición; su contenido llega con el módulo Captura. No es un módulo habilitable ni aparece en el menú lateral.

### Control de versiones

No se versionan `node_modules`, artefactos de compilación (`dist/`, `.expo/`, `web-build/`), archivos de entorno (`.env*` salvo los de cada entorno y `.env.example`, ver "Entornos") ni claves o certificados (ver `.gitignore`).

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
npm run format     # aplica el formato (Prettier)
npm run format:check  # comprueba el formato sin modificar archivos
npm test           # pruebas unitarias (Jest)
```

Para compilar la web: `npm run build -w @life/web`. Para detener `npm run desktop`, `Ctrl+C` en la terminal; el aviso `STATUS_CONTROL_C_EXIT` de `cargo` es normal.

Antes de usar APIs de Expo o React Native se consulta la documentación de la versión instalada (ver `AGENTS.md`).

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

## Entornos y variables

La app tiene tres entornos. Cada uno tiene un archivo versionado con sus variables `EXPO_PUBLIC_*`, que apuntan a la URL de la API de ese entorno (la API usa su propia base de datos, ver el README del backend). Las tres apps usan los mismos nombres de variable:

| Entorno | Archivo | `EXPO_PUBLIC_APP_ENV` | `EXPO_PUBLIC_API_URL` |
|---|---|---|---|
| Desarrollo | `.env.development` | `development` | `http://localhost:5041` |
| Pruebas | `.env.test` | `test` | `http://localhost:5042` |
| Producción | `.env.production` | `production` | `https://api.life.example.com` (marcador: se reemplaza por la URL real al aprovisionar producción) |

Las variables `EXPO_PUBLIC_*` se incrustan en el código de la app y son visibles para cualquiera que la instale, por eso solo llevan valores públicos como la URL de la API; nunca contraseñas, claves ni tokens. Cualquier valor sensible vive en el backend o en los secretos de CI de GitHub Actions (ver el README del backend).

Se mantiene el prefijo `EXPO_PUBLIC_` también en la web (`envPrefix` de Vite) para que las tres apps compartan variables; no se usa `VITE_*`.

### Dónde está cada archivo

| App | Archivos de entorno | Plantilla |
|---|---|---|
| Móvil | `apps/mobile/.env.development`, `.env.test`, `.env.production` (Expo lee los `.env` de `apps/mobile`, no los de la raíz) | `apps/mobile/.env.example` |
| Web | `.env.*` de la raíz de `life-frontend` (`envDir` en `vite.config.ts`) | `apps/web/.env.example` |
| Escritorio | los mismos de la raíz, porque empaqueta la web | `apps/desktop/.env.example` |

Los archivos de cada entorno son públicos y se versionan; los de valores propios (`.env.local`) los ignora Git. Una prueba (`apps/desktop/src-tauri/envFiles.test.ts`) comprueba que los de la app móvil coinciden con los de la raíz y con los perfiles de `apps/mobile/eas.json`: al cambiar una URL se cambia en los tres lugares.

Para valores locales propios (por ejemplo, la IP de tu equipo al probar en un dispositivo físico) se copia la plantilla a `.env.local` (tiene prioridad sobre el archivo del entorno): en `apps/mobile/.env.local` para el móvil y en `life-frontend/.env.local` para la web y el escritorio. En el emulador de Android `localhost` es el propio emulador: allí se usa `http://10.0.2.2:5041`. Desde `life-frontend`:

```powershell
Copy-Item apps/mobile/.env.example apps/mobile/.env.local
Copy-Item .env.example .env.local
```

### Configuración tipada y error al iniciar

Cada app lee las variables en un único módulo (`apps/mobile/src/config/env.ts` y `apps/web/src/config/env.ts`), que usa `readAppEnv` de `@life/core`. Devuelve `{ appEnv, apiUrl }` y, si falta una variable o es inválida (ambiente distinto de `development`, `test` o `production`, o URL que no es http/https), lanza un error que nombra cada variable al iniciar la app. El resto del código importa `env` de ese módulo y no lee `process.env` ni `import.meta.env`.

### Cambiar de entorno

Desde `life-frontend`:

```powershell
# Móvil: Expo elige el archivo según NODE_ENV (desarrollo por defecto)
npm run mobile
$env:NODE_ENV = "test"; npm run mobile

# Web: Vite elige el archivo según el modo
npm run web                       # desarrollo
npm run build -w @life/web        # producción (.env.production)
npm run build:test -w @life/web   # pruebas (.env.test)

# Escritorio: empaqueta la web con el ambiente
npm run build -w @life/desktop        # producción
npm run build:test -w @life/desktop   # pruebas
```

### Compilaciones del móvil con EAS

`apps/mobile/eas.json` define los perfiles de EAS Build con sus variables: `development` (cliente de desarrollo, ambiente `development`), `preview` (distribución interna, ambiente `test`) y `production` (ambiente `production`). Desde `apps/mobile`:

```powershell
npx eas-cli@latest build --profile preview --platform android
```

La firma de las apps, el despliegue en tiendas y la firma de los instaladores de escritorio quedan fuera de esta configuración. El `projectId` de EAS lo agrega `eas init` cuando exista la cuenta de Expo.

## Cliente de la API

Todas las llamadas a la API salen por el cliente de `@life/api-client` (`createApiClient`); las pantallas y los componentes no usan `fetch` directamente. Cada app crea su instancia en `services/apiClient/` con `createSessionApiClient` de `@life/core`, que lo conecta con la sesión: toma el token de `session.getState().user?.token` y cierra la sesión (`signOut`) cuando la API responde 401.

Cada petición lleva `Accept`, `X-Correlation-Id` (un identificador por petición, el mismo al repetirla) y `Authorization: Bearer <token>` si hay sesión. El token no se escribe en logs. Tiene un tiempo de espera de 15 s (`API_DEFAULT_TIMEOUT_MS`, configurable con `timeoutMs` por cliente o por petición) y acepta un `AbortSignal` para cancelarla.

Los métodos `get`, `post`, `put`, `patch` y `delete` nunca lanzan excepciones: devuelven un `ApiResult<T>` (`@life/types`) que se distingue por `kind`:

| `kind` | Cuándo |
|---|---|
| `success` | `2xx`, con `data` (`null` si no hay cuerpo) |
| `badRequest` | `400`, con `problem.errors` por campo |
| `unauthorized` | `401` |
| `forbidden` | `403`, con `problem` |
| `notFound` | `404`, con `problem` |
| `conflict` | `409`, con `problem` |
| `serverError` | `5xx`, sin el cuerpo de la respuesta |
| `unexpected` | cualquier otro código, o un `2xx` con cuerpo que no es JSON |
| `networkError` | la petición no llegó a la API |
| `timeout` | se superó el tiempo de espera |
| `cancelled` | quien llama canceló la petición |

`problem` sigue el formato de error estandarizado de la API (`title`, `detail`, `errors`). Ante un `401`, si el cliente recibe `refreshSession`, la ejecuta (una sola vez aunque haya varias peticiones a la vez) y repite la petición una vez; si no hay renovación o el `401` persiste, llama a `onSessionExpired`. Mostrar los errores en pantalla corresponde a cada vista.
## Estado de conectividad

`@life/core` expone el estado de conectividad de la app. `connectivityStore` guarda `online` (hay red **y** el servidor responde) y `useConnectivity()` devuelve `{ online, status }`, donde `status` es `offline`, `syncing` (con conexión y `pendingOperations > 0` en la sesión) o `synced`. Cualquier componente puede leerlo; el indicador visual es `ModulConnectionIndicator` (`@life/ui`), que se muestra en `MobileHeader` y en el encabezado web (y de escritorio).

`createConnectivityMonitor` mantiene el estado: escucha la red del dispositivo (`expo-network` en móvil, los eventos `online`/`offline` del navegador en web y escritorio) y comprueba la alcanzabilidad del servidor con `GET api/v1/house` (`createServerProbe`; un `networkError`, un `timeout` o un `5xx` cuentan como no alcanzable) al arrancar, al cambiar la red y cada 15 s. Un cambio de estado solo se publica si se mantiene 2 s, para que la interfaz no parpadee. Cada app lo inicia al montarse desde `services/connectivity/`. La sincronización de datos y la cola de operaciones pendientes no forman parte de este módulo.

## Almacenamiento seguro de la sesión

Los tokens de la sesión (acceso y renovación) se guardan con la interfaz común `SessionTokenStorage` de `@life/core` (`load`, `save`, `clear`), con una implementación por plataforma. `createSessionServices` conecta el almacenamiento, el gestor de sesión (`createSessionManager`) y el cliente de la API; cada app lo crea en `services/session/`.

| Plataforma | Dónde se guardan | Entrega del token de renovación |
|---|---|---|
| Mobile | `expo-secure-store` (Keychain en iOS, Keystore en Android), con `WHEN_UNLOCKED_THIS_DEVICE_ONLY` | En el cuerpo de la respuesta; se guarda en el dispositivo |
| Escritorio (Tauri) | Almacén de credenciales del sistema, con el crate `keyring` y los comandos `secure_get`, `secure_set` y `secure_delete` (`apps/desktop/src-tauri/src/lib.rs`) | En el cuerpo de la respuesta; se guarda en el sistema |
| Web | El token de acceso solo en memoria; el de renovación en la cookie `HttpOnly` que pone el servidor (cabecera `X-Token-Delivery: cookie`, peticiones con `credentials: 'include'`) | Cookie; el código de la página nunca lo ve |

- **Al iniciar la app** se llama a `sessionManager.restore()`: reabre la sesión con el token de acceso vigente o, si venció, lo renueva con `POST /api/v1/auth/refresh`. En web no hay tokens en memoria tras recargar, así que renueva con la cookie.
- **Al autenticarse**, `sessionManager.signIn(credenciales)` llama a `POST /api/v1/auth/login` y guarda los tokens. Las vistas de inicio de sesión usan este método y muestran el resultado.
- **Al cerrar sesión**, `sessionManager.signOut()` borra los tokens del dispositivo, cierra la sesión y llama a `POST /api/v1/auth/logout` para revocarlos y borrar la cookie.
- **Si el servidor rechaza la sesión** (renovación con `401` o `400`), se borran los tokens y `session.expireSession()` pide iniciar sesión de nuevo sin poner a cero `pendingOperations`: los cambios pendientes de sincronizar se conservan. Un fallo de red al renovar no borra los tokens.
- Los tokens no se escriben en logs ni en `localStorage`. El nombre del usuario sale del claim `email` del token mientras no exista un perfil. Un valor guardado ilegible se descarta.
- En escritorio, la URL del origen de la app (`tauri://localhost` o `http://tauri.localhost`) debe estar en `Cors:AllowedOrigins` de la API. En Windows el Credential Manager limita cada credencial a unos 1280 caracteres: los tokens actuales caben con margen.

## Pantallas de acceso

Registro, inicio de sesión, recuperación de acceso, nueva contraseña y confirmación del correo, en mobile (`apps/mobile/src/app/`) y web y escritorio (`apps/web/src/pages/`). Todas se arman con `ModulAuthForm` y la lógica común vive en `@life/core`:

- `authValidation`: reglas de correo y contraseña iguales a las del backend (contraseña de 10 a 128 caracteres con mayúscula, minúscula y número; código de 6 dígitos).
- `authErrors`: convierte el resultado de la API en un mensaje genérico, para no revelar si un correo existe.
- `authService`: `register`, `verifyEmail`, `forgotPassword` y `resetPassword` sobre el cliente de la API.
- `useAuthForm`: errores por campo y del servidor, y bloqueo de envíos duplicados mientras se procesa (`createAuthSubmitter`).

Rutas:

| Pantalla | Mobile | Web y escritorio |
|---|---|---|
| Bienvenida | `/welcome` (primera sin sesión) | |
| Iniciar sesión | `/login` | `/login` |
| Registro | `/register` | `/register` |
| Recuperar acceso | `/forgot-password` | `/forgot-password` |
| Nueva contraseña | `/reset-password?token=` | `/reset-password?token=` |
| Confirmar correo | `/verify-email?email=` | `/verify-email?email=` |

- El registro termina con un diálogo y lleva a la confirmación, que pide el código de 6 dígitos enviado al correo (`POST /api/v1/auth/verify-email`). Sin `email` en la ruta, la confirmación redirige al inicio de sesión.
- La recuperación y la nueva contraseña usan `/api/v1/auth/forgot-password` y `/api/v1/auth/reset-password`, que todavía no existen en el backend: las pantallas están listas y responden con un error genérico hasta que se implementen.
- Los accesos con Google y Apple no tienen lógica: el botón de Google muestra "Próximamente".
- En mobile, `index` redirige a `/welcome` cuando no hay sesión.
- Para probar en un celular, `EXPO_PUBLIC_API_URL` en `apps/mobile/.env.local` debe apuntar a la IP de la PC y la API debe escuchar en toda la red (perfil `mobile` de `launchSettings.json`).

## Sistema de diseño

Los valores visuales viven en `packages/theme/` y los componentes en `packages/ui/` (las rutas de esta sección son relativas a `packages/`). Los componentes no usan valores fijos propios: consumen los estilos y colores del tema.

| Archivo | Contenido |
| --- | --- |
| `theme/colors/colors.ts` | Paleta clara (`lightColors`) y oscura (`darkColors`), y el tipo `ThemeColors` |
| `theme/typography/typography.ts` | Variantes de texto (tamaño, interlineado y peso) y límites de escala de fuente |
| `theme/dimension/dimension.ts` | Layout, breakpoints y escalas responsivas |
| `theme/styles/styles.ts` | `createViewStyles` (vistas) y `createTextStyles` (texto) |

Cada valor se cambia en su archivo y se refleja en todo lo que lo usa. Un color en `colors.ts` cambia todos los estilos que lo leen (`bg`, `bgPrimary`, `textBrand`, etc.).

### Componentes compartidos (`@life/ui`)

`packages/ui` agrupa los componentes que usan la app móvil, la web y el escritorio (Tauri muestra la web). Se importan desde un único punto y reciben los estilos como props booleanas, sin `style`; el texto recibe una variante:

```tsx
import { ModulButton, ModulText, ModulView } from '@life/ui';

<ModulView flex1 alignCenter justifyCenter gap16 p16 bg>
  <ModulText variant="screenTitle" textBrand>
    My tasks
  </ModulText>
  <ModulButton label="Save" onPress={save} />
</ModulView>
```

| Ruta (en `ui/`) | Contenido |
| --- | --- |
| `components/atoms/` | `ModulView`, `ModulText`, `ModulButton`, `ModulFloatingButton`, `ModulIcon`, `ModulFieldMessage`, `ModulSkeleton`, `ModulBadge`, `ModulAvatar`, `ModulProgress`, `ModulChip`, `ModulModal`, `ModulDrawer`, `ModulDatePicker` |
| `components/molecules/` | `ModulInput`, `ModulSwitch`, `ModulCheckbox`, `ModulChipGroup`, `ModulLoadingState`, `ModulEmptyState`, `ModulErrorState`, `ModulStateContainer`, `ModulOptionGroup`, `ModulListItem`, `ModulList`, `ModulMenu`, `ModulToast`, `ModulConfirmDialog`, `ModulMessages` (`ModulMessagesProvider`), `ModulBarChart`, `ModulCircularProgress`, `ModulDragDrop` (`ModulDragProvider`, `ModulDraggable`, `ModulDropZone`), `ModulHeader`, `ModulSidebar`, `ModulSidebarRail`, `ModulTabBar` |
| `components/organisms/` | `ModulDialog`, `ModulBottomSheet`, `ModulListPattern`, `ModulFormContainer` (`toFieldErrors`) |
| `components/templates/mobile/` | `ModulListView`, `ModulFormView`, `ModulDetailView`, `ModulAuthView`, `ModulSettingsView`, `ModulAgendaView` |
| `components/templates/web/` | `ModulPageWeb`, `ModulListViewWeb`, `ModulFormViewWeb`, `ModulDetailViewWeb`, `ModulAuthViewWeb`, `ModulSettingsViewWeb`, `ModulAgendaViewWeb` |
| `components/layouts/` | `ModulLayoutWeb`, el layout principal reutilizable |
| `theme/ThemeProvider.tsx` | Tema claro, oscuro o del sistema (`ThemeProvider`, `useTheme`) |
| `utils/` | `useStyles`, `useBreakpoint` y funciones auxiliares |
| `testing/` | `renderWithTheme` para las pruebas de componentes |

#### Átomos y moléculas

- `ModulText`: `variant` define el tamaño; los títulos (`screenTitle`, `sectionTitle`, `headerTitle`) llevan rol de encabezado. Con `link` se muestra como enlace (color de marca, subrayado y rol de enlace) y se usa con `onPress`.
- `ModulButton`: `variant` es `primary`, `secondary` o `danger` (`danger` queda por compatibilidad); las pantallas de acceso usan además `brand` (relleno morado), `brandOutline` (contorno morado) y `outline` (contorno neutro), con radio de 12, y `large` da un alto de 52. Con `icon` y sin `label` es un botón de solo icono y exige `accessibilityLabel`.
- Estados de los componentes: `ModulStateContainer` decide qué mostrar según el resultado de una consulta (carga, error, vacío o `children`, en ese orden de prioridad) y delega en `ModulLoadingState`, `ModulErrorState` y `ModulEmptyState`; todos los textos llegan ya traducidos por props y el reintento invoca `onRetry`. `ModulLoadingState` se anuncia como ocupado con `message` como nombre y acepta un `skeleton` (`ModulSkeleton`, bloque estático y oculto a lectores de pantalla) en lugar del indicador. `ModulErrorState` y `ModulFieldMessage` (con error) llevan un icono además del color. `ModulButton` acepta `loading` (bloquea y se anuncia ocupado), `success` y `error`; `ModulInput` acepta `loading` y `success`. Hover y presionado usan los tokens `hover` y `pressed` del tema, y el esqueleto el token `skeleton`, todos con valores en tema claro y oscuro. Las animaciones de los componentes salen de `utils/animations/animations.ts` (`animateTo`, `springTo`, con las duraciones y curvas comunes), agrupadas por tipo en regiones.
- `ModulFloatingButton`: botón de solo icono anclado abajo a la derecha; va dentro de un contenedor que ocupe la pantalla (`ModulView flex1`). Abre lo que defina la pantalla desde `onPress`.
- `ModulInput`: un solo campo con `type` (`text`, `password` con botón de mostrar y ocultar, `number`, `textarea`, `search` con botón de borrar, `date` y `time` con máscara `DD/MM/AAAA` y `HH:mm`, `select` con `options`), etiqueta, ayuda y error incluidos. `actionLabel` es el nombre accesible del botón del final del campo. Con `variant="floating"` el campo tiene borde de 12 y la etiqueta va dentro como texto guía: al enfocar o escribir sube animada al borde y vuelve a su lugar si queda vacío.
- `ModulSwitch` y `ModulCheckbox` (`type` `checkbox` o `radio`; `round` para la casilla redonda de tarea): controlados desde afuera con `checked` y `onChange`.
- `ModulBadge` (insignia y contador), `ModulAvatar` (imagen o iniciales), `ModulProgress` (valor de 0 a 100) y `ModulChip`. `ModulChipGroup` muestra los chips en un deslizador horizontal, o como control segmentado con `segmented`. `ModulChip` con `onRemove` es un chip de filtro activo.
- `ModulList`: lista virtualizada (solo renderiza lo cercano a la pantalla) con estados de carga, error y vacío; sus filas son `ModulListItem` (icono, descripción, acciones secundarias, `compact`). `ModulMenu` es el menú contextual y el selector de `ModulInput` lo reutiliza.
- `ModulMessagesProvider` (una vez por app, dentro de `ThemeProvider`) habilita `useMessages()`.
- `ModulDragProvider`, `ModulDraggable` y `ModulDropZone`: se arrastra con ratón o dedo; con teclado o lector de pantalla se activa el elemento y luego la zona. `ModulBarChart` y `ModulCircularProgress` dibujan con vistas y colores del tema, sin SVG.

```tsx
<ModulInput label="Email" value={email} onChangeText={setEmail} error={emailError} />
<ModulText link onPress={goToRecovery}>Forgot my password</ModulText>
<ModulFloatingButton icon="AddCircleIcon" accessibilityLabel="New task" onPress={openNewTask} />

const { toast, confirm } = useMessages();
if (await confirm({ title: 'Delete the task?', destructive: true })) toast('Task deleted', { type: 'success' });
```

Cada app envuelve su raíz con `<ThemeProvider>` y `<ModulMessagesProvider>`. La web resuelve `react-native` a `react-native-web` con un alias en `apps/web/vite.config.ts`.

#### Organismos, patrones y plantillas

Se arman solo con átomos y moléculas, sin diseño propio, y los textos llegan por props.

- `ModulDialog` y `ModulBottomSheet`: envuelven a `ModulModal` (que no cambia) con título y botón de cierre. Al abrirse llevan el foco al título; se cierran con el botón de retroceso de Android, con Escape en web y con el botón de cierre. `ModulBottomSheet` es el mismo diálogo anclado abajo. Se abren desde cualquier pantalla con `visible` y `onClose`; el contenido lo define cada pantalla.
- `ModulListPattern`: búsqueda (`ModulInput` `search`), filtros opcionales (`ModulChipGroup`) y `ModulList` con sus estados de carga, error y vacío.
- `ModulFormContainer`: muestra los errores generales del servidor (`serverErrors`) como alerta y las acciones de enviar y cancelar; `submitting` las deshabilita. `toFieldErrors(problem.errors)` convierte los errores por campo del servidor (`ApiProblem.errors`) en el `error` de cada `ModulInput`.
- `ModulAuthForm`: formulario de las pantallas de acceso. Recibe `fields` (cada uno es un `ModulInput` flotante), `submitLabel`, `serverError`, `aside`, `links`, `providers` y `result` (reemplaza el formulario por un mensaje con una acción). En mobile usa `ModulAuthView` con `variant="brand"`; en web y escritorio, con `brand` (nombre y mensaje), usa `ModulAuthViewWeb` con `layout="split"` (panel de marca y formulario).
- Plantillas de pantalla, una por plataforma. Móvil (`templates/mobile/`, una sola columna y controles táctiles de 44): `ModulListView`, `ModulFormView`, `ModulDetailView`, `ModulAuthView`, `ModulSettingsView` y `ModulAgendaView`. Web (`templates/web/`, sufijo `Web`, controles `compact` de 32): `ModulPageWeb` es la estructura común (contenido centrado de 1120 como máximo y encabezado con título y acciones) y las demás se apoyan en ella. `ModulListViewWeb` pone búsqueda y filtros en una fila y la lista en una tarjeta; `ModulFormViewWeb` muestra los campos en dos columnas dentro de una tarjeta de 720; `ModulDetailViewWeb` suma un panel lateral (`aside`); `ModulSettingsViewWeb` lista las secciones a la izquierda y muestra la elegida a la derecha; `ModulAgendaViewWeb` pone el título y la barra de herramientas en la misma fila; `ModulAuthViewWeb` centra el formulario en una tarjeta (o, con `layout="split"`, lo pone junto a un panel de marca). `ModulButton` y `ModulInput` aceptan `compact`.

```tsx
<ModulListScreen title="Tasks" searchLabel="Search" searchValue={query} onSearchChange={setQuery} data={tasks} renderItem={renderTask} emptyTitle="No tasks" />
<ModulBottomSheet visible={open} title="Filters" closeLabel="Close" onClose={() => setOpen(false)}>{filters}</ModulBottomSheet>
<ModulInput label="Email" value={email} onChangeText={setEmail} error={toFieldErrors(problem.errors).Email} />
```

### Convenciones de los componentes

- Los componentes empiezan con `Modul` y tienen props tipadas.
- Colores: siempre desde `ThemeColors`; nunca un hexadecimal dentro de un componente.
- Texto: el tamaño, interlineado y peso base salen de `typography`; el color y el peso extra, de las props (`textSecondary`, `textBrand`, `fwBold`, etc.).
- Los textos llegan por props; los componentes no llevan textos fijos ni lógica de negocio.
- Para agregar un estilo nuevo: la entrada en `createViewStyles` o `createTextStyles`; el tipo de la prop se actualiza solo.
- Si un comportamiento difiere por plataforma (hover, foco, área táctil), se agrega un archivo `.web.tsx` o `.native.tsx` junto al componente.
- Contraste: los pares de texto y fondo cumplen WCAG AA (4.5:1) en tema claro y oscuro; los bordes de interfaz, 3:1.
- Escalas responsivas: `useStyles` aplica `spacingScale` según el ancho de pantalla (teléfono, tablet o escritorio) al `padding`, `margin` y `gap` de los estilos del tema.
- `@life/ui` es, junto con `@life/icons`, el único paquete que importa `react-native`; la excepción está en `packages/eslint.config.mjs`.

## Pruebas unitarias

Las pruebas verifican la lógica de servicios, hooks y utilidades con Jest y el preset `jest-expo`, configurados en `jest.config.js` (un proyecto por zona). Cubren el caso exitoso, el error y los valores límite. Los componentes de `@life/ui` también se prueban (lo que muestran y cómo responden a la interacción); no hay pruebas de integración ni de extremo a extremo. Las versiones de `jest` y `jest-expo` deben ser las que `npx expo install` resuelve para la versión de Expo instalada. Los hooks se prueban con `renderHook` y los componentes con `renderWithTheme` (`packages/ui/testing/`), ambos de `@testing-library/react-native`. La configuración y las dependencias de pruebas del frontend no se comparten con las del backend.

Cada archivo vive en su propia carpeta (por ejemplo `utils/clampPercent/clampPercent.ts`) y su test va en esa misma carpeta, con el nombre `<archivo>.test.ts` (o `.test.tsx`); no hay una carpeta `tests/` centralizada. La lógica compartida vive en `packages/*` y la usan las tres plataformas: se prueba una sola vez ahí. Las adaptaciones propias de una plataforma se prueban junto a su implementación en `apps/<plataforma>/`.

| Proyecto | Dónde está el test | Entorno |
| --- | --- | --- |
| `mobile` | `apps/mobile/src/**/<archivo>.test.ts` | `jest-expo` |
| `web` | `apps/web/**/<archivo>.test.ts` | `jest-expo/node` |
| `desktop` | `apps/desktop/**/<archivo>.test.ts` | `jest-expo/node` |
| `packages` | `packages/<paquete>/**/<archivo>.test.ts` | `jest-expo/node` |
| `ui` | `packages/ui/**/<archivo>.test.ts` (hooks incluidos) | `jest-expo` |

| Qué | Dónde |
| --- | --- |
| Mocks de módulos nativos | `apps/mobile/jest.setup.ts` y `packages/ui/jest.setup.ts` (iconos); los módulos de Expo los mockea el preset |
| Alias `@/` (móvil) | Apunta a `apps/mobile/src/`, igual que en la app |

Desde `life-frontend`:

```powershell
npm test                              # todas las pruebas
npm test -- typography                # solo los archivos de prueba cuya ruta coincide
npm test -- -t "minimum font size"    # solo las pruebas cuyo nombre coincide
```

### Convención

Cada prueba tiene tres partes separadas por una línea en blanco: preparación, qué se prueba y la prueba (las aserciones). El nombre dice el comportamiento esperado, en inglés.

```ts
import { MIN_FONT_SIZE, typography } from '../typography';

describe('typography', () => {
  it('no text variant goes below the minimum font size', () => {
    const sizes = Object.values(typography).map((variant) => variant.fontSize);

    const smallestSize = Math.min(...sizes);

    expect(smallestSize).toBeGreaterThanOrEqual(MIN_FONT_SIZE);
  });
});
```

## Integración continua

El flujo `.github/workflows/frontend.yml` tiene dos jobs en GitHub Actions: `Compilar frontend` (instala con `npm ci` y ejecuta `npm run typecheck`) y `Validar código del frontend` (ejecuta, en pasos separados, `npm run typecheck`, `npm run lint` y `npm run format:check`). Se ejecutan en los pull requests hacia `review` y `main`, y en los push a `review`, `main` y `release/**`, cuando cambia algo en `life-frontend`. La versión de Node sale de `.nvmrc` y la caché de npm se basa en `package-lock.json`. No necesitan secretos. Las reglas que se aplican son las versionadas en el repositorio (ver "Formato y estilo"). El CI no ejecuta `npm test`: las pruebas se corren en local antes de pedir revisión (ver "Validación final").

Para que un error de validación bloquee el pull request, en GitHub (Settings > Branches > regla de protección de `review` y `main`) se activa "Require status checks to pass before merging" y se marcan los checks `Compilar frontend` y `Validar código del frontend`.

Para reproducir los mismos pasos en local, desde `life-frontend`:

```powershell
npm ci
npm run typecheck
npm run lint
npm run format:check
```

## Reglas y estándares de desarrollo

Referencia para desarrollar, mantener y modificar el frontend (React Native, Expo, React y TypeScript). Las reglas generales (puntos 1 a 5 y 7 a 9) son las mismas que las del README del backend; las específicas de cada tecnología están en el punto 6.

### 1. Regla del desarrollador

Esta regla tiene prioridad sobre las demás al implementar o modificar algo: reutilizar antes que crear, extender antes que duplicar y modificar solo lo necesario.

- Antes de implementar, se inspecciona el código para identificar componentes, hooks, servicios, utilidades, tipos y patrones reutilizables, y se usan de preferencia. No se crea una implementación nueva si ya existe una que cubra la necesidad; si hay que ampliarla, se modifica o se extiende la existente.
- Se implementa únicamente lo necesario para el objetivo. No se hacen refactorizaciones, mejoras ni cambios de comportamiento, de contratos o de componentes fuera del alcance. Una mejora que lo exceda se indica en el informe final, sin implementarla.
- Se revisan las referencias de lo modificado para detectar efectos secundarios y se comprueba que no cambie nada fuera del alcance.
- Los tests son obligatorios: todo cambio de lógica (servicios, hooks, utilidades, validaciones) incluye su test junto al archivo que cambia, con la estructura de tres partes de "Pruebas unitarias", y `npm test` debe pasar antes de pedir revisión. Si el cambio no es de lógica (solo documentación, estilos o configuración), se explica en la revisión por qué no lleva test.
- No se altera la funcionalidad existente por motivos de estilo.
- No se cambian contratos de la API, estructuras de datos ni reglas de negocio si el objetivo no lo requiere.
- No se eliminan pruebas existentes.
- No se reorganizan la arquitectura ni las carpetas sin necesidad, y no se agregan funcionalidades ajenas al objetivo.
- Si una corrección exige tocar funcionalidad o arquitectura fuera del alcance, se documenta el problema en lugar de cambiarlo.

### 2. Formato y estilo

- El código se formatea con las configuraciones versionadas: `.prettierrc` y `.prettierignore` (Prettier: comillas simples, punto y coma, coma final, ancho de 100, finales de línea LF), `.editorconfig` (indentación de 2 espacios, UTF-8, LF), ESLint (`eslint.config.*`) y TypeScript (`tsconfig.base.json` y el `tsconfig.json` de cada proyecto). Para formatear al guardar basta activar Prettier en el editor, que lee `.prettierrc` y `.editorconfig`.
- El formato no depende de carpetas de IDE: `.idea/`, `.vs/` y `.vscode/` no se versionan (están en el `.gitignore` de la raíz).
- Los finales de línea LF se fuerzan con el `.gitattributes` de la raíz del repositorio, para que `format:check` dé el mismo resultado en Windows y en el CI.
- El lint corre en las tres aplicaciones (`mobile`, `web` y `desktop`) y en todos los paquetes con `npm run lint`; cada uno tiene su `eslint.config.*`.
- Prettier se integra con ESLint mediante `eslint-config-prettier` (ya incluido en la configuración de `mobile`), para que las reglas de lint no contradigan el formato.
- No se introducen configuraciones nuevas ni se modifican las existentes sin una justificación técnica, y ninguna regla nueva contradice las actuales.
- Las mismas convenciones se aplican a todos los archivos equivalentes: un componente, un hook o una prueba nueva se formatea igual que los existentes, en las tres plataformas.

```powershell
npm run format          # aplicar el formato
npm run format:check    # comprobar sin modificar archivos
```

### 3. Idioma del código y nomenclatura

- Todo el código se escribe en inglés: nombres de componentes, funciones, hooks, variables, constantes, propiedades, tipos, interfaces, carpetas y archivos, rutas y nombres de las pruebas. La documentación (README) y los textos que ve el usuario final siguen en español; no se traducen ni el contenido de negocio que debe seguir en español.
- `PascalCase` para componentes, clases, interfaces y tipos; `camelCase` para variables, funciones, métodos y propiedades; `UPPER_SNAKE_CASE` para constantes globales (como `MIN_FONT_SIZE`). Los archivos de un componente o de una unidad de lógica llevan el nombre de lo que exportan.
- ESLint lo comprueba de forma parcial con la regla `id-match` de `eslint.english.cjs`, compartida por las tres apps y los paquetes: los identificadores solo pueden llevar letras sin tilde, dígitos, `_` y `$`, de modo que `número` o `año` hacen fallar `npm run lint`. La regla no detecta palabras en español sin tilde (`lista`, `tarea`); eso se revisa en la revisión de código.
- Un identificador existente en español se corrige solo cuando el cambio lo toca o lo requiere, y en el mismo cambio se actualizan todas sus referencias.
- Los términos son los mismos que en el backend (por ejemplo `Workspace`, `Task`), para que un concepto tenga un único nombre en todo el proyecto.

### 4. Comentarios y limpieza

- No se agregan comentarios salvo que sean necesarios: solo cuando explican un porqué que el código no deja claro por sí mismo. No se comenta lo que el código ya expresa y no se deja código comentado.
- En los archivos que se crean o modifican se eliminan los comentarios innecesarios, redundantes u obsoletos. Se conservan los que necesitan las herramientas (`eslint-disable` justificado, directivas de TypeScript, `/** */` que alimenta el autocompletado del IDE) y la documentación externa y técnica existente.
- Se eliminan los imports, variables, funciones y código muerto sin uso, y las duplicaciones innecesarias. No se agregan abstracciones que no aporten valor.
- Los archivos y carpetas se organizan por responsabilidad, sin dependencias circulares entre módulos ni entre paquetes.

### 5. Tipado y seguridad de tipos

- TypeScript en modo estricto: las props, estados, eventos, respuestas de la API, parámetros y retornos de funciones públicas están tipados.
- No se usa `any`, `@ts-ignore` ni conversiones (`as`) que no sean necesarias. Si el tipo no se conoce, se usa `unknown` y se acota antes de usarlo.
- Para representar los datos se usan interfaces o tipos concretos (los compartidos viven en `@life/types`); se evitan los tipos genéricos cuando se conoce el concreto.
- Se aprovecha la inferencia de tipos cuando no resta claridad.

### 6. Reglas específicas del frontend

- Componentes funcionales y Hooks; no se usan componentes de clase.
- Un componente no crece de más: cuando mezcla presentación y lógica, la lógica se separa en un hook o una utilidad con su prueba. La lógica de negocio no va en los componentes de presentación.
- Las llamadas HTTP se centralizan en los servicios (`services/` en móvil y web, y `@life/api-client`); los componentes y las pantallas no llaman a la API directamente. La URL sale de `env.apiUrl` (`EXPO_PUBLIC_API_URL`, ver "Entornos y variables").
- Los estilos se mantienen organizados y reutilizables en el tema y en las props de `@life/ui`; no se repiten valores en cada pantalla.
- Mobile, web y escritorio comparten lo común en `packages/*`. No se duplica lógica entre plataformas; lo que difiere se separa con archivos `.web.tsx` y `.native.tsx`, y el cambio se comprueba en las plataformas afectadas.
- Se mantiene un único layout principal reutilizable (`ModulLayoutWeb`, `ModulHeader`, `ModulSidebar`, `ModulTabBar`, etc.). Los módulos funcionales entran como vistas dentro de ese layout y no crean uno propio.
- Los textos visibles pasan por `translate` (`@life/core`), que hoy es una implementación temporal y se reemplazará por ModulApp Translation: el resto del código solo depende de su firma, y no se crean mecanismos de traducción paralelos.
- Los servicios, hooks, contextos y proveedores (`useTheme`, `useMessages`, `useSession`) se usan con sus tipos concretos, sin perder la información de tipos con `any`, `object` ni conversiones. No se agregan mecanismos de acceso alternativos cuando el proyecto ya tiene uno (proveedores y hooks para el tema y los mensajes, y el cliente de la API).
- Los tipos se exportan e importan por el paquete (`@life/types`, `@life/ui`) y no por rutas relativas entre paquetes, para aprovechar el autocompletado y la navegación del IDE.
- Se respeta la arquitectura actual (apps, paquetes compartidos y sistema de diseño) y la separación de responsabilidades: los paquetes no importan de las apps, y no se duplican componentes, hooks, servicios ni utilidades, ni se crean sistemas independientes para lo que ya está centralizado (tema, mensajes, traducción, sesión).

### 7. Seguridad y dependencias

- No se versionan credenciales, tokens, secretos ni datos sensibles; las variables `EXPO_PUBLIC_*` solo llevan valores públicos (ver "Entornos y variables").
- No se registra información sensible en la consola ni en logs.
- Se validan los datos de entrada (con `@life/validations`) y se respetan la autenticación, la autorización y los mecanismos de protección existentes.
- No se agregan dependencias innecesarias: se reutilizan las del proyecto, y las versiones se actualizan según la "Política de actualización", nunca por motivos estéticos.
- La configuración de entorno solo cambia con una justificación técnica.

### 8. Validación final

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

### 9. Informe de resultados

Al terminar un cambio de código, el resumen incluye: (1) archivos creados, modificados y eliminados; (2) reglas de formato y estilo aplicadas; (3) identificadores corregidos al inglés; (4) comentarios eliminados y anotaciones técnicas conservadas; (5) limpieza realizada; (6) problemas de tipado detectados y corregidos; (7) resultado de la compilación; (8) resultado de las pruebas; (9) problemas preexistentes o pendientes; (10) posibles mejoras fuera del alcance.
