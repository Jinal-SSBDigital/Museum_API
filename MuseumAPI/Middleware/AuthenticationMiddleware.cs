namespace MuseumAPI.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 1. Allow OPTIONS (CORS preflight)
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // 2. Add 'signin-google' to exclusions (Google's internal handler)
            if (path.Contains("/api/account/login") ||
                path.Contains("/api/account/register") ||
                path.Contains("/api/account/google-login") ||
                path.Contains("/api/account/google-callback") ||
                path.Contains("/signin-google")) // Crucial for the internal Google handler
            {
                await _next(context);
                return;
            }

            //        if (path.Contains("/api/account/google-login") ||
            //path.Contains("/api/account/google-callback") ||
            //path.Contains("/signin-google"))
            //        {
            //            await _next(context);
            //            return;
            //        }


            // 3. Only enforce JWT for other API calls
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized access. Missing token.");
                return;
            }

            await _next(context);
        }
    }
}