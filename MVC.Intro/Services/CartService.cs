using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    public class CartService
    {
        private const string CartSessionKey = "GerchevFragrance_Cart";
        private const string CouponSessionKey = "GerchevFragrance_Coupon";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public CartService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public List<CartItem> GetCartItems()
        {
            if (Session == null) return new List<CartItem>();
            var json = Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCartItems(List<CartItem> items)
        {
            if (Session == null) return;
            Session.SetString(CartSessionKey, JsonSerializer.Serialize(items));
        }

        /// <summary>
        /// Adds a product to cart. If variantId is provided, uses that variant's price/size.
        /// If the same product+size combination already exists, increments quantity.
        /// </summary>
        public async Task<bool> AddToCartAsync(Guid productId, int quantity = 1, Guid? variantId = null)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return false;

            string? selectedSize = null;
            decimal price = product.Price;

            if (variantId.HasValue && variantId != Guid.Empty)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
                if (variant != null && variant.IsAvailable)
                {
                    selectedSize = $"{variant.SizeMl} ml";
                    price = variant.Price;
                }
            }
            else if (product.Variants.Any())
            {
                // Default to the cheapest available variant
                var cheapest = product.Variants
                    .Where(v => v.IsAvailable)
                    .OrderBy(v => v.Price)
                    .FirstOrDefault();
                if (cheapest != null)
                {
                    selectedSize = $"{cheapest.SizeMl} ml";
                    price = cheapest.Price;
                }
            }
            else
            {
                selectedSize = product.Size;
            }

            var items = GetCartItems();
            var cartKey = $"{productId}|{selectedSize ?? "default"}";
            var existing = items.FirstOrDefault(i => i.CartKey == cartKey);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Brand = product.Brand ?? "Gerchev Fragrance",
                    Price = price,
                    ImageUrl = product.ImageUrl,
                    Size = selectedSize,
                    Concentration = product.Concentration,
                    Quantity = quantity
                });
            }

            SaveCartItems(items);
            return true;
        }

        /// <summary>
        /// Updates quantity for a specific cart item identified by productId + size key.
        /// </summary>
        public void UpdateQuantity(string cartKey, int quantity)
        {
            var items = GetCartItems();
            var existing = items.FirstOrDefault(i => i.CartKey == cartKey);
            if (existing != null)
            {
                if (quantity <= 0)
                    items.Remove(existing);
                else
                    existing.Quantity = quantity;
                SaveCartItems(items);
            }
        }

        /// <summary>Legacy overload for backwards compatibility (uses ProductId only)</summary>
        public void UpdateQuantity(Guid productId, int quantity)
        {
            var items = GetCartItems();
            var existing = items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                if (quantity <= 0)
                    items.Remove(existing);
                else
                    existing.Quantity = quantity;
                SaveCartItems(items);
            }
        }

        public void RemoveFromCart(string cartKey)
        {
            var items = GetCartItems();
            items.RemoveAll(i => i.CartKey == cartKey);
            SaveCartItems(items);
        }

        /// <summary>Legacy overload: removes first item with matching ProductId</summary>
        public void RemoveFromCart(Guid productId)
        {
            var items = GetCartItems();
            var toRemove = items.FirstOrDefault(i => i.ProductId == productId);
            if (toRemove != null) items.Remove(toRemove);
            SaveCartItems(items);
        }

        public void ClearCart()
        {
            if (Session == null) return;
            Session.Remove(CartSessionKey);
            Session.Remove(CouponSessionKey);
        }

        public string? GetAppliedCoupon()
        {
            return Session?.GetString(CouponSessionKey);
        }

        public async Task<(bool success, string message, decimal discountPercent)> ApplyCouponAsync(string code, string? customerEmail = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return (false, "Моля, въведете код за отстъпка.", 0);

            var normalizedCode = code.Trim().ToUpperInvariant();

            if (normalizedCode != "GERCHEV10")
                return (false, "Невалиден промоционален код.", 0);

            var couponQuery = _context.DiscountCoupons.Where(c => c.Code == normalizedCode && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow);
            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                var normEmail = customerEmail.Trim().ToLowerInvariant();
                couponQuery = couponQuery.Where(c => c.Email.ToLower() == normEmail);
            }

            var coupon = await couponQuery.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync();

            if (coupon == null)
            {
                var anyActive = await _context.DiscountCoupons.AnyAsync(c => c.Code == normalizedCode && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow);
                if (!anyActive && !string.IsNullOrWhiteSpace(customerEmail))
                    return (false, "Този код за отстъпка вече е използван или е изтекъл.", 0);
            }

            Session?.SetString(CouponSessionKey, normalizedCode);
            return (true, "Кодът за 10% отстъпка GERCHEV10 беше приложен успешно!", 10.00m);
        }

        public void RemoveCoupon()
        {
            Session?.Remove(CouponSessionKey);
        }

        public CartViewModel GetCartViewModel()
        {
            var items = GetCartItems();
            var couponCode = GetAppliedCoupon();
            decimal discount = string.IsNullOrEmpty(couponCode) ? 0m : 10.00m;

            return new CartViewModel
            {
                Items = items,
                AppliedCouponCode = couponCode,
                DiscountPercentage = discount
            };
        }
    }
}
