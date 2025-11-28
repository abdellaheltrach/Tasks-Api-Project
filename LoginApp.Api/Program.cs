using Azure.Core;
using LoginApp.Api.Models;
using LoginApp.Business.Services;
using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Data.Interceptors;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories;
using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.Drawing;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

// Register repositories & services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();


builder.Services.AddScoped<ITaskItemRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();





// Add Background Service for cleanup (optional but recommended for performance)
builder.Services.AddHostedService<RefreshTokenCleanupService>();

// Add SoftDelete interceptor
builder.Services.AddScoped<SoftDeleteInterceptor>();


// Enable controllers
builder.Services.AddControllers();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


// ✅ Add JWT settings from configuration (for injection in TokenService, etc.)
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

//  JWT Authentication Configuration

var jwtSettings = builder.Configuration.GetSection("Jwt"); //read Jwt Section from AppSettings
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);      //Encoding the "Key" for using it later

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //how to check who the user is.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    //what to do when an unauthorized user tries to access [Authorize] endpoints.
})

   // how to verify incoming tokens
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,  //Check that the “issuer” (the app that created the token) matches your jwtSettings["Issuer"]
           ValidateAudience = true,//Check that the token is meant for your app.
           ValidateLifetime = true,//Check that the token is not expired.
           ValidateIssuerSigningKey = true, //Check that the token’s signature is valid using your secret key.
           ValidIssuer = jwtSettings["Issuer"], //check values from jwtSettings["Issuer"]
           ValidAudience = jwtSettings["Audience"],// check values jwtSettings["Audience"]
           IssuerSigningKey = new SymmetricSecurityKey(key) //check if token hasn't been forged or modified by JWT-Key
       };
   });


builder.Services.ConfigureApplicationCookie(options =>
{
    //The cookie cannot be accessed via client - side JS(reduces XSS attacks).
    options.Cookie.HttpOnly = true;           // Prevents JavaScript access to the cookie.
    //The cookie is sent only in requests originating from the same site, not from external sites. Helps prevent CSRF.
    options.Cookie.SameSite = SameSiteMode.Strict; // Only send cookie in same-site requests (helps prevent CSRF).
    //ASP.NET will only send the cookie over HTTPS connections.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only send cookie over HTTPS.
});

var app = builder.Build();

// Auto-run migrations on startup (useful for Docker)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseCors("AllowAll");



app.UseAuthentication(); //Reads the JWT from the request header, validates it, and builds HttpContext.User
app.UseAuthorization();  //Checks [Authorize] attributes and decides whether to allow or deny access

app.UseFileServer(); //combining the next tow methods
//app.UseDefaultFiles();  // looks for index.html by default
//app.UseStaticFiles();   // serves HTML, CSS, JS


app.MapControllers();


app.Run();
