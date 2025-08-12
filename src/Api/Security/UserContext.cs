using Application.Abstractions;
using System.Security.Claims;

namespace Api.Security
{
    public sealed class UserContext : IUserContext
    {
        public Guid UserId { get; }

        public UserContext(IHttpContextAccessor accessor, IConfiguration config)
        {
            var ctx = accessor.HttpContext ?? throw new InvalidOperationException("No HttpContext");
            var headerKey = config.GetValue<string>("Auth:TrustedHeader") ?? "X-User-Id";

            // 1) ผ่าน API Gateway
            if (ctx.Request.Headers.TryGetValue(headerKey, out var vals) && Guid.TryParse(vals.FirstOrDefault(), out var idFromHeader))
            {
                UserId = idFromHeader;
                return;
            }

            // 2) ใช้ JWT ตรง ๆ (optional)
            var sub = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ctx.User.FindFirstValue("sub");
            if (Guid.TryParse(sub, out var idFromJwt))
            {
                UserId = idFromJwt;
                return;
            }

            throw new InvalidOperationException("UserId was not found in header or token");
        }
    }
}
