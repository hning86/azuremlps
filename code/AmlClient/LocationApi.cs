using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMachineLearning
{
    public sealed class LocationApi
    {
        const string ssl = "https://";
        const string manageApiBaseUrl = "management.azureml.net/";
        const string studioApiBaseURL = "studioapi.azureml.net/api/";

        public Uri Studio { get; private set; }

        public Uri Management { get; private set; }

        public LocationApi(string location)
        {
            string key = string.Empty;
            switch (location.ToLower())
            {
                case "south central us":
                    key = "";
                    break;
                case "west europe":
                    key = "europewest.";
                    break;
                case "southeast asia":
                    key = "asiasoutheast.";
                    break;
                default:
                    throw new Exception("Unsupported location: " + location);
            }

            this.Studio = new Uri(ssl + key + studioApiBaseURL);
            this.Management = new Uri(ssl + key + manageApiBaseUrl);
        }
    }
}
