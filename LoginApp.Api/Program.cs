using LoginApp.Business.Services;
using LoginApp.Business.Services.Interfaces;
using LoginApp.DataAccess.Data;
using LoginApp.DataAccess.Entities;
using LoginApp.DataAccess.Repositories;
using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

// Register repositories & services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<ITaskItemRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();


builder.Services.AddScoped<ITaskItemRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
// Enable controllers
builder.Services.AddControllers();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


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


var app = builder.Build();

app.UseCors("AllowAll");



app.UseAuthentication(); //Reads the JWT from the request header, validates it, and builds HttpContext.User
app.UseAuthorization();  //Checks [Authorize] attributes and decides whether to allow or deny access

app.UseFileServer(); //combining the next tow methods
//app.UseDefaultFiles();  // looks for index.html by default
//app.UseStaticFiles();   // serves HTML, CSS, JS


app.MapControllers();


app.Run();

