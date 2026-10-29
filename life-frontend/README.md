# life-frontend

Aplicación de ModulApp Life (React + TypeScript, React Native con Expo).

Este proyecto todavía no tiene código. La estructura y las instrucciones de ejecución se documentarán cuando se cree la app.

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
