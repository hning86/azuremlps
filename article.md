### Creating Multiple Unique Azure ML Trained Model and Web Service Endpoints from a Single Experiment

It is a common problem when you want to build a generic machine learning model, but trained on multiple datasets with the exact same schema but from different sources in order to produce models unique fit to a particular source.

Let's say you own a global franchise of bike rental business. You want to build a machine learning model to predict the demand based on historic data. Suppose you have 1,000 locations across the world and you have collected a dataset that include important features such as date, time, weather, traffic, etc. You could build a single model that is trained on the entire dataset indiscrimintoarily. However, intuitively a better approach is to produce a regression model for each of your locations, since they varies in sizes, geography, populatio size, bike-friendliness traffic configuration, etc. 

That being said, you probably do not want to create 1,000 experiments in Azure ML each representing a location. For one that's not a scalable solution. For two, it seems a awkward way to approach this since we are using essentially the exact same graph and choosing the exact same learning algorithm. The only thing different is the training dataset. 

Fortunately, we can accomplish this using [Azure ML retraining API](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/) and [Azure ML PowerShell](https://github.com/hning86/azuremlps) automation.
> Note: to make our sample run faster, I will reduce the number of locations to 10. But the exact same principles and procedures apply to 1,000 locations. The only difference is when you have 1,000 training dataset, you might want to think of how to parallelize the following PowerShell scripts. There are many examples on PowerShell multi-threading, which is beyond the scope of this article.   

First, let's start with the [training experiment](https://gallery.cortanaanalytics.com/Experiment/Bike-Rental-Training-Experiment-1). You can find it in the [Cortana Analytics Gallery](http://gallery.cortanaanalytics.com). Open this experiment in your [Azure ML Studio](https://studio.azureml.net) Workspace. 

> Note: in order to follow along, you probably want to use a Standard Workspace because the Free Workspace has a limitation of only allowing up to 3 Endpoints, including the default one being created in a Web Service. Since we will need to create one Endpoint for each customer, you need the scale a Standard Workspace gives you.

Notice the experiment uses a _Reader_ module to read in the training dataset named _customer001.csv_ from an Azure storage account for training. Let's assume we have collected training dataset from all bike rental locations, and stored the datasets in the same blob storage with file names ranging from _rentalloc001.csv_ to _rentalloc10.csv_.

A _Web Service Output_ module is already added to the _Train Model_ module. This basically tells the systems that, after this Experiment is deployed as a Web Service, calling this Web Service will produce a Trained Model in the format of a .ilearner file. 

Also note that we have made the URL of the _Reader_ module a web service parameter. This way we can pass in different training dataset and thus producing Trained Models that are specifically trained on that particular dataset. There are other ways to do this, such as using a Web Service-parameterized SQL query to get data from a SQL Azure database, or simply pass in dataset as input of the Web Service by inserting a _Web Service Input_ module.

Now, let's just run this training experiment using the default value _rental001.csv_ as training dataset. If you view the _Visualize_ outcome of the _Evaluate_ module, you can see you get a decent performance of _AUC = 0.91_. At this point, we are ready to deploy a Web Service out of this training experiment. Let's call the deployed Web Service _Bike Rental Training_. We will later come back to this Web Service.

Next, we will now create a predictive experiment out of this training experiment, and then deploy a scoring Web Service. You will need to make a few minor adjustement on the schema, assuming the input dataset doesn't contain the label column, and for output you only care about the instance id and the corresponding predicted value. To save yourself from the schema adjustment work, you can simply open the prepared [predicative experiment](http://gallery.cor.com) from Gallery, and simply run it and deploy it as a Web Service named _Bike Rental Scoring_. 

This Web Service comes with a default Endpoint. But we are not so interested in the default Endpoint that since it cannot be updated. What we need to do is to create 10 additional Endpoints, one for each location. To do that, run the following PowerShell command:

	For ($i = 1; $i -le 10; $i++) {
		$fileName = 'customer' + $i.ToString().PadLeft(3, '0') + '.csv'
		Add-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName $fileName -Description 'Income forecast for '+$fileName -ThrottleLevel High -MaxConcurrentCall 20
	}

Now you have created 10 endpoints, but they all contain the same Trained Model trained on _customer001.csv_. The next step is to update them with models uniquely trained on each customer's individual data. But we need to produce these models first, from the _Bike Rental Training_ Web Service.

Let's go back to our training experiment. First we will make the the Blob name field in the Import Data module a Web Service parameter. This way, we can pass in different training dataset. Next we will manually add a Web Service output port and attach it to the Train Model module. This allows the training experiment to produce a Trained Model as a Web Service output. We will also name the output "Trained Model". Optionally, we can also add a second Web Service Output and attach it to the Evaluate module such that we can observe the performance of each Trained Model. 

Now let's deploy this training experiment as a training Web Service. Once deployed, we can now call the BES Endpoint of this Web Service 10 times, feed it with different training dataset in each call, and produce 10 unique Trained Model as 10 .ilearner files.

> Note: Retraining API only works on BES Endpoint. It will not work on RRS Endpoint.

	For ($i = 1; $i -le 10; $i++) {
		Invoke-AmlWebServiceBESEndpoint
	}

If everything goes well, you should see 10 .ilearner files in your Azure storage account. Now we are ready to update (PATCH) our 10 scoring Web Service Endpoints with these models.

	For ($i = 1; $i -le 10; $i++) {
		Patch-AmlWebServiceEndpoint
	}

That's it. From a single training experiment, you have successfully created 10 predicative Web Service Endpoints, each contains a Trained Model uniquely trained on the dataset specific to that customer.