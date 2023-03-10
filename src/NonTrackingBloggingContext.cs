using Microsoft.EntityFrameworkCore;

namespace EFQuerying.Tracking;

public class NonTrackingBloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
            .HasData(
                new Blog { BlogId = 1, Url = @"https://devblogs.microsoft.com/dotnet", Rating = 5 },
                new Blog { BlogId = 2, Url = @"https://mytravelblog.com/", Rating = 4 });
    }

    #region OnConfiguring
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSqlServer(@"Data Source=localhost;Initial Catalog=EFQuerying.Tracking;Integrated Security=True;Trust Server Certificate=true")
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
    #endregion
}
