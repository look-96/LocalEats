param(
    [Parameter(Mandatory)]
    [uri]$ApiBaseUrl,
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$ApiPath,
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$JsonBody,
    [string]$CredentialPath = (Join-Path $env:LOCALAPPDATA 'LocalEats\AdminCredential.dpapi')
)

$ErrorActionPreference = 'Stop'
if (-not $ApiBaseUrl.IsAbsoluteUri -or $ApiBaseUrl.Scheme -ne 'https') {
    throw 'ApiBaseUrl must be an absolute HTTPS URL.'
}
if (-not $ApiPath.StartsWith('/')) {
    throw 'ApiPath must start with /.'
}

$resolvedPath = [System.IO.Path]::GetFullPath($CredentialPath)
if (-not (Test-Path -LiteralPath $resolvedPath)) {
    throw 'The LocalEats admin credential file was not found on this Windows account.'
}

$secureCredential = ConvertTo-SecureString ([System.IO.File]::ReadAllText($resolvedPath))
$credential = [System.Net.NetworkCredential]::new('', $secureCredential).Password
$email = Read-Host 'LocalEats administrator email'
$passwordSecure = Read-Host 'LocalEats administrator password' -AsSecureString
$password = [System.Net.NetworkCredential]::new('', $passwordSecure).Password
$baseUrl = $ApiBaseUrl.AbsoluteUri.TrimEnd('/')
$headers = $null

try {
    Write-Host "The administrator token and laptop credential will be sent to HTTPS host '$($ApiBaseUrl.Host)'."
    $hostConfirmation = Read-Host 'Type that exact host to continue'
    if ($hostConfirmation -cne $ApiBaseUrl.Host) {
        Write-Host 'Cancelled. No request was sent.'
        return
    }

    $loginBody = @{ email = $email; password = $password } | ConvertTo-Json
    $login = Invoke-RestMethod -Method Post -Uri "$baseUrl/api/identity/login?useCookies=false" -ContentType 'application/json' -Body $loginBody
    $accessToken = $login.accessToken
    if ([string]::IsNullOrWhiteSpace($accessToken)) {
        throw 'The API login did not return an access token.'
    }

    $confirmation = Read-Host 'This is a destructive admin request. Type DELETE to continue'
    if ($confirmation -cne 'DELETE') {
        Write-Host 'Cancelled. No request was sent.'
        return
    }

    $headers = @{
        Authorization = "Bearer $accessToken"
        'X-LocalEats-Admin-Key' = $credential
    }
    $target = [uri]::new($ApiBaseUrl, $ApiPath)
    Invoke-RestMethod -Method Delete -Uri $target -Headers $headers -ContentType 'application/json' -Body $JsonBody
    Write-Host 'Destructive request completed.'
}
finally {
    $password = $null
    $loginBody = $null
    $credential = $null
    $accessToken = $null
    if ($headers) { $headers.Clear() }
}
