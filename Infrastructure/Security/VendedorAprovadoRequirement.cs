using Microsoft.AspNetCore.Authorization;

namespace AutoMarket.Infrastructure.Security
{
    // Esta classe é um "marcador" que diz que existe esta regra
    public class VendedorAprovadoRequirement : IAuthorizationRequirement
    {
    }
}