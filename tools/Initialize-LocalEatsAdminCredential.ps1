param(
    [string]$ApiProject = (Join-Path $PSScriptRoot '..\src\Restaurants.API\Restaurants.API.csproj'),
    [string]$CredentialPath = (Join-Path $env:LOCALAPPDATA 'LocalEats\AdminCredential.dpapi'),
    [switch]$Reset
)

$ErrorActionPreference = 'Stop'
$apiProjectPath = [System.IO.Path]::GetFullPath($ApiProject)
$resolvedPath = [System.IO.Path]::GetFullPath($CredentialPath)

if (-not (Test-Path -LiteralPath $apiProjectPath)) {
    throw "API project was not found at '$apiProjectPath'."
}
if ((Test-Path -LiteralPath $resolvedPath) -and -not $Reset) {
    throw "Credential already exists at '$resolvedPath'. Use -Reset only after the old credential has been revoked."
}

[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($resolvedPath)) | Out-Null
$randomBytes = [byte[]]::new(32)
$generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $generator.GetBytes($randomBytes)
}
finally {
    $generator.Dispose()
}

$credential = [Convert]::ToBase64String($randomBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
$secureCredential = ConvertTo-SecureString -String $credential -AsPlainText -Force
$encryptedCredential = ConvertFrom-SecureString -SecureString $secureCredential
[System.IO.File]::WriteAllText($resolvedPath, $encryptedCredential)

$sha256 = [System.Security.Cryptography.SHA256]::Create()
try {
    $hashBytes = $sha256.ComputeHash([Text.Encoding]::UTF8.GetBytes($credential))
    $credentialHash = [BitConverter]::ToString($hashBytes).Replace('-', '')
}
finally {
    $sha256.Dispose()
}

dotnet user-secrets set 'Admin:DestructiveCommandCredentialHash' $credentialHash --project $apiProjectPath
if ($LASTEXITCODE -ne 0) {
    Remove-Item -LiteralPath $resolvedPath -Force -ErrorAction SilentlyContinue
    throw 'Could not save the credential hash to the API User Secrets store.'
}

Write-Host "Laptop-bound credential saved to $resolvedPath"
Write-Host 'The API User Secrets store contains only its SHA-256 hash.'
Write-Host 'Copy this hash into Google Secret Manager from Cloud Shell; never paste the decrypted credential into a terminal, cloud secret, or chat.'
Write-Output $credentialHash

$credential = $null
$secureCredential = $null
$randomBytes = $null
$hashBytes = $null
$credentialHash = $null
