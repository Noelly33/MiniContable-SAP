using System.Globalization;

namespace MiniContable.Core.Comun;

public readonly record struct Importe
{
    public decimal Valor { get; }

    public Importe(decimal valor)
    {
        if (valor < 0)
            throw new DominioException("El importe no puede ser negativo.");
        if (decimal.Round(valor, 2) != valor)
            throw new DominioException("El importe admite como máximo 2 decimales.");

        Valor = valor;
    }

    public static Importe Cero => new(0m);

    public static Importe operator +(Importe a, Importe b) => new(a.Valor + b.Valor);

    public override string ToString() => Valor.ToString("0.00", CultureInfo.InvariantCulture);
}
