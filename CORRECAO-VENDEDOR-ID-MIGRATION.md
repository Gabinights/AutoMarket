# ?? CORREÇÃO: Erro "Invalid column name 'VendedorId'"

## ? PROBLEMA IDENTIFICADO

O erro ocorre porque:
1. ? O modelo `Transacao.cs` tem o campo `VendedorId`
2. ? A base de dados **não tem** a coluna `VendedorId` na tabela `Transacoes`
3. ?? A migration foi criada mas **não foi aplicada** à base de dados

---

## ? SOLUÇÃO

### **Passo 1: Aplicar a Migration à Base de Dados**

Abra um **terminal** no diretório do projeto e execute:

```powershell
dotnet ef database update
```

**OU** no **Package Manager Console** do Visual Studio:

```powershell
Update-Database
```

---

### **Passo 2: Verificar se a Migration Foi Aplicada**

Após executar o comando acima, deve ver uma mensagem como:

```
Applying migration '20260103230428_AddVendedorIdToTransacao'.
Done.
```

---

### **Passo 3: Testar a Aplicação**

1. Execute a aplicação:
   ```powershell
   dotnet run
   ```

2. Faça login

3. Aceda a:
   ```
   https://localhost:XXXX/Transacoes/MinhasCompras
   ```

4. Verifique se o erro desapareceu

---

## ?? O QUE A MIGRATION FAZ

A migration `AddVendedorIdToTransacao` adiciona:

### **1. Nova Coluna `VendedorId`**
```sql
ALTER TABLE Transacoes ADD VendedorId INT NULL
```

### **2. Preenche Valores Existentes**
```sql
UPDATE t
SET t.VendedorId = v.VendedorId
FROM Transacoes t
INNER JOIN Veiculos v ON t.VeiculoId = v.Id
WHERE t.VendedorId IS NULL
```
- Para transações **já existentes**, copia o `VendedorId` do veículo associado

### **3. Torna Coluna Obrigatória**
```sql
ALTER TABLE Transacoes ALTER COLUMN VendedorId INT NOT NULL
```

### **4. Adiciona Índice**
```sql
CREATE INDEX IX_Transacoes_VendedorId ON Transacoes(VendedorId)
```

### **5. Adiciona Foreign Key**
```sql
ALTER TABLE Transacoes 
ADD CONSTRAINT FK_Transacoes_Vendedores_VendedorId 
FOREIGN KEY (VendedorId) REFERENCES Vendedores(Id)
ON DELETE RESTRICT
```

---

## ?? VERIFICAÇÃO MANUAL (Opcional)

Se quiser verificar se a coluna foi adicionada, pode executar no SQL Server:

```sql
-- Verificar estrutura da tabela
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Transacoes'
ORDER BY ORDINAL_POSITION
```

Deve ver a coluna `VendedorId` com:
- `DATA_TYPE`: int
- `IS_NULLABLE`: NO

---

## ?? TROUBLESHOOTING

### **Erro: "A network-related or instance-specific error occurred"**

**Causa:** Servidor SQL não está a correr

**Solução:**
1. Abrir SQL Server Management Studio (SSMS)
2. Iniciar serviço SQL Server
3. Tentar novamente

---

### **Erro: "Database 'AutoMarket' does not exist"**

**Causa:** Base de dados não foi criada

**Solução:**
```powershell
# Criar base de dados com todas as migrations
dotnet ef database update
```

---

### **Erro: "Cannot insert the value NULL into column 'VendedorId'"**

**Causa:** Transações antigas sem VendedorId

**Solução:** A migration corrigida já trata disto automaticamente:
1. Adiciona coluna como nullable
2. Preenche valores existentes
3. Torna coluna NOT NULL

---

### **Erro: "The 'AddVendedorIdToTransacao' migration has already been applied"**

**Mensagem:** Isto significa que a migration **já foi aplicada** com sucesso!

**Verificação:**
```powershell
# Ver migrations aplicadas
dotnet ef migrations list
```

Se `AddVendedorIdToTransacao` aparecer com `(Applied)`, está tudo OK.

---

## ?? PASSOS RÁPIDOS (RESUMO)

```powershell
# 1. Aplicar migration
dotnet ef database update

# 2. Executar aplicação
dotnet run

# 3. Testar MinhasCompras
# Aceder: https://localhost:XXXX/Transacoes/MinhasCompras
```

---

## ?? MIGRATIONS DO PROJETO

Após aplicar, deve ter estas migrations na BD:

```
? InitialCreate
? MoveNifToUser
? userFixes
? AddVeiculoModel
? InitialVeiculoSchema
? AddVendedorIdToTransacao  ? ESTA É A NOVA
```

Verificar com:
```powershell
dotnet ef migrations list
```

---

## ?? RESULTADO ESPERADO

Após aplicar a migration:

1. ? Coluna `VendedorId` adicionada à tabela `Transacoes`
2. ? Transações existentes preenchidas com `VendedorId` correto
3. ? Foreign key criada (Transacao ? Vendedor)
4. ? Índice criado para performance
5. ? Erro "Invalid column name 'VendedorId'" **resolvido**
6. ? Páginas MinhasCompras e MinhasVendas funcionam

---

## ?? SE AINDA HOUVER PROBLEMAS

### **Opção 1: Reset da Base de Dados** (?? APAGA TODOS OS DADOS)

```powershell
# Apagar base de dados
dotnet ef database drop --force

# Recriar com todas as migrations
dotnet ef database update
```

### **Opção 2: Aplicar Migration Específica**

```powershell
# Aplicar apenas a migration em falta
dotnet ef database update AddVendedorIdToTransacao
```

### **Opção 3: Verificar Logs**

```powershell
# Ver output detalhado
dotnet ef database update --verbose
```

---

## ? CHECKLIST FINAL

```
? Migration criada (AddVendedorIdToTransacao.cs existe)
? Migration aplicada (dotnet ef database update executado)
? Coluna VendedorId existe na tabela Transacoes
? Foreign key criada
? Aplicação compila sem erros
? Página MinhasCompras abre sem erro
? Página MinhasVendas abre sem erro
```

---

**Depois de aplicar a migration, o erro deve estar resolvido!** ?
