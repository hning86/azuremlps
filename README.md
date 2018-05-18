# PowerShell Module for Azure Machine Learning Studio & Web Services Beta v.0.3.7 
## Introduction
This is a preview release of PowerShell Commandlet Library for [Azure Machine Learning](https://studio.azureml.net). It allows you to interact with Azure Machine Learning Workspace, or Workspace for short, Datasets, Trained Models, Transforms, Custom Modules, Experiments, Web Services and Web Service Endpoints. The supported operations are:

* __Manage Workspace__
  * DEPRECATED: Create new Workspace using a management certificate (*[New-AmlWorkspace](#new-amlworkspace)*)
  * DEPRECATED: List all Workspaces in an Azure subscription (*[List-AmlWorkspaces](#list-amlworkspaces)*)
  * DEPRECATED: Delete a Workspace (*[Remove-AmlWorkspace](#remove-amlworkspace)*)
  * Add users to a Workspace (*[Add-AmlWorkspaceUsers](#add-amlworkspaceusers)*)
  * Get users of a Workspace (*[Get-AmlWorkspaceUsers](#get-amlworkspaceusers)*)
  * Get the metadata of a Workspace (*[Get-AmlWorkspace](#get-amlworkspace)*)  
* __Manage User Assets (Dataset, Trained Model, Transform)__
  * List all Datasets in an Experiment or Workspace (*[Get-AmlDataset](#get-amldataset)*)
  * Promote a Dataset from an Experiment into Workspace (*[Promote-AmlDataset](#promote-amldataset)*)
  * Download a Dataset file from Workspace to local file directory (*[Download-AmlDataset](#download-amldataset)*)
  * Upload a Dataset file from local file directory to Workspace (*[Upload-AmlDataset](#upload-amldataset)*)
  * Delete a Dataset file in Workspace (*[Remove-AmlDataset](#remove-amldataset)*)
  * List all Trained Models in an Experiment or Workspace (*[Get-AmlTrainedModel](#get-amltrainedmodel)*)
  * Promote a Trained Model from an Experiment into Workspace (*[Promote-AmlTrainedModel](#promote-amltrainedmodel)*)
  * List all Transforms in an Experiment or Workspace (*[Get-AmlTransform](#get-amltransform)*)
  * Promote a Transform from an Experiment into Workspace (*[Promote-AmlTransform](#promote-amltransform)*)
* __Manage Custom Module__
  * Add a new custom module to Workspace (*[New-AmlCustomModule](#new-amlcustommodule)*)
  * List all modules (*[Get-AmlModule](#get-amlmodule)*)
  * Update modules within an experiment (*[Update-AmlExperimentModule](#update-amlexperimentmodule)*)
* __Manage Experiment__
  * List all Experiments in Workspace (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Get the metadata of a specific Experiment (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Export a specific Experiment graph to a file in JSON format (*[Export-AmlExperimentGraph](#export-amlexperimentgraph)*)
  * Import a JSON file to overwrite an existing Experiment or create a new Experiment (*[Import-AmlExperimentGraph](#import-amlexperimentgraph)*)
  * Run an Experiment (*[Start-AmlExperiment](#start-amlexperiment)*)
  * Delete an Experiment (*[Remove-AmlExperiment](#remove-amlexperiment)*)
  * Copy an Experiment from a Workspace to another Workspace (*[Copy-AmlExperiment](#copy-amlexperiment)*)
  * Copy an Experiment from [Cortana Intellignce Gallery](https://gallery.cortanaintelligence.com/) (*[Copy-AmlExperimentFromGallery](#copy-amlexperimentfromgallery)*)
  * Find a module node in an Experiment using its comment (*[Get-AmlExperimentNode](#get-amlexperimentnode)*)
  * Replace a user asset in an Experiment with another asset from Workspace (*[Replace-AmlExperimentUserAsset](#replace-amlexperimentuserasset)*)
  * Update user assets in an Experiment with the latest version (*[Update-AmlExperimentUserAsset](#update-amlexperimentuserasset)*)
  * Export a Web Service definition file from the Experiment (*[Export-AmlWebServiceDefinitionFromExperiment](#export-amlwebservicedefinitionfromexperiment)*)
  * Download the output of a node in an Experiment (*[Download-AmlExperimentNodeOutput](#download-amlexperimentnodeoutput)*)
* __Manage Classic Web Service__
  * Deploy a classic Web Service from a Predictive Experiment (*[New-AmlWebService](#new-amlwebservice)*)
  * List all classic Web Services in Workspace (*[Get-AmlWebService](#get-amlwebservice)*)
  * Get the attributes of a specific classic Web Service (*[Get-AmlWebService](#get-amlwebservice)*)
  * Delete a classic Web Service (*[Remove-AmlWebService](#remove-amlwebservice)*)
* __Manage Classic Web Service Endpoint__
  * List all Endpoints of a classic Web Service (*[Get-AmlWebServiceEndpoint](#get-amlwebserviceendpoint)*)
  * Get attributes of a specific Endpoint of a classic Web Service (*[Get-AmlWebServiceEndpoint](#get-amlwebserviceendpoint)*)
  * Create a new Endpoint on an existing classic Web Service (*[Add-AmlWebServiceEndpoint](#add-amlwebserviceendpoint)*)
  * Delete a classic Web Service Endpoint (*[Remove-AmlWebServiceEndpoint](#remove-amlwebserviceendpoint)*)
  * Refresh a classic Web Service Endpoint (*[Refresh-AmlWebServiceEndpoint](#refresh-amlwebserviceendpoint)*)
  * Patch a classic Web Service Endpoint (*[Patch-AmlWebServiceEndpoint](#patch-amlwebserviceendpoint)*)
* __Call Azure ML Web Service APIs__
  * Invoke a RRS (Request-Response Service) API (*[Invoke-AmlWebServiceRRSEndpoint](#invoke-amlwebservicerrsendpoint)*)
  * Invoke a BES (Batch Execution Service) API (*[Invoke-AmlWebServiceBESEndpoint](#invoke-amlwebservicebesendpoint)*)
* __List Other User Assets in Workspace__
  * List all Annotations in an Experiment (*[Get-AmlAnnotation](#get-amlannotation)*)
  * List all Notebooks in a Workspace (*[Get-AmlNotebook](#get-amlnotebook)*)
  * Get a Notebook Session (*[Get-AmlNotebookSession](#get-amlnotebooksession)*)
  * List all Data Gateways in a Workspace (*[Get-AmlGateway](#get-amlgateway)*)

## System Requirement
This PowerShell module requires PowerShell 4.0 and .NET 4.5.2. 

Also, you need to have Owner access to at least one Azure Machine Learning Studio Workspace. You can obtain a free Workspace by simply logging in to [Azure Machine Learning Studio](https://studio.azureml.net) with any valid Microsoft account, or your school or work email address. 

For managing Web Service Endpoints, you can also use the API Key created for each endpoint as the authorization token, in lieu of the Workspace Authorization Token.

For more information on Azure Machine Learning, browse the [Azure Machine Learning Homepage](http://www.azure.com/ml). 

## Installation
Simply download the latest _AzureMLPS.zip_ from the [Releases area](https://github.com/hning86/azuremlps/releases), then unzip the file locally. You will find an _AzureMLPS.dll_ file which is a PowerShell module file, and a sample _config.json_ file. Run the PowerShell command _Unblock-File_ then _Import-Module_ to unblock the PowerShell module and then import it into the current PowerShell session:

```powershell
# Unblock the downloaded dll file so Windows can trust it.
Unblock-File .\AzureMLPS.dll
# import the PowerShell module into current session
Import-Module .\AzureMLPS.dll
```
## Configuration
Most of the commandlets require 3 pieces of key information in order to function properly:

* **Workspace ID**
	* This value can be found in Workspace Settings in ML Studio.
	
	![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/WorkspaceId.png)
	
* **Workspace Authorization Token**
	* This value can be found in Workspace Settings in ML Studio. Note you must be an Owner of the Workspace in order to have access to this token.
	
	![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/WorkspaceAuthorizationToken.png)
	
	* Please note for the Web Service Endpoint Management commandlets, you can also use the Endpoint API Key in lieu of the Workspace Authorization Token.
* **Location**
	* This value can be found in the Workspace drop-down. It is the Azure region the Workspace is provisioned in. Currently supported values for this configuration are:
		* South Central US (use this value for all Free Workspaces)
		* Southeast Asia
		* Japan East
		* West Europe	
		* Germany Central
		* West Central US
		
	![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/WorkspaceRegion.png)

There are 3 different ways to specify these values:

1. Create a default _config.json_ file in the same folder where the _AzureMLPS.dll_ file is located. A sample config file is included in the ZIP package so you can simply modify it:

	_config.json_

	```json		
	{
		"WorkspaceId": "d2f62586bed343d621441d55b3872d53",
		"AuthorizationToken": "288a8283e4944bff9c6651a3b6004ef4",
		"Location": "South Central US"
	}	
	```

	
	Then you can simply execute the commandlet like this:
	
	```powershell
	Get-AmlWorkspace
	```
	
2. Or, use the _-ConfigFile_ command parameter to supply the absolute path to a custom config file, using the exact same json format. Please note that relative path will NOT work. It has to be an absolute path. This overrides the default config file if it exists. For example:
	
	```powershell
	Get-AmlWorkspace -ConfigFile 'C:\Configs\MyWorkspace02Config.json'
	```
3. Or, specify values to the _-WorkspaceId_, _-AuthorizationToken_, and _-Location_ parameters directly in the commandlet. The values supplied here override the default and the custom config file. For example:

	```powershell
	Get-AmlWorkspace -WorkspaceId '0123456789abcdef01230123456789ab' -AuthorizationToken 'abcdef0123456789abcdef0123456789' -Location 'South Central US'
	```

For simplicity, the examples below all assume that a valid default config file exists.

## Usage
Remember you can always use _Get-Help_ on any of the following commandlet. For example, to see all supported parameters of _Get-AmlWorkspace_ commandlet, run the following command:

```powershell
Get-Help Get-AmlWorkspace
```

### Manage Workspace

#### New-AmlWorkspace

DEPRECATED: Please note that this commandlet has been deprecated. Use [Azure Resource Manager PowerShell commandlets](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-deploy-with-resource-manager-template/) instead to create a new Workspace.

To create a new Azure ML Workspace, you need to first generate a self-signed certificate, store it in the current user's certificate store, and then upload the public key portion (.cer file) into Azure management portal. The _New-AmlWorkspace_ commandlet will communicate with Azure management API using this certificate to ensure this is an authorized access. Read [more information](https://www.simple-talk.com/cloud/security-and-compliance/windows-azure-management-certificates/) on this subject.

```powershell
$azureSubscriptionId = '<azure_subscription_id>'
$mgmtCertThumb = '12345'
$location = 'South Central US'
$storageAccountName = '<my_storage_account_name>'
$storageAccountKey = '<my_storage_account_key>'
$ownerEmail = 'myname@mycompany.com'
# Create a new Azure ML Worksace named 'ABCD'
New-AmlWorkspace -AzureSubscriptionId $azureSubscriptionId -ManagementCertThumbprint $mgmtCertThumb -WorkspaceName 'ABCD' -Location $location -storageAccountName $storageAccountName -StorageAccountKey $storageAccountKey -OwnerEmail $ownerEmail
```

For quick reference, you can create self-signed certificate using _makecert.exe_, which is a command line tool that comes with Visual Studio and/or Windows SDK. The following command creates the private key in the CurrentUser/My store, and also output the public key in a file that you can upload into Azure management portal. 

```sh
makecert.exe -sky exchange -r -n 'CN=My_Azure_Management_Cert' -pe -a sha1 -len 2048 -ss My 'MyAzureMgmtCert.cer'
```

Or, you can use PowerShell commandlet _New-SelfSignedCertificate_ and _Export-Certificate_

```powershell
# Create the self-signed certificate in CurrentUser\My store.
$cert = New-SelfSignedCertificate -CertStoreLocation cert:\CurrentUser\My -Subject 'CN=My_Azure_Management_Cert' -KeySpec KeyExchange -KeyExportPolicy Exportable -HashAlgorithm SHA1 
# Export the public key as a .cer file
Export-Certificate -Cert $cert -Type CERT -FilePath 'c:\temp\mycert.cer'
```

And here is how you can grab the thumbprint of a particular certificate using PowerShell.

```powershell
(dir Cert:\CurrentUser\My | where Subject -eq 'CN=My_Azure_Management_Cert').Thumbprint
```

#### List-AmlWorkspaces

DEPRECATED: Please note that this commandlet has been deprecated. Use [Azure Resource Manager PowerShell commandlets](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-deploy-with-resource-manager-template/) instead to list existing Workspaces.

Please note that this commandlet can only list Standard Wokspaces. Free Workspaces are not tied to any Azure subscriptions so they will not be listed here. This commandlet requires the presence of the Azure management certificate.

```powershell
List-AmlWorkspace -AzureSubscriptionId '<azure_subscription_id>' -ManagementCertThumbprint '<management_cert_thumbprint>'
```

#### Remove-AmlWorkspace

DEPRECATED: Please note that this commandlet has been deprecated. Use [Azure Resource Manager PowerShell commandlets](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-deploy-with-resource-manager-template/) instead to delete a Workspace.

```powershell
Remove-AmlWorkspace -AzureSubscriptionId '<azure_subscription_id>' -ManagementCertThumbprint '<management_cert_thumbprint>' -WorkspaceId $workspaceId
```


#### Add-AmlWorkspaceUsers

Please note the email addresses are comma-separated. And the supported roles are Owner and User.

```powershell
Add-AmlWorkspaceUsers -Emails 'john@smith.com,jane@doe.com' -Role 'User'
```
This commandlet leverages the config.json file.


#### Get-AmlWorkspaceUsers

```powershell
# Get all users of the current workspace
Get-AmlWorkspaceUsers
```
This commandlet leverages the config.json file.


#### Get-AmlWorkspace

```powershell
# Returns the metadata of a Workspace
$ws = Get-AmlWorkspace
# Display the Workspace Name
$ws.FriendlyName
```
This commandlet leverages the config.json file.

### Manage User Assets (Dataset, Trained Model, Transform)
#### Get-AmlDataset

```powershell
# Get a list of all datasets in an Experiment 'abc':
$exp = Get-AmlExperiment | where Description -eq 'abc'
$ds = Get-AmlDataset -Scope Experiment -ExperimentId $exp.ExperimentId
# Get a list of all datasets in a Workspace:
$ds = Get-AmlDataset -Scope Workspace
# Display the list in a table format with selected properties
$ds | Format-Table Name,DataTypeId,Size,Owner
```
This commandlet leverages the config.json file.

#### Promote-AmlDataset
To use this commandlet, you need to first locate the module in your experiment where the output port produces the Dataset you'd like to promote. So you need to gather the experiment id, node id, and the name of the output port. In order to get the node id, you need to add a unique comment to that dataset-producing module first, and then use the Get-AmlExperimentNode commandlet to grab the node id.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteDataset.png)

Also, if there is already a Dataset of with the same name you supply to this commandlet, you must use -Overwrite parameter, otherwise you will receive a HTTP 409 (Conflict) error.

```powershell
# Find experiment named 'abc' and run it
$exp = Get-AmlExperiment | where Description -eq 'abc'
Start-AmlExperiment -ExperimentId $exp.ExperimentId
# Find the a node in the experiment with a comment 'Split My Data'. In this case it is a Split module
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Split My Data'
# Promote the outcome of one of the left output port of the Split node, and overwrite the previous version.
Promote-AmlDataset -ExperimentId $exp.ExperimentId -ModuleNodeId $node.Id -NodeOutputPortName 'Result dataset1' -DatasetName 'MyData' -DataSetDescription 'My Data' -Overwrite
```
This commandlet leverages the config.json file.

#### Download-AmlDataset
```powershell
# Find a dataset named 'Movie tweets' in the Workspace using Get-AmlDataset:
$dsMT = Get-AmlDataset | where Name -eq 'Movie tweets'
# Download the Movie Tweets dataset:
Download-AmlDataset -DatasetId $dsMT.Id -DownloadFileName 'C:\Temp\MovieTweets.csv'
```
This commandlet leverages the config.json file.

#### Upload-AmlDataset
```powershell
# Upload a local file in .csv format to Workspace
Upload-AmlDataset -FileFormat GenericCSV -UploadFileName 'C:\Temp\MovieTweets.csv' -DatasetName 'Movie Tweets' -Description 'Tweeter data on popular movies'
```
Please note the supported file formats are: 

* GenericCSV
* GenericCSVNoHeader
* GenericTSV
* GenericTSVNoHeader
* ARFF
* Zip
* RData
* PlainText
This commandlet leverages the config.json file.

#### Remove-AmlDataset
```powershell
# Find a dataset named 'Flight Data' in the Workspace using Get-AmlDataset:
$dsFlight = Get-AmlDataset | where Name -eq 'Flight Data'
# Delete the dataset from Workspace
Remove-AmlDataset -DatasetFamilyId $dsFlight.FamilyId
```
This commandlet leverages the config.json file.

#### Get-AmlTrainedModel
```powershell
# Get a list of all Trained Models in an Experiment 'abc':
$exp = Get-AmlExperiment | where Description -eq 'abc'
$trainedModels = Get-AmlTrainedModel -Scope Experiment -ExperimentId $exp.ExperimentId
# Get a list of all Trained Models in a Workspace:
$trainedModels = Get-AmlTrainedModel -Scope Workspace
# Display the list in a table format with selected properties
$trainedModels | Format-Table Name, Id, FamilyId
```
This commandlet leverages the config.json file.

#### Promote-AmlTrainedModel
To use this commandlet, you need to first locate the Train module in your experiment where the output port produces the Trained Model you'd like to promote. So you need to gather the experiment id, node id, and the name of the output port. In order to get the node id, you need to add a unique comment to the Train Model module first, and then use the *Get-AmlExperimentNode* commandlet to grab the node id.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteTrainedModel.png)

Also, if there is already a Trained Model of with the same name you supply to this commandlet, you must use *-Overwrite* parameter, otherwise you will receive a HTTP 409 (Conflict) error.

```powershell
# Find experiment named 'abc' and run it
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Run the experiment at least once so the result is cached.
Start-AmlExperiment -ExperimentId $exp.ExperimentId
# Find the Train Model module node in the experiment with a comment 'Train me'.
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Train me'
# Promote the Trained Model from the output port of the Train Model module, and overwrite the previous version.
Promote-AmlTrainedModel -ExperimentId $exp.ExperimentId -TrainModuleNodeId $node.Id -NodeOutputPortName 'Trained model' -TrainedModelName 'MyModel' -TrainedModelDescription 'My Model' -Overwrite
```
This commandlet leverages the config.json file.

#### Get-AmlTransform
```powershell
# Get a list of all Transforms in an Experiment 'abc':
$exp = Get-AmlExperiment | where Description -eq 'abc'
$transforms = Get-AmlTransform -Scope Experiment -ExperimentId $exp.ExperimentId
# Get a list of all Transforms in a Workspace:
$transforms = Get-AmlTransform -Scope Workspace
# Display the list in a table format with selected properties
$transforms | Format-Table Name,Id, FamilyId
```
This commandlet leverages the config.json file.

#### Promote-AmlTransform
To use this commandlet, you need to first locate the module in your experiment where the output port produces the Transform you'd like to promote. So you need to gather the experiment id, node id, and the name of the output port. In order to get the node id, you need to add a unique comment to the transform-producing module first, and then use the *Get-AmlExperimentNode* commandlet to grab the node id.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteTransform.png)

Also, if there is already a Transform of with the same name you supply to this commandlet, you must use *-Overwrite* parameter, otherwise you will receive a HTTP 409 (Conflict) error.

```powershell
# Find experiment named 'abc' and run it
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Run the experiment at least once so the result is cached.
Start-AmlExperiment -ExperimentId $exp.ExperimentId
# Find the Clean Missing Data module in the experiment where a Clean Transform is produced with a comment 'Clean me'.
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Clean me'
# Promote the Transform from the output port of the transform-producing module, and overwrite the previous version.
Promote-AmlTransform -ExperimentId $exp.ExperimentId -TransformModuleNodeId $node.Id -NodeOutputPortName 'Cleaning transformation' -TransformName 'CleanMe02' -TransformDescription 'Clean Me v2' -Overwrite
```
This commandlet leverages the config.json file.

### Manage Custom Module
#### New-AmlCustomModule
```powershell
# Upload a new Custom Module from C:\Temp\MyModule.zip
New-AmlCustomModule -CustomModuleZipFileName 'C:\Temp\MyModule.zip'
```
This commandlet leverages the config.json file.

#### Get-AmlModule
```powershell
# List all modules
Get-AmlModule
# List custom modules only
Get-AmlModule -Custom
# Get "Add Rows" module
Get-AmlModule | where Name -eq 'Add Rows'
```
This commandlet leverages the config.json file.

#### Update-AmlExperimentModule
This commandlet finds all modules, or modules with a specfic name in an experiment, checks to see if a new version exists in the worspace, and if so, update the experiment graph with the new version. This applies to both system built-in modules, as well as custom modules user creates.

```powershell
# Get the metadata of the Experiment named 'xyz' in the Workspace
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Update all "Add Row" modules within that experiment, provided that a new version of "Add Row" exists in the workspace.
Update-AmlExperimentModule -ExperimentId $exp.ExperimentId -ModuleName 'Add Row'
# Update all modules within that experiment, provided that a new version exists in the workspace.
Update-AmlExperimentModule -ExperimentId $exp.ExperimentId -All
```
This commandlet leverages the config.json file.

### Manage Experiment
#### Get-AmlExperiment 

```powershell
# Get all Experiments in the Workspace
$exps = Get-AmlExperiment
# Display all Experiments in a table format
$exps | Format-Table
```

```powershell
# Get the metadata of the Experiment named 'xyz' in the Workspace
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Display the Experiment status
$exp.Status.StatusCode
```
This commandlet leverages the config.json file.

#### Export-AmlExperimentGraph
```powershell
# Export an Experiment named "xyz" to "MyExp.json"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
Export-AmlExperimentGraph -ExperimentId $exp.ExperimentId -OutputFile 'c:\Temp\MyExp.json'
```
Please note that the exported JSON file only contains references to the exact instance and version of the assets (modules, trained models, datasets, etc.). The assets themselves are NOT serialized into the JSON file. As a consequence, when you import it back into the Workspace, unless you are importing it back into the same Workspace, or unless your graph only contains global assets such as built-in modules, you will not be able to create a valid Experiment. And you might see the Studio UX crashes when attempting to open this invalid experiment. In other words, importing an exported graph that contains user datasets, trained models, custom modules, etc. into a new workspace will not work. Also, please make sure you use absolute path when referring to the json file.

This commandlet leverages the config.json file.

#### Import-AmlExperimentGraph
```powershell
# Import a JSON file 'MyExp.json' to overwrite the Experiment where the file is exported out of
Import-AmlExperimentGraph -InputFile 'C:\Temp\MyExp.json' -Overwrite
# Import a JSON file 'MyExp.json' to create a new Experiment named 'abc'
Import-AmlExperimentGraph -InputFile 'MyExp.json' -NewName 'abc'
```
Please see the above warning in the Export-AmlExperimentGraph documentation about the limitation of importing a JSON graph. 
This commandlet leverages the config.json file.

#### Start-AmlExperiment
```powershell
# Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Run the Experiment
Start-AmlExperiment -ExperimentId $exp.ExperimentId
```
This commandlet leverages the config.json file.

#### Remove-AmlExperiment
```powershell
# Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Delete the Experiment
Remove-AmlExperiment -ExperimentId $exp.ExperimentId
```
This commandlet leverages the config.json file.

#### Copy-AmlExperiment
```powershell
# Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Make a copy of that Experiment in the current Workspace
Copy-AmlExperiment -ExperimentId $exp.ExperimentId -NewExperimentName 'xyz_Copy'
# Copy that Experiment from current Workspace to another Workspace within the same region
Copy-AmlExperiment -ExperimentId $exp.ExperimentId -DestinationWorkspaceId '<ws_id>' -DestinationWorkspaceAuthorizationToken '<auth_token>'
# Copy that Experiment from current Workspace to another Workspace in a different region (such as Southeast Asia)
Copy-AmlExperiment -ExperimentId $exp.ExperimentId -DestinationWorkspaceId '<ws_id>' -DestinationWorkspaceAuthorizationToken '<auth_token>' -DestinationLocation 'Southeast Asia'
```

This commandlet leverages the config.json file.

#### Copy-AmlExperimentFromGallery
This commandlet allows user to clone a published experiment from the [Cortana Intelligence Gallery](https://gallery.cortanaintelligence.com/) into the current Workspace.
```powershell
# These are the relevant parameters for the Gallery experiment https://gallery.cortanaintelligence.com/Experiment/Data-Mining-2016-Presidential-Campaign-Finance-Data-1
$pkgUri = 'https://storage.azureml.net/directories/e842ffe6058e42eb918a6b6abf0a7436/items'
$galleryUri = 'https://gallery.cortanaintelligence.com/Details/data-mining-2016-presidential-campaign-finance-data-1'
$entityId = 'Data-Mining-2016-Presidential-Campaign-Finance-Data-1'
# Clone this Experiment into the current Workspace
Copy-AmlExperimentFromGallery -PackageUri $pkgUri -GalleryUri $galleryUri -EntityId $entityId
```

This commandlet leverages the config.json file.


#### Get-AmlExperimentNode
This commandlet lets you find information, including Id, FamilyId etc. of node(s) with a certain user comment.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteTrainedModel.png)

```powershell
# Find the Experiment named "abc"
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Get the node(s) with a user comment "Train me"
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Train me'
# Display the node
$node
```
This commandlet leverages the config.json file.

#### Download-AmlExperimentNodeOutput
This commandlet lets you download the output payload, or the visualization JSON content, of a module node in an experiment.

```powershell
# Find the Experiment named "abc"
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Get the node(s) with a user comment "Train me"
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Train me'
# Download the output of the Train Model module, which is an ilearner file.
Download-AmlExperimentNodeOutput -ExperimentId $exp.ExperimentId -NodeId $node.Id -OutputPortName 'Trained model' -OutputType Payload -DownloadFileName 'myModel.ilearner'
# Download the visualization content of a Train Model module, which is a json file.
Download-AmlExperimentNodeOutput -ExperimentId $exp.ExperimentId -NodeId $node.Id -OutputPortName 'Trained model' -OutputType Visualization -DownloadFileName 'myModelViz.json'

# Get the node(s) with a user comment "Get CSV"
$node2 = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Get CSV'
# Download the output of an Convert to CSV module, which is a csv file.
Download-AmlExperimentNodeOutput -ExperimentId $exp.ExperimentId -NodeId $node2.Id -OutputPortName 'Results dataset' -DownloadFileName 'myScoredDataset.csv'

```
This commandlet leverages the config.json file.

#### Replace-AmlExperimentUserAsset

This commandlet lets you replace a user asset (Dataset, Trained Model or Transform) in an Experiment with another one from the Workspace. If you simply want to update an asset to the latest version, use *Update-AmlExperimentUserAsset* instead.
```powershell
# Find the Experiment named 'abc'
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Find the transform you want to replace
$oldTransform = Get-AmlTransform -Scope Experiment -ExperimentId $exp.ExperimentId | where Name -eq 'Transform A'
# Find the the Transform in the Workspcae you want to use instead
$newTransform = Get-AmlTransform -Scope Workspace | where Name -eq 'Transform B'
# Replace Transform A with Transform B
Replace-AmlExperimentUserAsset -ExperimentId $exp.ExperimentId -AssetType 'Transform' -ExistingAsset $oldTransform -NewAsset $newTransform
```
This commandlet leverages the config.json file.

#### Update-AmlExperimentUserAsset
This commandlet lets you update user assets (Dataset, Trained Model, Transform) in an Experiment to their latest versions.
```powershell
# Find the Experiment named 'abc'
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Update all user assets (including Datasets, Trained Models and Transforms) in an Experiment
Update-AmlExperimentUserAsset -ExperimentId $exp.ExperimentId -All 
# Update a particular trained model named "My Model"
Update-AmlExperimentUserAsset -ExperimentId $exp.ExperimentId -AssetType 'TrainedModel' -AssetName 'My Model'
```
This commandlet leverages the config.json file.

#### Export-AmlWebServiceDefinitionFromExperiment
This commandlet exports a Web Service definition file in JSON format. You can then deploy this definition in any web service plan you own as an ARM-based New (as opposed to Classic) Web Service by using the new ARM-based PowerShell command [New-AzureRmMlWebService](https://docs.microsoft.com/en-us/powershell/module/azurerm.machinelearning/New-AzureRmMlWebService?view=azurermps-2.2.0).

```powershell
# Find the predictive Experiment named 'abc'
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Run the Experiment to ensure all schemas are properly populated
Start-AmlExperiment -ExperimentId $exp.ExperimentId
# Export web service definition file to a json file.
Export-AmlWebServiceDefinitionFromExperiment -ExperimentId $exp.ExperimentId -OutputFile myWS.json
```

Please note that you will need to manually add storage account information and Web Service plan ID under the Properties node before you can deploy this Web Service. Here is an example:

```json
"StorageAccount":{
  "name": "<YourStorageAccountName>", 
  "key": "<YourStorageAccountKey>" 
}, 
"CommitmentPlan":{ 
  "id": "subscriptions//resourceGroups//providers/Microsoft.MachineLearning/commitmentPlans/<YourPlanName>"
}
```

This commandlet leverages the config.json file.

### Manage Classic Web Service

#### Get-AmlWebService
```powershell
# Get all classic Web Services in Workspace
$webServices = Get-AmlWebService
# Display them in table format
$webServices | Format-Table Id,Name,EndpointCount
```

```powershell
# Get metadata of a specific classic Web Service with Id stored in $webSvcId
Get-AmlWebService -WebServiceId $webSvcId
```
This commandlet leverages the config.json file.

#### New-AmlWebService

This commandlet deploys a new classic Web Service with a default endpoint from a Predictive Experiment.

```powershell
# Get the Predictive Experiment metadata 
$exp = (Get-AmlExperiment | where Description -eq 'xyz')[0]
# Deploy a new classic Web Service from the Predictive Experiment
$webService = New-AmlWebService -PredictiveExperimentId $exp.ExperimentId
# Display newly created classic Web Service
$webService
# Update an existing classic Web Service from the Predictive Experiment
$webService = New-AmlWebService -PredictiveExperimentId $exp.ExperimentId -Update
```

This commandlet leverages the config.json file.

#### Remove-AmlWebService 

```powershell
# Get the first classic Web Service named 'abc'
$webSvc = (Get-AmlWebService | Where Name -eq 'abc')[0]
# Delete the classic Web Service
Remove-AmlWebService -WebServiceId $webSvc.Id
```
This commandlet leverages the config.json file.

### Manage Classic Web Servcie Endpoint ###

#### Get-AmlWebServiceEndpoint

```powershell
# List all Endpoints of a classic Web Service named 'abc'
$webSvc = Get-AmlWebService | where Name -eq 'abc'
$endpoints = Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id
$endpoints | Format-Table
```

```powershell
# Show metadata of the Endpoint named 'ep01'
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01'
```
Please note that you can supply the Web Service Endpoint API Key as the value of the _-AuthorizationToken_ parameter in lieu of Workspace authorization token for this call. The same applies to the rest of the Endpoint management APIs.

```powershell
# Show metadata of the endpoint named 'ep01', where the apiKey is stored in $apiKey
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01' -AuthorizationToken $apiKey
```
This commandlet leverages the config.json file.

#### Remove-AmlWebServiceEndpoint
```powershell
# Delete the Endpoint named 'ep01'
Remove-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01'
```
This commandlet leverages the config.json file.

#### Add-AmlWebServiceEndpoint
```powershell
Add-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'newep01' -Description 'New Endpoint 01' -ThrottleLevel 'High' -MaxConcurrentCalls 20
```
Please note:

* For Free Workspace, the _-ThrottleLevel_ can only be set to 'Low', the default value. The supplied value for _-MaxConcurrentCalls_ is ignored and the parameter is always defaulted to 4. And the maximum number of Endpoints you can create (including the default one) on a Web Service is 3. 
* For Standard Workspace the _-ThrottleLevel_ values can be set to either 'Low' or 'High'. When it is set to 'Low', the supplied value of _-MaxConcurrentCalls_ is ignored and the parameter is defaulted to 4. When it is set to 'High', the valid value of _-MaxConcurrentCalls_ is between 1 and 200. Check out this [article](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-scaling-endpoints/) for more on Web Service Endpoints scaling. 

This commandlet leverages the config.json file.

#### Refresh-AmlWebServiceEndpoint
Refreshing Endpoint takes the graph behind the default endpoint and applies it to the specified non-default Endpoint. When you republish/update a Web Service from an Experiment, only the default Endpoint is updated. You will need to use the refresh method to update the non-default Endpoints. The _-OverwriteResources_ switch, when set, also causes the Trained Model used in the Endpoint to be replaced with the latest one from the Predictive Experiment. Without it, the Trained Model is not refreshed but the rest of the graph is. Also, default Endpoint cannot be refreshed.

```powershell
# Refresh the endpoint 'ep03'
Refresh-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep03' -OverwriteResources
```
This commandlet leverages the config.json file.

#### Patch-AmlWebServiceEndpoint
Patch Web Service Endpoint is used for updating a trained model in an existing Endpoint. Essentially, you can produce a Trained Model and save it in a _.ilearner_ format in an Azure storage account as a blob. You can accomplish that by calling the BES endpoint using [_Invoke-AmlWebServiceBESEndpoint_](#invoke-amlwebservicebesendpoint) commandlet on the training Web Service. And then you can use _Patch-AmlWebServiceEndpoint_ commandlet to replace a specified Trained Model in an existing non-default Web Service Endpoint with this new _.ilearner_ file. Please browse [Retraining Machine Learning models programatically](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/) for more details.

```powershell
# The name of the Trained Model in the existing Endpoint you are trying to patch. 
# You can obtain this from the Trained Model module in the Predictive Experiment graph, 
# or through the Resources field in the returned result of Get-WebServiceEndpoint commandlet.
$resName = 'Income Predictor [Trained Model]'
# This is the base location of the Windows Azure storage account where the new model is stored as a .ilearner file.
$baseLoc = 'http://mystorageaccount.blob.core.windows.net'
# The relative location of the .ilearner file, basically the container name and the path to the blob.
$relativeLoc = 'mycontainer/retrain/new_model.ilearner'
# The SAS token on the ilearner file to allow Read access.
$sasToken = '?sr=b&se=2016-02-05T04......'
# Web Service Id
$webSvcId = (Get-AmlWebService | where Name -eq 'xyz').Id
# endpoint name
$epName = 'ep02'
# Update the Endpoint with the new Trained Model.
Patch-AmlWebServiceEndpoint -WebServiceId $webSvcId -EndpointName $epName -ResourceName $resName -BaseLocation $baseLoc -RelativeLocation $relativeLoc -SasBlobToken $sasToken
```
This commandlet leverages the config.json file.

### Call Azure ML Web Service APIs
The following two commandlets allow you to call Azure ML Web Service API in Request-Response Service (RRS) style, as well as Batch Execution Service (BES) style. Please note that they do not require configuration files.

#### Invoke-AmlWebServiceRRSEndpoint
First, construct an json file as the input data to be scored on. You will need to feed it to the _-InputJsonFile_ parameter. Following is a sample _input.json_ file. You should follow the RRS API Documentation Help page of your Web Service Endpoint for the sample input more specific to your RRS Endpoint.

_input.json_

```json
{
	"Inputs": {
		"Input1" : {
			"ColumnNames": ["age", "sex", "bmi", "children", "smoker", "region"],
			"Values": [
				[20, "female", 21, 0, "no", "Northeast"],
				[30, "male", 41, 1, "yes", "Southwest"]
			]
		}
	},
	"GlobalParameters": { "Random seed": 12345}
}
```

Next, you can call the RRS Endpoint using the commandlet. Please note you can obtain the RRS POST request URL and the API key using the _Get-AmlWebServiceEndpoint_ commandlet. You can also read them off the RRS API Documentation page.

```powershell
# POST Request URL for RRS Endpoint 'abc' on Web Service 'xyz'
$webSvcId = (Get-AmlWebService | where Name -eq 'xyz').Id
$ep = Get-AmlWebServiceEndpoint -WebServiceId $webSvcId -EndpointName 'abc'
$postUrl =  $ep.ApiLocation + '/execute?api-version=2.0&details=true'
# Base-64 encoded API key
$apiKey = $ep.PrimaryKey
# Invoke RRS Endpoint
Invoke-AmlWebServiceRRSEndpoint -POSTRequestUrl $postUrl -ApiKey $apiKey -inputFile 'C:\Temp\input.json' -outputFile 'C:\Temp\predictions.json'
```
The above example shows feeding the input json data using a local file, and getting the results written back into a local file. You can also directly feed the input json string using _-InputJsonText_ parameter, and harvest the resulting json string directly without specifying the _-OutputJsonFile_ parameter. 

#### Invoke-AmlWebServiceBESEndpoint
First, store your input dataset, for example _input.csv_, in an Azure storage account as a blob. Then create a BES job configuration file in Json format locally which essentially references the input file location, as well as the desired output file location, both in Azure storage account. Again, please check with the BES API Documentation Help page for the sample request payload specific to your BES Endpoint.

_jobConfig.json_

```json
{
	"GolbalParameters": { "Random seed": 12345 },
	"Inputs": {
		"input1" : {
			"ConnetionString": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=mystorageaccountkey",
			"RelativeLocation": "mycontainer/input/input.csv"
		}
	},
	"Outputs": {
		"output1": {
			"ConnetionString": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=mystorageaccountkey",
			"RelativeLocation": "mycontainer/output/output.csv"
		}
	} 
}
```

Now you are ready to invoke the BES Endpoint.

```powershell
# Find the submit job request Url for BES Endpoint 'abc' on Web Service 'xyz'
$webSvcId = (Get-AmlWebService | where Name -eq 'xyz').Id
$ep = Get-AmlWebServiceEndpoint -WebServiceId $webSvcId -EndpointName 'abc'
$jobSubmitUrl =  $ep.ApiLocation + '/jobs?api-version=2.0'
# Base-64 encoded API key
$apiKey = $ep.PrimaryKey
# Invoke BES Endpoint
Invoke-AmlWebServiceBESEndpoint -SubmitJobRequestUrl $jobSubmitUrl -ApiKey $apiKey -JobConfigFile '.\jobConfig.json'
```

### List Other Assets in Workspace ###
#### Get-AmlAnnotation

```powershell
# Get all annotations in an Experiment
$annotations = Get-AmlAnnotation -ExperimentId $exp.ExperimentId
```

This commandlet leverages the config.json file.

#### Get-AmlNotebook

```powershell
# Get all Notebooks in in the Workspace
$nbs = Get-AmlNotebook
```

This commandlet leverages the config.json file.

#### Get-AMLNotebookSession 

```powershell
# Get a Notebook Session
$nbs = Get-AmlNotebookSession -FamilyId $nbs[0].FamilyId
```

This commandlet leverages the config.json file.

#### Get-AmlProjectContainer

```powershell
# Get all Project Containers in the Workspace
$projects = Get-AmlProjectContainer
```

This commandlet leverages the config.json file.

#### Get-AmlGateway

```powershell
# Get all Data Gateways in the Workspace
$gateways = Get-AmlGateway
```

This commandlet leverages the config.json file.

