param(
    [string]$MosquittoDirectory = 'C:\Program Files\mosquitto'
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$nativeDirectory = Join-Path $projectRoot 'deploy\native'
$passwordTool = Join-Path $MosquittoDirectory 'mosquitto_passwd.exe'
if (-not (Test-Path -LiteralPath $passwordTool)) {
    throw "Mosquitto was not found at $MosquittoDirectory"
}

New-Item -ItemType Directory -Force -Path $nativeDirectory | Out-Null
$configFile = Join-Path $nativeDirectory 'mosquitto.conf'
$envFile = Join-Path $nativeDirectory '.env'
if ((Test-Path -LiteralPath $configFile) -and (Test-Path -LiteralPath $envFile)) {
    Write-Host "Local MQTT is already configured at $nativeDirectory"
    return
}

$random = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$passwordBytes = New-Object byte[] 24
$random.GetBytes($passwordBytes)
$random.Dispose()
$password = [BitConverter]::ToString($passwordBytes).Replace('-', '')
$user = 'smartlivestock_demo'
$passwordFile = Join-Path $nativeDirectory ('passwords-' + [Guid]::NewGuid().ToString('N'))
& $passwordTool -b -c $passwordFile $user $password
if ($LASTEXITCODE -ne 0) { throw 'mosquitto_passwd failed' }

$passwordPath = $passwordFile.Replace('\', '/')
@"
listener 1884 127.0.0.1
allow_anonymous false
password_file $passwordPath
persistence false
log_dest stdout
"@ | Set-Content -LiteralPath $configFile -Encoding ascii

@"
MQTT_USER=$user
MQTT_PASSWORD=$password
"@ | Set-Content -LiteralPath $envFile -Encoding ascii

Write-Host "Local MQTT configured at $nativeDirectory"
Write-Host 'Credentials were saved to the ignored deploy/native/.env file.'
