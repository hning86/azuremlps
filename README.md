# PowerShell Commandlets for Azure Machine Learning Studio & Web Service APIs
## Introduction
This is a preview of PowerShell Commandlet Library for [Azure Machine Learning](https://studio.azureml.net). It allows you to interact with Azure Machine Learning Workspace, or Workspace for short. The supported operations are:

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
This PowerShell module requires PowerShell 4.0 and .NET 4.5.1. 
Also, you need to have Owner access to at least one Azure Machine Learning Studio Workspace. You can obtain a free Workspace by simply logging in to [Azure Machine Learning Studio](https://studio.azureml.net) with any valid Microsoft account, or your School or Work email address. For more information on Azure Machine Learning, see the [Azure Machine Learning Homepage](http://www.azure.com/ml). 

## Installation
Simply download the AzureMLPS.dll, then run the PowerShell command to import the module into the current PowerShell environment:

```
Import-Module .\AzureMLPS.dll
```
## Configuration
Most of the commandlets require 3 pieces of key information in order to function:

* **Workspace ID**: this value can be found in Workspace Settings in ML Studio.
* **Workspace Authorization Code**: this value can be found in Workspace Settings in ML Studio.
* **Region Name**: this value can be found in the Workspace dropdown. Currently supported values for this configuration are:
	* South Central US (use this value for all Free Workspaces)
	* Southeast Asia
	* Northern Europe

There are 3 ways to specify these values:

1. Create a default _config.json_ file in the same folder where you are running the PowerShell commandlets. A sample should look like this:

	__*config.json*__

	```
	{
		"Configuration": 
			{
				"WorkspaceId": "12341234-1234-1234-1234-123412341234",
				"AuthorizationCode": "12341234-1234-1234-1234-123412341234",
				"RegionName": "South Central US"
			}
	}
	```
	Then you can simple execute the commandlet like this:
	
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
# Returns a list of all attributes of a Workspace
$ws = Get-AmlWorkspace
# Display the Workspace Name
$ws.FriendlyName
```

You can find the value of Workspace Id and Workspace Authorization Token after you log onto your Azure Machine Learning Studio Workspace, and navigate to the Settings section. <br/>
The currently supported regions are: 'South Central US', 'Western Europe' and 'Southeast Asia'.

### Manage Dataset
#### Get-AmlDataset
```
# List all datasets in a Workspace:
$ds = Get-AmlDataset
# Display the list in a table format
$ds | Format-Table
```

#### Download-AmlDataset
```
#Find a dataset named 'Flight Data' in the Workspace using Get-AmlDataset:
$dsFlight = Get-AmlDataset | Where $_.Name = 'Flight Data'
#Download the Flight dataset:
Download-AmlDataset -DatasetId $ds.DatasetId -DownloadFileName 'C:\Temp\FlightData.csv'
```

#### Upload-AmlDataset
```
Upload-AmlDataset -FileFormat GenericCSV -UploadFromFileName 'C:\Temp\Flight.csv' -DatasetName 'Flight Data' -Description 'Flight Data'
```

#### Remove-AmlDataset
```
#Find a dataset named 'Flight Data' in the Workspace using Get-AmlDataset:
$dsFlight = Get-AmlDataset | Where $_.Name -eq 'Flight Data'
#Delete the dataset from Workspace
Remove-AmlDataset -DatasetId $dsFlight.DatasetId
```
### Manage Experiment ###
#### Get-AmlExperiment ####

```
#Get all experiments in the Workspace
$exps = Get-AmlExperiments
#Display all experiments in a table format
$exps | Format-Table
```
```
#Get the metadata of the first experiment in the Workspace
$exp = Get-AmlExperiment -ExperimentId $exps[0].Id
#Display the experiment name
$exp.Name
```

#### Start-AmlExperiment ####
```
#Find the experiment named "Experiment001"
$exp = Get-AmlExperiment | Where Name -eq 'Experiment001'
#Run an experiment
Start-AmlExperiment -ExperimentId $exps[0].Id
```
#### Remove-AmlExperiment ####
```
#Find the experiment named "Experiment002"
$exp = Get-AmlExperiment | Where Name -eq 'Experiment002'
#Delete an experiment
Remove-AmlExperiment -ExperimentId $expId
```
#### Copy-AmlExperiment ####

```
#Find the experiment named "Experiment003"
$exp = Get-AmlExperiment | Where Name -eq 'Experiment003'
#Copy that experiment from current Workspace to another Workspace
Copy-AmlExperiment -ExperimentId $exp.Id -DestinationWorkspaceId '<ws_id>' -DestionationWorkspaceAuthToken '<auth_token>'
```
### Manage Web Service ###
#### Get-AmlWebService ####
```
#Get all web services in Workspace
$webServices = Get-AmlWebService
#Display them in table format
$webServices | Format-Table
```
```
#Get metadata of a specific web service named 'MyWebService001'
$webservice = Get-AmlWebService | Where Name -eq 'MyWebService001'
Get-AmlWebSerivce -WebServiceId $webService.Id
```
#### New-AmlWebService

```
#Get the Experiment metadata 
$exp = Get-AmlExperiment | Where .Name -eq 'Experiment004'
#Deploy Web Service from it.
New-AmlWebService -ExperimentId $exp.Id
```

#### Remove-AmlWebService 

```
#Get the Web Service named 'WebService002'
$webSvc = Get-AmlWebService | Where .Name -eq 'WebService002'
#Delete it
Remove-AmlWebService -WebServiceId $webSvc.Id
```

### Manage Web Servcie Endpoint ###
#### Get-AmlWebServiceEndpoint

```
#List all endpoints of a web service named 'WebService003'
$webSvc = Get-AmlWebService | Where .Name -eq 'WebService003'
$endpoints = Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id
$endpoints | Format-Table
```

```
#Show metadata of the endpoint named 'endpoint01'
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'endpoint01'
```
#### Remove-AmlWebServiceEndpoint
```
#Find the endpoint named 'endpoint02'
Remove-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'endpoint02'
```
#### Add-AmlWebServiceEndpoint
```
Add-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'NewEndPoint' -MaxCon
```
#### Refresh-AmlWebServiceEndpoint
```
#Refresh the endpoint 'endpoint03'
Refresh-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'endpoint03' -OverwriteResource
```
Note: Refresh endpoint essentillay takes the workflow of the latest Predicative Experiment grpah and applies to the specified endpoint. The _-OverwriteResource_ switch, when set, will also replace the trained model used in the endpoint with the latest one from the Predicative Experiment. When this siwtch is not set, the trained model is not refreshed.
#### Patch-AmlWebServiceEndpoint
```
#Update the trained model of the endpoint with a newly trained model saved in an Azure blob as a .ilearner file.
Patch-AmlWebServiceEndpoint -WebServiceId '<web_svc_id>' -EndpointName '<endpoint_name>'
```
### Call Azure ML Web Service APIs
#### Invoke-AmlWebServiceRRSEndpoint
```
Invoke-AmlWebServiceRRSEndpoint -ApiLocation $apiLocation -ApiKey $apiKey -inputFile 'C:\Temp\Income.csv' -outputFile 'C:\Temp\Predication.csv'
```
#### Invoke-AmlWebServiceBESEndpoint
```
Invoke-AmlWebServiceBESEndpoint -ApiLocation -ApiKey -jsonBESConfigFile
```


