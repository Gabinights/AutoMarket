using AutoMarket.Models.DTOs;

namespace AutoMarket.Services.Interfaces
{
	public interface IAuthService
	{
		Task<RegisterResultDto> RegisterAsync(RegisterDto dto, string confirmationBaseUrl);
		Task<LoginResultDto> LoginAsync(LoginDto dto);
		Task LogoutAsync();
		Task<ConfirmEmailResultDto> ConfirmEmailAsync(string userId, string token);
		Task<UpdateNifResultDto> UpdateNifAsync(string userId, string nif);
		Task<SoftDeleteResultDto> SoftDeleteAsync(string userId);
	}
}