using MiniContable.Core.Comun;
using MiniContable.Core.Documentos;
using MiniContable.Core.Periodos;
using MiniContable.Core.PlanDeCuentas;

namespace MiniContable.Core.Tests;

public class DocumentoContableTests
{
    // Plan de cuentas mínimo para las pruebas
    private static readonly Cuenta Activos = new("1", "ACTIVOS", TipoCuenta.Activo, esDeMovimiento: false);
    private static readonly Cuenta Caja = new("1.1.01", "Caja", TipoCuenta.Activo, esDeMovimiento: true);
    private static readonly Cuenta Clientes = new("1.1.02", "Cuentas por cobrar clientes", TipoCuenta.Activo,
        esDeMovimiento: true, requiereTercero: true);
    private static readonly Cuenta Ventas = new("4.1.01", "Ventas", TipoCuenta.Ingreso, esDeMovimiento: true);
    private static readonly Cuenta IvaPorPagar = new("2.1.01", "IVA por pagar", TipoCuenta.Pasivo, esDeMovimiento: true);

    private static readonly DateOnly Fecha = new(2026, 10, 6);

    private static DocumentoContable FacturaDeVenta()
    {
        var doc = new DocumentoContable(ClaseDocumento.DR, Fecha, Fecha, "Factura 001-001-000000123");
        doc.AgregarPosicion(Clientes, DebeHaber.Debe, new Importe(115m), tercero: "Cliente ABC");
        doc.AgregarPosicion(Ventas, DebeHaber.Haber, new Importe(100m));
        doc.AgregarPosicion(IvaPorPagar, DebeHaber.Haber, new Importe(15m));
        return doc;
    }

    [Fact]
    public void Un_documento_cuadrado_se_contabiliza_con_su_numero()
    {
        var doc = FacturaDeVenta();

        doc.Contabilizar(new Periodo(2026, 10), numero: 1);

        Assert.Equal(EstadoDocumento.Contabilizado, doc.Estado);
        Assert.Equal(1, doc.Numero);
        Assert.Equal(115m, doc.TotalDebe);
        Assert.Equal(115m, doc.TotalHaber);
    }

    [Fact]
    public void Un_documento_descuadrado_no_se_contabiliza()
    {
        var doc = new DocumentoContable(ClaseDocumento.SA, Fecha, Fecha, "Asiento descuadrado");
        doc.AgregarPosicion(Caja, DebeHaber.Debe, new Importe(100m));
        doc.AgregarPosicion(Ventas, DebeHaber.Haber, new Importe(90m));

        var error = Assert.Throws<DominioException>(() => doc.Contabilizar(new Periodo(2026, 10), 1));

        Assert.Contains("no cuadra", error.Message);
        Assert.Equal(EstadoDocumento.Borrador, doc.Estado);
    }

    [Fact]
    public void Un_documento_con_una_sola_posicion_no_se_contabiliza()
    {
        var doc = new DocumentoContable(ClaseDocumento.SA, Fecha, Fecha, "Asiento incompleto");
        doc.AgregarPosicion(Caja, DebeHaber.Debe, new Importe(100m));

        Assert.Throws<DominioException>(() => doc.Contabilizar(new Periodo(2026, 10), 1));
    }

    [Fact]
    public void Una_cuenta_de_grupo_no_acepta_movimientos()
    {
        var doc = new DocumentoContable(ClaseDocumento.SA, Fecha, Fecha, "Asiento a cuenta de grupo");

        Assert.Throws<DominioException>(() => doc.AgregarPosicion(Activos, DebeHaber.Debe, new Importe(100m)));
    }

    [Fact]
    public void Una_cuenta_que_exige_tercero_no_acepta_posiciones_sin_tercero()
    {
        var doc = new DocumentoContable(ClaseDocumento.DR, Fecha, Fecha, "Factura sin cliente");

        Assert.Throws<DominioException>(() => doc.AgregarPosicion(Clientes, DebeHaber.Debe, new Importe(115m)));
    }

    [Fact]
    public void No_se_contabiliza_en_un_periodo_cerrado()
    {
        var periodo = new Periodo(2026, 10);
        periodo.Cerrar();

        Assert.Throws<DominioException>(() => FacturaDeVenta().Contabilizar(periodo, 1));
    }

    [Fact]
    public void No_se_contabiliza_si_la_fecha_no_pertenece_al_periodo()
    {
        Assert.Throws<DominioException>(() => FacturaDeVenta().Contabilizar(new Periodo(2026, 9), 1));
    }

    [Fact]
    public void Un_documento_contabilizado_no_se_puede_modificar()
    {
        var doc = FacturaDeVenta();
        doc.Contabilizar(new Periodo(2026, 10), 1);

        Assert.Throws<DominioException>(() => doc.AgregarPosicion(Caja, DebeHaber.Debe, new Importe(1m)));
    }

    [Fact]
    public void Anular_genera_el_documento_inverso_y_marca_el_original()
    {
        var periodo = new Periodo(2026, 10);
        var original = FacturaDeVenta();
        original.Contabilizar(periodo, 1);

        var inverso = original.Anular(Fecha, periodo, numeroAnulacion: 2, motivo: "Error en el cliente");

        Assert.Equal(EstadoDocumento.Anulado, original.Estado);
        Assert.Same(inverso, original.AnuladoPor);
        Assert.Same(original, inverso.AnulaA);
        Assert.Equal(EstadoDocumento.Contabilizado, inverso.Estado);

        // Cada posición del inverso tiene la misma cuenta e importe, pero el lado contrario
        Assert.All(original.Posiciones.Zip(inverso.Posiciones), par =>
        {
            Assert.Equal(par.First.Cuenta, par.Second.Cuenta);
            Assert.Equal(par.First.Importe, par.Second.Importe);
            Assert.Equal(par.First.DebeHaber.Contrario(), par.Second.DebeHaber);
        });
    }
}
