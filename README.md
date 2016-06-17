# PowerShell Module for Azure Machine Learning Studio & Web Services Beta v.0.2.7
## Introduction
This is a preview release of PowerShell Commandlet Library for [Azure Machine Learning](https://studio.azureml.net). It allows you to interact with Azure Machine Learning Workspace, or Workspace for short, Datasets, Trained Models, Transforms, Custom Modules, Experiments, Web Services and Web Service Endpoints. The supported operations are:

* __Manage Workspace__
  * Create new Workspace using a management certificate (*[New-AmlWorkspace](#new-amlworkspace)*) 
  * List all Workspaces in an Azure subscription (*[List-AmlWorkspaces](#list-amlworkspaces)*)
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
* __Manage Experiment__
  * List all Experiments in Workspace (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Get the metadata of a specific Experiment (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Export a specific Experiment graph to a file in JSON format (*[Export-AmlExperimentGraph](#export-amlexperimentgraph)*)
  * Import a JSON file to overwrite an existing Experiment or create a new Experiment (*[Import-AmlExperimentGraph](#import-amlexperimentgraph)*)
  * Run an Experiment (*[Start-AmlExperiment](#start-amlexperiment)*)
  * Delete an Experiment (*[Remove-AmlExperiment](#remove-amlexperiment)*)
  * Copy an Experiment from a Workspace to another Workspace within the same region (*[Copy-AmlExperiment](#copy-amlexperiment)*)
  * Find a module node in an Experiment using its comment. (*[Get-AmlExperimentNode](#get-amlexperimentnode)*)
  * Replace a user asset in an Experiment with another asset from Workspace (*[Replace-AmlExperimentUserAsset](#replace-amlexperimentuserasset)*)
  * Update user assets in an Experiment with the latest version (*[Update-AmlExperimentUserAsset](#update-amlexperimentuserasset)*)
* __Manage Web Service__
  * Deploy a Web Service from a Predictive Experiment (*[New-AmlWebService](#new-amlwebservice)*)
  * List all Web Services in Workspace (*[Get-AmlWebService](#get-amlwebservice)*)
  * Get the attributes of a specific Web Service (*[Get-AmlWebService](#get-amlwebservice)*)
  * Delete a Web Service (*[Remove-AmlWebService](#remove-amlwebservice)*)
* __Manage Web Service Endpoint__
  * List all Endpoints of a Web Service (*[Get-AmlWebServiceEndpoint](#get-amlwebserviceendpoint)*)
  * Get attributes of a specific Endpoint of a Web Service (*[Get-AmlWebServiceEndpoint](#get-amlwebserviceendpoint)*)
  * Create a new Endpoint on an existing Web Service (*[Add-AmlWebServiceEndpoint](#add-amlwebserviceendpoint)*)
  * Delete a Web Service Endpoint (*[Remove-AmlWebServiceEndpoint](#remove-amlwebserviceendpoint)*)
  * Refresh a Web Service Endpoint (*[Refresh-AmlWebServiceEndpoint](#refresh-amlwebserviceendpoint)*)
  * Patch a Web Service Endpoint (*[Patch-AmlWebServiceEndpoint](#patch-amlwebserviceendpoint)*)
* __Call Azure ML Web Service APIs__
  * Invoke a RRS (Request-Response Service) API (*[Invoke-AmlWebServiceRRSEndpoint](#invoke-amlwebservicerrsendpoint)*)
  * Invoke a BES (Batch Execution Service) API (*[Invoke-AmlWebServiceBESEndpoint](#invoke-amlwebservicebesendpoint)*)


## System Requirement
This PowerShell module requires PowerShell 4.0 and .NET 4.5.2. 

Also, you need to have Owner access to at least one Azure Machine Learning Studio Workspace. You can obtain a free Workspace by simply logging in to [Azure Machine Learning Studio](https://studio.azureml.net) with any valid Microsoft account, or your school or work email address. 

For managing Web Service Endpoints, you can also use the API Key created for each endpoint as the authorization token, in lieu of the Workspace Authorization Token.

For more information on Azure Machine Learning, browse the [Azure Machine Learning Homepage](http://www.azure.com/ml). 

## Installation
Simply download the _AzureMLPS.zip_ from the [Releases area](https://github.com/hning86/azuremlps/releases), then unzip the file locally. You will find a _AzureMLPS.dll_ file which is a PowerShell module file, and a sample _config.json_ file. Run the PowerShell command _Unblock-File_ then _Import-Module_ to unblock the PowerShell module and then import it into the current PowerShell session:

```
#Unblock the downloaded dll file so Windows can trust it.
Unblock-File .\AzureMLPS.dll
#import the PowerShell module into current session
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
		* West Europe	
		* Germany Central
		
	![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/WorkspaceRegion.png)

There are 3 different ways to specify these values:

1. Create a default _config.json_ file in the same folder where the _AzureMLPS.dll_ file is located. A sample config file is included in the ZIP package so you can simply modify it:

	_config.json_

	```		
	{
		"WorkspaceId": "d2f62586bed343d621441d55b3872d53",
		"AuthorizationToken": "288a8283e4944bff9c6651a3b6004ef4",
		"Location": "South Central US"
	}	
	```

	
	Then you can simply execute the commandlet like this:
	
	```
	Get-AmlWorkspace
	```
	
2. Or, use the _-ConfigFile_ command parameter to supply the absolute path to a custom config file, using the exact same json format. Please note that relative path will NOT work. It has to be an absolute path. This overrides the default config file if it exists. For example:
	
	```
	Get-AmlWorkspace -ConfigFile 'C:\Configs\MyWorkspace02Config.json'
	```
3. Or, specify values to the _-WorkspaceId_, _-AuthorizationToken_, and _-Location_ parameters directly in the commandlet. The values supplied here override the default and the custom config file. For example:

	```
	Get-AmlWorkspace -WorkspaceId '0123456789abcdef01230123456789ab' -AuthorizationToken 'abcdef0123456789abcdef0123456789' -Location 'South Central US'
	```

For simplicity, the examples below all assume that a valid default config file exists.

## Usage
Remember you can always use _Get-Help_ on any of the following commandlet. For example, to see all supported parameters of _Get-AmlWorkspace_ commandlet, run the following command:

```
Get-Help Get-AmlWorkspace
```

### Manage Workspace

#### New-AmlWorkspace

To create a new Azure ML Workspace, you need to first generate a self-signed certificate, store it in the current user's certificate store, and then upload the public key portion (.cer file) into Azure management portal. The _New-AmlWorkspace_ commandlet will communicate with Azure management API using this certificate to ensure this is an authorized access. Read [more information](https://www.simple-talk.com/cloud/security-and-compliance/windows-azure-management-certificates/) on this subject.

```
$azureSubscriptionId = '<azure_subscription_id>'
$mgmtCertThumb = '12345'
$location = 'South Central US'
$storageAccountName = '<my_storage_account_name'
$storageAccountKey = '<my_storage_account_key>'
$ownerEmail = 'myname@mycompany.com'
# Create a new Azure ML Worksace named 'ABCD'
New-AmlWorkspace -AzureSubscriptionId $azureSubscriptionId -ManagementCertThumbprint $mgmtCertThumb -WorkspaceName 'ABCD' -Location $location -storageAccountName $storageAccountName -StorageAccountKey $storageAccountKey -OwnerEmail $ownerEmail
```

For quick reference, you can create self-signed certificate using _makecert.exe_, which is a command line tool that comes with Visual Studio and/or Windows SDK. The following command creates the private key in the CurrentUser/My store, and also output the public key in a file that you can upload into Azure management portal. 

```
makecert.exe -sky exchange -r -n 'CN=My_Azure_Management_Cert' -pe -a sha1 -len 2048 -ss My 'MyAzureMgmtCert.cer'
```

Or, you can use PowerShell commandlet _New-SelfSignedCertificate_ and _Export-Certificate_

```
# Create the self-signed certificate in CurrentUser\My store.
$cert = New-SelfSignedCertificate -CertStoreLocation cert:\CurrentUser\My -Subject 'CN=My_Azure_Management_Cert' -KeySpec KeyExchange -KeyExportPolicy Exportable -HashAlgorithm SHA1 
# Export the public key as a .cer file
Export-Certificate -Cert $cert -Type CERT -FilePath 'c:\temp\mycert.cer'
```

And here is how you can grab the thumbprint of a particular certificate using PowerShell.

```
(dir Cert:\CurrentUser\My | where Subject -eq 'CN=My_Azure_Management_Cert').Thumbprint
```

#### List-AmlWorkspaces
Please note that this commandlet can only list Standard Wokspaces. Free Workspaces are not tied to any Azure subscriptions so they will not be listed here.

```
List-AmlWorkspace -AzureSubscriptionId '<azure_subscription_id>' -ManagementCertThumbprint '<management_cert_thumbprint>'
```

#### Add-AmlWorkspaceUsers

Please note the email addresses are comma-separated. And the supported roles are Owner and User.

```
Add-AmlWorkspaceUsers -Emails 'john@smith.com,jane@doe.com' -Role 'User'
```
This commandlet leverages the config.json file.


#### Get-AmlWorkspaceUsers


```
# Get all users of the current workspace
Get-AmlWorkspaceUsers
```
This commandlet leverages the config.json file.


#### Get-AmlWorkspace

```
# Returns the metadata of a Workspace
$ws = Get-AmlWorkspace
# Display the Workspace Name
$ws.FriendlyName
```
This commandlet leverages the config.json file.

### Manage User Assets (Dataset, Trained Model, Transform)
#### Get-AmlDataset
```
# Get a list of all datasets in an Experiment 'abc':
$exp = Get-AmlExperiment | where Description -eq 'abc'
$ds = Get-AmlDataset -Scope Experiment -ExperimentId $exp.ExperimentId
# Get a list of all datasets in a Workspace:
$ds = Get-AmlDataset -Scope Workspace
# Display the list in a table format with selected properties
$ds | Format-Table Name,DataTypeId,Size,Owner
```

#### Promote-AmlDataset
To use this commandlet, you need to first locate the module in your experiment where the output port produces the Dataset you'd like to promote. So you need to gather the experiment id, node id, and the name of the output port. In order to get the node id, you need to add a unique comment to the Train Model module first, and then use the Get-AmlExperimentNode commandlet to grab the node id.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteDataset.png)

Also, if there is already a Dataset of with the same name you supply to this commandlet, you must use -Overwrite parameter, otherwise you will receive a HTTP 409 (Conflict) error.

```
# Find experiment named 'abc' and run it
$exp = Get-AmlExperiment | where Description -eq 'abc'
Start-AmlExperiment -ExperimentId $exp.ExperimentId
# Find the a node in the experiment with a comment 'Split My Data'. In this case it is a Split module
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Split My Data'
# Promote the outcome of one of the left output port of the Split node, and overwrite the previous version.
Promote-AmlDataset -ExperimentId $exp.ExperimentId -ModuleNodeId $node.Id -NodeOutputName 'Result dataset1' -DatasetName 'MyData' -DataSetDescription 'My Data' -Overwrite
```

#### Download-AmlDataset
```
# Find a dataset named 'Movie tweets' in the Workspace using Get-AmlDataset:
$dsMT = Get-AmlDataset | where Name -eq 'Movie tweets'
# Download the Movie Tweets dataset:
Download-AmlDataset -DatasetId $dsMT.Id -DownloadFileName 'C:\Temp\MovieTweets.csv'
```

#### Upload-AmlDataset
```
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

#### Remove-AmlDataset
```
# Find a dataset named 'Flight Data' in the Workspace using Get-AmlDataset:
$dsFlight = Get-AmlDataset | where Name -eq 'Flight Data'
# Delete the dataset from Workspace
Remove-AmlDataset -DatasetFamilyId $dsFlight.FamilyId
```

#### Get-AmlTrainedModel
```
# Get a list of all Trained Models in an Experiment 'abc':
$exp = Get-AmlExperiment | where Description -eq 'abc'
$ds = Get-AmlTrainedModel -Scope Experiment -ExperimentId $exp.ExperimentId
# Get a list of all Trained Models in a Workspace:
$ds = Get-AmlTrainedModel -Scope Workspace
# Display the list in a table format with selected properties
$ds | Format-Table Name, Id, FamilyId
```

#### Promote-AmlTrainedModel
To use this commandlet, you need to first locate the Train module in your experiment where the output port produces the Trained Model you'd like to promote. So you need to gather the experiment id, node id, and the name of the output port. In order to get the node id, you need to add a unique comment to the Train Model module first, and then use the *Get-AmlExperimentNode* commandlet to grab the node id.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteTrainedModel.png)

Also, if there is already a Trained Model of with the same name you supply to this commandlet, you must use *-Overwrite* parameter, otherwise you will receive a HTTP 409 (Conflict) error.

```
# Find experiment named 'abc' and run it
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Run the experiment at least once so the result is cached.
Start-AmlExperiment -ExperimentId $exp.ExperimentId
# Find the Train Model module node in the experiment with a comment 'Train me'.
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Train me'
# Promote the Trained Model from the output port of the Train Model module, and overwrite the previous version.
Promote-AmlTrainedModel -ExperimentId $exp.ExperimentId -ModuleNodeId $node.Id -NodeOutputName 'Trained model' -TrainedModelName 'MyModel' -TrainedModelDescription 'My Model' -Overwrite
```


#### Get-AmlTransform
```
# Get a list of all Transforms in an Experiment 'abc':
$exp = Get-AmlExperiment | where Description -eq 'abc'
$ds = Get-AmlTransform -Scope Experiment -ExperimentId $exp.ExperimentId
# Get a list of all Transforms in a Workspace:
$ds = Get-AmlTransform -Scope Workspace
# Display the list in a table format with selected properties
$ds | Format-Table Name,Id, FamilyId
```

#### Promote-AmlTransform
To use this commandlet, you need to first locate the module in your experiment where the output port produces the Transform you'd like to promote. So you need to gather the experiment id, node id, and the name of the output port. In order to get the node id, you need to add a unique comment to the transform-producing module first, and then use the *Get-AmlExperimentNode* commandlet to grab the node id.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteTransform.png)

Also, if there is already a Transform of with the same name you supply to this commandlet, you must use *-Overwrite* parameter, otherwise you will receive a HTTP 409 (Conflict) error.

```
# Find experiment named 'abc' and run it
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Run the experiment at least once so the result is cached.
Start-AmlExperiment -ExperimentId $exp.ExperimentId
# Find the Clean Missing Data module in the experiment where a Clean Transform is produced with a comment 'Clean me'.
$node = Get-AmlExperimentNode -ExperimentId $exp.ExperimentId -Comment 'Clean me'
# Promote the Transform from the output port of the transform-producing module, and overwrite the previous version.
Promote-AmlTransform -ExperimentId $exp.ExperimentId -ModuleNodeId $node.Id -NodeOutputName 'Cleaning transformation' -TransformName 'CleanMe02' -TransformDescription 'Clean Me v2' -Overwrite
```


### Manage Custom Module
#### New-AmlCustomModule
```
# Upload a new Custom Module from C:\Temp\MyModule.zip
New-AmlCustomModule -CustomModuleZipFileName 'C:\Temp\MyModule.zip'
```

#### Get-AmlModule
```
# List all modules
Get-AmlModule
# List custom modules only
Get-AmlModule -Custom
# Get "Add Rows" module
Get-AmlModule | where Name -eq 'Add Rows'
```

### Manage Experiment
#### Get-AmlExperiment 

```
# Get all Experiments in the Workspace
$exps = Get-AmlExperiment
# Display all Experiments in a table format
$exps | Format-Table
```
```
# Get the metadata of the Experiment named 'xyz' in the Workspace
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Display the Experiment status
$exp.Status.StatusCode
```

#### Export-AmlExperimentGraph
```
# Export an Experiment named "xyz" to "MyExp.json"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
Export-AmlExperimentGraph -ExperimentId $exp.ExperimentId -OutputFile 'c:\Temp\MyExp.json'
```
Please note that the exported JSON file only contains references to the exact instance and version of the assets (modules, trained models, datasets, etc.). The assets themselves are NOT serialized into the JSON file. As a consequence, when you import it back into the Workspace, make sure the exact same instance and version of those assets do exist in the Workspace, otherwise you will not be able to create a valid Experiment. Also, please make sure you use absolute path when referring to the json file.

#### Import-AmlExperimentGraph
```
# Import a JSON file 'MyExp.json' to overwrite the Experiment where the file is exported out of
Import-AmlExperimentGraph -InputFile 'C:\Temp\MyExp.json' -Overwrite
# Import a JSON file 'MyExp.json' to create a new Experiment named 'abc'
Import-AmlExperimentGraph -InputFile 'MyExp.json' -NewName 'abc'
```


#### Start-AmlExperiment
```
# Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Run the Experiment
Start-AmlExperiment -ExperimentId $exp.ExperimentId
```
#### Remove-AmlExperiment
```
# Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Delete the Experiment
Remove-AmlExperiment -ExperimentId $exp.ExperimentId
```

#### Copy-AmlExperiment
```
# Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
# Copy that Experiment from current Workspace to another Workspace
Copy-AmlExperiment -ExperimentId $exp.ExperimentId -DestinationWorkspaceId '<ws_id>' -DestinationWorkspaceAuthorizationToken '<auth_token>'
```
Please note that the current Workspace and the destination Workspace must be in the same region. Cross-region copy is currently not supported.

#### Get-AmlExperimentNode
This commandlet lets you find information, including Id, FamilyId etc. of node(s) with a certain user comment.

![image](https://raw.githubusercontent.com/hning86/azuremlps/master/screenshots/PromoteTrainedModel.png)

```
# Find the Experiment named "abc"
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Get the node(s) with a user comment "Train My Model"
$node = Get-AmlExperiment -ExperimentId $exp.ExperimentId -Comment 'Train me'
# Display the node
$node
```

#### Replace-AmlExperimentUserAsset
This commandlet lets you replace a user asset (Dataset, Trained Model or Transform) in an Experiment with another one from the Workspace. If you simply want to update an asset to the latest version, use *Update-AmlExperimentUserAsset* instead.
```
# Find the Experiment named 'abc'
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Find the transform you want to replace
$oldTransform = Get-AmlTransform -Scope Experiment -ExperimentId $exp.ExperimentId | where Name -eq 'Transform A'
# Find the the Transform in the Workspcae you want to use instead
$newTransform = Get-AmlTransform -Scope Workspace | where Name -eq 'Transform B'
# Replace Transform A with Transform B
Replace-AmlExperimentUserAsset -ExperimentId $exp.ExperimentId -AssetType 'Transform' -ExistingAsset $oldTransform -NewAsset $newTransform
```


#### Update-AmlExperimentUserAsset
This commandlet lets you update user assets (Dataset, Trained Model, Transform) in an Experiment to their latest versions.
```
# Find the Experiment named 'abc'
$exp = Get-AmlExperiment | where Description -eq 'abc'
# Update all user assets (including Datasets, Trained Models and Transforms) in an Experiment
Update-AmlExperimentUserAsset -ExperimentId $exp.ExperimentId -All 
# Update a particular trained model named "My Model"
Update-AmlExperimentUserAsset -ExperimentId $exp.ExperimentId -AssetType 'TrainedModel' -AssetName 'My Model'
```

### Manage Web Service

#### Get-AmlWebService
```
# Get all Web Services in Workspace
$webServices = Get-AmlWebService
# Display them in table format
$webServices | Format-Table Id,Name,EndpointCount
```

```
# Get metadata of a specific Web Service with Id stored in $webSvcId
Get-AmlWebService -WebServiceId $webSvcId
```

#### New-AmlWebService

This commandlet deploys a new Web Service with a default endpoint from a Predictive Experiment.

```
# Get the Predictive Experiment metadata 
$exp = (Get-AmlExperiment | where Description -eq 'xyz')[0]
# Deploy Web Service from the Predictive Experiment
$webService = New-AmlWebService -PredictiveExperimentId $exp.ExperimentId
# Display newly created Web Service
$webService
```

<span style="color:red">Known issue: calling _New-AmlWebService_ will produce a new copy of web service. This is a server side issue that will be addressed soon.</span>


#### Remove-AmlWebService 

```
# Get the first Web Service named 'abc'
$webSvc = (Get-AmlWebService | Where Name -eq 'abc')[0]
# Delete the Web Service
Remove-AmlWebService -WebServiceId $webSvc.Id
```

### Manage Web Servcie Endpoint ###

#### Get-AmlWebServiceEndpoint

```
# List all Endpoints of a Web Service named 'abc'
$webSvc = Get-AmlWebService | where Name -eq 'abc'
$endpoints = Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id
$endpoints | Format-Table
```


```
# Show metadata of the Endpoint named 'ep01'
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01'
```
Please note that you can supply the Web Service Endpoint API Key as the value of the _-AuthorizationToken_ parameter in lieu of Workspace authorization token for this call. The same applies to the rest of the Endpoint management APIs.

```
# Show metadata of the endpoint named 'ep01', where the apiKey is stored in $apiKey
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01' -AuthorizationToken $apiKey
```

#### Remove-AmlWebServiceEndpoint
```
# Delete the Endpoint named 'ep01'
Remove-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01'
```
#### Add-AmlWebServiceEndpoint
```
Add-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'newep01' -Description 'New Endpoint 01' -ThrottleLevel 'High' -MaxConcurrentCalls 20
```
Please note:

* For Free Workspace, the _-ThrottleLevel_ can only be set to 'Low', the default value. The supplied value for _-MaxConcurrentCalls_ is ignored and the parameter is always defaulted to 4. And the maximum number of Endpoints you can create (including the default one) on a Web Service is 3. 
* For Standard Workspace the _-ThrottleLevel_ values can be set to either 'Low' or 'High'. When it is set to 'Low', the supplied value of _-MaxConcurrentCalls_ is ignored and the parameter is defaulted to 4. When it is set to 'High', the valid value of _-MaxConcurrentCalls_ is between 1 and 200. Check out this [article](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-scaling-endpoints/) for more on Web Service Endpoints scaling. 

#### Refresh-AmlWebServiceEndpoint
Refreshing Endpoint takes the graph behind the default endpoint and applies it to the specified non-default Endpoint. When you republish/update a Web Service from an Experiment, only the default Endpoint is updated. You will need to use the refresh method to update the non-default Endpoints. The _-OverwriteResources_ switch, when set, also causes the Trained Model used in the Endpoint to be replaced with the latest one from the Predictive Experiment. Without it, the Trained Model is not refreshed but the rest of the graph is. Also, default Endpoint cannot be refreshed.

```
# Refresh the endpoint 'ep03'
Refresh-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep03' -OverwriteResources
```

#### Patch-AmlWebServiceEndpoint
Patch Web Service Endpoint is used for updating a trained model in an existing Endpoint. Essentially, you can produce a Trained Model and save it in a _.ilearner_ format in an Azure storage account as a blob. You can accomplish that by calling the BES endpoint using [_Invoke-AmlWebServiceBESEndpoint_](#invoke-amlwebservicebesendpoint) commandlet on the training Web Service. And then you can use _Patch-AmlWebServiceEndpoint_ commandlet to replace a specified Trained Model in an existing non-default Web Service Endpoint with this new _.ilearner_ file. Please browse [Retraining Machine Learning models programatically](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/) for more details.

```
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
### Call Azure ML Web Service APIs
The following two commandlets allow you to call Azure ML Web Service API in Request-Response Service (RRS) style, as well as Batch Execution Service (BES) style. Please note that they do not require configuration files.

#### Invoke-AmlWebServiceRRSEndpoint
First, construct an json file as the input data to be scored on. You will need to feed it to the _-InputJsonFile_ parameter. Following is a sample _input.json_ file. You should follow the RRS API Documentation Help page of your Web Service Endpoint for the sample input more specific to your RRS Endpoint.

_input.json_

```
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

```
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

```
{
	"GolbalParameters": { "Random seed": 12345 },
	"Inputs": {
		"input1" : {
			"ConnetionString": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=mystorageaccountkey"
			"RelativeLocation": "mycontainer/input/input.csv"
		}
	},
	"Outputs": {
		"output1": {
			"ConnetionString": "DefaultEndpointsProtocol=https;AccountName=mystorageaccount;AccountKey=mystorageaccountkey"
			"RelativeLocation": "mycontainer/output/output.csv"
		}
	} 
}
```

Now you are ready to invoke the BES Endpoint.

```
# Find the submit job request Url for BES Endpoint 'abc' on Web Service 'xyz'
$webSvcId = (Get-AmlWebService | where Name -eq 'xyz').Id
$ep = Get-AmlWebServiceEndpoint -WebServiceId $webSvcId -EndpointName 'abc'
$jobSubmitUrl =  $ep.ApiLocation + '/jobs?api-version=2.0'
# Base-64 encoded API key
$apiKey = $ep.PrimaryKey
# Invoke BES Endpoint
Invoke-AmlWebServiceBESEndpoint -SubmitJobRequestUrl $jobSubmitUrl -ApiKey $apiKey -JobConfigFile '.\jobConfig.json'
```


