# ?? Configura��o Mailgun - AutoMarket

## O que � Mailgun?
Mailgun � um **servi�o de email profissional** usado por empresas como GitHub, Lyft e Slack. Oferece 5,000 emails/m�s GR�TIS nos primeiros 3 meses.

## ? Vantagens
- ? **5,000 emails/m�s gr�tis** (primeiros 3 meses)
- ? **Emails reais** enviados
- ? **APIs poderosas** (SMTP + REST API)
- ? **Email validation** inclu�da
- ? **Logs detalhados**
- ? **Webhooks** para tracking
- ? **Muito usado em startups**

## ?? Limita��es
- ? Requer cart�o de cr�dito (mesmo no free tier)
- ? Ap�s 3 meses: $35/m�s OU pay-as-you-go ($0.80/1000 emails)
- ? Setup mais complexo que SendGrid

---

## ?? Passo 1: Criar Conta Mailgun

1. Aceda: **https://signup.mailgun.com/new/signup**
2. Preencha:
   - Email
   - Password forte
   - Nome completo
3. **Verificar email**
4. **Adicionar cart�o de cr�dito** (n�o ser� cobrado no free tier)
5. Complete o perfil

---

## ?? Passo 2: Obter Credenciais SMTP

### Op��o A: Sandbox Domain (Testes)

1. Ap�s login, v� a: **Sending** ? **Domains**
2. Clique no **sandbox domain** (algo como `sandboxXXXXX.mailgun.org`)
3. V� ao separador **SMTP credentials**
4. Use as credenciais:
   - **SMTP Hostname**: `smtp.mailgun.org`
   - **Port**: 587
   - **Username**: `postmaster@sandboxXXXXX.mailgun.org`
   - **Password**: (clique em "Reset password" se necess�rio)

?? **Nota:** Sandbox s� envia para emails autorizados!

### Op��o B: Custom Domain (Produ��o)

Se tiver dom�nio pr�prio:
1. **Sending** ? **Domains** ? **Add New Domain**
2. Adicionar registos DNS no seu provider
3. Aguardar verifica��o (pode demorar 24-48h)

---

## ?? Passo 3: Autorizar Email Destinat�rio (Sandbox)

Se usar sandbox domain:

1. **Sending** ? **Domains** ? [Seu sandbox]
2. Separador **Authorized Recipients**
3. Adicione seu email pessoal
4. Clique em **"Add Recipient"**
5. **Verifique email** de confirma��o

---

## ?? Passo 4: Configurar AutoMarket

### Op��o A: Usando o Script PowerShell

```powershell
# Execute no diret�rio do projeto
.\setup-email-secrets.ps1

# Quando pedir:
# - Escolha op��o "2. Outro (SMTP customizado)"
# - SMTP Server: smtp.mailgun.org
# - SMTP Port: 587
# - SMTP Username: postmaster@sandboxXXXXX.mailgun.org
# - SMTP Password: [Sua password Mailgun]
# - Secure Options: 1. StartTls
# - From Email: noreply@sandboxXXXXX.mailgun.org
# - From Name: AutoMarket
```

### Op��o B: Manual (Comandos PowerShell)

```powershell
# Navegue para o projeto
cd "<path-to-your-project>"

**Nota:** Navegue até ao diretório onde clonou o projeto AutoMarket.

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
# Execute a aplica��o
dotnet run

# Registe utilizador com EMAIL AUTORIZADO
# (O email que adicionou em "Authorized Recipients")
# - V� a: https://localhost:XXXX/Conta/Register
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
? Cart�o adicionado (sem cobran�as no free tier)
? Sandbox domain configurado
? Email destinat�rio autorizado
? Credenciais SMTP copiadas
? User Secrets configurados
? Aplica��o executada
? Email recebido
? Link de confirma��o funcionou
```

---

## ?? Troubleshooting

### Erro: "Free accounts are for test purposes only"
- Sandbox s� envia para **Authorized Recipients**
- Adicione seu email em: **Authorized Recipients**

### Erro: "Invalid SMTP credentials"
- Reset password em: **Domains** ? [Sandbox] ? **SMTP credentials**
- Verificar username: `postmaster@sandboxXXXXX.mailgun.org`

### Email n�o chega
- Verificar logs: **Sending** ? **Logs**
- Status deve ser "delivered"
- Se "failed", ver motivo no log

### Sandbox domain n�o funciona para produ��o
- **Solu��o:** Adicionar custom domain
- **Sending** ? **Domains** ? **Add New Domain**
- Seguir instru��es de DNS

---

## ?? Pre�os

### Free Trial (3 meses):
- ? 5,000 emails/m�s
- ? Todas as features
- ? API completa
- ?? Requer cart�o de cr�dito

### Depois dos 3 meses:

**Foundation Plan**: $35/m�s
- 50,000 emails inclu�dos
- $0.80/1,000 emails adicionais
- Email validation: 1,000 gr�tis/m�s

**Pay-as-you-go**: $0/m�s
- $0.80/1,000 emails enviados
- Sem m�nimo mensal
- Email validation: $0.001/valida��o

---

## ?? Seguran�a

- ? API Keys com scopes
- ? Webhooks para tracking
- ? DKIM/SPF autom�tico
- ? TLS/SSL obrigat�rio
- ? Logs completos

**Revogar API Key:**
1. **Settings** ? **API Security**
2. Clique em "Delete" ao lado da key
3. Crie nova se necess�rio

---

## ?? Features Avan�adas

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
Receber notifica��es quando:
- Email entregue
- Email aberto
- Link clicado
- Bounce
- Spam complaint

Configurar em: **Sending** ? **Webhooks**

---

## ?? Compara��o: Mailgun vs SendGrid

| Feature | Mailgun | SendGrid |
|---------|---------|----------|
| **Free Tier** | 5k/m�s (3 meses) | 100/dia (sempre) |
| **Requer Cart�o** | ? Sim | ? N�o |
| **Email Validation** | ? Inclu�da | ? Paga |
| **REST API** | ? Muito boa | ? Muito boa |
| **Webhooks** | ? Completos | ? Completos |
| **Pre�o ap�s free** | $35/m�s ou PAYG | $19.95/m�s |
| **Setup** | M�dio | F�cil |

---

## ?? Recomenda��o

**Use Mailgun se:**
- ? Precisa de email validation
- ? Quer pagar s� pelo que usa (PAYG)
- ? Startup em crescimento
- ? Precisa de logs muito detalhados

**Use SendGrid se:**
- ? Quer come�ar sem cart�o
- ? 100 emails/dia � suficiente
- ? Setup mais r�pido
- ? Or�amento limitado inicial

