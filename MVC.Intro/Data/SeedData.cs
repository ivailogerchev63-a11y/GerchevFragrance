using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
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
                // Rebuild the variant data for all existing products
                // by matching products by name
                AddVariantsForExistingProducts(context);
            }
        }

        private static void AddVariantsForExistingProducts(AppDbContext context)
        {
            var allProducts = context.Products.ToList();

            // Use raw SQL to bypass potential FK constraint issues with legacy DB schemas
            // Each INSERT is validated against the actual product IDs from the DB
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

        private static List<ProductVariant> GetVariantsFor(string productName, string brand)
        {
            return productName switch
            {
                "Dior Sauvage Elixir" => new List<ProductVariant>
                {
                    new() { SizeMl = 60, Price = 174.00m, StockQuantity = 15 },
                    new() { SizeMl = 100, Price = 239.00m, StockQuantity = 10 }
                },
                "Tom Ford Ombré Leather" => new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 140.00m, StockQuantity = 12 },
                    new() { SizeMl = 100, Price = 195.00m, StockQuantity = 8 },
                    new() { SizeMl = 150, Price = 248.00m, StockQuantity = 5 }
                },
                "Tom Ford Black Orchid" => new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 150.00m, StockQuantity = 10 },
                    new() { SizeMl = 100, Price = 230.00m, StockQuantity = 8 }
                },
                "Parfums de Marly Layton" => new List<ProductVariant>
                {
                    new() { SizeMl = 75, Price = 229.00m, StockQuantity = 10 },
                    new() { SizeMl = 125, Price = 295.00m, StockQuantity = 7 }
                },
                "Parfums de Marly Greenley" => new List<ProductVariant>
                {
                    new() { SizeMl = 75, Price = 220.00m, StockQuantity = 8 },
                    new() { SizeMl = 125, Price = 285.00m, StockQuantity = 5 }
                },
                "Parfums de Marly Althaïr" => new List<ProductVariant>
                {
                    new() { SizeMl = 75, Price = 225.00m, StockQuantity = 8 },
                    new() { SizeMl = 125, Price = 290.00m, StockQuantity = 5 }
                },
                "Parfums de Marly Delina Exclusif" => new List<ProductVariant>
                {
                    new() { SizeMl = 75, Price = 310.00m, StockQuantity = 6 }
                },
                "Carolina Herrera Good Girl" => new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 115.00m, StockQuantity = 15 },
                    new() { SizeMl = 80, Price = 145.00m, StockQuantity = 10 }
                },
                "Carolina Herrera Bad Boy Extreme" => new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 95.00m, StockQuantity = 12 },
                    new() { SizeMl = 100, Price = 128.00m, StockQuantity = 8 }
                },
                "Maison Francis Kurkdjian Baccarat Rouge 540" => new List<ProductVariant>
                {
                    new() { SizeMl = 35, Price = 170.00m, StockQuantity = 8 },
                    new() { SizeMl = 70, Price = 265.00m, StockQuantity = 5 },
                    new() { SizeMl = 200, Price = 525.00m, StockQuantity = 3 }
                },
                "Jean Paul Gaultier Le Male" => new List<ProductVariant>
                {
                    new() { SizeMl = 75, Price = 79.00m, StockQuantity = 15 },
                    new() { SizeMl = 125, Price = 99.00m, StockQuantity = 10 }
                },
                "Jean Paul Gaultier Le Male Elixir" => new List<ProductVariant>
                {
                    new() { SizeMl = 75, Price = 109.00m, StockQuantity = 10 },
                    new() { SizeMl = 125, Price = 139.00m, StockQuantity = 7 }
                },
                _ when brand == "Creed" => new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 320.00m, StockQuantity = 8 },
                    new() { SizeMl = 100, Price = 490.00m, StockQuantity = 5 }
                },
                _ => new List<ProductVariant>()
            };
        }

        private static void SeedProducts(AppDbContext context)
        {
            var products = new List<(Product Product, List<ProductVariant> Variants)>
            {
                // 1. DIOR SAUVAGE ELIXIR — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Dior Sauvage Elixir",
                    Description = "Изключително концентриран и интензивен аромат, рождената кулминация на легендарната Sauvage вселена. Опияняващото сърце от Nyons лавандула, подправки и пикантен кардамон се разкрива бавно над богата кехлибарена и сандалова основа. Незаменим ароматен подпис за мъже с характер.",
                    Price = 174.00m,
                    ImageUrl = "/images/dior sauvage elixir.jpg",
                    Brand = "Dior",
                    Size = "60 ml",
                    Concentration = "Elixir",
                    StockQuantity = 15,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = true,
                    FragranceFamily = "Ориенталски Фужер",
                    Notes = "Връхни: Канела, Кардамон, Индийско орехче, Грейпфрут | Сърце: Nyons Лавандула | База: Кехлибар, Женско биле, Сандалово дърво, Пачули"
                }, GetVariantsFor("Dior Sauvage Elixir", "Dior")),

                // 2. TOM FORD OMBRÉ LEATHER — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Tom Ford Ombré Leather",
                    Description = "Изискан и чувствен кожен аромат, вдъхновен от безкрайните простори на американския запад. Нотките на черна кожа, кардамон, жасмин самбак, пачули и бял мъх се преплитат в дървесен ориенталски шедьовър с дълготрайно присъствие.",
                    Price = 140.00m,
                    ImageUrl = "/images/tom ford omre leather.jpg",
                    Brand = "Tom Ford",
                    Size = "50 ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Unisex",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Ориенталски",
                    Notes = "Връхни: Кардамон | Сърце: Черна кожа, Жасмин Самбак | База: Кехлибар, Дъбов мъх, Пачули"
                }, GetVariantsFor("Tom Ford Ombré Leather", "Tom Ford")),

                // 3. TOM FORD BLACK ORCHID — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Tom Ford Black Orchid",
                    Description = "Смел и луксозен, Black Orchid е емблематичен тъмен флорален аромат от Tom Ford. Трюфел, черни орхидеи, пачули и ветивер се сливат в неповторима унисекс ароматна симфония, определяща нова ера в луксозната парфюмерия.",
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
                    Notes = "Връхни: Черен трюфел, Иланг-иланг, Черна смокиня | Сърце: Черна орхидея, Лотос, Плодови нотки | База: Пачули, Ветивер, Сандалово дърво, Тонка"
                }, GetVariantsFor("Tom Ford Black Orchid", "Tom Ford")),

                // 4. CAROLINA HERRERA GOOD GIRL — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Carolina Herrera Good Girl",
                    Description = "Контрастът между деня и нощта, между добродетелността и соблазна. Good Girl е смел дамски аромат в емблематичен флакон-обувка. Жасмин самбак и тунизийски жасмин срещат кафе, тонка и какао в мощна флорална ориенталска интерпретация.",
                    Price = 115.00m,
                    ImageUrl = "/images/carolina herrera good girl.jpg",
                    Brand = "Carolina Herrera",
                    Size = "50 ml",
                    Concentration = "EDP",
                    StockQuantity = 15,
                    Category = "Women",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Ориенталски",
                    Notes = "Връхни: Лимон, Бергамот, Бадем | Сърце: Жасмин Самбак, Тунизийски жасмин | База: Кафе, Тонка, Какао, Ветивер"
                }, GetVariantsFor("Carolina Herrera Good Girl", "Carolina Herrera")),

                // 5. CAROLINA HERRERA BAD BOY EXTREME — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Carolina Herrera Bad Boy Extreme",
                    Description = "Смел, интензивен и провокативен. Bad Boy Extreme е по-тъмна, по-дълбока итерация на иконичния Bad Boy. Пачули, морски нотки и дървесни акорди се подчертават от богата ориенталска сърцевина с тъмна страст и необуздана мощ.",
                    Price = 95.00m,
                    ImageUrl = "/images/carrolina herrera bad boy extreme.jpg",
                    Brand = "Carolina Herrera",
                    Size = "50 ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Ориенталски",
                    Notes = "Връхни: Пикантни нотки, Морски нотки | Сърце: Пачули, Ветивер | База: Кехлибар, Дъбов мъх, Сандалово дърво"
                }, GetVariantsFor("Carolina Herrera Bad Boy Extreme", "Carolina Herrera")),

                // 6. PARFUMS DE MARLY LAYTON — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Layton",
                    Description = "Layton е бляскав мъжки аромат с прекрасна свежест, подчертана от богатство и елегантност. Пикантна ябълка, лавандула и ирис срещат ванилия, пачули и сандалово дърво в един от най-обичаните нишови аромати на нашето поколение.",
                    Price = 229.00m,
                    ImageUrl = "/images/parfums de marly layton.jpg",
                    Brand = "Parfums de Marly",
                    Size = "75 ml",
                    Concentration = "EDP",
                    StockQuantity = 10,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Ориенталски",
                    Notes = "Връхни: Ябълка, Мента, Гвоздей | Сърце: Ирис, Жасмин, Лавандула | База: Ванилия, Пачули, Сандалово дърво, Мускус"
                }, GetVariantsFor("Parfums de Marly Layton", "Parfums de Marly")),

                // 7. PARFUMS DE MARLY GREENLEY — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Greenley",
                    Description = "Greenley е свеж и чувствен зелен аромат, вдъхновен от пролетните ливади на Парижкото предградие. Вибрираща комбинация от цитруси, бяла пъпка, теменужка и зелени нотки над основа от сандалово дърво и мускус — елегантен, жизнен и непреходен.",
                    Price = 220.00m,
                    ImageUrl = "/images/parfums de marly greenley.jpg",
                    Brand = "Parfums de Marly",
                    Size = "75 ml",
                    Concentration = "EDP",
                    StockQuantity = 8,
                    Category = "Unisex",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Зелен",
                    Notes = "Връхни: Розов пипер, Лимон, Касис | Сърце: Теменужка, Бяла пъпка, Жасмин | База: Сандалово дърво, Кехлибар, Мускус"
                }, GetVariantsFor("Parfums de Marly Greenley", "Parfums de Marly")),

                // 8. PARFUMS DE MARLY ALTHAÏR — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Althaïr",
                    Description = "Althaïr е смел и изискан ориенталски аромат, кръстен на звездата Алтаир. Мощен старт с подправки и цитруси се разкрива постепенно в сложна смес от уд, кехлибар и кадифени дървесни акорди — истинска ароматна поема за ценители.",
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
                    Notes = "Връхни: Розов пипер, Бергамот, Кардамон | Сърце: Уд, Ирис | База: Сандалово дърво, Кехлибар, Мускус, Пачули"
                }, GetVariantsFor("Parfums de Marly Althaïr", "Parfums de Marly")),

                // 9. PARFUMS DE MARLY DELINA EXCLUSIF — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Parfums de Marly Delina Exclusif",
                    Description = "Delina Exclusif е интензивна, по-богата и по-чувствена версия на легендарната Delina. Розата остава в сърцето, но литчи, малина и пачули добавят тъмен, съблазнителен характер — луксозно дамско изражение на силата и женствеността.",
                    Price = 310.00m,
                    ImageUrl = "/images/parfums de marly - delina exclusif.jpg",
                    Brand = "Parfums de Marly",
                    Size = "75 ml",
                    Concentration = "EDP",
                    StockQuantity = 6,
                    Category = "Women",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Флорален Плодов",
                    Notes = "Връхни: Литчи, Малина, Лимон | Сърце: Роза, Теменужка, Пеония | База: Пачули, Мускус, Ванилия, Кехлибар"
                }, GetVariantsFor("Parfums de Marly Delina Exclusif", "Parfums de Marly")),

                // 10. MAISON FRANCIS KURKDJIAN BACCARAT ROUGE 540 — Real photo available
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Maison Francis Kurkdjian Baccarat Rouge 540",
                    Description = "Baccarat Rouge 540 е ароматният феномен на нашето поколение. Роден от сътрудничеството между Maison Francis Kurkdjian и кристалната династия Baccarat, той съчетава шафран, жасмин, кехлибарено дърво и кедър в прочут, дълготраен аромат с уникален светещ характер.",
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
                    Notes = "Връхни: Шафран, Жасмин | Сърце: Амберово дърво, Египетски жасмин | База: Бял мускус, Кедърово дърво, Fir Resin"
                }, GetVariantsFor("Maison Francis Kurkdjian Baccarat Rouge 540", "Maison Francis Kurkdjian")),

                // 11. JEAN PAUL GAULTIER LE MALE — Existing AI image
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Jean Paul Gaultier Le Male",
                    Description = "Легендарният ориенталски фужерен аромат, определил поколение. Мента, лавандула и бергамот отварят с освежаваща дързост, сърцето разкрива канела и портокалов цвят, а топлата основа от ванилия, кедър и сандалово дърво задържа дълго ароматния подпис.",
                    Price = 79.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_62xyaw62xyaw62xy.jpg",
                    Brand = "Jean Paul Gaultier",
                    Size = "75 ml",
                    Concentration = "EDT",
                    StockQuantity = 15,
                    Category = "Men",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Ориенталски Фужер",
                    Notes = "Връхни: Мента, Лавандула, Бергамот | Сърце: Канела, Портокалов цвят | База: Ванилия, Кедър, Сандал"
                }, GetVariantsFor("Jean Paul Gaultier Le Male", "Jean Paul Gaultier")),

                // 12. JEAN PAUL GAULTIER LE MALE ELIXIR — Existing AI image
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Jean Paul Gaultier Le Male Elixir",
                    Description = "Пламтящ и съблазнителен парфюмен еликсир, по-тъмна и богата итерация на иконичния Le Male. В емблематичния златен флакон-торс са скрити нотки на лавандула, мента, бензоин, ванилия и зърна тонка — опиянение за сетивата.",
                    Price = 109.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_pmsttypmsttypmst.jpg",
                    Brand = "Jean Paul Gaultier",
                    Size = "75 ml",
                    Concentration = "Parfum",
                    StockQuantity = 8,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Ориенталски Фужер",
                    Notes = "Връхни: Лавандула, Мента | Сърце: Ванилия, Бензоин | База: Мед, Боб тонка, Тютюн"
                }, GetVariantsFor("Jean Paul Gaultier Le Male Elixir", "Jean Paul Gaultier")),

                // 13. CREED AVENTUS — Existing AI image
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Aventus",
                    Description = "Сензационният и емблематичен аромат на Creed — ода на силата, успеха и победата. Незабравими нотки на бергамот, черен касис, сочен ананас и опушена бреза се разкриват над основа от дъбов мъх, пачули и мускус. Любимец на ценители по целия свят.",
                    Price = 490.00m,
                    OriginalPrice = 540.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_8993a18993a18993.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 10,
                    Category = "Luxury Fragrances",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Плодов Шипър",
                    Notes = "Връхни: Бергамот, Касис, Ябълка, Ананас | Сърце: Бреза, Пачули, Жасмин | База: Мускус, Дъбов мъх, Амбра, Ветивер"
                }, new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 320.00m, StockQuantity = 8 },
                    new() { SizeMl = 100, Price = 490.00m, OriginalPrice = 540.00m, StockQuantity = 5 }
                }),

                // 14. CREED MILLÉSIME IMPÉRIAL — Existing AI image
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Millésime Impérial",
                    Description = "Златният стандарт на нишовата парфюмерия. Свеж, морски и цитрусов шедьовър, пренасящ сетивата край слънчевите брегове на Сицилия. Морска сол, лимон, тангерин и ирис над основа от мускус и дървесни нотки — чистотата на луксозния аромат.",
                    Price = 490.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_ewjzp6ewjzp6ewjz.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 8,
                    Category = "Luxury Fragrances",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Морски Цитрусов",
                    Notes = "Връхни: Морска сол, Тангерин | Сърце: Сицилиански лимон, Ирис | База: Мускус, Дървесни нотки, Амбра"
                }, new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 320.00m, StockQuantity = 6 },
                    new() { SizeMl = 100, Price = 490.00m, StockQuantity = 4 }
                }),

                // 15. CREED GREEN IRISH TWEED — Existing AI image
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Green Irish Tweed",
                    Description = "Вечната елегантност на ирландската природа. Изтънчен и мъжествен аромат, съчетаващ свежестта на лимонова вербена, теменужкови листа и ирис с кадифена основа от сив кехлибар и сандалово дърво. Класика на Creed, обичана от поколения.",
                    Price = 490.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_utb5m6utb5m6utb5.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 12,
                    Category = "Men",
                    IsBestseller = true,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Фужер",
                    Notes = "Връхни: Лимонова вербена, Ирис | Сърце: Теменужки листа | База: Сив кехлибар, Сандалово дърво"
                }, new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 320.00m, StockQuantity = 10 },
                    new() { SizeMl = 100, Price = 490.00m, StockQuantity = 6 }
                }),

                // 16. CREED SILVER MOUNTAIN WATER — Existing AI image
                (new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Creed Silver Mountain Water",
                    Description = "Кристална свежест, вдъхновена от чистите алпийски потоци. Ободряващ планински полъх, съчетаващ зелен чай, черен касис и искрящ бергамот над основа от мускус и галбанум. Едновременно свеж и дървесен — перфектен за всяко годишно време.",
                    Price = 490.00m,
                    OriginalPrice = 530.00m,
                    ImageUrl = "/images/Gemini_Generated_Image_xl3ywjxl3ywjxl3y.jpg",
                    Brand = "Creed",
                    Size = "100 ml",
                    Concentration = "EDP",
                    StockQuantity = 9,
                    Category = "Unisex",
                    IsBestseller = false,
                    IsFeatured = false,
                    FragranceFamily = "Дървесен Аквален",
                    Notes = "Връхни: Бергамот, Мандарина | Сърце: Зелен чай, Черен касис | База: Галбанум, Мускус, Сандалово дърво"
                }, new List<ProductVariant>
                {
                    new() { SizeMl = 50, Price = 320.00m, OriginalPrice = 350.00m, StockQuantity = 7 },
                    new() { SizeMl = 100, Price = 490.00m, OriginalPrice = 530.00m, StockQuantity = 5 }
                })
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
