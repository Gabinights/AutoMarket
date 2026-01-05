# Script para corrigir encoding UTF-8 em ficheiros Markdown e PowerShell
# AutoMarket - Fix Encoding Phase 2

$files = @(
    "AUDIT_ARCHITECTURE.md",
    "AUDIT_SECURITY.md",
    "AUDIT_DATA_PERFORMANCE.md",
    "EXECUTIVE_REPORT.md",
    "fix-encoding.ps1",
    "fix-hardcoded-paths.ps1",
    "SETUP-MAILGUN.md",
    "SETUP-MAILTRAP.md",
    "SETUP-SENDGRID.md",
    "CORRECAO-ACCESS-DENIED-404.md",
    "PAGINA-PERFIL-COMPLETA.md"
)

# Mapa completo de substituições (incluindo novos padrões encontrados)
$replacements = @{
    # Padrões comuns
    "S?nior" = "Sénior"
    "S?nior" = "Sénior"
    "Viola??es" = "Violações"
    "SUM?RIO" = "SUMÁRIO"
    "SEGURAN?A" = "SEGURANÇA"
    "CR?TICO" = "CRÍTICO"
    "Vulner?veis" = "Vulneráveis"
    "Encripta??o" = "Encriptação"
    "Configura??o" = "Configuração"
    "documenta??o" = "documentação"
    "conte?do" = "conteúdo"
    "diret?rio" = "diretório"
    "m?s" = "mês"
    "GR?TIS" = "GRÁTIS"
    
    # Padrões já conhecidos
    "n?o" = "não"
    "aprova??o" = "aprovação"
    "obrigat?rio" = "obrigatório"
    "obrigat?ria" = "obrigatória"
    "confirma??o" = "confirmação"
    "M?nimo" = "Mínimo"
    "mai?scula" = "maiúscula"
    "min?scula" = "minúscula"
    "Seguran?a" = "Segurança"
    "car?cter" = "carácter"
    "prefer?ncias" = "preferências"
    "Ve?culos" = "Veículos"
    "Altera??es" = "Alterações"
    "Hist?rico" = "Histórico"
    "transa??es" = "transações"
    "Sess?o" = "Sessão"
    "permiss?es" = "permissões"
    "P?gina" = "Página"
    "valida??o" = "validação"
    "?ltimo" = "último"
    "Estat?sticas" = "Estatísticas"
    "dispon?vel" = "disponível"
    
    # Novos padrões específicos
    "requ?rem" = "requerem"
    "cart?o" = "cartão"
    "naveg?vel" = "navegável"
    "f?cil" = "fácil"
    "especif?co" = "específico"
    "t?cnico" = "técnico"
    "an?lise" = "análise"
    "pr?tico" = "prático"
    "cr?tico" = "crítico"
    "autom?tica" = "automática"
    "b?sico" = "básico"
    "p?blico" = "público"
    "usu?rio" = "usuário"
    "execu??o" = "execução"
    "solu??o" = "solução"
    "verifica??o" = "verificação"
    "corre??o" = "correção"
    "instala??o" = "instalação"
    
    # Cabeçalhos e termos técnicos
    "ARQUITETURA" = "ARQUITETURA"
    "PERFORMANCE" = "PERFORMANCE"
    "SEGURAN?A" = "SEGURANÇA"
    "SUMÁRIO" = "SUMÁRIO"
    "CRÍTICO" = "CRÍTICO"
    "VULNERABILIDADES" = "VULNERABILIDADES"
    "RECOMENDA??O" = "RECOMENDAÇÃO"
    "IMPLEMENTA??O" = "IMPLEMENTAÇÃO"
}

# Contador de ficheiros processados
$processedCount = 0
$successCount = 0
$errorCount = 0

Write-Host "`n=== INICIANDO CORREÇÃO DE ENCODING UTF-8 ===" -ForegroundColor Cyan
Write-Host "Ficheiros a processar: $($files.Count)`n" -ForegroundColor White

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "[$(($processedCount + 1))/$($files.Count)] Processando: $file" -ForegroundColor Yellow
        
        try {
            # Ler conteúdo com detecção automática de encoding
            $content = Get-Content -Path $file -Raw -Encoding Default
            
            # Aplicar todas as substituições
            $replacementsMade = 0
            foreach ($key in $replacements.Keys) {
                if ($content -match [regex]::Escape($key)) {
                    $content = $content -replace [regex]::Escape($key), $replacements[$key]
                    $replacementsMade++
                }
            }
            
            # Guardar com UTF-8 sem BOM (padrão para Markdown)
            $utf8NoBom = New-Object System.Text.UTF8Encoding $false
            [System.IO.File]::WriteAllText($file, $content, $utf8NoBom)
            
            if ($replacementsMade -gt 0) {
                Write-Host "  ? Corrigido: $file ($replacementsMade substituições)" -ForegroundColor Green
            } else {
                Write-Host "  ? OK: $file (sem correções necessárias)" -ForegroundColor Gray
            }
            
            $successCount++
        }
        catch {
            Write-Host "  ? ERRO: $file - $($_.Exception.Message)" -ForegroundColor Red
            $errorCount++
        }
        
        $processedCount++
    } else {
        Write-Host "  - Ficheiro não encontrado: $file" -ForegroundColor DarkGray
        $processedCount++
    }
}

# Sumário final
Write-Host "`n=== SUMÁRIO DA CORREÇÃO ===" -ForegroundColor Cyan
Write-Host "Total processados:  $processedCount" -ForegroundColor White
Write-Host "Sucesso:            $successCount" -ForegroundColor Green
Write-Host "Erros:              $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Gray" })
Write-Host "Não encontrados:    $($files.Count - $processedCount)" -ForegroundColor DarkGray

if ($successCount -gt 0) {
    Write-Host "`n? Encoding UTF-8 corrigido com sucesso!" -ForegroundColor Green
} else {
    Write-Host "`n? Nenhum ficheiro foi corrigido." -ForegroundColor Yellow
}

Write-Host "`nPróximos passos:" -ForegroundColor Cyan
Write-Host "1. Verificar os ficheiros corrigidos" -ForegroundColor White
Write-Host "2. git add ." -ForegroundColor White
Write-Host "3. git commit -m 'fix: Corrigir encoding UTF-8 em ficheiros Markdown e PowerShell'" -ForegroundColor White
Write-Host "4. git push origin S3`n" -ForegroundColor White
