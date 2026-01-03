# ?? Configuração SendGrid - AutoMarket

## O que é SendGrid?
SendGrid é um **serviço profissional de email** usado por empresas como Uber, Spotify e Airbnb. Oferece 100 emails/dia GRÁTIS.

## ? Vantagens
- ? **Emails reais** enviados aos utilizadores
- ? **100 emails/dia grátis** (para sempre)
- ? **Relatórios e analytics**
- ? **Alta deliverability** (não vai para spam)
- ? **API Key** (mais seguro que passwords)
- ? **Escalável** (até milhões de emails)

---

## ?? Passo 1: Criar Conta SendGrid

1. Aceda: **https://signup.sendgrid.com/**
2. Preencha:
   - Email
   - Password forte
   - Nome completo
   - Website: `automarket.local` (ou deixe em branco)
3. Confirme email
4. Complete o perfil:
   - **Role**: Developer
   - **Primary use**: Transactional Emails
   - **Do you send more than 100k emails/month**: No

---

## ?? Passo 2: Criar API Key

1. Após login, vá a: **Settings** ? **API Keys**
2. Clique em **"Create API Key"**
3. Configure:
   - **API Key Name**: `AutoMarket Production`
   - **API Key Permissions**: 
     - Escolha **"Restricted Access"**
     - Ative apenas: **Mail Send** ? Full Access
4. Clique em **"Create & View"**
5. **?? COPIE A API KEY AGORA** (só aparece uma vez!)
   - Formato: `SG.xxxx-xxxx.yyyy_zzzz`

---

## ?? Passo 3: Verificar Sender Identity

SendGrid exige verificar o email "From" para prevenir spam:

### Opção A: Single Sender Verification (Mais rápido)

1. Vá a: **Settings** ? **Sender Authentication** ? **Single Sender Verification**
2. Clique em **"Create New Sender"**
3. Preencha:
   - **From Name**: AutoMarket
   - **From Email Address**: seu-email@gmail.com (ou qualquer email que controle)
   - **Reply To**: (mesmo que From)
   - **Company**: AutoMarket
   - **Address, City, State, Zip, Country**: (pode ser fictício)
4. Clique em **"Create"**
5. **Verifique o email** que recebeu no endereço "From Email"
6. Clique no link de verificação

### Opção B: Domain Authentication (Produção)

Se tiver domínio próprio (automarket.com):
1. **Settings** ? **Sender Authentication** ? **Authenticate Your Domain**
2. Siga wizard para adicionar registos DNS
3. (Mais complexo - use Single Sender para começar)

---

## ?? Passo 4: Configurar AutoMarket

### Opção A: Usando o Script PowerShell

```powershell
# Execute no diretório do projeto
.\setup-email-secrets.ps1

# Quando pedir:
# - Escolha opção "2. Outro (SMTP customizado)"
# - SMTP Server: smtp.sendgrid.net
# - SMTP Port: 587
# - SMTP Username: apikey (exatamente assim!)
# - SMTP Password: [Cole a API Key SG.xxxx...]
# - Secure Options: 1. StartTls
# - From Email: [O email que verificou no passo 3]
# - From Name: AutoMarket
```

### Opção B: Manual (Comandos PowerShell)

```powershell
# Navegue para o projeto
cd "C:\Users\nunos\Source\Repos\AutoMarket2"

# Configure User Secrets
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.sendgrid.net"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUsername" "apikey"
dotnet user-secrets set "EmailSettings:SmtpPassword" "SG.sua-api-key-aqui"
dotnet user-secrets set "EmailSettings:FromEmail" "seu-email-verificado@gmail.com"
dotnet user-secrets set "EmailSettings:FromName" "AutoMarket"
dotnet user-secrets set "EmailSettings:SecureSocketOptions" "StartTls"
```

---

## ?? Passo 5: Testar

```powershell
# Execute a aplicação
dotnet run

# Registe um novo utilizador com SEU email real
# - Vá a: https://localhost:XXXX/Conta/Register
# - Use seu email pessoal
# - Clique em "Registar"

# Verifique sua caixa de entrada!
```

---

## ?? Passo 6: Monitorizar (Opcional)

1. Vá a: **Activity** no dashboard SendGrid
2. Veja estatísticas:
   - Emails enviados
   - Emails entregues
   - Aberturas (opens)
   - Cliques
   - Bounces/Spam

---

## ? Checklist

```
? Conta SendGrid criada
? API Key criada e copiada
? Sender Identity verificada
? User Secrets configurados
? Aplicação executada
? Email REAL recebido
? Link de confirmação funcionou
```

---

## ?? Troubleshooting

### Erro: "Mail Send permission required"
- Verificar que API Key tem permissão **Mail Send ? Full Access**

### Erro: "The from address does not match a verified Sender Identity"
- Verificar que `FromEmail` é exatamente o email verificado no passo 3
- Ir a **Settings** ? **Sender Authentication** e confirmar status

### Email não chega
- Verificar pasta spam/lixo
- Ir a **Activity** no SendGrid para ver status da entrega
- Verificar logs da aplicação para "Email sent successfully"

### Rate limit exceeded
- Free tier: 100 emails/dia
- Para mais, upgrade para plano pago (a partir de $19.95/mês para 50k emails)

---

## ?? Limites (Free Tier)

- ? **100 emails/dia** (para sempre!)
- ? Email support (help@sendgrid.com)
- ? Estatísticas básicas
- ? API completa
- ? Webhooks

**Suficiente para:**
- Desenvolvimento
- Produção inicial (até ~1000 utilizadores/mês)

---

## ?? Segurança

- ? API Key com scopes limitados
- ? Rotação de keys fácil
- ? Logs de acesso
- ? 2FA disponível
- ? Sem passwords pessoais

**Para revogar API Key:**
1. **Settings** ? **API Keys**
2. Clique no ícone de lixo ao lado da key
3. Crie nova key se necessário

---

## ?? Quando Fazer Upgrade?

Considere upgrade para plano pago quando:
- Enviar mais de 100 emails/dia
- Precisar de suporte prioritário
- Querer analytics avançadas
- Precisar de IP dedicado

**Preços:**
- **Essentials**: $19.95/mês ? 50k emails
- **Pro**: $89.95/mês ? 100k emails + IP dedicado
- **Premier**: Custom ? Milhões + Account Manager

---

## ?? Próximos Passos

### Para Produção:
1. ? Autenticar domínio próprio (automarket.com)
2. ? Configurar templates de email
3. ? Implementar webhooks (tracking de opens/clicks)
4. ? Configurar rate limiting no código
5. ? Monitorizar reputação do sender

### Exemplo de Template:
SendGrid permite criar templates HTML profissionais:
- **Marketing** ? **Dynamic Templates**
- Usar handlebars para personalização
- Testar antes de enviar

