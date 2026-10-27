## Ticket

LIF-<número>

## Descripción

<!-- Qué se cambió y por qué. -->

## Tipo de cambio

- [ ] Backend (`life-backend`)
- [ ] Frontend (`life-frontend`)
- [ ] Documentación / configuración

## Rama de origen y destino

- `dev-backend` o `dev-frontend` → `review` → `main`. No se hace push directo a `main` ni a `review`.

## Checklist

- [ ] El PR corresponde a un solo ticket.
- [ ] Los mensajes de commit siguen el patrón `TICKET : título : qué se hizo`.
- [ ] Backend: `dotnet build life-backend.slnx` compila sin errores.
- [ ] Frontend: `npx tsc --noEmit` y `npx expo lint` terminan sin errores.
- [ ] No se versionan cadenas de conexión, claves ni secretos.
- [ ] El PR tiene al menos una revisión aprobada antes de integrarse.

## Cómo verificar

<!-- Pasos para probar el cambio. -->
