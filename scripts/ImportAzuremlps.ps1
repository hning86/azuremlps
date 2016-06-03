function DownloadFile([string] $uri, [string] $output)
{
    $wc = New-Object System.Net.WebClient
    $wc.DownloadFile($uri, $output)
}

function UnzipFile([string] $file, [string] $output)
{
if (Test-Path $output) {
  return
  }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($file, $output)
}

function Import-AzureMlps()
{
[CmdletBinding()] 

    $modPath = "$env:UserProfile\documents\WindowsPowerShell\Modules"
    if ( !(Test-Path $modPath) ) {
        New-Item -Path $modPath -ItemType Directory | Out-Null
        }

    $zipFile = $modPath + "\azuremlps.zip"
    if ( !(Test-Path $zipFile)) {
        DownloadFile 'https://github.com/hning86/azuremlps/releases/download/0.2.5/AzureMLPS.zip' $zipFile
    }

    UnzipFile $zipFile ($modPath + "\azuremlps")



    Import-Module azuremlps -WarningAction SilentlyContinue
}
