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
using AzureMachineLearning;

namespace AzureMachineLearning.PowerShell
{
    public class AzureMLPsCmdletBase : PSCmdlet
    {
        protected AzureClient Azure { get; set; }              
        protected WorkspaceClient Workspace { get; set; }

        public AzureMLPsCmdletBase()
        {            

        }
    }

    public class AmlCmdlet : AzureMLPsCmdletBase
    {
        // default config.json file path.
        private string _configFilePath = "./config.json";

        [Parameter(Mandatory = false)]
        [ValidateSet("South Central US", "West Europe", "Southeast Asia")]
        public string Location { get; set; }

        [Parameter(Mandatory = false)]
        public string ConfigFile
        {
            get { return _configFilePath; }
            set
            {
                _configFilePath = value;
              
                //ReadConfigFromFile(true);
            }
        }

        [Parameter(Mandatory = false)]
        public string WorkspaceId { get; set; }

        [Parameter(Mandatory = false)]
        private string _authToken = string.Empty;

        [Parameter(Mandatory = false)]
        public string AuthorizationToken;

        public AmlCmdlet()
        {            
//            ReadConfigFromFile(false);
        }

        //private void ReadConfigFromFile(bool throwExceptionIfFileDoesnotExist)
        //{
        //    string currentPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //    _configFilePath = Path.Combine(currentPath, _configFilePath);
        //    var ws = WorkspaceConfig.FromFile(_configFilePath);

        //    if (throwExceptionIfFileDoesnotExist && ws == null)
        //        throw new Exception("Can't find config file: " + _configFilePath);

        //    Location = ws.Location;
        //    WorkspaceId = ws.WorkspaceId;
        //    AuthorizationToken = ws.AuthorizationToken;
        //}

        //protected WorkspaceSettings GetWorkspaceSetting()
        //{
        //    var ws = new WorkspaceSettings(this.WorkspaceId, this.AuthorizationToken, this.Location);
        //    return ws;
        //}                                           
    }    
}
