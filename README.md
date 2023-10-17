# RazorPDF

RazorPDF is a powerful library for .NET that provides capabilities to build PDF documents using Razor views. With RazorPDF, you can create complex PDF documents, render HTML content, and apply styles, all with the familiar Razor syntax and .NET environment. It's perfect for generating invoices, reports, forms, and more!

## Features
- Generate PDFs using familiar Razor syntax and views.
- Inject CSS for styling your PDFs.
- Comprehensive PDF settings like compression, size, orientation, and more.
- Asynchronous methods for building PDFs.

## Installation

You can add RazorPDF to your project via the NuGet package manager. Use the following command in your Package Manager Console:

```
Install-Package RazorPDF
```

## Configuration

To use RazorPDF in your project, you need to configure the services and application builder typically in your `Startup.cs`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPDF();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRazorPDF();
}
```

## Quick Start

Here's a quick demonstration of how you can generate a PDF from a Razor view.

First, create your Razor view (e.g., PDFViews/NicePDF.cshtml). This view will contain the HTML structure and content that will be rendered in your PDF.

```html
@model NicePDFModel

<!DOCTYPE html>
<html>
<head>
    <link rel="stylesheet" type="text/css" href="~/PDFStyle.css">
</head>
<body>
    <h1>My Nice PDF</h1>
    <table>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.Name</td>
                <td>@item.Value</td>
            </tr>
        }
    </table>
</body>
</html>
```

In your controller or service, use PDFBuilder to generate the PDF:

```csharp
public async Task CreatePDF()
{
    var pdf = await new PDFBuilder()
        .Settings(x =>
        {
            x.UseCompression = true;
        })
        .InjectCSS("wwwroot/PDFStyle.css")
        .RazorView("PDFViews/NicePDF", new NicePDFModel { 
            Items = new List<NicePDFModel.Item> {
                new NicePDFModel.Item { Name = "Something", Value = "10.42" },
                new NicePDFModel.Item { Name = "Something else", Value = "50.42" },
                new NicePDFModel.Item { Name = "Something more", Value = "21.42" },
            }
        })
        .BuildAsync();

    System.IO.File.WriteAllBytes("test.pdf", pdf);
}
```

This will create a PDF document from your Razor view with the data you provided and save it as test.pdf.
Contributing

Contributions to the RazorPDF library are welcome! If you're interested in improving RazorPDF
