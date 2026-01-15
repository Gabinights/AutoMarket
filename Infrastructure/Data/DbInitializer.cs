using AutoMarket.Models.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilizador>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

            // 1. Roles
            string[] roles = { Roles.Admin, Roles.Vendedor, Roles.Comprador };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Default Admin
            var adminEmail = configuration["DefaultAdmin:Email"];
            if (string.IsNullOrEmpty(adminEmail))
            {
                if (environment.IsProduction())
                {
                    throw new InvalidOperationException("O email do Admin não está configurado (DefaultAdmin:Email).");
                }
                adminEmail = "admin@automarket.com";
            }
            var adminPwd = configuration["DefaultAdmin:Password"];

            if (string.IsNullOrEmpty(adminPwd))
            {
                if (environment.IsProduction())
                {
                    throw new InvalidOperationException("A password do Admin não está configurada (DefaultAdmin:Password).");
                }
                adminPwd = "Password1231";
            }

            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                var adminUser = await EnsureUserAsync(userManager, adminEmail, adminPwd, "Administrador Sistema", Roles.Admin);

                if (environment.IsDevelopment())
                {
                    // 3. Comprador Teste
                    var compradorUser = await EnsureUserAsync(userManager, "comprador@automarket.com", "Password123!", "Comprador Exemplo", Roles.Comprador);
                    if (compradorUser != null)
                    {
                        if (!await context.Compradores.AnyAsync(c => c.UserId == compradorUser.Id))
                        {
                            context.Compradores.Add(new Comprador
                            {
                                UserId = compradorUser.Id,
                                ReceberNotificacoes = true
                            });
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // 4. Vendedor Particular (Aprovado)
                var vendedorPartUser = await EnsureUserAsync(userManager, "vendedor_part@automarket.com", "Password123!", "Vendedor Particular", Roles.Vendedor);
                if (vendedorPartUser != null)
                {
                    if (!await context.Vendedores.AnyAsync(v => v.UserId == vendedorPartUser.Id))
                    {
                        context.Vendedores.Add(new Vendedor
                        {
                            UserId = vendedorPartUser.Id,
                            TipoConta = TipoConta.Vendedor,
                            Status = StatusAprovacao.Aprovado,
                            ApprovedByAdminId = adminUser?.Id 
                        });
                        await context.SaveChangesAsync();
                    }
                }

                // 5. Vendedor Empresa (Aprovado)
                var vendedorEmpUser = await EnsureUserAsync(userManager, "vendedor_emp@automarket.com", "Password123!", "Stand AutoMarket Lda", Roles.Vendedor);
                if (vendedorEmpUser != null)
                {
                    if (!await context.Vendedores.AnyAsync(v => v.UserId == vendedorEmpUser.Id))
                    {
                        context.Vendedores.Add(new Vendedor
                        {
                            UserId = vendedorEmpUser.Id,
                            TipoConta = TipoConta.Empresa,
                            Status = StatusAprovacao.Aprovado,
                            ApprovedByAdminId = adminUser?.Id
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static async Task<Utilizador?> EnsureUserAsync(UserManager<Utilizador> userManager, string email, string password, string nome, string role)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new Utilizador
                {
                    UserName = email,
                    Email = email,
                    Nome = nome,
                    Contacto = "N/A", 
                    EmailConfirmed = true,
                    DataRegisto = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"[DBInitializer] Erro ao criar utilizador {email}: {error.Description}");
                    }
                    return null;
                }
            }
            return user;
        }
    }
}