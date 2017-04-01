using System;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.Web.Script.Serialization;
using System.Web;
using AzureML.Contract;
using AzureML;

namespace AzureML.PowerShell
{
    public class AzureMLPsCmdletBase : PSCmdlet
    {        
        protected DataContractJsonSerializer ser;        
        protected ManagementSDK Sdk { get; private set; }
        public AzureMLPsCmdletBase()
        {            
            Sdk = new ManagementSDK("powershell_" + AzureML.ManagementSDK.Version);
        }        
    }

    public class AzureMLPsCmdlet : AzureMLPsCmdletBase
    {
        // default config.json file path.
        private string _configFilePath = "./config.json";
        [Parameter(Mandatory = false)]
        [ValidateSet("South Central US", "West Europe", "Southeast Asia", "Japan East", "Germany Central", "Integration Test", "West Central US")]
        public string Location { get; set; }
        [Parameter(Mandatory = false)]
        public string ConfigFile
        {
            get { return _configFilePath; }
            set
            {
                _configFilePath = value;
              
                ReadConfigFromFile(true);
            }
        }
        [Parameter(Mandatory = false)]
        public string WorkspaceId { get; set; }
        [Parameter(Mandatory = false)]
        private string _authToken = string.Empty;
        [Parameter(Mandatory = false)]
        public string AuthorizationToken;
        [Parameter(Mandatory = false)]
        public SwitchParameter IntegrationTest;
        public AzureMLPsCmdlet()
        {            
            ReadConfigFromFile(false);
        }        

        private void ReadConfigFromFile(bool throwExceptionIfFileDoesnotExist)
        {   
            string currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _configFilePath = Path.Combine(currentPath, _configFilePath);            
            if (throwExceptionIfFileDoesnotExist && (!File.Exists(_configFilePath)))
                throw new Exception("Can't find config file: " + _configFilePath);
            if (File.Exists(_configFilePath))
            {
                string configString = File.ReadAllText(_configFilePath);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                WorkspaceSetting config = jss.Deserialize<WorkspaceSetting>(configString);
                Location = config.Location;
                WorkspaceId = config.WorkspaceId;
                AuthorizationToken = config.AuthorizationToken;
            }
        }

        protected WorkspaceSetting GetWorkspaceSetting()
        {
            WorkspaceSetting setting = new WorkspaceSetting
            {
                WorkspaceId = WorkspaceId,
                AuthorizationToken = AuthorizationToken,
                Location = Location
            };
            return setting;
        }                                           
        protected WorkspaceSetting GetWorkspaceSetting( string location, string wsId, string wsAuthToken)
        {
            WorkspaceSetting setting = new WorkspaceSetting
            {
                WorkspaceId = wsId,
                AuthorizationToken = wsAuthToken,
                Location = location
            };
            return setting;
        }
    }    
}
