# PowerShell Module for Azure Machine Learning Studio & Web Services Beta v.0.2
## Introduction
This is a preview release of PowerShell Commandlet Library for [Azure Machine Learning](https://studio.azureml.net). It allows you to interact with Azure Machine Learning Workspace, or Workspace for short, Datasets, Experiments, Web Services and Web Service Endpoints. The supported operations are:

* __Manage Workspace__
  * Create new Workspace using a management certificate (*[New-AmlWorkspace](#new-amlworkspace)*) 
  * List all Workspaces in an Azure subscription (*[List-AmlWorkspaces](#list-amlworkspaces)*)
  * Add users to a Workspace (*[Add-AmlWorkspaceUsers](#add-amlworkspaceusers)*)
  * Get the metadata of a Workspace (*[Get-AmlWorkspace](#get-amlworkspace)*)
* __Manage Dataset__
  * List all Datasets in Workspace (*[Get-AmlDataset](#get-amldataset)*)
  * Download a Dataset file from Workspace to local file directory (*[Download-AmlDataset](#download-amldataset)*)
  * Upload a Dataset file from local file directory to Workspace (*[Upload-AmlDataset](#upload-amldataset)*)
  * Delete a Dataset file in Workspace (*[Remove-AmlDataset](#remove-amldataset)*)
* __Manage Experiment__
  * List all Experiments in Workspace (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Get the metadata of a specific Experiment (*[Get-AmlExperiment](#get-amlexperiment)*)
  * Export a specific Experiment graph to a file in JSON format (*[Export-AmlExperimentGraph](#export-amlexperimentgraph)*)
  * Import a JSON file to overwrite an existing Experiment or create a new Experiment (*[Import-AmlExperimentGraph](#import-amlexperimentgraph)*)
  * Run an Experiment (*[Start-AmlExperiment](#start-amlexperiment)*)
  * Delete an Experiment (*[Remove-AmlExperiment](#remove-amlexperiment)*)
  * Copy an Experiment from a Workspace to another Workspace within the same region (*[Copy-AmlExperiment](#copy-amlexperiment)*)
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
		* Northern Europe		
		
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
New-AmlWorkspace -AzureSubscriptionId $azureSubscriptionId -ManagementCertThumbprint -$mgmtCertThumb -WorkspaceName 'ABCD' -Location $location -storageAccountName $storageAccountName -StorageAccountKey $storageAccountKey -OwnerEmail $ownerEmail
```

For quick reference, you can create self-signed certificate using _makecert.exe_, which is a command line tool that comes with Visual Studio and/or Windows SDK. The following command creates the private key in the CurrentUser/My store, and also output the public key in a file that you can upload into Azure management portal. 

```
makecert.exe -sky exchange -r -n 'CN=My_Azure_Management_Cert' -pe -a sha1 -len 2048 -ss My 'MyAzureMgmtCert.cer'
```

Or, you can use PowerShell commandlet _New-SelfSignedCertificate_ and _Export-Certificate_

```
# Create the self-signed certificate in CurrentUser\My store.
$cert = New-SelfSignedCertificate -CertStoreLocation cert:\CurrentUser\My -Subject 'CN=My_Azure_Management_Cert' -KeySpec KeyExchange -KeyExportPolicy Exportable -HashAlgorithm SHA1 -CertStoreLocation Cert:\CurrentUser\My  
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

#### Get-AmlWorkspace

```
# Returns the metadata of a Workspace
$ws = Get-AmlWorkspace
# Display the Workspace Name
$ws.FriendlyName
```
This commandlet leverages the config.json file.

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
#Find a dataset named 'Flight Data' in the Workspace using Get-AmlDataset:
$dsFlight = Get-AmlDataset | where Name -eq 'Flight Data'
#Delete the dataset from Workspace
Remove-AmlDataset -DatasetFamilyId $dsFlight.FamilyId
```
### Manage Experiment
#### Get-AmlExperiment 

```
#Get all Experiments in the Workspace
$exps = Get-AmlExperiment
#Display all Experiments in a table format
$exps | Format-Table
```
```
#Get the metadata of the Experiment named 'xyz' in the Workspace
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Display the Experiment status
$exp.Status.StatusCode
```

#### Export-AmlExperimentGraph
```
#Export an Experiment named "xyz" as a "MyExp.json" file
$exp = Get-AmlExperiment | where Description -eq 'xyz'
Export-AmlExperimentGraph -ExperimentId $exp.ExperimentId -OutputFile 'MyExp.json'
```
Please note that the exported JSON file only contains references to the exact instance and version of the assets (modules, trained models, datasets, etc.). The assets themselves are NOT serialized into the JSON file. As a consequence, when you import it back into the Workspace, make sure the exact same instance and version of those assets do exist in the Workspace, otherwise you will not be able to create a valid Experiment.

#### Import-AmlExperimentGraph
```
#Import a JSON file 'MyExp.json' to overwrite the Experiment where the file is exported out of
Import-AmlExperimentGraph -InputFile 'MyExp.json' -Overwrite
#Import a JSON file 'MyExp.json' to create a new Experiment named 'abc'
Import-AmlExperimentGraph -InputFile 'MyExp.json' -NewName 'abc'
```


#### Start-AmlExperiment
```
#Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Run the Experiment
Start-AmlExperiment -ExperimentId $exp.ExperimentId
```
#### Remove-AmlExperiment
```
#Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Delete the Experiment
Remove-AmlExperiment -ExperimentId $exp.ExperimentId
```
#### Copy-AmlExperiment

```
#Find the Experiment named "xyz"
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Copy that Experiment from current Workspace to another Workspace
Copy-AmlExperiment -ExperimentId $exp.ExperimentId -DestinationWorkspaceId '<ws_id>' -DestinationWorkspaceAuthorizationToken '<auth_token>'
```
Please note that the current Workspace and the destination Workspace must be in the same region. Cross-region copy is currently not supported.

### Manage Web Service

#### Get-AmlWebService
```
#Get all Web Services in Workspace
$webServices = Get-AmlWebService
#Display them in table format
$webServices | Format-Table Id,Name,EndpointCount
```

```
#Get metadata of a specific Web Service with Id stored in $webSvcId
Get-AmlWebService -WebServiceId $webSvcId
```

#### New-AmlWebService

This commandlet deploys a new Web Service with a default endpoint from a Predictive Experiment.

```
#Get the Predictive Experiment metadata 
$exp = Get-AmlExperiment | where Description -eq 'xyz'
#Deploy Web Service from the Predictive Experiment
$webService = New-AmlWebService -PredictiveExperimentId $exp.ExperimentId
#Display newly created Web Service
$webService
```

<span style="color:red">Known issue: calling _New-AmlWebService_ will produce a new copy of the predictive experiment as well as a new copy of web service. This is a server side issue that will be addressed soon.</span>


#### Remove-AmlWebService 

```
#Get the Web Service named 'abc'
$webSvc = Get-AmlWebService | Where Name -eq 'abc'
#Delete the Web Service
Remove-AmlWebService -WebServiceId $webSvc.Id
```

### Manage Web Servcie Endpoint ###

#### Get-AmlWebServiceEndpoint

```
#List all Endpoints of a Web Service named 'abc'
$webSvc = Get-AmlWebService | where Name -eq 'abc'
$endpoints = Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id
$endpoints | Format-Table
```


```
#Show metadata of the Endpoint named 'ep01'
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01'
```
Please note that you can supply the Web Service Endpoint API Key as the value of the _-AuthorizationToken_ parameter in lieu of Workspace authorization token for this call. The same applies to the rest of the Endpoint management APIs.

```
#Show metadata of the endpoint named 'ep01', where the apiKey is stored in $apiKey
Get-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep01' -AuthorizationToken $apiKey
```

#### Remove-AmlWebServiceEndpoint
```
#Delete the Endpoint named 'ep01'
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
Refreshing Endpoint essentially takes the graph of the latest parent Predictive Experiment and applies it to the specified non-default Endpoint. The _-OverwriteResources_ switch, when set, also causes the Trained Model used in the Endpoint to be replaced with the latest one from the Predictive Experiment. When this switch left unset, the Trained Model is not refreshed but the rest of the graph is. Also, default Endpoint cannot be refreshed.

```
#Refresh the endpoint 'ep03'
Refresh-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName 'ep03' -OverwriteResources
```

#### Patch-AmlWebServiceEndpoint
Patch Web Service Endpoint is used for updating a trained model in an existing Endpoint. Essentially, you can produce a Trained Model and save it in a _.ilearner_ format in an Azure storage account as a blob. You can accomplish that by calling the BES endpoint using [_Invoke-AmlWebServiceBESEndpoint_](#invoke-amlwebservicebesendpoint) commandlet on the training Web Service. And then you can use _Patch-AmlWebServiceEndpoint_ commandlet to replace a specified Trained Model in an existing non-default Web Service Endpoint with this new _.ilearner_ file. Please browse [Retraining Machine Learning models programatically](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/) for more details.

```
#The name of the Trained Model in the existing Endpoint you are trying to patch. 
#You can obtain this from the Trained Model module in the Predictive Experiment graph, 
#or through the Resources field in the returned result of Get-WebServiceEndpoint commandlet.
$resName = 'Income Predictor [Trained Model]'
#This is the base location of the Windows Azure storage account where the new model is stored as a .ilearner file.
$baseLoc = 'http://mystorageaccount.blob.core.windows.net'
#The relative location of the .ilearner file, basically the container name and the path to the blob.
$relativeLoc = 'mycontainer/retrain/new_model.ilearner'
# The SAS token on the ilearner file to allow Read access.
$sasToken = '?sr=b&se=2016-02-05T04......'
#Web Service Id
$webSvcId = (Get-AmlWebService | where Name -eq 'xyz').Id
#endpoint name
$epName = 'ep02'
#Update the Endpoint with the new Trained Model.
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
#POST Request URL for RRS Endpoint 'abc' on Web Service 'xyz'
$webSvcId = (Get-AmlWebService | where Name -eq 'xyz').Id
$ep = Get-AmlWebServiceEndpoint -WebServiceId $webSvcId -EndpointName 'abc'
$postUrl =  $ep.ApiLocation + '/execute?api-version=2.0&details=true'
#Base-64 encoded API key
$apiKey = $ep.PrimaryKey
#Invoke RRS Endpoint
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
#Find the submit job request Url for BES Endpoint 'abc' on Web Service 'xyz'
$webSvcId = (Get-AmlWebService | where Name -eq 'xyz').Id
$ep = Get-AmlWebServiceEndpoint -WebServiceId $webSvcId -EndpointName 'abc'
$jobSubmitUrl =  $ep.ApiLocation + '/jobs?api-version=2.0'
#Base-64 encoded API key
$apiKey = $ep.PrimaryKey
#Invoke BES Endpoint
Invoke-AmlWebServiceBESEndpoint -SubmitJobRequestUrl $jobSubmitUrl -ApiKey $apiKey -JobConfigFile '.\jobConfig.json'
```


