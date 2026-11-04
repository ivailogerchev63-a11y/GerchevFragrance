namespace MVC.Intro.Models
{
    public class HomeViewModel
    {
        public Product? FeaturedFragrance { get; set; }
        public List<Product> Bestsellers { get; set; } = new();
        public List<Product> SpecialOffers { get; set; } = new();
        public List<Product> AllProducts { get; set; } = new();
        public List<CategoryCardModel> Categories { get; set; } = new();
        public string ActivePromoCode { get; set; } = "GERCHEV10";
    }

    public class CategoryCardModel
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}
