using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Essentials;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuestPdf
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public void ButtonClick(object o, RoutedEventArgs e)
        {
            CreatePdf();
        }

        private async void CreatePdf()
        {
            var file =
                await StorageFile
                    .GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Roboto-Regular.ttf", UriKind.Absolute));

            using (var fs = file.OpenStreamForReadAsync().GetAwaiter().GetResult())
            {
                FontManager.RegisterFontType("Roboto", fs);
            }

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var path = Path.Combine(localFolder.Path, "invoice.pdf");

            var model = InvoiceDocumentDataSource.GetInvoiceDetails();
            var document = new InvoiceDocument(model);
            document.GeneratePdf(path);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "PDF",
                File = new ShareFile(path)
            });
        }
    }

    public class InvoiceModel
    {
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        public Address SellerAddress { get; set; }
        public Address CustomerAddress { get; set; }

        public List<OrderItem> Items { get; set; }
        public string Comments { get; set; }
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class Address
    {
        public string CompanyName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public object Email { get; set; }
        public string Phone { get; set; }
    }

    public static class InvoiceDocumentDataSource
    {
        private static Random Random = new Random();

        public static InvoiceModel GetInvoiceDetails()
        {
            var items = Enumerable
                .Range(1, 8)
                .Select(i => GenerateRandomOrderItem())
                .ToList();

            return new InvoiceModel
            {
                InvoiceNumber = Random.Next(1_000, 10_000),
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now + TimeSpan.FromDays(14),

                SellerAddress = GenerateRandomAddress(),
                CustomerAddress = GenerateRandomAddress(),

                Items = items,
                Comments = Placeholders.Paragraph()
            };
        }

        private static OrderItem GenerateRandomOrderItem()
        {
            return new OrderItem
            {
                Name = Placeholders.Label(),
                Price = (decimal)Math.Round(Random.NextDouble() * 100, 2),
                Quantity = Random.Next(1, 10)
            };
        }

        private static Address GenerateRandomAddress()
        {
            return new Address
            {
                CompanyName = Placeholders.Name(),
                Street = Placeholders.Label(),
                City = Placeholders.Label(),
                State = Placeholders.Label(),
                Email = Placeholders.Email(),
                Phone = Placeholders.PhoneNumber()
            };
        }
    }

    public class InvoiceDocument : IDocument
    {
        public InvoiceModel Model { get; }

        public InvoiceDocument(InvoiceModel model)
        {
            Model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber(TextStyle.Default.FontType("Roboto"));
                        x.Span(" / ", TextStyle.Default.FontType("Roboto"));
                        x.TotalPages(TextStyle.Default.FontType("Roboto"));
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.Size(20).SemiBold().Color(Colors.Blue.Medium).FontType("Roboto");

            container.Row(row =>
            {
                row.RelativeColumn().Stack(stack =>
                {
                    stack.Item().Text($"Invoice #{Model.InvoiceNumber}", titleStyle);

                    stack.Item().Text(text =>
                    {
                        text.Span("Issue date: ", TextStyle.Default.SemiBold().FontType("Roboto"));
                        text.Span($"{Model.IssueDate:d}", TextStyle.Default.SemiBold().FontType("Roboto"));
                    });

                    stack.Item().Text(text =>
                    {
                        text.Span("Due date: ", TextStyle.Default.SemiBold().FontType("Roboto"));
                        text.Span($"{Model.DueDate:d}", TextStyle.Default.SemiBold().FontType("Roboto"));
                    });
                });

                row.ConstantColumn(100).Height(50).Placeholder();
            });
        }

        void ComposeContent(IContainer container)
        {
            container
                .PaddingVertical(40)
                .Stack(stack =>
                {
                    stack.Item().Background(Colors.Grey.Lighten3)
                        .AlignCenter()
                        .AlignMiddle()
                        .Height(200)
                        .Text("Content", TextStyle.Default.Size(16).FontType("Roboto"));
                    stack.Item().Background(Colors.Grey.Lighten3)
                        .AlignCenter()
                        .AlignMiddle()
                        .Height(200)
                        .Text("Content", TextStyle.Default.Size(16).FontType("Roboto"));
                    stack.Item().Background(Colors.Grey.Lighten3)
                        .AlignCenter()
                        .AlignMiddle()
                        .Height(200)
                        .Text("Content", TextStyle.Default.Size(16).FontType("Roboto"));
                    stack.Item().Background(Colors.Grey.Lighten3)
                        .AlignCenter()
                        .AlignMiddle()
                        .Height(200)
                        .Text("Content", TextStyle.Default.Size(16).FontType("Roboto"));
                });
        }
    }
}
