using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace WinAml
{
    public partial class WinAml : Form
    {
        internal string DefaultPublishSettings { get; private set; }

        internal AzureInfo AzureInfo { get; private set; }

        public WinAml()
        {
            InitializeComponent();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void pubSettingsLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           // e.Link.LinkData
        }

        // https://manage.windowsazure.com/publishsettings/index?client=xplat

        const string PubSettings = "https://manage.windowsazure.com/publishsettings/index?client=xplat"; 
        private void WinAml_Load(object sender, EventArgs e)
        {
            this.pubSettingsLbl.Links.Add(0, 1, PubSettings);

            var user = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var downloads = Path.Combine(user, "Downloads");
            var files = Directory.GetFiles(downloads);
            if (files.Length == 1)
            {
                this.DefaultPublishSettings = files[0];
            }

            this.pubFileDialog.InitialDirectory = downloads;
        }

        private void importBtn_Click(object sender, EventArgs e)
        {
            if (this.DefaultPublishSettings == null)
            {
                var result = this.pubFileDialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;

                this.DefaultPublishSettings = this.pubFileDialog.FileName;
            }

            this.AzureInfo = GetAzureInfo();
        }

        AzureInfo GetAzureInfo()
        {
            var xml = new XmlDocument();
            xml.Load(this.DefaultPublishSettings);
            this.pubSettingsDs.ReadXml(this.DefaultPublishSettings);
            return null;
        }
    }

    sealed class AzureInfo
    {
        public string SubId { get; set; }
        public string Thumb { get; set; }
    }
}
