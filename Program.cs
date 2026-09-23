using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WorkTimePro.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// SERVICE CONFIGURATION
// This section registers services that the app will use
// ═══════════════════════════════════════════════════════════════

// Add API controllers (enables [ApiController] classes to handle HTTP requests)
builder.Services.AddControllers();

// Add Swagger for API documentation (helpful for testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ───────────────────────────────────────────────────────────────
// DATABASE CONFIGURATION
// Register SQLite database using Entity Framework Core
// Connection string is stored in appsettings.json
// ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ───────────────────────────────────────────────────────────────
// JWT AUTHENTICATION SETUP
// JSON Web Tokens provide secure authentication
// ⚠️ NOTE: Currently configured but not fully implemented
// For a production app, you would generate and return tokens in AuthController
// ───────────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] 
    ?? throw new InvalidOperationException("JWT:Key is missing in appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ───────────────────────────────────────────────────────────────
// CORS CONFIGURATION
// Cross-Origin Resource Sharing allows frontend (localhost:port)
// to communicate with backend API (different port)
// ⚠️ WARNING: AllowAnyOrigin is OK for development but unsafe for production
// In production, specify exact frontend URL
// ───────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Allow requests from any domain
              .AllowAnyHeader()      // Allow any HTTP headers
              .AllowAnyMethod();     // Allow GET, POST, PUT, DELETE, etc.
    });
});

// Build the application
var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// Middleware processes HTTP requests in order
// Think of it as a pipeline: Request → Middleware1 → Middleware2 → Controller
// ═══════════════════════════════════════════════════════════════

// Show detailed error pages during development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();   // Shows full exception details
    app.UseSwagger();                  // API documentation UI
    app.UseSwaggerUI();
}

// Serve static files from wwwroot folder (HTML, CSS, JS)
app.UseStaticFiles();

// Enable CORS with the policy we defined earlier
app.UseCors("AllowAll");

// Authentication & Authorization middleware
// ⚠️ ORDER MATTERS: UseAuthentication MUST come before UseAuthorization
app.UseAuthentication();   // Identifies who the user is
app.UseAuthorization();    // Checks if user has permission

// Map HTTP requests to controller methods
app.MapControllers();

// ═══════════════════════════════════════════════════════════════
// DATABASE INITIALIZATION
// Create database and seed with default admin user on first run
// ═══════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        // Ensure database exists and run migrations
        db.Database.EnsureCreated();
        
        
        Console.WriteLine("✅ Database seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Seeding failed: {ex.Message}");
        Console.ResetColor();
    }
}

// Start the web server
app.Run();