using AutoMarket.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Data
{
    /// <summary>
    /// UserStore customizado que filtra utilizadores deletados (soft delete) em todas as operações do Identity.
    /// </summary>
    public class CustomUserStore : UserStore<Utilizador>
    {
        public CustomUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null)
            : base(context, describer)
        {
        }

        public override async Task<Utilizador?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            // IgnoreQueryFilters para contornar o filtro global e depois verificar manualmente
            var user = await Context.Set<Utilizador>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            return user != null && !user.IsDeleted ? user : null;
        }

        public override async Task<Utilizador?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            // IgnoreQueryFilters para contornar o filtro global e depois verificar manualmente
            var user = await Context.Set<Utilizador>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
            return user != null && !user.IsDeleted ? user : null;
        }

        public override async Task<Utilizador?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            // IgnoreQueryFilters para contornar o filtro global e depois verificar manualmente
            var user = await Context.Set<Utilizador>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
            return user != null && !user.IsDeleted ? user : null;
        }

        public override IQueryable<Utilizador> Users => base.Users.Where(u => !u.IsDeleted);
    }
}

