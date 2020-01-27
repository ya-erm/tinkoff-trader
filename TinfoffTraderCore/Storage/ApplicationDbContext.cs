using Microsoft.EntityFrameworkCore;
using TinkoffTraderCore.Models;

namespace TinkoffTraderCore.Storage
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Instrument> Instruments { get; set; }

        public DbSet<Fill> Fills { get; set; }

        public ApplicationDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=tinkoff-trader;Trusted_Connection=True;");
        }
    }
}
