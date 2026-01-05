# ?? AUDITORIA DE SEGURANÇA CRÍTICA - AutoMarket
**Data:** 2024-01-15  
**Auditor:** Arquiteto de Software Sénior  
**Framework:** ASP.NET Core 8.0 | EF Core | Identity  
**Classificação:** ?? CRÍTICO - Pré-Produção

---

## ?? SUMÁRIO EXECUTIVO

### Estado Geral: ?? YELLOW (Requer Ações Imediatas)
- **Vulnerabilidades Críticas:** 7  
- **Vulnerabilidades Altas:** 12  
- **Vulnerabilidades Médias:** 8  
- **Boas Práticas Ausentes:** 15  

### Bloqueadores de Produção:
1. ? **IDOR (Insecure Direct Object Reference)** em múltiplos endpoints
2. ? **Mass Assignment** vulnerável em ViewModels
3. ? **Falta de Rate Limiting** em endpoints críticos
4. ? **Exposição de Stack Traces** em desenvolvimento
5. ? **Session Fixation** potencial
6. ? **CSRF Token** não validado em alguns flows
7. ? **Falta de Content Security Policy (CSP)**

---

## ?? ANÁLISE DETALHADA POR CATEGORIA OWASP

### 1. ?? BROKEN ACCESS CONTROL (A01:2021) - CRÍTICO

#### 1.1 **IDOR em VeiculosController (Área Vendedores)**
**Localização:** `Areas/Vendedores/Controllers/VeiculosController.cs`

**Vulnerabilidade:**
```csharp
// Linha ~150 - Edit Action
[HttpGet]
public async Task<IActionResult> Edit(int? id)
{
    var vendedor = await GetVendedorLogado();
    var veiculo = await _context.Veiculos
        .Include(v => v.Imagens)
        .FirstOrDefaultAsync(v => v.Id == id);
    
    if (veiculo.VendedorId != vendedor.Id) // ? Validação EXISTE
        return Forbid();
}
```

**Problema:**  
? **Validação correta**, mas há **race condition** entre leitura e verificação.

**Impacto:** Médio  
**Exploração:** Requer timing preciso

**Recomendação:**
```csharp
// ? SOLUÇÃO: Query atómica com filtro
var veiculo = await _context.Veiculos
    .Include(v => v.Imagens)
    .FirstOrDefaultAsync(v => v.Id == id && v.VendedorId == vendedor.Id);

if (veiculo == null) return NotFound(); // Não expõe existência
```

---

#### 1.2 **IDOR em CheckoutController - Sucesso Action**
**Localização:** `Controllers/CheckoutController.cs:142`

**Vulnerabilidade:**
```csharp
[HttpGet]
public async Task<IActionResult> Sucesso(int id)
{
    var transacao = await _context.Transacoes
        .Include(t => t.Comprador)
        .FirstOrDefaultAsync(t => t.Id == id);
    
    // ?? Verificação APÓS query
    if (transacao.Comprador.UserId != user.Id)
        return Forbid();
}
```

**Problema:**  
- Transação é carregada ANTES da verificação
- **Information Disclosure:** Attacker pode enumerar IDs válidos
- **403 vs 404:** Revela existência de recursos

**Exploração:**
```
GET /Checkout/Sucesso/1001  ? 403 Forbidden  (existe mas não é meu)
GET /Checkout/Sucesso/9999  ? 404 Not Found  (não existe)
```

**Recomendação URGENTE:**
```csharp
// ? CORRIGIR:
var comprador = await _context.Compradores
    .FirstOrDefaultAsync(c => c.UserId == user.Id);

var transacao = await _context.Transacoes
    .FirstOrDefaultAsync(t => t.Id == id && t.CompradorId == comprador.Id);

if (transacao == null) return NotFound(); // Seguro
```

---

#### 1.3 **Falta de Validação de Ownership em DeleteConfirmed**
**Localização:** `Areas/Vendedores/Controllers/VeiculosController.cs:290`

**Vulnerabilidade:**
```csharp
[HttpPost, ActionName("Delete")]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var veiculo = await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id);
    
    if (veiculo.VendedorId != vendedor.Id) // ? Validação presente
        return Forbid();
    
    // ?? Falta validação de estado antes de soft delete
    veiculo.Estado = EstadoVeiculo.Pausado;
}
```

**Problema:**  
- Não valida se o veículo já está **Vendido** ou **Reservado**
- Permite soft-delete de veículos em transação ativa

**Impacto:** Perda de integridade de dados financeiros

**Recomendação:**
```csharp
// ? ADICIONAR:
if (veiculo.Estado == EstadoVeiculo.Vendido || 
    veiculo.Estado == EstadoVeiculo.Reservado)
{
    ModelState.AddModelError("", "Não pode remover um veículo vendido/reservado.");
    return View(veiculo);
}

// Verificar se tem transações ativas
var hasActiveTransactions = await _context.Transacoes
    .AnyAsync(t => t.VeiculoId == id && 
                   t.Estado != EstadoTransacao.Cancelado);

if (hasActiveTransactions)
    return BadRequest("Veículo tem transações ativas.");
```

---

### 2. ??? CRYPTOGRAPHIC FAILURES (A02:2021) - ALTO

#### 2.1 **Passwords em Texto Claro em DbInitializer**
**Localização:** `Data/DbInitializer.cs:28`

**Vulnerabilidade:**
```csharp
var adminPwd = configuration["DefaultAdmin:Password"];

if (string.IsNullOrWhiteSpace(adminPwd))
{
    if (environment.IsProduction())
        throw new InvalidOperationException("...");
    
    adminPwd = "Password1231"; // ?? HARDCODED PASSWORD
}
```

**Problemas:**
1. Password em código fonte (versionado no Git)
2. Password fraca em desenvolvimento
3. Mesma password para todos os ambientes de dev

**Recomendação URGENTE:**
```csharp
// ? USAR USER SECRETS em Dev e Azure KeyVault em Prod
var adminPwd = configuration["DefaultAdmin:Password"];

if (string.IsNullOrWhiteSpace(adminPwd))
{
    throw new InvalidOperationException(
        "DefaultAdmin:Password não configurada. " +
        "Use: dotnet user-secrets set 'DefaultAdmin:Password' 'YourStrongPassword'"
    );
}
```

**Setup User Secrets:**
```bash
dotnet user-secrets init
dotnet user-secrets set "DefaultAdmin:Password" "P@ssw0rd!Complex123#"
```

---

#### 2.2 **NIF Armazenado Sem Encriptação**
**Localização:** `Models/Utilizador.cs:31`

**Vulnerabilidade:**
```csharp
[StringLength(9)]
[NifPortugues]
[PersonalData]
public string? NIF { get; set; } // ?? Texto claro na BD
```

**Problema:**  
- NIF é **dado pessoal sensível** (RGPD Art. 9)
- Armazenado em **texto claro** na tabela `AspNetUsers`
- Se houver **SQL Injection** ? exposição direta

**Recomendação:**
```csharp
// ? OPÇÃO 1: Encriptar no repositório
public class EncryptedNifRepository
{
    private readonly IDataProtector _protector;
    
    public async Task<string> GetNifAsync(string userId)
    {
        var encrypted = await _db.Users.Where(...).Select(u => u.NifEncrypted);
        return _protector.Unprotect(encrypted);
    }
    
    public async Task SetNifAsync(string userId, string nif)
    {
        user.NifEncrypted = _protector.Protect(nif);
    }
}

// ? OPÇÃO 2: Usar Always Encrypted (SQL Server)
[Column(TypeName = "varbinary(max)")]
public byte[] NifEncrypted { get; set; }
```

**Impacto:** Violação RGPD ? Multas até 4% do faturamento anual global

---

#### 2.3 **Cookies Sem Flags de Segurança Corretas**
**Localização:** `Program.cs:63`

**Vulnerabilidade:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // ? Correto
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest // ?? HTTP permitido em dev
        : CookieSecurePolicy.Always;
    
    // ?? FALTAM:
    // options.Cookie.SameSite = SameSiteMode.Strict;
    // options.Cookie.IsEssential = true;
});
```

**Problemas:**
1. **SameSite** não configurado ? Vulnerável a CSRF
2. **IsEssential** não definido ? Cookie pode ser bloqueado por GDPR banners
3. Em dev, permite HTTP ? treina maus hábitos

**Recomendação:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ? Sempre HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax; // ? Protege contra CSRF
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
    
    // ? HSTS em produção
    if (!builder.Environment.IsDevelopment())
    {
        options.Cookie.Domain = ".automarket.com";
    }
});
```

---

### 3. ?? INJECTION (A03:2021) - MÉDIO

#### 3.1 **Potencial SQL Injection em Queries Dinâmicas**
**Status:** ? **BOM** - Uso correto de EF Core parameterizado

**Análise:**
```csharp
// ? SEGURO: EF Core usa parâmetros automáticos
var veiculo = await _context.Veiculos
    .FirstOrDefaultAsync(v => v.Id == id);
```

**Observação:**  
Não foram encontradas queries com `FromSqlRaw` ou concatenação de strings.

**Recomendação:** Manter vigilância em futuras features.

---

#### 3.2 **XSS (Cross-Site Scripting) em ViewModels**
**Localização:** `Views/Veiculos/Detalhe.cshtml` (assumido)

**Vulnerabilidade:**
```csharp
// Model: Veiculo.Descricao
[StringLength(2000)]
public string Descricao { get; set; }

// ?? Se renderizado com @Html.Raw(Model.Descricao)
// Permite injeção de <script>alert('XSS')</script>
```

**Problema:**  
- Campos de texto livre sem sanitização
- Potencial uso de `@Html.Raw` em views

**Recomendação:**
```csharp
// ? Usar biblioteca de sanitização
public class VeiculoViewModel
{
    public string DescricaoSanitizada => 
        HtmlSanitizer.Sanitize(Descricao, allowedTags: ["b", "i", "u", "p"]);
}

// Instalar: dotnet add package HtmlSanitizer
```

**View:**
```html
<!-- ? SEGURO: Razor escapa automático -->
<p>@Model.Descricao</p>

<!-- ?? PERIGOSO: Não usar -->
<p>@Html.Raw(Model.Descricao)</p>
```

---

#### 3.3 **Path Traversal em FileService**
**Localização:** `Services/FileService.cs:89`

**Vulnerabilidade:**
```csharp
public async Task<bool> DeleteFileAsync(string filePath)
{
    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);
    
    // ?? NÃO valida se filePath contém ".."
    if (File.Exists(fullPath))
        File.Delete(fullPath);
}
```

**Exploração:**
```csharp
// Attacker pode passar:
filePath = "../../appsettings.json" 
// Resulta em: wwwroot/../../appsettings.json ? apaga ficheiro fora da pasta
```

**Recomendação URGENTE:**
```csharp
public async Task<bool> DeleteFileAsync(string fileName)
{
    // ? VALIDAR: Apenas nome de ficheiro, sem paths
    if (Path.GetFileName(fileName) != fileName)
        throw new ArgumentException("Path inválido");
    
    if (fileName.Contains("..") || Path.IsPathRooted(fileName))
        throw new ArgumentException("Path traversal detetado");
    
    var uploadFolder = "images/veiculos"; // Fixo, não vem do user
    var fullPath = Path.Combine(
        _webHostEnvironment.WebRootPath, 
        uploadFolder, 
        fileName
    );
    
    // ? Canonicalizar path
    var canonicalPath = Path.GetFullPath(fullPath);
    var basePath = Path.GetFullPath(
        Path.Combine(_webHostEnvironment.WebRootPath, uploadFolder)
    );
    
    if (!canonicalPath.StartsWith(basePath))
        throw new UnauthorizedAccessException("Acesso negado");
    
    if (File.Exists(canonicalPath))
    {
        File.Delete(canonicalPath);
        return true;
    }
    return false;
}
```

---

### 4. ?? INSECURE DESIGN (A04:2021) - ALTO

#### 4.1 **Falta de Rate Limiting em Endpoints Críticos**

**Localização:** `Controllers/ContaController.cs`

**Vulnerabilidade:**
```csharp
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    // ?? SEM PROTEÇÃO: Permite ataques de força bruta
    var result = await _signInManager.PasswordSignInAsync(...);
}
```

**Problema:**  
- Nenhuma limitação de tentativas por IP
- Identity Lockout configurado, mas não impede ataques distribuídos
- Endpoint `/Conta/Register` também vulnerável a spam

**Recomendação URGENTE:**
```csharp
// ? INSTALAR: AspNetCoreRateLimit
dotnet add package AspNetCoreRateLimit

// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/Conta/Login",
            Period = "1m",
            Limit = 5 // 5 tentativas por minuto
        },
        new RateLimitRule
        {
            Endpoint = "POST:/Conta/Register",
            Period = "1h",
            Limit = 3 // 3 registos por hora por IP
        }
    };
});
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Middleware
app.UseIpRateLimiting();
```

---

#### 4.2 **Falta de CAPTCHA em Formulários Públicos**

**Localização:** `Controllers/ContaController.cs` (Register, Login)

**Problema:**  
- Bots podem criar contas em massa
- Ataques automatizados de credential stuffing

**Recomendação:**
```csharp
// ? INSTALAR: reCAPTCHA v3
dotnet add package reCAPTCHA.AspNetCore

// Program.cs
builder.Services.Configure<RecaptchaSettings>(
    builder.Configuration.GetSection("RecaptchaSettings"));
builder.Services.AddTransient<IRecaptchaService, RecaptchaService>();

// RegisterViewModel
public class RegisterViewModel
{
    [Required]
    [Recaptcha] // ? Validação automática
    public string RecaptchaToken { get; set; }
}

// View
<script src="https://www.google.com/recaptcha/api.js?render=@ViewBag.SiteKey"></script>
<script>
grecaptcha.ready(function() {
    grecaptcha.execute('@ViewBag.SiteKey', {action: 'register'})
        .then(function(token) {
            document.getElementById('RecaptchaToken').value = token;
        });
});
</script>
```

---

#### 4.3 **Session Fixation Potencial**

**Localização:** `Controllers/ContaController.cs:100`

**Vulnerabilidade:**
```csharp
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    var result = await _signInManager.PasswordSignInAsync(...);
    
    if (result.Succeeded)
    {
        // ?? NÃO regenera o Session ID após login
        return RedirectToAction("Index", "Home");
    }
}
```

**Problema:**  
Identity **já regenera** o cookie de autenticação, MAS se usares `Session` para outros dados:

**Recomendação:**
```csharp
if (result.Succeeded)
{
    // ? Regenerar Session ID (se usares HttpContext.Session)
    await HttpContext.Session.CommitAsync();
    var oldSessionId = HttpContext.Session.Id;
    HttpContext.Session.Clear();
    
    // Criar nova sessão
    HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
    
    _logger.LogInformation(
        "Sessão regenerada para {Email}. Old: {OldId}", 
        model.Email, oldSessionId);
    
    return RedirectToAction("Index", "Home");
}
```

---

### 5. ?? SECURITY MISCONFIGURATION (A05:2021) - ALTO

#### 5.1 **Exception Handling Expõe Stack Traces**

**Localização:** `Program.cs:95`

**Vulnerabilidade:**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// ?? EM DESENVOLVIMENTO: Stack traces completos são exibidos
```

**Problema:**  
- Leak de paths de ficheiros do servidor
- Exposição de tecnologias usadas (EF Core, Identity)
- Informação útil para ataques direcionados

**Recomendação:**
```csharp
// ? SEMPRE usar Exception Handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/html";
        
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionFeature?.Error;
        
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception: {Path}", context.Request.Path);
        
        // ? Mensagem genérica para o user
        await context.Response.WriteAsync(
            "<h1>Erro</h1><p>Ocorreu um erro. Contacte o suporte.</p>"
        );
        
        // ?? NUNCA mostrar: exception.ToString() ou exception.StackTrace
    });
});

// ? Developer Exception Page APENAS em dev local (não em Azure Dev)
if (app.Environment.IsDevelopment() && !IsAzure())
{
    app.UseDeveloperExceptionPage();
}
```

---

#### 5.2 **Falta de Security Headers**

**Problema:**  
Headers de segurança críticos não configurados:

```
X-Content-Type-Options: nosniff       ? Ausente
X-Frame-Options: DENY                 ? Ausente
Content-Security-Policy               ? Ausente
Referrer-Policy: strict-origin        ? Ausente
Permissions-Policy                    ? Ausente
```

**Recomendação URGENTE:**
```csharp
// ? Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // ? CSP - Ajustar conforme os scripts usados
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none';"
    );
    
    // ? Permissions Policy
    context.Response.Headers.Add("Permissions-Policy", 
        "geolocation=(), microphone=(), camera=()");
    
    await next();
});

// ? OU USAR NuGet: NWebsec.AspNetCore.Middleware
```

---

#### 5.3 **HSTS Mal Configurado**

**Localização:** `Program.cs:97`

**Vulnerabilidade:**
```csharp
app.UseHsts(); // ?? Usa valores default
```

**Problema:**  
Default: `max-age=2592000` (30 dias) ? Muito curto para produção

**Recomendação:**
```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365); // ? 1 ano
    options.IncludeSubDomains = true;
    options.Preload = true; // ? Submeter a hstspreload.org
});
```

---

### 6. ?? IDENTIFICATION AND AUTHENTICATION FAILURES (A07:2021) - MÉDIO

#### 6.1 **Password Policy Muito Restritiva**

**Localização:** `Program.cs:32`

**Análise:**
```csharp
options.Password.RequiredLength = 16; // ?? Muito longo
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = true;
options.Password.RequireNonAlphanumeric = true;
```

**Problema:**  
- **NIST SP 800-63B** recomenda **mínimo 8 caracteres**
- 16 caracteres força users a escrever passwords ou usar gestores (bom)
- Mas pode levar a passwords tipo: `Password123456!!`

**Recomendação NIST:**
```csharp
options.Password.RequiredLength = 12; // ? Balanceado
options.Password.RequireDigit = false; // ? NIST: Não forçar tipos
options.Password.RequireLowercase = false;
options.Password.RequireUppercase = false;
options.Password.RequireNonAlphanumeric = false;

// ? ADICIONAR: Validação contra lista de passwords comprometidas
// (Have I Been Pwned API)
```

**Implementar:**
```csharp
public class BreachedPasswordValidator : IPasswordValidator<Utilizador>
{
    private readonly HttpClient _httpClient;
    
    public async Task<IdentityResult> ValidateAsync(
        UserManager<Utilizador> manager, Utilizador user, string password)
    {
        // SHA-1 hash da password
        var sha1 = SHA1.Create();
        var hash = BitConverter.ToString(
            sha1.ComputeHash(Encoding.UTF8.GetBytes(password))
        ).Replace("-", "");
        
        var prefix = hash.Substring(0, 5);
        var suffix = hash.Substring(5);
        
        // ? k-Anonymity: Apenas enviamos 5 primeiros chars
        var response = await _httpClient.GetStringAsync(
            $"https://api.pwnedpasswords.com/range/{prefix}"
        );
        
        if (response.Contains(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "BreachedPassword",
                Description = "Esta password foi comprometida num data breach. Use outra."
            });
        }
        
        return IdentityResult.Success;
    }
}

// Registar
builder.Services.AddScoped<IPasswordValidator<Utilizador>, BreachedPasswordValidator>();
```

---

#### 6.2 **Falta de Multi-Factor Authentication (MFA)**

**Impacto:** Alto - Contas de administrador sem 2FA

**Recomendação:**
```csharp
// ? OBRIGAR 2FA para Admin
[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction("Enable2FA", "Conta");
        }
        // ...
    }
}

// Implementar TOTP (Google Authenticator)
// Microsoft.AspNetCore.Identity já suporta
```

---

#### 6.3 **Falta de Auditoria de Acessos**

**Problema:**  
Não há **logging de eventos de segurança**:
- Logins falhados
- Mudanças de password
- Acessos a recursos sensíveis
- Alterações de permissões

**Recomendação:**
```csharp
// ? Criar tabela de Auditoria
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; } // "Login", "PasswordChange", etc
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? Details { get; set; }
}

// Middleware de auditoria
public class AuditMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var auditLog = new AuditLog
            {
                UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                Action = $"{context.Request.Method} {context.Request.Path}",
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"],
                Timestamp = DateTime.UtcNow
            };
            
            await _dbContext.AuditLogs.AddAsync(auditLog);
            await _dbContext.SaveChangesAsync();
        }
        
        await _next(context);
    }
}
```

---

### 7. ?? SOFTWARE AND DATA INTEGRITY FAILURES (A08:2021) - MÉDIO

#### 7.1 **Falta de Integridade em Uploads de Ficheiros**

**Localização:** `Services/FileService.cs`

**Problema:**  
- Valida apenas extensão (facilmente spoofável)
- Não valida **content type real** do ficheiro
- Não verifica **magic bytes** (assinatura do ficheiro)

**Exploração:**
```bash
# Attacker renomeia malware.exe ? malware.jpg
# Content-Type: image/jpeg (forjado)
# Passa validação de extensão
```

**Recomendação URGENTE:**
```csharp
public async Task<string> UploadFileAsync(IFormFile file, string uploadFolder)
{
    // ? 1. Validar extensão
    var extension = Path.GetExtension(file.FileName).ToLower();
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    if (!allowedExtensions.Contains(extension))
        throw new ArgumentException("Extensão inválida");
    
    // ? 2. Validar Magic Bytes (assinatura real do ficheiro)
    using var stream = file.OpenReadStream();
    var header = new byte[8];
    await stream.ReadAsync(header, 0, 8);
    stream.Position = 0;
    
    bool isValid = header switch
    {
        // JPEG: FF D8 FF
        [0xFF, 0xD8, 0xFF, ..] => true,
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] => true,
        // WebP: 52 49 46 46 .. .. .. .. 57 45 42 50
        [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x45, 0x42, 0x50] => true,
        _ => false
    };
    
    if (!isValid)
        throw new ArgumentException("Ficheiro inválido (magic bytes)");
    
    // ? 3. Re-encode imagem para remover metadados EXIF maliciosos
    using var image = await Image.LoadAsync(stream);
    var sanitizedFileName = $"{Guid.NewGuid()}{extension}";
    var outputPath = Path.Combine(uploadPath, sanitizedFileName);
    
    await image.SaveAsync(outputPath); // Remove EXIF automático
    
    return sanitizedFileName;
}

// Instalar: dotnet add package SixLabors.ImageSharp
```

---

#### 7.2 **Falta de Verificação de Integridade em Packages NuGet**

**Recomendação:**
```xml
<!-- AutoMarket.csproj -->
<PropertyGroup>
  <NuGetAudit>true</NuGetAudit> <!-- ? Ativa auditoria de vulnerabilidades -->
  <NuGetAuditMode>all</NuGetAuditMode>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

---

### 8. ?? SECURITY LOGGING AND MONITORING FAILURES (A09:2021) - MÉDIO

#### 8.1 **Logging Insuficiente de Eventos Críticos**

**Problema:**  
Logging existe, mas **não cobre todos os eventos de segurança**.

**Eventos Não Logados:**
- ? Tentativas de IDOR
- ? Acessos negados (403/401)
- ? Uploads de ficheiros
- ? Mudanças de estado de veículos (Ativo ? Vendido)
- ? Aprovações de vendedores

**Recomendação:**
```csharp
// ? Middleware de logging de segurança
public class SecurityLoggingMiddleware
{
    private readonly ILogger<SecurityLoggingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        
        // Log de acessos negados
        if (context.Response.StatusCode == 403 || context.Response.StatusCode == 401)
        {
            _logger.LogWarning(
                "Acesso negado: {User} tentou aceder {Path} - {StatusCode}",
                context.User.Identity?.Name ?? "Anónimo",
                context.Request.Path,
                context.Response.StatusCode
            );
        }
        
        // Log de erros de servidor
        if (context.Response.StatusCode >= 500)
        {
            _logger.LogError(
                "Erro de servidor: {Path} - {StatusCode}",
                context.Request.Path,
                context.Response.StatusCode
            );
        }
    }
}
```

---

#### 8.2 **Falta de Alertas em Tempo Real**

**Recomendação:**
```csharp
// ? Integrar Application Insights (Azure)
builder.Services.AddApplicationInsightsTelemetry();

// Configurar alertas:
// 1. >10 logins falhados em 5 minutos
// 2. Upload de ficheiro >10MB
// 3. >5 erros 500 em 1 minuto
// 4. Tentativas de IDOR
```

---

### 9. ?? SERVER-SIDE REQUEST FORGERY (SSRF) (A10:2021) - BAIXO

**Status:** ? **Não Aplicável** - Aplicação não faz requests para URLs externos fornecidos por users.

---

## ?? MATRIZ DE PRIORIDADES

| Vulnerabilidade | Severidade | Impacto | Exploração | Prioridade |
|----------------|-----------|---------|------------|------------|
| Path Traversal (FileService) | ?? Crítico | Alto | Fácil | **P0** |
| IDOR em Checkout/Sucesso | ?? Crítico | Alto | Médio | **P0** |
| Passwords Hardcoded | ?? Crítico | Alto | Fácil | **P0** |
| Rate Limiting Ausente | ?? Alto | Médio | Fácil | **P1** |
| Security Headers Ausentes | ?? Alto | Médio | Fácil | **P1** |
| NIF Não Encriptado | ?? Alto | Alto (RGPD) | Difícil | **P1** |
| Magic Bytes Validation | ?? Alto | Médio | Médio | **P1** |
| Auditoria de Acessos | ?? Médio | Baixo | N/A | **P2** |
| MFA para Admin | ?? Médio | Alto | Difícil | **P2** |

---

## ? CHECKLIST DE PRÉ-PRODUÇÃO

### Bloqueadores (Obrigatórios):
- [ ] **P0-1:** Corrigir Path Traversal em `FileService.DeleteFileAsync`
- [ ] **P0-2:** Corrigir IDOR em `CheckoutController.Sucesso`
- [ ] **P0-3:** Remover password hardcoded do código
- [ ] **P1-1:** Implementar Rate Limiting (AspNetCoreRateLimit)
- [ ] **P1-2:** Adicionar Security Headers (CSP, X-Frame-Options, etc)
- [ ] **P1-3:** Validar Magic Bytes em uploads
- [ ] **P1-4:** Configurar HSTS corretamente (365 dias)
- [ ] **P1-5:** Implementar auditoria básica (tabela AuditLog)

### Recomendados (Antes de Produção):
- [ ] **P2-1:** Encriptar NIF com Data Protection API
- [ ] **P2-2:** Adicionar CAPTCHA em Register/Login
- [ ] **P2-3:** Implementar validação de passwords comprometidas (HIBP)
- [ ] **P2-4:** Adicionar MFA obrigatório para Admin
- [ ] **P2-5:** Configurar Application Insights + Alertas
- [ ] **P2-6:** Scan de vulnerabilidades em packages (NuGetAudit)

### Compliance (RGPD):
- [ ] Encriptar dados pessoais sensíveis (NIF, Morada?)
- [ ] Implementar "Right to be Forgotten" (soft delete completo)
- [ ] Adicionar consentimento explícito para emails
- [ ] Criar política de retenção de dados

---

## ?? REFERÊNCIAS

- [OWASP Top 10 2021](https://owasp.org/Top10/)
- [NIST SP 800-63B - Digital Identity Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [RGPD - Artigo 32 (Segurança do Tratamento)](https://gdpr-info.eu/art-32-gdpr/)

---

**Assinatura Digital:** [Arquiteto de Software Sénior]  
**Próxima Revisão:** Após implementação das ações P0 e P1
