using MiniContable.Core.Comun;
using MiniContable.Core.PlanDeCuentas;
using MiniContable.Core.Periodos;

namespace MiniContable.Core.Documentos;


public class DocumentoContable
{
    private readonly List<Posicion> _posiciones = new();

    public ClaseDocumento Clase { get; }
    public DateOnly FechaDocumento { get; }
    public DateOnly FechaContable { get; }
    public string Texto { get; }
    public EstadoDocumento Estado { get; private set; } = EstadoDocumento.Borrador;
    public long? Numero { get; private set; }

    /// <summary>Si este documento es una anulación, el documento que anula.</summary>
    public DocumentoContable? AnulaA { get; private set; }

    /// <summary>Si este documento fue anulado, el documento inverso que lo anuló.</summary>
    public DocumentoContable? AnuladoPor { get; private set; }

    public IReadOnlyList<Posicion> Posiciones => _posiciones;

    public decimal TotalDebe => Total(DebeHaber.Debe);
    public decimal TotalHaber => Total(DebeHaber.Haber);

    public DocumentoContable(ClaseDocumento clase, DateOnly fechaDocumento, DateOnly fechaContable, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new DominioException("El documento necesita un texto descriptivo.");

        Clase = clase;
        FechaDocumento = fechaDocumento;
        FechaContable = fechaContable;
        Texto = texto.Trim();
    }

    public void AgregarPosicion(
        Cuenta cuenta,
        DebeHaber debeHaber,
        Importe importe,
        string? tercero = null,
        string? centroCosto = null,
        string? texto = null)
    {
        AsegurarBorrador();
        _posiciones.Add(new Posicion(_posiciones.Count + 1, cuenta, debeHaber, importe, tercero, centroCosto, texto));
    }

    /// <summary>
    /// Valida las reglas contables y deja el documento contabilizado con su número.
    /// El número lo entrega el numerador (Infrastructure) dentro de la misma transacción.
    /// </summary>
    public void Contabilizar(Periodo periodo, long numero)
    {
        AsegurarBorrador();

        if (_posiciones.Count < 2)
            throw new DominioException("Un documento necesita al menos 2 posiciones.");
        if (TotalDebe != TotalHaber)
            throw new DominioException($"El documento no cuadra: debe {TotalDebe:0.00} y haber {TotalHaber:0.00}.");
        if (!periodo.Contiene(FechaContable))
            throw new DominioException($"La fecha contable {FechaContable:dd/MM/yyyy} no pertenece al periodo {periodo}.");
        if (periodo.Estado != EstadoPeriodo.Abierto)
            throw new DominioException($"El periodo {periodo} está cerrado.");
        if (numero <= 0)
            throw new DominioException("El número de documento debe ser mayor que cero.");

        Numero = numero;
        Estado = EstadoDocumento.Contabilizado;
    }

    /// <summary>
    /// Anula este documento generando y contabilizando su documento inverso (equivale a FB08 en SAP).
    /// Nada se borra: el original queda Anulado y ambos quedan enlazados.
    /// </summary>
    public DocumentoContable Anular(DateOnly fechaContable, Periodo periodo, long numeroAnulacion, string motivo)
    {
        if (Estado != EstadoDocumento.Contabilizado)
            throw new DominioException("Solo se puede anular un documento contabilizado.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DominioException("La anulación necesita un motivo.");

        var inverso = new DocumentoContable(Clase, fechaContable, fechaContable,
            $"Anulación de {Clase} {Numero}: {motivo.Trim()}");

        foreach (var p in _posiciones)
            inverso.AgregarPosicion(p.Cuenta, p.DebeHaber.Contrario(), p.Importe, p.Tercero, p.CentroCosto, p.Texto);

        inverso.Contabilizar(periodo, numeroAnulacion);
        inverso.AnulaA = this;

        AnuladoPor = inverso;
        Estado = EstadoDocumento.Anulado;

        return inverso;
    }

    private decimal Total(DebeHaber lado) =>
        _posiciones.Where(p => p.DebeHaber == lado).Sum(p => p.Importe.Valor);

    private void AsegurarBorrador()
    {
        if (Estado != EstadoDocumento.Borrador)
            throw new DominioException("Un documento contabilizado o anulado no se puede modificar.");
    }
}
