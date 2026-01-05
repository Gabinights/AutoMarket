# ?? RELATÓRIO DE AUDITORIA - FINDINGIDENTIFICADOS

## ?? FICHEIROS DESNECESSÁRIOS ENCONTRADOS

### 1. **Ficheiros Auto-Gerados (Razor Cache)**
- ? `Areas\Public\Views\**\*.cshtml.g5QftJKJM1NMQfXY.ide.g.cs` (71 ficheiros)
- ? `obj\Debug\net8.0\*.cs` (4 ficheiros)
- **Causa:** Cache do Razor compiler
- **Ação:** Limpar pasta `obj/bin`

### 2. **Entidades/Models Obsoletos ou Duplicados**
- ? `Models\Entities\Carro.cs` - **DUPLICADO de Veiculo.cs**
- ? `Models\Entities\CarroImagem.cs` - **DUPLICADO de VeiculoImagem.cs**
- ? `Models\ViewModels\CarroViewModel.cs` - **Obsoleto**
- ? `Models\Enums\EstadoCarro.cs` - **Obsoleto**
- ?? `Models\Entities\CarrinhoItem.cs` - Referenciado apenas em Carrinho (pode não usar)

### 3. **ViewModels Não Utilizados**
- ? `Models\ViewModels\EmailConfirmationViewModel.cs` - Não é usado em nenhuma view
- ? `Models\ViewModels\CarrinhoWidgetViewModel.cs` - Referenciado mas não importado
- ?? `Models\ViewModels\CheckoutViewModel.cs` - Existe mas usa-se `Transacao` diretamente

### 4. **DTOs Duplicados**
- ?? `Models\DTOs\VeiculoDto.cs` - Duplicado: utiliza-se `Veiculo` entidade diretamente
- ? `Models\DTOs\CheckoutDto.cs` - Não é utilizado em nenhum lugar

### 5. **Controllers Desnecessários ou Duplicados**
- ?? `Areas\Public\Controllers\CarrinhoController.cs` - Aparenta estar duplicado com `CheckoutController`
- ?? `Areas\Public\Controllers\TransacoesController.cs` - Potencialmente duplicado com `CheckoutController`
- ?? `Areas\Vendedores\Controllers\VeiculosController.cs` - Tem Views com nomes de CRUD automático (Create, Edit, Delete, Details)

### 6. **Views Duplicadas/Obsoletas**
- ? `Areas\Public\Views\Transacao\Checkout.cshtml` - Duplicado: `Areas\Public\Views\Checkout\Index.cshtml`
- ? `Areas\Admin\Views\HistoricoTransacoes.cshtml` - Obsoleto, nunca utilizado
- ? `Areas\Admin\Views\Index.cshtml` - Conflita com `Dashboard.cshtml`
- ?? `Areas\Public\Views\Carrinho\CartIndex.cshtml` - Função duplicada com Checkout

### 7. **Services Potencialmente Redundantes**
- ?? `CheckoutService` vs `TransacoesController` (duplicam lógica de checkout)
- ?? `CarrinhoService` - Parece não ser utilizado (carrinho usa session)
- ?? `ViewRenderService` - Não é utilizado em nenhum lugar (render de emails não é usado)

### 8. **Ficheiros Comentados/Obsoletos no Código**

#### Em `Program.cs`:
- ? Código comentado de auto-login (linhas ~80-100)
- ? Comentários antigos sobre middleware removido

#### Em `AuthService.cs`:
- ?? Múltiplos comentários em português quebrados (encoding issue)

#### Em `ContaController.cs`:
- ? Comentários sobre funcionalidades não implementadas

### 9. **Enums Redundantes**
- ? `Models\Enums\EstadoCarro.cs` - Duplicado de `EstadoVeiculo`
- ?? `Models\Enums\MetodoPagamento.cs` - Pode ser string enum no CheckoutViewModel

### 10. **Utility Classes Não Utilizados**
- ?? `Infrastructure\Utils\SessionExtensions.cs` - Métodos não são utilizados
- ?? `Infrastructure\Utils\NifValidator.cs` - Validation feita no Service

---

## ?? ANÁLISE DE DRY (Don't Repeat Yourself)

### ? Código Duplicado Encontrado:

#### 1. **Validação de NIF**
- `NifValidator.cs` + `Models\Attributes\NifPortuguesAttribute.cs` + `DadosFiscaisViewModel.cs`
- **Refatorizar:** Usar apenas o Attribute

#### 2. **Lógica de Filtros de Veículos**
- `VeiculosController.BuildVeiculosQuery()` + `VeiculoSearchFiltersDto`
- **OK:** Já consolidado

#### 3. **Gestão de Imagens**
- `FileService` + `VeiculoImagem` + Views múltiplas
- **Potencial:** Consolidar em uma classe única

#### 4. **Auditoria**
- `AuditoriaLog` + `AuditoriaService` + métodos em múltiplos controllers
- **Refatorizar:** Criar middleware automático para auditoria

#### 5. **Email Sending**
- `EmailSender` + `EmailAuthService` + `EmailTemplateService` + `EmailFailureTracker`
- **Problema:** 4 ficheiros para uma funcionalidade simples
- **Refatorizar:** Consolidar em um único serviço

---

## ??? PLANO DE LIMPEZA

### **Ficheiros a ELIMINAR IMEDIATAMENTE:**

1. ?? `Models\Entities\Carro.cs` - Duplicado de Veiculo
2. ?? `Models\Entities\CarroImagem.cs` - Duplicado de VeiculoImagem
3. ?? `Models\Enums\EstadoCarro.cs` - Duplicado
4. ?? `Models\ViewModels\CarroViewModel.cs` - Obsoleto
5. ?? `Models\ViewModels\EmailConfirmationViewModel.cs` - Não usado
6. ?? `Models\DTOs\CheckoutDto.cs` - Não usado
7. ?? `Areas\Public\Views\Transacao\Checkout.cshtml` - Duplicado
8. ?? `Areas\Admin\Views\HistoricoTransacoes.cshtml` - Obsoleto
9. ?? `Areas\Admin\Views\Index.cshtml` - Conflita com Dashboard

### **Ficheiros a LIMPAR (Remover Comentários):**

1. ?? `Program.cs` - Remover código comentado de auto-login
2. ?? `AuthService.cs` - Corrigir comentários em português
3. ?? `ContaController.cs` - Remover comentários obsoletos

### **Refatorizar (Consolidar):**

1. ?? `EmailService` - Consolidar 4 ficheiros em 1
2. ?? `AuditoriaMiddleware` - Criar middleware automático
3. ?? `CarrinhoController` + `TransacoesController` + `CheckoutController` - Unificar

### **Pasta Completa a Limpar:**

1. ??? `obj/Debug/**/*.g.cs` - Cache do Razor (recriar automaticamente)

---

## ?? AVISOS IMPORTANTES

### Ficheiros Marcados Como ?? (Revisar):

- **CarrinhoService** - Verificar se é realmente utilizado
- **SessionExtensions** - Verificar métodos não utilizados
- **VeiculoService Interface** - Verificar implementação
- **IEmailQueue Interface** - Não tem implementação (orphan interface)

---

## ?? IMPACTO DA LIMPEZA

### Antes:
- Controllers: 16
- Services: 12+
- Views: 54+
- Entities: 18
- ViewModels: 7
- DTOs: 4
- **Total: ~120 ficheiros**

### Depois:
- Controllers: 14 (-2)
- Services: 10 (-2)
- Views: 50 (-4)
- Entities: 16 (-2)
- ViewModels: 5 (-2)
- DTOs: 2 (-2)
- **Total: ~110 ficheiros (-10)**

### Linhas de Código Removidas: ~500-700 linhas

---

## ? PRÓXIMOS PASSOS

1. ?? Eliminar 9 ficheiros principais
2. ?? Limpar comentários obsoletos
3. ?? Refatorizar EmailService
4. ?? Testar build
5. ?? Testar app completa
6. ?? Gerar relatório final
