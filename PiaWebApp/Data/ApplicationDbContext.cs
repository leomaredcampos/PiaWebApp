using Microsoft.EntityFrameworkCore;
using PiaWebApp.Models;

namespace PiaWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // For existing 'datagroup1' table (Promo)
        public DbSet<Promo> Promos { get; set; } = default!;

        // For 'datagroup5' table (PromoDetails)
        public DbSet<PromoDetails> PromoDetails { get; set; } = default!;


        // For 'Accesstbl' table (Accesstbl)
        public DbSet<Accesstbl> Accesstbl { get; set; } = default!;


    }
}