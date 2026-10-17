using MiniContable.Core.Comun;

namespace MiniContable.Core.Periodos;

public enum EstadoPeriodo
{
    Abierto,
    Cerrado
}

/// <summary>
/// Mes contable de un ejercicio (equivale a OB52 en SAP).
/// En un periodo cerrado no se puede contabilizar.
/// </summary>
public class Periodo
{
    public int Ejercicio { get; }
    public int Mes { get; }
    public EstadoPeriodo Estado { get; private set; } = EstadoPeriodo.Abierto;

    public Periodo(int ejercicio, int mes)
    {
        if (ejercicio < 2000 || ejercicio > 2100)
            throw new DominioException($"Ejercicio fuera de rango: {ejercicio}.");
        if (mes < 1 || mes > 12)
            throw new DominioException($"Mes inválido: {mes}.");

        Ejercicio = ejercicio;
        Mes = mes;
    }

    public bool Contiene(DateOnly fecha) => fecha.Year == Ejercicio && fecha.Month == Mes;

    public void Cerrar() => Estado = EstadoPeriodo.Cerrado;

    public void Reabrir() => Estado = EstadoPeriodo.Abierto;

    public override string ToString() => $"{Mes:00}/{Ejercicio}";
}
