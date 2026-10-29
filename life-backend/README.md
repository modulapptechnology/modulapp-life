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

## Compilar y ejecutar

Desde `life-backend`:

```powershell
dotnet restore
dotnet build life-backend.slnx
dotnet run --project API
```

La API escucha en `http://localhost:5041` y `https://localhost:7183`.

## Control de versiones

El `.gitignore` excluye `bin/`, `obj/`, los archivos de entorno, los secretos y los archivos de IDE (`.idea/`, `.vs/`, `.vscode/`). Tras compilar, `git status` no debe mostrar archivos de `bin/` ni `obj/`.
