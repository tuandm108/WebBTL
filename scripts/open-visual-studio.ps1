param(
    [string]$MosquittoDirectory = 'C:\Program Files\mosquitto'
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$envFile = Join-Path $projectRoot 'deploy\native\.env'

if (-not (Test-Path -LiteralPath $envFile)) {
    & (Join-Path $PSScriptRoot 'setup-local-mqtt.ps1') -MosquittoDirectory $MosquittoDirectory
}

$mqttVars = ConvertFrom-StringData (Get-Content -LiteralPath $envFile -Raw)
if ([string]::IsNullOrWhiteSpace($mqttVars.MQTT_USER) -or
    [string]::IsNullOrWhiteSpace($mqttVars.MQTT_PASSWORD)) {
    throw "MQTT credentials are missing from $envFile"
}

$runningVisualStudio = Get-Process devenv -ErrorAction SilentlyContinue
if ($runningVisualStudio) {
    throw 'Close all Visual Studio windows, then run this script again so the new process inherits the local MQTT credentials.'
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw 'Visual Studio was not found. Install Visual Studio with ASP.NET and web development support.'
}
$visualStudio = & $vswhere -latest -products * -property productPath
if (-not $visualStudio -or -not (Test-Path -LiteralPath $visualStudio)) {
    throw 'Visual Studio IDE (devenv.exe) was not found.'
}

$env:Mqtt__User = $mqttVars.MQTT_USER
$env:Mqtt__Password = $mqttVars.MQTT_PASSWORD
$env:MQTT_USER = $mqttVars.MQTT_USER
$env:MQTT_PASSWORD = $mqttVars.MQTT_PASSWORD
$env:MQTT_PORT = '1884'

$solution = Join-Path $projectRoot 'SmartLivestock.sln'
Start-Process -FilePath $visualStudio -ArgumentList ('"' + $solution + '"') -WorkingDirectory $projectRoot -WindowStyle Normal
Write-Host "Opened $solution with local MQTT settings."
