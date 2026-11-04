using MVC.Intro.Services;

namespace MVC.Intro.Data
{
    /// <summary>
    /// One-time, idempotent conversion of already-stored product prices from
    /// BGN to EUR for databases that were seeded before the euro switch.
    ///
    /// Why a sentinel file instead of a DB flag: the database is created with
    /// <c>EnsureCreated()</c> (no EF migrations), so adding a new marker table
    /// would not be applied to an existing fragrance.db. A sentinel file next
    /// to the .db file is schema-free, survives restarts, and guarantees the
    /// conversion runs exactly once — so prices are never converted twice.
    ///
    /// Fresh databases are already seeded in EUR (see <see cref="SeedData"/>),
    /// so this routine only matters for pre-existing BGN data. On a brand-new
    /// DB it simply writes the marker and converts nothing meaningful, because
    /// the sentinel is created right after seeding.
    /// </summary>
    public static class CurrencyMigration
    {
        /// <summary>
        /// Converts every stored Product.Price and ProductSize.Price from BGN
        /// to EUR using the official fixed rate, exactly once per database.
        /// </summary>
        /// <param name="context">The application db context.</param>
        /// <param name="dbFolder">Folder that holds fragrance.db (for the sentinel).</param>
        /// <param name="alreadyEur">
        /// True when the database was just freshly seeded (values are already EUR).
        /// In that case we only drop the sentinel and skip conversion.
        /// </param>
        public static void EnsurePricesInEur(AppDbContext context, string dbFolder, bool alreadyEur)
        {
            var sentinel = Path.Combine(dbFolder, ".prices-in-eur");

            // Already converted on a previous run → never touch prices again.
            if (File.Exists(sentinel)) return;

            if (!alreadyEur)
            {
                // Convert base product prices.
                var products = context.Products.ToList();
                foreach (var p in products)
                {
                    p.Price = CurrencyService.ConvertBgnToEur(p.Price);
                }

                // Convert per-size prices (skip the 0-priced "unavailable" sizes —
                // converting 0 stays 0, but we leave them untouched for clarity).
                var sizes = context.ProductSizes.ToList();
                foreach (var s in sizes)
                {
                    if (s.Price > 0m)
                        s.Price = CurrencyService.ConvertBgnToEur(s.Price);
                }

                context.SaveChanges();
            }

            // Mark this database as EUR so the conversion is never repeated.
            File.WriteAllText(sentinel,
                $"Prices converted to EUR using 1 EUR = {CurrencyService.BgnPerEur} BGN on {DateTime.UtcNow:O}.");
        }
    }
}
