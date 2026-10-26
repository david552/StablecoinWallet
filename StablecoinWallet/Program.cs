using Microsoft.EntityFrameworkCore;
using StablecoinWallet.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("WalletDatabase")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:WalletDatabase is required. Docker Compose supplies it automatically.");

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Local Compose serves HTTP. HTTPS termination can be configured at a reverse proxy.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health"); // Process liveness; this does not test the database.
app.Run();
