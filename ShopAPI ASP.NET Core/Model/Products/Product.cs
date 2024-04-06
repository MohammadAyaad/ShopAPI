namespace ShopAPI.Model.Products
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public double Rating { get; set; } 
        public long TimesRates { get; set; }

        public ICollection<ProductVariant> ProductVariants { get; set; }
        public ICollection<ProductRating> ProductRatings { get; set; }
    }
}
