# PowerShell Commandlets for Azure Machine Learning Studio & Web Service APIs
## Introduction
This is a preview release of PowerShell Commandlet Library for [Azure Machine Learning](https://studio.azureml.net). It allows you to interact with Azure Machine Learning Workspace, or Workspace for short. The supported operations are:

* __Manage Workspace__
  * Get the metadata of Workspace(*[Get-AmlWorkspace](#get-amlworkspace)*)
* __Manage Dataset__
  * List all Datasets in Workspace (*[Get-AmlDataset](#get-amldataset)*)
  * Download a Dataset file from Workspace to local file directory (*[Download-AmlDataset](#download-amldataset)*)
  * Upload a Dataset file from local file directory to Workspace (*[Upload-AmlDataset](#upload-amldataset)*)
  * Delete a Dataset file in Workspace (*[Remove-AmlDataset](#remove-amldataset)*)
* __Manage Experiment__
  * List all Experiments in Workspace (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Get the metadata of a specific Experiment (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Run an Experiment (*[Start-AmlExperiment](#start-amlexperiment)*)
  * Delete an Experiment (*[Remove-AmlExperiment](#remove-amlexperiment)*)
  * Copy an Experiment from a Workspace to another Workspace (*[Copy-AmlExperiment](#copy_amlexperiment)*)
* __Manage Web Service__
  * List all Web Services in Workspace (*[Get-AmlWebService](#get-amlwebservice)*)
  * Get the attributes of a specific Seb Service (*[Get-AmlWebService](#get-amlwebservice)*)
  * Deploy a Web Service from a Predictable Experiment (*[New-AmlWebService](#new-amlwebservice)*)
  * Delete a Web Service (*[Remove-AmlWebService](#remove-amlwebservice)*)
* __Manage Web Service Endpoint__
  * List all Endpoints of a Web Service (*[Get-AmlWebServiceEndpoint](get-amlwebserviceendpoint)*)
  * Get attributes of a specific Endpoint of a Web Service (*[Get-AmlWebServiceEndpoint](#get-amlwebserviceendpoint)*)
  * Delete a Web Service Endpoint (*[Remove-AmlWebServiceEndpoint](#remove-amlwebserviceendpoint)*)
  * Create a new Web Service Endpoint in an existing Web Service (*[Add-AmlWebServiceEndpoint](#add-amlwebserviceendpoint)*)
  * Refresh a Web Service Endpoint (*[Refresh-AmlWebServiceEndpoint](#refresh_amlwebserviceendpoint)*)
  * Patch a Web Service Endpoint (*[Patch-AmlWebServiceEndpoint](#patch_amlwebserviceendpoint)*)
* __Call Azure ML Web Service APIs__
  * Execute a RRS (Request-Response Service) API (*[Invoke-AmlWebServiceRRSEndpoint](#invoke-amlwebservicerrsendpoint)*)
  * Execute a BES (Batch Execution Service) API (*[Invoke-AmlWebServiceBESEndpoint](#invoke_amlwebservicebesendpoint)*)

## System Requirement
This PowerShell module requires PowerShell 4.0 and .NET 4.5.2. 

Also, you need to have Owner access to at least one Azure Machine Learning Studio Workspace. You can obtain a free Workspace by simply logging in to [Azure Machine Learning Studio](https://studio.azureml.net) with any valid Microsoft account, or your School or Work email address. 

For managing Web Service Endpoints, you can also use the API Key created for each endpoint as the authorization token, in lieu of the Workspace Authorization Token.

For more information on Azure Machine Learning, see the [Azure Machine Learning Homepage](http://www.azure.com/ml). 

## Installation
Simply download the _AzureMLPS.dll_ which is a PowerShell module file, then run the PowerShell command _Import-Module_ to import the module into the current PowerShell environment:

```
Import-Module .\AzureMLPS.dll
```
## Configuration
Most of the commandlets require 3 pieces of key information in order to function:

* **Workspace ID**
	* This value can be found in Workspace Settings in ML Studio.
* **Workspace Authorization Token**
	* This value can be found in Workspace Settings in ML Studio.
	* Please note for the Web Service Endpoint Management commandlets, you can also use the Endpoint API Key in lieu of the Workspace Authorization Token
* **Region Name**
	* This value can be found in the Workspace dropdown. Currently supported values for this configuration are:
		* South Central US (use this value for all Free Workspaces)
		* Southeast Asia
		* Northern Europe

There are 3 ways to specify these values:

1. Create a default _config.json_ file in the same folder where you are running the PowerShell commandlets. A sample should look like this:

	__*config.json*__

	```		
	{
		"WorkspaceId": "12341234-1234-1234-1234-123412341234",
		"AuthorizationToken": "12341234-1234-1234-1234-123412341234",
		"RegionName": "South Central US"
	}	
	```

	
	Then you can simply execute the commandlet like this:
	
	```
	Get-AmlWorkspace
	```
	
2. Or, use the _-ConfigFile_ command parameter to supply the path and name to a custom config file, using the exact same json format. This overrides the default config file. For example:
	
	```
	Get-AmlWorkspace -ConfigFile 'C:\Config\myConfig.json'
	```
3. Or, use the _-WorkspaceId_, _-AuthorizationToken_, and _-RegionName_ parameters directly in the commandlet. The values supplied here override the default and the custom config file. For eample:

	```
	Get-AmlWorkspace -WorkspaceId '0123456789abcdef01230123456789ab' -AuthorizationToken 'abcdef0123456789abcdef0123456789' -RegionName 'South Central US'
	```

For simplicity, the examples below all assumes the valid default config file exists.

## Usage
Remember you can always use *Get-Help* on any of the following commandlet. For example, to understand how to use Get-AmlWorkspace, run the following command:

```
Get-Help Get-AmlWorkspace
```

### Manage Workspace
#### Get-AmlWorkspace

```
# Returns the attributes of a Workspace
$ws = Get-AmlWorkspace
# Display the Workspace Name
$ws.FriendlyName
```

### Manage Dataset
#### Get-AmlDataset
```
# Get a list of all datasets in a Workspace:
$ds = Get-AmlDataset
# Display the list in a table format with selected properties
$ds | Format-Table Name,DataTypeId,Size,Owner
```

#### Download-AmlDataset
```
#Find a dataset named 'Movie tweets' in the Workspace using Get-AmlDataset:
$dsMT = Get-AmlDataset | where Name -eq 'Movie tweets'
#Download the Movie Tweets dataset:
Download-AmlDataset -DatasetId $dsMT.Id -DownloadFileName 'C:\Temp\MovieTweets.csv'
```

#### Upload-AmlDataset
```
#Upload a local file in .csv format to Workspace
Upload-AmlDataset -FileFormat GenericCSV -UploadFileName 'C:\Temp\MovieTweets.csv' -DatasetName 'Movie Tweets' -DatasetDescription 'Tweeter data on popular movies'
```
Note the supported file formats are: 

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
#Find a dataset named 'Flight Data' in the Workspace using Get-AmlDataset:
$dsFlight = Get-AmlDataset | where Name -eq 'Flight Data'
#Delete the dataset from Workspace
Remove-AmlDataset -DatasetFamilyId $dsFlight.FamilyId
```
### Manage Experiment ###
#### Get-AmlExperiment ####

```
#Get all experiments in the Workspace
$exps = Get-AmlExperiment
#Display all experiments in a table format
$exps | Format-Table
```
```
#Get the metadata of the experiment named 'xyz' in the Workspace
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Display the experiment status
$exp.Status.StatusCode
```

#### Start-AmlExperiment ####
```
#Find the experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Run an experiment
Start-AmlExperiment -ExperimentId $exp.ExperimentId
```
#### Remove-AmlExperiment ####
```
#Find the experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Delete an experiment
Remove-AmlExperiment -ExperimentId $exp.ExperimentId
```
#### Copy-AmlExperiment ####

```
#Find the experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Copy that experiment from current Workspace to another Workspace
Copy-AmlExperiment -ExperimentId $exp.ExperimentId -DestinationWorkspaceId '<ws_id>' -DestinationWorkspaceAuthorizationToken '<auth_token>'
```
Please note that the current Workspace and the destination Workspace must be in the same region. Cross-region copy is currently not supported.

### Manage Web Service ###

#### Get-AmlWebService ####
```
#Get all web services in Workspace
$webServices = Get-AmlWebService
#Display them in table format
$webServices | Format-Table Id, Name, EndpointCount
```

```
#Get metadata of a specific web service with Id stored in $webSvcId
Get-AmlWebService -WebServiceId $webSvcId
```

#### New-AmlWebService

This commandlet deploys a new Web Service with a default endpoint from a Predicative Experiment.

```
#Get the Predicative Experiment metadata 
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Deploy Web Service from it.
$webService = New-AmlWebService -PredicativeExperimentId $exp.ExperimentId
#Display newly created Web Service
$webService
```

#### Remove-AmlWebService 

```
#Get the Web Service named 'abc'
$webSvc = Get-AmlWebService | Where Name -eq 'abc'
#Delete it
Remove-AmlWebService -WebServiceId $webSvc.Id
```

### Manage Web Servcie Endpoint ###
#### Get-AmlWebServiceEndpoint

```
#List all endpoints of a web service named 'abc'
$webSvc = Get-AmlWebService | where Name -eq 'abc'
$endpoints = Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id
$endpoints | Format-Table
```


```
#Show metadata of the endpoint named 'ep01'
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01'
```
Please note that you can supply the Web Service Endpoint API Key as the value of the _-AuthorizationToken_ parameter (in lieu of Workspace authorization token) for this call. The same applies to the rest of the endpoint management APIs.

```
#Show metadata of the endpoint named 'ep01', where the apiKey is stored in $apiKey
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01' -AuthorizationToken $apiKey
```

#### Remove-AmlWebServiceEndpoint
```
#Find the endpoint named 'ep01'
Remove-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01'
```
#### Add-AmlWebServiceEndpoint
```
Add-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'NewEP' -Description 'New Endpoint' -ThrottleLevel 'High' -MaxConcurrentCalls 20
```

* For Free Workspace, the Throttle Level can only be set to 'Low', the default value. The supplied value for MaxConcurrentCalls is ignored and the parameter is always defaulted to 4. And the maximum number of Endpoints you can create (including the default one) on a Web Service is 3. 
* For Standard Workspace the ThrottleLevel values can be either 'Low' or 'High'. When it is set to 'Low', the supplied valueof MaxConcurrentCalls is ignored and the parameter is always defaulted to 4.

#### Refresh-AmlWebServiceEndpoint
```
#Refresh the endpoint 'ep03'
Refresh-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep03' -OverwriteResources
```
Note: Refresh endpoint essentillay takes the workflow of the latest Predicative Experiment graph and applies it to the specified non-default Endpoint. The _-OverwriteResources_ switch, when set, will also cause the trained model used in the Endpoint to be replaced with the latest one from the Predicative Experiment. When this switch is not set, the trained model is not refreshed but the rest of the graph is. Also, default Endpoint cannot be refreshed.

#### Patch-AmlWebServiceEndpoint
Patch Web Service Endpoint is used for retraining Web Service API. Essentially, you can produce a trained model and saved it in a _.ilearner_ format in Azure blob, and then replace the trianed model in an existing non-default Web Service Endpoint with this new _.ilearner_ file. For more details, please read [Retraining Machine Learning models programatically](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/) for more details.

```
#Patch an Endpoint with a new trained model.
#This is the name of the trained model in the existing Endpoint.
$resName = 'Trained Model 01'
#This is the base location of the Windows Azure storage account where the new model is stored.
$baseLoc = 'http://mystorageaccount.blob.core.windows.net'
$relativeLoc = 'mycontainer/retrain/newmodel.ilearner'
$sasToken = '?sr=b&se=2016-02-05T04......'
Patch-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep03' -ResourceName $resName -BaseLocation $baseLoc -RelativeLocation $relativeLoc -SasBlobToken $sasToken
```
### Call Azure ML Web Service APIs
#### Invoke-AmlWebServiceRRSEndpoint
```
$region = 'South Central US'
$apiLocation = 'https://ssouthcentral.services.azureml.net/workspaces'
$apiKey = ''
Invoke-AmlWebServiceRRSEndpoint -ApiLocation $apiLocation -ApiKey $apiKey -inputFile 'C:\Temp\Income.csv' -outputFile 'C:\Temp\Predication.csv'
```
#### Invoke-AmlWebServiceBESEndpoint
```
Invoke-AmlWebServiceBESEndpoint -ApiLocation -ApiKey -jsonBESConfigFile
```


