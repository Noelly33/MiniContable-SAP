using MiniContable.Core.Comun;

namespace MiniContable.Core.Tests;

public class ImporteTests
{
    [Fact]
    public void Un_importe_negativo_no_es_valido()
    {
        Assert.Throws<DominioException>(() => new Importe(-1m));
    }

    [Fact]
    public void Un_importe_con_mas_de_2_decimales_no_es_valido()
    {
        Assert.Throws<DominioException>(() => new Importe(10.555m));
    }

    [Fact]
    public void Dos_importes_se_suman_sin_perder_precision()
    {
        var total = new Importe(0.10m) + new Importe(0.20m);

        Assert.Equal(new Importe(0.30m), total);
    }
}
