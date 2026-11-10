namespace MVC.Intro.Helpers
{
    public static class CurrencyHelper
    {
        public const decimal BgnPerEuro = 1.95583m;

        public static decimal ToEuro(decimal amountInBgn)
            => Math.Round(amountInBgn / BgnPerEuro, 2, MidpointRounding.AwayFromZero);

        public static string FormatEuro(decimal amountInBgn)
            => $"{ToEuro(amountInBgn):F2} €";
    }
}
