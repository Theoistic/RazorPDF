using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Theoistic.PDFWebTest.Views.PDFViews
{
    public class NicePDFModel
    {
        
        public List<Item> Items { get; set; }

        public class Item
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

    }
}
