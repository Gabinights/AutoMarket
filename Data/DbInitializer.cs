using AutoMarket.Constants; // Onde tens a classe Roles
using AutoMarket.Models;
using Microsoft.AspNetCore.Identity;

namespace AutoMarket.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // 1. Obter os serviços necessários
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilizador>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // 2. Garantir que as Roles existem
            string[] roles = { Roles.Admin, Roles.Vendedor, Roles.Comprador };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. Verificar se existe algum Admin
            // Procuramos por qualquer utilizador que tenha a Role 'Admin'
            var usersInRole = await userManager.GetUsersInRoleAsync(Roles.Admin);

            if (usersInRole.Count == 0)
            {
                // 4. Ler credenciais seguras do appsettings.json ou Secrets
                var adminEmail = configuration["DefaultAdmin:Email"] ?? "admin@automarket.com";
                var adminPassword = configuration["DefaultAdmin:Password"];

                if (string.IsNullOrEmpty(adminPassword))
                {
                    // Log de aviso ou throw exception se for crítico
                    // Para dev, podemos definir uma default, mas em prod é perigoso
                    return;
                }

                var newAdmin = new Utilizador
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nome = "Administrador Sistema",
                    EmailConfirmed = true, // Importante para não bloquear login
                    DataRegisto = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, Roles.Admin);
                }
            }
        }
    }
}