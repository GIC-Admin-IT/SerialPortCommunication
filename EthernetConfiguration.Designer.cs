
namespace SerialPortCommunication
{
    partial class EthernetConfiguration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EthernetConfiguration));
            this.groupBoxEthernetSettings = new System.Windows.Forms.GroupBox();
            this.txtDefaultGateway = new System.Windows.Forms.TextBox();
            this.txtSubnetMask = new System.Windows.Forms.TextBox();
            this.txtDownloadPort = new System.Windows.Forms.TextBox();
            this.txtMonitoringport = new System.Windows.Forms.TextBox();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.lblSubnetMask = new System.Windows.Forms.Label();
            this.lblDefaultGateway = new System.Windows.Forms.Label();
            this.lblMonitoringPort = new System.Windows.Forms.Label();
            this.lblDownloadPort = new System.Windows.Forms.Label();
            this.lblIPAddress = new System.Windows.Forms.Label();
            this.checkBoxDHCP = new System.Windows.Forms.CheckBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.groupBoxEthernetSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxEthernetSettings
            // 
            this.groupBoxEthernetSettings.Controls.Add(this.txtDefaultGateway);
            this.groupBoxEthernetSettings.Controls.Add(this.txtSubnetMask);
            this.groupBoxEthernetSettings.Controls.Add(this.txtDownloadPort);
            this.groupBoxEthernetSettings.Controls.Add(this.txtMonitoringport);
            this.groupBoxEthernetSettings.Controls.Add(this.txtIPAddress);
            this.groupBoxEthernetSettings.Controls.Add(this.lblSubnetMask);
            this.groupBoxEthernetSettings.Controls.Add(this.lblDefaultGateway);
            this.groupBoxEthernetSettings.Controls.Add(this.lblMonitoringPort);
            this.groupBoxEthernetSettings.Controls.Add(this.lblDownloadPort);
            this.groupBoxEthernetSettings.Controls.Add(this.lblIPAddress);
            this.groupBoxEthernetSettings.Controls.Add(this.checkBoxDHCP);
            this.groupBoxEthernetSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxEthernetSettings.Location = new System.Drawing.Point(124, 32);
            this.groupBoxEthernetSettings.Name = "groupBoxEthernetSettings";
            this.groupBoxEthernetSettings.Size = new System.Drawing.Size(553, 316);
            this.groupBoxEthernetSettings.TabIndex = 6;
            this.groupBoxEthernetSettings.TabStop = false;
            this.groupBoxEthernetSettings.Text = "Ethernet Settings";
            // 
            // txtDefaultGateway
            // 
            this.txtDefaultGateway.Location = new System.Drawing.Point(402, 101);
            this.txtDefaultGateway.Name = "txtDefaultGateway";
            this.txtDefaultGateway.Size = new System.Drawing.Size(100, 21);
            this.txtDefaultGateway.TabIndex = 11;
            this.txtDefaultGateway.Text = "192.168.0.2";
            // 
            // txtSubnetMask
            // 
            this.txtSubnetMask.Location = new System.Drawing.Point(402, 60);
            this.txtSubnetMask.Name = "txtSubnetMask";
            this.txtSubnetMask.Size = new System.Drawing.Size(100, 21);
            this.txtSubnetMask.TabIndex = 10;
            this.txtSubnetMask.Text = "255.255.248.0";
            // 
            // txtDownloadPort
            // 
            this.txtDownloadPort.Location = new System.Drawing.Point(153, 101);
            this.txtDownloadPort.Name = "txtDownloadPort";
            this.txtDownloadPort.Size = new System.Drawing.Size(100, 21);
            this.txtDownloadPort.TabIndex = 9;
            this.txtDownloadPort.Text = "5000";
            // 
            // txtMonitoringport
            // 
            this.txtMonitoringport.Location = new System.Drawing.Point(153, 150);
            this.txtMonitoringport.Name = "txtMonitoringport";
            this.txtMonitoringport.Size = new System.Drawing.Size(100, 21);
            this.txtMonitoringport.TabIndex = 8;
            this.txtMonitoringport.Text = "1100";
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(153, 60);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(100, 21);
            this.txtIPAddress.TabIndex = 7;
            this.txtIPAddress.Text = "192.168.4.4";
            // 
            // lblSubnetMask
            // 
            this.lblSubnetMask.AutoSize = true;
            this.lblSubnetMask.Location = new System.Drawing.Point(276, 63);
            this.lblSubnetMask.Name = "lblSubnetMask";
            this.lblSubnetMask.Size = new System.Drawing.Size(79, 15);
            this.lblSubnetMask.TabIndex = 6;
            this.lblSubnetMask.Text = "Subnet Mask";
            // 
            // lblDefaultGateway
            // 
            this.lblDefaultGateway.AutoSize = true;
            this.lblDefaultGateway.Location = new System.Drawing.Point(276, 104);
            this.lblDefaultGateway.Name = "lblDefaultGateway";
            this.lblDefaultGateway.Size = new System.Drawing.Size(96, 15);
            this.lblDefaultGateway.TabIndex = 5;
            this.lblDefaultGateway.Text = "Default Gateway";
            // 
            // lblMonitoringPort
            // 
            this.lblMonitoringPort.AutoSize = true;
            this.lblMonitoringPort.Location = new System.Drawing.Point(30, 153);
            this.lblMonitoringPort.Name = "lblMonitoringPort";
            this.lblMonitoringPort.Size = new System.Drawing.Size(91, 15);
            this.lblMonitoringPort.TabIndex = 4;
            this.lblMonitoringPort.Text = "Monitoring Port";
            // 
            // lblDownloadPort
            // 
            this.lblDownloadPort.AutoSize = true;
            this.lblDownloadPort.Location = new System.Drawing.Point(30, 104);
            this.lblDownloadPort.Name = "lblDownloadPort";
            this.lblDownloadPort.Size = new System.Drawing.Size(88, 15);
            this.lblDownloadPort.TabIndex = 3;
            this.lblDownloadPort.Text = "Download Port";
            // 
            // lblIPAddress
            // 
            this.lblIPAddress.AutoSize = true;
            this.lblIPAddress.Location = new System.Drawing.Point(30, 63);
            this.lblIPAddress.Name = "lblIPAddress";
            this.lblIPAddress.Size = new System.Drawing.Size(65, 15);
            this.lblIPAddress.TabIndex = 2;
            this.lblIPAddress.Text = "IP Address";
            // 
            // checkBoxDHCP
            // 
            this.checkBoxDHCP.AutoSize = true;
            this.checkBoxDHCP.Location = new System.Drawing.Point(6, 28);
            this.checkBoxDHCP.Name = "checkBoxDHCP";
            this.checkBoxDHCP.Size = new System.Drawing.Size(60, 19);
            this.checkBoxDHCP.TabIndex = 1;
            this.checkBoxDHCP.Text = "DHCP";
            this.checkBoxDHCP.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(451, 378);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(101, 38);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOK
            // 
            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(249, 378);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(101, 38);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // EthernetConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBoxEthernetSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EthernetConfiguration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EthernetConfiguration";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EthernetConfiguration_FormClosed_1);
            this.Load += new System.EventHandler(this.EthernetConfiguration_Load);
            this.groupBoxEthernetSettings.ResumeLayout(false);
            this.groupBoxEthernetSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxEthernetSettings;
        private System.Windows.Forms.TextBox txtDefaultGateway;
        private System.Windows.Forms.TextBox txtSubnetMask;
        private System.Windows.Forms.TextBox txtDownloadPort;
        private System.Windows.Forms.TextBox txtMonitoringport;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Label lblSubnetMask;
        private System.Windows.Forms.Label lblDefaultGateway;
        private System.Windows.Forms.Label lblMonitoringPort;
        private System.Windows.Forms.Label lblDownloadPort;
        private System.Windows.Forms.Label lblIPAddress;
        private System.Windows.Forms.CheckBox checkBoxDHCP;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
    }
}