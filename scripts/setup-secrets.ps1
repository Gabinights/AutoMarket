#!/usr/bin/env pwsh

Write-Host "?? Configurando User Secrets para AutoMarket..." -ForegroundColor Cyan

# Verificar se User Secrets foi inicializado
$secretsPath = $env:APPDATA + "\Microsoft\UserSecrets"
if (-not (Test-Path $secretsPath)) {
    Write-Host "Inicializando User Secrets..." -ForegroundColor Yellow
    dotnet user-secrets init
}

# Encryption Key
Write-Host "`n?? Configurando Encryption Key..." -ForegroundColor Green
$encKey = Read-Host "Encryption Key (pressione Enter para usar padrão dev)" 
if ([string]::IsNullOrEmpty($encKey)) {
    $encKey = "dev-key-encriptacao-secreta-minimo-32-caracteres"
}
dotnet user-secrets set "Encryption:Key" $encKey
Write-Host "? Encryption Key configurado" -ForegroundColor Green

# Database
Write-Host "`n?? Configurando Database..." -ForegroundColor Green
$dbConnection = "Server=(localdb)\mssqllocaldb;Database=automarket_dev;Integrated Security=true;TrustServerCertificate=true;"
Write-Host "Usando LocalDB padrão: $dbConnection" -ForegroundColor Gray
dotnet user-secrets set "ConnectionStrings:DefaultConnection" $dbConnection
Write-Host "? Database configurado" -ForegroundColor Green

# Email
Write-Host "`n?? Configurando Email..." -ForegroundColor Green
$smtpServer = Read-Host "SMTP Server (padrão: smtp.gmail.com)"
if ([string]::IsNullOrEmpty($smtpServer)) { $smtpServer = "smtp.gmail.com" }

$smtpUser = Read-Host "SMTP Username (seu email)"
$smtpPass = Read-Host "SMTP Password (app password)" -AsSecureString
$smtpPassText = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($smtpPass))

$fromEmail = Read-Host "From Email (padrão: $smtpUser)"
if ([string]::IsNullOrEmpty($fromEmail)) { $fromEmail = $smtpUser }

dotnet user-secrets set "EmailSettings:SmtpServer" $smtpServer
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUsername" $smtpUser
dotnet user-secrets set "EmailSettings:SmtpPassword" $smtpPassText
dotnet user-secrets set "EmailSettings:FromEmail" $fromEmail
dotnet user-secrets set "EmailSettings:FromName" "AutoMarket Dev"
Write-Host "? Email configurado" -ForegroundColor Green

# Admin
Write-Host "`n?? Configurando Admin..." -ForegroundColor Green
$adminEmail = Read-Host "Admin Email (padrão: admin-dev@localhost)"
if ([string]::IsNullOrEmpty($adminEmail)) { $adminEmail = "admin-dev@localhost" }

$adminPass = Read-Host "Admin Password (padrão: DevPassword@123456)" -AsSecureString
$adminPassText = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($adminPass))
if ([string]::IsNullOrEmpty($adminPassText)) { $adminPassText = "DevPassword@123456" }

dotnet user-secrets set "DefaultAdmin:Email" $adminEmail
dotnet user-secrets set "DefaultAdmin:Password" $adminPassText
Write-Host "? Admin configurado" -ForegroundColor Green

# Verificar
Write-Host "`n? Todos os secrets foram configurados!" -ForegroundColor Green
Write-Host "`n?? Secrets carregados:" -ForegroundColor Cyan
dotnet user-secrets list

Write-Host "`n? Setup completo! Execute 'dotnet run' para iniciar a aplicação." -ForegroundColor Green
