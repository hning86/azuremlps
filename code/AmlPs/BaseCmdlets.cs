using System;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Web.Script.Serialization;
using System.Web;
using System.Diagnostics;
using Microsoft.PowerShell.Commands;
using AzureMachineLearning;
using Microsoft.WindowsAzure.Commands.Profile;
using Microsoft.WindowsAzure.Commands.Profile.Models;

namespace AzureMachineLearning.PowerShell
{
    public class AmlCmdlet : PSCmdlet
    {
        public const string WorkspaceStorageClientParameter = "_AmlWorkspaceStorageClient";
        public const string WorkspaceClientParameter = "_AmlWorkspaceClient";
        public const string AzureClientParameter = "_AmlAzureClient";

        protected AzureClient AzureClient { get; private set; }
        protected WorkspaceStorageClient StorageClient { get; private set; }
        protected WorkspaceClient WorkspaceClient { get; private set; }

        protected void SetAzureClient(AzureClient client)
        {
            AzureClient = client;
            SessionState.PSVariable.Set(AzureClientParameter, AzureClient);
        }

        protected AzureClient GetAzureClient()
        {
            // check property
            if (AzureClient != null)
                return AzureClient;

            // try to get from session variables
            try
            {
                var client = (AzureClient)SessionState.PSVariable.GetValue(AzureClientParameter);
                return client;
            }
            catch { }

            // if still here, try to set from current subscription
            // suggest "Import-AzurePublishSettings" in error
            var azs = new GetAzureSubscriptionCommand();
            azs.ExtendedDetails = true;

            foreach (var sub in azs.Invoke<PSAzureSubscriptionExtended>())
            {
                if (sub.IsCurrent)
                {
                    var id = sub.SubscriptionId;
                    var cert = sub.Certificate;
                    var client = new AzureClient(id, cert);
                    return client;
                }
            }

            throw new AmlException("no current Azure subscription, 'Add-AzureAccount' or use 'Import-AzurePublishSettingsFile'");
        }

        protected WorkspaceStorageClient GetWorkspaceStorageClient()
        {
            try
            {
                var c = (WorkspaceStorageClient)SessionState.PSVariable.GetValue(WorkspaceStorageClientParameter);
                return c;
            }
            catch { }

            var azureClient = GetAzureClient();

            var client = new WorkspaceStorageClient(AzureClient);
            SessionState.PSVariable.Set(WorkspaceStorageClientParameter, client);
            return client;
        }

    }
}
