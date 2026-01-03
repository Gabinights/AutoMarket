using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Services;
using AutoMarket.Security;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AutoMarket.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "pt-PT", "en-US" };
    options.SetDefaultCulture("pt-PT")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

// Adiciona o DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona o ASP.NET Core Identity
builder.Services.AddIdentity<Utilizador, IdentityRole>(options =>
{
    // --- Configuração de Password ---
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 16; // Regra do utilizador
    // --- Configuração de Lockout ---
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    // --- Configuração de Utilizador / SignIn ---
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, VendedorAprovadoHandler>();
builder.Services.AddSingleton<EmailFailureTracker>(sp =>
    new EmailFailureTracker(maxFailures: 5, failureWindow: TimeSpan.FromMinutes(5), circuitBreakerTimeout: TimeSpan.FromMinutes(1)));
// Adiciona o serviço de email
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<EmailTemplateService>();
builder.Services.AddScoped<IEmailAuthService, EmailAuthService>();

// Adiciona o serviço de ficheiros
builder.Services.AddScoped<IFileService, FileService>();

// --- Configuração de Cookies de Sessão ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Conta/Login";
    options.SlidingExpiration = true;
});

// Adiciona o serviço de renderização de views
builder.Services.AddScoped<ViewRenderService>();

// Registar políticas de autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VendedorAprovado", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            if (!user.Identity?.IsAuthenticated ?? false)
                return false;

            // Verifica se o utilizador tem email confirmado
            if (!user.HasClaim(c => c.Type == "email_verified" && c.Value == "true"))
            {
                // Fallback: Verificar se email foi confirmado no AspNetUsers
                return user.Claims.Any(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            }

            return true;
        }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use localization
var localizationOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    var user = context.HttpContext.User;

    // Se for erro 403 (Proibido) e for Vendedor
    if (response.StatusCode == 403 && user.IsInRole(Roles.Vendedor))
    {
        response.Redirect("/Conta/AguardandoAprovacao");
    }
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "vendedores",
    areaName: "Vendedores",
    pattern: "Vendedores/{controller=Carros}/{action=Index}/{id?}");

// ============================================================
// INICIALIZAR BASE DE DADOS (Roles, Admin, Categorias, Dados Teste)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.InitializeAsync(services);
}

app.Run();
