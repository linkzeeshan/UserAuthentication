namespace UserManagementApp
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using UserManagementApp.Model;

    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            SeedRole(modelBuilder);
        }

        private static void SeedRole(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "Doctor", ConcurrencyStamp = "2", NormalizedName = "DT" },
                new IdentityRole() { Name = "Patient", ConcurrencyStamp = "3", NormalizedName = "PT" },
                new IdentityRole() { Name = "Tanent", ConcurrencyStamp = "4", NormalizedName = "Tanent" }
                );
        }
    }

}
