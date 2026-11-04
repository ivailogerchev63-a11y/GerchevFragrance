using System.Text.Json;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    /// <summary>
    /// Stores the shopping cart and applied coupon in the HTTP session.
    /// </summary>
    public class CartService
    {
        private const string CartSessionKey   = "ShoppingCart";
        private const string CouponSessionKey = "AppliedCoupon";       // "CODE|percent"

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session =>
            _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("No active HTTP session.");

        // ── Items ────────────────────────────────────────────────────────────────

        public List<CartItem> GetItems()
        {
            var json = Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public CartViewModel GetCart()
        {
            var vm    = new CartViewModel { Items = GetItems() };
            var raw   = Session.GetString(CouponSessionKey);
            if (!string.IsNullOrEmpty(raw))
            {
                var parts = raw.Split('|');
                if (parts.Length == 2 && int.TryParse(parts[1], out var pct))
                {
                    vm.CouponCode      = parts[0];
                    vm.DiscountPercent = pct;
                }
            }
            return vm;
        }

        public void AddOrUpdate(CartItem incoming)
        {
            var items    = GetItems();
            var existing = items.FirstOrDefault(i =>
                i.ProductId     == incoming.ProductId &&
                i.ProductSizeId == incoming.ProductSizeId);

            if (existing != null) existing.Quantity += incoming.Quantity;
            else                  items.Add(incoming);

            SaveItems(items);
        }

        public void UpdateQuantity(Guid productSizeId, int quantity)
        {
            var items = GetItems();
            var item  = items.FirstOrDefault(i => i.ProductSizeId == productSizeId);
            if (item == null) return;

            if (quantity <= 0) items.Remove(item);
            else               item.Quantity = quantity;

            SaveItems(items);
        }

        public void RemoveItem(Guid productSizeId)
        {
            var items = GetItems();
            items.RemoveAll(i => i.ProductSizeId == productSizeId);
            SaveItems(items);
        }

        public void Clear()
        {
            Session.Remove(CartSessionKey);
            Session.Remove(CouponSessionKey);
        }

        // ── Coupon ───────────────────────────────────────────────────────────────

        public void ApplyCoupon(string code, int discountPercent)
        {
            Session.SetString(CouponSessionKey, $"{code.ToUpperInvariant()}|{discountPercent}");
        }

        public void RemoveCoupon()
        {
            Session.Remove(CouponSessionKey);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SaveItems(List<CartItem> items)
        {
            Session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
        }
    }
}
