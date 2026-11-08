# life-backend

API de ModulApp Life (.NET 10, Clean Architecture, EF Core + SQL Server).

## Estructura de capas

La solución es `life-backend.slnx` y contiene cinco proyectos de producción, más `Support` (biblioteca de pruebas) y los proyectos de `tests/`:

| Proyecto | Responsabilidad | Referencia a |
|---|---|---|
| `Domain` | Entidades y reglas de negocio | Ninguno |
| `Application` | Casos de uso e interfaces que implementan las capas externas | `Domain` |
| `Infrastructure` | Servicios externos (correo, archivos, integraciones) | `Application`, `Domain` |
| `Persistence` | Acceso a datos con EF Core y SQL Server | `Application`, `Domain` |
| `API` | Endpoints HTTP y punto de entrada | `Application`, `Infrastructure`, `Persistence` |

### Regla de dependencias

Las dependencias apuntan hacia adentro: `Domain` no referencia a ningún proyecto y no se permiten referencias distintas a las de la tabla.

### Convenciones de la solución

- Cada proyecto vive en su propia carpeta, con el mismo nombre que su `.csproj`.
- Los nombres de proyecto no llevan prefijo (`API`, no `Life.API`).
- El código de un módulo de negocio se organiza por funcionalidad dentro de cada capa (por ejemplo `Tasks/`), no por tipo técnico.
- Los secretos, cadenas de conexión y archivos de entorno no se versionan: se usan `user-secrets` o `appsettings.Development.local.json`.

## Inyección de dependencias

Cada capa expone un método de extensión que `API` invoca en `Program.cs`:

| Capa | Método | Qué registra |
|---|---|---|
| `Application` | `AddApplication()` | Casos de uso y validadores |
| `Infrastructure` | `AddInfrastructure()` | Servicios externos |
| `Persistence` | `AddPersistence(configuration)` | `DbContext` y repositorios |

Tiempos de vida:

- **Singleton**: una instancia para toda la aplicación (configuración, clientes HTTP compartidos).
- **Scoped**: una instancia por petición HTTP (casos de uso, `DbContext`, repositorios).
- **Transient**: una instancia nueva cada vez que se pide (servicios livianos y sin estado).

Las capas internas dependen de interfaces definidas en `Application`; las implementaciones se registran en `Infrastructure` y `Persistence`.

## Versiones requeridas

| Herramienta | Versión | Dónde se fija |
|---|---|---|
| SDK de .NET | 10.0.100 o superior de la banda 10.0.1xx (`latestFeature`) | `global.json` |
| Framework destino | `net10.0` | `<TargetFramework>` de cada `.csproj` |
| Paquetes NuGet | Una versión por paquete para toda la solución | `Directory.Packages.props` |
| `dotnet-ef` | Igual a la versión de EF Core | `.config/dotnet-tools.json` |

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

La API escucha en `http://localhost:5041` y, con el perfil `https`, también en `https://localhost:7183`. Para levantar la base de datos local antes de ejecutarla, ver "Docker para desarrollo local".

## Entornos y configuración

El backend tiene tres entornos, que se eligen con la variable `ASPNETCORE_ENVIRONMENT`. Cada uno lee `appsettings.json` y encima `appsettings.<Entorno>.json`:

| Entorno | `ASPNETCORE_ENVIRONMENT` | Archivo | Base de datos (`Database:Name`) | URL local |
|---|---|---|---|---|
| Desarrollo | `Development` | `appsettings.Development.json` | `ModulLifeDev` | `http://localhost:5041` |
| Pruebas | `Testing` | `appsettings.Testing.json` | `ModulLifeApiTest` | `http://localhost:5042` |
| Producción | `Production` | `appsettings.Production.json` | `ModulLifeProd` | La define el despliegue |

Cada entorno usa su propia base de datos: los datos de producción no se usan fuera de producción. Los archivos versionados solo llevan el nombre de la base; el servidor, el usuario y la contraseña no se versionan.

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

Los ajustes propios de un entorno se agregan en su `appsettings.<Entorno>.json`, nunca en el código.

### Variables de entorno y secretos

La configuración se lee de las fuentes estándar de ASP.NET Core, de menor a mayor prioridad: `appsettings.json`, `appsettings.<Entorno>.json`, `user-secrets` (solo en desarrollo), `appsettings.Development.local.json` (solo en desarrollo) y variables de entorno. Los archivos versionados no llevan secretos ni cadenas de conexión.

| Clave | Qué es | Dónde se define |
|---|---|---|
| `ConnectionStrings:Default` | Cadena de conexión a SQL Server | `user-secrets` o `appsettings.Development.local.json` en desarrollo; variable `ConnectionStrings__Default` en el resto |
| `ConnectionStrings:Test` | Cadena del servidor para las pruebas de integración | `user-secrets` de `Support` o variable `ConnectionStrings__Test` |
| `Database:Name` | Nombre de la base de cada entorno | Se versiona en `appsettings.<Entorno>.json` |
| `Cors:AllowedOrigins` | Orígenes permitidos por CORS | Se versiona en `appsettings.<Entorno>.json`; en producción, variable `Cors__AllowedOrigins__0`, etc. |

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

Se usa `Database=master` porque la base `ModulLifeDev` se crea con las migraciones, que no forman parte de esta configuración. Con la API en marcha (`dotnet run --project API`), en Desarrollo:

- `http://localhost:5041/swagger` abre Swagger UI (el documento está en `/openapi/v1.json`).
- `GET http://localhost:5041/house/database` responde `200` con `{"database":"master","status":"connected"}` cuando la API alcanza SQL Server, y `500` si no puede.

### Ampliar los servicios

Cuando el proyecto necesite otro servicio (caché, cola de mensajes, correo de pruebas), se agrega un bloque bajo `services:` en `docker-compose.yml`, con imagen fija, puerto, volumen si guarda datos y sus variables en `.env` y `.env.example`. Después se agrega una fila a la tabla de arriba. Los Dockerfiles de la API y de la app, y los contenedores en CI y producción, no forman parte de esta configuración.

## Acceso a la base de datos

`Persistence` accede a SQL Server con EF Core (`Microsoft.EntityFrameworkCore.SqlServer`). `AddPersistence(configuration)` registra `LifeDbContext` con tiempo de vida Scoped y la cadena `ConnectionStrings:Default` del ambiente (ver "Variables de entorno y secretos"). La cadena nunca se versiona. Si falta, el error se produce al pedir el `DbContext` por primera vez, con un mensaje que indica dónde configurarla; la API no falla al arrancar. El modelo de datos de los módulos se agrega después en `Persistence`.

### Base común de persistencia

Toda entidad hereda de `BaseEntity` (`Domain/Common`) y `Persistence` le aplica sin configuración adicional la misma configuración (`ApplyBaseEntityConfiguration` en `LifeDbContext`):

| Campo | Comportamiento |
|---|---|
| `Id` (GUID) | Lo puede generar el cliente o el backend: si llega con valor se conserva; si llega vacío, EF genera uno secuencial. |
| `CreatedAt`, `UpdatedAt` | En UTC; los fija `LifeDbContext` al guardar (la creación no se modifica después). |
| `IsDeleted`, `DeletedAt` | Eliminación lógica: `Remove` marca el registro como eliminado en lugar de borrarlo y las consultas lo excluyen por defecto; se incluye con `IgnoreQueryFilters()`. |
| `RowVersion` | Versión de fila (`rowversion`) para control de concurrencia, expuesta a los clientes. Para detectar un conflicto al sincronizar, el caso de uso asigna la versión que envió el cliente a `Entry(entity).Property(e => e.RowVersion).OriginalValue`; si está desactualizada, `SaveChanges` lanza `DbUpdateConcurrencyException`. |

Todas las propiedades `DateTime` del modelo se guardan en UTC y se leen con `Kind = Utc` (`UtcDateTimeConverter`).

**Transacción por caso de uso.** `Application` define `IUnitOfWork`, registrado Scoped por `AddPersistence`. Cada caso de uso que modifica datos ejecuta su lógica con `ExecuteInTransactionAsync`: guarda los cambios y confirma al terminar, y ante cualquier excepción revierte todo. Respeta la política de reintentos del `DbContext`, por lo que la operación puede ejecutarse más de una vez y no debe tener efectos fuera de la base.

**Historial de versiones.** Una entidad con contenido de texto que conserva su historial declara `IHasVersionHistory` (`Domain/Common`), un marcador sin miembros. El periodo de retención se configura en `VersionHistory:RetentionDays` (90 por defecto, en `appsettings.json`) y se lee con `VersionHistoryOptions`; un valor no numérico o menor que 1 hace fallar `AddPersistence`. El almacenamiento y la consulta del historial no forman parte de esta base.

### Modelo de usuarios

`User` y `ExternalAccount` (`Domain/Users`) heredan de `BaseEntity`, así que tienen GUID, auditoría, eliminación lógica y versión de fila. Se guardan en las tablas `Users` y `ExternalAccounts`, configuradas en `Persistence/Configuration` y expuestas en `LifeDbContext` (`Users`, `ExternalAccounts`).

| Campo de `User` | Detalle |
|---|---|
| `Email` | Obligatorio, hasta 320 caracteres. Se compara sin distinguir mayúsculas (columna con collation `SQL_Latin1_General_CP1_CI_AS`, fija y no depende de la base). Índice único filtrado `[IsDeleted] = 0`: la base rechaza dos usuarios activos con el mismo correo; el de un usuario eliminado lógicamente se puede volver a registrar. |
| `PasswordHash` | Solo el hash de la contraseña; el modelo no tiene ningún campo con la contraseña en texto plano. Es opcional para las cuentas que solo ingresan con una cuenta externa. El registro y el inicio de sesión se describen en "Registro con correo y contraseña" e "Inicio de sesión con correo y contraseña". |
| `FailedLoginAttempts`, `LockedUntil` | Contraseñas incorrectas seguidas (0 por defecto) y fin del bloqueo temporal (vacío si no está bloqueado). |
| `EmailVerifiedAt` | Momento en que el usuario confirmó su correo; vacío mientras no lo confirme. |
| `Name` | Obligatorio, hasta 200 caracteres. |
| `Status` | `Pending` (valor inicial), `Active` o `Blocked`; se guarda como número. |
| `AvatarUrl`, `Locale`, `TimeZone` | Datos de perfil opcionales. |

`ExternalAccount` vincula un usuario con una cuenta de un proveedor externo: `UserId` (obligatorio, sin borrado en cascada), `Provider` (hasta 50) y `ProviderUserId` (hasta 256). El par proveedor + identificador es único entre las cuentas no eliminadas, por lo que la misma cuenta externa no se vincula a dos usuarios.

### Registro con correo y contraseña

`AuthController` expone el registro y la confirmación del correo. Los casos de uso son `RegisterUser` y `VerifyEmail` (`Application/Users`); `AddInfrastructure(configuration)` registra el hash, el generador de tokens y el envío de correo, y `AddPersistence` el `IUserRepository`.

| Endpoint | Resultado |
|---|---|
| `POST /api/v1/auth/register` con `{ "email", "password" }` | `201` con `{ id, email }` y envía el correo de verificación. `409` si el correo ya está registrado (sin distinguir mayúsculas, sin datos adicionales). `400` con el detalle por campo si el correo o la contraseña no son válidos. `429` al superar el límite de intentos. |
| `GET /api/v1/auth/verify-email?token=<token>` | `204` y el usuario pasa a `Active` con `EmailVerifiedAt`. `400` si el enlace no existe, ya se usó o venció (mismo mensaje para los tres casos). `429` al superar el límite. |

- **Política de contraseña** (`RegisterUserRequestValidator`): de 10 a 128 caracteres, con al menos una mayúscula, una minúscula y un número. La contraseña nunca se guarda ni se devuelve: se almacena solo el hash PBKDF2-SHA256 con sal aleatoria y 600 000 iteraciones (`Pbkdf2PasswordHasher`, formato `PBKDF2-SHA256$<iteraciones>$<sal>$<hash>`).
- **Enlace de verificación**: token aleatorio de 32 bytes, de un solo uso. En `EmailVerificationTokens` solo se guarda su hash SHA-256 (`TokenHash`, único entre los no eliminados), con `ExpiresAt` y `UsedAt`. El usuario queda `Pending` hasta confirmar; un usuario `Blocked` conserva su estado y solo registra `EmailVerifiedAt`.
- **Correo**: se envía después de confirmar la transacción. Si el envío falla se registra el error (sin el correo ni el token en el mensaje) y el registro igual responde `201`; reenviar el enlace no forma parte de este cambio.
- **Límite de frecuencia**: política `Registration` de ASP.NET Core, ventana fija por IP, que aplican los dos endpoints. Detrás de un proxy hay que configurar los encabezados reenviados para que la IP sea la del cliente.

| Configuración | Detalle |
|---|---|
| `EmailVerification:VerificationUrl` | **Obligatoria.** URL absoluta del endpoint de confirmación; el enlace del correo es esa URL con `?token=`. En Desarrollo y Pruebas está en `appsettings.<Entorno>.json`; en Producción se define por variable de entorno (`EmailVerification__VerificationUrl`). Sin ella la API no inicia. |
| `EmailVerification:TokenLifetimeMinutes` | Vigencia del enlace; 1440 (24 horas) por defecto. |
| `Email:Provider` | `Smtp` (por defecto) o `Log`. `Log` solo escribe el enlace en el log y se usa en Desarrollo y Pruebas; no se usa en Producción. |
| `Email:Smtp:Host`, `Port` (587), `EnableSsl` (`true`), `UserName`, `Password`, `FromAddress` | Servidor SMTP. El usuario y la contraseña van en `user-secrets` o variables de entorno (`Email__Smtp__Password`), nunca en archivos versionados. |
| `RateLimiting:Registration:PermitLimit` / `WindowSeconds` | Intentos permitidos por IP y duración de la ventana; 5 por 600 segundos por defecto (1000 en Pruebas). |

`tests/API.Tests/Registration` prueba los endpoints sin base de datos, con un repositorio en memoria; `Infrastructure.Tests` prueba `UserRepository` contra la base de pruebas.

### Inicio de sesión con correo y contraseña

`AuthController` expone `POST /api/v1/auth/login`. El caso de uso es `LoginUser` (`Application/Users`): busca al usuario por correo (sin distinguir mayúsculas), verifica la contraseña, aplica el bloqueo, audita el intento y emite la sesión. `AddInfrastructure(configuration)` registra la verificación del hash, el emisor del token y las opciones; `AddPersistence`, el repositorio.

| Endpoint | Resultado |
|---|---|
| `POST /api/v1/auth/login` con `{ "email", "password" }` | `200` con `{ accessToken, tokenType: "Bearer", expiresAt }`. `401` con el mismo mensaje si el correo no existe, la contraseña es incorrecta, la cuenta está bloqueada temporalmente o el usuario no está `Active`. `400` con el detalle por campo si falta el correo o la contraseña o el correo no tiene formato válido. `429` al superar el límite de intentos por IP. |

- **Mensaje genérico y tiempo constante**: todas las causas de rechazo devuelven `401` con el mismo cuerpo, así que la respuesta no revela si el correo existe, si la cuenta está bloqueada o si falta verificar el correo. `Pbkdf2PasswordHasher.Verify` compara con `CryptographicOperations.FixedTimeEquals` y, si no hay hash (correo inexistente o cuenta sin contraseña) o está malformado, calcula igual un PBKDF2 contra un hash fijo para que el tiempo de respuesta no delate la diferencia.
- **Quién puede entrar**: solo los usuarios `Active` (correo verificado). Un usuario `Pending` o `Blocked` recibe `401` aunque la contraseña sea correcta.
- **Bloqueo temporal**: cada contraseña incorrecta de un usuario existente suma a `Users.FailedLoginAttempts`. Al llegar al máximo, `Users.LockedUntil` queda en el momento actual más la duración del bloqueo y, mientras dure, se rechaza el inicio de sesión aunque la contraseña sea correcta, sin sumar intentos ni extender el bloqueo. Al vencer, el conteo empieza de nuevo. Un inicio de sesión correcto lo reinicia. Un bloqueo es por cuenta y no afecta a otras. La respuesta `429` con el tiempo restante, la espera creciente y el registro de los bloqueos son del límite de intentos fallidos de la funcionalidad siguiente y no están en este cambio.
- **Auditoría**: cada intento, exitoso o fallido, guarda una fila en `LoginAttempts` con el usuario (vacío si el correo no existe), el correo indicado, la IP de origen, el resultado (`LoginOutcome`: `Succeeded`, `InvalidCredentials`, `AccountLocked`, `AccountInactive`) y el momento en UTC. Nunca se guarda la contraseña. Se escribe en la misma transacción que el contador, por lo que el intento fallido no se pierde.
- **Sesión**: un token de acceso JWT firmado con HMAC-SHA256 (`JwtAccessTokenIssuer`) con `sub`, `email`, `jti`, `iss`, `aud`, `iat` y `exp`. El login también devuelve un token de renovación (ver "Gestión de sesiones y tokens"). La revocación manual de sesiones queda fuera.
- **Límite de frecuencia**: política `Login` de ASP.NET Core, ventana fija por IP, que se suma al bloqueo por cuenta. Detrás de un proxy hay que configurar los encabezados reenviados para que la IP sea la del cliente.

| Configuración | Detalle |
|---|---|
| `Authentication:AccessToken:SigningKey` | **Obligatoria**, de al menos 32 bytes. Es un secreto: va en `user-secrets` o en variable de entorno (`Authentication__AccessToken__SigningKey`), nunca en un archivo versionado. Sin ella la API no inicia. En Pruebas hay un valor falso en `appsettings.Testing.json`. |
| `Authentication:AccessToken:LifetimeMinutes` | Vigencia del token de acceso; 15 por defecto. |
| `Authentication:AccessToken:Issuer` / `Audience` | Emisor y audiencia del token; `life-api` y `life-app` por defecto. |
| `Authentication:Login:MaxFailedAttempts` | Contraseñas incorrectas seguidas que bloquean la cuenta; 5 por defecto. |
| `Authentication:Login:LockoutMinutes` | Duración del bloqueo; 15 por defecto. |
| `RateLimiting:Login:PermitLimit` / `WindowSeconds` | Intentos permitidos por IP y duración de la ventana; 5 por 600 segundos por defecto (1000 en Pruebas). |

Para desarrollo local, desde `life-backend`:

```powershell
dotnet user-secrets set "Authentication:AccessToken:SigningKey" "<valor aleatorio de al menos 32 caracteres>" --project API
```

`tests/API.Tests/Login` prueba el endpoint sin base de datos, con un repositorio en memoria; `Infrastructure.Tests` prueba el hash, el emisor del token y `UserRepository`.

### Gestión de sesiones y tokens

Cada sesión es un par: un token de acceso JWT de corta duración y un token de renovación opaco.

- **Validación**: `AddAccessTokenAuthentication` (`API/Extensions`) registra JwtBearer con la misma clave, emisor y audiencia con que se emite, algoritmo HS256 y sin tolerancia de reloj. `UseAuthentication` va antes de `UseAuthorization`. Un token expirado, alterado o ausente en un endpoint protegido devuelve `401`. Proteger los endpoints es de la tarea de autorización.
- **Renovación**: `POST /api/v1/auth/refresh` con `{ "refreshToken" }` devuelve `200` con la misma forma que el login (`accessToken`, `tokenType`, `expiresAt`, `refreshToken`, `refreshTokenExpiresAt`). No exige un token de acceso vigente, así que funciona tras un periodo sin conexión mientras el token de renovación no haya vencido. `401` si es desconocido, vencido, revocado o el usuario no está `Active`; `400` si falta; `429` por el límite de frecuencia de la política `Login`.
- **Rotación**: cada renovación marca el token usado (`UsedAt`) y emite uno nuevo con vigencia completa, de la misma familia (`FamilyId`) y dispositivo (`DeviceId`). El login acepta un `deviceId` opcional (máx. 100 caracteres); si no llega, el servidor genera uno.
- **Reutilización**: presentar un token ya rotado devuelve `401` y revoca (`RevokedAt`) todos los tokens de su familia, de modo que el token nuevo del atacante o del dueño legítimo deja de servir y hay que iniciar sesión de nuevo.
- **Almacenamiento**: la tabla `RefreshTokens` guarda solo el hash SHA-256 (`TokenHash`, único entre activos); el valor en claro solo viaja en la respuesta.

| Configuración | Detalle |
|---|---|
| `Authentication:RefreshToken:LifetimeDays` | Vigencia del token de renovación; 60 por defecto, para cubrir el uso sin conexión. |
| `Authentication:AccessToken:*` | Vigencia (`LifetimeMinutes`), emisor, audiencia y `SigningKey` (secreto, nunca versionado), ya descritos arriba. |

`tests/API.Tests/Login/RefreshTests` y `tests/API.Tests/Security/AccessTokenValidationTests` prueban los endpoints sin base de datos; `Application.Tests/Users/RefreshSessionTests`, la rotación y la reutilización; `Infrastructure.Tests/Persistence/RefreshTokenRepositoryTests`, el repositorio contra la base de pruebas. La migración es `AddRefreshTokens`.

### Restricciones de integridad

La base impide los estados inconsistentes sin depender del código. Toda entidad nueva sigue esta convención:

| Aspecto | Convención |
|---|---|
| Claves foráneas | Una por relación, `IsRequired()` salvo que la relación sea opcional por negocio. Nombre `FK_<Tabla>_<Principal>_<Columna>`, el que genera EF. |
| Borrado | Restringido (`Restrict`) por defecto: la base rechaza borrar un registro con dependientes. `ApplyIntegrityConventions` (en `LifeDbContext`) cambia a `Restrict` toda relación que no declare su comportamiento, en lugar de la cascada que EF asigna a las claves obligatorias. La cascada solo se admite declarándola con `OnDelete(DeleteBehavior.Cascade)` y justificándola en el cambio. |
| Índices | Toda clave foránea tiene un índice sobre sus columnas (EF lo crea, `IX_<Tabla>_<Columna>`), y las columnas de búsqueda frecuente declaran el suyo con `HasIndex`. Una prueba de `API.Tests` falla si una clave foránea queda sin índice. |
| Unicidad | Índice único con `HasFilter("[IsDeleted] = 0")`: la unicidad solo rige entre los registros activos, para que uno eliminado lógicamente no bloquee su reutilización. |
| Nulabilidad | Las columnas son obligatorias salvo que el dato sea opcional por negocio; los textos declaran siempre `HasMaxLength`. |
| Valores enumerados | Una columna `enum` lleva una restricción `CHECK` (`CK_<Tabla>_<Columna>`) con sus valores válidos. |

En `Users`, `CK_Users_Status` limita `Status` a los valores de `UserStatus`; la base rechaza cualquier otro. El borrado restringido protege el borrado físico: la eliminación lógica (`Remove` de EF) no la bloquea la base, y su política se define aparte. Las restricciones de los módulos funcionales se agregan con cada módulo siguiendo esta misma convención.

### Resiliencia ante fallos transitorios

El `DbContext` usa `EnableRetryOnFailure`: ante un error transitorio de conexión reintenta hasta **6 veces**, con una espera máxima de **30 segundos** entre intentos (`DependencyInjection.MaxRetryCount` y `MaxRetryDelay` en `Persistence`). Un error que no es transitorio (credenciales inválidas, base inexistente) falla de inmediato sin reintentar. Para cambiar la política se modifican esas dos constantes.

### Usuario de aplicación de privilegios mínimos

En cada ambiente la API se conecta con un usuario propio, no con `sa` ni con una cuenta administradora. Ese usuario solo lee y escribe datos (`db_datareader` y `db_datawriter`); no es `sysadmin`, `dbcreator` ni `db_owner` y no puede cambiar el esquema. El script `Persistence/Scripts/create-application-user.sql` lo crea. Lo ejecuta una persona con una cuenta administradora, una vez por ambiente y con la base ya creada; la API nunca lo ejecuta. Desde `life-backend`, solo en tu base local:

```powershell
sqlcmd -S localhost,1433 -U sa -C -i Persistence/Scripts/create-application-user.sql -v AppUser=life_app AppPassword="<contraseña>" Database=ModulLifeDev
```

Después se usa ese usuario en la cadena de conexión (`User Id=life_app;Password=<contraseña>`), guardada con `user-secrets`. La contraseña no se escribe en el repositorio. Como las migraciones cambian el esquema, se aplican con otra cuenta con permisos de esquema (en local, la del contenedor).

## Migraciones de la base de datos

Todo cambio de esquema se hace con una migración de EF Core, nunca modificando la base a mano. Las migraciones viven en `Persistence/Migrations` y la migración inicial es `InitialCreate` y solo crea la tabla de historial `__EFMigrationsHistory`. `AddUsersAndExternalAccountsTables` crea las tablas de usuarios y cuentas externas, `AddUserStatusCheckConstraint` agrega la restricción de `Status` de usuarios y `AddEmailVerificationTokens` agrega `EmailVerifiedAt` a `Users` y la tabla `EmailVerificationTokens`, y `AddLoginLockoutAndAttempts` agrega `FailedLoginAttempts` y `LockedUntil` a `Users` y la tabla de auditoría `LoginAttempts`. Los modelos de cada módulo se agregan después con sus propias migraciones. Aplicarlas en producción queda fuera de esta configuración.

### Convención de nombres

El nombre lo define quien genera la migración y describe el cambio en inglés, en PascalCase y empezando por un verbo (`AddTasksTable`, `AddDueDateToTasks`, `RenameListsColumn`). EF antepone la marca de tiempo (`yyyyMMddHHmmss_`), que fija el orden. El nombre no lleva el número de ticket.

### Herramienta

`dotnet-ef` se fija en `.config/dotnet-tools.json`, así que todos usan la misma versión. Una sola vez, desde `life-backend`:

```powershell
dotnet tool restore
```

Los comandos usan `Persistence` como proyecto de migraciones y `API` como proyecto de inicio, y leen la cadena `ConnectionStrings:Default` del ambiente. Como arrancan la API, también necesitan `Authentication:AccessToken:SigningKey` (ver "Inicio de sesión con correo y contraseña"); sin ella fallan con "must be at least 32 bytes long". Generar una migración no se conecta a la base.

### Generar y revisar una migración

```powershell
dotnet ef migrations add <Nombre> --project Persistence --startup-project API --output-dir Migrations
```

Antes de aplicarla se revisa lo que generó: los archivos `<marca>_<Nombre>.cs` y `.Designer.cs` y el `LifeDbContextModelSnapshot.cs`, y el SQL que ejecutará:

```powershell
dotnet ef migrations script --idempotent --project Persistence --startup-project API
```

Una migración ya compartida (subida a una rama) no se edita: el cambio se corrige con una migración nueva. Solo una migración que nadie más tiene se puede deshacer con `dotnet ef migrations remove --project Persistence --startup-project API`. Los archivos de `Persistence/Migrations` los genera la herramienta y no se reformatean a mano (`.editorconfig` los marca como código generado).

### Aplicar migraciones

Solo en tu base local de pruebas, con la cuenta con permisos de esquema (no con el usuario de aplicación):

```powershell
dotnet ef database update --project Persistence --startup-project API
```

Sobre una base vacía o inexistente, aplica todas las migraciones en orden y termina sin errores. Para ver cuáles están aplicadas:

```powershell
dotnet ef migrations list --project Persistence --startup-project API
```

Las pruebas de `MigrationsTests` (en `API.Tests`) comprueban que la migración inicial existe y que el modelo coincide con el snapshot; si falla la segunda, falta generar una migración. Las de `MigrationsTests` en `Infrastructure.Tests` aplican todas las migraciones sobre una base vacía de pruebas.

## Estructura de la API

La estructura común de la API vive en el proyecto `API`:

- `Extensions/ApiServiceCollectionExtensions.cs` registra controladores, versionado, OpenAPI y CORS.
- `Extensions/ApiApplicationBuilderExtensions.cs` define el pipeline de middleware; el orden de las llamadas es el orden de ejecución (Swagger en Desarrollo, HTTPS, CORS, autorización, controladores). Los nuevos middleware se agregan ahí, no en `Program.cs`.
- `Controllers/` contiene los controladores y `Filters/` los filtros globales.

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

### Validación de entradas y contratos (DTOs)

- Los endpoints reciben y devuelven DTOs (por ejemplo `HouseStatusResponse`), nunca entidades de `Domain`. Los DTOs de petición y respuesta viven en `Application`, en la carpeta de su funcionalidad (`Application/<Feature>/`).
- Cada DTO de petición tiene su validador: una clase que hereda de `AbstractValidator<TRequest>` (FluentValidation) en `Application`. `AddApplication` registra automáticamente todos los validadores del ensamblado.
- `API/Filters/ValidationFilter.cs` es un filtro global: valida cada argumento de la acción que tenga validador registrado. Un endpoint nuevo solo necesita su DTO y su validador; el controlador no llama a la validación.
- Si hay errores responde `400` con `application/problem+json` y los campos con sus mensajes:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Quantity": ["'Quantity' must be between 1 and 100. You entered 0."]
  }
}
```

- Un cuerpo JSON con un tipo incorrecto también responde `400`, pero con el mensaje genérico `El valor no es válido.`, para no exponer tipos, rutas ni posiciones internas (`ValidationProblemFactory`).
- No se validan aquí las reglas de negocio de módulos específicos.
- `tests/API.Tests/Validation` prueba el patrón con un controlador y un validador que solo existen en el servidor de pruebas.

### Swagger y `API.http`

En Desarrollo, `http://localhost:5041/swagger` abre Swagger UI y el documento OpenAPI está en `/openapi/v1.json`. `API/API.http` trae peticiones de ejemplo para la ruta base versionada y para comprobar CORS.

## Pruebas unitarias

Las pruebas unitarias viven en `life-backend/tests/`, separadas del código de producción, con un proyecto xUnit por capa probada. No acceden a base de datos ni a red. Las pruebas de `Infrastructure`, `Persistence` y las de integración no van aquí (las de integración de la API están en `tests/API.Tests`, ver más abajo).

| Proyecto (en `tests/`) | Qué prueba | Referencia a |
|---|---|---|
| `Domain.Tests` | Las reglas de `Domain` | `Domain` |
| `Application.Tests` | Los casos de uso y validaciones de `Application`; usa NSubstitute para simular dependencias | `Application` |

Las aserciones son las de xUnit (`Assert`). Cada proyecto de pruebas respeta la regla de dependencias: solo referencia la capa que prueba. Las dependencias y la configuración de pruebas del backend son propias: no se comparten con las del frontend. Las versiones están centralizadas en `Directory.Packages.props`.

Desde `life-backend`:

```powershell
dotnet test tests/Domain.Tests/Domain.Tests.csproj
dotnet test tests/Application.Tests/Application.Tests.csproj
```

### Convención

- Las carpetas y archivos de cada proyecto replican la estructura de la capa que prueban (`Application/Tasks/CreateTask.cs` se prueba en `tests/Application.Tests/Tasks/CreateTaskTests.cs`).
- Una clase por clase probada, con el sufijo `Tests`, en el namespace `<Proyecto>.<Carpeta>` (por ejemplo `Application.Tests.Tasks`).
- Nombre del método: `Metodo_Escenario_ResultadoEsperado` con las tres partes en inglés, por ejemplo `Create_WithEmptyTitle_ThrowsException`.
- Cada caso de uso o regla lleva como mínimo una prueba del caso exitoso, una de cada rechazo y una por cada valor límite.
- Cada prueba tiene tres partes separadas por una línea en blanco: preparación, qué se prueba y la prueba (las aserciones).

```csharp
[Fact]
public void AddApplication_WithValidCollection_ReturnsSameCollection()
{
    var services = new ServiceCollection();

    var result = services.AddApplication();

    Assert.Same(services, result);
}
```

### Entorno de pruebas de integración

Las pruebas de integración y de flujos críticos usan una base SQL Server aislada por proyecto de pruebas, que se crea y se elimina en cada ejecución. El nombre es `ModulLife<Proyecto>Test`: `ModulLifeApiTest` para `API.Tests` y `ModulLifeSupportTest` para `Support.Tests`. Preparar o limpiar una base no afecta a las demás.

Nunca se usa la base de desarrollo ni la de producción: `TestDatabaseOptions` rechaza cualquier nombre de base que no termine en `Test` (incluidos `ModulLifeDev`, `ModulLifeProd` y `master`) y siempre reemplaza la base indicada en la cadena de conexión.

| Proyecto | Qué contiene |
|---|---|
| `Support` | Biblioteca reutilizable desde cualquier proyecto de pruebas: ciclo de vida de la base (`TestDatabase`), fixture de xUnit (`TestDatabaseFixture`), esquema provisional (`Schema.sql`), datos semilla (`SeedData`) y constructores (`TestUserBuilder`, `TestWorkspaceBuilder`) |
| `tests/Support.Tool` | Comando `prepare` / `clean` para preparar o limpiar el entorno a mano |
| `tests/Support.Tests` | Pruebas del propio entorno; las que necesitan la base se omiten si no hay cadena de conexión |
| `tests/API.Tests` | Pruebas de integración de la API |
| `tests/Infrastructure.Tests` | Pruebas de integración de `Persistence` e `Infrastructure` contra SQL Server (ver "Pruebas de integración de Persistence e Infrastructure") |

Los datos semilla y constructores viven en `life-backend/Support` y son solo del backend: el frontend mantiene su propia configuración y dependencias de pruebas y no los comparte. Los usuarios (`dbo.Users`) y espacios de trabajo (`dbo.Workspaces`) de `Support` son tipos provisionales de pruebas. Cuando existan las entidades y las migraciones, `Schema.sql` y los constructores se reemplazan por ellas.

#### Configurar la cadena de conexión

Se apunta al servidor de `docker-compose.yml` (la base de `Database=` se ignora). Desde `life-backend`:

```powershell
dotnet user-secrets set "ConnectionStrings:Test" "Server=localhost,1433;User Id=sa;Password=<contraseña de .env>;TrustServerCertificate=True" --project Support
```

También se acepta la variable de entorno `ConnectionStrings__Test`. La cadena no se versiona. El nombre de la base no se configura: sale del proyecto de pruebas que la usa.

#### Preparar y limpiar con un único comando

```powershell
docker compose up -d
dotnet run --project tests/Support.Tool -- prepare Api
dotnet run --project tests/Support.Tool -- clean Api
```

El último argumento es el proyecto de pruebas (`Api` o `Support`) y define la base: `Api` usa `ModulLifeApiTest`. `prepare` crea la base si no existe, recrea el esquema y carga los datos semilla, así que siempre parte del mismo estado. `clean` elimina esa base completa y no deja datos residuales ni afecta a las bases de otros proyectos.

Datos semilla cargados por `prepare`:

| Cuenta | Correo | Rol en la semilla |
|---|---|---|
| Test owner | `owner@life.test` | Propietario de "Main test workspace" |
| Test member | `member@life.test` | Usuario adicional |

Todavía no hay autenticación, por lo que las cuentas no llevan contraseña; se agrega cuando exista ese modelo.

#### Usar el entorno desde otro proyecto de pruebas

Se referencia `Support` y se usa el fixture, que prepara la base antes de las pruebas de la clase y la elimina al terminar. Cada proyecto de pruebas declara su fixture con el nombre del proyecto, que define su base (`ModulLife<Proyecto>Test`):

```csharp
public sealed class DatabaseFixture : TestDatabaseFixture
{
    public DatabaseFixture() : base("MyProject")
    {
    }
}

public class MyIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _database;

    public MyIntegrationTests(DatabaseFixture database) => _database = database;
}
```

Para datos adicionales se usan los constructores, por ejemplo `new TestUserBuilder().WithEmail("other@life.test").Build()`, y `TestDatabase.InsertAsync` para guardarlos. Las pruebas que dependen de la base se marcan con `[RequiresTestDatabaseFact]` para omitirlas cuando no hay cadena configurada.

#### Pruebas de integración de la API

`tests/API.Tests` levanta la aplicación completa en memoria (`WebApplicationFactory<Program>`, entorno `Testing`) y la prueba por HTTP contra la base `ModulLifeApiTest`. Referencia `API` y `Support`, y se ejecuta por separado de las unitarias:

```powershell
dotnet test tests/API.Tests/API.Tests.csproj
```

- `ApiFactory` es una sola instancia compartida (colección `Api`): prepara la base al empezar y la elimina al terminar, y las clases se ejecutan en serie. Sin `ConnectionStrings:Test` las pruebas con base se omiten; las que no la necesitan (por ejemplo, el 401) corren igual.
- `ApiFactory.CreateApiClient(userId)` devuelve un cliente autenticado con la cabecera `X-Test-User` y el esquema de autenticación de pruebas; sin argumento, el cliente es anónimo.
- Cada prueba crea sus propios datos con `TemporaryWorkspace.CreateAsync(...)` y los elimina al terminar (`await using`), así no depende del orden ni deja residuos.
- Hay un `TestWorkspacesController` que solo existe en el servidor de pruebas: representa el patrón de permiso del propietario y aislamiento por espacio de trabajo sobre las tablas provisionales. Cuando existan la autenticación y los espacios de trabajo reales, se sustituye por sus endpoints y se reutilizan las mismas pruebas.

#### Pruebas de integración de Persistence e Infrastructure

`tests/Infrastructure.Tests` prueba `Persistence` e `Infrastructure` contra SQL Server real, para detectar errores de mapeo, restricciones y esquema que no aparecen con pruebas en memoria. Está separado de las pruebas unitarias, referencia `Infrastructure`, `Persistence` y `Support` (no referencia `API`) y se ejecuta con un solo comando:

```powershell
dotnet test tests/Infrastructure.Tests/Infrastructure.Tests.csproj
```

- Usa la cadena `ConnectionStrings:Test` (ver "Configurar la cadena de conexión"), que no se versiona. Sin ella, las pruebas con base se omiten.
- Cada ejecución parte de una base vacía: `TestDatabase.PrepareEmptyAsync` elimina y crea la base sin el esquema provisional ni datos semilla. La base se elimina al terminar.
- `MigrationsTests` usa su propia base (`ModulLifeInfrastructureMigrationsTest`) y comprueba que todas las migraciones se aplican en orden sobre una base vacía, que no quedan migraciones pendientes ni cambios del modelo sin migrar, y que se crean las tablas `Users` y `ExternalAccounts`.
- El resto de las pruebas comparten una base (`ModulLifeInfrastructureTest`, colección `MigratedDatabase`) con las migraciones ya aplicadas y se ejecutan en serie. Cada prueba crea sus propios datos con correos y cuentas únicos, así no depende del orden.
- `UserPersistenceTests`, `ExternalAccountPersistenceTests` y `UnitOfWorkTests` cubren lectura, escritura, eliminación lógica, versión de fila y las restricciones de integridad: correo único entre usuarios activos sin distinguir mayúsculas, cuenta externa única por proveedor, clave foránea de la cuenta externa, longitud máxima y reversión de la transacción.
- Todavía no hay repositorios: las pruebas usan `LifeDbContext` y `UnitOfWork`. Cuando existan, se agregan en `tests/Infrastructure.Tests/Persistence` con las mismas fixtures.

## Integración continua

El flujo `.github/workflows/backend.yml` tiene dos jobs en GitHub Actions: `Compilar backend` (restaura y compila `life-backend.slnx`) y `Validar código del backend` (ejecuta, en pasos separados, `dotnet format --verify-no-changes` y una compilación con las advertencias como errores, para que los analizadores y las reglas de estilo del `.editorconfig` bloqueen el pull request). Las reglas son las versionadas en el repositorio: `.editorconfig` y `Directory.Build.props` (`EnforceCodeStyleInBuild` y `AnalysisLevel`). Se ejecutan en los pull requests hacia `review` y `main`, y en los push a `review`, `main` y `release/**`, cuando cambia algo en `life-backend`. La versión del SDK sale de `global.json` y los paquetes de NuGet se guardan en caché según `Directory.Packages.props` y los `.csproj`. La compilación no necesita secretos. El CI no ejecuta `dotnet test`: las pruebas se corren en local antes de pedir revisión (ver "Validación final").

Para que un error de compilación o de validación bloquee el pull request, en GitHub (Settings > Branches > regla de protección de `review` y `main`) se activa "Require status checks to pass before merging" y se marcan los checks `Compilar backend` y `Validar código del backend`.

Para reproducir los mismos pasos en local, desde `life-backend`:

```powershell
dotnet restore life-backend.slnx
dotnet build life-backend.slnx --configuration Release
dotnet format life-backend.slnx --verify-no-changes
dotnet build life-backend.slnx --configuration Release -warnaserror
```

## Control de versiones

El `.gitignore` excluye `bin/`, `obj/`, los archivos de entorno, los secretos y los archivos de IDE (`.idea/`, `.vs/`, `.vscode/`). Tras compilar, `git status` no debe mostrar archivos de `bin/` ni `obj/`.

## Reglas y estándares de desarrollo

Referencia para desarrollar, mantener y modificar el backend (.NET 10, C#, SQL Server y Entity Framework Core). Las reglas generales (puntos 1 a 5 y 7 a 9) son las mismas que las del README del frontend; las específicas de cada tecnología están en el punto 6.

### 1. Regla del desarrollador

Esta regla tiene prioridad sobre las demás al implementar o modificar algo: reutilizar antes que crear, extender antes que duplicar y modificar solo lo necesario.

- Antes de implementar, se inspecciona el código para identificar clases, interfaces, servicios, métodos, validadores y patrones reutilizables, y se usan de preferencia. No se crea una implementación nueva si ya existe una que cubra la necesidad; si hay que ampliarla, se modifica o se extiende la existente.
- Se implementa únicamente lo necesario para el objetivo. No se hacen refactorizaciones, mejoras ni cambios de comportamiento, de contratos o de procesos fuera del alcance. Una mejora que lo exceda se indica en el informe final, sin implementarla.
- Se revisan las referencias de lo modificado para detectar efectos secundarios y se comprueba que no cambie nada fuera del alcance.
- Las pruebas son obligatorias: todo cambio de lógica en `Domain` o `Application` incluye su prueba en el proyecto de su capa, con la estructura de tres partes de "Pruebas unitarias", y `dotnet test` debe pasar antes de pedir revisión.
- No se altera la funcionalidad existente por motivos de estilo.
- No se cambian contratos de la API, estructuras de datos ni reglas de negocio si el objetivo no lo requiere.
- No se eliminan pruebas existentes.
- No se modifican las migraciones de EF Core ni otros archivos históricos por motivos de normalización (están marcadas como código generado en `.editorconfig`).
- No se reorganizan la arquitectura ni las carpetas sin necesidad, y no se agregan funcionalidades ajenas al objetivo.
- Si una corrección exige tocar funcionalidad o arquitectura fuera del alcance, se documenta el problema en lugar de cambiarlo.

### 2. Formato y estilo

- El código se formatea con las configuraciones versionadas: `.editorconfig`, `Directory.Build.props` (`EnforceCodeStyleInBuild` y `AnalysisLevel`), los analizadores de Roslyn y `dotnet format`.
- `.editorconfig` define: indentación de 4 espacios en C# y 2 en JSON, XML y YAML, ancho de línea objetivo de 110 caracteres, finales de línea CRLF, UTF-8, `using` fuera del namespace con `System` primero, namespaces con alcance de archivo y llaves siempre. Visual Studio, Rider y VS Code lo leen sin configuración adicional.
- El formato no depende de carpetas de IDE: `.idea/`, `.vs/` y `.vscode/` no se versionan y están en el `.gitignore`.
- El `.gitattributes` de la raíz del repositorio fuerza CRLF en `life-backend` para que `dotnet format` dé el mismo resultado en Windows y en el CI de Linux. `dotnet format` no parte las líneas largas: el límite de 110 se respeta a mano y el IDE lo muestra con la regla `max_line_length`.
- No se introducen configuraciones nuevas ni se modifican las existentes sin una justificación técnica, y ninguna regla nueva contradice las actuales.
- Las mismas convenciones se aplican a todos los archivos equivalentes: una clase, una prueba o un `.csproj` nuevo se formatea igual que los existentes.

Desde `life-backend`:

```powershell
dotnet format life-backend.slnx                        # aplicar el formato
dotnet format life-backend.slnx --verify-no-changes    # comprobar sin modificar archivos
```

### 3. Idioma del código y nomenclatura

- Todo el código se escribe en inglés: nombres de archivos, carpetas, namespaces, clases, interfaces, métodos, propiedades, variables, constantes, rutas, campos de JSON, nombres de las pruebas (`Metodo_Escenario_Resultado` con las tres partes en inglés) y mensajes de excepción. La documentación (README) y los textos que ve el usuario final siguen en español; no se traducen ni el contenido de negocio que debe seguir en español.
- Convenciones oficiales de C#, definidas en `.editorconfig`: `PascalCase` para tipos, interfaces (con prefijo `I`), métodos, propiedades y constantes; `camelCase` para variables locales y parámetros; `_camelCase` para campos privados.
- Un identificador existente en español se corrige solo cuando el cambio lo toca o lo requiere, y en el mismo cambio se actualizan todas sus referencias.
- Los términos son los mismos que en el frontend (por ejemplo `Workspace`, `Task`), para que un concepto tenga un único nombre en todo el proyecto.
- El esquema provisional de `Support` (`dbo.Users`, `dbo.Workspaces`) también está en inglés; cuando existan las entidades y migraciones reales, las reemplazan.

### 4. Comentarios y limpieza

- No se agregan comentarios salvo que sean necesarios: solo cuando explican un porqué que el código no deja claro por sí mismo. No se comenta lo que el código ya expresa y no se deja código comentado.
- En los archivos que se crean o modifican se eliminan los comentarios innecesarios, redundantes u obsoletos. Se conservan las directivas para el compilador y los analizadores (`#pragma`, `[SuppressMessage]` justificado, `#nullable`), la documentación XML (`///`) de los contratos y de la API pública o la que genera documentación, y la documentación externa y técnica existente.
- Se eliminan los `using`, variables, métodos y código muerto sin uso (el IDE los marca con IDE0005 y similares), y las duplicaciones innecesarias. No se agregan abstracciones ni patrones que no aporten valor.
- Los archivos y carpetas se organizan por funcionalidad dentro de cada capa, sin dependencias circulares.

### 5. Tipado y seguridad de tipos

- `Nullable` habilitado en todos los proyectos: los parámetros, propiedades y retornos reflejan si admiten `null`.
- No se usa `dynamic` ni `object` si existe el tipo concreto o la interfaz; se evitan las conversiones inseguras (`(T)x`) a favor de `is`, `as` con comprobación o genéricos.
- Los datos se representan con tipos concretos (DTOs, `record`, entidades); `var` solo cuando el tipo es evidente en la misma línea.
- Los servicios, dependencias, propiedades, parámetros y respuestas están correctamente tipados.

### 6. Reglas específicas del backend

- Se respeta Clean Architecture y la "Regla de dependencias": `API`, `Application`, `Domain`, `Infrastructure` y `Persistence` mantienen sus responsabilidades, cada responsabilidad vive en una sola capa y no se agregan referencias distintas a las de la tabla de capas.
- Los controladores no llevan lógica de negocio: reciben el DTO, invocan el caso de uso de `Application` y devuelven el resultado. La entrada y la salida de la API usan DTOs (ver "Validación de entradas y contratos"); las entidades de `Domain` no se exponen.
- Todo se resuelve con inyección de dependencias, con contratos definidos en `Application` e implementados en `Infrastructure` y `Persistence`. Los servicios se piden por constructor con su interfaz o tipo concreto. Si hay que resolver uno manualmente, se prefiere `GetRequiredService<T>()` a `GetService(Type)`, y `object` o `dynamic` no se usan para guardarlo. La resolución por `Type` en tiempo de ejecución solo se acepta cuando el tipo no se conoce en compilación, como en el `ValidationFilter`, que busca el validador de cada argumento.
- No se crean mecanismos alternativos de resolución cuando ya existe uno (los métodos `AddApplication`, `AddInfrastructure` y `AddPersistence`). Los contratos se respetan tal como los define la arquitectura, y se aprovechan el autocompletado, la navegación y el análisis estático del IDE, que dependen de los tipos explícitos.
- Las operaciones de E/S son asíncronas (`async`/`await`, métodos `Async`) y propagan el `CancellationToken` a las operaciones que lo aceptan (EF Core, `HttpClient`, etc.); no se bloquea con `.Result` ni `.Wait()`.
- En EF Core se evitan las consultas duplicadas y los problemas N+1: se cargan los datos relacionados con `Include` o proyecciones (`Select`) y no se consulta dentro de un bucle; las consultas de solo lectura usan `AsNoTracking()`.
- Las excepciones y los errores de entrada se gestionan con los mecanismos centralizados (hoy el `ValidationFilter` y `ValidationProblemFactory`); no se atrapan excepciones en cada controlador para devolver respuestas ad hoc, y no se exponen detalles internos al cliente.
- No se duplican servicios, utilidades, validaciones, consultas ni lógica de negocio, ni se crean sistemas independientes para lo que ya está centralizado. No se introducen patrones innecesarios (repositorios genéricos, mediadores, etc.) si el proyecto no los usa.

### 7. Seguridad y dependencias

- No se versionan credenciales, tokens, secretos ni cadenas de conexión; se usan `user-secrets`, `appsettings.Development.local.json` o variables de entorno (ver "Variables de entorno y secretos").
- No se registra información sensible en los logs ni en las respuestas de error.
- Se validan los datos de entrada con FluentValidation y se respetan los mecanismos de autenticación, autorización y protección existentes (usuario de aplicación de privilegios mínimos, CORS, `ValidationFilter`).
- No se agregan paquetes innecesarios: se reutilizan los de `Directory.Packages.props`, y las versiones se actualizan según la "Política de actualización", nunca por motivos estéticos.
- La configuración de entorno solo cambia con una justificación técnica.

### 8. Validación final

Cuando se modifica código, antes de pedir revisión se ejecuta, desde `life-backend`:

```powershell
dotnet format life-backend.slnx --verify-no-changes
dotnet build life-backend.slnx --configuration Release -warnaserror
dotnet test tests/Domain.Tests/Domain.Tests.csproj
dotnet test tests/Application.Tests/Application.Tests.csproj
```

- Las pruebas de integración (`tests/API.Tests` y `tests/Infrastructure.Tests`) usan la base de pruebas y se ejecutan solo en el entorno local (ver "Entorno de pruebas de integración").
- El formato, la compilación y los analizadores no reportan errores, ni `using` o referencias sin uso.
- Las pruebas existentes y las de lo modificado pasan.
- No cambiaron contratos ni comportamientos funcionales, y se revisaron los efectos secundarios en las funcionalidades relacionadas.
- Los errores que introdujo el cambio se corrigen; los problemas que ya existían y no se pueden corregir sin ampliar el alcance se identifican y se reportan.
- No se afirma que una validación fue exitosa si no se ejecutó.

### 9. Informe de resultados

Al terminar un cambio de código, el resumen incluye: (1) archivos creados, modificados y eliminados; (2) reglas de formato y estilo aplicadas; (3) identificadores corregidos al inglés; (4) comentarios eliminados y anotaciones técnicas conservadas; (5) limpieza realizada; (6) problemas de tipado detectados y corregidos; (7) resultado de la compilación; (8) resultado de las pruebas; (9) problemas preexistentes o pendientes; (10) posibles mejoras fuera del alcance.
