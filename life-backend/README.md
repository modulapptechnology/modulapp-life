# life-backend

API de ModulApp Life (.NET 10, Clean Architecture, EF Core + SQL Server).

## Estructura de capas

La solución es `life-backend.slnx` y contiene cinco proyectos:

| Proyecto | Responsabilidad | Referencia a |
|---|---|---|
| `Domain` | Entidades y reglas de negocio | Ninguno |
| `Application` | Casos de uso e interfaces que implementan las capas externas | `Domain` |
| `Infrastructure` | Servicios externos (correo, archivos, integraciones) | `Application`, `Domain` |
| `Persistence` | Acceso a datos con EF Core y SQL Server | `Application`, `Domain` |
| `API` | Endpoints HTTP y punto de entrada | `Application`, `Infrastructure`, `Persistence` |

### Regla de dependencias

Las dependencias apuntan hacia adentro: `Domain` no referencia a ningún proyecto y no se permiten referencias distintas a las de la tabla.

### Convenciones

- Cada proyecto vive en su propia carpeta, con el mismo nombre que su `.csproj`.
- Los nombres de proyecto no llevan prefijo (`API`, no `Life.API`).
- El código de un módulo de negocio se organiza por funcionalidad dentro de cada capa (por ejemplo `Tareas/`), no por tipo técnico.
- Los secretos, cadenas de conexión y archivos de entorno no se versionan: se usan `user-secrets` o `appsettings.Development.local.json`.

## Inyección de dependencias

Cada capa expone un método de extensión que `API` invoca en `Program.cs`:

| Capa | Método | Qué registra |
|---|---|---|
| `Application` | `AddApplication()` | Casos de uso |
| `Infrastructure` | `AddInfrastructure()` | Servicios externos |
| `Persistence` | `AddPersistence()` | `DbContext` y repositorios |

Tiempos de vida:

- **Singleton**: una instancia para toda la aplicación (configuración, clientes HTTP compartidos).
- **Scoped**: una instancia por petición HTTP (casos de uso, `DbContext`, repositorios).
- **Transient**: una instancia nueva cada vez que se pide (servicios livianos y sin estado).

Las capas internas dependen de interfaces definidas en `Application`; las implementaciones se registran en `Infrastructure` y `Persistence`.

## Formato y estilo

Las reglas están en `.editorconfig` (indentación de 4 espacios, finales de línea LF, UTF-8, `using` fuera del namespace con `System` primero, namespaces con alcance de archivo, llaves siempre y nombres: PascalCase para tipos y miembros, camelCase para parámetros y variables, `_camelCase` para campos privados e `I` para interfaces). Visual Studio, Rider y VS Code las leen sin configuración adicional, así que el formato no depende de carpetas de IDE: `.idea/`, `.vs/` y `.vscode/` no se versionan y están en el `.gitignore`.

Desde `life-backend`:

```powershell
# Aplicar el formato
dotnet format life-backend.slnx

# Comprobar sin modificar archivos
dotnet format life-backend.slnx --verify-no-changes
```

### Comentarios

No se agregan comentarios salvo que sean necesarios: solo cuando explican un porqué que el código no deja claro por sí mismo. No se comenta lo que el código ya expresa y no se deja código comentado.

## Versiones requeridas

| Herramienta | Versión | Dónde se fija |
|---|---|---|
| SDK de .NET | 10.0.100 o superior de la banda 10.0.1xx (`latestFeature`) | `global.json` |
| Framework destino | `net10.0` | `<TargetFramework>` de cada `.csproj` |
| Paquetes NuGet | Una versión por paquete para toda la solución | `Directory.Packages.props` |

Las versiones de los paquetes NuGet se declaran solo en `Directory.Packages.props`; los `.csproj` referencian el paquete sin `Version`. Para agregar o actualizar un paquete se cambia únicamente ese archivo.

### Política de actualización

- Las versiones se actualizan en un ticket propio, nunca como parte de otro cambio.
- Los paquetes `Microsoft.*` de una misma línea (por ejemplo `10.0.x`) se actualizan juntos.
- Un cambio de versión mayor del SDK o del framework destino actualiza `global.json`, los `.csproj` y esta sección en el mismo cambio.
- Tras actualizar, `dotnet restore` y `dotnet build life-backend.slnx` deben terminar sin errores ni advertencias nuevas.

## Compilar y ejecutar

Desde `life-backend`:

```powershell
dotnet restore
dotnet build life-backend.slnx
dotnet run --project API
```

La API escucha en `http://localhost:5041` y `https://localhost:7183`. Para levantar la base de datos local antes de ejecutarla, ver "Docker para desarrollo local".

## Docker para desarrollo local

`docker-compose.yml` define los servicios que el proyecto necesita en desarrollo. Hoy tiene uno:

| Servicio | Imagen | Contenedor | Puerto | Datos |
|---|---|---|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | `life-sqlserver` | `1433` | Volumen `sqlserver-data` (se conservan al detener el contenedor) |

Requisito: Docker Desktop (o Docker Engine con el plugin Compose).

### Contraseña

La contraseña del usuario `sa` se lee de la variable `MSSQL_SA_PASSWORD`, definida en un `.env` local que no se versiona. La plantilla `.env.example` sí se versiona y no lleva valores reales. Desde `life-backend`:

```powershell
Copy-Item .env.example .env
```

Se edita `.env` con una contraseña que cumpla la política de SQL Server (8 o más caracteres con mayúsculas, minúsculas, números y símbolos). Sin esa variable, `docker compose` se detiene con un mensaje de error.

### Iniciar y detener

Desde `life-backend`:

```powershell
docker compose up -d          # inicia los contenedores
docker compose ps             # muestra el estado de cada contenedor
docker compose down           # detiene y elimina los contenedores (conserva los datos)
docker compose down -v        # además borra el volumen con los datos
```

### Apuntar la API a la base del contenedor

Se guarda la cadena de conexión con `user-secrets` (la misma contraseña de `.env`). Desde `life-backend`:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=master;User Id=sa;Password=<contraseña de .env>;TrustServerCertificate=True" --project API
```

Se usa `Database=master` porque la base `life_dev` se crea con las migraciones, que no forman parte de esta configuración. Con la API en marcha (`dotnet run --project API`), en Desarrollo:

- `http://localhost:5041/swagger` abre Swagger UI (el documento está en `/openapi/v1.json`).
- `GET http://localhost:5041/house/database` responde `200` con `{"database":"master","status":"connected"}` cuando la API alcanza SQL Server, y `500` si no puede.

### Ampliar los servicios

Cuando el proyecto necesite otro servicio (caché, cola de mensajes, correo de pruebas), se agrega un bloque bajo `services:` en `docker-compose.yml`, con imagen fija, puerto, volumen si guarda datos y sus variables en `.env` y `.env.example`. Después se agrega una fila a la tabla de arriba. Los Dockerfiles de la API y de la app, y los contenedores en CI y producción, no forman parte de esta configuración.

## Integración continua

El flujo `.github/workflows/backend.yml` restaura y compila `life-backend.slnx` en GitHub Actions. Se ejecuta en los pull requests hacia `review` y `main`, y en los push a `review`, `main` y `release/**`, cuando cambia algo en `life-backend`. La versión del SDK sale de `global.json` y los paquetes de NuGet se guardan en caché según `Directory.Packages.props` y los `.csproj`. La compilación no necesita secretos.

Para que una compilación fallida bloquee el pull request, en GitHub (Settings > Branches > regla de protección de `review` y `main`) se activa "Require status checks to pass before merging" y se marca el check `Compilar backend`.

Para reproducir el mismo paso en local: `dotnet restore life-backend.slnx` y `dotnet build life-backend.slnx --configuration Release`.

## Entornos

El backend tiene tres entornos, que se eligen con la variable `ASPNETCORE_ENVIRONMENT`. Cada uno lee `appsettings.json` y encima `appsettings.<Entorno>.json`:

| Entorno | `ASPNETCORE_ENVIRONMENT` | Archivo | Base de datos (`Database:Name`) | URL local |
|---|---|---|---|---|
| Desarrollo | `Development` | `appsettings.Development.json` | `life_dev` | `http://localhost:5041` |
| Pruebas | `Testing` | `appsettings.Testing.json` | `life_test` | `http://localhost:5042` |
| Producción | `Production` | `appsettings.Production.json` | `life_prod` | La define el despliegue |

Cada entorno usa su propia base de datos: los datos de producción no se usan fuera de producción. Los archivos versionados solo llevan el nombre de la base; el servidor, el usuario y la contraseña no se versionan (ver Convenciones).

### Cambiar de entorno

Desde `life-backend`:

```powershell
# Desarrollo (por defecto con dotnet run)
dotnet run --project API

# Pruebas
dotnet run --project API --launch-profile testing

# Producción (sin perfil de lanzamiento; la variable la define el servidor)
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project API --no-launch-profile
```

Los ajustes propios de un entorno se agregan en su `appsettings.<Entorno>.json`, nunca en el código. Las cadenas de conexión y los secretos se configuran con `user-secrets` o `appsettings.Development.local.json`.

## Variables de entorno y secretos

La configuración se lee de las fuentes estándar de ASP.NET Core, de menor a mayor prioridad: `appsettings.json`, `appsettings.<Entorno>.json`, `user-secrets` (solo en desarrollo), `appsettings.Development.local.json` (solo en desarrollo) y variables de entorno. Los archivos versionados no llevan secretos ni cadenas de conexión.

| Clave | Qué es | Dónde se define |
|---|---|---|
| `ConnectionStrings:Default` | Cadena de conexión a SQL Server | `user-secrets` o `appsettings.Development.local.json` en desarrollo; variable `ConnectionStrings__Default` en el resto |

La clave `Database:Name` (nombre de la base de cada entorno) sí se versiona en `appsettings.<Entorno>.json`.

### Configuración local

Opción A, `user-secrets` (recomendada; el secreto queda fuera del repositorio). Desde `life-backend`:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "<cadena de conexión local>" --project API
dotnet user-secrets list --project API
```

Opción B, archivo local ignorado por Git. Desde `life-backend/API`:

```powershell
Copy-Item appsettings.Development.local.example.json appsettings.Development.local.json
```

Después se reemplaza el valor de `ConnectionStrings:Default` en `appsettings.Development.local.json`. La plantilla `appsettings.Development.local.example.json` sí se versiona y solo contiene un valor de ejemplo.

### Secretos de CI

En GitHub Actions los secretos se definen en el repositorio (Settings > Secrets and variables > Actions) y se pasan al proceso como variable de entorno; nunca se escriben en el flujo de trabajo. Secreto requerido: `DB_CONNECTION_STRING`, usado así:

```yaml
env:
  ConnectionStrings__Default: ${{ secrets.DB_CONNECTION_STRING }}
```

Los secretos de producción se gestionan fuera de este repositorio.

## Estructura de la API

La estructura común de la API vive en el proyecto `API`:

- `Extensions/ApiServiceCollectionExtensions.cs` registra controladores, versionado, OpenAPI y CORS.
- `Extensions/ApiApplicationBuilderExtensions.cs` define el pipeline de middleware; el orden de las llamadas es el orden de ejecución (Swagger en Desarrollo, HTTPS, CORS, autorización, controladores). Los nuevos middleware se agregan ahí, no en `Program.cs`.
- `Controllers/` contiene los controladores.

### Convención de rutas y versionado

- La ruta base es `api/v{version}/[controller]`; la versión va en el segmento de URL (`/api/v1/house`). Sin versión en la URL la API responde `404`.
- Todo controlador hereda de `ApiControllerBase`, que ya declara `[ApiController]`, la versión `1.0` y la ruta.
- Los controladores no contienen lógica de negocio: reciben la petición y delegan en `Application`.
- Una versión nueva se declara con `[ApiVersion("2.0")]` en los controladores que la soporten.

### CORS

Solo se permiten los orígenes de `Cors:AllowedOrigins`, definidos por ambiente en `appsettings.<Entorno>.json`:

| Ambiente | Orígenes |
|---|---|
| Desarrollo | `http://localhost:5173` (web con Vite), `http://localhost:8081` (Expo web), `http://tauri.localhost` y `tauri://localhost` (escritorio) |
| Pruebas | Ninguno |
| Producción | Ninguno hasta que se configuren (por variable de entorno `Cors__AllowedOrigins__0`, etc.) |

Un origen no configurado no recibe la cabecera `Access-Control-Allow-Origin` y el navegador bloquea la respuesta.

### Swagger y `API.http`

En Desarrollo, `http://localhost:5041/swagger` abre Swagger UI y el documento OpenAPI está en `/openapi/v1.json`. `API/API.http` trae peticiones de ejemplo para la ruta base versionada y para comprobar CORS.

## Control de versiones

El `.gitignore` excluye `bin/`, `obj/`, los archivos de entorno, los secretos y los archivos de IDE (`.idea/`, `.vs/`, `.vscode/`). Tras compilar, `git status` no debe mostrar archivos de `bin/` ni `obj/`.
