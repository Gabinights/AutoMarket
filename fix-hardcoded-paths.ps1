# Script para remover paths hardcoded de ficheiros de documentação
# AutoMarket - Remove Hardcoded Paths

$files = @{
    "SETUP-MAILGUN.md" = 'C:\Users\nunos\Source\Repos\AutoMarket2'
    "SETUP-MAILTRAP.md" = 'C:\Users\nunos\Source\Repos\AutoMarket2'
    "SETUP-SENDGRID.md" = 'C:\Users\nunos\Source\Repos\AutoMarket2'
}

$replacementText = '<path-to-your-project>'

foreach ($file in $files.Keys) {
    if (Test-Path $file) {
        Write-Host "Corrigindo: $file" -ForegroundColor Cyan
        
        # Ler conteúdo
        $content = Get-Content -Path $file -Raw -Encoding UTF8
        
        # Substituir path hardcoded
        $oldPath = $files[$file]
        $content = $content -replace [regex]::Escape($oldPath), $replacementText
        
        # Adicionar nota se ainda não existir
        if ($content -notmatch "Nota:.*navegue até ao diretório") {
            $content = $content -replace '(cd\s+"[^"]+"|cd\s+<path-to-your-project>)', "`$1`r`n`r`n**Nota:** Navegue até ao diretório onde clonou o projeto AutoMarket."
        }
        
        # Guardar com UTF-8
        [System.IO.File]::WriteAllText($file, $content, [System.Text.Encoding]::UTF8)
        
        Write-Host "? Corrigido: $file" -ForegroundColor Green
    } else {
        Write-Host "? Ficheiro não encontrado: $file" -ForegroundColor Red
    }
}

Write-Host "`n? Paths hardcoded removidos de todos os ficheiros!" -ForegroundColor Green
