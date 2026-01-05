# ?? Configura��o MailTrap - AutoMarket

## O que � MailTrap?
MailTrap � um **servi�o de email fake** para desenvolvimento. Os emails n�o s�o realmente enviados - ficam numa caixa de entrada virtual que s� voc� v�.

## ? Vantagens
- ? Sem passwords pessoais
- ? Interface web para ver emails
- ? Testa todo o fluxo de email
- ? 100% GR�TIS (at� 500 emails/m�s)
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

1. Ap�s login, v� a: **Email Testing** ? **Inboxes**
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

### Op��o A: Usando o Script PowerShell

```powershell
# Execute no diret�rio do projeto
.\setup-email-secrets.ps1

# Quando pedir:
# - Escolha op��o "2. Outro (SMTP customizado)"
# - SMTP Server: sandbox.smtp.mailtrap.io
# - SMTP Port: 587
# - SMTP Username: [Cole o username do MailTrap]
# - SMTP Password: [Cole a password do MailTrap]
# - Secure Options: 1. StartTls
# - From Email: noreply@automarket.com
# - From Name: AutoMarket
```

### Op��o B: Manual (Comandos PowerShell)

```powershell
# Navegue para o projeto
cd "<path-to-your-project>"

**Nota:** Navegue até ao diretório onde clonou o projeto AutoMarket.

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
# Execute a aplica��o
dotnet run

# Registe um novo utilizador
# - V� a: https://localhost:XXXX/Conta/Register
# - Preencha o formul�rio
# - Clique em "Registar"
```

---

## ?? Passo 5: Ver Email

1. Volte ao MailTrap: **https://mailtrap.io/inboxes**
2. Clique na sua **Inbox**
3. Deve ver o email de confirma��o!
4. Clique no email para ver o conte�do
5. Copie o link de confirma��o
6. Cole no browser para confirmar

---

## ? Checklist

```
? Conta MailTrap criada
? Credenciais SMTP copiadas
? User Secrets configurados
? Aplica��o executada
? Utilizador registado
? Email apareceu no MailTrap
? Link de confirma��o funcionou
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

### Email n�o aparece no MailTrap
- Verificar logs da aplica��o para "Email sent successfully"
- Verificar se inbox correta est� selecionada
- Aguardar 1-2 minutos (pode haver delay)

### Erro de conex�o SMTP
- Tentar porta 2525 em vez de 587
- Verificar firewall/antiv�rus

---

## ?? Limites (Free Tier)

- ? 500 emails/m�s
- ? 1 inbox
- ? Emails armazenados por 48h
- ? 50 requests/segundo

(Mais que suficiente para desenvolvimento!)

---

## ?? Seguran�a

- ? Emails nunca s�o realmente enviados
- ? Apenas voc� v� os emails
- ? Sem risco de spam
- ? Sem passwords pessoais expostas
- ? Pode revogar credenciais a qualquer momento

---

## ?? Pr�ximos Passos

Quando for para **produ��o**, migrar para:
- **SendGrid** (100 emails/dia gr�tis)
- **Mailgun** (5,000 emails/m�s gr�tis)
- **Azure Communication Services** (pay-as-you-go)

