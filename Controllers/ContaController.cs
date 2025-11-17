using AutoMarket.Models;
using AutoMarket.Models.ViewModels;
using AutoMarket.Models.Enums;
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

        public ContaController(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            // Validar explicitamente o TipoConta
            if (model.TipoConta != "Comprador" && model.TipoConta != "Vendedor")
            {
                ModelState.AddModelError(string.Empty, "Tipo de conta inválido. Deve ser 'Comprador' ou 'Vendedor'.");
                return View(model);
            }

            var user = new Utilizador
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                Morada = model.Morada,
                Contactos = model.Contactos,
                EmailConfirmed = false
            };

            if (model.TipoConta == "Vendedor")
            {
                user.StatusAprovacao = StatusAprovacaoEnum.Pendente;
            }
            else // Comprador
            {
                user.StatusAprovacao = StatusAprovacaoEnum.Aprovado;
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Gerar token de confirmação de email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmarEmail", "Conta", 
                    new { userId = user.Id, token = token }, 
                    Request.Scheme);

                // TODO: Enviar email com o link de confirmação
                // await _emailSender.SendEmailAsync(user.Email, "Confirme o seu email", 
                //     $"Por favor, confirme o seu email clicando neste link: {confirmationLink}");

                TempData["MensagemStatus"] = "Conta criada com sucesso! Por favor, verifique o seu email para confirmar a conta.";
                return RedirectToAction("Index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        /// <summary>
        /// Action que exibe uma mensagem informando que a conta de vendedor aguarda aprovação do administrador.
        /// </summary>
        [HttpGet]
        public IActionResult AguardarAprovacao()
        {
            return Content("Conta criada como vendedor. Aguarda aprovação do administrador.");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                TempData["MensagemStatus"] = "Link de confirmação inválido.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["MensagemStatus"] = "Utilizador não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["MensagemStatus"] = "Email confirmado com sucesso! Pode agora fazer login.";
                return RedirectToAction("Login", "Conta");
            }
            else
            {
                TempData["MensagemStatus"] = "Erro ao confirmar o email. O link pode ter expirado.";
                return RedirectToAction("Index", "Home");
            }
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

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Verificar se o email está confirmado
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Login inválido.");
                    return View(model);
                }

                // Verificar status de aprovação
                if (user.StatusAprovacao != StatusAprovacaoEnum.Aprovado)
                {
                    ModelState.AddModelError(string.Empty, "Login inválido.");
                    return View(model);
                }
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
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
