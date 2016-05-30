using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace AzureMachineLearning
{
    public sealed class WorkspaceConfig
    {
        public string WorkspaceId { get; set; }
        public string Location { get; set; }
    }

    public class WorkspaceSettings
    {
        public static WorkspaceConfig FromFile(string path)
        {
            if (!File.Exists(path))
                return null;

            var t = File.ReadAllText(path);
            var ws = JsonConvert.DeserializeObject<WorkspaceConfig>(t);
            return ws;
        }

        public WorkspaceSettings()
        {
        }

        public WorkspaceSettings(string id, string auth, string location)
        {
            this.WorkspaceId = id;
            this.AuthorizationToken = auth;
            this.Location = location;

            if (this.IsValid())
                SetApiUrl();
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(this.Location))
                return false;
            if (string.IsNullOrEmpty(this.WorkspaceId))
                return false;
            if (string.IsNullOrEmpty(this.AuthorizationToken))
                return false;
            return true;
        }

    }
}
