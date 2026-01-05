# Análise: CustomClaimsPrincipalFactory

## ✅ A Proposta Faz Sentido

A implementação de `CustomClaimsPrincipalFactory` é **benéfica** para o projeto porque:

1. **Performance**: Reduz queries à BD que são feitas repetidamente
   - `VeiculosController.GetVendedorLogado()` faz query em cada request
   - `CheckoutController` faz query para obter `Comprador`
   - `VendedorAprovadoHandler` faz query para obter `Status`

2. **Simplicidade**: Facilita acesso aos IDs sem queries
   - `User.FindFirst("VendedorId")?.Value` em vez de query à BD
   - `User.FindFirst("CompradorId")?.Value` em vez de query à BD

3. **Otimização de Policies**: `VendedorAprovadoHandler` pode usar a claim `StatusVendedor` diretamente

## ⚠️ Correções Necessárias

### 1. Classe Base e Método
A proposta está **correta**:
- `UserClaimsPrincipalFactory<Utilizador, IdentityRole>` ✅
- `GenerateClaimsAsync(Utilizador user)` ✅

### 2. NIF Não Deve Ser Adicionado
O código já está correto (comentado):
```csharp
// CUIDADO: Isto coloca o NIF no Cookie. Só fazer se o Cookie for encriptado/seguro.
// if (!string.IsNullOrEmpty(user.NIF)) {
//     identity.AddClaim(new Claim("NIF", user.NIF)); 
// }
```
**Razão**: O NIF está encriptado na BD (RGPD compliance). Não deve ser exposto no cookie mesmo que encriptado.

### 3. Refresh do Cookie Quando Status Muda
**Problema**: Se o `StatusVendedor` mudar (ex: de Pendente para Aprovado), o cookie precisa ser refrescado.

**Solução**: Após aprovação/rejeição no `AdminController`, fazer refresh do cookie:
```csharp
await _signInManager.RefreshSignInAsync(user);
```

### 4. Otimização do VendedorAprovadoHandler
Após implementar as claims, o handler pode ser otimizado:

**Antes** (faz query à BD):
```csharp
var status = await _context.Vendedores
    .Where(v => v.UserId == userId)
    .Select(v => v.Status)
    .FirstOrDefaultAsync();
```

**Depois** (usa claim):
```csharp
var statusClaim = context.User.FindFirst("StatusVendedor")?.Value;
if (Enum.TryParse<StatusAprovacao>(statusClaim, out var status) 
    && status == StatusAprovacao.Aprovado)
{
    context.Succeed(requirement);
}
```

## 📝 Implementação Recomendada

### 1. Criar o Ficheiro
**Localização**: `Security/CustomClaimsPrincipalFactory.cs`

### 2. Código Corrigido
```csharp
using System.Security.Claims;
using AutoMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Data;
using AutoMarket.Models.Enums;

namespace AutoMarket.Security
{
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
                identity.AddClaim(new Claim("StatusVendedor", vendedor.Status.ToString()));
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
            // O NIF está encriptado na BD e não deve ser exposto no cookie

            return identity;
        }
    }
}
```

### 3. Atualizar Program.cs
Adicionar após `.AddDefaultTokenProviders()`:
```csharp
.AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();
```

### 4. Otimizar VeiculosController (Opcional)
Pode simplificar `GetVendedorLogado()`:
```csharp
private int? GetVendedorIdFromClaims()
{
    var vendedorIdClaim = User.FindFirst("VendedorId")?.Value;
    return int.TryParse(vendedorIdClaim, out var id) ? id : null;
}
```

**Nota**: Se precisar do objeto `Vendedor` completo, manter a query. Se apenas precisar do ID, usar a claim.

### 5. Atualizar AdminController
Após aprovar/rejeitar vendedor, refrescar o cookie:
```csharp
var user = await _userManager.FindByIdAsync(vendedor.UserId);
if (user != null)
{
    await _signInManager.RefreshSignInAsync(user);
}
```

## ⚠️ Considerações Importantes

1. **Tamanho do Cookie**: Claims adicionam dados ao cookie. Monitorar se necessário.
2. **Cache de Claims**: Claims são geradas uma vez por login. Mudanças na BD não refletem automaticamente.
3. **Refresh Necessário**: Quando `StatusVendedor` muda, fazer refresh do cookie.
4. **Segurança**: Cookies já estão configurados como `HttpOnly` e `Secure` no `Program.cs` ✅

## ✅ Conclusão

A implementação é **recomendada** com as seguintes ações:
1. ✅ Criar `CustomClaimsPrincipalFactory`
2. ✅ Registrar no `Program.cs`
3. ⚠️ Atualizar `AdminController` para refrescar cookie após mudanças de status
4. 🔄 (Opcional) Otimizar `VendedorAprovadoHandler` para usar claims
5. 🔄 (Opcional) Simplificar `GetVendedorLogado()` se apenas ID for necessário

