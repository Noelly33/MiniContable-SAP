using MiniContable.Core.Comun;

namespace MiniContable.Core.PlanDeCuentas;

/// <summary>
/// Cuenta de mayor del plan de cuentas (equivale a FS00 en SAP).
/// Solo las cuentas de movimiento aceptan asientos; las de grupo solo agrupan.
/// </summary>
public class Cuenta
{
    public string Codigo { get; }
    public string Nombre { get; private set; }
    public TipoCuenta Tipo { get; }
    public bool EsDeMovimiento { get; }
    public bool RequiereTercero { get; }
    public bool RequiereCentroCosto { get; }

    public Cuenta(
        string codigo,
        string nombre,
        TipoCuenta tipo,
        bool esDeMovimiento,
        bool requiereTercero = false,
        bool requiereCentroCosto = false)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DominioException("La cuenta necesita un código.");
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DominioException("La cuenta necesita un nombre.");
        if (!esDeMovimiento && (requiereTercero || requiereCentroCosto))
            throw new DominioException("Una cuenta de grupo no puede exigir tercero ni centro de costo.");

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
        Tipo = tipo;
        EsDeMovimiento = esDeMovimiento;
        RequiereTercero = requiereTercero;
        RequiereCentroCosto = requiereCentroCosto;
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DominioException("La cuenta necesita un nombre.");
        Nombre = nombre.Trim();
    }

    public override string ToString() => $"{Codigo} {Nombre}";
}
