using System.Collections.Generic;

namespace CivilX.Shared.Models
{
    public class UserProductsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ProductInfo> Products { get; set; }
        
        public UserProductsResponse()
        {
            Products = new List<ProductInfo>();
        }
    }
}
