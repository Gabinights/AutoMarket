# ?? Setup de Desenvolvimento - AutoMarket

## ? Quick Start (5 minutos)

### 1. Inicializar User Secrets
```bash
cd C:\Users\dmalm\Documents\utad\3ano1sem\LAB4\projeto
dotnet user-secrets init
```

### 2. Configurar Secrets (copiar e colar)

```bash
# Encryption Key (CRÍTICO - RGPD)
dotnet user-secrets set "Encryption:Key" "dev-key-encriptacao-secreta-minimo-32-caracteres"

# Database (LocalDB)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=automarket_dev;Integrated Security=true;TrustServerCertificate=true;"

# Email (use conta pessoal para testar)
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUsername" "seu-email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "sua-app-password-google"
dotnet user-secrets set "EmailSettings:FromEmail" "seu-email@gmail.com"
dotnet user-secrets set "EmailSettings:FromName" "AutoMarket Dev"

# Admin (desenvolvimento)
dotnet user-secrets set "DefaultAdmin:Email" "admin-dev@localhost"
dotnet user-secrets set "DefaultAdmin:Password" "DevPassword@123456"
```

### 3. Verificar
```bash
dotnet user-secrets list
```

### 4. Rodar
```bash
dotnet run
```

---

## ?? Para Novos Devs

Se é novo no projeto:

1. `git clone https://github.com/Gabinights/AutoMarket.git`
2. `cd AutoMarket`
3. `dotnet user-secrets init`
4. Cole os comandos acima
5. `dotnet run`

---

## ?? Importante

- ? **NÃO** existe `appsettings.json` com credenciais no Git
- ? User Secrets está protegido (local apenas)
- ? Cada dev tem suas próprias credenciais
- ? Produção usa variáveis de ambiente

---

## ?? Problemas?

**Secrets não carregam?**
```bash
dotnet user-secrets list
```

**Esqueceu um secret?**
```bash
dotnet user-secrets set "chave" "novo-valor"
```

**Limpar tudo?**
```bash
dotnet user-secrets clear
```
