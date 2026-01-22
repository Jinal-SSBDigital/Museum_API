//namespace MuseumAPI.Middleware
//{
//    public class AuthenticationMiddleware
//    {
//        private readonly RequestDelegate _next;

//        public AuthenticationMiddleware(RequestDelegate next)
//        {
//            _next = next;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            var path = context.Request.Path.Value?.ToLower() ?? "";

//            // 1. Allow OPTIONS (CORS preflight)
//            if (HttpMethods.IsOptions(context.Request.Method))
//            {
//                await _next(context);
//                return;
//            }

//            // 2. Add 'signin-google' to exclusions (Google's internal handler)
//            if (path.Contains("/api/account/login") ||
//                path.Contains("/api/account/register") ||
//                path.Contains("/api/account/google-login") ||
//                path.Contains("/api/account/google-callback") ||
//                path.Contains("/signin-google")) // Crucial for the internal Google handler
//            {
//                await _next(context);
//                return;
//            }

//            //        if (path.Contains("/api/account/google-login") ||
//            //path.Contains("/api/account/google-callback") ||
//            //path.Contains("/signin-google"))
//            //        {
//            //            await _next(context);
//            //            return;
//            //        }


//            // 3. Only enforce JWT for other API calls
//            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
//            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
//            {
//                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                await context.Response.WriteAsync("Unauthorized access. Missing token.");
//                return;
//            }

//            await _next(context);
//        }
//    }
//}

using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MuseumAPI.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public AuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 1️⃣ Allow preflight (CORS)
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // 2️⃣ Allow public endpoints (NO TOKEN REQUIRED)
            if (path.StartsWith("/api/account/login") ||
                path.StartsWith("/api/account/register") ||
                path.StartsWith("/api/account/google-login"))
            {
                await _next(context);
                return;
            }

            // 3️⃣ Check Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authHeader) ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Missing token");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // 4️⃣ Validate JWT
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ClockSkew = TimeSpan.Zero
                }, out _);

                // ✅ Token valid → continue to controller
                await _next(context);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid or expired token");
            }
        }
    }
}
