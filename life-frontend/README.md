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
  types/        tipos de TypeScript
```

No se versionan `node_modules`, artefactos de compilación (`dist/`, `.expo/`, `web-build/`), archivos de entorno (`.env*`) ni claves o certificados (ver `.gitignore`).

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
