using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database Context (SQL Server via EF Core) ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── HttpClient for Flask ML API ──
builder.Services.AddHttpClient<MlApiService>(client =>
{
    var flaskUrl = builder.Configuration["FlaskApiUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(flaskUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// ── Application Services ──
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AnalyticsService>();

// ── Controllers ──
builder.Services.AddControllers();

// ── Swagger / OpenAPI ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Reverse Logistics CSML API",
        Version = "v1",
        Description = "Cost-Sensitive Machine Learning Framework for E-Commerce Return Management"
    });
});

// ── CORS for Angular UI ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.MapControllers();

app.Run();
