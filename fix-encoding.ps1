# Script para corrigir encoding UTF-8 em ficheiros com caracteres corruptos
# AutoMarket - Fix Encoding Issues

$files = @(
    "Views/Conta/AccessDenied.cshtml",
    "Views/Conta/AlterarPassword.cshtml",
    "Views/Conta/Perfil.cshtml",
    "Views/Transacoes/MinhasVendas.cshtml"
)

$replacements = @{
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
}

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Corrigindo: $file" -ForegroundColor Cyan
        
        # Ler conteúdo
        $content = Get-Content -Path $file -Raw -Encoding Default
        
        # Aplicar substituições
        foreach ($key in $replacements.Keys) {
            $content = $content -replace [regex]::Escape($key), $replacements[$key]
        }
        
        # Guardar com UTF-8 (sem BOM para .cshtml)
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($file, $content, $utf8NoBom)
        
        Write-Host "? Corrigido: $file" -ForegroundColor Green
    } else {
        Write-Host "? Ficheiro não encontrado: $file" -ForegroundColor Red
    }
}

Write-Host "`n? Encoding UTF-8 corrigido em todos os ficheiros!" -ForegroundColor Green
