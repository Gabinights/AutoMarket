using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMarket.Services.Implementations;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Security;
using AutoMarket.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "pt-PT", "en-US" };
    options.SetDefaultCulture("pt-PT")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )));

builder.Services.AddIdentity<Utilizador, IdentityRole>(options =>
{
    options.Password.RequireDigit = false; // Simplificado para testes
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6; // Simplificado para testes
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // ❌ Desativado - sem confirmação de email
})
.AddEntityFrameworkStores<ApplicationDbContext>() 
.AddUserStore<CustomUserStore>()                
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, VendedorAprovadoHandler>();
builder.Services.AddSingleton<EmailFailureTracker>(sp =>
    new EmailFailureTracker(maxFailures: 5, failureWindow: TimeSpan.FromMinutes(5), circuitBreakerTimeout: TimeSpan.FromMinutes(1)));
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<EmailTemplateService>();
builder.Services.AddScoped<IEmailAuthService, EmailAuthService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<ICarrinhoService, CarrinhoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IVisitaService, VisitaService>();
builder.Services.AddScoped<IDenunciaService, DenunciaService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
builder.Services.AddScoped<IEstatisticasService, EstatisticasService>();
builder.Services.AddScoped<IGestaoUtilizadoresService, GestaoUtilizadoresService>();
builder.Services.AddScoped<IFavoritoService, FavoritoService>();
builder.Services.AddScoped<IMensagensService, MensagensService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<IVendedorService, VendedorService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();

builder.Services.AddHostedService<LimparReservasHostedService>();

builder.Services.AddSession(options => {                 
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Public/Conta/Login";
    options.AccessDeniedPath = "/Public/Home/Index";
    options.SlidingExpiration = true;
});

var app = builder.Build();

var encryptionService = app.Services.GetRequiredService<IEncryptionService>();
NifEncryptionHelper.Initialize(encryptionService);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
var localizationOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
app.UseAuthentication();
app.UseMiddleware<UserBlockingMiddleware>();
app.UseAuthorization();

app.UseStatusCodePages(context =>
{
    return Task.CompletedTask;
});
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Public" }
);

app.MapAreaControllerRoute(
    name: "vendedores",
    areaName: "Vendedores",
    pattern: "Vendedores/{controller=Carros}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();