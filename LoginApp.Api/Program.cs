using LoginApp.Api.Models;
using LoginApp.Business.Services;
using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Data.Interceptors;
using LoginApp.DataAccess.Repositories;
using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;



var builder = WebApplication.CreateBuilder(args);


#region Database Configuration
// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));
#endregion

#region Dependency Injection (Services & Repositories)
// Register repositories & services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();


builder.Services.AddScoped<ITaskItemRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
#endregion

#region Swagger / OpenAPI Configuration
// register swager services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your valid token in the text input below."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion

#region Rate Limiting Configuration
// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Global limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});
#endregion

#region Background Services & Interceptors
// Add Background Service for cleanup 
builder.Services.AddHostedService<RefreshTokenCleanupService>();

// Add SoftDelete interceptor
builder.Services.AddScoped<SoftDeleteInterceptor>();
#endregion

#region Controllers & CORS Configuration
// Enable controllers
builder.Services.AddControllers();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        b => b.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
#endregion

#region Authentication & JWT Configuration
// Add JWT settings from configuration 
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
#endregion

#region Cookie Policy Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    //The cookie cannot be accessed via client - side JS(reduces XSS attacks).
    options.Cookie.HttpOnly = true;           // Prevents JavaScript access to the cookie.
    //The cookie is sent only in requests originating from the same site, not from external sites. Helps prevent CSRF.
    options.Cookie.SameSite = SameSiteMode.Strict; // Only send cookie in same-site requests (helps prevent CSRF).
    //ASP.NET will only send the cookie over HTTPS connections.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Only send cookie over HTTPS.
});
#endregion

var app = builder.Build();

#region Middleware & Pipeline
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        //options.RoutePrefix = string.Empty;
    });
}


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

app.UseCors("AllowFrontend");



app.UseAuthentication(); //Reads the JWT from the request header, validates it, and builds HttpContext.User
app.UseAuthorization();  //Checks [Authorize] attributes and decides whether to allow or deny access

app.UseFileServer(); //combining the next tow methods
//app.UseDefaultFiles();  // looks for index.html by default
//app.UseStaticFiles();   // serves HTML, CSS, JS


app.UseRateLimiter();
app.MapControllers();
#endregion


app.Run();
