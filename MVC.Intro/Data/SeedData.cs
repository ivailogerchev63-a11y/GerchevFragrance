using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    /// <summary>
    /// Authoritative perfume catalogue for Gerchev Fragrance.
    ///
    /// IDENTITY RULE: products are matched to the database by the natural key
    /// (Brand + Name + Concentration) via <see cref="CatalogueSync"/>, NOT by
    /// image filename. Every image below was identified by VISUALLY inspecting
    /// the actual bottle/packaging text — filenames in the source folder were
    /// found to be scrambled and are never trusted as proof of identity.
    ///
    /// All prices are in EUR (€). Prices were verified against official brand
    /// sites / authorised European retailers (sources recorded in comments).
    /// </summary>
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // On a brand-new database simply insert the whole catalogue.
            if (!context.Products.Any())
            {
                context.Products.AddRange(BuildCatalogue());
                context.SaveChanges();
            }
            // Existing databases are reconciled by CatalogueSync (called from Program.cs).
        }

        /// <summary>Exposed so the reconciliation step can read the canonical catalogue.</summary>
        public static List<Product> Catalogue() => BuildCatalogue();

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
            // ════════════════════════ EXISTING PRODUCTS (images corrected) ════════════════════════

            // YSL Y EDP — image was broken (62xyaw missing); correct verified photo assigned.
            // Source: yslbeauty.com · fragrantica.com. Official sizes 60/100 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Y",
                Brand           = "Yves Saint Laurent",
                Description     = "Смел, съвременен мъжки аромат, въплъщаващ свободолюбивия дух на YSL. " +
                                  "Открива се с живото ухапване на джинджифил и бергамот, разгръща " +
                                  "сърцето си в лавандула и здравец, и завършва с дълбоки дървесни " +
                                  "и тонка-бийн акорди. Интензивен, дълготраен и неустоимо модерен.",
                Price           = 92.03m,
                ImageUrl        = "/images/YSL_Y_verified.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 18,
                FragranceFamily = "Ароматен — Свеж",
                TopNotes        = "Джинджифил, Бергамот, Зелена ябълка",
                HeartNotes      = "Лавандула, Здравец, Градински чай",
                BaseNotes       = "Ветивер, Кедър, Тонка бийн",
                Sizes = new List<ProductSize>
                {
                    Sz(60,  85.00m),
                    Sz(100, 92.03m),
                    Sz(150, 0, available: false,
                       reason: "YSL Y EDP не се произвежда в 150 ml. Официалните размери са 60 ml и 100 ml.")
                }
            },

            // JPG Le Male EDT — image was wrong (izenq4 was actually YSL Y). No standalone
            // Le Male EDT photo was available in the folder, so the image stays flagged.
            // Source: jeanpaulgaultier.com · fragrantica.com. Official sizes 40/75/125/200 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Le Male",
                Brand           = "Jean Paul Gaultier",
                Description     = "Емблематичният мъжки аромат на Jean Paul Gaultier — морски, " +
                                  "лавандулов и топло ванилов. Иконичната бяло-синя раирана торсо " +
                                  "бутилка е символ на поколение. Свежите начални ноти от мента " +
                                  "и бергамот отстъпват на сърце от лавандула и канела, завършващо " +
                                  "в кремообразна ванилия и сандалово дърво.",
                Price           = 61.35m,
                // No verified standalone Le Male EDT photo in the source folder → placeholder fallback.
                ImageUrl        = "/images/JPG_Le_Male_verified_PENDING.jpg",
                Concentration   = "EDT",
                Size            = "125ml",
                StockQuantity   = 22,
                FragranceFamily = "Ориенталски — Флорален",
                TopNotes        = "Мента, Лавандула, Бергамот",
                HeartNotes      = "Канела, Кимион, Лавандула, Градински чай, Портокалов цвят",
                BaseNotes       = "Ванилия, Тонка бийн, Сандалово дърво, Кедър",
                Sizes = new List<ProductSize>
                {
                    Sz(75,  61.35m),
                    Sz(125, 78.00m),
                    Sz(150, 0, available: false,
                       reason: "Le Male EDT не се произвежда в 150 ml. Официалните размери са 40/75/125/200 ml.")
                }
            },

            // JPG Le Male Elixir Parfum — image verified correct (pmstty).
            // Source: jeanpaulgaultier.com. Official sizes 75/125 ml.
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
                Price           = 97.14m,
                ImageUrl        = "/images/JPG_Le_Male_Elixir_verified.jpg",
                Concentration   = "Parfum",
                Size            = "125ml",
                StockQuantity   = 9,
                FragranceFamily = "Ориенталски — Сладък",
                TopNotes        = "Мента, Лавандула",
                HeartNotes      = "Лавандула, Ладан, Бензоин",
                BaseNotes       = "Тонка бийн, Ванилия",
                Sizes = new List<ProductSize>
                {
                    Sz(75,  82.00m),
                    Sz(125, 97.14m),
                    Sz(150, 0, available: false,
                       reason: "Le Male Elixir Parfum се произвежда в 75 ml и 125 ml. 150 ml не е официален размер.")
                }
            },

            // Dior Sauvage Elixir — image was broken; VERIFIED correct Elixir photo assigned
            // (navy bottle reading "SAUVAGE ELIXIR / Dior"). Distinct from EDT/EDP/Parfum.
            // Source: dior.com (60 ml = 244,00 €).
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Sauvage Elixir",
                Brand           = "Christian Dior",
                Description     = "Най-интензивната итерация на Sauvage — изключително концентриран " +
                                  "аромат с безпрецедентна дълготрайност. Богати топли подправки " +
                                  "от канела и кардамон, есенция от лавандула и луксозни дървесни " +
                                  "акорди оформят мощна, завладяваща следа. Произвежда се в уникален " +
                                  "флакон с крушовиден дизайн.",
                Price           = 244.00m,
                ImageUrl        = "/images/Dior_Sauvage_Elixir_verified.jpg",
                Concentration   = "Elixir",
                Size            = "60ml",
                StockQuantity   = 7,
                FragranceFamily = "Ароматен — Дървесен — Подправки",
                TopNotes        = "Грейпфрут, Канела, Кардамон, Индийско орехче",
                HeartNotes      = "Лавандула, Женско биле (сладник)",
                BaseNotes       = "Сандалово дърво, Амбра, Пачули, Ароматни дървесни ноти",
                Sizes = new List<ProductSize>
                {
                    Sz(60,  244.00m),
                    Sz(100, 0, available: false,
                       reason: "Dior Sauvage Elixir се предлага основно в 60 ml. 100 ml не е стандартен размер."),
                    Sz(150, 0, available: false,
                       reason: "Dior Sauvage Elixir не се произвежда в 150 ml.")
                }
            },

            // Tom Ford Ombré Leather RESERVE (Parfum, 2026) — a SEPARATE product from the
            // standard Ombré Leather EDP. Its image stays flagged (no Reserve photo in folder).
            // Source: fragrantica.com (Ombré Leather Reserve, 2026).
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Ombre Leather Reserve",
                Brand           = "Tom Ford",
                Description     = "Ombre Leather Reserve е по-дълбока, по-смолиста интерпретация на " +
                                  "кожения шедьовър на Tom Ford. Ароматните хвойнови и кипарисови " +
                                  "ноти улавят духа на пустинните планини, докато черната кожа и " +
                                  "бензоинът оформят плътна, мъжествена основа.",
                Price           = 168.72m,
                // Distinct from standard Ombré Leather EDP; no verified Reserve photo available.
                ImageUrl        = "/images/Tom_Ford_Ombre_Leather_Reserve_PENDING.jpg",
                Concentration   = "Parfum",
                Size            = "100ml",
                StockQuantity   = 11,
                FragranceFamily = "Кожен — Дървесен",
                TopNotes        = "Кипарис, Хвойна, Черен пипер",
                HeartNotes      = "Зелен акорд, Цветни ноти, Pepperwood™, Ветивер",
                BaseNotes       = "Черна кожа, Бензоин",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  127.82m),
                    Sz(100, 168.72m),
                    Sz(150, 0, available: false,
                       reason: "Tom Ford Ombre Leather Reserve не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // Creed Aventus EDP — image verified correct (8993a1).
            // Source: creedfragrance.com · fragrantica.com. Official sizes 50/100 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Aventus",
                Brand           = "Creed",
                Description     = "Смел и уверен — Aventus е ултимативният ароматен израз, " +
                                  "издържал изпитанието на времето. Открива се с пресен " +
                                  "калабрийски бергамот в хармония с иконичния акорд от ананас. " +
                                  "Сърцето от бреза и пачули отстъпва на база от дъбов мъх, " +
                                  "амбергрис и мускус.",
                Price           = 281.21m,
                ImageUrl        = "/images/Creed_Aventus_verified.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 10,
                FragranceFamily = "Плодово — Дървесен",
                TopNotes        = "Бергамот, Ананас, Касис, Ябълка",
                HeartNotes      = "Бреза, Пачули, Хвойнови плодове, Жасмин",
                BaseNotes       = "Мускус, Дъбов мъх, Амбергрис, Ванилия",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  204.51m),
                    Sz(100, 281.21m),
                    Sz(150, 0, available: false,
                       reason: "Creed Aventus EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // Creed Millésime Impérial EDP — image verified correct (ewjzp6).
            // Source: creedfragrance.com · fragrantica.com. Official sizes 50/100 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Millesime Imperial",
                Brand           = "Creed",
                Description     = "Пресен и повдигащ духа, Millesime Imperial излъчва " +
                                  "блясък с ярки цитруси и солен морски акорд над топла " +
                                  "златна основа. Вдъхновен от слънчевата Сицилия и " +
                                  "лазурното Средиземно море. Унисекс аромат от 1995 г.",
                Price           = 255.64m,
                ImageUrl        = "/images/Creed_Millesime_Imperial_verified.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 8,
                FragranceFamily = "Свеж — Цитрусов — Морски",
                TopNotes        = "Морска сол, Плодови ноти",
                HeartNotes      = "Сицилиански лимон, Бергамот, Ирис, Мандарина",
                BaseNotes       = "Морски ноти, Мускус, Дървесни ноти",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  184.06m),
                    Sz(100, 255.64m),
                    Sz(150, 0, available: false,
                       reason: "Creed Millesime Imperial EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // Creed Green Irish Tweed EDP — image was broken (utb5m6 missing); no verified
            // GIT photo in folder, so the image stays flagged.
            // Source: creedfragrance.com · fragrantica.com. Official sizes 50/100 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Green Irish Tweed",
                Brand           = "Creed",
                Description     = "Зелен, свеж и неподвластен на времето — Green Irish Tweed " +
                                  "е прецизно изработено фужерно произведение. Съчетава свежи " +
                                  "лимонови и вербенови ноти с цветя от ирис и завършва с " +
                                  "топло сандалово дърво и амбергрис. Роден през 1985 г.",
                Price           = 245.41m,
                ImageUrl        = "/images/Creed_Green_Irish_Tweed_PENDING.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 12,
                FragranceFamily = "Ароматен — Флорален — Зелен",
                TopNotes        = "Лимон, Вербена",
                HeartNotes      = "Ирис, Теменужени листа",
                BaseNotes       = "Сандалово дърво, Амбергрис",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  178.95m),
                    Sz(100, 245.41m),
                    Sz(150, 0, available: false,
                       reason: "Creed Green Irish Tweed EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // Creed Silver Mountain Water EDP — image verified correct (xl3ywj).
            // Source: creedfragrance.com · fragrantica.com.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Silver Mountain Water",
                Brand           = "Creed",
                Description     = "Ултимативната алпийска есенция — Silver Mountain Water " +
                                  "пресъздава необятната красота на Алпите. Майсторско " +
                                  "съчетание от остри цитруси, зелен чай и горски плодове, " +
                                  "закотвено от дървесни ноти и мускус. Безвременен аромат от 1995 г.",
                Price           = 230.08m,
                ImageUrl        = "/images/Creed_Silver_Mountain_Water_verified.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 14,
                FragranceFamily = "Свеж — Цитрусов — Зелен",
                TopNotes        = "Бергамот, Мандарина, Нероли",
                HeartNotes      = "Зелен чай, Касис",
                BaseNotes       = "Сандалово дърво, Мускус, Петитгрен, Галбанум",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  168.72m),
                    Sz(100, 230.08m),
                    Sz(150, 0, available: false,
                       reason: "Creed Silver Mountain Water EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // ════════════════════════ NEW PRODUCTS (verified from images) ════════════════════════

            // Tom Ford Ombré Leather EDP (2018) — standard EDP, bottle reads just "OMBRÉ LEATHER".
            // Source: tomfordbeauty.co.uk · fragrantica.com. Official sizes 50/100 ml.
            // Price: Sephora US ~$156 (100 ml) → EU retail ≈ €155 (100 ml).
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Ombre Leather",
                Brand           = "Tom Ford",
                Description     = "Ombré Leather улавя духа на американския запад — прах, носен от " +
                                  "вятъра, кожа до кожа. Чувствеността на богата черна кожа е " +
                                  "текстурирана с пачули и ветивер, докато флоралните тонове на " +
                                  "жасмин самбак пресъздават дивата пустинна красота, а амбра и мъх " +
                                  "имитират топлината на скалите и пясъка.",
                Price           = 155.00m,
                ImageUrl        = "/images/Tom_Ford_Ombre_Leather_EDP_verified.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 13,
                FragranceFamily = "Кожен — Флорален",
                TopNotes        = "Кардамон",
                HeartNotes      = "Черна кожа, Жасмин самбак",
                BaseNotes       = "Амбра, Мъх, Пачули",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  118.00m),
                    Sz(100, 155.00m),
                    Sz(150, 0, available: false,
                       reason: "Tom Ford Ombré Leather EDP не се произвежда в 150 ml. Официалните размери са 50 ml и 100 ml.")
                }
            },

            // Tom Ford Black Orchid EDP — bottle reads "TOM FORD BLACK ORCHID".
            // Source: tomfordbeauty.com · fragrantica.com. Sizes 30/50/100 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Black Orchid",
                Brand           = "Tom Ford",
                Description     = "Луксозен и чувствен аромат с богати, тъмни акорди. Black Orchid " +
                                  "се открива с черен трюфел и иланг-иланг, докоснати от бергамот " +
                                  "и касис. Сърцето разкрива черна орхидея, подправки и пикантни " +
                                  "ноти, а основата от мексикански шоколад, пачули, ванилия, тамян " +
                                  "и сандалово дърво създава незабравима следа.",
                Price           = 165.00m,
                ImageUrl        = "/images/Tom_Ford_Black_Orchid_verified.jpg",
                Concentration   = "EDP",
                Size            = "100ml",
                StockQuantity   = 10,
                FragranceFamily = "Ориенталски — Флорален",
                TopNotes        = "Черен трюфел, Гардения, Касис, Иланг-иланг, Бергамот",
                HeartNotes      = "Орхидея, Подправки, Плодови ноти, Лотос",
                BaseNotes       = "Мексикански шоколад, Пачули, Ванилия, Тамян, Амбра, Сандалово дърво",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  120.00m),
                    Sz(100, 165.00m),
                    Sz(150, 0, available: false,
                       reason: "Tom Ford Black Orchid EDP не се произвежда в 150 ml. Официалните размери са 30/50/100 ml.")
                }
            },

            // Maison Francis Kurkdjian Baccarat Rouge 540 EDP — red-label clear bottle.
            // Source: franciskurkdjian.com (official). Sizes 35/70/200 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Baccarat Rouge 540",
                Brand           = "Maison Francis Kurkdjian",
                Description     = "Поетична алхимия, родена от срещата на Maison Francis Kurkdjian " +
                                  "и Baccarat. Легнал върху кожата като амбров, флорален и дървесен " +
                                  "полъх — ефирен жасмин и сияен шафран подсилват минералните ноти " +
                                  "на амбергрис и дървесните тонове на свежо срязан кедър. " +
                                  "Графичен, луксозен и изключително концентриран подпис.",
                Price           = 320.00m,
                ImageUrl        = "/images/MFK_Baccarat_Rouge_540_verified.jpg",
                Concentration   = "EDP",
                Size            = "70ml",
                StockQuantity   = 6,
                FragranceFamily = "Амбров — Флорален — Дървесен",
                TopNotes        = "Шафран, Жасмин",
                HeartNotes      = "Амбров дървесен акорд, Амбергрис, Хедион",
                BaseNotes       = "Смола от ела, Кедър, Захар, Амброксан, Дъбов мъх",
                Sizes = new List<ProductSize>
                {
                    Sz(35,  195.00m),
                    Sz(70,  320.00m),
                    Sz(150, 0, available: false,
                       reason: "Baccarat Rouge 540 EDP се произвежда в 35/70/200 ml. 150 ml не е официален размер.")
                }
            },

            // Parfums de Marly Layton EDP — navy bottle, horses crest "PARFUMS de MARLY PARIS".
            // Source: parfums-de-marly.com (125 ml = 225 €). Sizes 75/125/200 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Layton",
                Brand           = "Parfums de Marly",
                Description     = "Елегантен и многолик унисекс аромат. Layton съчетава свежестта " +
                                  "на ябълка, лавандула и бергамот с флорално сърце от жасмин, " +
                                  "виолетка и здравец, което се разгръща в топла, сладка основа от " +
                                  "ванилия, карамелизирано кафе и благородни дървесни ноти. " +
                                  "Интензивен и неустоимо изискан.",
                Price           = 225.00m,
                ImageUrl        = "/images/Parfums_de_Marly_Layton_verified.jpg",
                Concentration   = "EDP",
                Size            = "125ml",
                StockQuantity   = 9,
                FragranceFamily = "Ориенталски — Флорален",
                TopNotes        = "Ябълка, Лавандула, Бергамот, Мандарина",
                HeartNotes      = "Жасмин, Виолетка, Здравец",
                BaseNotes       = "Ванилия, Кардамон, Сандалово дърво, Гаяк, Пачули, Кумарин",
                Sizes = new List<ProductSize>
                {
                    Sz(75,  170.00m),
                    Sz(125, 225.00m),
                    Sz(150, 0, available: false,
                       reason: "Parfums de Marly Layton се произвежда в 75/125/200 ml. 150 ml не е официален размер.")
                }
            },

            // Parfums de Marly Greenley EDP — green bottle, horses crest.
            // Source: parfums-de-marly.com (125 ml = 225 €) · fragrantica.com. Sizes 75/125 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Greenley",
                Brand           = "Parfums de Marly",
                Description     = "Свеж, жизнен и ведър унисекс аромат, пресъздаващ чистата енергия " +
                                  "и свежия полъх на пролетта. Greenley се открива с искрящ взрив от " +
                                  "зелена ябълка, сицилиански бергамот и мандарина, разгръща сърце от " +
                                  "петитгрен, кедър и помароза, и завършва с топла основа от амброксан, " +
                                  "мускус и пачули.",
                Price           = 225.00m,
                ImageUrl        = "/images/Parfums_de_Marly_Greenley_verified.jpg",
                Concentration   = "EDP",
                Size            = "125ml",
                StockQuantity   = 8,
                FragranceFamily = "Свеж — Дървесен — Зелен",
                TopNotes        = "Зелена ябълка, Калабрийски бергамот, Мандарина",
                HeartNotes      = "Петитгрен, Кедър, Кашмеран, Помароза, Виолетов лист",
                BaseNotes       = "Дъбов мъх, Мускус, Амбервуд, Пачули",
                Sizes = new List<ProductSize>
                {
                    Sz(75,  170.00m),
                    Sz(125, 225.00m),
                    Sz(150, 0, available: false,
                       reason: "Parfums de Marly Greenley се произвежда в 75 ml и 125 ml. 150 ml не е официален размер.")
                }
            },

            // Parfums de Marly Althaïr EDP — caramel/tan bottle, horses crest "ALTHAIR".
            // Source: parfums-de-marly.com (125 ml = 225 €) · libertylondon.com. Sizes 75/125 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Althair",
                Brand           = "Parfums de Marly",
                Description     = "Елегантен, щедър и харизматичен — Althaïr съчетава мекотата на " +
                                  "бурбонска ванилия, насладата на пралине и сиянието на портокалов " +
                                  "цвят. Пикантна канела и кардамон внасят топлина, докато кремообразни " +
                                  "дървесни ноти и амбра обгръщат аромата в чувствена, трайна следа.",
                Price           = 225.00m,
                ImageUrl        = "/images/Parfums_de_Marly_Althair_verified.jpg",
                Concentration   = "EDP",
                Size            = "125ml",
                StockQuantity   = 7,
                FragranceFamily = "Ориенталски — Сладък — Гурме",
                TopNotes        = "Бергамот, Портокалов цвят, Канела, Кардамон",
                HeartNotes      = "Елеми, Бурбонска ванилия",
                BaseNotes       = "Амброксан, Гаяково дърво, Пралине, Мускус",
                Sizes = new List<ProductSize>
                {
                    Sz(75,  170.00m),
                    Sz(125, 225.00m),
                    Sz(150, 0, available: false,
                       reason: "Parfums de Marly Althaïr се произвежда в 75 ml и 125 ml. 150 ml не е официален размер.")
                }
            },

            // Parfums de Marly Delina Exclusif EDP — pink bottle with tassel "DELINA EXCLUSIF".
            // Source: parfums-de-marly.com. Sizes 30/75 ml. (75 ml ≈ €385 EU retail.)
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Delina Exclusif",
                Brand           = "Parfums de Marly",
                Description     = "Пищна, наситена роза при здрач. Delina Exclusif е по-дълбока, " +
                                  "по-интензивна интерпретация на емблематичната Delina — дамаска роза " +
                                  "с обгръщаща, дръзка и незабравима следа. Личи, круша и бергамот " +
                                  "водят към турска роза и тамян, а ванилия, амбра и дървесни ноти " +
                                  "оформят чувствена, луксозна основа.",
                Price           = 385.00m,
                ImageUrl        = "/images/Parfums_de_Marly_Delina_Exclusif_verified.jpg",
                Concentration   = "EDP",
                Size            = "75ml",
                StockQuantity   = 5,
                FragranceFamily = "Флорален — Ориенталски — Роза",
                TopNotes        = "Бергамот, Личи, Круша, Тамян",
                HeartNotes      = "Турска роза, Уд, Тамян",
                BaseNotes       = "Ванилия, Амбра, Дървесни ноти",
                Sizes = new List<ProductSize>
                {
                    Sz(30,  195.00m),
                    Sz(75,  385.00m),
                    Sz(150, 0, available: false,
                       reason: "Parfums de Marly Delina Exclusif се произвежда в 30 ml и 75 ml. 150 ml не е официален размер.")
                }
            },

            // Carolina Herrera Good Girl EDP — midnight-blue stiletto bottle.
            // Source: carolinaherrera.com · sephora.com. Sizes 30/50/80 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Good Girl",
                Brand           = "Carolina Herrera",
                Description     = "Чувствен амбров флорален аромат, който възпява двойствеността на " +
                                  "съвременната жена, в иконичната бутилка стилето в среднощно синьо. " +
                                  "Ярък жасмин и кремообразна тубероза контрастират с топлината на " +
                                  "печен тонка боб и какао, докато бадем и кафе добавят смела " +
                                  "жизненост.",
                Price           = 125.00m,
                ImageUrl        = "/images/Carolina_Herrera_Good_Girl_verified.jpg",
                Concentration   = "EDP",
                Size            = "80ml",
                StockQuantity   = 15,
                FragranceFamily = "Амбров — Флорален",
                TopNotes        = "Бадем, Кафе, Бергамот, Лимон",
                HeartNotes      = "Тубероза, Жасмин самбак, Ирис, Портокалов цвят",
                BaseNotes       = "Тонка боб, Какао, Сандалово дърво, Ванилия",
                Sizes = new List<ProductSize>
                {
                    Sz(30,  78.00m),
                    Sz(50,  98.00m),
                    Sz(80,  125.00m),
                    Sz(150, 0, available: false,
                       reason: "Carolina Herrera Good Girl EDP се произвежда в 30/50/80 ml. 150 ml не е официален размер.")
                }
            },

            // Carolina Herrera Bad Boy EDT — black/gold lightning-bolt bottle.
            // Source: carolinaherrera.com · sephora.com. Sizes 50/100/150 ml.
            new Product
            {
                Id              = Guid.NewGuid(),
                Name            = "Bad Boy",
                Brand           = "Carolina Herrera",
                Description     = "Наелектризиращ амбров ароматен парфюм, който възпява бунтаря в " +
                                  "мъжа, в иконичната бутилка-светкавица. Свеж градински чай и " +
                                  "искрящ пипер с бергамот контрастират с топъл тонка боб, богато " +
                                  "какао и земен ветивер, за смел, магнетичен и чувствен аромат.",
                Price           = 120.00m,
                ImageUrl        = "/images/Carolina_Herrera_Bad_Boy_verified.jpg",
                Concentration   = "EDT",
                Size            = "100ml",
                StockQuantity   = 16,
                FragranceFamily = "Ориенталски — Ароматен",
                TopNotes        = "Градински чай, Черен и бял пипер, Зелен бергамот",
                HeartNotes      = "Кедър",
                BaseNotes       = "Тонка боб, Какао, Ветивер, Амбров дървесен акорд",
                Sizes = new List<ProductSize>
                {
                    Sz(50,  88.00m),
                    Sz(100, 120.00m),
                    Sz(150, 145.00m)
                }
            }
        };
    }
}
