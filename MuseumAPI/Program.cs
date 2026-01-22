using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MuseumAPI.Data;
using MuseumAPI.Services;
using MuseumAPI.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

// =======================
// Kestrel (HTTP + HTTPS)
// =======================
// DO NOT bind 0.0.0.0 for HTTPS in dev
builder.WebHost.UseUrls(
    "http://0.0.0.0:7074",      // Flutter / Android
    "https://localhost:7074"   // Swagger / Browser
);

// =======================
// Services
// =======================
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// =======================
// CORS (Flutter)
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =======================
// Authentication
// =======================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
});

//builder.Services
//    .AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    })
//    // Temporary external cookie for Google
//    .AddCookie("External")
//    .AddGoogle("Google", options =>
//    {
//        options.ClientId = "399756093714-isk7k30iuu7r1gr8o96d5usfmcqjevjj.apps.googleusercontent.com";
//        options.ClientSecret = "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";
//        //options.ClientId = "547069491176-rs7qkshfeoubgsm5ibj670l22jal83dq.apps.googleusercontent.com";
//        //options.ClientSecret = "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"; // for mvc project

//        options.SignInScheme = "External";
//    });

// =======================
// MVC + Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =======================
// Middleware
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();   // HTTPS only affects browser (7075)
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run(); //21-01-26






//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.Google;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using MuseumAPI.Data;
//using MuseumAPI.Services;
//using MuseumAPI.Middleware;
//using System.Text;

////var builder = WebApplication.CreateBuilder(args);

////builder.Services.AddDbContext<AppDbContext>(options =>
////    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));
//// Bind Kestrel to all interfaces (0.0.0.0) on port 7074 so Android device can reach it
//builder.WebHost.UseUrls("http://0.0.0.0:7074");

//builder.Services.AddScoped<ILoginService, LoginService>();
//builder.Services.AddScoped<ICustomerService, CustomerService>();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
//    });
//});

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(
//            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
//})
//.AddCookie("External") // 👈 TEMP scheme ONLY for Google
//.AddGoogle("Google", options =>
//{
//    options.ClientId = "547069491176-rs7qkshfeoubgsm5ibj670l22jal83dq.apps.googleusercontent.com";
//    options.ClientSecret = "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";

//    options.SignInScheme = "External"; // ✅ THIS FIXES THE ERROR
//});


//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseCors("AllowAll");

//app.UseAuthentication();
//app.UseMiddleware<AuthenticationMiddleware>();
//app.UseAuthorization();



//app.MapControllers();

//app.Run();