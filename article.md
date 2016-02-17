## Creating Multiple Azure ML Trained Models and Web Service Endpoints from a Single Experiment

It is a common problem when you want to build a generic machine learning workflow, train the algorithm on multiple datasets with the exact same feature set but with different feature values in order to produce models uniquely fit to a particular dataset.

Let's say you own a global franchise of bike rental business. You want to build a regression model to predict the rental demand based on historic data. Suppose you have 1,000 rental locations across the world and you have collected a dataset that include important features such as date, time, weather, traffic, etc. for each location. You could build a single model that is trained on the entire dataset indiscriminately. However, a better approach is to produce a regression model for each of your locations, since they varies in sizes, volume, geography, population size, bike-friendliness traffic configuration, etc. 

That being said, you probably do not want to create 1,000 experiments in Azure ML each representing a location. For one that's not a scalable solution. And two, it seems a awkward way to approach this since we are using essentially the exact same graph and choosing the exact same learning algorithm. The only thing different is the training dataset. 

Fortunately, we can accomplish this using [Azure ML retraining API](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/) and [Azure ML PowerShell](https://github.com/hning86/azuremlps) automation.

> Note: to make our sample run faster, I will reduce the number of locations to 10. But the exact same principles and procedures apply to 1,000 locations. The only difference is when you have 1,000 training dataset, you probably want to think of how to parallelize the following PowerShell scripts. There are many examples on PowerShell multi-threading, which is beyond the scope of this article.   

First, let's start with the [training experiment](https://gallery.cortanaanalytics.com/Experiment/Bike-Rental-Training-Experiment-1). You can find it in the [Cortana Analytics Gallery](http://gallery.cortanaanalytics.com). Open this experiment in your [Azure ML Studio](https://studio.azureml.net) Workspace. 

> Note: in order to follow along, you probably want to use a Standard Workspace because the Free Workspace has a limitation of only allowing up to 3 Endpoints, including the default one being created in a Web Service. Since we will need to create one Endpoint for each customer, you need the scale a Standard Workspace gives you.

Notice the experiment uses a _Reader_ module to read in the training dataset named _customer001.csv_ from an Azure storage account for training. Let's assume we have collected training dataset from all bike rental locations, and stored the datasets in the same blob storage with file names ranging from _rentalloc001.csv_ to _rentalloc10.csv_.

A _Web Service Output_ module is already added to the _Train Model_ module. This basically tells the systems that, after this Experiment is deployed as a Web Service, calling this Web Service will produce a Trained Model in the format of a .ilearner file. 

Also note that we have made the URL of the _Reader_ module a web service parameter. This way we can pass in different training dataset and thus producing Trained Models that are specifically trained on that particular dataset. There are other ways to do this, such as using a Web Service-parameterized SQL query to get data from a SQL Azure database, or simply pass in dataset as input of the Web Service by inserting a _Web Service Input_ module.

Now, let's just run this training experiment using the default value _rental001.csv_ as training dataset. If you view the _Visualize_ outcome of the _Evaluate_ module, you can see you get a decent performance of _AUC = 0.91_. At this point, we are ready to deploy a Web Service out of this training experiment. Let's call the deployed Web Service _Bike Rental Training_. We will later come back to this Web Service.

Next, we will now re-open the training experiment, create a predictive experiment out of it, and then deploy a scoring Web Service. We will need to make a few minor adjustment on the schema, assuming the input dataset doesn't contain the label column, and for output you only care about the instance id and the corresponding predicted value. To save yourself from the schema adjustment work, you can simply open the already prepared [predicative experiment](https://gallery.cortanaanalytics.com/Experiment/Bike-Rental-Scoring-Experiment-1) from Gallery, run it and then deploy it as a Web Service named _Bike Rental Scoring_. 

This Web Service comes with a default Endpoint. But we are not so interested in the default Endpoint that since it cannot be updated. What we need to do is to create 10 additional Endpoints, one for each location. First, we will set up our PowerShell environment:

	Import-Module .\AzureMLPS.dll
	$scoringSvc = Get-AmlWebService | where Name -eq 'Bike Rental Scoring'
	$trainingSvc = Get-AmlWebService | where Name -eq 'Bike Rental Training'
	
Then, run the following PowerShell command:

	# Create 10 endpoints on the scoring web service.
	For ($i = 1; $i -le 10; $i++){
	    $seq = $i.ToString().PadLeft(3, '0');
	    $endpointName = 'rentalloc' + $seq;
	    Write-Host ('adding endpoint ' + $endpontName + '...')
	    Add-AmlWebServiceEndpoint -WebServiceId $scoringSvc.Id -EndpointName $endpointName -Description $endpointName     
	}

Now you have created 10 endpoints, they all contain the same Trained Model trained on _customer001.csv_. The next step is to update them with models uniquely trained on each customer's individual data. But we need to produce these models first from the _Bike Rental Training_ Web Service. Let's go back to our _Bike Rental Training_ Web Service. We need to call its BES Endpoint 10 times with 10 different training datasets in order to produce 10 different models. We will leverage the _InovkeAmlWebServiceBESEndpoint_ PowerShell commandlet to accomplish this.

	# Invoke the retraining API 10 times.
	# This is the default (and the only) Endpoint on the Traing Web Service 
	$trainingSvcEp = (Get-AmlWebServiceEndpoint -WebServiceId $trainingSvc.Id)[0];
	$submitJobRequestUrl = $trainingSvcEp.ApiLocation + '/jobs?api-version=2.0';
	$apiKey = $trainingSvcEp.PrimaryKey;
	For ($i = 1; $i -le 10; $i++){
	    $seq = $i.ToString().PadLeft(3, '0');
	    $inputFileName = 'https://bostonmtc.blob.core.windows.net/hai/retrain/bike_rental/BikeRental001.csv'# + $seq + '.csv';
	    $configContent = '{ "GlobalParameters": { "URI": "' + $inputFileName + '" }, "Outputs": { "output1": { "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccount>;AccountKey=<mykey>", "RelativeLocation": "hai/retrain/bike_rental/model' + $seq + '.ilearner" } } }';
	    Write-Host ('training regression model on ' + $inputFileName + ' for rental location ' + $seq + '...');
	    Invoke-AmlWebServiceBESEndpoint -JobConfigString $configContent -SubmitJobRequestUrl $submitJobRequestUrl -ApiKey $apiKey
	}

> Please note that BES Endpoint is the only supported mode for this operation. RRS cannot be used for producing Trained Models.

As you can see above, instead of construction 10 different BES job configuration json file, we dynamically create the config string instead and feed it to the _jobConfigString_ parameter of the _InvokeAmlWebServceBESEndpoint_ commandlet, since there is really need to persist them on the disk. 

If everything goes well, after a while, you should see 10 .ilearner files, from model001.ilearner to model010.ilearner, in your Azure storage account. Now we are ready to update our 10 scoring Web Service Endpoints with these models using the _Patch-AmlWebServiceEndpoint_ commandlet. Remember again that we can only patch the non-default endpoints we programmatically created earlier. 

	# Patch the 10 endpoints with respective .ilearner models.
	$baseLoc = 'http://bostonmtc.blob.core.windows.net/'
	$sasToken = '<my_blob_sas_token>'
	For ($i = 1; $i -le 10; $i++){
	    $seq = $i.ToString().PadLeft(3, '0');
	    $endpointName = 'rentalloc' + $seq;
	    $relativeLoc = 'hai/retrain/bike_rental/model' + $seq + '.ilearner';
	    Write-Host ('Patching endpoint ' + $endpointName + '...');
	    Patch-AmlWebServiceEndpoint -WebServiceId $scoringSvc.Id -EndpointName $endpointName -ResourceName 'Bike Rental [trained model]' -BaseLocation $baseLoc -RelativeLocation $relativeLoc -SasBlobToken $sasToken
	}

This should run fairly quickly and when the execution finished, you have successfully created 10 predicative Web Service Endpoints, each contains a Trained Model uniquely trained on the dataset specific to that customer, all from a single training experiment. To verify this, you can try calling these Endpoints using _InvokeAmlWebServiceRRSEndpoint_ commandlet, feeding them with the same input data, and you should expect to see different predication results since the models are trained with different training set.

Here is the full source:
	
	Import-Module .\AzureMLPS.dll
	$scoringSvc = Get-AmlWebService | where Name -eq 'Bike Rental Scoring'
	$trainingSvc = Get-AmlWebService | where Name -eq 'Bike Rental Training'
	
	# Create 10 endpoints on the scoring web service.
	For ($i = 1; $i -le 10; $i++){
	    $seq = $i.ToString().PadLeft(3, '0');
	    $endpointName = 'rentalloc' + $seq;
	    Write-Host ('adding endpoint ' + $endpontName + '...')
	    Add-AmlWebServiceEndpoint -WebServiceId $scoringSvc.Id -EndpointName $endpointName -Description $endpointName     
	}
	
	# Invoke the retraining API 10 times to produce 10 regression models in .ilearner format.
	$trainingSvcEp = (Get-AmlWebServiceEndpoint -WebServiceId $trainingSvc.Id)[0];
	$submitJobRequestUrl = $trainingSvcEp.ApiLocation + '/jobs?api-version=2.0';
	$apiKey = $trainingSvcEp.PrimaryKey;
	For ($i = 1; $i -le 10; $i++){
	    $seq = $i.ToString().PadLeft(3, '0');
	    $inputFileName = 'https://bostonmtc.blob.core.windows.net/hai/retrain/bike_rental/BikeRental001.csv'# + $seq + '.csv';
	    $configContent = '{ "GlobalParameters": { "URI": "' + $inputFileName + '" }, "Outputs": { "output1": { "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=<myaccount>;AccountKey=<mykey>", "RelativeLocation": "hai/retrain/bike_rental/model' + $seq + '.ilearner" } } }';
	    Write-Host ('training regression model on ' + $inputFileName + ' for rental location ' + $seq + '...');
	    Invoke-AmlWebServiceBESEndpoint -JobConfigString $configContent -SubmitJobRequestUrl $submitJobRequestUrl -ApiKey $apiKey
	}
	
	# Patch the 10 endpoints with respective .ilearner models.
	$baseLoc = 'http://bostonmtc.blob.core.windows.net/'
	$sasToken = '?test'
	For ($i = 1; $i -le 10; $i++){
	    $seq = $i.ToString().PadLeft(3, '0');
	    $endpointName = 'rentalloc' + $seq;
	    $relativeLoc = 'hai/retrain/bike_rental/model' + $seq + '.ilearner';
	    Write-Host ('Patching endpoint ' + $endpointName + '...');
	    Patch-AmlWebServiceEndpoint -WebServiceId $scoringSvc.Id -EndpointName $endpointName -ResourceName 'Bike Rental [trained model]' -BaseLocation $baseLoc -RelativeLocation $relativeLoc -SasBlobToken $sasToken
	}