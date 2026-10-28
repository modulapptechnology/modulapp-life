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

## Compilar y ejecutar

Desde `life-backend`:

```powershell
dotnet restore
dotnet build life-backend.slnx
dotnet run --project API
```

La API escucha en `http://localhost:5041` y `https://localhost:7183`.

## Control de versiones

El `.gitignore` excluye `bin/`, `obj/`, los archivos de entorno, los secretos y los archivos de IDE (`.idea/`, `.vs/`). Tras compilar, `git status` no debe mostrar archivos de `bin/` ni `obj/`.
