using Microsoft.EntityFrameworkCore;
using APRegistrationAPI.Data;
using APRegistrationAPI.Repositories;
using APRegistrationAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure PostgreSQL Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
    ));

// Register repositories
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();

// Register email service
builder.Services.AddHttpClient<IEmailService, ResendEmailService>();

// Configure CORS - IMPORTANT: Update with your frontend domain
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",  // Vite default dev server
                    "http://localhost:3000",  // Alternative port
                    "https://yourdomain.com"  // Production domain - UPDATE THIS
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AP Exam Registration API",
        Version = "v1",
        Description = "API for managing AP Exam registrations at Amberson High School",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Amberson High School",
            Email = "info@ambersonhighschool.com"
        }
    });
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AP Registration API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Auto-run migrations on startup (optional - comment out for production)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

app.Run();