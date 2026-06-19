using StackExchange.Redis;

namespace API.Middleware;

public class AnonymousSessionMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
{
    private readonly RequestDelegate _next = next;
    private readonly IConnectionMultiplexer _redis = redis;

    private const string anonymousUserCookieName = "AnonyId";

    public async Task InvokeAsync(HttpContext context)
    {
        var redis = _redis.GetDatabase();

        string? anonymousId = context.Request.Cookies[anonymousUserCookieName];
        
        bool isLoggedIn = context.User.Identity?.IsAuthenticated ?? false;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            string userId = context.User.Identity?.Name!;
            if (!string.IsNullOrEmpty(userId))
            {
                context.Items["BasketKey"] = $"basket:user:{userId}";
            }

            

            if (!string.IsNullOrEmpty(anonymousId))
            {
                // Merge anonymous basket if it exists
                await MergeAnonymousBasket(anonymousId, userId, redis);

                // Delete AnonymousId cookie (no longer needed)
                context.Response.Cookies.Delete(anonymousUserCookieName);
            }

        }
        else
        {
            anonymousId = GetOrCreateAnonymousId(context);

            context.Items["BasketKey"] = $"basket:anonymous:{anonymousId}";
        }

        await _next(context);
    }

    private static string GetOrCreateAnonymousId(HttpContext context)
    {
        string? anonymousId = context.Request.Cookies[anonymousUserCookieName];

        if (string.IsNullOrEmpty(anonymousId))
        {
            // Create NEW AnonymousId (first visit)
            anonymousId = Guid.NewGuid().ToString();

            context.Response.Cookies.Append(anonymousUserCookieName, anonymousId, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(7), // Persistent cookie
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
                // For production with HTTPS:
                // Secure = CookieSecurePolicy.Require,
            });
        }

        return anonymousId;
    }

    public static async Task MergeAnonymousBasket(
        string anonymousId, 
        string userId, 
        IDatabase redis)
    {
        string anonKey = $"basket:anonymous:{anonymousId}";
        string userKey = $"basket:user:{userId}";
        
        // 1. Get anonymous basket (all products + quantities)
        var anonCart = await redis.HashGetAllAsync(anonKey);
        
        if (anonCart.Length == 0)
        {
            // Delete empty key (cleanup)
            await redis.KeyDeleteAsync(anonKey);
            return;
        }
        
        foreach (var entry in anonCart)
        {
            await redis.HashIncrementAsync(userKey, entry.Name, (int)entry.Value);
        }
        
        await redis.KeyDeleteAsync(anonKey);

        if(await redis.KeyExistsAsync(userKey))
        {
            // if the useKey not exist in redis, this code does nothing. 
            await redis.KeyExpireAsync(userKey, TimeSpan.FromDays(7));
        }
    }
}