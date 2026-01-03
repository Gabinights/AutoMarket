# ?? Configuração MailTrap - AutoMarket

## O que é MailTrap?
MailTrap é um **serviço de email fake** para desenvolvimento. Os emails não são realmente enviados - ficam numa caixa de entrada virtual que só você vê.

## ? Vantagens
- ? Sem passwords pessoais
- ? Interface web para ver emails
- ? Testa todo o fluxo de email
- ? 100% GRÁTIS (até 500 emails/mês)
- ? Sem risco de spam/vazamentos

---

## ?? Passo 1: Criar Conta MailTrap

1. Aceda: **https://mailtrap.io/**
2. Clique em **"Sign Up Free"**
3. Registe-se com:
   - Email (pode ser qualquer)
   - Password forte
   - OU login com GitHub/Google

---

## ?? Passo 2: Obter Credenciais SMTP

1. Após login, vá a: **Email Testing** ? **Inboxes**
2. Clique em **"My Inbox"** (ou crie uma nova inbox)
3. No separador **"SMTP Settings"**, escolha:
   - **Integration**: .NET
   - Copie as credenciais que aparecem:
     - **Host**: sandbox.smtp.mailtrap.io
     - **Port**: 587 (ou 2525)
     - **Username**: (algo como `1a2b3c4d5e6f7g`)
     - **Password**: (algo como `9h8i7j6k5l4m3n`)

---

## ?? Passo 3: Configurar AutoMarket

### Opção A: Usando o Script PowerShell

```powershell
# Execute no diretório do projeto
.\setup-email-secrets.ps1

# Quando pedir:
# - Escolha opção "2. Outro (SMTP customizado)"
# - SMTP Server: sandbox.smtp.mailtrap.io
# - SMTP Port: 587
# - SMTP Username: [Cole o username do MailTrap]
# - SMTP Password: [Cole a password do MailTrap]
# - Secure Options: 1. StartTls
# - From Email: noreply@automarket.com
# - From Name: AutoMarket
```

### Opção B: Manual (Comandos PowerShell)

```powershell
# Navegue para o projeto
cd "C:\Users\nunos\Source\Repos\AutoMarket2"

# Configure User Secrets
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:SmtpServer" "sandbox.smtp.mailtrap.io"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUsername" "SEU-USERNAME-MAILTRAP"
dotnet user-secrets set "EmailSettings:SmtpPassword" "SUA-PASSWORD-MAILTRAP"
dotnet user-secrets set "EmailSettings:FromEmail" "noreply@automarket.com"
dotnet user-secrets set "EmailSettings:FromName" "AutoMarket"
dotnet user-secrets set "EmailSettings:SecureSocketOptions" "StartTls"
```

---

## ?? Passo 4: Testar

```powershell
# Execute a aplicação
dotnet run

# Registe um novo utilizador
# - Vá a: https://localhost:XXXX/Conta/Register
# - Preencha o formulário
# - Clique em "Registar"
```

---

## ?? Passo 5: Ver Email

1. Volte ao MailTrap: **https://mailtrap.io/inboxes**
2. Clique na sua **Inbox**
3. Deve ver o email de confirmação!
4. Clique no email para ver o conteúdo
5. Copie o link de confirmação
6. Cole no browser para confirmar

---

## ? Checklist

```
? Conta MailTrap criada
? Credenciais SMTP copiadas
? User Secrets configurados
? Aplicação executada
? Utilizador registado
? Email apareceu no MailTrap
? Link de confirmação funcionou
```

---

## ?? Troubleshooting

### Erro: "Email settings not configured"
```powershell
# Verificar secrets configurados
dotnet user-secrets list

# Deve mostrar:
# EmailSettings:SmtpServer = sandbox.smtp.mailtrap.io
# EmailSettings:SmtpUsername = xxxxx
```

### Email não aparece no MailTrap
- Verificar logs da aplicação para "Email sent successfully"
- Verificar se inbox correta está selecionada
- Aguardar 1-2 minutos (pode haver delay)

### Erro de conexão SMTP
- Tentar porta 2525 em vez de 587
- Verificar firewall/antivírus

---

## ?? Limites (Free Tier)

- ? 500 emails/mês
- ? 1 inbox
- ? Emails armazenados por 48h
- ? 50 requests/segundo

(Mais que suficiente para desenvolvimento!)

---

## ?? Segurança

- ? Emails nunca são realmente enviados
- ? Apenas você vê os emails
- ? Sem risco de spam
- ? Sem passwords pessoais expostas
- ? Pode revogar credenciais a qualquer momento

---

## ?? Próximos Passos

Quando for para **produção**, migrar para:
- **SendGrid** (100 emails/dia grátis)
- **Mailgun** (5,000 emails/mês grátis)
- **Azure Communication Services** (pay-as-you-go)

