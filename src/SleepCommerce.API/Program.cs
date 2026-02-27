using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SleepCommerce.API.Middleware;
using SleepCommerce.Application.Interfaces;
using SleepCommerce.Application.Services;
using SleepCommerce.Application.Validators;
using SleepCommerce.Domain.Interfaces;
using SleepCommerce.Infrastructure.Data;
using SleepCommerce.Infrastructure.Data.Seed;
using SleepCommerce.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SleepCommerce API",
        Description = "API para gerenciamento de produtos do SleepCommerce",
        Version = "v1"
    });

    var apiXmlFile = Path.Combine(AppContext.BaseDirectory, "SleepCommerce.API.xml");
    if (File.Exists(apiXmlFile))
        options.IncludeXmlComments(apiXmlFile);

    var appXmlFile = Path.Combine(AppContext.BaseDirectory, "SleepCommerce.Application.xml");
    if (File.Exists(appXmlFile))
        options.IncludeXmlComments(appXmlFile);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductRequestValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await ProductSeed.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();

public partial class Program {
    protected Program()
    {
    }
}
