# life-frontend

Aplicación de ModulApp Life (React + TypeScript, React Native con Expo).

Este proyecto todavía no tiene código. La estructura y las instrucciones de ejecución se documentarán cuando se cree la app.

## Formato y estilo

Las reglas están versionadas en el repositorio: `.prettierrc` y `.prettierignore` (Prettier: comillas simples, punto y coma, coma final, ancho de 100, finales de línea LF) y `.editorconfig` (indentación de 2 espacios, UTF-8, LF). `.vscode/settings.json` activa Prettier al guardar y `.vscode/extensions.json` recomienda las extensiones necesarias.

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
