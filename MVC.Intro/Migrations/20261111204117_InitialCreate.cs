using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerchevCarting.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All tables use IF NOT EXISTS so this migration is safe to run against
            // a database that was previously created by EnsureCreated() — which
            // creates tables but does NOT write to __EFMigrationsHistory.
            // The only table that was actually missing is ProductVariants.

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AspNetRoles"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_AspNetRoles"" PRIMARY KEY,
                    ""Name"" TEXT NULL,
                    ""NormalizedName"" TEXT NULL,
                    ""ConcurrencyStamp"" TEXT NULL
                );");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""RoleNameIndex""
                ON ""AspNetRoles"" (""NormalizedName"");");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AspNetUsers"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_AspNetUsers"" PRIMARY KEY,
                    ""FirstName"" TEXT NULL,
                    ""LastName"" TEXT NULL,
                    ""UserName"" TEXT NULL,
                    ""NormalizedUserName"" TEXT NULL,
                    ""Email"" TEXT NULL,
                    ""NormalizedEmail"" TEXT NULL,
                    ""EmailConfirmed"" INTEGER NOT NULL,
                    ""PasswordHash"" TEXT NULL,
                    ""SecurityStamp"" TEXT NULL,
                    ""ConcurrencyStamp"" TEXT NULL,
                    ""PhoneNumber"" TEXT NULL,
                    ""PhoneNumberConfirmed"" INTEGER NOT NULL,
                    ""TwoFactorEnabled"" INTEGER NOT NULL,
                    ""LockoutEnd"" TEXT NULL,
                    ""LockoutEnabled"" INTEGER NOT NULL,
                    ""AccessFailedCount"" INTEGER NOT NULL
                );");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""EmailIndex""
                ON ""AspNetUsers"" (""NormalizedEmail"");");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""UserNameIndex""
                ON ""AspNetUsers"" (""NormalizedUserName"");");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AspNetRoleClaims"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AspNetRoleClaims"" PRIMARY KEY AUTOINCREMENT,
                    ""RoleId"" TEXT NOT NULL,
                    ""ClaimType"" TEXT NULL,
                    ""ClaimValue"" TEXT NULL,
                    CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId""
                        FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_AspNetRoleClaims_RoleId""
                ON ""AspNetRoleClaims"" (""RoleId"");");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AspNetUserClaims"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AspNetUserClaims"" PRIMARY KEY AUTOINCREMENT,
                    ""UserId"" TEXT NOT NULL,
                    ""ClaimType"" TEXT NULL,
                    ""ClaimValue"" TEXT NULL,
                    CONSTRAINT ""FK_AspNetUserClaims_AspNetUsers_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_AspNetUserClaims_UserId""
                ON ""AspNetUserClaims"" (""UserId"");");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AspNetUserLogins"" (
                    ""LoginProvider"" TEXT NOT NULL,
                    ""ProviderKey"" TEXT NOT NULL,
                    ""ProviderDisplayName"" TEXT NULL,
                    ""UserId"" TEXT NOT NULL,
                    CONSTRAINT ""PK_AspNetUserLogins"" PRIMARY KEY (""LoginProvider"", ""ProviderKey""),
                    CONSTRAINT ""FK_AspNetUserLogins_AspNetUsers_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_AspNetUserLogins_UserId""
                ON ""AspNetUserLogins"" (""UserId"");");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AspNetUserRoles"" (
                    ""UserId"" TEXT NOT NULL,
                    ""RoleId"" TEXT NOT NULL,
                    CONSTRAINT ""PK_AspNetUserRoles"" PRIMARY KEY (""UserId"", ""RoleId""),
                    CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId""
                        FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_AspNetUserRoles_AspNetUsers_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_AspNetUserRoles_RoleId""
                ON ""AspNetUserRoles"" (""RoleId"");");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""AspNetUserTokens"" (
                    ""UserId"" TEXT NOT NULL,
                    ""LoginProvider"" TEXT NOT NULL,
                    ""Name"" TEXT NOT NULL,
                    ""Value"" TEXT NULL,
                    CONSTRAINT ""PK_AspNetUserTokens"" PRIMARY KEY (""UserId"", ""LoginProvider"", ""Name""),
                    CONSTRAINT ""FK_AspNetUserTokens_AspNetUsers_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""DiscountCoupons"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_DiscountCoupons"" PRIMARY KEY,
                    ""Code"" TEXT NOT NULL,
                    ""Email"" TEXT NOT NULL,
                    ""DiscountPercentage"" TEXT NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ExpiresAt"" TEXT NOT NULL,
                    ""IsUsed"" INTEGER NOT NULL,
                    ""UsedAt"" TEXT NULL,
                    ""OrderId"" TEXT NULL
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Orders"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Orders"" PRIMARY KEY,
                    ""OrderNumber"" TEXT NOT NULL,
                    ""CustomerName"" TEXT NOT NULL,
                    ""CustomerEmail"" TEXT NOT NULL,
                    ""CustomerPhone"" TEXT NOT NULL,
                    ""ShippingAddress"" TEXT NOT NULL,
                    ""City"" TEXT NOT NULL,
                    ""PostalCode"" TEXT NULL,
                    ""Notes"" TEXT NULL,
                    ""PaymentMethod"" TEXT NOT NULL,
                    ""Subtotal"" decimal(18,2) NOT NULL,
                    ""DiscountAmount"" decimal(18,2) NOT NULL,
                    ""AppliedCouponCode"" TEXT NULL,
                    ""ShippingFee"" decimal(18,2) NOT NULL,
                    ""TotalAmount"" decimal(18,2) NOT NULL,
                    ""OrderDate"" TEXT NOT NULL,
                    ""Status"" TEXT NOT NULL,
                    ""ConfirmationEmailSent"" INTEGER NOT NULL
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Products"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Products"" PRIMARY KEY,
                    ""Name"" TEXT NOT NULL,
                    ""Description"" TEXT NULL,
                    ""Price"" decimal(18,2) NOT NULL,
                    ""OriginalPrice"" decimal(18,2) NULL,
                    ""ImageUrl"" TEXT NULL,
                    ""Brand"" TEXT NULL,
                    ""Size"" TEXT NULL,
                    ""Concentration"" TEXT NULL,
                    ""StockQuantity"" INTEGER NOT NULL,
                    ""Category"" TEXT NULL,
                    ""IsBestseller"" INTEGER NOT NULL,
                    ""IsFeatured"" INTEGER NOT NULL,
                    ""Notes"" TEXT NULL,
                    ""FragranceFamily"" TEXT NULL
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""OrderItems"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_OrderItems"" PRIMARY KEY,
                    ""OrderId"" TEXT NOT NULL,
                    ""ProductId"" TEXT NOT NULL,
                    ""ProductName"" TEXT NOT NULL,
                    ""ProductBrand"" TEXT NULL,
                    ""ImageUrl"" TEXT NULL,
                    ""SelectedSize"" TEXT NULL,
                    ""UnitPrice"" decimal(18,2) NOT NULL,
                    ""Quantity"" INTEGER NOT NULL,
                    CONSTRAINT ""FK_OrderItems_Orders_OrderId""
                        FOREIGN KEY (""OrderId"") REFERENCES ""Orders"" (""Id"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_OrderItems_OrderId""
                ON ""OrderItems"" (""OrderId"");");

            // ProductVariants is the table that was missing from the existing database.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""ProductVariants"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_ProductVariants"" PRIMARY KEY,
                    ""ProductId"" TEXT NOT NULL,
                    ""SizeMl"" INTEGER NOT NULL,
                    ""Price"" decimal(18,2) NOT NULL,
                    ""OriginalPrice"" decimal(18,2) NULL,
                    ""StockQuantity"" INTEGER NOT NULL,
                    ""IsAvailable"" INTEGER NOT NULL,
                    CONSTRAINT ""FK_ProductVariants_Products_ProductId""
                        FOREIGN KEY (""ProductId"") REFERENCES ""Products"" (""Id"") ON DELETE CASCADE
                );");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_ProductVariants_ProductId""
                ON ""ProductVariants"" (""ProductId"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "DiscountCoupons");
            migrationBuilder.DropTable(name: "OrderItems");
            migrationBuilder.DropTable(name: "ProductVariants");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "AspNetUsers");
            migrationBuilder.DropTable(name: "Orders");
            migrationBuilder.DropTable(name: "Products");
        }
    }
}
