# Script para corrigir encoding UTF-8 em ficheiros Markdown e PowerShell
# AutoMarket - Fix Encoding Phase 2 - Versao Corrigida

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
    "PAGINA-PERFIL-COMPLETA.md",
    "CORRECOES_FASE2_ENCODING.md",
    "CORRECOES_IMPLEMENTADAS.md",
    "RELATORIO_FINAL_COMPLETO.md"
)

# Mapa CORRIGIDO de substituicoes - SEM duplicados, SEM no-ops
$replacements = @{
    # Padrao 1: Caractere unico corrupto (?)
    "S?nior" = "Sénior"
    "SUM?RIO" = "SUMÁRIO"
    "SEGURAN?A" = "SEGURANÇA"
    "CR?TICO" = "CRÍTICO"
    "Configura??o" = "Configuração"
    "documenta??o" = "documentação"
    "conte?do" = "conteúdo"
    "diret?rio" = "diretório"
    "m?s" = "mês"
    "GR?TIS" = "GRÁTIS"
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
    "execu??o" = "execução"
    "solu??o" = "solução"
    "verifica??o" = "verificação"
    "corre??o" = "correção"
    "instala??o" = "instalação"
    "RECOMENDA??O" = "RECOMENDAÇÃO"
    "IMPLEMENTA??O" = "IMPLEMENTAÇÃO"
    "CORRE??ES" = "CORREÇÕES"
    "substitui??es" = "substituições"
    "substitu??es" = "substituições"
    "modifica??o" = "modificação"
    "descri??o" = "descrição"
    "fun??o" = "função"
    "vers?o" = "versão"
    "informa??o" = "informação"
    "navega??o" = "navegação"
    "integra??o" = "integração"
    "migra??o" = "migração"
    "autentica??o" = "autenticação"
    "autoriza??o" = "autorização"
    "codifica??o" = "codificação"
    "tradi??o" = "tradição"
    "preven??o" = "prevenção"
    
    # Padrao 2: Interrogacao (?)
    "S?nior" = "Sénior"
    "Vulner?veis" = "Vulneráveis"
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
    
    # Padrao 3: Dupla interrogacao (??)
    "Viola??es" = "Violações"
    "Encripta??o" = "Encriptação"
}

# Contador de ficheiros processados
$processedCount = 0
$successCount = 0
$errorCount = 0

Write-Host "`n=== INICIANDO CORRECAO DE ENCODING UTF-8 ===" -ForegroundColor Cyan
Write-Host "Ficheiros a processar: $($files.Count)`n" -ForegroundColor White

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "[$(($processedCount + 1))/$($files.Count)] Processando: $file" -ForegroundColor Yellow
        
        try {
            # Leitura com deteccao automatica de encoding usando StreamReader
            $content = $null
            
            # Tentar UTF-8 primeiro
            try {
                $content = Get-Content -Path $file -Raw -Encoding UTF8 -ErrorAction Stop
            }
            catch {
                # Fallback: usar StreamReader com auto-deteccao
                $streamReader = New-Object System.IO.StreamReader($file, $true)
                $content = $streamReader.ReadToEnd()
                $streamReader.Close()
                Write-Host "  ! Encoding detectado automaticamente" -ForegroundColor DarkYellow
            }
            
            if ($null -eq $content) {
                throw "Falha ao ler ficheiro"
            }
            
            # Aplicar todas as substituicoes
            $replacementsMade = 0
            foreach ($key in $replacements.Keys) {
                $oldContent = $content
                $content = $content -replace [regex]::Escape($key), $replacements[$key]
                if ($content -ne $oldContent) {
                    $replacementsMade++
                }
            }
            
            # Guardar com UTF-8 sem BOM (padrao para Markdown)
            $utf8NoBom = New-Object System.Text.UTF8Encoding $false
            [System.IO.File]::WriteAllText($file, $content, $utf8NoBom)
            
            if ($replacementsMade -gt 0) {
                Write-Host "  + Corrigido: $file ($replacementsMade substituicoes)" -ForegroundColor Green
            } else {
                Write-Host "  = OK: $file (sem correcoes necessarias)" -ForegroundColor Gray
            }
            
            $successCount++
        }
        catch {
            Write-Host "  X ERRO: $file - $($_.Exception.Message)" -ForegroundColor Red
            $errorCount++
        }
        
        $processedCount++
    } else {
        Write-Host "  - Ficheiro nao encontrado: $file" -ForegroundColor DarkGray
        $processedCount++
    }
}

# Sumario final
Write-Host "`n=== SUMARIO DA CORRECAO ===" -ForegroundColor Cyan
Write-Host "Total processados:  $processedCount" -ForegroundColor White
Write-Host "Sucesso:            $successCount" -ForegroundColor Green
Write-Host "Erros:              $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Gray" })
Write-Host "Nao encontrados:    $($files.Count - $processedCount)" -ForegroundColor DarkGray

if ($successCount -gt 0) {
    Write-Host "`n+ Encoding UTF-8 corrigido com sucesso!" -ForegroundColor Green
} else {
    Write-Host "`n! Nenhum ficheiro foi corrigido." -ForegroundColor Yellow
}

Write-Host "`nProximos passos:" -ForegroundColor Cyan
Write-Host "1. Verificar os ficheiros corrigidos" -ForegroundColor White
Write-Host "2. git add ." -ForegroundColor White
Write-Host "3. git commit -m 'fix: Corrigir encoding UTF-8 em ficheiros Markdown e PowerShell'" -ForegroundColor White
Write-Host "4. git push origin S3`n" -ForegroundColor White
