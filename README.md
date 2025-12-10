📘 AutoMarket - Guia de Desenvolvimento & Arquitetura
[!URGENT] LEIAM ISTO ANTES DE ESCREVER CÓDIGO Este documento define como trabalhamos. Seguir estas regras evita que partam a base de dados ou criem conflitos de merge impossíveis de resolver.

1. Stack Tecnológica
Framework: ASP.NET Core 8.0 (MVC)

Base de Dados: SQL Server + Entity Framework Core (Code First)

Autenticação: ASP.NET Core Identity (Com extensão de perfis)

Front-end: Razor Views, Bootstrap 5, jQuery (AJAX).

2. A Nossa Estrutura (Onde fica o quê?)
Não inventem pastas novas. Sigam este mapa:

Plaintext

AutoMarket/
├── Controllers/              # O "Cérebro". Recebe pedidos e decide o que fazer.
│   ├── AdminController.cs    # Aprovar Vendedores, gerir bloqueios.
│   ├── CarrosController.cs   # Criar carros (Vendedor) e Listar (Público).
│   └── ContaController.cs    # Lógica de Login/Registo (NÃO MEXER sem falar com o Lead).
├── Data/
│   ├── ApplicationDbContext.cs # Onde as tabelas são definidas.
│   └── DbInitializer.cs      # Cria o Admin e Categorias se a BD estiver vazia.
├── Models/                   # A "Verdade". Classes que viram tabelas na BD.
│   ├── ViewModels/           # "Papéis de Rascunho". Classes só para formulários (ex: RegisterViewModel).
├── Services/                 # Lógica pesada (Emails, PDFs, Cálculos complexos).
├── Views/                    # O HTML (Interface).
└── wwwroot/                  # Imagens, CSS e JS estáticos.
3. Workflow de Desenvolvimento (Como não partir tudo)
A. Trabalhar com Base de Dados (Migrations)
Sempre que alterarem um ficheiro na pasta Models/:

Parem a aplicação.

Abram a Package Manager Console.

Criar a "fotografia" da mudança: Add-Migration NomeDescritivoDaMudanca (Ex: AddCampoCorToCarro).

Aplicar à BD: Update-Database.

Nunca apaguem a pasta Migrations manualmente a não ser que a base de dados seja resetada.

B. Git (Controlo de Versões)
Nunca trabalhem diretamente na main ou master.

Começar tarefa: git checkout -b feat/nome-da-funcionalidade (Ex: feat/upload-imagens).

Durante o trabalho: Façam commits pequenos.

Acabar: Abram um Pull Request (PR) no GitHub.

Regra de Ouro: Antes de fazerem o PR, façam git pull origin main na vossa branch para garantir que não há conflitos.

4. Regras de Implementação (Ler Obrigatório)
🔐 Autenticação (Quem és tu?)
Não usamos a classe IdentityUser diretamente para guardar dados de negócio.

Se precisarem de dados de Venda (NIF, Stand), usem a tabela Vendedores.

Se precisarem de dados de Compra (Favoritos), usem a tabela Compradores.

Exemplo: Para saber o NIF do utilizador logado, não está no User. Têm de ir à tabela Vendedores procurar pelo UserId.

🛡️ Autorização (O que podes fazer?)
Não façam if (User.Identity.Name == "admin"). Isso é proibido. Usem atributos em cima dos Controllers ou Actions:

[Authorize(Roles = "Admin")] -> Só para chefes.

[Authorize(Policy = "VendedorAprovado")] -> Só para vendedores que já foram aceites.

⚡ Performance (Não matem o servidor)
Quando fizerem pesquisas na Base de Dados para listagens (ex: Catálogo de Carros):

ERRADO: _context.Carros.ToList().Where(c => c.Preco > 1000)

Porquê? Traz 1 milhão de carros para a memória RAM e só depois filtra.

CERTO: _context.Carros.Where(c => c.Preco > 1000).ToList()

Porquê? O filtro é feito no SQL Server. Só vêm os carros certos.

🖼️ Uploads de Imagens
Base de Dados: Guarda apenas o nome (ferrari_123.jpg).

Pasta wwwroot/images: Guarda o ficheiro real.

Nunca tentem guardar o ficheiro binário dentro do SQL Server.

5. Dúvidas Comuns (FAQ)
"Onde ponho a lógica de enviar Email?" -> Pasta Services. Não ponham no Controller.

"Criei um campo novo no Model mas dá erro." -> Esqueceste-te de fazer Add-Migration e Update-Database.

"O Login não funciona." -> Verifica se tens o DbInitializer corrido e se o user existe na tabela AspNetUsers.

