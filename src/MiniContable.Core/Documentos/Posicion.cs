using MiniContable.Core.Comun;
using MiniContable.Core.PlanDeCuentas;

namespace MiniContable.Core.Documentos;

/// <summary>
/// Línea de un documento contable. Solo se crea desde DocumentoContable.AgregarPosicion.
/// </summary>
public class Posicion
{
    public int Linea { get; }
    public Cuenta Cuenta { get; }
    public DebeHaber DebeHaber { get; }
    public Importe Importe { get; }
    public string? Tercero { get; }
    public string? CentroCosto { get; }
    public string? Texto { get; }

    internal Posicion(
        int linea,
        Cuenta cuenta,
        DebeHaber debeHaber,
        Importe importe,
        string? tercero,
        string? centroCosto,
        string? texto)
    {
        if (!cuenta.EsDeMovimiento)
            throw new DominioException($"La cuenta {cuenta.Codigo} es de grupo: no acepta movimientos.");
        if (importe.Valor == 0)
            throw new DominioException("El importe de una posición debe ser mayor que cero.");
        if (cuenta.RequiereTercero && string.IsNullOrWhiteSpace(tercero))
            throw new DominioException($"La cuenta {cuenta.Codigo} exige un tercero.");
        if (cuenta.RequiereCentroCosto && string.IsNullOrWhiteSpace(centroCosto))
            throw new DominioException($"La cuenta {cuenta.Codigo} exige un centro de costo.");

        Linea = linea;
        Cuenta = cuenta;
        DebeHaber = debeHaber;
        Importe = importe;
        Tercero = tercero;
        CentroCosto = centroCosto;
        Texto = texto;
    }
}
