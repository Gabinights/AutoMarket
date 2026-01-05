# ??? AUDITORIA DE ARQUITETURA E SEPARATION OF CONCERNS - AutoMarket
**Data:** 2024-01-15  
**Auditor:** Arquiteto de Software Sénior  
**Padrões Avaliados:** SOLID, Clean Architecture, DDD Lite  

---

## ?? SUMÁRIO EXECUTIVO

### Estado Geral: ?? YELLOW (Refactoring Recomendado)
- **Violações SOLID:** 18  
- **Acoplamento Forte:** 9 pontos críticos  
- **Responsabilidades Misturadas:** 23 ocorrências  
- **Ausência de Abstração:** 6 componentes  

### Problemas Arquiteturais Principais:
1. ?? **Fat Controllers** - Lógica de negócio nos controllers
2. ?? **DbContext usado diretamente** - Sem Repository Pattern
3. ?? **Ausência de camada de Application Services**
4. ?? **ViewModels com lógica de conversão** (Anemic Domain Model)
5. ?? **Falta de Unit of Work** - Transações espalhadas
6. ?? **Serviços com responsabilidades múltiplas**

---

## ?? ANÁLISE POR PRINCÍPIO SOLID

### 1. ?? SINGLE RESPONSIBILITY PRINCIPLE (SRP) - VIOLADO

#### 1.1 **ContaController - 8 Responsabilidades**

**Localização:** `Controllers/ContaController.cs`

**Violações:**
```csharp
public class ContaController : Controller
{
    // ? 1. Autenticação
    public async Task<IActionResult> Login(LoginViewModel model)
    
    // ? 2. Registo de utilizadores
    public async Task<IActionResult> Register(RegisterViewModel model)
    
    // ? 3. Gestão de perfis
    public async Task<IActionResult> Perfil(EditarPerfilViewModel model)
    
    // ? 4. Gestão de passwords
    public async Task<IActionResult> AlterarPassword(AlterarPasswordViewModel model)
    
    // ? 5. Soft Delete
    public async Task<IActionResult> ApagarConta()
    
    // ? 6. Confirmação de email
    public async Task<IActionResult> ConfirmarEmail(string userId, string token)
    
    // ? 7. Dados fiscais
    public async Task<IActionResult> PreencherDadosFiscais(DadosFiscaisViewModel model)
    
    // ? 8. Lógica de negócio (verificação de vendedor aprovado)
}
```

**Problema:**  
Controller com **450+ linhas** e **múltiplas razões para mudar**.

**Impacto:**
- Difícil de testar
- Difícil de manter
- Violação clara de SRP

**Refactoring Proposto:**
```
ContaController ? Dividir em:
??? AuthenticationController (Login, Logout)
??? RegistrationController (Register, ConfirmarEmail)
??? ProfileController (Perfil, AlterarPassword, ApagarConta)
??? FiscalDataController (PreencherDadosFiscais)

+ Criar Application Services:
??? IAuthenticationService
??? IUserRegistrationService
??? IUserProfileService
??? IFiscalDataService
```

---

#### 1.2 **VeiculosController (Área Vendedores) - Lógica Misturada**

**Localização:** `Areas/Vendedores/Controllers/VeiculosController.cs`

**Violações:**
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateVeiculoViewModel viewModel)
{
    // ? 1. Validação de negócio
    if (!ModelState.IsValid) { ... }
    
    // ? 2. Upload de ficheiros
    var uploadedFiles = await _fileService.UploadMultipleFilesAsync(...);
    
    // ? 3. Conversão de ViewModel para Entity
    var veiculo = viewModel.ToVeiculo(vendedor.Id);
    
    // ? 4. Criação de imagens
    foreach (var fileName in uploadedFiles) {
        imagens.Add(new VeiculoImagem { ... });
    }
    
    // ? 5. Persistência direta
    _context.Veiculos.Add(veiculo);
    await _context.SaveChangesAsync();
    
    // ? 6. Logging
    _logger.LogInformation(...);
}
```

**Problema:**  
Controller faz:
- Validação
- Conversão de dados
- Lógica de upload
- Persistência
- Logging

**Refactoring Proposto:**
```csharp
// ? Controller fino
[HttpPost]
public async Task<IActionResult> Create(CreateVeiculoViewModel viewModel)
{
    if (!ModelState.IsValid)
        return View(viewModel);
    
    var vendedorId = await GetVendedorIdAsync();
    var command = new CreateVeiculoCommand
    {
        VendedorId = vendedorId,
        Titulo = viewModel.Titulo,
        // ... mapear campos
        Fotos = viewModel.Fotos
    };
    
    var result = await _veiculoService.CreateAsync(command);
    
    if (!result.IsSuccess)
    {
        ModelState.AddModelError("", result.ErrorMessage);
        return View(viewModel);
    }
    
    TempData["Sucesso"] = "Veículo criado com sucesso!";
    return RedirectToAction(nameof(Index));
}

// ? Service com responsabilidade única
public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repository;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<int>> CreateAsync(CreateVeiculoCommand command)
    {
        // 1. Upload de imagens
        var imageUrls = await _fileService.UploadMultipleAsync(
            command.Fotos, "images/veiculos"
        );
        
        // 2. Criar entidade
        var veiculo = Veiculo.Create(
            command.VendedorId,
            command.Titulo,
            command.Marca,
            // ...
            imageUrls
        );
        
        // 3. Validação de domínio
        if (!veiculo.IsValid(out var errors))
            return Result<int>.Failure(errors);
        
        // 4. Persistir
        await _repository.AddAsync(veiculo);
        await _unitOfWork.CommitAsync();
        
        return Result<int>.Success(veiculo.Id);
    }
}
```

---

#### 1.3 **CheckoutController - Transações Financeiras Misturadas**

**Localização:** `Controllers/CheckoutController.cs:50`

**Violação:**
```csharp
[HttpPost]
public async Task<IActionResult> ProcessarCompra(CheckoutViewModel model)
{
    // ? Responsabilidades misturadas:
    // 1. Validação de carrinho
    var itensCarrinho = _carrinhoService.GetItens();
    
    // 2. Gestão de utilizador
    var comprador = await _context.Compradores.FirstOrDefaultAsync(...);
    if (comprador == null) {
        comprador = new Comprador { ... };
        _context.Add(comprador);
    }
    
    // 3. Atualização de NIF
    if (model.QueroFaturaComNif && ...) {
        user.NIF = model.NifFaturacao;
        await _userManager.UpdateAsync(user);
    }
    
    // 4. Lógica de transação financeira
    foreach (var item in itensCarrinho) {
        var transacao = new Transacao { ... };
        _context.Transacoes.Add(transacao);
        veiculoDb.Estado = EstadoVeiculo.Reservado;
    }
    
    // 5. Gestão de transações de BD
    using var dbTransaction = await _context.Database.BeginTransactionAsync();
    await dbTransaction.CommitAsync();
}
```

**Problemas:**
1. **93 linhas** num único método
2. Lógica de **3 domínios** diferentes (Carrinho, Utilizador, Transação)
3. Transação de BD **manual** (deve ser automática)
4. **Erro de design:** NIF atualizado DENTRO de uma transação de compra

**Refactoring Proposto:**
```csharp
// ? CQRS Pattern
public class CheckoutController : Controller
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    public async Task<IActionResult> ProcessarCompra(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);
        
        var command = new ProcessPurchaseCommand
        {
            UserId = User.GetUserId(),
            NifFaturacao = model.NifFaturacao,
            Morada = model.Morada,
            CodigoPostal = model.CodigoPostal,
            MetodoPagamento = model.MetodoPagamento
        };
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.ErrorMessage);
            return View("Index", model);
        }
        
        return RedirectToAction("Sucesso", new { id = result.TransactionId });
    }
}

// ? Command Handler com Saga Pattern
public class ProcessPurchaseCommandHandler 
    : IRequestHandler<ProcessPurchaseCommand, Result<int>>
{
    private readonly ICarrinhoService _carrinhoService;
    private readonly ITransacaoService _transacaoService;
    private readonly ICompradorService _compradorService;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<int>> Handle(
        ProcessPurchaseCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Validar carrinho
        var carrinho = await _carrinhoService.GetCarrinhoAsync(request.UserId);
        if (carrinho.IsEmpty)
            return Result<int>.Failure("Carrinho vazio");
        
        // 2. Obter/criar comprador
        var comprador = await _compradorService
            .GetOrCreateAsync(request.UserId);
        
        // 3. Processar transação (dentro de UnitOfWork automático)
        var transacaoId = await _transacaoService.ProcessarCompraAsync(
            comprador.Id,
            carrinho,
            new DadosEnvio
            {
                NIF = request.NifFaturacao,
                Morada = request.Morada,
                CodigoPostal = request.CodigoPostal
            },
            request.MetodoPagamento
        );
        
        // 4. Commit automático
        await _unitOfWork.CommitAsync();
        
        // 5. Limpar carrinho (fora da transação principal)
        await _carrinhoService.LimparAsync(request.UserId);
        
        return Result<int>.Success(transacaoId);
    }
}
```

---

### 2. ?? OPEN/CLOSED PRINCIPLE (OCP) - VIOLADO

#### 2.1 **FileService - Validação Hardcoded**

**Localização:** `Services/FileService.cs:23`

**Violação:**
```csharp
public async Task<string> UploadFileAsync(IFormFile file, string uploadFolder)
{
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    
    if (!allowedExtensions.Contains(extension))
        throw new ArgumentException(...);
    
    // ? Se quisermos permitir PDF ou GIF, temos de MODIFICAR este método
}
```

**Problema:**  
Classe **fechada para extensão**, aberta para **modificação**.

**Refactoring Proposto:**
```csharp
// ? Strategy Pattern
public interface IFileValidator
{
    bool IsValid(IFormFile file);
    string GetErrorMessage();
}

public class ImageFileValidator : IFileValidator
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    
    public bool IsValid(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        return _allowedExtensions.Contains(extension);
    }
}

public class DocumentFileValidator : IFileValidator
{
    private readonly string[] _allowedExtensions = { ".pdf", ".docx" };
    // ...
}

public class FileService : IFileService
{
    private readonly IFileValidator _validator;
    
    public FileService(IFileValidator validator)
    {
        _validator = validator; // ? Injeção de dependência
    }
    
    public async Task<string> UploadFileAsync(IFormFile file, string uploadFolder)
    {
        if (!_validator.IsValid(file))
            throw new ArgumentException(_validator.GetErrorMessage());
        
        // ... resto do upload
    }
}

// ? Registar no DI
builder.Services.AddScoped<IFileValidator, ImageFileValidator>(); // Para imagens
// builder.Services.AddScoped<IFileValidator, DocumentFileValidator>(); // Para docs
```

---

#### 2.2 **Estado de Veículo - Enum Limitado**

**Localização:** `Models/Enums/EstadoVeiculo.cs`

**Problema:**
```csharp
public enum EstadoVeiculo
{
    Ativo = 0,
    Reservado = 1,
    Vendido = 2,
    Pausado = 3
    // ? Se precisarmos de "EmManutenção" ou "Leilão", temos de modificar
}
```

**Refactoring Proposto:**
```csharp
// ? State Pattern (para lógica complexa)
public abstract class VeiculoState
{
    public abstract bool CanTransitionTo(VeiculoState newState);
    public abstract bool CanEdit { get; }
    public abstract bool CanDelete { get; }
    public abstract bool IsVisibleToPublic { get; }
}

public class AtivoState : VeiculoState
{
    public override bool CanTransitionTo(VeiculoState newState) =>
        newState is ReservadoState or PausadoState;
    
    public override bool CanEdit => true;
    public override bool CanDelete => true;
    public override bool IsVisibleToPublic => true;
}

public class ReservadoState : VeiculoState
{
    public override bool CanTransitionTo(VeiculoState newState) =>
        newState is VendidoState or AtivoState;
    
    public override bool CanEdit => false;
    public override bool CanDelete => false;
    public override bool IsVisibleToPublic => false;
}

// ? Entity com state
public class Veiculo
{
    public VeiculoState Estado { get; private set; } = new AtivoState();
    
    public Result MudarEstado(VeiculoState novoEstado)
    {
        if (!Estado.CanTransitionTo(novoEstado))
            return Result.Failure($"Não pode mudar de {Estado} para {novoEstado}");
        
        Estado = novoEstado;
        return Result.Success();
    }
}
```

---

### 3. ?? LISKOV SUBSTITUTION PRINCIPLE (LSP) - PARCIALMENTE VIOLADO

#### 3.1 **Hierarquia Utilizador ? Vendedor/Comprador**

**Localização:** `Models/Utilizador.cs`, `Models/Vendedor.cs`, `Models/Comprador.cs`

**Análise:**
```csharp
// ? BOM: Não há herança direta
public class Utilizador : IdentityUser { ... }

public class Vendedor
{
    public string UserId { get; set; } // ? Composição, não herança
    public Utilizador User { get; set; }
}

public class Comprador
{
    public string UserId { get; set; }
    public Utilizador User { get; set; }
}
```

**Avaliação:** ? **CORRETO** - Usa **composição** em vez de herança múltipla.

**Observação:**  
Se um utilizador pode ser **Vendedor E Comprador** ao mesmo tempo, esta estrutura permite isso.

---

### 4. ?? INTERFACE SEGREGATION PRINCIPLE (ISP) - VIOLADO

#### 4.1 **IEmailSender - Interface Fat**

**Localização:** `Services/Interfaces/IEmailSender.cs`

**Problema Potencial:**
```csharp
public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string message, 
                       CancellationToken cancellationToken = default);
    
    // ?? Se adicionarmos métodos como:
    // Task SendBulkEmailAsync(...);
    // Task SendWithAttachmentsAsync(...);
    // Task SendTemplatedEmailAsync(...);
    
    // Classes que só precisam de envio simples são forçadas a implementar tudo
}
```

**Refactoring Proposto:**
```csharp
// ? Segregar interfaces
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string message);
}

public interface IBulkEmailSender
{
    Task SendBulkAsync(IEnumerable<string> recipients, string subject, string message);
}

public interface IAttachmentEmailSender
{
    Task SendWithAttachmentsAsync(string to, string subject, string message, 
                                  IEnumerable<EmailAttachment> attachments);
}

public interface ITemplatedEmailSender
{
    Task SendTemplatedAsync<T>(string to, string templateName, T data);
}

// ? Implementação pode suportar múltiplas interfaces
public class EmailSender : IEmailSender, ITemplatedEmailSender
{
    // ... implementação
}
```

---

### 5. ?? DEPENDENCY INVERSION PRINCIPLE (DIP) - VIOLADO

#### 5.1 **Controllers Dependem de Implementações Concretas**

**Localização:** `Areas/Vendedores/Controllers/VeiculosController.cs:24`

**Violação:**
```csharp
public class VeiculosController : Controller
{
    private readonly ApplicationDbContext _context; // ? Dependência CONCRETA
    private readonly IFileService _fileService;    // ? Abstração (correto)
    
    // ? Controller conhece detalhes de EF Core
    public async Task<IActionResult> Index()
    {
        var veiculos = await _context.Veiculos
            .Where(v => v.VendedorId == vendedor.Id)
            .OrderByDescending(v => v.DataCriacao)
            .Include(v => v.Imagens) // ? EF Core específico
            .ToListAsync();
    }
}
```

**Problemas:**
1. **Acoplamento forte** com EF Core
2. Impossível trocar ORM sem quebrar controllers
3. Difícil de **mockar** em testes unitários
4. Viola **Hexagonal Architecture** (Ports & Adapters)

**Refactoring Proposto:**
```csharp
// ? Interface de domínio (Port)
public interface IVeiculoRepository
{
    Task<IEnumerable<Veiculo>> GetByVendedorAsync(int vendedorId);
    Task<Veiculo?> GetByIdAsync(int id);
    Task<Veiculo?> GetByIdForVendedorAsync(int veiculoId, int vendedorId);
    Task AddAsync(Veiculo veiculo);
    Task UpdateAsync(Veiculo veiculo);
    Task DeleteAsync(int id);
}

// ? Implementação EF Core (Adapter)
public class EFVeiculoRepository : IVeiculoRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IEnumerable<Veiculo>> GetByVendedorAsync(int vendedorId)
    {
        return await _context.Veiculos
            .Where(v => v.VendedorId == vendedorId && v.Estado != EstadoVeiculo.Pausado)
            .OrderByDescending(v => v.DataCriacao)
            .Include(v => v.Imagens)
            .Include(v => v.Categoria)
            .ToListAsync();
    }
    
    public async Task<Veiculo?> GetByIdForVendedorAsync(int veiculoId, int vendedorId)
    {
        return await _context.Veiculos
            .Include(v => v.Imagens)
            .FirstOrDefaultAsync(v => v.Id == veiculoId && v.VendedorId == vendedorId);
    }
}

// ? Controller depende de ABSTRAÇÃO
public class VeiculosController : Controller
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IFileService _fileService;
    
    public async Task<IActionResult> Index()
    {
        var vendedorId = await GetVendedorIdAsync();
        var veiculos = await _veiculoRepository.GetByVendedorAsync(vendedorId);
        
        var viewModel = veiculos.Select(v => ListVeiculoViewModel.FromVeiculo(v));
        return View(viewModel);
    }
    
    public async Task<IActionResult> Edit(int id)
    {
        var vendedorId = await GetVendedorIdAsync();
        var veiculo = await _veiculoRepository.GetByIdForVendedorAsync(id, vendedorId);
        
        if (veiculo == null)
            return NotFound(); // ? Seguro contra IDOR
        
        var viewModel = EditVeiculoViewModel.FromVeiculo(veiculo);
        return View(viewModel);
    }
}
```

---

## ??? PADRÕES ARQUITETURAIS RECOMENDADOS

### 1. **Repository Pattern** ? CRÍTICO

**Benefícios:**
- Abstração da persistência
- Testabilidade (mocking fácil)
- Troca de ORM sem quebrar código
- Queries centralizadas

**Estrutura Proposta:**
```
Infrastructure/
??? Repositories/
?   ??? EFVeiculoRepository.cs
?   ??? EFTransacaoRepository.cs
?   ??? EFCompradorRepository.cs
?   ??? EFVendedorRepository.cs
Domain/
??? Repositories/ (Interfaces)
?   ??? IVeiculoRepository.cs
?   ??? ITransacaoRepository.cs
?   ??? ICompradorRepository.cs
?   ??? IVendedorRepository.cs
```

**Implementação Base:**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class EFRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public EFRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);
    
    // ... implementações base
}

// ? Especialização
public class EFVeiculoRepository : EFRepository<Veiculo>, IVeiculoRepository
{
    public async Task<IEnumerable<Veiculo>> GetByVendedorAsync(int vendedorId) =>
        await _dbSet
            .Where(v => v.VendedorId == vendedorId)
            .Include(v => v.Imagens)
            .ToListAsync();
}
```

---

### 2. **Unit of Work Pattern** ? CRÍTICO

**Problema Atual:**
```csharp
// ? Transações manuais espalhadas
using var transaction = await _context.Database.BeginTransactionAsync();
try {
    _context.Veiculos.Add(veiculo);
    await _context.SaveChangesAsync();
    _context.Transacoes.Add(transacao);
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
}
```

**Solução:**
```csharp
public interface IUnitOfWork : IDisposable
{
    IVeiculoRepository Veiculos { get; }
    ITransacaoRepository Transacoes { get; }
    ICompradorRepository Compradores { get; }
    IVendedorRepository Vendedores { get; }
    
    Task<int> CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Veiculos = new EFVeiculoRepository(context);
        Transacoes = new EFTransacaoRepository(context);
        Compradores = new EFCompradorRepository(context);
        Vendedores = new EFVendedorRepository(context);
    }
    
    public IVeiculoRepository Veiculos { get; }
    public ITransacaoRepository Transacoes { get; }
    public ICompradorRepository Compradores { get; }
    public IVendedorRepository Vendedores { get; }
    
    public async Task<int> CommitAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task RollbackAsync()
    {
        if (_transaction != null)
            await _transaction.RollbackAsync();
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

// ? Uso no Service
public class TransacaoService : ITransacaoService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<int> ProcessarCompraAsync(...)
    {
        var comprador = await _unitOfWork.Compradores.GetByUserIdAsync(userId);
        var veiculo = await _unitOfWork.Veiculos.GetByIdAsync(veiculoId);
        
        var transacao = new Transacao { ... };
        await _unitOfWork.Transacoes.AddAsync(transacao);
        
        veiculo.Estado = EstadoVeiculo.Reservado;
        await _unitOfWork.Veiculos.UpdateAsync(veiculo);
        
        await _unitOfWork.CommitAsync(); // ? Transação automática
        
        return transacao.Id;
    }
}
```

---

### 3. **Service Layer (Application Services)** ? ALTO

**Estrutura Proposta:**
```
Application/
??? Services/
?   ??? VeiculoService.cs
?   ??? TransacaoService.cs
?   ??? UserRegistrationService.cs
?   ??? AuthenticationService.cs
??? Commands/
?   ??? CreateVeiculoCommand.cs
?   ??? ProcessPurchaseCommand.cs
?   ??? RegisterUserCommand.cs
??? Queries/
    ??? GetVeiculosQuery.cs
    ??? GetTransacoesQuery.cs
```

**Exemplo:**
```csharp
public interface IVeiculoService
{
    Task<Result<int>> CreateAsync(CreateVeiculoCommand command);
    Task<Result> UpdateAsync(UpdateVeiculoCommand command);
    Task<Result> DeleteAsync(int veiculoId, int vendedorId);
    Task<IEnumerable<VeiculoDto>> GetByVendedorAsync(int vendedorId);
}

public class VeiculoService : IVeiculoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ILogger<VeiculoService> _logger;
    
    public async Task<Result<int>> CreateAsync(CreateVeiculoCommand command)
    {
        // 1. Validação de negócio
        if (string.IsNullOrWhiteSpace(command.Titulo))
            return Result<int>.Failure("Título obrigatório");
        
        // 2. Upload de imagens
        var imageUrls = await _fileService.UploadMultipleAsync(
            command.Fotos, "images/veiculos"
        );
        
        // 3. Criar entidade
        var veiculo = new Veiculo
        {
            VendedorId = command.VendedorId,
            Titulo = command.Titulo,
            Marca = command.Marca,
            Modelo = command.Modelo,
            Preco = command.Preco,
            // ...
            Imagens = imageUrls.Select(url => new VeiculoImagem
            {
                CaminhoFicheiro = url,
                IsCapa = imageUrls.First() == url
            }).ToList()
        };
        
        // 4. Persistir
        await _unitOfWork.Veiculos.AddAsync(veiculo);
        await _unitOfWork.CommitAsync();
        
        _logger.LogInformation(
            "Veículo {VeiculoId} criado por vendedor {VendedorId}",
            veiculo.Id, command.VendedorId
        );
        
        return Result<int>.Success(veiculo.Id);
    }
}
```

---

### 4. **CQRS (Command Query Responsibility Segregation)** ?? RECOMENDADO

**Problema Atual:**
Controllers misturam **comandos** (Write) e **queries** (Read).

**Solução:**
```csharp
// ? INSTALAR: MediatR
dotnet add package MediatR

// Commands (Write)
public class CreateVeiculoCommand : IRequest<Result<int>>
{
    public int VendedorId { get; set; }
    public string Titulo { get; set; }
    public decimal Preco { get; set; }
    public List<IFormFile> Fotos { get; set; }
}

public class CreateVeiculoCommandHandler 
    : IRequestHandler<CreateVeiculoCommand, Result<int>>
{
    private readonly IVeiculoService _veiculoService;
    
    public async Task<Result<int>> Handle(
        CreateVeiculoCommand request, 
        CancellationToken cancellationToken)
    {
        return await _veiculoService.CreateAsync(request);
    }
}

// Queries (Read)
public class GetVeiculosByVendedorQuery : IRequest<IEnumerable<VeiculoDto>>
{
    public int VendedorId { get; set; }
}

public class GetVeiculosByVendedorQueryHandler 
    : IRequestHandler<GetVeiculosByVendedorQuery, IEnumerable<VeiculoDto>>
{
    private readonly IVeiculoRepository _repository;
    
    public async Task<IEnumerable<VeiculoDto>> Handle(
        GetVeiculosByVendedorQuery request, 
        CancellationToken cancellationToken)
    {
        var veiculos = await _repository.GetByVendedorAsync(request.VendedorId);
        return veiculos.Select(v => VeiculoDto.FromEntity(v));
    }
}

// ? Controller fino
[HttpPost]
public async Task<IActionResult> Create(CreateVeiculoViewModel viewModel)
{
    var command = new CreateVeiculoCommand
    {
        VendedorId = await GetVendedorIdAsync(),
        Titulo = viewModel.Titulo,
        // ...
    };
    
    var result = await _mediator.Send(command);
    
    return result.IsSuccess 
        ? RedirectToAction(nameof(Index))
        : View(viewModel);
}

[HttpGet]
public async Task<IActionResult> Index()
{
    var query = new GetVeiculosByVendedorQuery
    {
        VendedorId = await GetVendedorIdAsync()
    };
    
    var veiculos = await _mediator.Send(query);
    return View(veiculos);
}
```

---

### 5. **Specification Pattern** ?? RECOMENDADO

**Problema:**
Queries complexas espalhadas por repositórios.

**Solução:**
```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
}

public class VeiculosAtivosSpec : ISpecification<Veiculo>
{
    public Expression<Func<Veiculo, bool>> Criteria =>
        v => v.Estado == EstadoVeiculo.Ativo;
    
    public List<Expression<Func<Veiculo, object>>> Includes => new()
    {
        v => v.Imagens,
        v => v.Categoria
    };
    
    public Expression<Func<Veiculo, object>> OrderByDescending =>
        v => v.DataCriacao;
}

public class VeiculosPorVendedorSpec : ISpecification<Veiculo>
{
    private readonly int _vendedorId;
    
    public VeiculosPorVendedorSpec(int vendedorId)
    {
        _vendedorId = vendedorId;
    }
    
    public Expression<Func<Veiculo, bool>> Criteria =>
        v => v.VendedorId == _vendedorId && v.Estado != EstadoVeiculo.Pausado;
    
    // ...
}

// ? Repository com Specification
public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAsync(ISpecification<T> spec);
}

public class EFRepository<T> : IRepository<T>
{
    public async Task<IEnumerable<T>> GetAsync(ISpecification<T> spec)
    {
        var query = _dbSet.Where(spec.Criteria);
        
        query = spec.Includes.Aggregate(query, (current, include) => 
            current.Include(include));
        
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        
        if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);
        
        return await query.ToListAsync();
    }
}

// ? Uso
var spec = new VeiculosAtivosSpec()
    .And(new VeiculosPorVendedorSpec(vendedorId));

var veiculos = await _repository.GetAsync(spec);
```

---

## ?? ANÁLISE DE ACOPLAMENTO

### Diagrama de Dependências Atual:
```
????????????????
? Controllers  ? ???
????????????????   ?
                   ? Dependem diretamente de:
                   ?
????????????????????????????????
? ApplicationDbContext (EF)    ? ? ALTO ACOPLAMENTO
????????????????????????????????
                   ?
                   ?
????????????????????????????????
? SQL Server                   ?
????????????????????????????????
```

### Diagrama Proposto (Hexagonal Architecture):
```
??????????????????????????????
?       Presentation         ?
?    (Controllers, Views)    ?
??????????????????????????????
           ?
           ? (Depende de abstrações)
??????????????????????????????
?     Application Layer      ?
?  (Services, Commands/Queries) ?
??????????????????????????????
           ?
           ? (Depende de interfaces)
??????????????????????????????
?      Domain Layer          ?
? (Entities, Repositories*)  ?
?  *Apenas Interfaces        ?
??????????????????????????????
           ?
           ? (Implementa interfaces)
??????????????????????????????
?   Infrastructure Layer     ?
? (EF Repositories, DbContext)?
??????????????????????????????
           ?
           ?
??????????????????????????????
?      Database (SQL)        ?
??????????????????????????????
```

---

## ?? MÉTRICAS DE QUALIDADE

### Cyclomatic Complexity (McCabe):
```
Método                                    | Linhas | Complexity | Target
------------------------------------------|--------|------------|--------
ContaController.Register                  |   82   |    12      |  < 10  ?
CheckoutController.ProcessarCompra        |   93   |    15      |  < 10  ?
VeiculosController.Create                 |   74   |    8       |  < 10  ?
VeiculosController.Edit (POST)            |   89   |    10      |  < 10  ??
```

### Lines of Code (LOC):
```
Classe                    | LOC  | Target | Status
--------------------------|------|--------|--------
ContaController           | 456  | < 300  | ?
CheckoutController        | 187  | < 300  | ?
VeiculosController        | 412  | < 300  | ?
AdminController           | 89   | < 300  | ?
```

### Coupling Metrics:
```
Componente                | Acoplamento Eferente | Ideal | Status
--------------------------|---------------------|-------|--------
Controllers ? DbContext   |         6           |   0   | ?
Controllers ? Services    |         3           | 3-5   | ?
Services ? Repositories   |         0           | 2-4   | ? (não existem)
```

---

## ? ROADMAP DE REFACTORING

### Fase 1: Fundações (2-3 semanas)
- [ ] Criar interfaces de repositório (`IVeiculoRepository`, etc)
- [ ] Implementar `EFRepository<T>` base
- [ ] Implementar repositórios especializados
- [ ] Criar `IUnitOfWork` e implementação
- [ ] Registar no DI container
- [ ] **Escrever testes unitários** para repositórios

### Fase 2: Service Layer (2 semanas)
- [ ] Criar `Application/Services/` folder
- [ ] Implementar `VeiculoService`
- [ ] Implementar `TransacaoService`
- [ ] Implementar `UserRegistrationService`
- [ ] Implementar `AuthenticationService`
- [ ] **Escrever testes unitários** para services

### Fase 3: Refactor Controllers (1-2 semanas)
- [ ] Refatorar `VeiculosController` para usar `IVeiculoService`
- [ ] Refatorar `CheckoutController` para usar `ITransacaoService`
- [ ] Refatorar `ContaController` para usar services
- [ ] Remover dependências diretas de `ApplicationDbContext`
- [ ] **Escrever testes de integração** para controllers

### Fase 4: CQRS (Opcional, 1 semana)
- [ ] Instalar MediatR
- [ ] Criar Commands/Queries
- [ ] Criar Handlers
- [ ] Refatorar controllers para usar `IMediator`

### Fase 5: Cleanup & Otimização (1 semana)
- [ ] Remover código duplicado
- [ ] Aplicar Specification Pattern para queries complexas
- [ ] Review de métricas de qualidade
- [ ] Documentação de arquitetura

---

## ?? REFERÊNCIAS

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Unit of Work - Martin Fowler](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [CQRS - Greg Young](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)

---

**Assinatura Digital:** [Arquiteto de Software Sénior]  
**Próxima Revisão:** Após implementação da Fase 1
