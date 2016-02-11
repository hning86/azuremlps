### Creating Multiple Unique Azure ML Trained Model and Web Service Endpoints from a Single Experiment

It is a common problem when you want to build a generic machine learning model, but trained on multipel

Let's say you want to build a machine learning model to predict . Suppose you have 1,000 restaurants as your customers. You can probably collect a lot of historical data and build a machine learning model in order to predict monthly income. 

However, since each restaurant varies in sizes, locations, and caters to different customer bases, it is probably not a good idea to apply the same model to all restaurant blindly, unless you are able to capture various relevant features that uniquely describe each restaurant and incorporate them into your trainign data. Instead, it is simpler and preferable to train the model using data specific to that restaurant, thus creating a customized trained model uniquely fitted for that particular restaurant. 

Furthermore, you probably do not want to create 1,000 experiments in Azure ML either, since we are using the exact same graph and choosing the exact same learning algorithm. The only thing different is the training dataset. 

Well, it turns out that we can accomplish this using [Azure ML retraining API](https://azure.microsoft.com/en-us/documentation/articles/machine-learning-retrain-models-programmatically/) and [Azure ML PowerShell](https://github.com/hning86/azuremlps) automation.
> Note: to make our sample run faster, I will reduce the number of customers to 10. But the exact same principles and procedures apply to 1,000 customers. The only difference is when you have 1,000 customers, you might want to think of how to parallelize the following PowerShell scripts. There are many examples on PowerShell multi-threading, which is beyond the scope of this article.   

First, let's start with the [template experiment](http://gallery.cortanaanalytics.com). You can find it in the [Cortana Analytics Gallery](http://gallery.cortanaanalytics.com). Open this experiment in your [Azure ML Studio](https://studio.azureml.net) Workspace. 

> Note: in order to follow along, you probably want to use a Standard Workspace because the Free Workspace has a limitation of only allowing up to 3 Endpoints, including the default one being created in a Web Service. Since we will need to create one Endpoint for each customer, you need the scale a Standard Workspace gives you.

Notice the experiment uses a Import Data module to read in the training dataset named _customer001.csv_ from an Azure storage account for training. Let's assume we have collected training dataset on all of our customers, and stored the datasets in the same location with file names ranging from _customer001.csv_ to _customer10.csv_.

Now, let's just run this training experiment using _customer001.csv_ as training dataset. As you can see you get a decent result with AUC = 0.91. Assuming that's satisfactory, we will now create a predictive experiment out of the training experiment, and then deploy a scoring Web Service. This Web Service comes with a default Endpoint. But we are not so interested in the default Endpoint that since it cannot be updated. What we need to do is to create 10 additional Endpoints, one for each restaurant. To do that, run the following PowerShell command:

	For ($i = 1; $i -le 10; $i++) {
		$fileName = 'customer' + $i.ToString().PadLeft(3, '0') + '.csv'
		Add-AmlWebServiceEndpoint -WebServiceId $webSvc.Id -EndpointName $fileName -Description 'Income forecast for '+$fileName -ThrottleLevel High -MaxConcurrentCall 20
	}

Now you have created 10 endpoints, but they all contain the same Trained Model trained on _customer001.csv_. The next step is to update them with models uniquely trained on each customer's individual data.

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