using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Options;
using AutoMarket.Infrastructure.Security;
using AutoMarket.Models.Constants;
using AutoMarket.Services;
using AutoMarket.Services.Implementations;
using AutoMarket.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

// Adiciona o serviço de encriptação (RGPD compliance)
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// Adiciona o DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona o ASP.NET Core Identity com UserStore customizado (soft delete)
builder.Services.AddIdentity<Utilizador, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 16;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
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
builder.Services.AddScoped<ViewRenderService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

builder.Services.AddScoped<ICarrinhoService, CarrinhoService>();

// Opções
builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("Reservation"));



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
app.UseAuthorization();
app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;
    var user = context.HttpContext.User;

    if (response.StatusCode == 403 && user.IsInRole(Roles.Vendedor))
    {
        response.Redirect("/Conta/AguardandoAprovacao");
    }
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

app.Run();