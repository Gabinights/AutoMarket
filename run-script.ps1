# Extensões que queremos processar
$extensions = @("*.cs", "*.cshtml", "*.js", "*.css", "*.json", "*.html")

# Obter todos os ficheiros recursivamente
$files = Get-ChildItem -Path . -Recurse -Include $extensions -File | Where-Object { 
    $_.FullName -notmatch "\\bin\\" -and 
    $_.FullName -notmatch "\\obj\\" -and 
    $_.FullName -notmatch "\\.git\\" -and
    $_.FullName -notmatch "\\lib\\" # Ignorar bibliotecas externas se existirem
}

# Definir o encoding UTF-8 com BOM
$utf8WithBom = New-Object System.Text.UTF8Encoding $true

foreach ($file in $files) {
    # Ler o conteúdo atual
    $content = [System.IO.File]::ReadAllText($file.FullName)
    
    # Escrever de volta forçando o BOM
    [System.IO.File]::WriteAllText($file.FullName, $content, $utf8WithBom)
    
    Write-Host "Processado: $($file.Name)" -ForegroundColor Green
}

Write-Host "Concluído! Todos os ficheiros estão agora em UTF-8 com BOM." -ForegroundColor Yellow
