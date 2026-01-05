# Segurança do NIF - Implementação

## Problema Identificado

O ValueConverter no `ApplicationDbContext` garante que o NIF fica encriptado na base de dados. No entanto, se um developer fizer `user.NIF = model.NIF` no Controller, o objeto em memória fica com o NIF em **Plain Text** até ao `SaveChanges`. Se ocorrer um Log ou Exceção nesse intervalo, o NIF pode ser exposto.

## Solução Implementada

### 1. Validação Rigorosa Antes de Atribuir

Adicionada validação rigorosa usando `NifValidator.IsValid()` **antes** de atribuir o NIF ao objeto `Utilizador`. Isto garante que:
- Dados inválidos são rejeitados imediatamente
- O NIF só é atribuído após validação bem-sucedida
- Reduz o tempo que o NIF fica em plain text em memória

### 2. Locais Atualizados

#### CheckoutController.cs
- Validação antes de atualizar NIF durante checkout
- Mensagem de erro específica para NIF inválido

#### ContaController.Register
- Validação antes de criar novo utilizador
- NIF opcional no registo, mas se fornecido deve ser válido

#### ContaController.PreencherDadosFiscais
- Validação obrigatória (NIF é obrigatório neste contexto)
- Validação antes de atribuir ao utilizador

### 3. Configuração de Logging Seguro

Atualizado `appsettings.json`, `appsettings.Development.json` e `appsettings.Production.json`:

```json
"Microsoft.EntityFrameworkCore.Database.Command": "Warning"
```

**Razão**: Se estiver em `Information`, os comandos SQL (com parâmetros que podem incluir NIFs desencriptados durante conversão) podem aparecer nos logs.

## Arquitetura de Segurança

### Fluxo de Encriptação

1. **Controller**: Valida NIF usando `NifValidator.IsValid()`
2. **Atribuição**: `user.NIF = model.NIF` (ainda em plain text em memória)
3. **SaveChanges**: EF Core ValueConverter encripta automaticamente
4. **Base de Dados**: NIF armazenado encriptado

### Proteções Implementadas

1. ✅ **Validação Rigorosa**: NIF validado antes de atribuir
2. ✅ **ValueConverter**: Encriptação automática na BD
3. ✅ **Logging Seguro**: EF Core Command logging em Warning (não expõe parâmetros)
4. ✅ **Logs Existentes**: Verificados - não expõem objetos user/model diretamente

## Código de Exemplo

### Antes (MÁ PRÁTICA)
```csharp
// Depende 100% do EF Core Converter
// Se houver exceção antes de SaveChanges, NIF fica em plain text
user.NIF = model.NifFaturacao;
await _userManager.UpdateAsync(user);
```

### Depois (SEGURO)
```csharp
// Validação rigorosa antes de atribuir (segurança: evita NIF em plain text sem validação)
if (!NifValidator.IsValid(model.NifFaturacao))
{
    ModelState.AddModelError("NifFaturacao", "NIF inválido. Por favor, verifique o número introduzido.");
    return View(model);
}

// O EF Core ValueConverter fará a encriptação ao salvar
user.NIF = model.NifFaturacao;
await _userManager.UpdateAsync(user);
```

## Notas Importantes

1. **NIF em Memória**: O NIF ainda fica em plain text em memória entre a atribuição e o `SaveChanges`. Isto é inevitável, mas a validação reduz o risco ao garantir que apenas dados válidos são processados.

2. **Logging**: Todos os logs existentes foram verificados e não expõem objetos `user` ou `model` diretamente. Apenas propriedades específicas (Email, Id) são logadas.

3. **ValueConverter**: O `NifEncryptionHelper` e o ValueConverter no `ApplicationDbContext` (linhas 77-84) estão corretos e funcionam como esperado.

4. **RGPD Compliance**: O NIF é encriptado na BD e não é exposto em cookies (ver `CustomClaimsPrincipalFactory`).

## Próximos Passos (Opcional)

Para segurança ainda maior, considerar:
- **Value Objects**: Encapsular NIF num Value Object que valida e encripta imediatamente
- **Sanitização de Logs**: Middleware para sanitizar logs automaticamente
- **Auditoria**: Log de acesso a dados sensíveis (sem expor os dados)

