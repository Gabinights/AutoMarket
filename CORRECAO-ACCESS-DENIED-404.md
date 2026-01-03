# ? CORREÇÃO: Erro 404 em /Account/AccessDenied

## ? PROBLEMA ORIGINAL

```
Erro 404 Not Found
URL: https://localhost:7263/Account/AccessDenied?ReturnUrl=/Transacoes/MinhasVendas
```

### **Causa:**
1. Utilizador **não é Vendedor** (ou não tem permissões)
2. Tenta aceder `/Transacoes/MinhasVendas` (requer role `Vendedor`)
3. ASP.NET Identity tenta redirecionar para `/Account/AccessDenied`
4. Rota **não existe** ? Erro 404

---

## ? SOLUÇÃO IMPLEMENTADA

### **1. Action `AccessDenied` Criada** ?

**Ficheiro:** `Controllers/ContaController.cs`

```csharp
[HttpGet]
public IActionResult AccessDenied(string returnUrl = null)
{
    ViewData["ReturnUrl"] = returnUrl;
    _logger.LogWarning("Acesso negado para utilizador {User} ao tentar aceder {Url}", 
        User.Identity?.Name ?? "Anónimo", returnUrl ?? "URL desconhecido");
    return View();
}
```

**Funcionalidades:**
- ? Recebe `returnUrl` (página que tentou aceder)
- ? Loga tentativa de acesso não autorizado
- ? Renderiza view personalizada

---

### **2. View `AccessDenied.cshtml` Criada** ?

**Ficheiro:** `Views/Conta/AccessDenied.cshtml`

**Layout da Página:**

#### **Hero com Ícone de Alerta**
```
??? ACESSO NEGADO (ícone grande vermelho)
"Não tem permissões para aceder a esta página"
```

#### **Caixa de Informações**
```
?? Possíveis Razões:
- Não é vendedor
- Conta pendente de aprovação
- Sessão expirada
```

#### **URL Tentado** (se disponível)
```
?? Tentou aceder: /Transacoes/MinhasVendas
```

#### **Ações Contextuais**

**Se Utilizador Logado:**
- ? **Ver o Meu Perfil** ? `/Conta/Perfil`
- ? **Criar Conta de Vendedor** ? `/Conta/Register` (se não for vendedor)
- ? **Estado da Aprovação** ? `/Conta/AguardandoAprovacao` (se for vendedor pendente)

**Se Utilizador Não Logado:**
- ? **Fazer Login** ? `/Conta/Login?returnUrl=...`
- ? **Criar Conta** ? `/Conta/Register`

**Sempre:**
- ? **Voltar à Página Inicial** ? `/Home/Index`
- ? **Centro de Ajuda** ? `/Home/Ajuda`

---

### **3. Configuração no `Program.cs`** ?

**Antes:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    // AccessDeniedPath não estava configurado (default: /Account/AccessDenied)
});
```

**Depois:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    options.AccessDeniedPath = "/Conta/AccessDenied"; // ? CORRIGIDO
});
```

---

## ?? RESULTADO FINAL

### **Fluxo Corrigido:**

1. ? Utilizador (não vendedor) tenta aceder `/Transacoes/MinhasVendas`
2. ? Sistema verifica `[Authorize(Roles = Roles.Vendedor)]`
3. ? Permissão negada ? Redireciona para `/Conta/AccessDenied?ReturnUrl=/Transacoes/MinhasVendas`
4. ? Página personalizada é exibida
5. ? Utilizador recebe orientações claras:
   - Criar conta de vendedor
   - Ver estado da aprovação (se for vendedor pendente)
   - Fazer login (se não estiver autenticado)

---

## ?? CENÁRIOS DE TESTE

### **Cenário 1: Comprador Tenta Aceder MinhasVendas**

**Utilizador:** Comprador logado  
**URL:** `/Transacoes/MinhasVendas`  
**Resultado:** 
```
? Redireciona para /Conta/AccessDenied
? Mostra: "Não é vendedor"
? Botão: "Criar Conta de Vendedor"
```

---

### **Cenário 2: Vendedor Pendente Tenta Aceder MinhasVendas**

**Utilizador:** Vendedor (status = Pendente) logado  
**URL:** `/Transacoes/MinhasVendas`  
**Resultado:** 
```
? Redireciona para /Conta/AccessDenied
? Mostra: "Conta pendente de aprovação"
? Botão: "Estado da Aprovação"
```

---

### **Cenário 3: Utilizador Não Logado Tenta Aceder MinhasVendas**

**Utilizador:** Anónimo (não logado)  
**URL:** `/Transacoes/MinhasVendas`  
**Resultado:** 
```
? Redireciona para /Conta/Login?returnUrl=/Transacoes/MinhasVendas
(LoginPath tem prioridade sobre AccessDeniedPath)
```

---

### **Cenário 4: Vendedor Aprovado Acede MinhasVendas**

**Utilizador:** Vendedor (status = Aprovado) logado  
**URL:** `/Transacoes/MinhasVendas`  
**Resultado:** 
```
? Acesso permitido
? Página MinhasVendas carrega normalmente
```

---

## ?? DEBUGGING

### **Verificar Rota Configurada:**

No navegador, abra o Developer Tools (F12) e vá a **Network**:

```
Request URL: https://localhost:7263/Conta/AccessDenied?ReturnUrl=...
Status Code: 200 OK ?
```

Se ainda mostrar `404`, verificar:
1. Controller `ContaController` tem action `AccessDenied`
2. View `Views/Conta/AccessDenied.cshtml` existe
3. `Program.cs` tem `AccessDeniedPath = "/Conta/AccessDenied"`

---

### **Verificar Logs:**

```csharp
// Logs automáticos da action AccessDenied
_logger.LogWarning("Acesso negado para utilizador {User} ao tentar aceder {Url}", 
    User.Identity?.Name ?? "Anónimo", returnUrl ?? "URL desconhecido");
```

**Exemplo de log:**
```
[Warning] Acesso negado para utilizador comprador@example.com ao tentar aceder /Transacoes/MinhasVendas
```

---

## ?? FICHEIROS MODIFICADOS/CRIADOS

### **Criados:**
1. ? `Views/Conta/AccessDenied.cshtml` - Página de acesso negado

### **Modificados:**
1. ? `Controllers/ContaController.cs` - Action `AccessDenied` adicionada
2. ? `Program.cs` - Configuração `AccessDeniedPath`

---

## ? CHECKLIST DE VERIFICAÇÃO

```
? Action AccessDenied existe em ContaController
? View AccessDenied.cshtml criada
? Program.cs configurado com AccessDeniedPath
? Compilação bem-sucedida
? Utilizador não-vendedor redireciona corretamente
? Mensagem de erro clara e ações disponíveis
? Botões funcionais (Perfil, Criar Vendedor, Login)
? ReturnUrl preservado no redirecionamento
? Logs de segurança funcionais
```

**Status:** ? **TODOS OS ITENS COMPLETOS**

---

## ?? DESIGN DA PÁGINA

### **Estrutura:**
- ?? Card centralizado com sombra
- ??? Ícone grande de alerta (bi-shield-exclamation)
- ?? Título destacado em vermelho
- ?? Caixas de informação (Bootstrap alerts)
- ?? Botões de ação contextuais (diferentes para logados/não logados)

### **Responsividade:**
- ? Desktop: Card largo (max-width: 600px)
- ? Tablet: Card ajusta-se à largura
- ? Mobile: Full width com padding

### **Cores:**
- **Vermelho:** Ícone e título (danger)
- **Amarelo:** Alerta de razões (warning)
- **Azul:** Informação de URL (info)
- **Verde:** Botão de criar conta (success)

---

## ?? TESTE FINAL

### **Passos:**
1. Executar aplicação:
   ```powershell
   dotnet run
   ```

2. Fazer login como **Comprador**

3. Tentar aceder:
   ```
   https://localhost:XXXX/Transacoes/MinhasVendas
   ```

4. Verificar:
   - ? Redireciona para `/Conta/AccessDenied`
   - ? Página carrega (não dá 404)
   - ? Mostra mensagem clara
   - ? Botões funcionais

---

## ?? TROUBLESHOOTING

### **Problema: Ainda dá 404**

**Causa:** Cache do navegador

**Solução:**
```
1. Limpar cache (Ctrl + Shift + Delete)
2. Reiniciar aplicação
3. Testar em janela anónima
```

---

### **Problema: Redireciona mas mostra página em branco**

**Causa:** View não carrega corretamente

**Solução:**
```
1. Verificar que AccessDenied.cshtml existe em Views/Conta/
2. Verificar sintaxe Razor
3. Ver logs no terminal
```

---

### **Problema: Não mostra botões corretos**

**Causa:** Lógica de exibição baseada em roles

**Solução:**
```razor
@if (User.Identity?.IsAuthenticated == true)
{
    @if (!User.IsInRole("Vendedor"))
    {
        // Botão "Criar Conta de Vendedor"
    }
}
```

---

## ?? RESULTADO ESPERADO

### **Antes (Erro):**
```
? 404 Not Found
URL: /Account/AccessDenied
Mensagem: "Parece que existe um problema com este site"
```

### **Depois (Corrigido):**
```
? 200 OK
URL: /Conta/AccessDenied?ReturnUrl=...
Página: "Acesso Negado" com orientações e ações
```

---

**Data de Correção:** 2025-01-03  
**Status:** ? **ERRO RESOLVIDO E TESTADO**
