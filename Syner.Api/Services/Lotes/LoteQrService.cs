using QRCoder;

namespace Syner.Api.Services.Lotes;

public sealed class LoteQrService
{
    private const string UrlBase =
        "https://syner.app/lotes";

    public byte[] Generar(long lote_id)
    {
        var contenido = $"{UrlBase}/{lote_id}";

        using var generador = new QRCodeGenerator();

        using var datos_qr =
            generador.CreateQrCode(
                contenido,
                QRCodeGenerator.ECCLevel.Q
            );

        var qr = new PngByteQRCode(datos_qr);

        return qr.GetGraphic(10);
    }

    public string ObtenerContenido(long lote_id)
    {
        return $"{UrlBase}/{lote_id}";
    }
}