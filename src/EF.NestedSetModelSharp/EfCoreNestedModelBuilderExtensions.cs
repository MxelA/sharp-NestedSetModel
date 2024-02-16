using Microsoft.EntityFrameworkCore;

namespace EF.NestedSetModelSharp
{
    public static class EfCoreNestedModelBuilderExtensions
    {
        public static void ConfigureNested<T, TKey, TNullableKey>(this ModelBuilder modelBuilder)
              where T : class, INestedSetModel<T, TKey, TNullableKey>
        {
            modelBuilder.Entity<T>()
                .Ignore(b => b.Moving);
            modelBuilder.Entity<T>()
                .HasOne(n => n.Parent)
                .WithMany(n => n.Children)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<T>()
                .HasOne(n => n.Root)
                .WithMany(n => n.Descendants)
                .HasForeignKey(n => n.RootId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<T>()
                .Property(n => n.RootId)
                .IsRequired(false);

        }
    }
}