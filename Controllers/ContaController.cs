using AutoMarket.Models;
using AutoMarket.Models.ViewModels;
using AutoMarket.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AutoMarket.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ApplicationDbContext _context;

        public ContaController(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new Utilizador
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                Morada = model.Morada,
                Contactos = model.Contactos,
            };

            if (model.TipoConta == "Vendedor")
            {
                user.StatusAprovacao = "Pendente";
            }
            else // Comprador
            {
                user.StatusAprovacao = "Aprovado";
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (model.TipoConta == "Comprador")
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else // Vendedor
                {
                    TempData["MensagemStatus"] = "Conta de vendedor criada com sucesso! Aguarde aprovação para começar a vender.";
                    return RedirectToAction("Index", "Home");
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AguardarAprovacao()
        {
            return Content("Conta criada como vendedor. Aguarda aprovação do administrador.");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Login inválido.");
                    return View(model);
                }
                if (user.StatusAprovacao != "Aprovado")
                {
                    await _signInManager.SignOutAsync();
                    string msg = user.StatusAprovacao == "Pendente"
                        ? "Aguardando aprovação do administrador."
                        : "Conta de vendedor rejeitada.";
                    ModelState.AddModelError(string.Empty, msg);
                    return View(model);
                }
                return RedirectToAction("Index", "Home");
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Conta bloqueada devido a tentativas inválidas. Tente novamente mais tarde.");
                return View(model);
            }
            ModelState.AddModelError(string.Empty, "Login inválido.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
