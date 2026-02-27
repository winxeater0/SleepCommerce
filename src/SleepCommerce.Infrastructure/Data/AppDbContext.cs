using Microsoft.EntityFrameworkCore;
using SleepCommerce.Domain.Entities;

namespace SleepCommerce.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Produto> Produtos => Set<Produto>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
