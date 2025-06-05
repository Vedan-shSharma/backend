using EduSyncAPI.Services;
using EduSyncAPI.Data;
using Microsoft.EntityFrameworkCore;
using edusync_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Azure-specific configuration
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080); // Listen on port 8080 for Azure
});

// Add environment variables support
builder.Configuration.AddEnvironmentVariables();

// Add user secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    // Read allowed origins from configuration
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",    // React dev server HTTP
                "https://localhost:5173",   // React dev server HTTPS
                "http://localhost:5172",    // API HTTP
                "https://localhost:7136",
                "https://thankful-mushroom-08adc020f.6.azurestaticapps.net",
                "https://edusyncvirendraback-hbcaavg5d2afaxg0.centralindia-01.azurewebsites.net"  // Add your Azure backend URL
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<BlobStorageService>();

// Add DB context with SQL Server
builder.Services.AddDbContext<EduSyncDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Force HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Add default route for health check
app.MapGet("/", () => "EduSync API is running!");

app.UseHttpsRedirection();

// Use the CORS policy before authorization and routing
app.UseCors("AllowFrontend");

app.UseAuthorization();

// Ensure controllers are mapped
app.MapControllers();

// Add error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred.");
        throw;
    }
});

app.Run();




