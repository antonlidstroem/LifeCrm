using Microsoft.AspNetCore.Http;

namespace LifeCrm.Api.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        public SecurityHeadersMiddleware(RequestDelegate next) { _next = next; }

        public async Task InvokeAsync(HttpContext context)
        {
            var h = context.Response.Headers;
            h["X-Content-Type-Options"]  = "nosniff";
            h["X-Frame-Options"]         = "DENY";
            h["Referrer-Policy"]         = "strict-origin-when-cross-origin";
            h["X-XSS-Protection"]        = "1; mode=block";
            h["Permissions-Policy"]      = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
            h["Content-Security-Policy"] =
             "default-src 'self'; " +
             "script-src 'self' 'wasm-unsafe-eval'; " +
             "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
             "font-src 'self' https://fonts.gstatic.com; " +
             "img-src 'self' data:; " +
             // ÄNDRING HÄR: Tillåt anslutningar till localhost på alla portar (för API + Hot Reload)
             "connect-src 'self' https://localhost:* http://localhost:* wss://localhost:* ws://localhost:*; " +
             "frame-ancestors 'none'; " +
             "base-uri 'self'; " +
             "form-action 'self';";
            await _next(context);
        }
    }
}
