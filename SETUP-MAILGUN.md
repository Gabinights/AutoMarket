# ?? Configuração Mailgun - AutoMarket

## O que é Mailgun?
Mailgun é um **serviço de email profissional** usado por empresas como GitHub, Lyft e Slack. Oferece 5,000 emails/mês GRÁTIS nos primeiros 3 meses.

## ? Vantagens
- ? **5,000 emails/mês grátis** (primeiros 3 meses)
- ? **Emails reais** enviados
- ? **APIs poderosas** (SMTP + REST API)
- ? **Email validation** incluída
- ? **Logs detalhados**
- ? **Webhooks** para tracking
- ? **Muito usado em startups**

## ?? Limitações
- ? Requer cartão de crédito (mesmo no free tier)
- ? Após 3 meses: $35/mês OU pay-as-you-go ($0.80/1000 emails)
- ? Setup mais complexo que SendGrid

---

## ?? Passo 1: Criar Conta Mailgun

1. Aceda: **https://signup.mailgun.com/new/signup**
2. Preencha:
   - Email
   - Password forte
   - Nome completo
3. **Verificar email**
4. **Adicionar cartão de crédito** (não será cobrado no free tier)
5. Complete o perfil

---

## ?? Passo 2: Obter Credenciais SMTP

### Opção A: Sandbox Domain (Testes)

1. Após login, vá a: **Sending** ? **Domains**
2. Clique no **sandbox domain** (algo como `sandboxXXXXX.mailgun.org`)
3. Vá ao separador **SMTP credentials**
4. Use as credenciais:
   - **SMTP Hostname**: `smtp.mailgun.org`
   - **Port**: 587
   - **Username**: `postmaster@sandboxXXXXX.mailgun.org`
   - **Password**: (clique em "Reset password" se necessário)

?? **Nota:** Sandbox só envia para emails autorizados!

### Opção B: Custom Domain (Produção)

Se tiver domínio próprio:
1. **Sending** ? **Domains** ? **Add New Domain**
2. Adicionar registos DNS no seu provider
3. Aguardar verificação (pode demorar 24-48h)

---

## ?? Passo 3: Autorizar Email Destinatário (Sandbox)

Se usar sandbox domain:

1. **Sending** ? **Domains** ? [Seu sandbox]
2. Separador **Authorized Recipients**
3. Adicione seu email pessoal
4. Clique em **"Add Recipient"**
5. **Verifique email** de confirmação

---

## ?? Passo 4: Configurar AutoMarket

### Opção A: Usando o Script PowerShell

```powershell
# Execute no diretório do projeto
.\setup-email-secrets.ps1

# Quando pedir:
# - Escolha opção "2. Outro (SMTP customizado)"
# - SMTP Server: smtp.mailgun.org
# - SMTP Port: 587
# - SMTP Username: postmaster@sandboxXXXXX.mailgun.org
# - SMTP Password: [Sua password Mailgun]
# - Secure Options: 1. StartTls
# - From Email: noreply@sandboxXXXXX.mailgun.org
# - From Name: AutoMarket
```

### Opção B: Manual (Comandos PowerShell)

```powershell
# Navegue para o projeto
cd "C:\Users\nunos\Source\Repos\AutoMarket2"

# Configure User Secrets
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.mailgun.org"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUsername" "postmaster@sandboxXXXXX.mailgun.org"
dotnet user-secrets set "EmailSettings:SmtpPassword" "sua-password-mailgun"
dotnet user-secrets set "EmailSettings:FromEmail" "noreply@sandboxXXXXX.mailgun.org"
dotnet user-secrets set "EmailSettings:FromName" "AutoMarket"
dotnet user-secrets set "EmailSettings:SecureSocketOptions" "StartTls"
```

---

## ?? Passo 5: Testar

```powershell
# Execute a aplicação
dotnet run

# Registe utilizador com EMAIL AUTORIZADO
# (O email que adicionou em "Authorized Recipients")
# - Vá a: https://localhost:XXXX/Conta/Register
# - Use email autorizado
# - Clique em "Registar"

# Verifique sua caixa de entrada!
```

---

## ?? Passo 6: Monitorizar

1. **Sending** ? **Logs**
2. Veja:
   - Emails enviados
   - Status de entrega
   - Aberturas/Cliques (com tracking ativado)
   - Bounces/Complaints

---

## ?? Passo 7: Usar REST API (Alternativa ao SMTP)

Mailgun oferece REST API mais poderosa:

### Obter API Key:
1. **Settings** ? **API Keys**
2. Copie **Private API key**

### Modificar EmailSender.cs (Opcional):

```csharp
// Adicionar pacote NuGet
// dotnet add package RestSharp

public async Task SendEmailViaMailgunAPI(string to, string subject, string message)
{
    var client = new RestClient("https://api.mailgun.net/v3");
    var request = new RestRequest("YOUR_DOMAIN/messages", Method.Post);
    
    request.AddParameter("domain", "YOUR_DOMAIN", ParameterType.UrlSegment);
    request.AddHeader("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{API_KEY}"))}");
    
    request.AddParameter("from", "noreply@YOUR_DOMAIN");
    request.AddParameter("to", to);
    request.AddParameter("subject", subject);
    request.AddParameter("html", message);
    
    var response = await client.ExecuteAsync(request);
    // Handle response
}
```

---

## ? Checklist

```
? Conta Mailgun criada
? Cartão adicionado (sem cobranças no free tier)
? Sandbox domain configurado
? Email destinatário autorizado
? Credenciais SMTP copiadas
? User Secrets configurados
? Aplicação executada
? Email recebido
? Link de confirmação funcionou
```

---

## ?? Troubleshooting

### Erro: "Free accounts are for test purposes only"
- Sandbox só envia para **Authorized Recipients**
- Adicione seu email em: **Authorized Recipients**

### Erro: "Invalid SMTP credentials"
- Reset password em: **Domains** ? [Sandbox] ? **SMTP credentials**
- Verificar username: `postmaster@sandboxXXXXX.mailgun.org`

### Email não chega
- Verificar logs: **Sending** ? **Logs**
- Status deve ser "delivered"
- Se "failed", ver motivo no log

### Sandbox domain não funciona para produção
- **Solução:** Adicionar custom domain
- **Sending** ? **Domains** ? **Add New Domain**
- Seguir instruções de DNS

---

## ?? Preços

### Free Trial (3 meses):
- ? 5,000 emails/mês
- ? Todas as features
- ? API completa
- ?? Requer cartão de crédito

### Depois dos 3 meses:

**Foundation Plan**: $35/mês
- 50,000 emails incluídos
- $0.80/1,000 emails adicionais
- Email validation: 1,000 grátis/mês

**Pay-as-you-go**: $0/mês
- $0.80/1,000 emails enviados
- Sem mínimo mensal
- Email validation: $0.001/validação

---

## ?? Segurança

- ? API Keys com scopes
- ? Webhooks para tracking
- ? DKIM/SPF automático
- ? TLS/SSL obrigatório
- ? Logs completos

**Revogar API Key:**
1. **Settings** ? **API Security**
2. Clique em "Delete" ao lado da key
3. Crie nova se necessário

---

## ?? Features Avançadas

### Email Validation API:
```csharp
// Validar email antes de enviar
GET https://api.mailgun.net/v4/address/validate
  ?address=email@example.com
  &api_key=YOUR_KEY

// Resposta:
{
  "address": "email@example.com",
  "is_valid": true,
  "parts": { "local_part": "email", "domain": "example.com" }
}
```

### Webhooks:
Receber notificações quando:
- Email entregue
- Email aberto
- Link clicado
- Bounce
- Spam complaint

Configurar em: **Sending** ? **Webhooks**

---

## ?? Comparação: Mailgun vs SendGrid

| Feature | Mailgun | SendGrid |
|---------|---------|----------|
| **Free Tier** | 5k/mês (3 meses) | 100/dia (sempre) |
| **Requer Cartão** | ? Sim | ? Não |
| **Email Validation** | ? Incluída | ? Paga |
| **REST API** | ? Muito boa | ? Muito boa |
| **Webhooks** | ? Completos | ? Completos |
| **Preço após free** | $35/mês ou PAYG | $19.95/mês |
| **Setup** | Médio | Fácil |

---

## ?? Recomendação

**Use Mailgun se:**
- ? Precisa de email validation
- ? Quer pagar só pelo que usa (PAYG)
- ? Startup em crescimento
- ? Precisa de logs muito detalhados

**Use SendGrid se:**
- ? Quer começar sem cartão
- ? 100 emails/dia é suficiente
- ? Setup mais rápido
- ? Orçamento limitado inicial

