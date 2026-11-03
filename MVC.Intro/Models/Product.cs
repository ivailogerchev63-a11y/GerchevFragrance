namespace MVC.Intro.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public string? Size { get; set; }
        public string? Concentration { get; set; }
        public int StockQuantity { get; set; }
    }
}
