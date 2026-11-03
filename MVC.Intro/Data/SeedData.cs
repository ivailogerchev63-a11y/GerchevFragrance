using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    /// <summary>
    /// Seeds and keeps the product catalogue up-to-date.
    ///
    /// Strategy: instead of "if any products exist → skip everything", we check each
    /// product individually by ImageUrl (the one truly unique field per bottle photo).
    /// Products that already exist are left untouched; missing ones are inserted.
    /// This means re-running the app after adding new images always fills in the gaps.
    ///
    /// Image → Verified product:
    ///   Gemini_Generated_Image_62xyaw62xyaw62xy.jpg  → YSL Y EDP
    ///   Gemini_Generated_Image_izenq4izenq4izen.jpg  → Jean Paul Gaultier Le Male EDT
    ///   Gemini_Generated_Image_pmsttypmsttypmst.jpg  → Jean Paul Gaultier Le Male Elixir Parfum
    ///   images (1).jpg                               → Christian Dior Sauvage Elixir
    ///   images.jpg                                   → Tom Ford Ombré Leather Reserve Parfum
    ///   Gemini_Generated_Image_8993a18993a18993.jpg  → Creed Aventus EDP
    ///   Gemini_Generated_Image_ewjzp6ewjzp6ewjz.jpg  → Creed Millésime Impérial EDP
    ///   Gemini_Generated_Image_utb5m6utb5m6utb5.jpg  → Creed Green Irish Tweed EDP
    ///   Gemini_Generated_Image_xl3ywjxl3ywjxl3y.jpg  → Creed Silver Mountain Water EDP
    /// </summary>
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // Build the full catalogue definition
            var catalogue = BuildCatalogue();

            // Collect images already in the DB so we skip duplicates
            var existingImages = context.Products
                .Select(p => p.ImageUrl)
                .ToHashSet();

            var toAdd = catalogue
                .Where(p => !existingImages.Contains(p.ImageUrl))
                .ToList();

            if (toAdd.Count == 0) return;

            context.Products.AddRange(toAdd);
            context.SaveChanges();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static ProductSize Sz(int ml, decimal price,
                                       bool available = true, string? reason = null) =>
            new ProductSize
            {
                Id                = Guid.NewGuid(),
                Ml                = ml,
                Price             = price,
                IsAvailable       = available,
                UnavailableReason = reason
            };

        // ── Full catalogue ───────────────────────────────────────────────────────

        private static List<Product> BuildCatalogue() => new()
        {
            // ── 1 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Yves Saint Laurent Y Eau de Parfum
            // yslbeauty.com · fragrantica.com
            // Official sizes: 60 ml, 100 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Y",
                Brand           = "Yves Saint Laurent",
                Description     = "Смел, съвременен мъжки аромат, въплъщаващ свободолюбивия дух на YSL. " +
                                  "Открива се с живото ухапване на джинджифил и бергамот, разгръща " +
                                  "сърцето си в лавандула и здравец, и завършва с дълбоки дървесни " +
                                  "и тонка-бийн акорди. Интензивен, дълготраен и неустоимо модерен.",
                Price           = 179.99m,
                ImageUrl        = "/images/Gemini_Generated_Image_62xyaw62xyaw62xy.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 18,
                FragranceFamily = "Ароматен — Свеж",
                TopNotes        = "Джинджифил, Бергамот, Зелена ябълка",
                HeartNotes      = "Лавандула, Здравец, Градински чай",
                BaseNotes       = "Ветивер, Кедър, Тонка бийн",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  139.99m),
                    Sz(100, 179.99m),
                    Sz(150, 0, available: false,
                       reason: "YSL Y EDP не се произвежда в 150 ml. 150 ml е налично само в EDT версията.")
                }
            },

            // ── 2 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Jean Paul Gaultier Le Male Eau de Toilette
            // jeanpaulgaultier.com · fragrantica.com
            // Official sizes: 40 ml, 75 ml, 125 ml, 200 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Le Male",
                Brand           = "Jean Paul Gaultier",
                Description     = "Емблематичният мъжки аромат на Jean Paul Gaultier — морски, " +
                                  "лавандулов и топло ванилов. Иконичната бяло-зелена раирана торсо " +
                                  "бутилка е символ на поколение. Свежите начални ноти от мента " +
                                  "и бергамот отстъпват на сърце от лавандула и канела, завършващо " +
                                  "в кремообразна ванилия и сандалово дърво.",
                Price           = 119.99m,
                ImageUrl        = "/images/Gemini_Generated_Image_izenq4izenq4izen.jpg",
                Concentration   = "EDT",
                Size            = "125ml",
                StockQuantity   = 22,
                FragranceFamily = "Ориенталски — Флорален",
                TopNotes        = "Мента, Лавандула, Бергамот",
                HeartNotes      = "Канела, Кимион, Лавандула, Градински чай, Портокалов цвят",
                BaseNotes       = "Ванилия, Тонка бийн, Сандалово дърво, Кедър",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  0, available: false,
                       reason: "Le Male EDT официално се предлага в 40 ml, 75 ml, 125 ml и 200 ml. 50 ml не е стандартен размер."),
                    Sz(100, 0, available: false,
                       reason: "Le Male EDT официално се предлага в 75 ml и 125 ml. 100 ml не е стандартен размер."),
                    Sz(150, 0, available: false,
                       reason: "Le Male EDT не се произвежда в 150 ml. Следващият официален размер е 200 ml.")
                }
            },

            // ── 3 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Jean Paul Gaultier Le Male Elixir Parfum
            // jeanpaulgaultier.com (official)
            // Official sizes: 75 ml, 125 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Le Male Elixir",
                Brand           = "Jean Paul Gaultier",
                Description     = "Ултра-интензивната версия на класическия Le Male. Топла, сладка " +
                                  "и животинска — тонка бийнът, ванилията и ладанът създават " +
                                  "хипнотизиращ и дълготраен аромат. Иконичната торсо бутилка е " +
                                  "преработена в злато с бордо ивици, символизирайки еликсир за " +
                                  "съблазняване. Парфюмова концентрация за максимална интензивност.",
                Price           = 189.99m,
                ImageUrl        = "/images/Gemini_Generated_Image_pmsttypmsttypmst.jpg",
                Concentration   = "Parfum",
                Size            = "125ml",
                StockQuantity   = 9,
                FragranceFamily = "Ориенталски — Сладък",
                TopNotes        = "Мента, Лавандула",
                HeartNotes      = "Лавандула, Ладан, Бензоин",
                BaseNotes       = "Тонка бийн, Ванилия",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  0, available: false,
                       reason: "Le Male Elixir Parfum се произвежда в 75 ml и 125 ml. 50 ml не е официален размер."),
                    Sz(100, 0, available: false,
                       reason: "Le Male Elixir Parfum се произвежда в 75 ml и 125 ml. 100 ml не е официален размер."),
                    Sz(150, 0, available: false,
                       reason: "Le Male Elixir Parfum не се произвежда в 150 ml. Официалните размери са 75 ml и 125 ml.")
                }
            },

            // ── 4 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Christian Dior Sauvage Elixir
            // dior.com (official, multiple regions)
            // Official size: 60 ml only
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Sauvage Elixir",
                Brand           = "Christian Dior",
                Description     = "Най-интензивната итерация на Sauvage — изключително концентриран " +
                                  "аромат с безпрецедентна дълготрайност. Богати топли подправки " +
                                  "от канела и кардамон, лавандула и луксозни дървесни акорди " +
                                  "оформят мощна, завладяваща следа. Произвежда се само в 60 ml — " +
                                  "прецизен еликсир в уникален флакон.",
                Price           = 259.99m,
                ImageUrl        = "/images/images (1).jpg",
                Concentration   = "Elixir",
                Size            = "60ml",
                StockQuantity   = 7,
                FragranceFamily = "Ароматен — Дървесен",
                TopNotes        = "Канела, Кардамон, Грейпфрут",
                HeartNotes      = "Лавандула, Мускатов орех",
                BaseNotes       = "Амброксан, Сандалово дърво, Кашмирано дърво",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  0, available: false,
                       reason: "Dior Sauvage Elixir се произвежда само в 60 ml. 50 ml не е официален размер."),
                    Sz(100, 0, available: false,
                       reason: "Dior Sauvage Elixir се произвежда само в 60 ml. 100 ml не е официален размер."),
                    Sz(150, 0, available: false,
                       reason: "Dior Sauvage Elixir се произвежда само в 60 ml. 150 ml не е официален размер.")
                }
            },

            // ── 5 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Tom Ford Ombré Leather Reserve Parfum
            // tomfordbeauty.com · sephora.com
            // Official sizes: 50 ml, 100 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Ombre Leather Reserve",
                Brand           = "Tom Ford",
                Description     = "Ombre Leather Reserve е парфюмовата интерпретация на " +
                                  "оригиналния кожен шедьовър — по-дълбока, по-смолиста и " +
                                  "по-завладяваща. Ароматните сребристозелени хвойнови игли " +
                                  "улавят духа на пустинните планини. Кипарисът добавя свежа " +
                                  "борова нота, виолетовият лист внася зелена светлина, а " +
                                  "черната кожа и пинйоново дърво финализират един незабравим аромат.",
                Price           = 329.99m,
                ImageUrl        = "/images/images.jpg",
                Concentration   = "Parfum",
                Size            = "100ml",
                StockQuantity   = 11,
                FragranceFamily = "Кожен — Дървесен",
                TopNotes        = "Хвойна, Кипарис, Черен пипер",
                HeartNotes      = "Виолетов лист, Черна кожа",
                BaseNotes       = "Пинйоново дърво, Сандалово дърво, Мирра",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  249.99m),
                    Sz(100, 329.99m),
                    Sz(150, 0, available: false,
                       reason: "Tom Ford Ombre Leather Reserve Parfum не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // ── 6 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Creed Aventus Eau de Parfum
            // creedfragrance.com · fragrantica.com
            // Official sizes: 50 ml, 100 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Aventus",
                Brand           = "Creed",
                Description     = "Смел и уверен — Aventus е ултимативният ароматен израз, " +
                                  "издържал изпитанието на времето. Открива се с пресен " +
                                  "калабрийски бергамот в хармония с иконичния акорд от ананас. " +
                                  "Сърцето от бреза и пачули отстъпва на база от дъбов мъх, " +
                                  "амбергрис и мускус. Един от най-харесваните мъжки парфюми " +
                                  "в историята.",
                Price           = 549.99m,
                ImageUrl        = "/images/Gemini_Generated_Image_8993a18993a18993.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 10,
                FragranceFamily = "Плодово — Дървесен",
                TopNotes        = "Бергамот, Ананас, Касис, Ябълка",
                HeartNotes      = "Бреза, Пачули, Хвойнови плодове, Жасмин",
                BaseNotes       = "Мускус, Дъбов мъх, Амбергрис, Ванилия",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  399.99m),
                    Sz(100, 549.99m),
                    Sz(150, 0, available: false,
                       reason: "Creed Aventus EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // ── 7 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Creed Millesime Imperial Eau de Parfum
            // creedfragrance.com · fragrantica.com
            // Official sizes: 50 ml, 100 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Millesime Imperial",
                Brand           = "Creed",
                Description     = "Пресен и повдигащ духа, Millesime Imperial излъчва " +
                                  "блясък с ярки цитруси и солен морски акорд над топла " +
                                  "златна основа. Вдъхновен от слънчевата Сицилия и " +
                                  "лазурното Средиземно море. Унисекс аромат, стартирал " +
                                  "през 1994 г., продължава да печели сърца и до днес.",
                Price           = 499.99m,
                ImageUrl        = "/images/Gemini_Generated_Image_ewjzp6ewjzp6ewjz.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 8,
                FragranceFamily = "Свеж — Цитрусов — Морски",
                TopNotes        = "Морска сол, Плодови ноти",
                HeartNotes      = "Сицилиански лимон, Бергамот, Ирис, Мандарина",
                BaseNotes       = "Морски ноти, Мускус, Дървесни ноти",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  359.99m),
                    Sz(100, 499.99m),
                    Sz(150, 0, available: false,
                       reason: "Creed Millesime Imperial EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // ── 8 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Creed Green Irish Tweed Eau de Parfum
            // creedfragrance.com · fragrantica.com
            // Official sizes: 50 ml, 100 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Green Irish Tweed",
                Brand           = "Creed",
                Description     = "Зелен, свеж и неподвластен на времето — Green Irish Tweed " +
                                  "е прецизно изработено фужерно произведение. Вдъхновен от " +
                                  "изумруденозелените ирландски пейзажи, той съчетава свежи " +
                                  "лимонови и вербенови ноти с цветя от ирис и завършва с " +
                                  "топло сандалово дърво и амбергрис. Роден през 1985 г.",
                Price           = 479.99m,
                ImageUrl        = "/images/Gemini_Generated_Image_utb5m6utb5m6utb5.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 12,
                FragranceFamily = "Ароматен — Флорален — Зелен",
                TopNotes        = "Лимон, Вербена",
                HeartNotes      = "Ирис, Теменужени листа",
                BaseNotes       = "Сандалово дърво, Амбергрис",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  349.99m),
                    Sz(100, 479.99m),
                    Sz(150, 0, available: false,
                       reason: "Creed Green Irish Tweed EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // ── 9 ─────────────────────────────────────────────────────────────────
            // VERIFIED: Creed Silver Mountain Water Eau de Parfum
            // creedfragrance.com · decantboutique.com
            // Official sizes: 30 ml, 50 ml, 100 ml, 240 ml, 500 ml
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Silver Mountain Water",
                Brand           = "Creed",
                Description     = "Ултимативната алпийска есенция — Silver Mountain Water " +
                                  "пресъздава необятната красота на Алпите. Майсторски " +
                                  "съчетание от остри цитруси, зелен чай и горски плодове, " +
                                  "закотвено от дървесини и мускус. Чистотата на ледените " +
                                  "алпийски потоци в един безвременен аромат от 1995 г.",
                Price           = 449.99m,
                ImageUrl        = "/images/Gemini_Generated_Image_xl3ywjxl3ywjxl3y.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 14,
                FragranceFamily = "Свеж — Цитрусов — Зелен",
                TopNotes        = "Бергамот, Мандарина, Нероли",
                HeartNotes      = "Зелен чай, Касис",
                BaseNotes       = "Сандалово дърво, Мускус, Петитгрен, Галбанум",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  329.99m),
                    Sz(100, 449.99m),
                    Sz(150, 0, available: false,
                       reason: "Creed Silver Mountain Water EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            }
        };
    }
}
