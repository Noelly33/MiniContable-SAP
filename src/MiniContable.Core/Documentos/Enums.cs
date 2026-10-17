namespace MiniContable.Core.Documentos;

/// <summary>Clases de documento de SAP FI.</summary>
public enum ClaseDocumento
{
    SA, // Asiento manual
    KR, // Factura de proveedor
    DR, // Factura de cliente
    KZ, // Pago a proveedor
    DZ  // Cobro a cliente
}

public enum DebeHaber
{
    Debe,
    Haber
}

public enum EstadoDocumento
{
    Borrador,
    Contabilizado,
    Anulado
}

public static class DebeHaberExtensions
{
    public static DebeHaber Contrario(this DebeHaber valor) =>
        valor == DebeHaber.Debe ? DebeHaber.Haber : DebeHaber.Debe;
}
