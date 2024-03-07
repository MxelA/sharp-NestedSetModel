using Microsoft.EntityFrameworkCore;

namespace EF.NestedSetModelSharp.Tests
{
    public class AppDbContext : DbContext
    {
        public DbSet<ClothingCategory> Nodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // define the database to use
            optionsBuilder.UseNpgsql(DbSql.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureNested<ClothingCategory, int, int?>();
        }
    }
}