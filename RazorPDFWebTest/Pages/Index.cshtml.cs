﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPDF;
using RazorPDFWebTest.PDFViews;
using RazorPDFWebTest.Views.PDFViews;

namespace RazorPDFWebTest.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
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
    }
}