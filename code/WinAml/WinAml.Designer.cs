namespace WinAml
{
    partial class WinAml
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabsControl = new System.Windows.Forms.TabControl();
            this.profilePage = new System.Windows.Forms.TabPage();
            this.pubSettingsLbl = new System.Windows.Forms.LinkLabel();
            this.importBtn = new System.Windows.Forms.Button();
            this.amlWorkspaces = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.subscriptionIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.regionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ownerIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.storageAccountNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.workspaceStateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.editorLinkDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorizationTokenDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.workspaceRdfeBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pubFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.pubSettingsDs = new System.Data.DataSet();
            this.tabsControl.SuspendLayout();
            this.profilePage.SuspendLayout();
            this.amlWorkspaces.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspaceRdfeBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pubSettingsDs)).BeginInit();
            this.SuspendLayout();
            // 
            // tabsControl
            // 
            this.tabsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabsControl.Controls.Add(this.profilePage);
            this.tabsControl.Controls.Add(this.amlWorkspaces);
            this.tabsControl.Location = new System.Drawing.Point(5, 12);
            this.tabsControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabsControl.Name = "tabsControl";
            this.tabsControl.SelectedIndex = 0;
            this.tabsControl.Size = new System.Drawing.Size(1363, 496);
            this.tabsControl.TabIndex = 0;
            // 
            // profilePage
            // 
            this.profilePage.Controls.Add(this.pubSettingsLbl);
            this.profilePage.Controls.Add(this.importBtn);
            this.profilePage.Location = new System.Drawing.Point(10, 48);
            this.profilePage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.profilePage.Name = "profilePage";
            this.profilePage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.profilePage.Size = new System.Drawing.Size(1343, 438);
            this.profilePage.TabIndex = 1;
            this.profilePage.Text = "Profile";
            this.profilePage.UseVisualStyleBackColor = true;
            // 
            // pubSettingsLbl
            // 
            this.pubSettingsLbl.AutoSize = true;
            this.pubSettingsLbl.Location = new System.Drawing.Point(381, 72);
            this.pubSettingsLbl.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.pubSettingsLbl.Name = "pubSettingsLbl";
            this.pubSettingsLbl.Size = new System.Drawing.Size(347, 32);
            this.pubSettingsLbl.TabIndex = 2;
            this.pubSettingsLbl.TabStop = true;
            this.pubSettingsLbl.Text = "Download publish settings";
            this.pubSettingsLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.pubSettingsLbl_LinkClicked);
            // 
            // importBtn
            // 
            this.importBtn.Location = new System.Drawing.Point(444, 130);
            this.importBtn.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.importBtn.Name = "importBtn";
            this.importBtn.Size = new System.Drawing.Size(200, 55);
            this.importBtn.TabIndex = 1;
            this.importBtn.Text = "Import Profile";
            this.importBtn.UseVisualStyleBackColor = true;
            this.importBtn.Click += new System.EventHandler(this.importBtn_Click);
            // 
            // amlWorkspaces
            // 
            this.amlWorkspaces.Controls.Add(this.dataGridView1);
            this.amlWorkspaces.Location = new System.Drawing.Point(10, 48);
            this.amlWorkspaces.Name = "amlWorkspaces";
            this.amlWorkspaces.Padding = new System.Windows.Forms.Padding(3);
            this.amlWorkspaces.Size = new System.Drawing.Size(1343, 438);
            this.amlWorkspaces.TabIndex = 2;
            this.amlWorkspaces.Text = "Workspaces";
            this.amlWorkspaces.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.subscriptionIdDataGridViewTextBoxColumn,
            this.regionDataGridViewTextBoxColumn,
            this.descriptionDataGridViewTextBoxColumn,
            this.ownerIdDataGridViewTextBoxColumn,
            this.storageAccountNameDataGridViewTextBoxColumn,
            this.workspaceStateDataGridViewTextBoxColumn,
            this.editorLinkDataGridViewTextBoxColumn,
            this.authorizationTokenDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.workspaceRdfeBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(43, 40);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 40;
            this.dataGridView1.Size = new System.Drawing.Size(1273, 348);
            this.dataGridView1.TabIndex = 0;
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            this.idDataGridViewTextBoxColumn.HeaderText = "Id";
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            this.idDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // subscriptionIdDataGridViewTextBoxColumn
            // 
            this.subscriptionIdDataGridViewTextBoxColumn.DataPropertyName = "SubscriptionId";
            this.subscriptionIdDataGridViewTextBoxColumn.HeaderText = "SubscriptionId";
            this.subscriptionIdDataGridViewTextBoxColumn.Name = "subscriptionIdDataGridViewTextBoxColumn";
            this.subscriptionIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // regionDataGridViewTextBoxColumn
            // 
            this.regionDataGridViewTextBoxColumn.DataPropertyName = "Region";
            this.regionDataGridViewTextBoxColumn.HeaderText = "Region";
            this.regionDataGridViewTextBoxColumn.Name = "regionDataGridViewTextBoxColumn";
            this.regionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // descriptionDataGridViewTextBoxColumn
            // 
            this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
            this.descriptionDataGridViewTextBoxColumn.HeaderText = "Description";
            this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
            this.descriptionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // ownerIdDataGridViewTextBoxColumn
            // 
            this.ownerIdDataGridViewTextBoxColumn.DataPropertyName = "OwnerId";
            this.ownerIdDataGridViewTextBoxColumn.HeaderText = "OwnerId";
            this.ownerIdDataGridViewTextBoxColumn.Name = "ownerIdDataGridViewTextBoxColumn";
            this.ownerIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // storageAccountNameDataGridViewTextBoxColumn
            // 
            this.storageAccountNameDataGridViewTextBoxColumn.DataPropertyName = "StorageAccountName";
            this.storageAccountNameDataGridViewTextBoxColumn.HeaderText = "StorageAccountName";
            this.storageAccountNameDataGridViewTextBoxColumn.Name = "storageAccountNameDataGridViewTextBoxColumn";
            this.storageAccountNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // workspaceStateDataGridViewTextBoxColumn
            // 
            this.workspaceStateDataGridViewTextBoxColumn.DataPropertyName = "WorkspaceState";
            this.workspaceStateDataGridViewTextBoxColumn.HeaderText = "WorkspaceState";
            this.workspaceStateDataGridViewTextBoxColumn.Name = "workspaceStateDataGridViewTextBoxColumn";
            this.workspaceStateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // editorLinkDataGridViewTextBoxColumn
            // 
            this.editorLinkDataGridViewTextBoxColumn.DataPropertyName = "EditorLink";
            this.editorLinkDataGridViewTextBoxColumn.HeaderText = "EditorLink";
            this.editorLinkDataGridViewTextBoxColumn.Name = "editorLinkDataGridViewTextBoxColumn";
            this.editorLinkDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // authorizationTokenDataGridViewTextBoxColumn
            // 
            this.authorizationTokenDataGridViewTextBoxColumn.DataPropertyName = "AuthorizationToken";
            this.authorizationTokenDataGridViewTextBoxColumn.HeaderText = "AuthorizationToken";
            this.authorizationTokenDataGridViewTextBoxColumn.Name = "authorizationTokenDataGridViewTextBoxColumn";
            this.authorizationTokenDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // workspaceRdfeBindingSource
            // 
            this.workspaceRdfeBindingSource.DataSource = typeof(AzureMachineLearning.Workspace);
            // 
            // pubFileDialog
            // 
            this.pubFileDialog.DefaultExt = "publishsettings";
            // 
            // pubSettingsDs
            // 
            this.pubSettingsDs.DataSetName = "PublishSettingsDs";
            // 
            // WinAml
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1381, 532);
            this.Controls.Add(this.tabsControl);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "WinAml";
            this.Text = "WinAml";
            this.Load += new System.EventHandler(this.WinAml_Load);
            this.tabsControl.ResumeLayout(false);
            this.profilePage.ResumeLayout(false);
            this.profilePage.PerformLayout();
            this.amlWorkspaces.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.workspaceRdfeBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pubSettingsDs)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabsControl;
        private System.Windows.Forms.TabPage profilePage;
        private System.Windows.Forms.LinkLabel pubSettingsLbl;
        private System.Windows.Forms.Button importBtn;
        private System.Windows.Forms.OpenFileDialog pubFileDialog;
        private System.Windows.Forms.TabPage amlWorkspaces;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn subscriptionIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn regionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ownerIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn storageAccountNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn workspaceStateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn editorLinkDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorizationTokenDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource workspaceRdfeBindingSource;
        private System.Data.DataSet pubSettingsDs;
    }
}

