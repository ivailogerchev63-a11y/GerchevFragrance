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

        public async Task<bool> AddToCartAsync(Guid productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var items = GetCartItems();
            var existing = items.FirstOrDefault(i => i.ProductId == productId);
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
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Size = product.Size,
                    Concentration = product.Concentration,
                    Quantity = quantity
                });
            }

            SaveCartItems(items);
            return true;
        }

        public void UpdateQuantity(Guid productId, int quantity)
        {
            var items = GetCartItems();
            var existing = items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                if (quantity <= 0)
                {
                    items.Remove(existing);
                }
                else
                {
                    existing.Quantity = quantity;
                }
                SaveCartItems(items);
            }
        }

        public void RemoveFromCart(Guid productId)
        {
            var items = GetCartItems();
            items.RemoveAll(i => i.ProductId == productId);
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

            // Validate specifically for GERCHEV10
            if (normalizedCode != "GERCHEV10")
            {
                return (false, "Невалиден промоционален код.", 0);
            }

            // Check if there is a valid coupon in DB
            var couponQuery = _context.DiscountCoupons.Where(c => c.Code == normalizedCode && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow);
            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                var normEmail = customerEmail.Trim().ToLowerInvariant();
                couponQuery = couponQuery.Where(c => c.Email.ToLower() == normEmail);
            }

            var coupon = await couponQuery.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync();

            // Also allow the standard GERCHEV10 code for customers who just received it
            if (coupon == null)
            {
                // Fallback check: check if code is GERCHEV10 and active in general
                var anyActive = await _context.DiscountCoupons.AnyAsync(c => c.Code == normalizedCode && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow);
                if (!anyActive && !string.IsNullOrWhiteSpace(customerEmail))
                {
                    return (false, "Този код за отстъпка вече е използван или е изтекъл.", 0);
                }
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
            decimal discount = 0m;

            if (!string.IsNullOrEmpty(couponCode))
            {
                discount = 10.00m;
            }

            return new CartViewModel
            {
                Items = items,
                AppliedCouponCode = couponCode,
                DiscountPercentage = discount
            };
        }
    }
}
