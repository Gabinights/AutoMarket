using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace AutoMarket.Infrastructure.Security
{
    public class UserBlockingMiddleware
    {
        private readonly RequestDelegate _next; private readonly IMemoryCache _cache; private readonly ILogger<UserBlockingMiddleware> _logger;

        public UserBlockingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<UserBlockingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Chave de cache única para o estado do utilizador
                    var cacheKey = $"User_Status_{userId}";

                    // Tenta obter do cache. Se não existir, vai à BD.
                    if (!_cache.TryGetValue(cacheKey, out bool isAllowed))
                    {
                        // Scope necessário para injetar UserManager (que é Scoped) dentro do Middleware (que é Singleton)
                        // Isto é crucial para evitar memory leaks e erros de concorrência
                        using (var scope = context.RequestServices.CreateScope())
                        {
                            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Utilizador>>();
                            var user = await userManager.FindByIdAsync(userId);

                            // Regras de bloqueio:
                            // 1. User não existe (foi hard deleted ou soft deleted e o UserStore filtrou)
                            // 2. User tem flag IsBlocked
                            // 3. User tem flag IsDeleted (caso o UserStore não tenha filtrado)
                            if (user == null || user.IsBlocked || user.IsDeleted)
                            {
                                isAllowed = false;
                            }
                            else
                            {
                                isAllowed = true;
                            }

                            // Guardar em cache por 10 minutos (reduz load na BD)
                            // Se o admin bloquear, o serviço deve invalidar/atualizar esta entrada manualmente
                            var cacheOptions = new MemoryCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                                .SetPriority(CacheItemPriority.High);

                            _cache.Set(cacheKey, isAllowed, cacheOptions);
                        }
                    }

                    if (!isAllowed)
                    {
                        _logger.LogWarning("Utilizador {UserId} tentou aceder mas está bloqueado/eliminado. Efetuando logout forçado.", userId);

                        // Forçar Logout imediato
                        await context.SignOutAsync(IdentityConstants.ApplicationScheme);

                        // Redirecionar para página de bloqueio ou login
                        context.Response.Redirect("/Public/Conta/Login");
                        return; // Interrompe o pipeline aqui
                    }
                }
            }

            await _next(context);
        }
    }
}