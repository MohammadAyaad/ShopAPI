using Newtonsoft.Json.Linq;

namespace ShopAPI.Model.Products
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public JObject VariantProperties { get; set; }
        public double Price { get; set; }
        public double AvailableQuantity { get; set; }

        public Product Product { get; set; }
    }
}
