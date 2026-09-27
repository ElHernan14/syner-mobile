using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Syner.Api.Domain.Entities;

namespace Syner.Api.Services.Lotes;

public sealed class LotePdfService
{
    private readonly LoteQrService _lote_qr_service;

    public LotePdfService(
        LoteQrService lote_qr_service)
    {
        _lote_qr_service = lote_qr_service;
    }

    public byte[] Generar(Lote lote)
    {
        var qr = _lote_qr_service.Generar(lote.Id);
        var porcentaje_progreso =
            lote.CantidadCupos > 0
                ? (decimal)lote.CuposOcupados /
                  lote.CantidadCupos *
                  100
                : 0;

        var documento = Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(40);

                pagina.DefaultTextStyle(
                    TextStyle.Default.FontSize(11)
                );

                pagina.Header()
                    .Column(columna =>
                    {
                        columna.Item()
                            .Text("SYNER")
                            .FontSize(28)
                            .Bold();

                        columna.Item()
                            .Text("Ficha del lote")
                            .FontSize(14);
                    });

                pagina.Content()
                    .PaddingVertical(25)
                    .Column(columna =>
                    {
                        columna.Spacing(12);

                        columna.Item()
                            .Text(lote.Nombre)
                            .FontSize(22)
                            .Bold();

                        columna.Item()
                            .Text(lote.Descripcion)
                            .FontSize(11);

                        columna.Item()
                            .LineHorizontal(1);

                        columna.Item()
                            .Text("Información del lote")
                            .FontSize(15)
                            .Bold();

                        columna.Item()
                            .Table(tabla =>
                            {
                                tabla.ColumnsDefinition(
                                    columnas =>
                                    {
                                        columnas.RelativeColumn();
                                        columnas.RelativeColumn();
                                    }
                                );

                                Fila(tabla, "Categoría", lote.Categoria);
                                Fila(
                                    tabla,
                                    "Estado",
                                    lote.Estado
                                );
                                Fila(
                                    tabla,
                                    "Precio de mercado",
                                    lote.PrecioMercado
                                        .ToString("C2")
                                );
                                Fila(
                                    tabla,
                                    "Precio por cupo",
                                    lote.PrecioCupo
                                        .ToString("C2")
                                );
                                Fila(
                                    tabla,
                                    "Ahorro",
                                    $"{lote.PorcentajeAhorro:0.##}%"
                                );
                                Fila(
                                    tabla,
                                    "Cupos",
                                    $"{lote.CuposOcupados} / {lote.CantidadCupos}"
                                );
                                Fila(
                                    tabla,
                                    "Progreso",
                                    $"{porcentaje_progreso:0.##}%"
                                );
                                Fila(
                                    tabla,
                                    "Fecha de inicio",
                                    lote.FechaInicio
                                        .ToString("dd/MM/yyyy")
                                );
                                Fila(
                                    tabla,
                                    "Fecha de fin",
                                    lote.FechaFin
                                        .ToString("dd/MM/yyyy")
                                );
                            });

                        columna.Item()
                            .PaddingTop(10)
                            .Text("Proveedor")
                            .FontSize(15)
                            .Bold();

                        columna.Item()
                            .Text(
                                lote.Proveedor?.Nombre
                                ?? "Sin proveedor"
                            );

                        if (!string.IsNullOrWhiteSpace(
                                lote.Proveedor?.Descripcion))
                        {
                            columna.Item()
                                .Text(
                                    lote.Proveedor.Descripcion
                                );
                        }

                        columna.Item()
                            .PaddingTop(15)
                            .Text(
                                "Documento generado automáticamente por SYNER."
                            )
                            .FontSize(9);

                        columna.Item()
                            .PaddingTop(20)
                            .AlignCenter()
                            .Text("Código QR del lote")
                            .FontSize(15)
                            .Bold();

                        columna.Item()
                            .PaddingTop(8)
                            .AlignCenter()
                            .Width(140)
                            .Image(qr);

                        columna.Item()
                            .PaddingTop(8)
                            .AlignCenter()
                            .Text(
                                _lote_qr_service.ObtenerContenido(lote.Id)
                            )
                            .FontSize(8);
                    });

                pagina.Footer()
                    .AlignCenter()
                    .Text(texto =>
                    {
                        texto.Span("SYNER · Página ");
                        texto.CurrentPageNumber();
                    });
            });
        });

        return documento.GeneratePdf();
    }

    private static void Fila(
        TableDescriptor tabla,
        string etiqueta,
        string valor)
    {
        tabla.Cell()
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .Text(etiqueta)
            .SemiBold();

        tabla.Cell()
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .Text(valor);
    }
}