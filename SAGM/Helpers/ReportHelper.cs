using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using QRCoder;
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
using Colors = QuestPDF.Helpers.Colors;

namespace SAGM.Helpers
{
    public class ReportHelper : IReportHelper
    {
        private readonly IWebHostEnvironment _host;
        private readonly SAGMContext _context;
        private readonly IUserHelper _userHelper;

        public ReportHelper(IWebHostEnvironment host, SAGMContext context, IUserHelper userHelper)
        {
            _host = host;
            _context = context;
            _userHelper = userHelper;

        }

        NumberFormatInfo nfi = new CultureInfo("es-MX", false).NumberFormat;


        public async Task<byte[]> GenerateQuoteReportPDFAsync(int QuoteId)
        {
            nfi.CurrencyDecimalDigits = 2;
            //nfi.CurrencyDecimalSeparator = ",";
            nfi.NumberDecimalDigits = 2;


            Quote quote = await _context.Quotes
                .Include(q => q.QuoteStatus)
                .Include(q => q.Currency)
                .Include(q => q.Customer)
                .Include(q => q.QuoteDetails).ThenInclude(qd => qd.Unit)
                .Include(q => q.QuoteDetails).ThenInclude(qd => qd.Material)
                .FirstOrDefaultAsync(q => q.QuoteId == QuoteId);

            string QrUri;
            byte[] qrCodeImage;
            QrUri = $"https://sagm.azurewebsites.net/Quotes/Details/{quote.QuoteId.ToString()}";
            qrCodeImage = QRCodeImage(QrUri);


            User seller = await _userHelper.GetUserAsync(quote.Seller);

            Contact finalUser = await _context.Contacts.FindAsync(quote.FinalUserId);
            Contact buyer = await _context.Contacts.FindAsync(quote.BuyerContactId);

            QuestPDF.Settings.License = LicenseType.Community;
            var Report = QuestPDF.Fluent.Document.Create(document =>
            {

                document.Page(page =>
                {
                string path = Path.Combine(_host.WebRootPath, "Images\\");
                var rutaimagen = $"{path}{"simaq_header.png"}";
                byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);

                decimal subtotal = 0;
                decimal total = 0;
                decimal iva = quote.Tax / 100;
                decimal ivatotal = 0;
                decimal discount = quote.Discount * -1;



                page.Size(PageSizes.Letter);
                page.Margin(5, QuestPDF.Infrastructure.Unit.Millimetre);

                page.Header().ShowOnce().Column(col =>
                {
                    col.Item().Row(row => row.RelativeItem().Height(68).Image(imageData));
                    col.Item().Background("FFBB11").Row(row => row.RelativeItem().Height(20).AlignRight().PaddingRight(5).Text(quote.QuoteName).FontColor("#626567").Bold());
                    col.Item().Border(1).BorderColor(Colors.BlueGrey.Lighten5).Table(table =>
                    {

                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();

                        });
                        table.Header(header =>
                        {
                            header.Cell().BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).AlignLeft().Text("Datos del cliente").Bold();
                            header.Cell().Padding(3).AlignLeft().Text("Datos del proveedor").Bold();
                        });

                        //Cliente
                        table.Cell().Row(1).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text(quote.Customer.CustomerNickName).FontSize(9);
                        table.Cell().Row(2).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text(quote.Customer.Address).FontSize(9);
                        table.Cell().Row(3).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text($"{"Comprador:"} {buyer.Name} {buyer.LastName}").FontSize(9);
                        table.Cell().Row(4).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(1).Text($"{"Usuario:"} {finalUser.Name} {finalUser.LastName}").FontSize(9); 

                        //SIMAQ
                        table.Cell().Row(1).Column(2).PaddingLeft(2).Text("SILLA MAQUINADOS ALTA PRECISION").FontSize(9);
                        table.Cell().Row(2).Column(2).PaddingLeft(2).Text("Aramberri #503, Col. Lazaro Cardenas Ampliación, Escobedo, NL.").FontSize(9);
                        table.Cell().Row(3).Column(2).PaddingLeft(2).Text("81 83 97 66 79, 81 15 34 99 71").FontSize(9);
                        table.Cell().Row(4).Column(2).PaddingLeft(2).Text($"{"Vendedor:"} {seller.FirstName} {seller.LastName}").FontSize(9);

                    });
                });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Table(table => {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(10, QuestPDF.Infrastructure.Unit.Millimetre);//#
                                columns.ConstantColumn(25, QuestPDF.Infrastructure.Unit.Millimetre);//Material
                                columns.ConstantColumn(90, QuestPDF.Infrastructure.Unit.Millimetre);//Descripción
                                columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//U.Medida
                                columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//Cantidad
                                columns.RelativeColumn();//Precio
                                columns.RelativeColumn();//Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("626567").Element(encabezado).Text("#").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Material").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Descripción").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("U.M").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Cantidad").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Precio").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Total").FontSize(9).FontColor("FFBB11").Bold();
                            });

                            int counter = 1;

                            foreach (QuoteDetail qd in quote.QuoteDetails)
                            {

                                subtotal += qd.Quantity * qd.Price;
                          

                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(counter.ToString()).FontSize(9); //#
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(qd.Material.MaterialName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignLeft().Text(qd.Description).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(qd.Unit.UnitName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text(qd.Quantity.ToString("N", nfi)).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text(qd.Price.ToString("N", nfi)).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text((qd.Price * qd.Quantity).ToString("N", nfi)).FontSize(9);
                                counter += 1;
                            }
                            ivatotal += iva * (subtotal+discount);
                            total = subtotal + discount + ivatotal ;
                        });
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            t.Cell().ColumnSpan(5);
                            t.Cell().Element(totales).Padding(1).AlignRight().Text("Subtotal").FontSize(9).Bold();
                            t.Cell().Element(totales).Padding(1).AlignRight().Text(subtotal.ToString("N", nfi)).FontSize(9).Bold();

                            t.Cell().ColumnSpan(5);
                            t.Cell().Element(totales).Padding(1).AlignRight().Text("Descuento").FontSize(9).Bold();
                            t.Cell().Element(totales).Padding(1).AlignRight().Text(discount.ToString("N", nfi)).FontSize(9).Bold();

                            t.Cell().ColumnSpan(5);
                            t.Cell().Element(totales).Padding(1).AlignRight().Text("IVA").FontSize(9).Bold();
                            t.Cell().Element(totales).Padding(1).AlignRight().Text(ivatotal.ToString("N", nfi)).FontSize(9).Bold();

                            t.Cell().ColumnSpan(4);
                            t.Cell().Element(totales).Padding(1).AlignRight().Text(quote.Currency.Curr + " ").FontSize(9).Bold();
                            t.Cell().Element(totales).Padding(1).AlignRight().Text("Total").FontSize(9).Bold();
                            t.Cell().Element(totales).Padding(1).AlignRight().Text(total.ToString("N", nfi)).FontSize(9).Bold();

                            t.Cell().ColumnSpan(4);
                            t.Cell().ColumnSpan(2).Element(totales).Padding(1).AlignRight().Text("Tipo de cambio").FontSize(9).Bold();
                            t.Cell().Element(totales).Padding(1).AlignRight().Text(quote.ExchangeRate.ToString("N", nfi)).FontSize(9).Bold();

                            //Comentarios
                            t.Cell().ColumnSpan(7).PaddingTop(20).Background("626567").AlignLeft().Element(comment)
                            .Text("Comentarios").FontSize(9).FontColor("FFBB11").Bold();

                            t.Cell().ColumnSpan(7).PaddingLeft(1).Width(205, QuestPDF.Infrastructure.Unit.Millimetre).Border(1).BorderColor(Colors.Grey.Medium).BorderRight(2).AlignLeft().Text(quote.Comments).FontSize(9);
                        });
                    });

                    


                    page.Footer().Row(row =>
                    {
                        var rutaimagen = $"{path}{"simaq_footer.png"}";
                        byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);
                        row.ConstantItem(65).Height(65).AlignBottom().Image(qrCodeImage).FitWidth().FitHeight();
                        row.ConstantItem(100).Height(65).AlignCenter().AlignBottom().Text(txt =>
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
                    .BorderColor(Colors.BlueGrey.Lighten5).PaddingLeft(2    );
                


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
            var Report = QuestPDF.Fluent.Document.Create(document =>
            {

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

                    page.Header().ShowOnce().Column(col =>
                    {
                        col.Item().Row(row => row.RelativeItem().Height(68).Image(imageData));
                        col.Item().Background("FFBB11").Row(row => row.RelativeItem().Height(20).AlignRight().PaddingRight(5).Text(workorder.WorkOrderName).FontColor("#626567").Bold());
                        col.Item().Border(1).BorderColor(Colors.BlueGrey.Lighten5).Table(table =>
                        {

                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
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


                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(10, QuestPDF.Infrastructure.Unit.Millimetre);//#
                            columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//Material
                            columns.ConstantColumn(100, QuestPDF.Infrastructure.Unit.Millimetre);//Descripción
                            columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//U.Medida
                            columns.ConstantColumn(18, QuestPDF.Infrastructure.Unit.Millimetre);//Cantidad
                            columns.RelativeColumn();//Precio
                            columns.RelativeColumn();//Total
                        });

                        table.Header(header =>
                        {
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

        public async Task<byte[]> GenerateOrderReportPDFAsync(int OrderId)
        {
            {
              

                nfi.CurrencyDecimalDigits = 2;
                //nfi.CurrencyDecimalSeparator = ",";
                nfi.NumberDecimalDigits = 2;


                Data.Entities.Order order = await _context.Orders
                    .Include(o => o.OrderStatus)
                    .Include(o => o.Currency)
                    .Include(o => o.Supplier)
                    .Include(o => o.OrderDetails).ThenInclude(od => od.Unit)
                    .Include(o => o.OrderDetails).ThenInclude(od => od.Material)
                    .FirstOrDefaultAsync(o => o.OrderId == OrderId);


                //-----------------------------------------------------


                string QrUri;
                byte[] qrCodeImage;
                QrUri = $"https://sagm.azurewebsites.net/Orders/Details/{order.OrderId.ToString()}";
                qrCodeImage = QRCodeImage(QrUri);
                //using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                //using (QRCodeData qrCodeData = qrGenerator.CreateQrCode, QRCodeGenerator.ECCLevel.Q))
                //using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                //{
                    
                //    qrCodeImage = qrCode.GetGraphic(10);

                //   // QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeImage));
                //}
               


                //-----------------------------------------------------




                Contact seller = await _context.Contacts.FindAsync(order.SupplierContactId);
                User buyer = await _userHelper.GetUserAsync(order.Buyer);

                QuestPDF.Settings.License = LicenseType.Community;
                var Report = QuestPDF.Fluent.Document.Create(document =>
                {

                    document.Page(page =>
                    {
                        string path = Path.Combine(_host.WebRootPath, "Images\\");
                        var rutaimagen = $"{path}{"simaq_header.png"}";
                        byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);

                        decimal subtotal = 0;
                        decimal total = 0;
                        decimal iva = order.Tax / 100;
                        decimal ivatotal = 0;



                        page.Size(PageSizes.Letter);
                        page.Margin(5, QuestPDF.Infrastructure.Unit.Millimetre);

                        page.Header().ShowOnce().Column(col =>
                        {
                            col.Item().Row(row => row.RelativeItem().Height(68).Image(imageData));
                            //col.Item().Row(row => row.RelativeItem().Height(40).Image(qrCodeImage).FitWidth().FitHeight());
                            col.Item().Background("FFBB11").Row(row => row.RelativeItem().Height(20).AlignRight().PaddingRight(5).Text(order.OrderName).FontColor("#626567").Bold());
                            col.Item().Border(1).BorderColor(Colors.BlueGrey.Lighten5).Table(table =>
                            {

                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();

                                });
                                table.Header(header =>
                                {
                                    header.Cell().BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).AlignLeft().Text("Datos del proveedor").Bold();
                                    header.Cell().Padding(3).AlignLeft().Text("Datos del cliente").Bold();
                                });

                                //Cliente
                                table.Cell().Row(1).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(order.Supplier.SupplierNickName).FontSize(9);
                                table.Cell().Row(2).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(order.Supplier.TaxId).FontSize(9);
                                table.Cell().Row(3).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(order.Supplier.Address).FontSize(9);
                                table.Cell().Row(4).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text($"{"Vendedor:"} {seller.Name} {seller.LastName}").FontSize(9);

                                //SIMAQ
                                table.Cell().Row(1).Column(2).Padding(3).Text("SILLA MAQUINADOS ALTA PRECISION").FontSize(9);
                                table.Cell().Row(2).Column(2).Padding(3).Text("SMA1108195B6").FontSize(9);
                                table.Cell().Row(3).Column(2).Padding(3).Text("Aramberri #503, Col. Lazaro Cardenas Ampliación, Escobedo, NL.").FontSize(9);
                                table.Cell().Row(4).Column(2).Padding(3).Text("81 83 97 66 79, 81 15 34 99 71").FontSize(9);
                                table.Cell().Row(5).Column(2).Padding(3).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text($"{"Comprador:"} {buyer.FirstName} {buyer.LastName}").FontSize(9);


                            });
                        });


                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(10, QuestPDF.Infrastructure.Unit.Millimetre);//#
                                columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//Material
                                columns.ConstantColumn(100, QuestPDF.Infrastructure.Unit.Millimetre);//Descripción
                                columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//U.Medida
                                columns.ConstantColumn(18, QuestPDF.Infrastructure.Unit.Millimetre);//Cantidad
                                columns.RelativeColumn();//Precio
                                columns.RelativeColumn();//Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("626567").Element(encabezado).Text("#").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Material").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Descripción").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("U.M").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Cantidad").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Precio").FontSize(9).FontColor("FFBB11").Bold();
                                header.Cell().Background("626567").Element(encabezado).Text("Total").FontSize(9).FontColor("FFBB11").Bold();
                            });

                            int counter = 1;

                            foreach (OrderDetail qd in order.OrderDetails)
                            {

                                subtotal += qd.Quantity * qd.Price;
                               

                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(counter.ToString()).FontSize(9); //#
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(qd.Material.MaterialName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignLeft().Text(qd.Description).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(qd.Unit.UnitName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text(qd.Quantity.ToString("N", nfi)).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text(qd.Price.ToString("N", nfi)).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text((qd.Price * qd.Quantity).ToString("N", nfi)).FontSize(9);
                            }
                            ivatotal += iva * subtotal;
                            total = subtotal + ivatotal;

                            table.Footer(foot =>
                            {
                                //foot.Cell().Row(1).Column(1).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).Text(order.Supplier.SupplierNickName).FontSize(9);
                                //totales
                                foot.Cell().ColumnSpan(5);
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text("Subtotal").FontSize(9).Bold();
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text(subtotal.ToString("N", nfi)).FontSize(9).Bold();

                                foot.Cell().ColumnSpan(5);
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text("IVA").FontSize(9).Bold();
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text(ivatotal.ToString("N", nfi)).FontSize(9).Bold();

                                foot.Cell().ColumnSpan(4);
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text(order.Currency.Curr + " ").FontSize(9).Bold();
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text("Total").FontSize(9).Bold();
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text(total.ToString("N", nfi)).FontSize(9).Bold();

                                foot.Cell().ColumnSpan(4);
                                foot.Cell().ColumnSpan(2).Element(totales).Padding(1).AlignRight().Text("Tipo de cambio").FontSize(9).Bold();
                                foot.Cell().Element(totales).Padding(1).AlignRight().Text(order.ExchangeRate.ToString("N", nfi)).FontSize(9).Bold();

                                //Comentarios
                                foot.Cell().ColumnSpan(7).PaddingTop(20).Background("626567").AlignLeft().Element(comment)
                                .Text("Comentarios").FontSize(9).FontColor("FFBB11").Bold();

                                foot.Cell().ColumnSpan(7).PaddingLeft(1).Width(205, QuestPDF.Infrastructure.Unit.Millimetre).Border(1).BorderColor(Colors.Grey.Medium).AlignLeft().Text(order.Comments).FontSize(9);
                            });
                        });




                        page.Footer().Row(row =>
                        {
                            var rutaimagen = $"{path}{"simaq_footer.png"}";
                            byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);
                            row.ConstantItem(65).Height(65).AlignBottom().Image(qrCodeImage).FitWidth().FitHeight();
                            row.ConstantItem(100).Height(65).AlignCenter().AlignBottom().Text(txt =>
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

        public async Task<byte[]> GenerateRemisionReportPDFAsync(int workOrderDeliveryId)
        {
            nfi.CurrencyDecimalDigits = 2;
            nfi.NumberDecimalDigits = 2;

            WorkOrderDelivery remision = await _context.WorkOrderDeliveries
                .Include(r => r.WorkOrder).ThenInclude(r => r.Customer)
                .Include(r => r.WorkOrder).ThenInclude(r => r.WorkOrderDetails).ThenInclude(d => d.Unit)
                .Include(r => r.WorkOrder).ThenInclude(r => r.WorkOrderDetails).ThenInclude(d => d.Material)
                .Include(r => r.WorkOrderDeliveryDetails)
                .FirstOrDefaultAsync(r => r.WorkOrderDeliveryId == workOrderDeliveryId);


            Contact buyer = await _context.Contacts.FindAsync(remision.WorkOrder.BuyerContactId);
            Contact finalUser = await _context.Contacts.FindAsync(remision.WorkOrder.FinalUserId);

            QuestPDF.Settings.License = LicenseType.Community;
            var Report = QuestPDF.Fluent.Document.Create(document =>
            {

                document.Page(page =>
                {
                    string path = Path.Combine(_host.WebRootPath, "Images\\");
                    var rutaimagen = $"{path}{"simaq_header_remision.jpg"}";
                    byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);

                    page.Size(PageSizes.Letter);
                    page.Margin(5, QuestPDF.Infrastructure.Unit.Millimetre);

                    page.Header().ShowOnce().Column(col =>
                    {
                        col.Item().Row(row => row.RelativeItem().Height(68).Image(imageData));
                        col.Item().Row(row => row.RelativeItem().Height(20).AlignRight().PaddingRight(5).Text(remision.WorkOrderDeliveryName).FontColor("626567").BackgroundColor("5aede8").Bold());
                        col.Item().Border(1).BorderColor(Colors.BlueGrey.Lighten5).Table(table =>
                        {

                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).AlignLeft().Text("Datos del cliente").Bold();
                                header.Cell().Padding(3).AlignLeft().Text("Datos del proveedor").Bold();
                            });

                            //Cliente
                            table.Cell().Row(1).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text(remision.WorkOrder.Customer.CustomerNickName).FontSize(9);
                            table.Cell().Row(2).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text(remision.WorkOrder.Customer.Address).FontSize(9);
                            table.Cell().Row(3).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text($"{"Comprador:"} {buyer.Name} {buyer.LastName}").FontSize(9);
                            table.Cell().Row(4).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(1).Text($"{"Usuario:"} {finalUser.Name} {finalUser.LastName}").FontSize(9);
                            

                            //SIMAQ
                            table.Cell().Row(1).Column(2).PaddingLeft(2).Text("SILLA MAQUINADOS ALTA PRECISION").FontSize(9);
                            table.Cell().Row(2).Column(2).PaddingLeft(2).Text("Aramberri #503, Col. Lazaro Cardenas Ampliación, Escobedo, NL.").FontSize(9);
                            table.Cell().Row(3).Column(2).PaddingLeft(2).Text("81 83 97 66 79, 81 15 34 99 71").FontSize(9);


                        });
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Table(table => {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(10, QuestPDF.Infrastructure.Unit.Millimetre);//#
                                columns.RelativeColumn();//Material
                                columns.ConstantColumn(120, QuestPDF.Infrastructure.Unit.Millimetre);//Descripción
                                columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//U.Medida
                                columns.ConstantColumn(18, QuestPDF.Infrastructure.Unit.Millimetre);//Cantidad

                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("5aede8").Element(encabezado).Text("#").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("Material").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("Descripción").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("U.M").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("Cantidad").FontSize(9).FontColor("626567").Bold();

                            });

                            int counter = 1;

                            foreach (WorkOrderDeliveryDetail dd in remision.WorkOrderDeliveryDetails)
                            {



                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(counter.ToString()).FontSize(9); //#
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(dd.workOrderDetail.Material.MaterialName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignLeft().Text(dd.workOrderDetail.Description).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(dd.workOrderDetail.Unit.UnitName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text(dd.Quantity.ToString("N", nfi)).FontSize(9);

                                counter += 1;
                            }
                        });
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();


                            });
                      
                            //Comentarios
                            t.Cell().PaddingTop(20).Background("5aede8").AlignLeft().Element(comment).Text("Comentarios").FontSize(9).FontColor("626567").Bold();
                            t.Cell().PaddingLeft(1).Width(205, QuestPDF.Infrastructure.Unit.Millimetre).Border(1).BorderColor(Colors.Grey.Medium).AlignLeft().Text(remision.Comments).FontSize(9);

                            
                        });
                        col.Item().PaddingTop(5).Table(t => 
                        {
                            t.ColumnsDefinition( columns => 
                            {
                                columns.ConstantColumn(45, QuestPDF.Infrastructure.Unit.Millimetre);
                                columns.ConstantColumn(80, QuestPDF.Infrastructure.Unit.Millimetre);//#
                            });

                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("Nombre de quien recibe").FontSize(10);
                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("");
                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("Fecha").FontSize(10); ;
                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("");
                            t.Cell().Height(15, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("Firma").FontSize(10); ;
                            t.Cell().Height(15, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("");
                        });
                    });




                    page.Footer().Row(row =>
                    {
                        var rutaimagen = $"{path}{"simaq_footer_remision.jpg"}";
                        byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);
                        row.ConstantItem(140).Height(65).AlignBottom().Text(txt =>
                        {
                            txt.Span("Página ").FontSize(8).FontFamily("Tahoma").FontColor("626567");
                            txt.CurrentPageNumber().FontSize(8).FontFamily("Tahoma").FontColor("626567");
                            txt.Span(" de ").FontSize(8).FontFamily("Tahoma").FontColor("626567");
                            txt.TotalPages().FontSize(8).FontFamily("Tahoma").FontColor("626567");
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

            static IContainer comment(IContainer container)
            {
                return container
                    .Width(202, QuestPDF.Infrastructure.Unit.Millimetre)
                    .BorderLeft(1)
                    .BorderColor(Colors.BlueGrey.Lighten5).PaddingLeft(2);



            }

            return Report;
        }

        public async Task<byte[]> GenerateReceiptReportPDFAsync(int orderReceiptId)
        {
            nfi.CurrencyDecimalDigits = 2;
            nfi.NumberDecimalDigits = 2;

            OrderReceipt receipt = await _context.OrderReceipts
             .Include(r => r.Order).ThenInclude(r => r.Supplier)
             .Include(r => r.Order).ThenInclude(r => r.OrderDetails).ThenInclude(d => d.Unit)
             .Include(r => r.Order).ThenInclude(r => r.OrderDetails).ThenInclude(d => d.Material)
             .Include(r => r.OrderReceiptDetails)
             .FirstOrDefaultAsync(r => r.OrderReceiptId == orderReceiptId);

            Contact seller = await _context.Contacts.FindAsync(receipt.Order.SupplierContactId);

            QuestPDF.Settings.License = LicenseType.Community;
            var Report = QuestPDF.Fluent.Document.Create(document =>
            {

                document.Page(page =>
                {
                    string path = Path.Combine(_host.WebRootPath, "Images\\");
                    var rutaimagen = $"{path}{"simaq_header_receipt.jpg"}";
                    byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);

                    page.Size(PageSizes.Letter);
                    page.Margin(5, QuestPDF.Infrastructure.Unit.Millimetre);

                    page.Header().ShowOnce().Column(col =>
                    {
                        col.Item().Row(row => row.RelativeItem().Height(68).Image(imageData));
                        col.Item().Row(row => row.RelativeItem().Height(20).AlignRight().PaddingRight(5).Text(receipt.ReceiptName).FontColor("626567").BackgroundColor("5aede8").Bold());
                        col.Item().Border(1).BorderColor(Colors.BlueGrey.Lighten5).Table(table =>
                        {

                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Padding(3).AlignLeft().Text("Datos del cliente").Bold();
                                header.Cell().Padding(3).AlignLeft().Text("Datos del proveedor").Bold();
                            });

                            //Cliente
                            table.Cell().Row(1).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text(receipt.Order.Supplier.SupplierName).FontSize(9);
                            table.Cell().Row(2).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text(receipt.Order.Supplier.Address).FontSize(9);
                            table.Cell().Row(3).Column(1).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text($"{"Vendedor:"} {seller.Name} {seller.LastName}").FontSize(9);


                            //SIMAQ
                            table.Cell().Row(1).Column(2).PaddingLeft(2).Text("SILLA MAQUINADOS ALTA PRECISION").FontSize(9);
                            table.Cell().Row(2).Column(2).PaddingLeft(2).Text("Aramberri #503, Col. Lazaro Cardenas Ampliación, Escobedo, NL.").FontSize(9);
                            table.Cell().Row(3).Column(2).PaddingLeft(2).Text("81 83 97 66 79, 81 15 34 99 71").FontSize(9);
                            table.Cell().Row(4).Column(2).PaddingLeft(2).BorderRight(1).BorderColor(Colors.BlueGrey.Lighten5).Text(receipt.Order.Buyer).FontSize(9);

                        });
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Table(table => {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(10, QuestPDF.Infrastructure.Unit.Millimetre);//#
                                columns.RelativeColumn();//Material
                                columns.ConstantColumn(100, QuestPDF.Infrastructure.Unit.Millimetre);//Descripción
                                columns.ConstantColumn(17, QuestPDF.Infrastructure.Unit.Millimetre);//U.Medida
                                columns.ConstantColumn(18, QuestPDF.Infrastructure.Unit.Millimetre);//Cantidad

                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("5aede8").Element(encabezado).Text("#").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("Material").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("Descripción").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("U.M").FontSize(9).FontColor("626567").Bold();
                                header.Cell().Background("5aede8").Element(encabezado).Text("Cantidad").FontSize(9).FontColor("626567").Bold();

                            });

                            int counter = 1;

                            foreach (OrderReceiptDetail dd in receipt.OrderReceiptDetails)
                            {



                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(counter.ToString()).FontSize(9); //#
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(dd.OrderDetail.Material.MaterialName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignLeft().Text(dd.OrderDetail.Description).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignCenter().Text(dd.OrderDetail.Unit.UnitName).FontSize(9);
                                table.Cell().Element(contenido).Padding(1).AlignRight().Text(dd.Quantity.ToString("N", nfi)).FontSize(9);

                                counter += 1;
                            }
                        });
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();


                            });

                            //Comentarios
                            t.Cell().PaddingTop(20).Background("5aede8").AlignLeft().Element(comment).Text("Comentarios").FontSize(9).FontColor("626567").Bold();
                            t.Cell().PaddingLeft(1).Width(205, QuestPDF.Infrastructure.Unit.Millimetre).Border(1).BorderColor(Colors.Grey.Medium).AlignLeft().Text(receipt.Comments).FontSize(9);


                        });
                        col.Item().PaddingTop(7).Table(t =>
                        {
                            t.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(45, QuestPDF.Infrastructure.Unit.Millimetre);
                                columns.ConstantColumn(80, QuestPDF.Infrastructure.Unit.Millimetre);//#
                            });

                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("Nombre de quien recibe").FontSize(10);
                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("");
                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("Fecha").FontSize(10); ;
                            t.Cell().Height(7, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("");
                            t.Cell().Height(15, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("Firma").FontSize(10); ;
                            t.Cell().Height(15, QuestPDF.Infrastructure.Unit.Millimetre).Element(contenido).Padding(1).AlignLeft().Text("");
                        });
                    });




                    page.Footer().Row(row =>
                    {
                        var rutaimagen = $"{path}{"simaq_footer_receipt.jpg"}";
                        byte[] imageData = System.IO.File.ReadAllBytes(rutaimagen);
                        row.ConstantItem(140).Height(65).AlignBottom().Text(txt =>
                        {
                            txt.Span("Página ").FontSize(8).FontFamily("Tahoma").FontColor("626567");
                            txt.CurrentPageNumber().FontSize(8).FontFamily("Tahoma").FontColor("626567");
                            txt.Span(" de ").FontSize(8).FontFamily("Tahoma").FontColor("626567");
                            txt.TotalPages().FontSize(8).FontFamily("Tahoma").FontColor("626567");
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

            static IContainer comment(IContainer container)
            {
                return container
                    .Width(202, QuestPDF.Infrastructure.Unit.Millimetre)
                    .BorderLeft(1)
                    .BorderColor(Colors.BlueGrey.Lighten5).PaddingLeft(2);



            }

            return Report;



            throw new NotImplementedException();
        }

        private byte[] QRCodeImage(string url)
        {
            string QrUri;
            byte[] qrCodeImage;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {

                qrCodeImage = qrCode.GetGraphic(10);

                // QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeImage));
            }

            return qrCodeImage;
        }

    }
}
