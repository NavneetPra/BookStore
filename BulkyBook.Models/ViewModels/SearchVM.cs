using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Models.ViewModels
{
    public class SearchVM
    {
        public string SearchString { get; set; }
        public IEnumerable<Product> SearchResults { get; set; }
    }
}
