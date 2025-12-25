using AutoMarket.Controllers;
using AutoMarket.Data;
using AutoMarket.Models;
using AutoMarket.Models.ViewModels;
using AutoMarket.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // Criar o ViewModel
            var model = new CheckoutViewModel
            {
                // Pré-preenchemos com os dados do perfil para facilitar a vida ao user
                NomeCompleto = user.Nome, // Assumindo que tens esta prop

                // Se o user já tiver NIF no perfil, ativamos a opção de fatura e preenchemos
                NifFaturacao = user.NIF,
                QueroFaturaComNif = !string.IsNullOrEmpty(user.NIF)
            };

            return View(model);
        }

        /*

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarCompra(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);

            // 1. Criar a Encomenda (Snapshot dos dados atuais)
            var encomenda = new Encomenda
            {
                UserId = user.Id,
                Data = DateTime.UtcNow,
                Estado = EstadoEncomenda.Pendente,
                Total = _carrinhoService.GetTotal(), // Exemplo

                // DADOS FISCAIS DA TRANSAÇÃO (Ficam aqui para sempre)
                NifFaturacao = model.QueroFaturaComNif ? model.NifFaturacao : null,
                NomeFaturacao = model.QueroFaturaComNif ? model.NomeFaturacao : model.NomeCompleto,
                MoradaEntrega = model.Morada + ", " + model.CodigoPostal
            };

            // 2. (Opcional) Atualizar o Perfil do User "Silenciosamente"
            // Se o user forneceu um NIF válido agora e não tinha nenhum no perfil, guardamos para a próxima.
            if (model.QueroFaturaComNif && string.IsNullOrEmpty(user.NIF) && !string.IsNullOrEmpty(model.NifFaturacao))
            {
                user.NIF = model.NifFaturacao;
                await _userManager.UpdateAsync(user);
            }

            _context.Encomendas.Add(encomenda);
            await _context.SaveChangesAsync();

            // Limpar carrinho, enviar email, etc.
            return RedirectToAction("Sucesso");
        }
                */

        /*
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (QueroFaturaComNif)
            {
                if (string.IsNullOrWhiteSpace(NifFaturacao))
                    yield return new ValidationResult("Insira o NIF", new[] { nameof(NifFaturacao) });

                // Apenas valida se o número é real, não importa se é 1, 2, 5 ou 9.
                else if (!NifValidator.IsValid(NifFaturacao))
                    yield return new ValidationResult("NIF Inválido", new[] { nameof(NifFaturacao) });
            }
        }
        */
    }
}