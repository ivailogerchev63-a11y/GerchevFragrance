using MVC.Intro.Models;
using Xunit;

namespace GerchevCarting.Tests;

public class CartViewModelShippingTests
{
    [Fact]
    public void Total_includes_standard_shipping_when_subtotal_is_below_threshold()
    {
        var cart = new CartViewModel
        {
            Items =
            {
                new CartItem { Price = 100m, Quantity = 1 }
            }
        };

        Assert.Equal(6.00m, cart.ShippingFee);
        Assert.Equal(106.00m, cart.Total);
        Assert.Equal(106.00m, cart.GetOrderTotal(0m));
    }

    [Fact]
    public void Total_has_free_shipping_when_subtotal_meets_threshold()
    {
        var cart = new CartViewModel
        {
            Items =
            {
                new CartItem { Price = 150m, Quantity = 1 }
            }
        };

        Assert.Equal(0m, cart.ShippingFee);
        Assert.Equal(150.00m, cart.Total);
    }

    [Fact]
    public void PlaceOrder_total_keeps_shipping_after_discount()
    {
        var cart = new CartViewModel
        {
            Items =
            {
                new CartItem { Price = 100m, Quantity = 1 }
            },
            DiscountPercentage = 10m
        };

        var orderTotal = cart.GetOrderTotal(cart.DiscountAmount);

        Assert.Equal(6.00m, cart.ShippingFee);
        Assert.Equal(10.00m, cart.DiscountAmount);
        Assert.Equal(96.00m, orderTotal);
    }
}
