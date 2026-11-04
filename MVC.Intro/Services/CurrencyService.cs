using System.Globalization;

namespace MVC.Intro.Services
{
    /// <summary>
    /// Single source of truth for currency conversion and EUR formatting.
    ///
    /// The whole store is priced in EUR (€). Any legacy values that were
    /// originally entered in BGN are converted using the official fixed
    /// irrevocable conversion rate set for Bulgaria's euro adoption:
    ///
    ///     1 EUR = 1.95583 BGN   →   EUR = BGN / 1.95583
    ///
    /// Converted amounts are rounded to 2 decimals using commercial
    /// (round-half-away-from-zero) rounding, which is the rounding rule
    /// prescribed by the EU/Bulgarian dual-display and conversion regulations.
    /// </summary>
    public static class CurrencyService
    {
        /// <summary>Official irrevocable fixed rate: 1 EUR = 1.95583 BGN.</summary>
        public const decimal BgnPerEur = 1.95583m;

        /// <summary>Currency symbol shown to customers.</summary>
        public const string EuroSymbol = "€";

        /// <summary>ISO 4217 code.</summary>
        public const string EuroCode = "EUR";

        /// <summary>
        /// Converts a BGN amount to EUR using the official fixed rate and the
        /// legally applicable rounding (2 decimals, half away from zero).
        /// </summary>
        public static decimal ConvertBgnToEur(decimal amountBgn) =>
            Math.Round(amountBgn / BgnPerEur, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Formats an amount already expressed in EUR as "€149.99".
        /// Uses InvariantCulture so the decimal separator is always a dot and
        /// the symbol is placed before the number, matching the required format.
        /// </summary>
        public static string FormatEur(decimal amountEur) =>
            EuroSymbol + amountEur.ToString("0.00", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Short alias used inside Razor views: <c>@Money.Eur(price)</c> → "€149.99".
    /// Keeps the view markup clean and guarantees consistent formatting.
    /// </summary>
    public static class Money
    {
        public static string Eur(decimal amountEur) => CurrencyService.FormatEur(amountEur);
    }
}
