using API.Middleware;
using API.Hubs;
using Core;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStripeWebhookEventService, StripeWebhookEventService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<BookContext>();
builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis")
        ?? throw new Exception("Cannot get redis connection string");
    var configuation = ConfigurationOptions.Parse(connectionString, true);
    return ConnectionMultiplexer.Connect(configuation);
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Cannot get default database connection string");

builder.Services.AddDbContext<BookContext>(opt =>
    opt.UseSqlServer(defaultConnection));

builder.Services.AddScoped<IBookRepository, BookRepository>(); 
builder.Services.AddOpenApiDocument();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    // app.UseSwaggerUi(options =>
    // {
    //     options.DocumentPath = "/openapi/v1.json";
    // }); 
    app.UseSwaggerUi();

}

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200", "https://localhost:4200")
    .SetIsOriginAllowed(_ => true));

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<AnonymousSessionMiddleware>();
app.UseAuthorization();


app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>();
app.MapHub<PaymentStatusHub>("/hubs/payment-status");
app.MapFallbackToController("Index", "Fallback");


// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookContext>();

    // Apply all pending migrations automatically on startup.
    context.Database.Migrate();

    if (!context.Books.Any())
    {
        // Seed once when database is empty.
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "First Book", Author = "Author 1", Description = "Description 1", Price = 9.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Second Book", Author = "Author 2", Description = "Description 2", Price = 14.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Third Book", Author = "Author 3", Description = "Description 3", Price = 19.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fourth Book", Author = "Author 4", Description = "Description 4", Price = 24.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fifth Book", Author = "Author 5", Description = "Description 5", Price = 29.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Sixth Book", Author = "Author 6", Description = "Description 6", Price = 34.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Seventh Book", Author = "Author 7", Description = "Description 7", Price = 39.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Eighth Book", Author = "Author 8", Description = "Description 8", Price = 44.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Ninth Book", Author = "Author 9", Description = "Description 9", Price = 49.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Tenth Book", Author = "Author 10", Description = "Description 10", Price = 54.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Eleventh Book", Author = "Author 11", Description = "Description 11", Price = 59.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twelfth Book", Author = "Author 12", Description = "Description 12", Price = 64.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Thirteenth Book", Author = "Author 13", Description = "Description 13", Price = 69.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fourteenth Book", Author = "Author 14", Description = "Description 14", Price = 74.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fifteenth Book", Author = "Author 15", Description = "Description 15", Price = 79.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Sixteenth Book", Author = "Author 16", Description = "Description 16", Price = 84.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Seventeenth Book", Author = "Author 17", Description = "Description 17", Price = 89.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Eighteenth Book", Author = "Author 18", Description = "Description 18", Price = 94.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Nineteenth Book", Author = "Author 19", Description = "Description 19", Price = 99.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twentieth Book", Author = "Author 20", Description = "Description 20", Price = 104.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-first Book", Author = "Author 21", Description = "Description 21", Price = 109.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-second Book", Author = "Author 22", Description = "Description 22", Price = 114.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-third Book", Author = "Author 23", Description = "Description 23", Price = 119.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-fourth Book", Author = "Author 24", Description = "Description 24", Price = 124.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-fifth Book", Author = "Author 25", Description = "Description 25", Price = 129.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-sixth Book", Author = "Author 26", Description = "Description 26", Price = 134.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-seventh Book", Author = "Author 27", Description = "Description 27", Price = 139.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-eighth Book", Author = "Author 28", Description = "Description 28", Price = 144.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-ninth Book", Author = "Author 29", Description = "Description 29", Price = 149.99m });
        context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Thirtieth Book", Author = "Author 30", Description = "Description 30", Price = 154.99m });
        context.SaveChanges();
    }
}

app.Run();
