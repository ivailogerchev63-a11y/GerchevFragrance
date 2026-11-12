namespace MVC.Intro.Helpers
{
    /// <summary>
    /// All prices in the database are stored in EUR.
    /// BGN conversion uses the fixed BNB rate: 1 EUR = 1.95583 BGN.
    /// </summary>
    public static class CurrencyHelper
    {
        /// <summary>Fixed BNB exchange rate: 1 EUR = 1.95583 BGN</summary>
        public const decimal EurToBgn = 1.95583m;

        /// <summary>
        /// Formats a price in EUR and shows BGN equivalent in parentheses.
        /// Example: "174.00 € (340.31 лв)"
        /// </summary>
        public static string FormatEuro(decimal amountInEur)
        {
            var bgn = amountInEur * EurToBgn;
            return $"{amountInEur:F2} € ({bgn:F2} лв)";
        }

        /// <summary>Returns only the EUR string — "174.00 €"</summary>
        public static string FormatEurOnly(decimal amountInEur)
            => $"{amountInEur:F2} €";

        /// <summary>Returns only the BGN string — "340.31 лв"</summary>
        public static string FormatBgn(decimal amountInEur)
            => $"{amountInEur * EurToBgn:F2} лв";

        /// <summary>Converts EUR to BGN value</summary>
        public static decimal ToBgn(decimal amountInEur)
            => Math.Round(amountInEur * EurToBgn, 2);
    }
}
