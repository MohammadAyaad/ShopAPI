namespace ShopAPI.Model.Products
{
    public class ProductRating
    {
        public Guid Id { get; set; }
        public string RaterEmail { get; }
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public long RatedAt { get; set; }
        public string Comment {  get; set; }
        public double Rating { get; set; }
        public long UpVotes {  get; set; }
        public long DownVotes { get; set; }
        public double Score { get; set; }

        public Product Product { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
