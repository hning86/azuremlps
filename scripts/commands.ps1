# Get-AzurePublishSettingsFile

Import-AzurePublishSettingsFile -PublishSettingsFile '~\Downloads\VoD - Dev-5-20-2016-credentials.publishsettings'

# import-module C:\Repos\azuremlps\code\amlps\bin\Debug\AzureMLPS.dll

#copy dll
copy C:\Repos\azuremlps\code\amlps\bin\Debug\*.dll .
import-module ./AzureMLPS.dll


$cert = (dir Cert:\CurrentUser\My | ? { $_.FriendlyName -match "VoD" })

$thumb = $cert.Thumbprint

$sub = Get-AzureSubscription | ? { $_.SubscriptionName -match "VoD" }

$subid = $sub.SubscriptionId

Select-AzureSubscription -SubscriptionId $subid

$store = Get-AzureStorageAccount -StorageAccountName verbatimviewerdev
$store | Get-AzureStorageContainer 

$key = Get-AzureStorageKey -StorageAccountName verbatimviewerdev

$m = @{ AzureSubscriptionId = $subid; ManagementCertThumbprint = $thumb; }
$ws = List-AmlWorkspaces -AzureSubscriptionId $subid -ManagementCertThumbprint $thumb | ? { $_.Name -match "verbatim" }
#$ws = List-AmlWorkspaces @m | ? { $_.Name -match "voxdev" }

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
$mws = Get-AmlTrainedModel @a
$wws = Get-AmlWebService @a
