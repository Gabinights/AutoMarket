# ?? Guia Rápido: Como Confirmar Email no Desenvolvimento

## ? Por Que Preciso de Senha de App do Gmail?

O Gmail **não aceita senhas normais** em aplicações por segurança. Se tentar, vai dar erro:
```
? SMTP authentication failed
```

### Mas você **NÃO PRECISA** configurar Gmail agora! 

## ? Solução Simples (Recomendada para Testes)

### **Método 1: Copiar Link dos Logs** (Mais Rápido!)

1. **Registar um utilizador** na aplicação
2. **Abrir Output no Visual Studio:**
   - Menu: `View` ? `Output` (ou `Ctrl+Alt+O`)
   - No dropdown, selecionar: **"AutoMarket - ASP.NET Core Web Server"**
3. **Procurar por:**
   ```
   ===========================================
   LINK DE CONFIRMAÇÃO (Desenvolvimento):
   https://localhost:5001/Conta/ConfirmarEmail?userId=...
   ===========================================
   ```
4. **Copiar o link completo** e colar no navegador
5. ? **Pronto!** Email confirmado, pode fazer login

---

### **Método 2: Desativar Confirmação Temporariamente**

Se está apenas a testar localmente e não quer procurar o link nos logs:

1. Abrir `Program.cs`
2. Procurar por `options.SignIn.RequireConfirmedEmail = true;` (linha ~50)
3. Mudar para:
   ```csharp
   options.SignIn.RequireConfirmedEmail = false;
   ```
4. Reiniciar a aplicação
5. ? Agora pode fazer login **imediatamente** após registar

**Importante:** Lembre-se de voltar a `true` antes de colocar em produção!

---

## ?? Quando Configurar Gmail de Verdade?

Configure Gmail quando:
- ? For colocar a aplicação **online** (produção)
- ? Quiser testar o **fluxo completo** de confirmação por email
- ? Precisar que **outros utilizadores** recebam emails

### Como Configurar (5 minutos)

Siga o guia completo em: **`CONFIGURAR_EMAIL.md`**

Resumo:
1. Gmail ? Segurança ? Ativar verificação em 2 etapas
2. Gmail ? Senhas de app ? Gerar nova senha (16 caracteres)
3. Visual Studio ? Clique direito no projeto ? **Manage User Secrets**
4. Colar configuração:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SmtpUsername": "seu-email@gmail.com",
       "SmtpPassword": "senha-de-16-caracteres",
       "FromEmail": "seu-email@gmail.com"
     }
   }
   ```

---

## ?? Resumo

| Situação | Solução |
|----------|---------|
| **Desenvolvimento/Testes** | Use o link dos logs OU desative confirmação |
| **Produção** | Configure Gmail com senha de app |
| **Testar emails de verdade** | Configure Gmail |

**Recomendação:** Para agora, use o **Método 1** (copiar link dos logs) ou **Método 2** (desativar confirmação). É mais rápido e não precisa mexer no Gmail! ??
