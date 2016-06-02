function DownloadFile([string] $uri, [string] $output)
{
    $wc = New-Object System.Net.WebClient
    $wc.DownloadFile($uri, $output)
}

function Import-AzureMlps()
{
[CmdletBinding()] 

    $modPath = "$env:UserProfile\documents\WindowsPowerShell\Modules"
    if ( !(Test-Path $modPath) ) {
        New-Item -Path $modPath -ItemType Directory | Out-Null
        }

    $mlpsPath = $modPath + "\azuremlps"
    if ( !(Test-Path $mlpsPath)) {
        Push-Location $modPath 
        DownloadFile 'https://github.com/hning86/azuremlps/releases/download/0.2.5/AzureMLPS.zip' $modPath/AzureMLPS.zip
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory('.\azuremlps.zip', '.')
        Pop-Location
    }

    Import-Module azuremlps
}

Import-AzureMlps
