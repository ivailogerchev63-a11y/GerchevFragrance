using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    /// <summary>
    /// One-time catalogue correction applied at startup.
    /// - Removes placeholder "images" / "images(1)" product records.
    /// - Updates existing product names, descriptions, notes, concentrations and images
    ///   to match the visually confirmed images in wwwroot/images.
    /// - Corrects existing ProductVariant prices to verified retail values.
    /// - Adds YSL Y EDP if not already present.
    /// - Idempotent: safe to run on every startup.
    /// </summary>
    public static class CatalogueCorrection
    {
        // Verified retail prices (EUR) from official sources, November 2026
        // Sources listed in SeedData.cs

        public static async Task ApplyAsync(AppDbContext db)
        {
            // ── 1. Remove placeholder "images" / "images(1)" products ─────────────────
            bool removedPlaceholders = await RemovePlaceholderProductsAsync(db);
            if (removedPlaceholders)
                await db.SaveChangesAsync();

            // ── 2. Correct existing product data ──────────────────────────────────────
            bool correctedProducts = await CorrectExistingProductsAsync(db);
            // Raw SQL — no SaveChangesAsync needed

            // ── 3. Correct existing variant prices ────────────────────────────────────
            await CorrectVariantPricesAsync(db);
            // Raw SQL — no SaveChangesAsync needed

            // ── 4. Add new product: YSL Y EDP ─────────────────────────────────────────
            await AddYslYEdpAsync(db);

            // ── 5. Add missing variants to existing products ───────────────────────────
            await AddMissingVariantsAsync(db);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 1. Remove placeholder products
        // ─────────────────────────────────────────────────────────────────────────────

        private static async Task<bool> RemovePlaceholderProductsAsync(AppDbContext db)
        {
            // Names that are clearly accidental image-filename products
            var placeholderNames = new[] { "images", "images(1)", "images (1)", "image" };

            var placeholders = await db.Products
                .Include(p => p.Variants)
                .Where(p => placeholderNames.Contains(p.Name.ToLower().Trim()))
                .ToListAsync();

            if (!placeholders.Any()) return false;

            foreach (var p in placeholders)
            {
                // Check whether this product ID appears in historical orders
                var hasOrders = await db.OrderItems.AnyAsync(oi => oi.ProductId == p.Id);
                if (hasOrders)
                {
                    // Soft-delete: zero stock, unfeature, rename to make clear it is inactive
                    p.StockQuantity = 0;
                    p.IsFeatured = false;
                    p.IsBestseller = false;
                    p.Name = $"[REMOVED] {p.Name}";
                }
                else
                {
                    // Hard delete — no order history
                    db.ProductVariants.RemoveRange(p.Variants);
                    db.Products.Remove(p);
                }
            }

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 2. Correct existing product metadata
        // ─────────────────────────────────────────────────────────────────────────────

        private static async Task<bool> CorrectExistingProductsAsync(AppDbContext db)
        {
            var products = await db.Products.ToListAsync();
            bool changed = false;

            foreach (var p in products)
            {
                var correction = GetProductCorrection(p.Name);
                if (correction == null) continue;

                // Use raw SQL updates to avoid EF concurrency issues on startup
                var id = p.Id.ToString();

                if (correction.Notes != null && p.Notes != correction.Notes)
                {
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE Products SET Notes = '{EscapeSql(correction.Notes)}' WHERE Id = '{id}'");
                    changed = true;
                }
                if (correction.Description != null && p.Description != correction.Description)
                {
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE Products SET Description = '{EscapeSql(correction.Description)}' WHERE Id = '{id}'");
                    changed = true;
                }
                if (correction.FragranceFamily != null && p.FragranceFamily != correction.FragranceFamily)
                {
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE Products SET FragranceFamily = '{EscapeSql(correction.FragranceFamily)}' WHERE Id = '{id}'");
                    changed = true;
                }
                if (correction.Concentration != null && p.Concentration != correction.Concentration)
                {
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE Products SET Concentration = '{EscapeSql(correction.Concentration)}' WHERE Id = '{id}'");
                    changed = true;
                }
                if (correction.BasePrice.HasValue && p.Price != correction.BasePrice.Value)
                {
                    var priceStr = correction.BasePrice.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE Products SET Price = {priceStr} WHERE Id = '{id}'");
                    changed = true;
                }
                if (correction.RemoveOriginalPrice && p.OriginalPrice.HasValue)
                {
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE Products SET OriginalPrice = NULL WHERE Id = '{id}'");
                    changed = true;
                }
            }

            // Detach all tracked products so subsequent operations work cleanly
            if (changed)
            {
                foreach (var entry in db.ChangeTracker.Entries<Product>().ToList())
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            return changed;
        }

        private static string EscapeSql(string s) => s.Replace("'", "''");

        // ─────────────────────────────────────────────────────────────────────────────
        // 3. Correct existing variant prices
        // ─────────────────────────────────────────────────────────────────────────────

        private static async Task CorrectVariantPricesAsync(AppDbContext db)
        {
            var variants = await db.ProductVariants
                .Include(v => v.Product)
                .ToListAsync();

            foreach (var v in variants)
            {
                var productName = v.Product?.Name ?? "";
                var correction = GetVariantCorrection(productName, v.SizeMl);
                if (correction == null) continue;

                if (v.Price != correction.Price)
                {
                    var priceStr = correction.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    await db.Database.ExecuteSqlRawAsync(
                        $"UPDATE ProductVariants SET Price = {priceStr} WHERE Id = '{v.Id}'");
                }
                if (v.OriginalPrice != correction.OriginalPrice)
                {
                    if (correction.OriginalPrice.HasValue)
                    {
                        var opStr = correction.OriginalPrice.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                        await db.Database.ExecuteSqlRawAsync(
                            $"UPDATE ProductVariants SET OriginalPrice = {opStr} WHERE Id = '{v.Id}'");
                    }
                    else
                    {
                        await db.Database.ExecuteSqlRawAsync(
                            $"UPDATE ProductVariants SET OriginalPrice = NULL WHERE Id = '{v.Id}'");
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // 4. Add YSL Y EDP if not present
        // ─────────────────────────────────────────────────────────────────────────────

        private static async Task AddYslYEdpAsync(AppDbContext db)
        {
            // Check by name to avoid duplicates
            var exists = await db.Products.AnyAsync(p =>
                p.Name.ToLower().Contains("yves saint laurent") ||
                (p.Brand != null && p.Brand.ToLower().Contains("yves saint laurent")));

            if (exists) return;

            var productId = Guid.NewGuid().ToString();
            var notes = "Връхни: Джинджифил, Бергамот, Зелена ябълка | Сърце: Лавандула, Салвия, Геран | База: Ветивер, Кедър, Ела, Боб тонка";
            var description = "Y Eau de Parfum е смело и интензивно изражение на съвременната мъжественост от Yves Saint Laurent. Пикантен джинджифил, бергамот и зелена ябълка отварят с електризираща свежест, сърцето разкрива лавандула, салвия и геран, а дълбоката основа от ветивер, кедър и боб тонка осигурява неустоим ароматен подпис.";

            // Insert product via raw SQL to bypass FK issues
            await db.Database.ExecuteSqlRawAsync(
                $"INSERT OR IGNORE INTO Products (Id, Name, Description, Price, OriginalPrice, ImageUrl, Brand, Size, Concentration, StockQuantity, Category, IsBestseller, IsFeatured, Notes, FragranceFamily) " +
                $"VALUES ('{productId}', 'Yves Saint Laurent Y', '{EscapeSql(description)}', 95.00, NULL, " +
                $"'/images/Gemini_Generated_Image_izenq4izenq4izen.jpg', 'Yves Saint Laurent', '60 ml', 'EDP', 12, 'Men', 0, 0, '{EscapeSql(notes)}', 'Дървесен Ароматен')");

            // Insert variants via raw SQL
            var v1 = Guid.NewGuid().ToString();
            var v2 = Guid.NewGuid().ToString();
            var v3 = Guid.NewGuid().ToString();

            await db.Database.ExecuteSqlRawAsync(
                $"INSERT OR IGNORE INTO ProductVariants (Id, ProductId, SizeMl, Price, OriginalPrice, StockQuantity, IsAvailable) VALUES ('{v1}', '{productId}', 60, 95.00, NULL, 12, 1)");
            await db.Database.ExecuteSqlRawAsync(
                $"INSERT OR IGNORE INTO ProductVariants (Id, ProductId, SizeMl, Price, OriginalPrice, StockQuantity, IsAvailable) VALUES ('{v2}', '{productId}', 100, 130.00, NULL, 8, 1)");
            await db.Database.ExecuteSqlRawAsync(
                $"INSERT OR IGNORE INTO ProductVariants (Id, ProductId, SizeMl, Price, OriginalPrice, StockQuantity, IsAvailable) VALUES ('{v3}', '{productId}', 200, 180.00, NULL, 5, 1)");
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Correction lookup tables
        // ─────────────────────────────────────────────────────────────────────────────

        // ─────────────────────────────────────────────────────────────────────────────
        // 5. Add missing variants to existing products
        // ─────────────────────────────────────────────────────────────────────────────

        private static async Task AddMissingVariantsAsync(AppDbContext db)
        {
            var products = await db.Products.Include(p => p.Variants).ToListAsync();

            foreach (var product in products)
            {
                var existingSizes = product.Variants.Select(v => v.SizeMl).ToHashSet();
                var allExpected = SeedData.GetVariantsForPublic(product.Name, product.Brand ?? "");

                foreach (var v in allExpected.Where(v => !existingSizes.Contains(v.SizeMl)))
                {
                    var id = Guid.NewGuid().ToString();
                    var productId = product.Id.ToString();
                    var priceStr = v.Price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                    var opStr = v.OriginalPrice.HasValue
                        ? v.OriginalPrice.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        : "NULL";

                    await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF");
                    await db.Database.ExecuteSqlRawAsync(
                        $"INSERT OR IGNORE INTO ProductVariants (Id, ProductId, SizeMl, Price, OriginalPrice, StockQuantity, IsAvailable) VALUES ('{id}', '{productId}', {v.SizeMl}, {priceStr}, {opStr}, {v.StockQuantity}, 1)");
                    await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON");
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Correction lookup tables
        // ─────────────────────────────────────────────────────────────────────────────

        private record ProductCorrection(
            string? Notes,
            string? Description,
            string? FragranceFamily,
            string? Concentration,
            decimal? BasePrice,
            bool RemoveOriginalPrice = false);

        private static ProductCorrection? GetProductCorrection(string name) => name switch
        {
            "Dior Sauvage Elixir" => new(
                Notes: "Връхни: Индийско орехче, Канела, Кардамон, Грейпфрут | Сърце: AOP Лавандула (Nyons) | База: Женско биле, Сандалово дърво, Кехлибар, Пачули",
                Description: "Изключително концентрирана есенция, кулминацията на легендарната Sauvage вселена. Опияняващото сърце от AOP лавандула от Nyons, пикантен кардамон и индийско орехче се разкриват бавно над богата амберна и дървесна основа. Уникалната концентрация Elixir пленява сетивата с дълготраен, мощен и завладяващ ароматен шлейф.",
                FragranceFamily: "Ориенталски Фужер",
                Concentration: "Elixir",
                BasePrice: 169.00m),

            "Tom Ford Ombré Leather" => new(
                Notes: "Връхни: Кардамон, Жасмин Самбак | Сърце: Черна кожа, Бял мъх | База: Кехлибар, Пачули, Дървесни нотки",
                Description: "Изискан и чувствен кожен аромат, вдъхновен от безкрайните простори на американския запад. Нотките на черна кожа, кардамон и жасмин самбак се преплитат с пачули, бял мъх и кехлибар в дървесен ориенталски шедьовър с дълготрайно присъствие.",
                FragranceFamily: "Дървесен Ориенталски",
                Concentration: "EDP",
                BasePrice: 150.00m),

            "Tom Ford Black Orchid" => new(
                Notes: "Връхни: Черен трюфел, Гардения, Черен касис, Иланг-иланг | Сърце: Черна орхидея, Подправки, Плодови нотки, Лотос | База: Мексикански шоколад, Пачули, Ванилия, Тамян, Кехлибар, Сандалово дърво",
                Description: "Смел и луксозен, Black Orchid е емблематичен тъмен флорален аромат от Tom Ford. Черен трюфел, гардения, иланг-иланг и черни орхидеи се сливат с мексикански шоколад, пачули и кехлибар в неповторима унисекс ароматна симфония.",
                FragranceFamily: "Флорален Ориенталски",
                Concentration: "EDP",
                BasePrice: 150.00m),

            "Carolina Herrera Good Girl" => new(
                Notes: "Връхни: Бергамот, Лимон, Бадем, Кафе | Сърце: Туберозa, Жасмин Самбак, Ирис, Портокалов цвят | База: Боб тонка, Какао, Сандалово дърво, Ванилия",
                Description: "Контрастът между деня и нощта, между добродетелността и соблазна. Good Girl е смел дамски аромат в емблематичния флакон-обувка. Жасмин самбак и туберозата срещат кафе, тонка и какао в мощна флорална ориенталска интерпретация.",
                FragranceFamily: "Флорален Ориенталски",
                Concentration: "EDP",
                BasePrice: 85.00m),

            "Carolina Herrera Bad Boy Extreme" => new(
                Notes: "Връхни: Джинджифил, Бергамот | Сърце: Какао, Ветивер | База: Пачули, Боб тонка",
                Description: "Смел, интензивен и провокативен. Bad Boy Extreme е по-тъмна, по-дълбока итерация на иконичния Bad Boy. Пикантен джинджифил, бергамот и какао абсолют се разкриват над богата основа от ветивер, пачули и боб тонка.",
                FragranceFamily: "Дървесен Ориенталски",
                Concentration: "EDP",
                BasePrice: 80.00m),

            "Parfums de Marly Layton" => new(
                Notes: "Връхни: Бергамот, Лавандула, Ябълка, Мандарина | Сърце: Виолетка, Жасмин, Геран | База: Ванилия, Гуаяково дърво, Пачули, Розов пипер",
                Description: "Layton е бляскав мъжки аромат с прекрасна свежест, подчертана от богатство и елегантност. Пикантна ябълка, лавандула, бергамот и мандарина срещат виолетка, жасмин и геран в един от най-обичаните нишови аромати на нашето поколение.",
                FragranceFamily: "Дървесен Ориенталски",
                Concentration: "EDP",
                BasePrice: 225.00m),

            "Parfums de Marly Greenley" => new(
                Notes: "Връхни: Зелена ябълка, Сицилиански бергамот, Мандарина | Сърце: Кашмеран, Кедър, Поmarose, Виолетково листо | База: Дъбов мъх, Мускус, Амберово дърво",
                Description: "Greenley е свеж и чувствен зелен аромат от Parfums de Marly. Сицилиански бергамот, зелена ябълка и мандарина отварят с вибрираща свежест, сърцето разкрива кашмеран, кедър и виолетково листо, а основата от дъбов мъх, мускус и амберово дърво осигурява дълготрайна топлина.",
                FragranceFamily: "Флорален Зелен",
                Concentration: "EDP",
                BasePrice: 225.00m),

            "Parfums de Marly Althaïr" => new(
                Notes: "Връхни: Бергамот, Портокалов цвят, Канела, Кардамон | Сърце: Бурбонска ванилия, Елеми | База: Амброксан, Гуаяково дърво, Пралине, Мускус",
                Description: "Althaïr е смел и изискан ориенталски аромат от Parfums de Marly. Пикантно начало от канела, кардамон и бергамот се разкрива в топло сърце от бурбонска ванилия и елеми, а основата от амброксан, гуаяково дърво и пралине създава опияняващ, сладък и дълготраен шлейф.",
                FragranceFamily: "Ориенталски Дървесен",
                Concentration: "EDP",
                BasePrice: 225.00m),

            "Parfums de Marly Delina Exclusif" => new(
                Notes: "Връхни: Бергамот, Личи, Круша, Тамян | Сърце: Турска роза, Уд, Тамян | База: Ванилия, Кехлибар, Дървесни нотки",
                Description: "Delina Exclusif е интензивна, по-богата и по-чувствена версия на легендарната Delina. Личи, круша и тамян отварят с екзотична свежест, сърцето разкрива дамаска роза и уд, а основата от ванилия и кехлибар осигурява луксозно дамско изражение на силата и женствеността.",
                FragranceFamily: "Флорален Плодов",
                Concentration: "EDP",
                BasePrice: 305.00m),

            "Maison Francis Kurkdjian Baccarat Rouge 540" => new(
                Notes: "Връхни: Пресен жасмин, Шафран | Сърце: Кедърово дърво, Горчиви бадеми | База: Суров мускус, Дъбов мъх, Амбра, Ванилия",
                Description: "Baccarat Rouge 540 е ароматният феномен на нашето поколение. Роден от сътрудничеството между Maison Francis Kurkdjian и кристалната династия Baccarat, той съчетава шафран, жасмин, кедър и горчиви бадеми в прочут, дълготраен аромат с уникален светещ характер.",
                FragranceFamily: "Флорален Амберов",
                Concentration: "EDP",
                BasePrice: 265.00m),

            "Jean Paul Gaultier Le Male" => new(
                Notes: "Връхни: Мента, Лавандула, Бергамот | Сърце: Канела, Кимион, Портокалов цвят, Салвия | База: Ванилия, Боб тонка, Кедър, Сандалово дърво",
                Description: "Легендарният ориенталски фужерен аромат, определил поколение. Мента, лавандула и бергамот отварят с освежаваща дързост, сърцето разкрива канела и портокалов цвят, а топлата основа от ванилия, кедър и сандалово дърво задържа дълго ароматния подпис в иконичния флакон-торс.",
                FragranceFamily: "Ориенталски Фужер",
                Concentration: "EDT",
                BasePrice: 72.00m),

            "Jean Paul Gaultier Le Male Elixir" => new(
                Notes: "Връхни: Лавандула, Бензоин | Сърце: Ванилия, Боб тонка | База: Мед, Тютюн, Мускус",
                Description: "Пламтящ и съблазнителен парфюмен еликсир, по-тъмна и богата итерация на иконичния Le Male. Лавандула и бензоин отварят с топлота, сърцето разкрива ванилия и боб тонка, а основата от мед и тютюн завършва опиянителното усещане в емблематичния златен флакон-торс.",
                FragranceFamily: "Ориенталски Фужер",
                Concentration: "Parfum",
                BasePrice: 95.00m),

            "Creed Aventus" => new(
                Notes: "Връхни: Бергамот, Черен касис, Ябълка, Лимон, Розов пипер | Сърце: Ананас, Пачули, Мароканска жасмина, Бреза | База: Мускус, Дъбов мъх, Кедър, Амброксан",
                Description: "Сензационният и емблематичен аромат на Creed — ода на силата, успеха и победата. Незабравими нотки на бергамот, черен касис, сочен ананас и опушена бреза се разкриват над основа от дъбов мъх, пачули и мускус. Любимец на ценители по целия свят от 2010 година.",
                FragranceFamily: "Плодов Шипър",
                Concentration: "EDP",
                BasePrice: 320.00m,
                RemoveOriginalPrice: true),

            "Creed Millésime Impérial" => new(
                Notes: "Връхни: Ирис, Лимон, Бергамот | Сърце: Морска сол, Морски акорд | База: Сицилиански мандарина, Мускус, Сандалово дърво, Кехлибар",
                Description: "Златният стандарт на нишовата парфюмерия. Свеж, морски и цитрусов шедьовър, пренасящ сетивата край слънчевите брегове на Сицилия. Морска сол, бергамот, ирис и мандарина над основа от мускус, сандалово дърво и кехлибар.",
                FragranceFamily: "Морски Цитрусов",
                Concentration: "EDP",
                BasePrice: 320.00m),

            "Creed Green Irish Tweed" => new(
                Notes: "Връхни: Лимон, Лимонова вербена | Сърце: Ирис, Теменужкови листа | База: Сандалово дърво, Кехлибар",
                Description: "Вечната елегантност на ирландската природа. Изтънчен и мъжествен аромат, съчетаващ свежестта на лимонова вербена и теменужкови листа с ирис, кадифена основа от сандалово дърво и сив кехлибар. Класика на Creed, обичана от поколения мъже с изтънчен вкус.",
                FragranceFamily: "Дървесен Фужер",
                Concentration: "EDP",
                BasePrice: 320.00m),

            "Creed Silver Mountain Water" => new(
                Notes: "Връхни: Бергамот, Мандарина, Нероли | Сърце: Зелен чай, Пъпки от черен касис | База: Сандалово дърво, Мускус, Петитгрен, Галбанум",
                Description: "Кристална свежест, вдъхновена от чистите алпийски потоци. Ободряващ планински полъх, съчетаващ бергамот, мандарина и нероли с зелен чай и пъпки от черен касис над основа от сандалово дърво, мускус и галбанум.",
                FragranceFamily: "Дървесен Аквален",
                Concentration: "EDP",
                BasePrice: 320.00m,
                RemoveOriginalPrice: true),

            _ => null
        };

        private record VariantCorrection(decimal Price, decimal? OriginalPrice = null);

        private static VariantCorrection? GetVariantCorrection(string productName, int sizeMl) =>
            (productName, sizeMl) switch
            {
                // Dior Sauvage Elixir — dior.com / sephora.fr
                ("Dior Sauvage Elixir", 60)  => new(169.00m),
                ("Dior Sauvage Elixir", 100) => new(247.00m),

                // Tom Ford Ombré Leather — tomfordbeauty.com
                ("Tom Ford Ombré Leather", 50)  => new(150.00m),
                ("Tom Ford Ombré Leather", 100) => new(210.00m),
                ("Tom Ford Ombré Leather", 150) => new(275.00m),

                // Tom Ford Black Orchid — tomfordbeauty.com
                ("Tom Ford Black Orchid", 50)  => new(150.00m),
                ("Tom Ford Black Orchid", 100) => new(210.00m),

                // Parfums de Marly — parfums-de-marly.com
                ("Parfums de Marly Layton",         75)  => new(225.00m),
                ("Parfums de Marly Layton",         125) => new(295.00m),
                ("Parfums de Marly Greenley",       75)  => new(225.00m),
                ("Parfums de Marly Greenley",       125) => new(295.00m),
                ("Parfums de Marly Althaïr",        75)  => new(225.00m),
                ("Parfums de Marly Althaïr",        125) => new(295.00m),
                ("Parfums de Marly Delina Exclusif", 75) => new(305.00m),

                // Carolina Herrera — carolinaherrera.com / flaconi.de
                ("Carolina Herrera Good Girl", 50)  => new(85.00m),
                ("Carolina Herrera Good Girl", 80)  => new(122.00m),
                ("Carolina Herrera Good Girl", 100) => new(153.00m),
                ("Carolina Herrera Bad Boy Extreme", 50)  => new(80.00m),
                ("Carolina Herrera Bad Boy Extreme", 100) => new(117.00m),

                // MFK Baccarat Rouge 540 — franciskurkdjian.com EU
                ("Maison Francis Kurkdjian Baccarat Rouge 540", 35)  => new(170.00m),
                ("Maison Francis Kurkdjian Baccarat Rouge 540", 70)  => new(265.00m),
                ("Maison Francis Kurkdjian Baccarat Rouge 540", 200) => new(525.00m),

                // Jean Paul Gaultier — jeanpaulgaultier.com / flaconi.de
                ("Jean Paul Gaultier Le Male",  75)  => new(72.00m),
                ("Jean Paul Gaultier Le Male",  125) => new(90.00m),
                ("Jean Paul Gaultier Le Male",  200) => new(120.00m),
                ("Jean Paul Gaultier Le Male Elixir", 75)  => new(95.00m),
                ("Jean Paul Gaultier Le Male Elixir", 125) => new(120.00m),
                ("Jean Paul Gaultier Le Male Elixir", 200) => new(155.00m),

                // Creed — creedfragrance.com
                ("Creed Aventus",             50)  => new(275.00m),
                ("Creed Aventus",             100) => new(320.00m),
                ("Creed Aventus",             250) => new(680.00m),
                ("Creed Millésime Impérial",  50)  => new(220.00m),
                ("Creed Millésime Impérial",  100) => new(320.00m),
                ("Creed Green Irish Tweed",   50)  => new(220.00m),
                ("Creed Green Irish Tweed",   100) => new(320.00m),
                ("Creed Silver Mountain Water", 50)  => new(220.00m),
                ("Creed Silver Mountain Water", 100) => new(320.00m),

                _ => null
            };
    }
}
