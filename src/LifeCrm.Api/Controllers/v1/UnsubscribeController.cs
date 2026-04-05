using System.Security.Cryptography;
using System.Text;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Api.Controllers.v1
{
    /// <summary>
    /// Public (no auth) endpoint that processes unsubscribe link clicks.
    /// Token = HMAC-SHA256 of "{contactId}:{orgId}" signed with the JWT secret key.
    /// </summary>
    [AllowAnonymous]
    public class UnsubscribeController : ApiControllerBase
    {
        private readonly AppDbContext             _db;
        private readonly IConfiguration           _config;
        private readonly ILogger<UnsubscribeController> _logger;

        public UnsubscribeController(
            AppDbContext db, IConfiguration config,
            ILogger<UnsubscribeController> logger)
        { _db = db; _config = config; _logger = logger; }

        /// <summary>
        /// GET /api/v1/unsubscribe?token={token}
        /// Validates the HMAC token, sets EmailOptOut = true, returns HTML confirmation.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Unsubscribe(
            [FromQuery] string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
                return HtmlPage("Ogiltig länk", "Avregistreringslänken saknar en token.", ok: false);

            if (!TryDecodeToken(token, out var contactId, out var orgId))
                return HtmlPage("Ogiltig länk",
                    "Länken är ogiltig eller har löpt ut. Kontakta organisationen direkt.", ok: false);

            var contact = await _db.Contacts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    c => c.Id == contactId && c.OrganizationId == orgId && !c.IsDeleted, ct);

            if (contact is null)
                return HtmlPage("Kontakt hittades inte",
                    "Vi kunde inte hitta dina uppgifter. Du kan ha redan avregistrerats.", ok: false);

            if (contact.EmailOptOut)
                return HtmlPage("Redan avregistrerad",
                    $"E-postadressen är redan avregistrerad från e-postutskick.", ok: true);

            contact.EmailOptOut    = true;
            contact.LastModifiedAt = DateTimeOffset.UtcNow;
            contact.LastModifiedBy = "unsubscribe-link";
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Contact {ContactId} unsubscribed via email link.", contactId);

            return HtmlPage("Avregistrerad",
                "Du har avregistrerats från våra e-postutskick. " +
                "Kontakta oss direkt om du vill prenumerera igen.", ok: true);
        }

        // ── Token helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Generate a signed token for a contact. Called from SendNewsletterHandler
        /// to append an unsubscribe footer to each email.
        /// </summary>
        public static string GenerateToken(Guid contactId, Guid orgId, string secretKey)
        {
            var payload = $"{contactId}:{orgId}";
            var key     = Encoding.UTF8.GetBytes(secretKey);
            var data    = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(key);
            var sig = hmac.ComputeHash(data);

            // Base64Url encode: payload.signature
            var payloadB64 = Convert.ToBase64String(data)
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var sigB64 = Convert.ToBase64String(sig)
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            return $"{payloadB64}.{sigB64}";
        }

        private bool TryDecodeToken(
            string token, out Guid contactId, out Guid orgId)
        {
            contactId = Guid.Empty;
            orgId     = Guid.Empty;

            try
            {
                var parts = token.Split('.');
                if (parts.Length != 2) return false;

                static byte[] FromBase64Url(string s)
                {
                    s = s.Replace('-', '+').Replace('_', '/');
                    s += new string('=', (4 - s.Length % 4) % 4);
                    return Convert.FromBase64String(s);
                }

                var payloadBytes = FromBase64Url(parts[0]);
                var sigBytes     = FromBase64Url(parts[1]);

                var secretKey = _config.GetSection("Jwt")["SecretKey"]
                    ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");
                var key = Encoding.UTF8.GetBytes(secretKey);

                using var hmac     = new HMACSHA256(key);
                var expectedSig    = hmac.ComputeHash(payloadBytes);
                if (!CryptographicOperations.FixedTimeEquals(sigBytes, expectedSig))
                    return false;   // signature mismatch

                var payload = Encoding.UTF8.GetString(payloadBytes);
                var segments = payload.Split(':');
                if (segments.Length != 2) return false;

                contactId = Guid.Parse(segments[0]);
                orgId     = Guid.Parse(segments[1]);
                return true;
            }
            catch { return false; }
        }

        private ContentResult HtmlPage(string title, string message, bool ok) =>
            Content($"""
                <!DOCTYPE html>
                <html lang="sv">
                <head>
                  <meta charset="utf-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1" />
                  <title>{title} — LifeCrm</title>
                  <style>
                    body {{ font-family: sans-serif; display:flex; justify-content:center;
                           align-items:center; min-height:100vh; margin:0; background:#f5f5f5; }}
                    .card {{ background:#fff; border-radius:8px; padding:40px 48px;
                             box-shadow:0 2px 12px rgba(0,0,0,.1); max-width:480px; text-align:center; }}
                    h1 {{ font-size:1.5rem; margin-bottom:12px;
                          color:{(ok ? "#2e7d32" : "#c62828")}; }}
                    p {{ color:#555; line-height:1.6; }}
                  </style>
                </head>
                <body>
                  <div class="card">
                    <h1>{title}</h1>
                    <p>{message}</p>
                  </div>
                </body>
                </html>
                """,
                "text/html");
    }
}
