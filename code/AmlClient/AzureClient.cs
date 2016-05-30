using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Net;

namespace AzureMachineLearning
{
    public class AzureClient
    {
        internal X509Certificate2 ManagementCertificate { get; private set; }
        internal string SubscriptionId { get; private set; }

        public string ManagementApi { get; private set; } = "https://management.core.windows.net/";

        // https://management.core.windows.net/{subscriptionid}/cloudservices/amlsdk/resources/machinelearning/~/workspaces/

        public string ResourceApi { get; private set; }

        public AzureClient(string id, string cert)
        {
            this.SubscriptionId = id;
            this.ManagementCertificate = GetStoreCertificate(cert);
            this.ResourceApi = this.ManagementApi + this.SubscriptionId + "/cloudservices/amlsdk/resources/machinelearning/~/";
        }

        public HttpWebRequest ResourceRequest(string resource)
        {
            return ResourceRequest(resource, HttpMethod.Get);
        }

        public HttpWebRequest ResourceRequest(string resource, HttpMethod method)
        {
            var url = this.ResourceApi + resource;
            var req = this.CreateRequest(url, method);
            return req;
        }

        public HttpWebRequest CreateRequest(string reqUrl)
        {
            return CreateRequest(reqUrl, HttpMethod.Get);
        }

        public HttpWebRequest CreateRequest(string reqUrl, HttpMethod method)
        {
            HttpWebRequest httpReq = (HttpWebRequest)HttpWebRequest.Create(reqUrl);
            httpReq.Method = method.Method;
            httpReq.ContentType = "application/json";
            httpReq.Headers.Add("x-ms-version", "2014-10-01");
            httpReq.ClientCertificates.Add(this.ManagementCertificate);
            return httpReq;
        }

        private static X509Certificate2 GetStoreCertificate(string thumbprint)
        {
            List<StoreLocation> locations = new List<StoreLocation>  {
                StoreLocation.CurrentUser,
                StoreLocation.LocalMachine
            };

            foreach (var location in locations)
            {
                X509Store store = new X509Store("My", location);
                try
                {
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection certificates = store.Certificates.Find(
                      X509FindType.FindByThumbprint, thumbprint, false);
                    if (certificates.Count == 1)
                    {
                        return certificates[0];
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            throw new ArgumentException(string.Format(
              "A Certificate with Thumbprint '{0}' could not be located.",
              thumbprint));
        }

    }
}
