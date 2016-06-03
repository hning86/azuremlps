. $PSScriptRoot\ImportAzuremlps.ps1
. $PSScriptRoot\UsePublisherSettings.ps1

Import-AzureMlps
$settings = Use-PublisherSettings

#$store = Get-AzureStorageAccount -StorageAccountName verbatimviewerdev
#$store | Get-AzureStorageContainer 
#$key = Get-AzureStorageKey -StorageAccountName verbatimviewerdev

$m = @{ AzureSubscriptionId = $settings.Id; ManagementCertThumbprint = $settings.Account; }

$list = List-AmlWorkspaces @m

#$ws = List-AmlWorkspaces @m | ? { $_.Name -match "voxdev" }

$ws = List-AmlWorkspaces @m | ? { $_.Name -eq 'verbatimviewerdev' }

$a = @{
    WorkspaceId = $ws.Id;
    Location = $ws.Region;
    AuthorizationToken = $ws.AuthorizationToken.PrimaryToken 
}

# $vws = Get-AmlWorkspace -WorkspaceId $ws.Id -Location $ws.Region -AuthorizationToken $ws.AuthorizationToken.PrimaryToken

# $ews = Get-AmlExperiment -WorkspaceId $ws.Id -Location $ws.Region -AuthorizationToken $ws.AuthorizationToken.PrimaryToken

# $dws = Get-AmlDataset -WorkspaceId $ws.Id -Location $ws.Region -AuthorizationToken $ws.AuthorizationToken.PrimaryToken

# $mws = Get-AmlTrainedModel -WorkspaceId $ws.Id -Location $ws.Region -AuthorizationToken $ws.AuthorizationToken.PrimaryToken

$aws = Get-AmlWorkspace @a
$ews = Get-AmlExperiment @a
$dws = Get-AmlDataset @a
#$mws = Get-AmlTrainedModel @a
$wws = Get-AmlWebService @a
$trainWs = $wws | ? { $_.Name -eq 'vox train experiment' }
$trainEps = Get-AmlWebServiceEndpoint @a -WebServiceId $trainWs.Id
