using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Models;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SAGM.Helpers
{
    public class ReportHelper : IReportHelper
    {
        private readonly IWebHostEnvironment _host;
        private readonly SAGMContext _context;
        private readonly UserHelper _userHelper;

        public ReportHelper(IWebHostEnvironment host, SAGMContext context )
        {
            _host = host;
            _context = context;

        }

        NumberFormatInfo nfi = new CultureInfo("es-MX", false).NumberFormat;
      
        
        public async Task<byte[]> GenerateQuoteReportPDFAsync(int QuoteId)
        {
            nfi.CurrencyDecimalDigits = 2;
            //nfi.CurrencyDecimalSeparator = ",";
            nfi.NumberDecimalDigits = 2;


            Quote quote =await  _context.Quotes
                .Include(q => q.QuoteStatus)
                .Include(q => q.Currency)
                .Include(q => q.Customer)
                .Include(q => q.QuoteDetails).ThenInclude(qd => qd.Unit)
                .Include(q => q.QuoteDetails).ThenInclude(qd => qd.Material)
                .FirstOrDefaultAsync(q => q.QuoteId == QuoteId);



            Contact finalUser = await _context.Contacts.FindAsync(quote.FinalUserId);
            Contact buyer = await _context.Contacts.FindAsync(quote.BuyerContactId);

            QuestPDF.Settings.License = LicenseType.Community;
            var Report = QuestPDF.Fluent.Document.Create(document => {

                document.Page(page =>
                {
                    string path = Path.Combine(_host.WebRootPath, "Images\\");
                    var rutaimagen = $"{path}{"simaq_header.png"}";
                    byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);

                    decimal subtotal = 0;
                    decimal total = 0;
                    decimal iva = quote.Tax / 100; 
                    decimal ivatotal = 0;

              

                    page.Size(PageSizes.Letter);
                    page.Margin(5, QuestPDF.Infrastructure.Unit.Millimetre);

                    page.Header().ShowOnce().Column(col => {
                        col.Item().Row(row => row.RelativeItem().Height(68).Image(imageData));
                        col.Item().Background("FFBB11").Row(row => row.RelativeItem().Height(20).AlignRight().PaddingRight(5).Text(quote.QuoteName).FontColor("#626567").Bold());
                        col.Item().Border(1).BorderColor(Colors.BlueGrey.Lighten5).Table(table =>
                        {

                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header => {
                                header.Cell().BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).AlignLeft().Text("Datos del cliente").Bold();
                                header.Cell().Padding(3).AlignLeft().Text("Datos del proveedor").Bold();
                            });

                            //Cliente
                            table.Cell().Row(1).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(quote.Customer.CustomerNickName).FontSize(9);
                            table.Cell().Row(2).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(quote.Customer.Address).FontSize(9);
                            table.Cell().Row(3).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text($"{"Comprador:"} {finalUser.Name} {finalUser.LastName}").FontSize(9);
                            table.Cell().Row(4).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text($"{"Usuario:"} {buyer.Name} {buyer.LastName}").FontSize(9);

                            //SIMAQ
                            table.Cell().Row(1).Column(2).Padding(3).Text("SILLA MAQUINADOS ALTA PRECISION").FontSize(9);
                            table.Cell().Row(2).Column(2).Padding(3).Text("Aramberri #503, Col. Lazaro Cardenas Ampliación, Escobedo, NL.").FontSize(9);
                            table.Cell().Row(3).Column(2).Padding(3).Text("81 83 97 66 79, 81 15 34 99 71").FontSize(9);


                        });
                    });


                    page.Content().PaddingVertical(10).Table(table => {
                        table.ColumnsDefinition(columns => {
                            columns.ConstantColumn(10, QuestPDF.Infrastructure.Unit.Millimetre);//#
                            columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//Material
                            columns.ConstantColumn(100, QuestPDF.Infrastructure.Unit.Millimetre);//Descripción
                            columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//U.Medida
                            columns.ConstantColumn(18, QuestPDF.Infrastructure.Unit.Millimetre);//Cantidad
                            columns.RelativeColumn();//Precio
                            columns.RelativeColumn();//Total
                        });

                        table.Header(header => {
                            header.Cell().Background("626567").Element(encabezado).Text("#").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Material").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Descripción").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("U.M").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Cantidad").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Precio").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Total").FontSize(9).FontColor("FFBB11").Bold();
                        });

                        int counter = 1;

                        foreach (QuoteDetail qd in quote.QuoteDetails) {

                            subtotal += qd.Quantity * qd.Price;
                            ivatotal += iva * subtotal; 
                            total = subtotal + ivatotal;

                            table.Cell().Element(contenido).Padding(1).AlignCenter().Text(counter.ToString()).FontSize(9); //#
                            table.Cell().Element(contenido).Padding(1).AlignCenter().Text(qd.Material.MaterialName).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignLeft().Text(qd.Description).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignCenter().Text(qd.Unit.UnitName).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignRight().Text(qd.Quantity.ToString("N", nfi)).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignRight().Text(qd.Price.ToString("N", nfi)).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignRight().Text((qd.Price * qd.Quantity).ToString("N",nfi)).FontSize(9);                            
                        }

                        table.Footer(foot =>
                        {
                            //totales
                            foot.Cell().ColumnSpan(5);
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text("Subtotal").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(subtotal.ToString("N", nfi)).FontSize(9).Bold();

                            foot.Cell().ColumnSpan(5);
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text("IVA").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(ivatotal.ToString("N", nfi)).FontSize(9).Bold();

                            foot.Cell().ColumnSpan(4);
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(quote.Currency.Curr + " ").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text("Total").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(total.ToString("N", nfi)).FontSize(9).Bold();

                            foot.Cell().ColumnSpan(4);
                            foot.Cell().ColumnSpan(2).Element(totales).Padding(1).AlignRight().Text("Tipo de cambio").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(quote.ExchangeRate.ToString("N", nfi)).FontSize(9).Bold();

                            //Comentarios
                            foot.Cell().ColumnSpan(7).PaddingTop(20).Background("626567").AlignLeft().Element(comment)
                            .Text("Comentarios").FontSize(9).FontColor("FFBB11").Bold();

                            foot.Cell().ColumnSpan(7).PaddingLeft(1).Width(205,QuestPDF.Infrastructure.Unit.Millimetre).Border(1).BorderColor(Colors.Grey.Medium).AlignLeft().Text(quote.Comments).FontSize(9);
                        });
                    });


                  

                    page.Footer().Row(row =>
                    {
                        var rutaimagen = $"{path}{"simaq_footer.png"}";
                        byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);
                        row.ConstantItem(140).Height(65).AlignBottom().Text(txt =>
                        {
                            txt.Span("Pagina ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                        row.RelativeItem().Height(65);
                        row.ConstantItem(120).Height(65).AlignBottom().Image(imageData);
                    });

                });
            }).GeneratePdf();

            static IContainer encabezado(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.BlueGrey.Lighten5)
                    .AlignCenter();

            }

            static IContainer contenido(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.BlueGrey.Lighten5);

            }

            static IContainer totales(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.BlueGrey.Lighten5);

            }

            static IContainer comment(IContainer container)
            {
                return container
                    .Width(202, QuestPDF.Infrastructure.Unit.Millimetre)
                    .BorderLeft(1)
                    .BorderColor(Colors.BlueGrey.Lighten5);
                    

            }

            return Report;
        }

        public async Task<byte[]> GenerateWorkOrderReportPDFAsync(int WorkOrderId)
        {
            nfi.CurrencyDecimalDigits = 2;
            //nfi.CurrencyDecimalSeparator = ",";
            nfi.NumberDecimalDigits = 2;


            WorkOrder workorder = await _context.WorkOrders
                .Include(w => w.WorkOrderStatus)
                .Include(w => w.Customer)
                .Include(w => w.WorkOrderDetails).ThenInclude(wd => wd.Unit)
                .Include(w => w.WorkOrderDetails).ThenInclude(wd => wd.Material)
                .FirstOrDefaultAsync(q => q.WorkOrderId == WorkOrderId);



            Contact finalUser = await _context.Contacts.FindAsync(workorder.FinalUserId);
            Contact buyer = await _context.Contacts.FindAsync(workorder.BuyerContactId);

            QuestPDF.Settings.License = LicenseType.Community;
            var Report = QuestPDF.Fluent.Document.Create(document => {

                document.Page(page =>
                {
                    string path = Path.Combine(_host.WebRootPath, "Images\\");
                    var rutaimagen = $"{path}{"simaq_header.png"}";
                    byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);

                    decimal subtotal = 0;
                    decimal total = 0;
                    decimal iva = workorder.Tax / 100;
                    decimal ivatotal = 0;



                    page.Size(PageSizes.Letter);
                    page.Margin(5, QuestPDF.Infrastructure.Unit.Millimetre);

                    page.Header().ShowOnce().Column(col => {
                        col.Item().Row(row => row.RelativeItem().Height(68).Image(imageData));
                        col.Item().Background("FFBB11").Row(row => row.RelativeItem().Height(20).AlignRight().PaddingRight(5).Text(workorder.WorkOrderName).FontColor("#626567").Bold());
                        col.Item().Border(1).BorderColor(Colors.BlueGrey.Lighten5).Table(table =>
                        {

                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header => {
                                header.Cell().BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).AlignLeft().Text("Datos del cliente").Bold();
                                header.Cell().Padding(3).AlignLeft().Text("Datos del proveedor").Bold();
                            });

                            //Cliente
                            table.Cell().Row(1).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(workorder.Customer.CustomerNickName).FontSize(9);
                            table.Cell().Row(2).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(workorder.Customer.Address).FontSize(9);
                            table.Cell().Row(3).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text($"{"Comprador:"} {finalUser.Name} {finalUser.LastName}").FontSize(9);
                            table.Cell().Row(4).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text($"{"Usuario:"} {buyer.Name} {buyer.LastName}").FontSize(9);

                            //SIMAQ
                            table.Cell().Row(1).Column(2).Padding(3).Text("SILLA MAQUINADOS ALTA PRECISION").FontSize(9);
                            table.Cell().Row(2).Column(2).Padding(3).Text("Aramberri #503, Col. Lazaro Cardenas Ampliación, Escobedo, NL.").FontSize(9);
                            table.Cell().Row(3).Column(2).Padding(3).Text("81 83 97 66 79, 81 15 34 99 71").FontSize(9);


                        });
                    });


                    page.Content().PaddingVertical(10).Table(table => {
                        table.ColumnsDefinition(columns => {
                            columns.ConstantColumn(10, QuestPDF.Infrastructure.Unit.Millimetre);//#
                            columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//Material
                            columns.ConstantColumn(100, QuestPDF.Infrastructure.Unit.Millimetre);//Descripción
                            columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//U.Medida
                            columns.ConstantColumn(18, QuestPDF.Infrastructure.Unit.Millimetre);//Cantidad
                            columns.RelativeColumn();//Precio
                            columns.RelativeColumn();//Total
                        });

                        table.Header(header => {
                            header.Cell().Background("626567").Element(encabezado).Text("#").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Material").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Descripción").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("U.M").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Cantidad").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Precio").FontSize(9).FontColor("FFBB11").Bold();
                            header.Cell().Background("626567").Element(encabezado).Text("Total").FontSize(9).FontColor("FFBB11").Bold();
                        });

                        int counter = 1;

                        foreach (WorkOrderDetail wd in workorder.WorkOrderDetails)
                        {

                            subtotal += wd.Quantity * wd.Price;
                            ivatotal += iva * subtotal;
                            total = subtotal + ivatotal;

                            table.Cell().Element(contenido).Padding(1).AlignCenter().Text(counter.ToString()).FontSize(9); //#
                            table.Cell().Element(contenido).Padding(1).AlignCenter().Text(wd.Material.MaterialName).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignLeft().Text(wd.Description).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignCenter().Text(wd.Unit.UnitName).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignRight().Text(wd.Quantity.ToString("N", nfi)).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignRight().Text(wd.Price.ToString("N", nfi)).FontSize(9);
                            table.Cell().Element(contenido).Padding(1).AlignRight().Text((wd.Price * wd.Quantity).ToString("N", nfi)).FontSize(9);
                        }

                        table.Footer(foot =>
                        {
                            //totales
                            foot.Cell().ColumnSpan(5);
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text("Subtotal").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(subtotal.ToString("N", nfi)).FontSize(9).Bold();

                            foot.Cell().ColumnSpan(5);
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text("IVA").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(ivatotal.ToString("N", nfi)).FontSize(9).Bold();

                            foot.Cell().ColumnSpan(5);
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text("Total").FontSize(9).Bold();
                            foot.Cell().Element(totales).Padding(1).AlignRight().Text(total.ToString("N", nfi)).FontSize(9).Bold();

                            //Comentarios
                            foot.Cell().ColumnSpan(7).PaddingTop(20).Background("626567").AlignLeft().Element(comment)
                            .Text("Comentarios").FontSize(9).FontColor("FFBB11").Bold();

                            foot.Cell().ColumnSpan(7).PaddingLeft(1).Width(205, QuestPDF.Infrastructure.Unit.Millimetre).Border(1).BorderColor(Colors.Grey.Medium).AlignLeft().Text(workorder.Comments).FontSize(9);
                        });
                    });




                    page.Footer().Row(row =>
                    {
                        var rutaimagen = $"{path}{"simaq_footer.png"}";
                        byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);
                        row.ConstantItem(140).Height(65).AlignBottom().Text(txt =>
                        {
                            txt.Span("Pagina ").FontSize(10);
                            txt.CurrentPageNumber().FontSize(10);
                            txt.Span(" de ").FontSize(10);
                            txt.TotalPages().FontSize(10);
                        });
                        row.RelativeItem().Height(65);
                        row.ConstantItem(120).Height(65).AlignBottom().Image(imageData);
                    });

                });
            }).GeneratePdf();

            static IContainer encabezado(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.BlueGrey.Lighten5)
                    .AlignCenter();

            }

            static IContainer contenido(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.BlueGrey.Lighten5);

            }

            static IContainer totales(IContainer container)
            {
                return container
                    .Border(1)
                    .BorderColor(Colors.BlueGrey.Lighten5);

            }

            static IContainer comment(IContainer container)
            {
                return container
                    .Width(202, QuestPDF.Infrastructure.Unit.Millimetre)
                    .BorderLeft(1)
                    .BorderColor(Colors.BlueGrey.Lighten5);


            }

            return Report;
        }



    }
}
