// RTL Support provided by Credo inc (www.credo.co.il  ||   info@credo.co.il)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Invoices;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.NNBoxGenerator;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Services.Common
{
    /// <summary>
    /// PDF service
    /// </summary>
    public partial class PdfService : IPdfService
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly ICompanyService _companyService;
        private readonly IWorkContext _workContext;
        private readonly IShippingService _shippingServices;
        private readonly IInvoiceService _invoiceService;
        private readonly IBoxesGeneratorServices _boxPackingService;

        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly BaseColor color;

        #endregion

        #region Ctor

        public PdfService(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAddressAttributeFormatter addressAttributeFormatter,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            INopFileProvider fileProvider,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            ICompanyService companyService, IShippingService shippingServices, IInvoiceService invoiceService,
            IBoxesGeneratorServices boxPackingService)
        {
            _addressSettings = addressSettings;
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _addressAttributeFormatter = addressAttributeFormatter;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _measureService = measureService;
            _fileProvider = fileProvider;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _vendorService = vendorService;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _companyService = companyService;
            color = new BaseColor(227, 227, 227);// silver
            _shippingServices = shippingServices;
            _invoiceService = invoiceService;
            _boxPackingService = boxPackingService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get font
        /// </summary>
        /// <returns>Font</returns>
        protected virtual Font GetFont()
        {
            //nopCommerce supports Unicode characters
            //nopCommerce uses Free Serif font by default (~/App_Data/Pdf/FreeSerif.ttf file)
            //It was downloaded from http://savannah.gnu.org/projects/freefont
            return GetFont(_pdfSettings.FontFileName);
        }

        /// <summary>
        /// Get font
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <returns>Font</returns>
        protected virtual Font GetFont(string fontFileName)
        {
            if (fontFileName == null)
                throw new ArgumentNullException(nameof(fontFileName));

            var fontPath = _fileProvider.Combine(_fileProvider.MapPath("~/App_Data/Pdf/"), fontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 9, Font.NORMAL);
            return font;
        }

        /// <summary>
        /// Get font direction
        /// </summary>
        /// <param name="lang">Language</param>
        /// <returns>Font direction</returns>
        protected virtual int GetDirection(Language lang)
        {
            return lang.Rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;
        }


        /// <summary>
        /// Get element alignment
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="isOpposite">Is opposite?</param>
        /// <returns>Element alignment</returns>
        protected virtual int GetAlignment(Language lang, bool isOpposite = false)
        {
            //if we need the element to be opposite, like logo etc`.
            if (!isOpposite)
                return lang.Rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

            return lang.Rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;
        }

        /// <summary>
        /// Get PDF cell
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <returns>PDF cell</returns>
        protected virtual PdfPCell GetPdfCell(string resourceKey, Language lang, Font font)
        {
            return new PdfPCell(new Phrase(_localizationService.GetResource(resourceKey, lang.Id), font));
        }

        /// <summary>
        /// Get PDF cell
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="font">Font</param>
        /// <returns>PDF cell</returns>
        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }

        /// <summary>
        /// Get paragraph
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <param name="args">Locale arguments</param>
        /// <returns>Paragraph</returns>
        protected virtual Paragraph GetParagraph(string resourceKey, Language lang, Font font, params object[] args)
        {
            return GetParagraph(resourceKey, string.Empty, lang, font, args);
        }

        /// <summary>
        /// Get paragraph
        /// </summary>
        /// <param name="resourceKey">Locale</param>
        /// <param name="indent">Indent</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        /// <param name="args">Locale arguments</param>
        /// <returns>Paragraph</returns>
        protected virtual Paragraph GetParagraph(string resourceKey, string indent, Language lang, Font font, params object[] args)
        {
            var formatText = _localizationService.GetResource(resourceKey, lang.Id);
            return new Paragraph(indent + (args.Any() ? string.Format(formatText, args) : formatText), font);
        }

        /// <summary>
        /// Print footer
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="pdfWriter">PDF writer</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        protected virtual void PrintFooter(PdfSettings pdfSettingsByStore, PdfWriter pdfWriter, Rectangle pageSize, Language lang, Font font)
        {
            if (string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) && string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2))
                return;

            var column1Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1)
                ? new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn1
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            var column2Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2)
                ? new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn2
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

            if (!column1Lines.Any() && !column2Lines.Any())
                return;

            var totalLines = Math.Max(column1Lines.Count, column2Lines.Count);
            const float margin = 43;

            //if you have really a lot of lines in the footer, then replace 9 with 10 or 11
            var footerHeight = totalLines * 9;
            var directContent = pdfWriter.DirectContent;
            directContent.MoveTo(pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight);
            directContent.LineTo(pageSize.GetRight(margin), pageSize.GetBottom(margin) + footerHeight);
            directContent.Stroke();

            var footerTable = new PdfPTable(2)
            {
                WidthPercentage = 100f,
                RunDirection = GetDirection(lang)
            };
            footerTable.SetTotalWidth(new float[] { 250, 250 });

            //column 1
            if (column1Lines.Any())
            {
                var column1 = new PdfPCell(new Phrase())
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                foreach (var footerLine in column1Lines)
                {
                    column1.Phrase.Add(new Phrase(footerLine, font));
                    column1.Phrase.Add(new Phrase(Environment.NewLine));
                }

                footerTable.AddCell(column1);
            }
            else
            {
                var column = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
                footerTable.AddCell(column);
            }

            //column 2
            if (column2Lines.Any())
            {
                var column2 = new PdfPCell(new Phrase())
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                foreach (var footerLine in column2Lines)
                {
                    column2.Phrase.Add(new Phrase(footerLine, font));
                    column2.Phrase.Add(new Phrase(Environment.NewLine));
                }

                footerTable.AddCell(column2);
            }
            else
            {
                var column = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
                footerTable.AddCell(column);
            }

            footerTable.WriteSelectedRows(0, totalLines, pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight, directContent);
        }

        /// <summary>
        /// Print order notes
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="order">Order</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="font">Font</param>
        protected virtual void PrintOrderNotes(PdfSettings pdfSettingsByStore, Order order, Language lang, Font titleFont, Document doc, Font font)
        {
            if (!pdfSettingsByStore.RenderOrderNotes)
                return;

            var orderNotes = order.OrderNotes
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList();

            if (!orderNotes.Any())
                return;

            var notesHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cellOrderNote = GetPdfCell("PDFInvoice.OrderNotes", lang, titleFont);
            cellOrderNote.Border = Rectangle.NO_BORDER;
            notesHeader.AddCell(cellOrderNote);
            doc.Add(notesHeader);
            doc.Add(new Paragraph(" "));

            var notesTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            notesTable.SetWidths(lang.Rtl ? new[] { 70, 30 } : new[] { 30, 70 });

            //created on
            cellOrderNote = GetPdfCell("PDFInvoice.OrderNotes.CreatedOn", lang, font);
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            //note
            cellOrderNote = GetPdfCell("PDFInvoice.OrderNotes.Note", lang, font);
            cellOrderNote.BackgroundColor = BaseColor.LightGray;
            cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
            notesTable.AddCell(cellOrderNote);

            foreach (var orderNote in orderNotes)
            {
                cellOrderNote = GetPdfCell(_dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc), font);
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

                cellOrderNote = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(_orderService.FormatOrderNoteText(orderNote), true, true), font);
                cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                notesTable.AddCell(cellOrderNote);

                //should we display a link to downloadable files here?
                //I think, no. Anyway, PDFs are printable documents and links (files) are useful here
            }

            doc.Add(notesTable);
        }
        protected virtual void PrintOrderNNBoxGenerator(PdfSettings pdfSettingsByStore, Order order, Language lang, Font titleFont, Document doc, Font font)
        {
            const string indent = "";
            var GetBoxGenerator = _shippingServices.GetBoxByOrder(order.Id, order.CustomerId).ToList();

            if (!GetBoxGenerator.Any())
                return;

         
            
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cellProducts = GetPdfCell("OrderDetail.PackingSummary", lang, titleFont);
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));

            var orderItems = order.OrderItems;

            var count = 5 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            //var widths = new Dictionary<int, int[]>
            //{
            //    { 4, new[] {  10, 50,20, 20 } },
            //    { 5, new[] { 45, 15, 15, 10, 15 } },
            //    { 6, new[] { 40, 13, 13, 12, 10, 12 } }
            //};

            //productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            //qty
            var cellProductItem = GetPdfCell("orderdetail.boxtype", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //product name
            cellProductItem = GetPdfCell("orderdetail.boxsize", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            productsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
            productsTable.AddCell(cellProductItem);


            //product name
            cellProductItem = GetPdfCell("orderdetail.totalweight", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            productsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
            productsTable.AddCell(cellProductItem);

           
            cellProductItem = GetPdfCell("orderdetail.contentsweight", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
            productsTable.AddCell(cellProductItem);

            cellProductItem = GetPdfCell("orderdetail.contentsproducts", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
            productsTable.AddCell(cellProductItem);


            foreach (var orderItem in GetBoxGenerator)
            {
                var BoxInfo = new BSBox();

                if (orderItem.Container != null)
                    if (orderItem.Container?.ID != 0)
                        BoxInfo = _shippingServices.GetBoxById(orderItem.Container.ID);


                var BoxContentWeight = orderItem.PercentItemWeightPacked + BoxInfo.WeigthBox;
                var Products = new List<ItemProductSummary>();
                string productsInfo = "";
                if (orderItem.IsAsShip)
                {
                    var productBox = _boxPackingService.GetBSItemPackList(orderItem.Id);
                    var ProductsGroup = productBox.GroupBy(r => r.ProductId);

                    foreach (var prod in ProductsGroup)
                    {
                        var product = _productService.GetProductById(prod.Key);
                        if (product != null)
                        {
                            var ProductItem = new ItemProductSummary();
                            ProductItem.Id = product.Id;
                            ProductItem.ProductName = product.Name;
                            ProductItem.Sku = product.Sku;
                            ProductItem.Quantity = prod.Count();
                            productsInfo =  product.Name + " (Qty: "+ prod.Count() + "), " + productsInfo;
                            ProductItem.Weight = product.Weight;
                            Products.Add(ProductItem);
                        }
                    }
                    //var product = _productServices.GetProductById(BoxGenerator.Id);

                }
                else
                {
                    var ProductsGroup = orderItem.PackedItems.GroupBy(r => r.ID);

                    foreach (var r in ProductsGroup)
                    {
                        var product = _productService.GetProductById(r.Key);
                        if (product != null)
                        {
                            var ProductItem = new ItemProductSummary();
                            ProductItem.Id = product.Id;
                            ProductItem.ProductName = product.Name;
                            productsInfo = product.Name  + ", "+ productsInfo;
                            ProductItem.Sku = product.Sku;
                            ProductItem.Quantity = r.Count();
                            ProductItem.Weight = product.Weight;
                            Products.Add(ProductItem);
                        }
                    }
                }

                var size = BoxInfo?.Height + " in x " + BoxInfo?.Width + " in x  " + BoxInfo?.Length + " in ";

                if (orderItem.IsAsShip)
                {
                    size = orderItem.Container.Height + " in x " + orderItem.Container.Width + " in x " + orderItem.Container.Length + " in ";
                    BoxInfo.Name = "Own";
                }

                var totalweight = Math.Round(orderItem.PercentItemWeightPacked);
                var contentWeight = Math.Round(BoxContentWeight);

                //Name
                cellProductItem = GetPdfCell(BoxInfo?.Name, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(size, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(totalweight, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(contentWeight, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);

                cellProductItem = GetPdfCell(productsInfo, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);

            }

            doc.Add(productsTable);

        }
        
        /// <summary>
        /// Print totals
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">PDF document</param>
        protected virtual void PrintTotals(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //vendors cannot see totals
            if (vendorId != 0)
                return;
            doc.Add(new Phrase(Environment.NewLine, new Font { Size = 2f }));
            Chunk line = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, color, Element.ALIGN_CENTER, 5f));
            doc.Add(line);

            //subtotal
            var totalsTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            totalsTable.DefaultCell.Border = Rectangle.BOX;

            //subtotal
            var sutTotalTable = new PdfPTable(3)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            sutTotalTable.DefaultCell.Border = Rectangle.BOX;
            sutTotalTable.SetWidths(new[] { 75, 15, 10 });
            sutTotalTable.HorizontalAlignment = Rectangle.ALIGN_RIGHT;

            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax
                var orderSubtotalInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                var orderSubtotalInclTaxStr = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, lang, true);

                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id)}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                p.PaddingTop = 5;
                p.PaddingBottom = 5;

                var pv = GetPdfCell($" {orderSubtotalInclTaxStr}", font);
                pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                pv.Border = Rectangle.NO_BORDER;
                pv.PaddingTop = 5;
                pv.PaddingBottom = 5;

                sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                sutTotalTable.AddCell(p);
                sutTotalTable.AddCell(pv);
                //totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax
                var orderSubtotalExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                var orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, lang, false);

                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id)}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;

                var pv = GetPdfCell($" {orderSubtotalExclTaxStr}", font);
                pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                pv.Border = Rectangle.NO_BORDER;

                sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                sutTotalTable.AddCell(p);
                sutTotalTable.AddCell(pv);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                    !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax
                    var orderSubTotalDiscountInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(
                        -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", lang.Id)} ", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;

                    var pv = GetPdfCell($" {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pv.Border = Rectangle.NO_BORDER;

                    sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                    sutTotalTable.AddCell(p);
                    sutTotalTable.AddCell(pv);
                }
                else
                {
                    //excluding tax
                    var orderSubTotalDiscountExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(
                        -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", lang.Id)}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;

                    var pv = GetPdfCell($" {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pv.Border = Rectangle.NO_BORDER;

                    sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                    sutTotalTable.AddCell(p);
                    sutTotalTable.AddCell(pv);
                }
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    var orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(
                        orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Shipping", lang.Id)}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;

                    var pv = GetPdfCell($" {orderShippingInclTaxStr}", font);
                    pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pv.Border = Rectangle.NO_BORDER;

                    sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                    sutTotalTable.AddCell(p);
                    sutTotalTable.AddCell(pv);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    var orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(
                        orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Shipping", lang.Id)} ", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;

                    var pv = GetPdfCell($" {orderShippingExclTaxStr}", font);
                    pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pv.Border = Rectangle.NO_BORDER;

                    sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                    sutTotalTable.AddCell(p);
                    sutTotalTable.AddCell(pv);
                }
            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id)}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;

                    var pv = GetPdfCell($" {paymentMethodAdditionalFeeInclTaxStr}", font);
                    pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pv.Border = Rectangle.NO_BORDER;

                    sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                    sutTotalTable.AddCell(p);
                    sutTotalTable.AddCell(pv);

                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id)}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;

                    var pv = GetPdfCell($" {paymentMethodAdditionalFeeExclTaxStr}", font);
                    pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pv.Border = Rectangle.NO_BORDER;

                    sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                    sutTotalTable.AddCell(p);
                    sutTotalTable.AddCell(pv);

                }
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, lang);
                }
            }

            if (displayTax)
            {
                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Tax", lang.Id)}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;

                var pv = GetPdfCell($" {taxStr}", font);
                pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                pv.Border = Rectangle.NO_BORDER;

                sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                sutTotalTable.AddCell(p);
                sutTotalTable.AddCell(pv);
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(_localizationService.GetResource("PDFInvoice.TaxRate", lang.Id),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = _priceFormatter.FormatPrice(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, lang);

                    var p = GetPdfCell($"{taxRate}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;

                    var pv = GetPdfCell($" {taxValue}", font);
                    pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pv.Border = Rectangle.NO_BORDER;

                    sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                    sutTotalTable.AddCell(p);
                    sutTotalTable.AddCell(pv);
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                var orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, lang);

                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", lang.Id)}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;

                var pv = GetPdfCell($" {orderDiscountInCustomerCurrencyStr}", font);
                pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                pv.Border = Rectangle.NO_BORDER;

                sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                sutTotalTable.AddCell(p);
                sutTotalTable.AddCell(pv);

            }

            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                var gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", lang.Id),
                    gcuh.GiftCard.GiftCardCouponCode);
                var gcAmountStr = _priceFormatter.FormatPrice(
                    -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, lang);

                var p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;

                var pv = GetPdfCell($" {gcAmountStr}", font);
                pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                pv.Border = Rectangle.NO_BORDER;

                sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                sutTotalTable.AddCell(p);
                sutTotalTable.AddCell(pv);

            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                var rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", lang.Id),
                    -order.RedeemedRewardPointsEntry.Points);
                var rpAmount = _priceFormatter.FormatPrice(
                    -_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, lang);

                var p = GetPdfCell($"{rpTitle}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;

                var pv = GetPdfCell($"  {rpAmount}", font);
                pv.HorizontalAlignment = Element.ALIGN_RIGHT;
                pv.Border = Rectangle.NO_BORDER;

                sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
                sutTotalTable.AddCell(p);
                sutTotalTable.AddCell(pv);

            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);

            var pTotal = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id)}", titleFont);
            pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            pTotal.Border = Rectangle.NO_BORDER;
            pTotal.BackgroundColor = color;

            var pvt = GetPdfCell($"{orderTotalStr}", font);
            pvt.HorizontalAlignment = Element.ALIGN_RIGHT;
            pvt.Border = Rectangle.NO_BORDER;
            pvt.BackgroundColor = color;

            sutTotalTable.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER });
            sutTotalTable.AddCell(pTotal);
            sutTotalTable.AddCell(pvt);

            totalsTable.AddCell(sutTotalTable);
            doc.Add(totalsTable);
        }

        /// <summary>
        /// Print checkout attributes
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="order">Order</param>
        /// <param name="doc">Document</param>
        /// <param name="lang">Language</param>
        /// <param name="font">Font</param>
        protected virtual void PrintCheckoutAttributes(int vendorId, Order order, Document doc, Language lang, Font font)
        {
            //vendors cannot see checkout attributes
            if (vendorId != 0 || string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                return;

            doc.Add(new Paragraph(" "));
            var attribTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var cCheckoutAttributes = GetPdfCell(HtmlHelper.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true, true), font);
            cCheckoutAttributes.Border = Rectangle.NO_BORDER;
            cCheckoutAttributes.HorizontalAlignment = Element.ALIGN_RIGHT;
            attribTable.AddCell(cCheckoutAttributes);
            doc.Add(attribTable);
        }


        /// <summary>
        /// Print Terms
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font</param>
        protected virtual void PrintOrderInfo(Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont, Company company)
        {
            var productsTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            productsTable.SetWidths(new[] { 50, 50 });

            //product name
            var cellProductItem = GetPdfCell("PDFInvoice.Terms", lang, font);
            cellProductItem.BackgroundColor = color;
            productsTable.AddCell(cellProductItem);

            //price
            cellProductItem = GetPdfCell("PDFInvoice.Po", lang, font);
            cellProductItem.BackgroundColor = color;
            productsTable.AddCell(cellProductItem);

            productsTable.AddCell(new Phrase(company.Terms, font));
            productsTable.AddCell(new Phrase(order.PO, font));

            doc.Add(productsTable);

            doc.Add(new Phrase(Environment.NewLine, new Font { Size = 2f }));
            Chunk line = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, color, Element.ALIGN_CENTER, 2f));
            doc.Add(line);
        }


        /// <summary>
        /// Print products
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="attributesFont">Product attributes font</param>
        protected virtual void PrintProducts(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            //var cellProducts = GetPdfCell("PDFInvoice.Product(s)", lang, titleFont);
            ////cellProducts.Border = Rectangle.NO_BORDER;
            ////productsHeader.AddCell(cellProducts);
            //doc.Add(productsHeader);
            //doc.Add(new Paragraph(" "));

            var orderItems = order.OrderItems;

            var count = 4 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { 4, new[] {  10, 50,20, 20 } },
                { 5, new[] { 45, 15, 15, 10, 15 } },
                { 6, new[] { 40, 13, 13, 12, 10, 12 } }
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            //qty
            var cellProductItem = GetPdfCell("PDFInvoice.ProductQuantity", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            //cellProductItem.PaddingTop = 5;
            cellProductItem.PaddingBottom = 5;
            productsTable.AddCell(cellProductItem);

            //product name
            cellProductItem = GetPdfCell("PDFInvoice.ProductName", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            //productsTable.DefaultCell.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
            cellProductItem.PaddingBottom = 5;
            productsTable.AddCell(cellProductItem);


           

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = GetPdfCell("PDFInvoice.SKU", lang, font);
                cellProductItem.BackgroundColor = color;
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = GetPdfCell("PDFInvoice.VendorName", lang, font);
                cellProductItem.BackgroundColor = color;
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }

            //price
            cellProductItem = GetPdfCell("PDFInvoice.ProductPrice", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            cellProductItem.PaddingBottom = 5;
            productsTable.AddCell(cellProductItem);


            //total
            cellProductItem = GetPdfCell("PDFInvoice.ProductTotal", lang, font);
            cellProductItem.BackgroundColor = color;
            cellProductItem.Border = Rectangle.NO_BORDER;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorsByIds(orderItems.Select(item => item.Product.VendorId).ToArray()) : new List<Vendor>();
            int i = 1;
            foreach (var orderItem in orderItems)
            {
                var p = orderItem.Product;

                //a vendor should have access only to his products
                if (vendorId > 0 && p.VendorId != vendorId)
                    continue;

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                if (i % 2 == 0)
                    cellProductItem.BackgroundColor = color;
                cellProductItem.Border = Rectangle.NO_BORDER;
                cellProductItem.PaddingTop = 5;
                cellProductItem.PaddingBottom = 5;
                productsTable.AddCell(cellProductItem);

                //var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };

                ////product name
                //var name = _localizationService.GetLocalized(p, x => x.Name, lang.Id);

                //var cellName = new PdfPCell(new Paragraph(orderItem.Product.Name, font));
                //if (i % 2 == 0)
                //    cellName.BackgroundColor = color;
                //cellName.Border = Rectangle.NO_BORDER;
                //productsTable.AddCell(cellName);
                //cellProductItem.AddElement(new Paragraph(name, font));

                cellProductItem = GetPdfCell(p.Name, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                if (i % 2 == 0)
                    cellProductItem.BackgroundColor = color;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);

                ////attributes
                //if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                //{
                //    var attributesParagraph =
                //        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                //            attributesFont);
                //    var cellattribute = new PdfPCell(attributesParagraph);
                //    if (i % 2 == 0)
                //        cellattribute.BackgroundColor = color;
                //    cellattribute.Border = Rectangle.NO_BORDER;
                //    pAttribTable.AddCell(cellattribute);
                //}

                ////rental info
                //if (orderItem.Product.IsRental)
                //{
                //    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                //        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalStartDateUtc.Value)
                //        : string.Empty;
                //    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                //        ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalEndDateUtc.Value)
                //        : string.Empty;
                //    var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                //        rentalStartDate, rentalEndDate);

                //    var rentalInfoParagraph = new PdfPCell(new Paragraph(rentalInfo, attributesFont));
                //    if (i % 2 == 0)
                //        rentalInfoParagraph.BackgroundColor = color;
                //    rentalInfoParagraph.Border = Rectangle.NO_BORDER;
                //    pAttribTable.AddCell(rentalInfoParagraph);
                //}


                //var cellTable = new PdfPCell(pAttribTable);
                //if (i % 2 == 0)
                //    cellTable.BackgroundColor = color;
                //cellTable.Border = Rectangle.NO_BORDER;
                //productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = _productService.FormatSku(p, orderItem.AttributesXml);
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    if (i % 2 == 0)
                        cellProductItem.BackgroundColor = color;
                    cellProductItem.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cellProductItem);
                }

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == p.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    if (i % 2 == 0)
                        cellProductItem.BackgroundColor = color;
                    cellProductItem.Border = Rectangle.NO_BORDER;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang, false);
                }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                if (i % 2 == 0)
                    cellProductItem.BackgroundColor = color;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);


                //total
                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang, false);
                }

                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                if (i % 2 == 0)
                    cellProductItem.BackgroundColor = color;
                cellProductItem.Border = Rectangle.NO_BORDER;
                productsTable.AddCell(cellProductItem);
                i++;
            }

            doc.Add(productsTable);
        }

        /// <summary>
        /// Print addresses
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="doc">Document</param>
        protected virtual void PrintAddresses(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            var addressTable = new PdfPTable(2) { RunDirection = GetDirection(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;
            addressTable.SetWidths(new[] { 50, 50 });

            //billing info
            PrintBillingInfo(vendorId, lang, titleFont, order, font, addressTable);

            //shipping info
            PrintShippingInfo(lang, order, titleFont, font, addressTable);

            addressTable.AddCell("");

            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }

        /// <summary>
        /// Print shipping info
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">PDF table for address</param>
        protected virtual void PrintShippingInfo(Language lang, Order order, Font titleFont, Font font, PdfPTable addressTable)
        {
            var shippingAddress = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
            };
            
            shippingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
            shippingAddress.DefaultCell.DisableBorderSide(1);
            shippingAddress.DefaultCell.DisableBorderSide(2);
            shippingAddress.DefaultCell.DisableBorderSide(3);


            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                //cell = new PdfPCell();
                //cell.Border = Rectangle.NO_BORDER;
                const string indent = "";

                

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddress == null)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    PdfPCell cellHead = new PdfPCell(new Phrase(GetParagraph("PDFInvoice.ShippingInformation", lang, titleFont)));
                    cellHead.Border = Rectangle.NO_BORDER;
                    shippingAddress.AddCell(cellHead);

                    var cellShipping = new PdfPCell(GetParagraph("PDFInvoice.ShippingMethod", indent, lang, font, order.ShippingMethod));
                    cellShipping.DisableBorderSide(1);
                    //if (order.PickupInStore && order.PickupAddress == null)

                    cellShipping.Border = Rectangle.NO_BORDER;
                    shippingAddress.AddCell(cellShipping);

                    shippingAddress.AddCell(GetParagraph("PDFInvoice.Name", indent, lang, font, order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName));

                    if (!string.IsNullOrEmpty(order.ShippingAddress.Company))
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Company", indent, lang, font, order.ShippingAddress.Company));
                    if (_addressSettings.PhoneEnabled)
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Phone", indent, lang, font, order.ShippingAddress.PhoneNumber));
                    if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(order.ShippingAddress.FaxNumber))
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Fax", indent, lang, font, order.ShippingAddress.FaxNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Address", indent, lang, font, order.ShippingAddress.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.ShippingAddress.Address2))
                        shippingAddress.AddCell(GetParagraph("PDFInvoice.Address2", indent, lang, font, order.ShippingAddress.Address2));
                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{indent}{order.ShippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(order.ShippingAddress.County) ? $"{order.ShippingAddress.County}, " : string.Empty)}" +
                            $"{(order.ShippingAddress.StateProvince != null ? _localizationService.GetLocalized(order.ShippingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{order.ShippingAddress.ZipPostalCode}";
                        shippingAddress.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                        shippingAddress.AddCell(
                            new Paragraph(indent + _localizationService.GetLocalized(order.ShippingAddress.Country, x => x.Name, lang.Id), font));
                    //custom attributes
                    var customShippingAddressAttributes =
                        _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        //TODO: we should add padding to each line (in case if we have several custom address attributes)
                        shippingAddress.AddCell(new Paragraph(
                            indent + HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }

                }
                else if (order.PickupAddress != null)
                {
                    //shippingAddress.AddCell(GetParagraph("Pickup Point: Shipping Method: Pickup", lang, titleFont));
                    //addressTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    var cellShipping = new PdfPCell(GetParagraph("PDFInvoice.ShippingMethodPickup", indent, lang, font,""));
                    cellShipping.DisableBorderSide(1);
                    //if (order.PickupInStore && order.PickupAddress == null)

                    cellShipping.Border = Rectangle.NO_BORDER;
                    shippingAddress.AddCell(cellShipping);

                    if (!string.IsNullOrEmpty(order.PickupAddress.Address1))
                        shippingAddress.AddCell(new Paragraph($"{string.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.PickupAddress.Address1)}", font));


                    if (!string.IsNullOrEmpty(order.PickupAddress.City) ||   !string.IsNullOrEmpty(order.PickupAddress.ZipPostalCode))
                    {
                        var addressLine = $"{indent}{order.PickupAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(order.PickupAddress.County) ? $"{order.PickupAddress.County}, " : string.Empty)}" +
                            $"{(order.PickupAddress.StateProvince != null  ? _localizationService.GetLocalized(order.PickupAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{order.PickupAddress.ZipPostalCode}";

                        shippingAddress.AddCell(new Paragraph(addressLine, font));
                    }

                    if (order.PickupAddress.Country != null)
                        shippingAddress.AddCell(
                            new Paragraph(indent + _localizationService.GetLocalized(order.PickupAddress.Country, x => x.Name, lang.Id), font));
                  
                }
               
                PdfPCell cellFooter = new PdfPCell();
                cellFooter.Border = Rectangle.NO_BORDER;
                shippingAddress.AddCell(cellFooter);
               
                addressTable.AddCell(shippingAddress);
            }
            else
            {
                shippingAddress.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddress);
            }
        }

        /// <summary>
        /// Print billing info
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">Address PDF table</param>
        protected virtual void PrintBillingInfo(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        {
            const string indent = "";
            var billingAddress = new PdfPTable(1) { RunDirection = GetDirection(lang) };
           
            billingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
            billingAddress.DefaultCell.DisableBorderSide(1);
            billingAddress.DefaultCell.DisableBorderSide(2);
            billingAddress.DefaultCell.DisableBorderSide(3);

            PdfPCell cellHead = new PdfPCell(new Phrase(GetParagraph("PDFInvoice.BillingInformation", lang, titleFont))) ;
            cellHead.Border = Rectangle.NO_BORDER;
            billingAddress.AddCell(cellHead);

            billingAddress.AddCell(GetParagraph("PDFInvoice.Name", indent, lang, font, order.BillingAddress.FirstName + " " + order.BillingAddress.LastName));

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(order.BillingAddress.Company))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Company", indent, lang, font, order.BillingAddress.Company));

            if (_addressSettings.PhoneEnabled && !string.IsNullOrEmpty(order.BillingAddress.PhoneNumber) && order.BillingAddress.PhoneNumber!="0")
                billingAddress.AddCell(GetParagraph("PDFInvoice.Phone", indent, lang, font, order.BillingAddress.PhoneNumber));
            if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(order.BillingAddress.FaxNumber))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Fax", indent, lang, font, order.BillingAddress.FaxNumber));
            if (_addressSettings.StreetAddressEnabled)
                billingAddress.AddCell(GetParagraph("PDFInvoice.Address", indent, lang, font, order.BillingAddress.Address1));
            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.BillingAddress.Address2))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Address2", indent, lang, font, order.BillingAddress.Address2));
            if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
            {
                var addressLine = $"{indent}{order.BillingAddress.City}, " +
                    $"{(!string.IsNullOrEmpty(order.BillingAddress.County) ? $"{order.BillingAddress.County}, " : string.Empty)}" +
                    $"{(order.BillingAddress.StateProvince != null ? _localizationService.GetLocalized(order.BillingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                    $"{order.BillingAddress.ZipPostalCode}";
                billingAddress.AddCell(new Paragraph(addressLine, font));
            }

            //if (_addressSettings.CountryEnabled && order.BillingAddress.Country != null)
            //    billingAddress.AddCell(new Paragraph(indent + _localizationService.GetLocalized(order.BillingAddress.Country, x => x.Name, lang.Id),
            //        font));

            //VAT number
            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddress.AddCell(GetParagraph("PDFInvoice.VATNumber", indent, lang, font, order.VatNumber));

            //custom attributes
            var customBillingAddressAttributes =
                _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            {
                //TODO: we should add padding to each line (in case if we have several custom address attributes)
                billingAddress.AddCell(
                    new Paragraph(indent + HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true), font));
            }

            //Vendors payment details
            if (vendorId == 0)
            {
                //payment method
                var paymentMethod = _paymentPluginManager.LoadPluginBySystemName(order.PaymentMethodSystemName);
                var paymentMethodStr = paymentMethod != null
                    ? _localizationService.GetLocalizedFriendlyName(paymentMethod, lang.Id)
                    : order.PaymentMethodSystemName;

                if (!string.IsNullOrEmpty(paymentMethodStr))
                {
                    billingAddress.AddCell(new Paragraph(" "));
                    billingAddress.AddCell(GetParagraph("PDFInvoice.PaymentMethod", indent, lang, font, paymentMethodStr));
                    billingAddress.AddCell(new Paragraph());
                }

                //custom values
                var customValues = _paymentService.DeserializeCustomValues(order);
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        billingAddress.AddCell(new Paragraph(" "));
                        billingAddress.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, font));
                        billingAddress.AddCell(new Paragraph());
                    }
                }
            }

            PdfPCell cellFooter = new PdfPCell();
            cellFooter.Border = Rectangle.NO_BORDER;
            billingAddress.AddCell(cellFooter);
            addressTable.AddCell(billingAddress);
        }

        /// <summary>
        /// Print header
        /// </summary>
        /// <param name="pdfSettingsByStore">PDF settings</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">Document</param>
        protected virtual void PrintHeader(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Font titleFontLeft, Document doc)
        {
            //logo
            var logoPicture = _pictureService.GetPictureById(pdfSettingsByStore.LogoPictureId);
          
            var logoExists = true;
            //header
            var headerTable = new PdfPTable(logoExists ? 2 : 1)
            {
                RunDirection = GetDirection(lang)
            };

            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.6f, 0.4f });
            headerTable.WidthPercentage = 100f;

            if (logoExists)
            {
                var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                if(!string.IsNullOrEmpty(logoFilePath))
                {
                    var logo = Image.GetInstance(logoFilePath);
                    logo.Alignment = Element.ALIGN_LEFT; //GetAlignment(lang, true);
                    logo.ScaleToFit(200f, 200f);

                    var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
                    cellLogo.AddElement(logo);
                    headerTable.AddCell(cellLogo);
                }
            }
           //headerTable.AddCell("1");

            //store info
            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
            var anchor = new Anchor(store.Url.Trim('/'), font)
            {
                Reference = store.Url
            };

            var cellHeader = GetPdfCell(string.Format(_localizationService.GetResource("", lang.Id), order.Id), titleFont);

            var fontLeftsub = GetFont();
            fontLeftsub.SetStyle(Font.NORMAL);
            fontLeftsub.Size = 14f;

            var orderID = string.Format(_localizationService.GetResource("", lang.Id), order.Id);
            cellHeader.Phrase.Add(GetParagraph(orderID, lang, font));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));


            if (!string.IsNullOrEmpty(order.tranId))
            {
                if (order.tranId != order.Id.ToString())
                {
                    cellHeader.PaddingTop = 20;
                    cellHeader.Phrase.Add(GetParagraph("PDFInvoice.NSNum#", lang, fontLeftsub, order.tranId));
                    cellHeader.PaddingBottom = 20;
                    cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                }
            }

            var invoiceNum = "";
            var invoice = _invoiceService.GetInvoicesByCustomerOrderId(order.CustomerId, order.Id);

            if (invoice == null)
                invoice = _invoiceService.GetInvoicesByCustomerOrderId(Convert.ToInt32(order.CompanyId), order.Id);

            if (invoice == null)
            {
                if (order.CompanyId != null && order.CompanyId != 6633)
                {
                    var CompanyId = _companyService.GetCompanyById(Convert.ToInt32(order.CompanyId));
                    invoice = _invoiceService.GetInvoicesByCustomerOrderId(Convert.ToInt32(CompanyId.NetsuiteId), order.Id);
                }
            }

            if (invoice != null)
                invoiceNum = invoice.InvoiceNo;

            //cellHeader.Phrase.Add(new Phrase(anchor));
            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            if (!string.IsNullOrEmpty(invoiceNum))
            {
                cellHeader.PaddingTop = 20;
                cellHeader.Phrase.Add(GetParagraph("PDFInvoice.OrderNp", lang, fontLeftsub, invoiceNum));
                cellHeader.PaddingBottom = 20;
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            }
            cellHeader.Phrase.Add(GetParagraph("PDFInvoice.OrderDate", lang, fontLeftsub, _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("MM/dd/yyyy", new CultureInfo(lang.LanguageCulture))));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));


            cellHeader.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);
            doc.Add(headerTable);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print an order to PDF
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
        /// <returns>A path of generated file</returns>
        public virtual string PrintOrderToPdf(Order order, int languageId = 0, int vendorId = 0)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var fileName = $"order_{order.OrderGuid}_{CommonHelper.GenerateRandomDigitCode(4)}.pdf";
            var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var orders = new List<Order> { order };
                PrintOrdersToPdf(fileStream, orders, languageId, vendorId);
            }

            return filePath;
        }

        /// <summary>
        /// Print orders to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
        public virtual void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));



            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;

            //fonts header left
            var titleFontLeft = GetFont();
            titleFontLeft.SetStyle(Font.NORMAL);
            titleFontLeft.Size = 20f;
            titleFontLeft.Color = BaseColor.Black;


            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var ordCount = orders.Count;
            var ordNum = 0;

            foreach (var order in orders)
            {
                //by default _pdfSettings contains settings for the current active store
                //and we need PdfSettings for the store which was used to place an order
                //so let's load it based on a store of the current order
                var pdfSettingsByStore = _settingService.LoadSetting<PdfSettings>(order.StoreId);

                var lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = _workContext.WorkingLanguage;

                //header
                PrintHeader(pdfSettingsByStore, lang, order, font, titleFont, titleFontLeft, doc);

                //addresses
                PrintAddresses(vendorId, lang, titleFont, order, font, doc);


                if (order.CompanyId.HasValue)
                {
                    var company = _companyService.GetCompanyById((int)order.CompanyId);
                    if (company != null)
                        //Terms
                        PrintOrderInfo(lang, titleFont, doc, order, font, attributesFont, company);
                }


                //products
                PrintProducts(vendorId, lang, titleFont, doc, order, font, attributesFont);

                //checkout attributes
                PrintCheckoutAttributes(vendorId, order, doc, lang, font);

                //totals
                PrintTotals(vendorId, lang, order, font, titleFont, doc);

                //order notes
                PrintOrderNotes(pdfSettingsByStore, order, lang, titleFont, doc, font);

                //order NN Box Generator
                //PrintOrderNNBoxGenerator(pdfSettingsByStore, order, lang, titleFont, doc, font);

                //footer
                PrintFooter(pdfSettingsByStore, pdfWriter, pageSize, lang, font);

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        public virtual void PrintPackagingSlipsToPdf(Stream stream, IList<Shipment> shipments, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipments == null)
                throw new ArgumentNullException(nameof(shipments));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var shipmentCount = shipments.Count;
            var shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                var order = shipment.Order;

                var lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = _workContext.WorkingLanguage;

                var addressTable = new PdfPTable(1);
                if (lang.Rtl)
                    addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;

                addressTable.AddCell(GetParagraph("PDFPackagingSlip.Shipment", lang, titleFont, shipment.Id));
                addressTable.AddCell(GetParagraph("PDFPackagingSlip.Order", lang, titleFont, order.CustomOrderNumber));

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddress == null)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(order.ShippingAddress.Company))
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Company", lang, font, order.ShippingAddress.Company));

                    addressTable.AddCell(GetParagraph("PDFPackagingSlip.Name", lang, font, order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Phone", lang, font, order.ShippingAddress.PhoneNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Address", lang, font, order.ShippingAddress.Address1));

                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.ShippingAddress.Address2))
                        addressTable.AddCell(GetParagraph("PDFPackagingSlip.Address2", lang, font, order.ShippingAddress.Address2));

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{order.ShippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(order.ShippingAddress.County) ? $"{order.ShippingAddress.County}, " : string.Empty)}" +
                            $"{(order.ShippingAddress.StateProvince != null ? _localizationService.GetLocalized(order.ShippingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{order.ShippingAddress.ZipPostalCode}";
                        addressTable.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                        addressTable.AddCell(new Paragraph(_localizationService.GetLocalized(order.ShippingAddress.Country, x => x.Name, lang.Id), font));

                    //custom attributes
                    var customShippingAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        addressTable.AddCell(new Paragraph(HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                }
                else
                    if (order.PickupAddress != null)
                {
                    addressTable.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Pickup", lang.Id), titleFont));

                    addressTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    if (!string.IsNullOrEmpty(order.PickupAddress.Address1))
                        addressTable.AddCell(new Paragraph($"   {string.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.PickupAddress.Address1)}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.City))
                        addressTable.AddCell(new Paragraph($"   {order.PickupAddress.City}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.County))
                        addressTable.AddCell(new Paragraph($"   {order.PickupAddress.County}", font));
                    if (order.PickupAddress.Country != null)
                        addressTable.AddCell(new Paragraph($"   {_localizationService.GetLocalized(order.PickupAddress.Country, x => x.Name, lang.Id)}", font));
                    if (!string.IsNullOrEmpty(order.PickupAddress.ZipPostalCode))
                        addressTable.AddCell(new Paragraph($"   {order.PickupAddress.ZipPostalCode}", font));
                    addressTable.AddCell(new Paragraph(" "));
                }

                addressTable.AddCell(new Paragraph(" "));

                addressTable.AddCell(GetParagraph("PDFPackagingSlip.ShippingMethod", lang, font, order.ShippingMethod));
                addressTable.AddCell(new Paragraph(" "));
                doc.Add(addressTable);

                var productsTable = new PdfPTable(3) { WidthPercentage = 100f };
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] { 20, 20, 60 });
                }
                else
                {
                    productsTable.SetWidths(new[] { 60, 20, 20 });
                }

                //product name
                var cell = GetPdfCell("PDFPackagingSlip.ProductName", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //SKU
                cell = GetPdfCell("PDFPackagingSlip.SKU", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = GetPdfCell("PDFPackagingSlip.QTY", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                foreach (var si in shipment.ShipmentItems)
                {
                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    //product name
                    var orderItem = _orderService.GetOrderItemById(si.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var p = orderItem.Product;
                    var name = _localizationService.GetLocalized(p, x => x.Name, lang.Id);
                    productAttribTable.AddCell(new Paragraph(name, font));
                  
                    //attributes
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }

                    //rental info
                    if (orderItem.Product.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                            ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                            ? _productService.FormatRentalDate(orderItem.Product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                        var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);

                        var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                        productAttribTable.AddCell(rentalInfoParagraph);
                    }

                    productsTable.AddCell(productAttribTable);

                    //SKU
                    var sku = _productService.FormatSku(p, orderItem.AttributesXml);
                    cell = GetPdfCell(sku ?? string.Empty, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = GetPdfCell(si.Quantity, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                doc.Add(productsTable);

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        /// <summary>
        /// Print products to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        public virtual void PrintProductsToPdf(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var lang = _workContext.WorkingLanguage;

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();

            var productNumber = 1;
            var prodCount = products.Count;

            foreach (var product in products)
            {
                var productName = _localizationService.GetLocalized(product, x => x.Name, lang.Id);
                var productDescription = _localizationService.GetLocalized(product, x => x.FullDescription, lang.Id);

                var productTable = new PdfPTable(1) { WidthPercentage = 100f };
                productTable.DefaultCell.Border = Rectangle.NO_BORDER;
                if (lang.Rtl)
                {
                    productTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                }

                productTable.AddCell(new Paragraph($"{productNumber}. {productName}", titleFont));
                productTable.AddCell(new Paragraph(" "));
                productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(productDescription, decode: true)), font));
                productTable.AddCell(new Paragraph(" "));

                if (product.ProductType == ProductType.SimpleProduct)
                {
                    //simple product
                    //render its properties such as price, weight, etc
                    var priceStr = $"{product.Price:0.00} {_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode}";
                    if (product.IsRental)
                        priceStr = _priceFormatter.FormatRentalProductPeriod(product, priceStr);
                    productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Price", lang.Id)}: {priceStr}", font));
                    productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.SKU", lang.Id)}: {product.Sku}", font));

                    if (product.IsShipEnabled && product.Weight > decimal.Zero)
                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Weight", lang.Id)}: {product.Weight:0.00} {_measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name}", font));

                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.StockQuantity", lang.Id)}: {_productService.GetTotalStockQuantity(product)}", font));

                    productTable.AddCell(new Paragraph(" "));
                }

                var pictures = _pictureService.GetPicturesByProductId(product.Id);
                if (pictures.Any())
                {
                    var table = new PdfPTable(2) { WidthPercentage = 100f };
                    if (lang.Rtl)
                    {
                        table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    }

                    foreach (var pic in pictures)
                    {
                        var picBinary = _pictureService.LoadPictureBinary(pic);
                        if (picBinary == null || picBinary.Length <= 0)
                            continue;

                        var pictureLocalPath = _pictureService.GetThumbLocalPath(pic, 200, false);
                        var cell = new PdfPCell(Image.GetInstance(pictureLocalPath))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    if (pictures.Count % 2 > 0)
                    {
                        var cell = new PdfPCell(new Phrase(" "))
                        {
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    productTable.AddCell(table);
                    productTable.AddCell(new Paragraph(" "));
                }

                if (product.ProductType == ProductType.GroupedProduct)
                {
                    //grouped product. render its associated products
                    var pvNum = 1;
                    foreach (var associatedProduct in _productService.GetAssociatedProducts(product.Id, showHidden: true))
                    {
                        productTable.AddCell(new Paragraph($"{productNumber}-{pvNum}. {_localizationService.GetLocalized(associatedProduct, x => x.Name, lang.Id)}", font));
                        productTable.AddCell(new Paragraph(" "));

                        //uncomment to render associated product description
                        //string apDescription = associated_localizationService.GetLocalized(product, x => x.ShortDescription, lang.Id);
                        //if (!string.IsNullOrEmpty(apDescription))
                        //{
                        //    productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(apDescription)), font));
                        //    productTable.AddCell(new Paragraph(" "));
                        //}

                        //uncomment to render associated product picture
                        //var apPicture = _pictureService.GetPicturesByProductId(associatedProduct.Id).FirstOrDefault();
                        //if (apPicture != null)
                        //{
                        //    var picBinary = _pictureService.LoadPictureBinary(apPicture);
                        //    if (picBinary != null && picBinary.Length > 0)
                        //    {
                        //        var pictureLocalPath = _pictureService.GetThumbLocalPath(apPicture, 200, false);
                        //        productTable.AddCell(Image.GetInstance(pictureLocalPath));
                        //    }
                        //}

                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Price", lang.Id)}: {associatedProduct.Price:0.00} {_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode}", font));
                        productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.SKU", lang.Id)}: {associatedProduct.Sku}", font));

                        if (associatedProduct.IsShipEnabled && associatedProduct.Weight > decimal.Zero)
                            productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.Weight", lang.Id)}: {associatedProduct.Weight:0.00} {_measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name}", font));

                        if (associatedProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                            productTable.AddCell(new Paragraph($"{_localizationService.GetResource("PDFProductCatalog.StockQuantity", lang.Id)}: {_productService.GetTotalStockQuantity(associatedProduct)}", font));

                        productTable.AddCell(new Paragraph(" "));

                        pvNum++;
                    }
                }

                doc.Add(productTable);

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #endregion
    }
}