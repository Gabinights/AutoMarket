# Script para corrigir encoding UTF-8 - usando Unicode hex codes
# AutoMarket - Solucao Definitiva

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

# Funcao para converter caracteres especiais
function Fix-PortugueseChars {
    param([string]$text)
    
    # Padrao 1: Substituir caracteres corruptos comuns
    $text = $text -replace 'S.nior', ([char]0x53 + [char]0xE9 + 'nior')  # Senior
    $text = $text -replace 'SUM.RIO', ([char]0x53 + [char]0x55 + [char]0x4D + [char]0xC1 + [char]0x52 + [char]0x49 + [char]0x4F)  # SUMARIO
    $text = $text -replace 'SEGURAN.A', ('SEGURAN' + [char]0xC7 + 'A')  # SEGURANCA
    $text = $text -replace 'CR.TICO', ([char]0x43 + [char]0x52 + [char]0xCD + [char]0x54 + [char]0x49 + [char]0x43 + [char]0x4F)  # CRITICO
    
    # Palavras comuns
    $text = $text -replace 'n.o([^a-z])', ('n' + [char]0xE3 + 'o$1')  # nao
    $text = $text -replace 'aprova..o', ('aprova' + [char]0xE7 + [char]0xE3 + 'o')  # aprovacao
    $text = $text -replace 'obrigat.ri([oa])', ('obrigat' + [char]0xF3 + 'ri$1')  # obrigatorio/a - FIX: added capture group
    $text = $text -replace 'confirma..o', ('confirma' + [char]0xE7 + [char]0xE3 + 'o')  # confirmacao
    $text = $text -replace 'M.nimo', ([char]0x4D + [char]0xED + 'nimo')  # Minimo
    $text = $text -replace 'mai.scula', ('mai' + [char]0xFA + 'scula')  # maiuscula
    $text = $text -replace 'Seguran.a', ('Seguran' + [char]0xE7 + 'a')  # Seguranca
    $text = $text -replace 'car.cter', ('car' + [char]0xE1 + 'cter')  # caracter
    $text = $text -replace 'prefer.ncias', ('prefer' + [char]0xEA + 'ncias')  # preferencias
    $text = $text -replace 'Ve.culos', ('Ve' + [char]0xED + 'culos')  # Veiculos
    $text = $text -replace 'Altera..es', ('Altera' + [char]0xE7 + [char]0xF5 + 'es')  # Alteracoes
    $text = $text -replace 'Hist.rico', ('Hist' + [char]0xF3 + 'rico')  # Historico
    $text = $text -replace 'transa..es', ('transa' + [char]0xE7 + [char]0xF5 + 'es')  # transacoes
    $text = $text -replace 'Sess.o', ('Sess' + [char]0xE3 + 'o')  # Sessao
    $text = $text -replace 'permiss.es', ('permiss' + [char]0xF5 + 'es')  # permissoes
    $text = $text -replace 'P.gina', ([char]0x50 + [char]0xE1 + 'gina')  # Pagina
    $text = $text -replace 'valida..o', ('valida' + [char]0xE7 + [char]0xE3 + 'o')  # validacao
    $text = $text -replace '.ltimo', ([char]0xFA + 'ltimo')  # ultimo
    $text = $text -replace 'Estat.sticas', ('Estat' + [char]0xED + 'sticas')  # Estatisticas
    $text = $text -replace 'dispon.vel', ('dispon' + [char]0xED + 'vel')  # disponivel
    
    return $text
}

$processedCount = 0
$successCount = 0

Write-Host "`n=== CORRIGINDO ENCODING UTF-8 ===" -ForegroundColor Cyan

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Processando: $file" -ForegroundColor Yellow
        
        try {
            # Ler ficheiro com encoding Windows-1252 (CP1252) explicitamente
            # Evita problemas de encoding system-dependent
            $encoding1252 = [System.Text.Encoding]::GetEncoding(1252)
            $content = [System.IO.File]::ReadAllText($file, $encoding1252)
            
            # Aplicar correcoes
            $fixedContent = Fix-PortugueseChars -text $content
            
            # Guardar como UTF-8 sem BOM
            $utf8NoBom = New-Object System.Text.UTF8Encoding $false
            [System.IO.File]::WriteAllText($file, $fixedContent, $utf8NoBom)
            
            Write-Host "  OK: $file" -ForegroundColor Green
            $successCount++
        }
        catch {
            Write-Host "  ERRO: $file - $($_.Exception.Message)" -ForegroundColor Red
        }
        
        $processedCount++
    }
}

Write-Host "`nProcessados: $successCount/$processedCount" -ForegroundColor Cyan
