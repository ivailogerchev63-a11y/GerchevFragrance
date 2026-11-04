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
                    Name = "Dior Sauvage Elixir",
                    Description = "Изключително концентриран и интензивен аромат, съчетаващ емблематичната свежест на Sauvage с опияняващо сърце от подправки, есенция от Nyons лавандула и богата кехлибарена основа.",
                    Price = 219.99m,
                    OriginalPrice = 249.99m,
                    ImageUrl = "/images/images.jpg",
                    Brand = "Dior",
                    Size = "50ml",
                    Concentration = "Elixir",
                    StockQuantity = 12,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = true,
                    Notes = "Връхни: Канела, Индийско орехче, Кардамон, Грейпфрут | Сърце: Nyons Лавандула | База: Кехлибар, Женско биле, Сандалово дърво"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Tom Ford Ombré Leather",
                    Description = "Изискан и чувствен кожен аромат, вдъхновен от американския запад, съчетаващ нотки на черна кожа, кардамон, жасмин самбак, пачули и бял мъх.",
                    Price = 159.99m,
                    OriginalPrice = 189.99m,
                    ImageUrl = "/images/images (1).jpg",
                    Brand = "Tom Ford",
                    Size = "75ml",
                    Concentration = "EDP",
                    StockQuantity = 20,
                    Category = "Unisex",
                    IsBestseller = true,
                    IsFeatured = false,
                    Notes = "Връхни: Кардамон | Сърце: Черна кожа, Жасмин Самбак | База: Кехлибар, Мъх, Пачули"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Jean Paul Gaultier Le Male",
                    Description = "Легендарният ориенталски фужерен аромат с връхни нотки на мента, лавандула и бергамот, сърце от канела и портокалов цвят и топла основа от ванилия, кедър и сандалово дърво.",
                    Price = 189.99m,
                    OriginalPrice = null,
                    ImageUrl = "/images/Gemini_Generated_Image_62xyaw62xyaw62xy.jpg",
                    Brand = "Jean Paul Gaultier",
                    Size = "100ml",
                    Concentration = "EDT",
                    StockQuantity = 15,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    Notes = "Връхни: Мента, Лавандула, Бергамот | Сърце: Канела, Портокалов цвят | База: Ванилия, Кедър, Сандал"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Yves Saint Laurent Y",
                    Description = "Дървесен и свеж мъжки аромат с искрящи нотки на ябълка, джинджифил и бергамот, елегантно сърце от салвия и лавандула, и основа от ветивер, кедър и боб тонка.",
                    Price = 249.99m,
                    OriginalPrice = null,
                    ImageUrl = "/images/Gemini_Generated_Image_izenq4izenq4izen.jpg",
                    Brand = "Yves Saint Laurent",
                    Size = "50ml",
                    Concentration = "EDP",
                    StockQuantity = 10,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    Notes = "Връхни: Ябълка, Джинджифил, Бергамот | Сърце: Салвия, Хвойна, Лавандула | База: Ветивер, Кедър, Тонка"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Jean Paul Gaultier Le Male Elixir",
                    Description = "Пламтящ и съблазнителен парфюмен еликсир с нотки на лавандула, мента, бензоин, ванилия и зърна тонка в емблематичен златен флакон.",
                    Price = 299.99m,
                    OriginalPrice = 329.99m,
                    ImageUrl = "/images/Gemini_Generated_Image_pmsttypmsttypmst.jpg",
                    Brand = "Jean Paul Gaultier",
                    Size = "100ml",
                    Concentration = "Parfum",
                    StockQuantity = 8,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    Notes = "Връхни: Лавандула, Мента | Сърце: Ванилия, Бензоин | База: Мед, Боб тонка, Тютюн"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Aventus",
                    Description = "Сензационният емблематичен аромат на Creed. Празник на силата, успеха и смелостта с незабравими нотки на бергамот, черен касис, сочен ананас и опушена бреза.",
                    Price = 549.99m,
                    OriginalPrice = 599.99m,
                    ImageUrl = "/images/Gemini_Generated_Image_8993a18993a18993.jpg",
                    Brand = "Creed",
                    Size = "100ml",
                    Concentration = "EDP",
                    StockQuantity = 10,
                    Category = "Luxury Fragrances",
                    IsBestseller = true,
                    IsFeatured = false,
                    Notes = "Връхни: Бергамот, Касис, Ябълка, Ананас | Сърце: Бреза, Пачули, Жасмин | База: Мускус, Дъбов мъх, Амбра"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Millésime Impérial",
                    Description = "Златният стандарт на нишовата парфюмерия. Свеж, морски и цитрусов шедьовър, пренасящ сетивата край слънчевите брегове на Сицилия.",
                    Price = 499.99m,
                    OriginalPrice = null,
                    ImageUrl = "/images/Gemini_Generated_Image_ewjzp6ewjzp6ewjz.jpg",
                    Brand = "Creed",
                    Size = "100ml",
                    Concentration = "EDP",
                    StockQuantity = 8,
                    Category = "Unisex",
                    IsBestseller = false,
                    IsFeatured = false,
                    Notes = "Връхни: Морска сол, Плодови нотки | Сърце: Сицилиански лимон, Бергамот, Ирис | База: Мускус, Дървесни нотки"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Green Irish Tweed",
                    Description = "Вечната елегантност на ирландската природа. Изтънчен аромат, съчетаващ свежестта на полеви цветя с мъжествена дървесна топлина.",
                    Price = 489.99m,
                    OriginalPrice = null,
                    ImageUrl = "/images/Gemini_Generated_Image_utb5m6utb5m6utb5.jpg",
                    Brand = "Creed",
                    Size = "100ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    Notes = "Връхни: Лимонова вербена, Ирис | Сърце: Виолетови листа | База: Сив кехлибар, Сандалово дърво"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Silver Mountain Water",
                    Description = "Кристална свежест, вдъхновена от чистите алпийски потоци. Ободряващ планински полъх със зелен чай, касис и искрящ бергамот.",
                    Price = 489.99m,
                    OriginalPrice = 529.99m,
                    ImageUrl = "/images/Gemini_Generated_Image_xl3ywjxl3ywjxl3y.jpg",
                    Brand = "Creed",
                    Size = "100ml",
                    Concentration = "EDP",
                    StockQuantity = 9,
                    Category = "Unisex",
                    IsBestseller = false,
                    IsFeatured = false,
                    Notes = "Връхни: Бергамот, Мандарина | Сърце: Зелен чай, Черен касис | База: Галбанум, Мускус, Сандалово дърво"
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
