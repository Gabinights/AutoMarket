using AutoMarket.Data;
using AutoMarket.Constants;
using AutoMarket.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AutoMarket.Models;

namespace AutoMarket.Security
{
    public class VendedorAprovadoHandler : AuthorizationHandler<VendedorAprovadoRequirement>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public VendedorAprovadoHandler(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            VendedorAprovadoRequirement requirement)
        {
            var userPrincipal = context.User;

            // 1. Verificar se está logado e se é Vendedor
            if (userPrincipal.Identity?.IsAuthenticated != true || !userPrincipal.IsInRole(Roles.Vendedor))
            {
                // Se não é vendedor, a policy não se aplica (ou falha)
                return;
            }

            // 2. Tentar obter StatusVendedor da claim (otimização - evita query à BD)
            var statusClaim = userPrincipal.FindFirst("StatusVendedor")?.Value;
            StatusAprovacao? status = null;

            if (!string.IsNullOrEmpty(statusClaim) && Enum.TryParse<StatusAprovacao>(statusClaim, out var parsedStatus))
            {
                status = parsedStatus;
            }
            else
            {
                // Fallback: Se a claim não existir, fazer query à BD (compatibilidade)
                var userId = _userManager.GetUserId(userPrincipal);
                if (string.IsNullOrEmpty(userId)) return;

                status = await _context.Vendedores
                    .Where(v => v.UserId == userId)
                    .Select(v => v.Status)
                    .FirstOrDefaultAsync();
            }

            // 3. O momento da verdade
            if (status == StatusAprovacao.Aprovado)
            {
                context.Succeed(requirement); // SUCESSO!
            }
            else
            {
                // Se falhar, podemos redirecionar ou deixar o ASP.NET retornar 403 Forbidden.
                // Dica Pro: Para redirecionar dentro de um Handler é preciso lógica extra,
                // geralmente o 403 é intercetado no Program.cs ou deixa-se falhar.

                // Opção Simples: Apenas não chamamos o Succeed. O sistema assume falha.
            }
        }
    }
}