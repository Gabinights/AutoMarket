using AutoMarket.Models;
using AutoMarket.Utils;

namespace AutoMarket.Services
{
    public class CarrinhoService : ICarrinhoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "CarrinhoCompras";
        private const string DefaultImagePlaceholder = "sem-imagem.jpg";
        // Precisamos do IHttpContextAccessor para aceder à Session fora de um Controller
        public CarrinhoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                {
                    throw new InvalidOperationException("HttpContext is null. Ensure this service is called within an HTTP request context.");
                }
                if (context.Session == null)
                {
                    throw new InvalidOperationException("Session is not available. Ensure that session middleware is configured.");
                }
                return context.Session;
            }
        }

        public List<CarrinhoItem> GetItens()
        {
            return Session.GetObjectFromJson<List<CarrinhoItem>>(SessionKey) ?? new List<CarrinhoItem>();
        }

        public void AdicionarItem(Veiculo veiculo)
        {
            if (veiculo == null) { throw new ArgumentNullException(nameof(veiculo)); }

            var itens = GetItens();

            // Como é um veículo único, verificamos se já lá está
            if (!itens.Any(i => i.VeiculoId == veiculo.Id))
            {
                itens.Add(new CarrinhoItem
                {
                    VeiculoId = veiculo.Id,
                    Titulo = veiculo.Titulo,
                    Marca = veiculo.Marca,
                    Modelo = veiculo.Modelo,
                    Preco = veiculo.Preco,
                    // Pega a primeira imagem ou uma placeholder
                    ImagemCapa = veiculo.Imagens?.FirstOrDefault()?.CaminhoFicheiro ?? DefaultImagePlaceholder
                });

                Session.SetObjectAsJson(SessionKey, itens);
            }
        }

        public void RemoverItem(int veiculoId)
        {
            var itens = GetItens();
            var item = itens.FirstOrDefault(i => i.VeiculoId == veiculoId);

            if (item != null)
            {
                itens.Remove(item);
                Session.SetObjectAsJson(SessionKey, itens);
            }
        }

        public void LimparCarrinho()
        {
            Session.Remove(SessionKey);
        }

        public decimal GetTotal()
        {
            return GetItens().Sum(i => i.Preco);
        }

        public int GetContagem()
        {
            return GetItens().Count;
        }
    }
}
