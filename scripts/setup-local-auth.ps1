param(
    [Parameter(Mandatory = $true)][string]$AdminEmail,
    [Parameter(Mandatory = $true)][SecureString]$AdminPassword
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$directory = Join-Path $root 'deploy/native'
$path = Join-Path $directory 'auth.env'
$password = [System.Net.NetworkCredential]::new('', $AdminPassword).Password

if ($password.Length -lt 12 -or $password.Length -gt 128) {
    throw 'Admin password must contain 12-128 characters.'
}
if ($password -cnotmatch '[A-Z]') { throw 'Admin password must contain an uppercase letter (A-Z).' }
if ($password -cnotmatch '[a-z]') { throw 'Admin password must contain a lowercase letter (a-z).' }
if ($password -notmatch '[0-9]') { throw 'Admin password must contain a digit (0-9).' }
if ($password -notmatch '[^\p{L}\p{N}]') { throw 'Admin password must contain a special character.' }
try {
    $address = [System.Net.Mail.MailAddress]::new($AdminEmail.Trim())
    if ($address.Address -ne $AdminEmail.Trim()) { throw 'Invalid address' }
} catch { throw 'Admin email is invalid.' }

if (Test-Path -LiteralPath $path) {
    $lines = @(Get-Content -LiteralPath $path)
    if (-not ($lines | Where-Object { $_ -match '^ADMIN_PASSWORD=' })) {
        throw 'Admin bootstrap is already complete. This script cannot reset an existing admin password.'
    }
    $keyLine = $lines | Where-Object { $_.TrimStart([char]0xFEFF) -match '^AUTH_API_KEY=' } | Select-Object -First 1
    if (-not $keyLine) { throw 'Existing auth.env has no API key.' }
    $key = ($keyLine.TrimStart([char]0xFEFF) -split '=', 2)[1]
    if ($key -notmatch '^[A-Fa-f0-9]{64}$') { throw 'Existing auth.env has an invalid API key.' }
} else {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    $rng.Dispose()
    $key = [BitConverter]::ToString($bytes).Replace('-', '')
}
@(
    "AUTH_API_KEY=$key",
    "ADMIN_EMAIL=$AdminEmail",
    "ADMIN_PASSWORD=$password"
) | Set-Content -LiteralPath $path -Encoding utf8
Write-Host 'Local authentication configuration saved. Keep deploy/native/auth.env private.'
