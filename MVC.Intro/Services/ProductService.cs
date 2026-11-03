using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    public class ProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly AppDbContext _context;

        public ProductService(ILogger<ProductService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>Returns all products including their size variants.</summary>
        public List<Product> GetAllProducts()
        {
            return _context.Products.Include(p => p.Sizes).ToList();
        }

        /// <summary>Returns a single product with its size variants, or throws if not found.</summary>
        public Product GetProductById(Guid id)
        {
            var product = _context.Products
                .Include(p => p.Sizes)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException($"Продуктът с ID {id} не е намерен.");

            return product;
        }

        /// <summary>Persists a new product (with its size variants) to the database.</summary>
        public Product AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.Id = Guid.NewGuid();

            // Ensure every size has a fresh ID and is linked to the product
            foreach (var size in product.Sizes)
            {
                size.Id = Guid.NewGuid();
                size.ProductId = product.Id;
            }

            _logger.LogInformation(
                "Adding product: {ProductName}, base price {ProductPrice:C}",
                product.Name, product.Price);

            _context.Products.Add(product);
            _context.SaveChanges();
            return product;
        }

        /// <summary>Removes a product and its size variants from the database.</summary>
        public void DeleteProduct(Guid id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
                throw new KeyNotFoundException($"Продуктът с ID {id} не е намерен.");

            _context.Products.Remove(product);
            _context.SaveChanges();
        }
    }
}
