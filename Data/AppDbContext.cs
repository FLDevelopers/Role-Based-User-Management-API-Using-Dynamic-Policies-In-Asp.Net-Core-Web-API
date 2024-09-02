using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApiWithRoleAuthentication.Models;

namespace WebApiWithRoleAuthentication.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Seed Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN"},
                new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
                );

            //Seed Admin Data
            var hasher = new PasswordHasher<IdentityUser>();
            var adminUser = new IdentityUser
            {
                UserName = "freetrained@freetrained.com",
                NormalizedUserName = "FREETRAINED@FREETRAINED.COM",
                Email = "freetrained@freetrained.com",
                NormalizedEmail = "FREETRAINED@FREETRAINED.COM",
                PhoneNumber = "1234567890",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = false,
            };

            adminUser.PasswordHash = hasher.HashPassword(adminUser, "freetrained123");

            builder.Entity<IdentityUser>().HasData(adminUser);

            //Assign Role To Admin
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "1",
                    UserId = adminUser.Id
                }
                );
        }
    }
}
