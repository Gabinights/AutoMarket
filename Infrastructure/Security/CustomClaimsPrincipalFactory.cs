using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Models.Enums;
using AutoMarket.Models.Entities;
using AutoMarket.Infrastructure.Data;

namespace AutoMarket.Infrastructure.Security
{
    /// <summary>
    /// Factory customizado para adicionar claims específicas do AutoMarket ao ClaimsPrincipal.
    /// Adiciona VendedorId, StatusVendedor e CompradorId para evitar queries repetidas à BD.
    /// </summary>
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<Utilizador, IdentityRole>
    {
        private readonly ApplicationDbContext _context;

        public CustomClaimsPrincipalFactory(
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ApplicationDbContext context)
            : base(userManager, roleManager, optionsAccessor)
        {
            _context = context;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Utilizador user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // 1. Injetar Claim de Vendedor se existir
            var vendedor = await _context.Vendedores
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.UserId == user.Id);

            if (vendedor != null)
            {
                identity.AddClaim(new Claim("VendedorId", vendedor.Id.ToString()));
                identity.AddClaim(new Claim("StatusVendedor", vendedor.Status.ToString())); // Útil para Policies
            }

            // 2. Injetar Claim de Comprador se existir
            var comprador = await _context.Compradores
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (comprador != null)
            {
                identity.AddClaim(new Claim("CompradorId", comprador.Id.ToString()));
            }
            
            // 3. NIF NÃO deve ser adicionado (RGPD compliance)
            // O NIF está encriptado na BD e não deve ser exposto no cookie, mesmo que encriptado.
            // Se necessário, desencriptar apenas quando necessário em operações específicas.

            return identity;
        }
    }
}

