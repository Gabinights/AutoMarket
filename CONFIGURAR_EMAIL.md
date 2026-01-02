# ?? Configuração de Email - AutoMarket

## Problema
Os emails de confirmação não estão sendo enviados porque o serviço SMTP não está configurado.

## Solução: Configurar Gmail SMTP

### Passo 1: Criar uma Senha de App no Gmail

1. Acesse sua conta Google em https://myaccount.google.com
2. Vá para **Segurança**
3. Ative a **Verificação em duas etapas** (se ainda não estiver ativa)
4. Depois de ativar, volte para **Segurança**
5. Procure por **Senhas de app** (App Passwords)
6. Selecione "Outro (nome personalizado)" e digite "AutoMarket"
7. Clique em **Gerar**
8. **Copie a senha gerada** (ela aparece sem espaços, exemplo: `abcd efgh ijkl mnop`)

### Passo 2: Configurar User Secrets

No Visual Studio, clique com o botão direito no projeto **AutoMarket** e selecione **Manage User Secrets**.

Adicione a seguinte configuração (substitua pelos seus dados):

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "seu-email@gmail.com",
    "SmtpPassword": "abcdefghijklmnop",
    "FromEmail": "seu-email@gmail.com",
    "FromName": "AutoMarket",
    "SecureSocketOptions": "StartTls"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AutoMarket;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
```

**Importante:** 
- `SmtpUsername`: Seu email completo do Gmail
- `SmtpPassword`: A senha de app gerada (sem espaços)
- `FromEmail`: Seu email completo do Gmail

### Passo 3: Reiniciar a Aplicação

Depois de salvar o arquivo `secrets.json`, reinicie a aplicação no Visual Studio (Stop + Start).

## Alternativa: Configuração via Terminal

Se preferir usar o terminal, execute os seguintes comandos na pasta do projeto:

```bash
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUsername" "seu-email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "abcdefghijklmnop"
dotnet user-secrets set "EmailSettings:FromEmail" "seu-email@gmail.com"
dotnet user-secrets set "EmailSettings:SecureSocketOptions" "StartTls"
```

## Verificar se Está Funcionando

1. Registre um novo utilizador
2. Verifique os **logs** na janela de Output do Visual Studio
3. Procure por mensagens como:
   - ? `Email sent successfully to xxx@xxx.com`
   - ? `Email settings not configured` (significa que ainda não configurou)
   - ? `SMTP authentication failed` (senha incorreta)

## Troubleshooting

### Erro: "SMTP authentication failed"
- Verifique se copiou a **senha de app** corretamente (sem espaços)
- Verifique se a verificação em duas etapas está ativa

### Erro: "Failed to connect to SMTP server"
- Verifique sua conexão com a internet
- Verifique se o firewall não está bloqueando a porta 587

### Não recebo o email
- Verifique a pasta de **Spam/Lixo Eletrônico**
- Aguarde alguns minutos (pode haver atraso)
- Verifique os logs para confirmar que o email foi enviado

## Solução Temporária para Desenvolvimento

Se não quiser configurar email agora, pode:

1. Desativar a confirmação de email no `Program.cs`:
```csharp
options.SignIn.RequireConfirmedEmail = false; // Mudar para false
```

2. Ou usar o link que aparece nos logs do Visual Studio
   - Registre um utilizador
   - Copie o link de confirmação dos logs
   - Cole no navegador
