using Microsoft.EntityFrameworkCore;
using SleepCommerce.Domain.Entities;

namespace SleepCommerce.Infrastructure.Data.Seed;

public static class ProductSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Produtos.AnyAsync())
            return;

        var produtos = new List<Produto>
        {
            new("Notebook Gamer", "Notebook com RTX 4060, 16GB RAM, 512GB SSD", 15, 5499.99m),
            new("Mouse Wireless", "Mouse sem fio ergonômico com sensor óptico", 150, 89.90m),
            new("Teclado Mecânico", "Teclado mecânico RGB com switches blue", 80, 299.90m),
            new("Monitor 27\"", "Monitor IPS 27 polegadas 144Hz QHD", 25, 1899.00m),
            new("Headset Gamer", "Headset com microfone, som surround 7.1", 60, 249.90m)
        };

        await context.Produtos.AddRangeAsync(produtos);
        await context.SaveChangesAsync();
    }
}
