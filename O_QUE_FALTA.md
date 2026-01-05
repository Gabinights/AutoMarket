# ?? O QUE FALTA - CHECKLIST PRÁTICO

## ?? CRÍTICO (Impede funcionamento)
Nada! Backend está 100% funcional.

---

## ?? IMPORTANTE (Views/UI para funcionalidades prontas)

### 1. **CRUD de Anúncios (Vendedor)**
**Status:** Lógica pronta, Views faltam
**Arquivos necessários:**
- [ ] `Areas/Vendedores/Views/Anuncios/Criar.cshtml` - Form para criar novo anúncio
- [ ] `Areas/Vendedores/Views/Anuncios/Editar.cshtml` - Form para editar anúncio
- [ ] `Areas/Vendedores/Views/Anuncios/Listar.cshtml` - Lista de anúncios do vendedor
- [ ] `Areas/Vendedores/Views/Anuncios/Galeria.cshtml` - Gestão de imagens

**O que fazer:**
1. Criar forms com campos: Título, Marca, Modelo, Ano, Preço, Km, Combustível, Caixa, Localização, Descrição
2. Upload de múltiplas imagens
3. Validação cliente + servidor
4. Botões: Criar, Editar, Pausar, Remover

---

### 2. **Listar Anúncios Vendidos/Reservados (Vendedor)**
**Status:** Lógica pronta, Views faltam
**Arquivos necessários:**
- [ ] `Areas/Vendedores/Views/Anuncios/Vendidos.cshtml` - Lista de veículos vendidos
- [ ] `Areas/Vendedores/Views/Anuncios/Reservados.cshtml` - Lista de veículos reservados

**O que fazer:**
1. Listar com filtros por estado
2. Mostrar data, comprador, preço
3. Opção de visualizar detalhes/transação

---

### 3. **Página de Detalhes do Veículo (Todos)**
**Status:** Controller pronto (VeiculosController), View falta
**Arquivos necessários:**
- [ ] `Areas/Public/Views/Veiculos/Detalhe.cshtml` - Página completa do anúncio

**O que fazer:**
1. Galeria de imagens (carousel)
2. Todos os detalhes do veículo
3. Botões: Reservar, Agendar Visita, Adicionar aos Favoritos
4. Dados do vendedor + contacto
5. Histórico de visitas/reservas (se comprador)

---

### 4. **Página de Listagem de Veículos (Publico)**
**Status:** Controller pronto, View falta
**Arquivos necessários:**
- [ ] `Areas/Public/Views/Veiculos/Index.cshtml` - Listagem com filtros

**O que fazer:**
1. Grid/Tabela com veículos
2. Filtros: Categoria, Marca, Ano, Preço, Km, Combustível, Caixa, Localização
3. Ordenação: Mais recentes, Preço, Km
4. Paginação
5. Thumbnails com preço
6. Link para detalhe

---

### 5. **Criar Novo Admin (Backoffice)**
**Status:** Backend pronto, UI falta
**Arquivos necessários:**
- [ ] `Areas/Admin/Views/Utilizadores/CriarAdmin.cshtml` - Form para criar novo admin

**O que fazer:**
1. Form com: Email, Password, Nome
2. Validações
3. Botão criar
4. Redirecionamento + notificação de sucesso

---

## ?? OPCIONAL (Melhorias/Polish)

### 6. **Gerir Anúncios - Interface Admin**
**Status:** Lógica pronta, UI falta
- [ ] `Areas/Admin/Views/Anuncios/Index.cshtml` - Lista de todos os anúncios (admin)
- [ ] `Areas/Admin/Views/Anuncios/Detalhe.cshtml` - Ver + pausar/remover

---

### 7. **Checkout Completo**
**Status:** Simulado pronto, UI falta
- [ ] `Areas/Public/Views/Checkout/Index.cshtml` - Página de pagamento
- [ ] `Areas/Public/Views/Checkout/Confirmacao.cshtml` - Confirmação

---

### 8. **Página de Termos e Políticas**
**Status:** Não implementada
- [ ] `Areas/Public/Views/Home/Termos.cshtml`
- [ ] `Areas/Public/Views/Home/Politicas.cshtml`
- [ ] `Areas/Public/Views/Home/Contactos.cshtml`

**O que fazer:**
1. Adicionar link no footer
2. Conteúdo básico (template)

---

## ?? RESUMO DO ESFORÇO

| Prioridade | Item | Esforço | Tempo |
|---|---|---|---|
| ?? Crítico | - | - | - |
| ?? Alto | CRUD Anúncios | 2-3 views | 2h |
| ?? Alto | Vendidos/Reservados | 2 views | 1h |
| ?? Alto | Detalhe Veículo | 1 view | 1h |
| ?? Alto | Listagem Veículos | 1 view | 1h |
| ?? Médio | Criar Admin | 1 view | 30min |
| ?? Baixo | Gerir Anúncios (Admin) | 2 views | 1h |
| ?? Baixo | Checkout UI | 2 views | 1h |
| ?? Baixo | Termos/Políticas | 3 views | 1h |

**Total: ~9h de trabalho frontend**

---

## ?? PRIORIDADE RECOMENDADA

**Se tempo limitado, fazer APENAS:**

1. ? **Detalhe de Veículo** (1h) - Crítico para usar o site
2. ? **Listagem de Veículos com Filtros** (1h) - Crítico para usar o site
3. ? **CRUD Anúncios** (2-3h) - Sem isto vendedor não consegue criar anúncio
4. ? **Checkout** (1h) - Completar fluxo de compra

**Estimado: 5-6h ? Sistema funcionalmente completo**

---

## ?? PRÓXIMOS PASSOS

1. Criar pasta `Areas/Vendedores/Views/Anuncios/`
2. Adicionar `VeiculoController` em `Areas/Public/Controllers/`
3. Criar views:
   - Veiculos/Index.cshtml (listagem)
   - Veiculos/Detalhe.cshtml (detalhe)
   - Anuncios/Criar.cshtml (criar anúncio)
   - Anuncios/Editar.cshtml (editar anúncio)
   - Anuncios/Listar.cshtml (meus anúncios)

---

**TL;DR:** Backend 100% pronto. Faltam apenas views para vendedor criar/editar anúncios e página de detalhes do veículo.
