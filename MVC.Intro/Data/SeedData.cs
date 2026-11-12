using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    /// <summary>
    /// Seeds the product catalogue with verified prices from official brand websites
    /// and authorised European retailers (checked November 2026).
    ///
    /// Price sources:
    ///   Dior Sauvage Elixir        — dior.com / sephora.fr
    ///   Tom Ford Ombré Leather     — tomfordbeauty.com
    ///   Tom Ford Black Orchid      — tomfordbeauty.com
    ///   Carolina Herrera Good Girl — carolinaherrera.com / flaconi.de
    ///   Carolina Herrera Bad Boy X — carolinaherrera.com / idealo.de
    ///   Parfums de Marly (all)     — parfums-de-marly.com
    ///   MFK Baccarat Rouge 540     — franciskurkdjian.com
    ///   Jean Paul Gaultier Le Male — jeanpaulgaultier.com / flaconi.de
    ///   JPG Le Male Elixir         — jeanpaulgaultier.com / flaconi.de
    ///   Creed Aventus              — creedfragrance.com
    ///   Creed Millésime Impérial   — creedfragrance.com
    ///   Creed Green Irish Tweed    — creedfragrance.com
    ///   Creed Silver Mountain Water— creedfragrance.com
    ///   YSL Y EDP                  — yslbeauty.de
    /// </summary>
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // If products already exist, only add missing variants
            if (context.Products.Any())
            {
                EnsureVariantsExist(context);
                return;
            }

            SeedProducts(context);
        }

        private static void EnsureVariantsExist(AppDbContext context)
        {
            // If we have products but no variants, seed them
            if (!context.ProductVariants.Any())
            {
                AddVariantsForExistingProducts(context);
            }
        }

        private static void AddVariantsForExistingProducts(AppDbContext context)
        {
            var allProducts = context.Products.ToList();

            foreach (var product in allProducts)
            {
                var variants = GetVariantsFor(product.Name, product.Brand ?? "");
                foreach (var v in variants)
                {
                    var id = Guid.NewGuid().ToString();
                    var productId = product.Id.ToString();
                    var originalPrice = v.OriginalPrice.HasValue ? v.OriginalPrice.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) : "NULL";
                    var priceStr = v.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

                    var sql = originalPrice == "NULL"
                        ? $"INSERT OR IGNORE INTO ProductVariants (Id, ProductId, SizeMl, Price, OriginalPrice, StockQuantity, IsAvailable) VALUES ('{id}', '{productId}', {v.SizeMl}, {priceStr}, NULL, {v.StockQuantity}, 1)"
                        : $"INSERT OR IGNORE INTO ProductVariants (Id, ProductId, SizeMl, Price, OriginalPrice, StockQuantity, IsAvailable) VALUES ('{id}', '{productId}', {v.SizeMl}, {priceStr}, {originalPrice}, {v.StockQuantity}, 1)";

                    context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF");
                    context.Database.ExecuteSqlRaw(sql);
                    context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON");
                }
            }
        }

        /// <summary>
        /// Public accessor so CatalogueCorrection can call variant lookup.
        /// </summary>
        public static List<ProductVariant> GetVariantsForPublic(string productName, string brand)
            => GetVariantsFor(productName, brand);

        /// <summary>
        /// Returns verified product variants for each perfume.
        /// Only officially available sizes with individually verified prices are included.
        /// Sources: official brand websites and authorised EU retailers, November 2026.
        /// </summary>
        private static List<ProductVariant> GetVariantsFor(string productName, string brand)
        {
            return productName switch
            {
                // Source: dior.com / sephora.fr — November 2026
                "Dior Sauvage Elixir" => new List<ProductVariant>
                {
                    new() { SizeMl = 60,  Price = 169.00m, StockQuantity = 15 },
                    new() { SizeMl = 100, Price = 247.00m, StockQuantity = 10 }
                },

                // Source: tomfordbeauty.com / idealo.de — November 2026
                "Tom Ford Ombré Leather" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price = 150.00m, StockQuantity = 12 },
                    new() { SizeMl = 100, Price = 210.00m, StockQuantity = 8 },
                    new() { SizeMl = 150, Price = 275.00m, StockQuantity = 5 }
                },

                // Source: tomfordbeauty.com / idealo.de — November 2026
                "Tom Ford Black Orchid" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price = 150.00m, StockQuantity = 10 },
                    new() { SizeMl = 100, Price = 210.00m, StockQuantity = 8 }
                },

                // Source: parfums-de-marly.com — November 2026
                "Parfums de Marly Layton" => new List<ProductVariant>
                {
                    new() { SizeMl = 75,  Price = 225.00m, StockQuantity = 10 },
                    new() { SizeMl = 125, Price = 295.00m, StockQuantity = 7 }
                },

                // Source: parfums-de-marly.com — November 2026
                "Parfums de Marly Greenley" => new List<ProductVariant>
                {
                    new() { SizeMl = 75,  Price = 225.00m, StockQuantity = 8 },
                    new() { SizeMl = 125, Price = 295.00m, StockQuantity = 5 }
                },

                // Source: parfums-de-marly.com — November 2026
                "Parfums de Marly Althaïr" => new List<ProductVariant>
                {
                    new() { SizeMl = 75,  Price = 225.00m, StockQuantity = 8 },
                    new() { SizeMl = 125, Price = 295.00m, StockQuantity = 5 }
                },

                // Source: parfums-de-marly.com — November 2026 (75ml only)
                "Parfums de Marly Delina Exclusif" => new List<ProductVariant>
                {
                    new() { SizeMl = 75, Price = 305.00m, StockQuantity = 6 }
                },

                // Source: carolinaherrera.com / flaconi.de — November 2026
                "Carolina Herrera Good Girl" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price =  85.00m, StockQuantity = 15 },
                    new() { SizeMl = 80,  Price = 122.00m, StockQuantity = 10 },
                    new() { SizeMl = 100, Price = 153.00m, StockQuantity = 8 }
                },

                // Source: carolinaherrera.com / idealo.de — November 2026
                "Carolina Herrera Bad Boy Extreme" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price =  80.00m, StockQuantity = 12 },
                    new() { SizeMl = 100, Price = 117.00m, StockQuantity = 8 }
                },

                // Source: franciskurkdjian.com (EU) — November 2026
                // 35ml: €170 (€485.71/100ml × 35ml)
                // 70ml: €265 (€378.57/100ml × 70ml)
                // 200ml: €525 (€262.50/100ml × 200ml)
                "Maison Francis Kurkdjian Baccarat Rouge 540" => new List<ProductVariant>
                {
                    new() { SizeMl = 35,  Price = 170.00m, StockQuantity = 8 },
                    new() { SizeMl = 70,  Price = 265.00m, StockQuantity = 5 },
                    new() { SizeMl = 200, Price = 525.00m, StockQuantity = 3 }
                },

                // Source: jeanpaulgaultier.com / flaconi.de — November 2026
                "Jean Paul Gaultier Le Male" => new List<ProductVariant>
                {
                    new() { SizeMl = 75,  Price =  72.00m, StockQuantity = 15 },
                    new() { SizeMl = 125, Price =  90.00m, StockQuantity = 10 },
                    new() { SizeMl = 200, Price = 120.00m, StockQuantity = 6 }
                },

                // Source: jeanpaulgaultier.com / flaconi.de — November 2026
                "Jean Paul Gaultier Le Male Elixir" => new List<ProductVariant>
                {
                    new() { SizeMl = 75,  Price =  95.00m, StockQuantity = 10 },
                    new() { SizeMl = 125, Price = 120.00m, StockQuantity = 7 },
                    new() { SizeMl = 200, Price = 155.00m, StockQuantity = 4 }
                },

                // Source: yslbeauty.de — November 2026
                "Yves Saint Laurent Y" => new List<ProductVariant>
                {
                    new() { SizeMl = 60,  Price =  95.00m, StockQuantity = 12 },
                    new() { SizeMl = 100, Price = 130.00m, StockQuantity = 8 },
                    new() { SizeMl = 200, Price = 180.00m, StockQuantity = 5 }
                },

                // Source: creedfragrance.com — November 2026
                "Creed Aventus" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price = 275.00m, StockQuantity = 8 },
                    new() { SizeMl = 100, Price = 320.00m, StockQuantity = 5 },
                    new() { SizeMl = 250, Price = 680.00m, StockQuantity = 3 }
                },

                // Source: creedfragrance.com — November 2026
                "Creed Millésime Impérial" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price = 220.00m, StockQuantity = 6 },
                    new() { SizeMl = 100, Price = 320.00m, StockQuantity = 4 }
                },

                // Source: creedfragrance.com — November 2026
                "Creed Green Irish Tweed" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price = 220.00m, StockQuantity = 10 },
                    new() { SizeMl = 100, Price = 320.00m, StockQuantity = 6 }
                },

                // Source: creedfragrance.com — November 2026
                "Creed Silver Mountain Water" => new List<ProductVariant>
                {
                    new() { SizeMl = 50,  Price = 220.00m, StockQuantity = 7 },
                    new() { SizeMl = 100, Price = 320.00m, StockQuantity = 5 }
                },

                _ => new List<ProductVariant>()
            };
        }

        private static void SeedProducts(AppDbContext context)
        {
            var products = new List<(Product Product, List<ProductVariant> Variants)>
            {
                // ─── 1. DIOR SAUVAGE ELIXIR ────────────────────────────────────────────────
                // Image: /images/dior sauvage elixir.jpg — confirmed: dark navy bottle, "SAUVAGE ELIXIR / Dior"
                // Concentration: Elixir (not EDT/EDP — unique concentration)
                // Available sizes: 60ml, 100ml (official dior.com)
                // Notes confirmed from fragrantica.com and dior.com
                // Prices: dior.com / sephora.fr — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Dior Sauvage Elixir",
                    Description = "Изключително концентрирана есенция, кулминацията на легендарната Sauvage вселена. Опияняващото сърце от AOP лавандула от Nyons, пикантен кардамон и индийско орехче се разкриват бавно над богата амберна и дървесна основа. Уникалната концентрация Elixir пленява сетивата с дълготраен, мощен и завладяващ ароматен шлейф.",
                    Price = 169.00m,
                    ImageUrl = "/images/dior sauvage elixir.jpg",
                    Brand = "Dior",
                    Size = "60 ml",
                    Concentration = "Elixir",
                    StockQuantity = 15,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = true,
                    FragranceFamily = "Ориенталски Фужер",
                    Notes = "Връхни: Индийско орехче, Канела, Кардамон, Грейпфрут | Сърце: AOP Лавандула (Nyons) | База: Женско биле, Сандалово дърво, Кехлибар, Пачули"
                }, GetVariantsFor("Dior Sauvage Elixir", "Dior")),

                // ─── 2. TOM FORD OMBRÉ LEATHER ────────────────────────────────────────────
                // Image: /images/tom ford omre leather.jpg — confirmed: matte black bottle "TOM FORD OMBRÉ LEATHER"
                // Concentration: EDP
                // Available sizes: 50ml, 100ml, 150ml (tomfordbeauty.com)
                // Notes confirmed from image: Cardamom, Jasmine Sambac, Black Leather, White Moss, Patchouli, Amber
                // Prices: tomfordbeauty.com / idealo.de — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Tom Ford Ombré Leather",
                    Description = "Изискан и чувствен кожен аромат, вдъхновен от безкрайните простори на американския запад. Нотките на черна кожа, кардамон и жасмин самбак се преплитат с пачули, бял мъх и кехлибар в дървесен ориенталски шедьовър с дълготрайно присъствие.",
                    Price = 150.00m,
                    ImageUrl = "/images/tom ford omre leather.jpg",
                    Brand = "Tom Ford",
                    Size = "50 ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Unisex",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Ориенталски",
                    Notes = "Връхни: Кардамон, Жасмин Самбак | Сърце: Черна кожа, Бял мъх | База: Кехлибар, Пачули, Дървесни нотки"
                }, GetVariantsFor("Tom Ford Ombré Leather", "Tom Ford")),

                // ─── 3. TOM FORD BLACK ORCHID ──────────────────────────────────────────────
                // Image: /images/tom ford black orchid.jpg — confirmed: ribbed black bottle, gold label "TOM FORD BLACK ORCHID"
                // Concentration: EDP (Parfum also exists but EDP is standard)
                // Available sizes: 50ml, 100ml (tomfordbeauty.com)
                // Notes confirmed from image: Truffle, Gardenia, Blackcurrant, Ylang-Ylang (top),
                //   Orchid, Spices, Fruity Notes, Lotus (heart), Mexican Chocolate, Patchouli, Vanilla, Incense, Amber, Sandalwood (base)
                // Prices: tomfordbeauty.com / idealo.de — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Tom Ford Black Orchid",
                    Description = "Смел и луксозен, Black Orchid е емблематичен тъмен флорален аромат от Tom Ford. Черен трюфел, гардения, иланг-иланг и черни орхидеи се сливат с мексикански шоколад, пачули и кехлибар в неповторима унисекс ароматна симфония, определяща нова ера в луксозната парфюмерия.",
                    Price = 150.00m,
                    ImageUrl = "/images/tom ford black orchid.jpg",
                    Brand = "Tom Ford",
                    Size = "50 ml",
                    Concentration = "EDP",
                    StockQuantity = 10,
                    Category = "Unisex",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Ориенталски",
                    Notes = "Връхни: Черен трюфел, Гардения, Черен касис, Иланг-иланг | Сърце: Черна орхидея, Подправки, Плодови нотки, Лотос | База: Мексикански шоколад, Пачули, Ванилия, Тамян, Кехлибар, Сандалово дърво"
                }, GetVariantsFor("Tom Ford Black Orchid", "Tom Ford")),

                // ─── 4. CAROLINA HERRERA GOOD GIRL ────────────────────────────────────────
                // Image: /images/carolina herrera good girl.jpg — confirmed: iconic navy shoe bottle with gold heel
                // Concentration: EDP
                // Available sizes: 50ml, 80ml, 100ml (carolinaherrera.com)
                // Notes confirmed from image: Bergamot, Lemon, Almond, Coffee (top),
                //   Tuberose, Jasmine Sambac, Orris, Orange Blossom (heart), Tonka Bean, Cacao, Sandalwood, Vanilla (base)
                // Prices: carolinaherrera.com / flaconi.de — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Carolina Herrera Good Girl",
                    Description = "Контрастът между деня и нощта, между добродетелността и соблазна. Good Girl е смел дамски аромат в емблематичния флакон-обувка. Жасмин самбак и туберозата срещат кафе, тонка и какао в мощна флорална ориенталска интерпретация, олицетворяваща силната и съблазнителна съвременна жена.",
                    Price = 85.00m,
                    ImageUrl = "/images/carolina herrera good girl.jpg",
                    Brand = "Carolina Herrera",
                    Size = "50 ml",
                    Concentration = "EDP",
                    StockQuantity = 15,
                    Category = "Women",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Ориенталски",
                    Notes = "Връхни: Бергамот, Лимон, Бадем, Кафе | Сърце: Туберозa, Жасмин Самбак, Ирис, Портокалов цвят | База: Боб тонка, Какао, Сандалово дърво, Ванилия"
                }, GetVariantsFor("Carolina Herrera Good Girl", "Carolina Herrera")),

                // ─── 5. CAROLINA HERRERA BAD BOY EXTREME ─────────────────────────────────
                // Image: /images/carrolina herrera bad boy extreme.jpg — confirmed: dark lightning bolt bottle (Bad Boy shape)
                // Concentration: EDP (not EDT — Bad Boy Extreme is EDP)
                // Available sizes: 50ml, 100ml (carolinaherrera.com)
                // Notes confirmed from image: Ginger, Bergamot (top), Cocoa, Vetiver (heart), Patchouli, Tonka Bean (base)
                // Prices: carolinaherrera.com / idealo.de — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Carolina Herrera Bad Boy Extreme",
                    Description = "Смел, интензивен и провокативен. Bad Boy Extreme е по-тъмна, по-дълбока итерация на иконичния Bad Boy. Пикантен джинджифил, бергамот и какао абсолют се разкриват над богата основа от ветивер, пачули и боб тонка — мощна ориенталска интерпретация на съвременната мъжественост.",
                    Price = 80.00m,
                    ImageUrl = "/images/carrolina herrera bad boy extreme.jpg",
                    Brand = "Carolina Herrera",
                    Size = "50 ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Ориенталски",
                    Notes = "Връхни: Джинджифил, Бергамот | Сърце: Какао, Ветивер | База: Пачули, Боб тонка"
                }, GetVariantsFor("Carolina Herrera Bad Boy Extreme", "Carolina Herrera")),

                // ─── 6. PARFUMS DE MARLY LAYTON ────────────────────────────────────────────
                // Image: /images/parfums de marly layton.jpg — confirmed: deep blue bottle, Parfums de Marly emblem
                // Concentration: EDP
                // Available sizes: 75ml, 125ml (parfums-de-marly.com)
                // Notes confirmed from image: Bergamot, Lavender, Apple, Mandarin, Violet, Jasmine, Geranium, Vanilla, Guaiac Wood, Patchouli, Pink Pepper
                // Prices: parfums-de-marly.com (official EU) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Layton",
                    Description = "Layton е бляскав мъжки аромат с прекрасна свежест, подчертана от богатство и елегантност. Пикантна ябълка, лавандула, бергамот и мандарина срещат виолетка, жасмин и геран, а основата от ванилия, гуаяково дърво и пачули завършва един от най-обичаните нишови аромати на нашето поколение.",
                    Price = 225.00m,
                    ImageUrl = "/images/parfums de marly layton.jpg",
                    Brand = "Parfums de Marly",
                    Size = "75 ml",
                    Concentration = "EDP",
                    StockQuantity = 10,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Ориенталски",
                    Notes = "Връхни: Бергамот, Лавандула, Ябълка, Мандарина | Сърце: Виолетка, Жасмин, Геран | База: Ванилия, Гуаяково дърво, Пачули, Розов пипер"
                }, GetVariantsFor("Parfums de Marly Layton", "Parfums de Marly")),

                // ─── 7. PARFUMS DE MARLY GREENLEY ──────────────────────────────────────────
                // Image: /images/parfums de marly greenley.jpg — confirmed: green bottle, Parfums de Marly emblem
                // Concentration: EDP
                // Available sizes: 75ml, 125ml (parfums-de-marly.com)
                // Notes confirmed from image: Green Apple, Sicilian Bergamot, Mandarin (top),
                //   Cashmeran, Cedarwood, Pomarose, Violet Leaf (heart), Oakmoss, Musk, Amberwood (base)
                // Prices: parfums-de-marly.com (official EU) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Greenley",
                    Description = "Greenley е свеж и чувствен зелен аромат от Parfums de Marly. Сицилиански бергамот, зелена ябълка и мандарина отварят с вибрираща свежест, сърцето разкрива кашмеран, кедър и виолетково листо, а основата от дъбов мъх, мускус и амберово дърво осигурява дълготрайна топлина.",
                    Price = 225.00m,
                    ImageUrl = "/images/parfums de marly greenley.jpg",
                    Brand = "Parfums de Marly",
                    Size = "75 ml",
                    Concentration = "EDP",
                    StockQuantity = 8,
                    Category = "Unisex",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Зелен",
                    Notes = "Връхни: Зелена ябълка, Сицилиански бергамот, Мандарина | Сърце: Кашмеран, Кедър, Поmarose, Виолетково листо | База: Дъбов мъх, Мускус, Амберово дърво"
                }, GetVariantsFor("Parfums de Marly Greenley", "Parfums de Marly")),

                // ─── 8. PARFUMS DE MARLY ALTHAÏR ───────────────────────────────────────────
                // Image: /images/parfums de marly althair.jpg — confirmed: terracotta/caramel bottle, "PARFUMS de MARLY ATHAIR"
                // Concentration: EDP
                // Available sizes: 75ml, 125ml (parfums-de-marly.com)
                // Notes confirmed from image: Bergamot, Orange Blossom, Cinnamon, Cardamom (top),
                //   Bourbon Vanilla, Elemi (heart), Ambroxan, Guaiac Wood, Praline, Musk (base)
                // Prices: parfums-de-marly.com (official EU) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Althaïr",
                    Description = "Althaïr е смел и изискан ориенталски аромат, кръстен на звездата Алтаир. Пикантно начало от канела, кардамон и бергамот се разкрива в топло сърце от бурбонска ванилия и елеми, а основата от амброксан, гуаяково дърво и пралине създава опияняващ, сладък и дълготраен шлейф.",
                    Price = 225.00m,
                    ImageUrl = "/images/parfums de marly althair.jpg",
                    Brand = "Parfums de Marly",
                    Size = "75 ml",
                    Concentration = "EDP",
                    StockQuantity = 8,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Ориенталски Дървесен",
                    Notes = "Връхни: Бергамот, Портокалов цвят, Канела, Кардамон | Сърце: Бурбонска ванилия, Елеми | База: Амброксан, Гуаяково дърво, Пралине, Мускус"
                }, GetVariantsFor("Parfums de Marly Althaïr", "Parfums de Marly")),

                // ─── 9. PARFUMS DE MARLY DELINA EXCLUSIF ───────────────────────────────────
                // Image: /images/parfums de marly - delina exclusif.jpg — confirmed: pink bottle with tassel, "DELINA EXCLUSIF"
                // Concentration: EDP
                // Available sizes: 75ml only (parfums-de-marly.com)
                // Notes confirmed from image: Bergamot, Lychee, Pear, Incense (top),
                //   Turkish Rose, Oud, Incense (heart), Vanilla, Amber, Woody Notes (base)
                // Prices: parfums-de-marly.com (official EU) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Delina Exclusif",
                    Description = "Delina Exclusif е интензивна, по-богата и по-чувствена версия на легендарната Delina. Литчи, круша и тамян отварят с екзотична свежест, сърцето разкрива дамаска роза и уд, а основата от ванилия и кехлибар осигурява луксозно дамско изражение на силата и женствеността.",
                    Price = 305.00m,
                    ImageUrl = "/images/parfums de marly - delina exclusif.jpg",
                    Brand = "Parfums de Marly",
                    Size = "75 ml",
                    Concentration = "EDP",
                    StockQuantity = 6,
                    Category = "Women",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Плодов",
                    Notes = "Връхни: Бергамот, Личи, Круша, Тамян | Сърце: Турска роза, Уд, Тамян | База: Ванилия, Кехлибар, Дървесни нотки"
                }, GetVariantsFor("Parfums de Marly Delina Exclusif", "Parfums de Marly")),

                // ─── 10. MAISON FRANCIS KURKDJIAN BACCARAT ROUGE 540 ───────────────────────
                // Image: /images/Maison Francis Kurkdjian Baccarat Rouge 540.jpg — confirmed: square crystal bottle, red label
                // Concentration: EDP
                // Available sizes: 35ml, 70ml, 200ml (franciskurkdjian.com EU)
                // Notes confirmed from image: Fresh Jasmine, Saffron (top),
                //   Cedarwood, Bitter Almonds (heart), Raw Musk, Oakmoss, Ambergris, Vanilla (base)
                // Prices: franciskurkdjian.com (official EU) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Maison Francis Kurkdjian Baccarat Rouge 540",
                    Description = "Baccarat Rouge 540 е ароматният феномен на нашето поколение. Роден от сътрудничеството между Maison Francis Kurkdjian и кристалната династия Baccarat, той съчетава шафран, жасмин, кедър и горчиви бадеми в прочут, дълготраен аромат с уникален светещ характер.",
                    Price = 265.00m,
                    ImageUrl = "/images/Maison Francis Kurkdjian Baccarat Rouge 540.jpg",
                    Brand = "Maison Francis Kurkdjian",
                    Size = "70 ml",
                    Concentration = "EDP",
                    StockQuantity = 5,
                    Category = "Luxury Fragrances",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Амберов",
                    Notes = "Връхни: Пресен жасмин, Шафран | Сърце: Кедърово дърво, Горчиви бадеми | База: Суров мускус, Дъбов мъх, Амбра, Ванилия"
                }, GetVariantsFor("Maison Francis Kurkdjian Baccarat Rouge 540", "Maison Francis Kurkdjian")),

                // ─── 11. JEAN PAUL GAULTIER LE MALE ───────────────────────────────────────
                // Image: /images/Gemini_Generated_Image_62xyaw62xyaw62xy.jpg — striped torso bottle, notes: Mint, Lavender, Bergamot / Cinnamon, Orange Blossom / Vanilla, Cedar, Sandalwood
                // Concentration: EDT (classic Le Male is EDT)
                // Available sizes: 75ml, 125ml, 200ml (jeanpaulgaultier.com)
                // Notes confirmed from image infographic
                // Prices: jeanpaulgaultier.com / flaconi.de — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Jean Paul Gaultier Le Male",
                    Description = "Легендарният ориенталски фужерен аромат, определил поколение. Мента, лавандула и бергамот отварят с освежаваща дързост, сърцето разкрива канела и портокалов цвят, а топлата основа от ванилия, кедър и сандалово дърво задържа дълго ароматния подпис в иконичния флакон-торс.",
                    Price = 72.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_62xyaw62xyaw62xy.jpg",
                    Brand = "Jean Paul Gaultier",
                    Size = "75 ml",
                    Concentration = "EDT",
                    StockQuantity = 15,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Ориенталски Фужер",
                    Notes = "Връхни: Мента, Лавандула, Бергамот | Сърце: Канела, Кимион, Портокалов цвят, Салвия | База: Ванилия, Боб тонка, Кедър, Сандалово дърво"
                }, GetVariantsFor("Jean Paul Gaultier Le Male", "Jean Paul Gaultier")),

                // ─── 12. JEAN PAUL GAULTIER LE MALE ELIXIR ────────────────────────────────
                // Image: /images/Gemini_Generated_Image_pmsttypmsttypmst.jpg — gold striped torso, notes from image: Lavender, Tonka Bean, Vanilla, Benzoin, Musk, Honey, Tobacco
                // Concentration: Parfum (official name: "Le Male Elixir Parfum")
                // Available sizes: 75ml, 125ml, 200ml (jeanpaulgaultier.com)
                // Notes confirmed from image infographic: Lavender, Benzoin (top), Tonka bean, Vanilla (heart), Musk, Honey, Tobacco (base)
                // Prices: jeanpaulgaultier.com / flaconi.de — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Jean Paul Gaultier Le Male Elixir",
                    Description = "Пламтящ и съблазнителен парфюмен еликсир, по-тъмна и богата итерация на иконичния Le Male. Лавандула и бензоин отварят с топлота, сърцето разкрива ванилия и боб тонка, а основата от мед и тютюн завършва опиянителното усещане в емблематичния златен флакон-торс.",
                    Price = 95.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_pmsttypmsttypmst.jpg",
                    Brand = "Jean Paul Gaultier",
                    Size = "75 ml",
                    Concentration = "Parfum",
                    StockQuantity = 8,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Ориенталски Фужер",
                    Notes = "Връхни: Лавандула, Бензоин | Сърце: Ванилия, Боб тонка | База: Мед, Тютюн, Мускус"
                }, GetVariantsFor("Jean Paul Gaultier Le Male Elixir", "Jean Paul Gaultier")),

                // ─── 13. CREED AVENTUS ─────────────────────────────────────────────────────
                // Image: /images/Gemini_Generated_Image_8993a18993a18993.jpg — Creed bottle, notes infographic visible: Bergamot, Pineapple, Blackcurrant, Apple / Birch, Patchouli, Jasmine / Musk, Oakmoss, Ambergris, Vanilla
                // Concentration: EDP
                // Available sizes: 50ml, 100ml, 250ml (creedfragrance.com)
                // Notes confirmed from fragrantica.com and image
                // Prices: creedfragrance.com (official EU shop-all listing: 320€/100ml confirmed) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Aventus",
                    Description = "Сензационният и емблематичен аромат на Creed — ода на силата, успеха и победата. Незабравими нотки на бергамот, черен касис, сочен ананас и опушена бреза се разкриват над основа от дъбов мъх, пачули и мускус. Любимец на ценители по целия свят от 2010 година.",
                    Price = 320.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_8993a18993a18993.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 10,
                    Category = "Luxury Fragrances",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Плодов Шипър",
                    Notes = "Връхни: Бергамот, Черен касис, Ябълка, Лимон, Розов пипер | Сърце: Ананас, Пачули, Мароканска жасмина, Бреза | База: Мускус, Дъбов мъх, Кедър, Амброксан"
                }, GetVariantsFor("Creed Aventus", "Creed")),

                // ─── 14. CREED MILLÉSIME IMPÉRIAL ─────────────────────────────────────────
                // Image: /images/Gemini_Generated_Image_ewjzp6ewjzp6ewjz.jpg — gold Creed bottle, notes infographic: "CREED AVENTUS MILLÉSIME IMPÉRIAL" label visible, notes: Iris Flowers, Bergamot, Sea Salt, Marine (the image infographic shows Millésime Impérial)
                // Note: image caption misleadingly says "Aventus" in top-left but the BOTTLE clearly reads "MILLÉSIME IMPÉRIAL"
                // Concentration: EDP
                // Available sizes: 50ml, 100ml (creedfragrance.com)
                // Notes corrected from official fragrantica.com and creedfragrance.com
                // Prices: creedfragrance.com — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Millésime Impérial",
                    Description = "Златният стандарт на нишовата парфюмерия. Свеж, морски и цитрусов шедьовър, пренасящ сетивата край слънчевите брегове на Сицилия. Морска сол, бергамот, ирис и мандарина над основа от мускус, сандалово дърво и кехлибар — въплъщение на средиземноморска свежест и луксозна елегантност.",
                    Price = 320.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_ewjzp6ewjzp6ewjz.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 8,
                    Category = "Luxury Fragrances",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Морски Цитрусов",
                    Notes = "Връхни: Ирис, Лимон, Бергамот | Сърце: Морска сол, Морски акорд | База: Сицилиански мандарина, Мускус, Сандалово дърво, Кехлибар"
                }, GetVariantsFor("Creed Millésime Impérial", "Creed")),

                // ─── 15. CREED GREEN IRISH TWEED ──────────────────────────────────────────
                // Image: /images/Gemini_Generated_Image_utb5m6utb5m6utb5.jpg — dark bottle, notes: Lemon, Verbena Leaves (top), Iris, Violet Leaves (heart), Sandalwood, Ambergris (base) — clearly GREEN IRISH TWEED
                // Concentration: EDP
                // Available sizes: 50ml, 100ml (creedfragrance.com)
                // Notes confirmed from image infographic
                // Prices: creedfragrance.com (320€/100ml) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Green Irish Tweed",
                    Description = "Вечната елегантност на ирландската природа. Изтънчен и мъжествен аромат, съчетаващ свежестта на лимонова вербена и теменужкови листа с ирис, кадифена основа от сандалово дърво и сив кехлибар. Класика на Creed, обичана от поколения мъже с изтънчен вкус.",
                    Price = 320.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_utb5m6utb5m6utb5.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Фужер",
                    Notes = "Връхни: Лимон, Лимонова вербена | Сърце: Ирис, Теменужкови листа | База: Сандалово дърво, Кехлибар"
                }, GetVariantsFor("Creed Green Irish Tweed", "Creed")),

                // ─── 16. CREED SILVER MOUNTAIN WATER ──────────────────────────────────────
                // Image: /images/Gemini_Generated_Image_xl3ywjxl3ywjxl3y.jpg — white/silver bottle, notes: Bergamot, Mandarin, Neroli (top), Green Tea, Blackcurrant Bud (heart), Sandalwood, Musk, Petitgrain, Galbanum (base)
                // Concentration: EDP
                // Available sizes: 50ml, 100ml (creedfragrance.com)
                // Notes confirmed from image infographic
                // Prices: creedfragrance.com (320€/100ml) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Silver Mountain Water",
                    Description = "Кристална свежест, вдъхновена от чистите алпийски потоци. Ободряващ планински полъх, съчетаващ бергамот, мандарина и нероли с зелен чай и пъпки от черен касис над основа от сандалово дърво, мускус и галбанум. Едновременно свеж и дървесен — перфектен за всяко годишно време.",
                    Price = 320.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_xl3ywjxl3ywjxl3y.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 9,
                    Category = "Unisex",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Аквален",
                    Notes = "Връхни: Бергамот, Мандарина, Нероли | Сърце: Зелен чай, Пъпки от черен касис | База: Сандалово дърво, Мускус, Петитгрен, Галбанум"
                }, GetVariantsFor("Creed Silver Mountain Water", "Creed")),

                // ─── 17. YVES SAINT LAURENT Y EDP ─────────────────────────────────────────
                // Image: sent by user — rectangular dark blue gradient bottle, YvesSaintLaurent logo, "Y" star design
                // Notes confirmed from image: Ginger, Bergamot, Green Apple (top), Lavender, Sage, Geranium (heart), Vetiver, Cedar, Fir, Tonka Bean (base)
                // Concentration: EDP (not EDT — this is the Y Eau de Parfum)
                // Available sizes: 60ml, 100ml, 200ml (yslbeauty.de official)
                // Prices: yslbeauty.de (official EU) — November 2026
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Yves Saint Laurent Y",
                    Description = "Y Eau de Parfum е смело и интензивно изражение на съвременната мъжественост от Yves Saint Laurent. Пикантен джинджифил, бергамот и зелена ябълка отварят с електризираща свежест, сърцето разкрива лавандула, салвия и геран, а дълбоката основа от ветивер, кедър и боб тонка осигурява неустоим ароматен подпис.",
                    Price = 95.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_izenq4izenq4izen.jpg",
                    Brand = "Yves Saint Laurent",
                    Size = "60 ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Ароматен",
                    Notes = "Връхни: Джинджифил, Бергамот, Зелена ябълка | Сърце: Лавандула, Салвия, Геран | База: Ветивер, Кедър, Ела, Боб тонка"
                }, GetVariantsFor("Yves Saint Laurent Y", "Yves Saint Laurent")),
            };

            // Add products and variants
            foreach (var (product, variants) in products)
            {
                context.Products.Add(product);
                foreach (var variant in variants)
                {
                    variant.ProductId = product.Id;
                    context.ProductVariants.Add(variant);
                }
            }

            context.SaveChanges();
        }
    }
}
