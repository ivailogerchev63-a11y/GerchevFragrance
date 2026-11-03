using System.Text.Json;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    /// <summary>
    /// Stores the shopping cart in the HTTP session as a JSON-serialised list of CartItem.
    /// </summary>
    public class CartService
    {
        private const string CartSessionKey = "ShoppingCart";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session =>
            _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("No active HTTP session.");

        // ── Read ────────────────────────────────────────────────────────────────

        public List<CartItem> GetItems()
        {
            var json = Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public CartViewModel GetCart()
        {
            return new CartViewModel { Items = GetItems() };
        }

        // ── Write ───────────────────────────────────────────────────────────────

        public void AddOrUpdate(CartItem incoming)
        {
            var items = GetItems();
            var existing = items.FirstOrDefault(i =>
                i.ProductId == incoming.ProductId &&
                i.ProductSizeId == incoming.ProductSizeId);

            if (existing != null)
                existing.Quantity += incoming.Quantity;
            else
                items.Add(incoming);

            Save(items);
        }

        public void UpdateQuantity(Guid productSizeId, int quantity)
        {
            var items = GetItems();
            var item = items.FirstOrDefault(i => i.ProductSizeId == productSizeId);
            if (item == null) return;

            if (quantity <= 0)
                items.Remove(item);
            else
                item.Quantity = quantity;

            Save(items);
        }

        public void RemoveItem(Guid productSizeId)
        {
            var items = GetItems();
            items.RemoveAll(i => i.ProductSizeId == productSizeId);
            Save(items);
        }

        public void Clear()
        {
            Session.Remove(CartSessionKey);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private void Save(List<CartItem> items)
        {
            Session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
        }
    }
}
