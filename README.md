# MiniContable

Sistema contable de partida doble que modela la contabilidad con los conceptos de **SAP FI** y se integra con **ERPNext**.

> Proyecto educativo de portafolio. No reemplaza un sistema contable oficial. Los datos son de demostración.

## Conceptos de SAP FI que replica

| SAP | MiniContable |
|---|---|
| Plan de cuentas (FS00) | `Cuenta`: tipo, cuentas de grupo y de movimiento |
| Periodos (OB52) | `Periodo`: no se contabiliza en un periodo cerrado |
| Clase de documento | `ClaseDocumento`: SA, KR, DR, KZ, DZ |
| Documento contable (FB50) | `DocumentoContable`: siempre cuadrado, con numeración |
| Anulación (FB08) | `DocumentoContable.Anular`: documento inverso, nunca se borra |

## Reglas contables

1. Todo documento tiene al menos 2 posiciones y el debe es igual al haber.
2. Solo se registra en cuentas de movimiento.
3. La fecha contable cae en un periodo abierto.
4. Un documento contabilizado no se modifica ni se borra: se anula con un documento inverso.
5. Los importes son `decimal` con 2 decimales, nunca `double`.

Cada regla tiene su prueba en `tests/MiniContable.Core.Tests`.

## Stack

.NET 10 · Blazor · EF Core + PostgreSQL · ERPNext (API REST) · xUnit · Testcontainers · WireMock.Net · Docker Compose · GitHub Actions

## Estructura

```
src/
  MiniContable.Core/            dominio: reglas contables, casos de uso, interfaces
  MiniContable.Infrastructure/  EF Core + PostgreSQL, numeración
  MiniContable.Integrations/    conectores ERPNext y SAP
  MiniContable.Web/             Blazor
tests/
  MiniContable.Core.Tests/
  MiniContable.Infrastructure.Tests/
  MiniContable.Integrations.Tests/
```

## Cómo correrlo

```powershell
dotnet build
dotnet test
dotnet run --project src/MiniContable.Web
```

## Hoja de ruta

- [x] Dominio: cuentas, periodos, documentos, contabilizar y anular
- [ ] Persistencia con EF Core + PostgreSQL y numeración sin huecos
- [ ] Pantallas: plan de cuentas, registro de asientos, libro diario y mayor
- [ ] Balance de comprobación y estados financieros
- [ ] Terceros, partidas abiertas y compensación
- [ ] Integración con ERPNext
- [ ] Conector SAP (APIs de prueba)
