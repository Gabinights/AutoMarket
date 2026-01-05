using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Utils;
using AutoMarket.Models.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Services.Implementations
{
	public class AuthService : IAuthService
	{
	private readonly UserManager<Utilizador> _userManager;
	private readonly SignInManager<Utilizador> _signInManager;
	private readonly ApplicationDbContext _context;
	private readonly IEmailAuthService _emailAuthService;
	private readonly ILogger<AuthService> _logger;
	private readonly IHostEnvironment _environment;

	public AuthService(
		UserManager<Utilizador> userManager,
		SignInManager<Utilizador> signInManager,
		ApplicationDbContext context,
		IEmailAuthService emailAuthService,
		ILogger<AuthService> logger,
		IHostEnvironment environment)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_context = context;
		_emailAuthService = emailAuthService;
		_logger = logger;
		_environment = environment;
	}

		public async Task<RegisterResultDto> RegisterAsync(RegisterDto dto, string confirmationBaseUrl)
		{
			var errors = new List<string>();

			if (!string.IsNullOrEmpty(dto.Nif) && !NifValidator.IsValid(dto.Nif))
			{
				errors.Add("NIF inválido. Por favor, verifique o número introduzido.");
				return new RegisterResultDto(false, errors, null);
			}

			await using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
			var user = new Utilizador
			{
				UserName = dto.Email,
				Email = dto.Email,
				Nome = dto.Nome,
				Morada = dto.Morada,
				PhoneNumber = dto.Contacto,
				NIF = dto.Nif,
				DataRegisto = DateTime.UtcNow
				// EmailConfirmed removido - não é necessário
			};

				var identityResult = await _userManager.CreateAsync(user, dto.Password);
				if (!identityResult.Succeeded)
				{
					errors.AddRange(identityResult.Errors.Select(e => e.Description));
					return new RegisterResultDto(false, errors, null);
				}

			if (dto.TipoConta == TipoConta.Vendedor || dto.TipoConta == TipoConta.Empresa)
			{
				var vendedor = new Vendedor
				{
					UserId = user.Id,
					TipoConta = dto.TipoConta,
					Status = _environment.IsDevelopment() ? StatusAprovacao.Aprovado : StatusAprovacao.Pendente,
					ApprovedByAdminId = _environment.IsDevelopment() ? user.Id : null // Auto-aprovar em dev
				};
				_context.Vendedores.Add(vendedor);
				await _userManager.AddToRoleAsync(user, Roles.Vendedor);
			}
				else
				{
					var comprador = new Comprador
					{
						UserId = user.Id,
						ReceberNotificacoes = false
					};
					_context.Compradores.Add(comprador);
					await _userManager.AddToRoleAsync(user, Roles.Comprador);
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				// Auto-login após registo
				await _signInManager.SignInAsync(user, isPersistent: false);

				_logger.LogInformation("Utilizador {Email} registado e auto-logado com sucesso", user.Email);

				return new RegisterResultDto(true, Enumerable.Empty<string>(), null);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro ao registar utilizador {Email}", dto.Email);
				errors.Add("Ocorreu um erro interno. Tente novamente.");
				return new RegisterResultDto(false, errors, null);
			}
		}

		public async Task<LoginResultDto> LoginAsync(LoginDto dto)
		{
			var errors = new List<string>();

			var signInResult = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, false);
			if (!signInResult.Succeeded)
			{
				errors.Add("Email ou password inválidos.");
				return new LoginResultDto(false, null, errors);
			}

			var user = await _userManager.FindByEmailAsync(dto.Email);
			if (user == null)
			{
				await _signInManager.SignOutAsync();
				errors.Add("Erro inesperado ao obter dados do utilizador.");
				return new LoginResultDto(false, null, errors);
			}

			if (user.IsBlocked || user.IsDeleted)
			{
				await _signInManager.SignOutAsync();
				errors.Add("Conta bloqueada ou eliminada.");
				return new LoginResultDto(false, null, errors);
			}

			if (await _userManager.IsInRoleAsync(user, Roles.Vendedor))
			{
				var vendedor = await _context.Vendedores
					.Select(v => new { v.UserId, v.Status })
					.FirstOrDefaultAsync(v => v.UserId == user.Id);

				if (vendedor != null && vendedor.Status != StatusAprovacao.Aprovado)
				{
					_logger.LogWarning("Vendedor {Email} tentou entrar mas está com status {Status}", user.Email, vendedor.Status);
					await _signInManager.SignOutAsync();
					return new LoginResultDto(false, nameof(StatusAprovacao.Pendente), errors);
				}
			}

			return new LoginResultDto(true, null, Enumerable.Empty<string>());
		}

		public Task LogoutAsync()
		{
			return _signInManager.SignOutAsync();
		}

		public async Task<ConfirmEmailResultDto> ConfirmEmailAsync(string userId, string token)
		{
			var errors = new List<string>();

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
			{
				errors.Add("Link de confirmação inválido ou incompleto.");
				return new ConfirmEmailResultDto(false, errors);
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				errors.Add("Utilizador não encontrado no sistema.");
				return new ConfirmEmailResultDto(false, errors);
			}

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (!result.Succeeded)
			{
				errors.AddRange(result.Errors.Select(e => e.Description));
				_logger.LogWarning("Falha na confirmação de email para {UserId}: {Errors}", userId, string.Join(", ", errors));
				return new ConfirmEmailResultDto(false, errors);
			}

			return new ConfirmEmailResultDto(true, Enumerable.Empty<string>());
		}

		public async Task<UpdateNifResultDto> UpdateNifAsync(string userId, string nif)
		{
			var errors = new List<string>();

			if (string.IsNullOrEmpty(nif))
			{
				errors.Add("O NIF é obrigatório.");
				return new UpdateNifResultDto(false, errors);
			}

			if (!NifValidator.IsValid(nif))
			{
				errors.Add("NIF inválido. Por favor, verifique o número introduzido.");
				return new UpdateNifResultDto(false, errors);
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				errors.Add("Utilizador não encontrado.");
				return new UpdateNifResultDto(false, errors);
			}

			user.NIF = nif;
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				errors.AddRange(result.Errors.Select(e => e.Description));
				return new UpdateNifResultDto(false, errors);
			}

			return new UpdateNifResultDto(true, Enumerable.Empty<string>());
		}

		public async Task<SoftDeleteResultDto> SoftDeleteAsync(string userId)
		{
			var errors = new List<string>();
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				errors.Add("Utilizador não encontrado.");
				return new SoftDeleteResultDto(false, errors);
			}

			try
			{
				await _signInManager.SignOutAsync();

				user.IsDeleted = true;

				var deletedEmailSuffix = $"deleted_{Guid.NewGuid():N}@automarket.invalid";
				user.Email = deletedEmailSuffix;
				user.UserName = deletedEmailSuffix;
				user.NormalizedEmail = deletedEmailSuffix.ToUpperInvariant();
				user.NormalizedUserName = deletedEmailSuffix.ToUpperInvariant();
				user.Nome = "Utilizador Eliminado";
				user.NIF = null;
				user.Morada = string.Empty;
				user.Contacto = string.Empty;

				var result = await _userManager.UpdateAsync(user);
				if (!result.Succeeded)
				{
					errors.AddRange(result.Errors.Select(e => e.Description));
					foreach (var error in result.Errors)
					{
						_logger.LogError("Erro ao atualizar utilizador {Id} após soft delete: {Error}", user.Id, error.Description);
					}
					return new SoftDeleteResultDto(false, errors);
				}

				_logger.LogInformation("Utilizador {Id} apagou a conta (Soft Delete). Sessão invalidada.", user.Id);
				return new SoftDeleteResultDto(true, Enumerable.Empty<string>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Erro crítico ao apagar conta do utilizador {Id}", user.Id);
				errors.Add("Erro crítico ao apagar conta.");
				return new SoftDeleteResultDto(false, errors);
			}
		}
	}
}
