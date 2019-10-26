using System.Collections.Generic;

namespace Services.Models
{
    public class QueriesModel
    {
        public List<ProductModel> Products { get; set; }
        public double MaxPrice { get; set; }
        public double MinPrice { get; set; }
        public double AveragePrice { get; set; }
    }
}