using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Products.Any()) return;

            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Noir Élégance",
                    Description = "Богат и завладяващ аромат с нотки на черна роза, амбра и сандалово дърво. Перфектен за вечерни поводи.",
                    Price = 189.99m,
                    ImageUrl = "/images/Gemini_Generated_Image_62xyaw62xyaw62xy.jpg",
                    Brand = "Maison Gerchev",
                    Size = "100ml",
                    Concentration = "EDP",
                    StockQuantity = 15
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Rose Mystique",
                    Description = "Интензивен цветен парфюм с дамасска роза, бергамот и мускус. Изискан и неустоим.",
                    Price = 249.99m,
                    ImageUrl = "/images/Gemini_Generated_Image_izenq4izenq4izen.jpg",
                    Brand = "Maison Gerchev",
                    Size = "50ml",
                    Concentration = "Parfum",
                    StockQuantity = 10
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Oud Prestige",
                    Description = "Ориенталски шедьовър с уд, кехлибар и тамян. Дълготраен и незабравим.",
                    Price = 299.99m,
                    ImageUrl = "/images/Gemini_Generated_Image_pmsttypmsttypmst.jpg",
                    Brand = "Maison Gerchev",
                    Size = "100ml",
                    Concentration = "EDP",
                    StockQuantity = 8
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Lumière Dorée",
                    Description = "Свеж и лъчист аромат с жасмин, мандарина и ванилия. Идеален за ежедневие.",
                    Price = 159.99m,
                    ImageUrl = "/images/images (1).jpg",
                    Brand = "Maison Gerchev",
                    Size = "75ml",
                    Concentration = "EDT",
                    StockQuantity = 20
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Velvet Amber",
                    Description = "Топъл и чувствен аромат с кехлибар, пачули и карамел. Обгръща с нежност.",
                    Price = 219.99m,
                    ImageUrl = "/images/images.jpg",
                    Brand = "Maison Gerchev",
                    Size = "50ml",
                    Concentration = "EDP",
                    StockQuantity = 12
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
