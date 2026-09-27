using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database (MySQL via Pomelo) ──────────────────────────────────────────
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// ── Services (Dependency Injection) ─────────────────────────────────────
builder.Services.AddScoped<ReservationManager>();
builder.Services.AddScoped<OrderProcessor>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ReportGenerator>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<LoggerService>();
builder.Services.AddScoped<AuthService>();

// ── CORS (allow local frontend during development, any port) ────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.SetIsOriginAllowed(origin => true) // dev only: allow any local origin (Live Server, file previews, etc.)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Controllers + Swagger ────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Restaurant API", Version = "v1" });
});

var app = builder.Build();

// ── Middleware Pipeline ──────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
