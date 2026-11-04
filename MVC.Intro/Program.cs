using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── MVC ──────────────────────────────────────────────────────────────
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // ── Database ─────────────────────────────────────────────────────────
            var dbFolder = Path.Combine(builder.Environment.ContentRootPath, "Database");
            Directory.CreateDirectory(dbFolder);
            var dbPath = Path.Combine(dbFolder, "fragrance.db");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // ── Identity ─────────────────────────────────────────────────────────
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit           = true;
                options.Password.RequiredLength         = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase       = false;
            })
            .AddEntityFrameworkStores<AppDbContext>();

            // ── Session (for shopping cart) ───────────────────────────────────────
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ── Application services ─────────────────────────────────────────────
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<CartService>();

            // ── Build ────────────────────────────────────────────────────────────
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();           // must be before Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // ── Seed database ─────────────────────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // EnsureCreated returns true when it just created the schema,
                // i.e. this is a brand-new database that SeedData fills in EUR.
                bool freshlyCreated = db.Database.EnsureCreated();
                SeedData.Initialize(db);

                // Convert any legacy BGN prices to EUR exactly once. A freshly
                // created DB is already seeded in EUR, so we only drop the marker.
                CurrencyMigration.EnsurePricesInEur(db, dbFolder, alreadyEur: freshlyCreated);

                // Reconcile the live catalogue with the verified canonical data:
                // fixes product images, adds newly identified perfumes and keeps
                // prices/notes correct. Idempotent and never deletes data.
                CatalogueSync.Reconcile(db);
            }

            // ── Routes ────────────────────────────────────────────────────────────
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}
