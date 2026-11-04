using Microsoft.EntityFrameworkCore;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    /// <summary>
    /// Idempotent reconciliation of the live database against the canonical
    /// catalogue defined in <see cref="SeedData"/>.
    ///
    /// Matching key: Brand + Name + Concentration (case-insensitive). This is the
    /// product's natural identity — NOT the image filename (filenames in the source
    /// folder were scrambled and are never trusted).
    ///
    /// Behaviour:
    ///   • Existing product (same natural key) → update ImageUrl, Price, Description,
    ///     fragrance family, notes and size variants to the verified values.
    ///   • New product (no match)             → insert it.
    ///   • Products in the DB that are NOT in the catalogue are LEFT UNTOUCHED
    ///     (nothing is ever deleted — orders/users/newsletter data are never affected).
    ///
    /// Running this every startup is safe: a second run finds everything already
    /// matching and makes no changes.
    /// </summary>
    public static class CatalogueSync
    {
        private static string Key(string? brand, string? name, string? conc) =>
            $"{brand?.Trim().ToLowerInvariant()}|{name?.Trim().ToLowerInvariant()}|{conc?.Trim().ToLowerInvariant()}";

        public static void Reconcile(AppDbContext context)
        {
            var canonical = SeedData.Catalogue();

            var existing = context.Products
                .Include(p => p.Sizes)
                .ToList();

            var byKey = existing
                .GroupBy(p => Key(p.Brand, p.Name, p.Concentration))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var want in canonical)
            {
                var k = Key(want.Brand, want.Name, want.Concentration);

                if (byKey.TryGetValue(k, out var current))
                {
                    // ── Update the existing product in place ──
                    current.Name            = want.Name;
                    current.Brand           = want.Brand;
                    current.Description     = want.Description;
                    current.Price           = want.Price;
                    current.ImageUrl        = want.ImageUrl;
                    current.Concentration   = want.Concentration;
                    current.Size            = want.Size;
                    current.FragranceFamily = want.FragranceFamily;
                    current.TopNotes        = want.TopNotes;
                    current.HeartNotes      = want.HeartNotes;
                    current.BaseNotes       = want.BaseNotes;
                    // StockQuantity is intentionally preserved (do not overwrite real stock).

                    // Replace size variants with the verified set.
                    // Clear the navigation first, then remove old rows and add new
                    // ones through the context. Reassigning the collection while also
                    // RemoveRange-ing the same tracked entities caused EF to batch a
                    // delete + update for one key → optimistic-concurrency error.
                    var oldSizes = current.Sizes.ToList();
                    current.Sizes.Clear();
                    context.ProductSizes.RemoveRange(oldSizes);

                    foreach (var s in want.Sizes)
                    {
                        context.ProductSizes.Add(new ProductSize
                        {
                            Id                = Guid.NewGuid(),
                            ProductId         = current.Id,
                            Ml                = s.Ml,
                            Price             = s.Price,
                            IsAvailable       = s.IsAvailable,
                            UnavailableReason = s.UnavailableReason
                        });
                    }
                }
                else
                {
                    // ── Insert the new product ──
                    context.Products.Add(want);
                }
            }

            context.SaveChanges();
        }
    }
}
