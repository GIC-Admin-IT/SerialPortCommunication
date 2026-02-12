namespace SerialPortCommunication
{
    partial class Download
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Download));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.cmbxFont = new System.Windows.Forms.CheckBox();
            this.cmbxApplication = new System.Windows.Forms.CheckBox();
            this.cmbxLadder = new System.Windows.Forms.CheckBox();
            this.cmbxFirmare = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.rtxtDataArea = new System.Windows.Forms.RichTextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.grpBoxUSB = new System.Windows.Forms.GroupBox();
            this.cmbxUSBPortDetails = new System.Windows.Forms.ComboBox();
            this.lblPortDetails = new System.Windows.Forms.Label();
            this.grpBoxEthernet = new System.Windows.Forms.GroupBox();
            this.txtBoxPortNumber = new System.Windows.Forms.TextBox();
            this.txtBoxIPAddress = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnUpload = new System.Windows.Forms.Button();
            this.grpBoxCOMPort = new System.Windows.Forms.GroupBox();
            this.cmbPortName = new System.Windows.Forms.ComboBox();
            this.cmbStopBit = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbDataBits = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbParity = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cmbBaudRate = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBoxFile = new System.Windows.Forms.GroupBox();
            this.btnConfigure = new System.Windows.Forms.Button();
            this.txtMacID = new System.Windows.Forms.TextBox();
            this.lblethernetFile = new System.Windows.Forms.Label();
            this.btnethernet = new System.Windows.Forms.Button();
            this.txtBoxEthernetFile = new System.Windows.Forms.TextBox();
            this.lblFontPath = new System.Windows.Forms.Label();
            this.lblLadPath = new System.Windows.Forms.Label();
            this.BtnFontFileBrowse = new System.Windows.Forms.Button();
            this.txtFontFilePath = new System.Windows.Forms.TextBox();
            this.BtnLadderBrowse = new System.Windows.Forms.Button();
            this.txtLadderFilePath = new System.Windows.Forms.TextBox();
            this.lblAppPath = new System.Windows.Forms.Label();
            this.lblFirmPath = new System.Windows.Forms.Label();
            this.btnApplicationBrowse = new System.Windows.Forms.Button();
            this.txtAppFilePath = new System.Windows.Forms.TextBox();
            this.btnFirmwareBrowse = new System.Windows.Forms.Button();
            this.txtFirmwareFilePath = new System.Windows.Forms.TextBox();
            this.chbxMacID = new System.Windows.Forms.CheckBox();
            this.chkBoxFontFile = new System.Windows.Forms.CheckBox();
            this.chkBoxApplicationFile = new System.Windows.Forms.CheckBox();
            this.chkBoxLadderFile = new System.Windows.Forms.CheckBox();
            this.chkBoxFirmawareFile = new System.Windows.Forms.CheckBox();
            this.chkboxEthernetSettings = new System.Windows.Forms.CheckBox();
            this.checkBoxRTCSync = new System.Windows.Forms.CheckBox();
            this.panelUpload = new System.Windows.Forms.Panel();
            this.statusStrip1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.grpBoxUSB.SuspendLayout();
            this.grpBoxEthernet.SuspendLayout();
            this.grpBoxCOMPort.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBoxFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.AllowItemReorder = true;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 466);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(745, 28);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(530, 23);
            this.toolStripStatusLabel1.Text = "Ready";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Padding = new System.Windows.Forms.Padding(3);
            this.toolStripProgressBar1.Size = new System.Drawing.Size(156, 22);
            this.toolStripProgressBar1.Step = 1;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(17, 23);
            this.toolStripStatusLabel2.Text = "%";
            // 
            // listBox1
            // 
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Items.AddRange(new object[] {
            "Serial",
            "USB",
            "Ethernet"});
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(109, 466);
            this.listBox1.TabIndex = 9;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.ListBox1_SelectedIndexChanged);
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(128, 96);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(121, 21);
            this.comboBox2.TabIndex = 0;
            this.comboBox2.Text = "Select Device";
            // 
            // groupBox10
            // 
            this.groupBox10.Location = new System.Drawing.Point(0, 0);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(200, 100);
            this.groupBox10.TabIndex = 0;
            this.groupBox10.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(104, 267);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Firmware File";
            this.label1.Visible = false;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.cmbxFont);
            this.groupBox7.Controls.Add(this.cmbxApplication);
            this.groupBox7.Controls.Add(this.cmbxLadder);
            this.groupBox7.Controls.Add(this.cmbxFirmare);
            this.groupBox7.Location = new System.Drawing.Point(152, 169);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(303, 74);
            this.groupBox7.TabIndex = 17;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Options";
            // 
            // cmbxFont
            // 
            this.cmbxFont.Location = new System.Drawing.Point(0, 0);
            this.cmbxFont.Name = "cmbxFont";
            this.cmbxFont.Size = new System.Drawing.Size(104, 24);
            this.cmbxFont.TabIndex = 0;
            // 
            // cmbxApplication
            // 
            this.cmbxApplication.Location = new System.Drawing.Point(0, 0);
            this.cmbxApplication.Name = "cmbxApplication";
            this.cmbxApplication.Size = new System.Drawing.Size(104, 24);
            this.cmbxApplication.TabIndex = 1;
            // 
            // cmbxLadder
            // 
            this.cmbxLadder.Location = new System.Drawing.Point(0, 0);
            this.cmbxLadder.Name = "cmbxLadder";
            this.cmbxLadder.Size = new System.Drawing.Size(104, 24);
            this.cmbxLadder.TabIndex = 2;
            // 
            // cmbxFirmare
            // 
            this.cmbxFirmare.Location = new System.Drawing.Point(0, 0);
            this.cmbxFirmare.Name = "cmbxFirmare";
            this.cmbxFirmare.Size = new System.Drawing.Size(104, 24);
            this.cmbxFirmare.TabIndex = 3;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.btnClear);
            this.groupBox6.Controls.Add(this.rtxtDataArea);
            this.groupBox6.Location = new System.Drawing.Point(622, 16);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(227, 268);
            this.groupBox6.TabIndex = 16;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Debug info";
            this.groupBox6.Visible = false;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(32, 221);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 12;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // rtxtDataArea
            // 
            this.rtxtDataArea.Location = new System.Drawing.Point(20, 23);
            this.rtxtDataArea.Name = "rtxtDataArea";
            this.rtxtDataArea.Size = new System.Drawing.Size(173, 191);
            this.rtxtDataArea.TabIndex = 1;
            this.rtxtDataArea.Text = "";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(97, 156);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 15);
            this.label8.TabIndex = 2;
            this.label8.Text = "Port No -";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(97, 85);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 15);
            this.label7.TabIndex = 1;
            this.label7.Text = "IP Address  - ";
            // 
            // grpBoxUSB
            // 
            this.grpBoxUSB.BackColor = System.Drawing.SystemColors.Control;
            this.grpBoxUSB.Controls.Add(this.cmbxUSBPortDetails);
            this.grpBoxUSB.Controls.Add(this.lblPortDetails);
            this.grpBoxUSB.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpBoxUSB.Location = new System.Drawing.Point(152, 12);
            this.grpBoxUSB.Name = "grpBoxUSB";
            this.grpBoxUSB.Size = new System.Drawing.Size(561, 152);
            this.grpBoxUSB.TabIndex = 41;
            this.grpBoxUSB.TabStop = false;
            this.grpBoxUSB.Text = "USB Port Settings";
            // 
            // cmbxUSBPortDetails
            // 
            this.cmbxUSBPortDetails.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbxUSBPortDetails.FormattingEnabled = true;
            this.cmbxUSBPortDetails.Location = new System.Drawing.Point(192, 65);
            this.cmbxUSBPortDetails.Name = "cmbxUSBPortDetails";
            this.cmbxUSBPortDetails.Size = new System.Drawing.Size(176, 23);
            this.cmbxUSBPortDetails.TabIndex = 17;
            // 
            // lblPortDetails
            // 
            this.lblPortDetails.AutoSize = true;
            this.lblPortDetails.Location = new System.Drawing.Point(125, 69);
            this.lblPortDetails.Name = "lblPortDetails";
            this.lblPortDetails.Size = new System.Drawing.Size(66, 15);
            this.lblPortDetails.TabIndex = 16;
            this.lblPortDetails.Text = "Port Name";
            // 
            // grpBoxEthernet
            // 
            this.grpBoxEthernet.BackColor = System.Drawing.SystemColors.Control;
            this.grpBoxEthernet.Controls.Add(this.txtBoxPortNumber);
            this.grpBoxEthernet.Controls.Add(this.txtBoxIPAddress);
            this.grpBoxEthernet.Controls.Add(this.label6);
            this.grpBoxEthernet.Controls.Add(this.label9);
            this.grpBoxEthernet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpBoxEthernet.Location = new System.Drawing.Point(152, 12);
            this.grpBoxEthernet.Name = "grpBoxEthernet";
            this.grpBoxEthernet.Size = new System.Drawing.Size(581, 160);
            this.grpBoxEthernet.TabIndex = 42;
            this.grpBoxEthernet.TabStop = false;
            this.grpBoxEthernet.Text = "Ethernet Port Settings";
            // 
            // txtBoxPortNumber
            // 
            this.txtBoxPortNumber.Location = new System.Drawing.Point(253, 101);
            this.txtBoxPortNumber.Name = "txtBoxPortNumber";
            this.txtBoxPortNumber.Size = new System.Drawing.Size(193, 21);
            this.txtBoxPortNumber.TabIndex = 14;
            this.txtBoxPortNumber.Text = "5000";
            this.txtBoxPortNumber.Visible = false;
            this.txtBoxPortNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBoxPortNumber_KeyPress);
            this.txtBoxPortNumber.Leave += new System.EventHandler(this.txtBoxPortNumber_Leave);
            // 
            // txtBoxIPAddress
            // 
            this.txtBoxIPAddress.Location = new System.Drawing.Point(253, 56);
            this.txtBoxIPAddress.Name = "txtBoxIPAddress";
            this.txtBoxIPAddress.Size = new System.Drawing.Size(193, 21);
            this.txtBoxIPAddress.TabIndex = 13;
            this.txtBoxIPAddress.Text = "192.168.4.5";
            this.txtBoxIPAddress.Leave += new System.EventHandler(this.txtBoxIPAddress_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(109, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 15);
            this.label6.TabIndex = 12;
            this.label6.Text = "Port Number";
            this.label6.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(109, 56);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 15);
            this.label9.TabIndex = 11;
            this.label9.Text = "IP Address";
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(211, 20);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(75, 23);
            this.btnUpload.TabIndex = 0;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // grpBoxCOMPort
            // 
            this.grpBoxCOMPort.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.grpBoxCOMPort.BackColor = System.Drawing.SystemColors.Control;
            this.grpBoxCOMPort.Controls.Add(this.cmbPortName);
            this.grpBoxCOMPort.Controls.Add(this.cmbStopBit);
            this.grpBoxCOMPort.Controls.Add(this.label10);
            this.grpBoxCOMPort.Controls.Add(this.cmbDataBits);
            this.grpBoxCOMPort.Controls.Add(this.label11);
            this.grpBoxCOMPort.Controls.Add(this.cmbParity);
            this.grpBoxCOMPort.Controls.Add(this.label12);
            this.grpBoxCOMPort.Controls.Add(this.cmbBaudRate);
            this.grpBoxCOMPort.Controls.Add(this.label13);
            this.grpBoxCOMPort.Controls.Add(this.label14);
            this.grpBoxCOMPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpBoxCOMPort.Location = new System.Drawing.Point(152, 12);
            this.grpBoxCOMPort.Name = "grpBoxCOMPort";
            this.grpBoxCOMPort.Size = new System.Drawing.Size(571, 160);
            this.grpBoxCOMPort.TabIndex = 42;
            this.grpBoxCOMPort.TabStop = false;
            this.grpBoxCOMPort.Text = "Com Port Settings";
            // 
            // cmbPortName
            // 
            this.cmbPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPortName.FormattingEnabled = true;
            this.cmbPortName.Location = new System.Drawing.Point(192, 65);
            this.cmbPortName.Name = "cmbPortName";
            this.cmbPortName.Size = new System.Drawing.Size(176, 23);
            this.cmbPortName.TabIndex = 15;
            this.cmbPortName.SelectedIndexChanged += new System.EventHandler(this.cmbPortName_SelectedIndexChanged);
            // 
            // cmbStopBit
            // 
            this.cmbStopBit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStopBit.FormattingEnabled = true;
            this.cmbStopBit.Items.AddRange(new object[] {
            "1",
            "2",
            "3"});
            this.cmbStopBit.Location = new System.Drawing.Point(379, 72);
            this.cmbStopBit.Name = "cmbStopBit";
            this.cmbStopBit.Size = new System.Drawing.Size(121, 23);
            this.cmbStopBit.TabIndex = 19;
            this.cmbStopBit.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(125, 67);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(66, 15);
            this.label10.TabIndex = 10;
            this.label10.Text = "Port Name";
            // 
            // cmbDataBits
            // 
            this.cmbDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataBits.FormattingEnabled = true;
            this.cmbDataBits.Items.AddRange(new object[] {
            "7",
            "8",
            "9"});
            this.cmbDataBits.Location = new System.Drawing.Point(379, 41);
            this.cmbDataBits.Name = "cmbDataBits";
            this.cmbDataBits.Size = new System.Drawing.Size(121, 23);
            this.cmbDataBits.TabIndex = 18;
            this.cmbDataBits.Visible = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(65, 76);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 15);
            this.label11.TabIndex = 11;
            this.label11.Text = "Baud Rate";
            this.label11.Visible = false;
            // 
            // cmbParity
            // 
            this.cmbParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbParity.FormattingEnabled = true;
            this.cmbParity.Items.AddRange(new object[] {
            "None ",
            "Even",
            "Odd"});
            this.cmbParity.Location = new System.Drawing.Point(132, 103);
            this.cmbParity.Name = "cmbParity";
            this.cmbParity.Size = new System.Drawing.Size(121, 23);
            this.cmbParity.TabIndex = 17;
            this.cmbParity.Visible = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(65, 107);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(37, 15);
            this.label12.TabIndex = 12;
            this.label12.Text = "Parity";
            this.label12.Visible = false;
            // 
            // cmbBaudRate
            // 
            this.cmbBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBaudRate.FormattingEnabled = true;
            this.cmbBaudRate.Items.AddRange(new object[] {
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.cmbBaudRate.Location = new System.Drawing.Point(132, 72);
            this.cmbBaudRate.Name = "cmbBaudRate";
            this.cmbBaudRate.Size = new System.Drawing.Size(121, 23);
            this.cmbBaudRate.TabIndex = 16;
            this.cmbBaudRate.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(312, 45);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(56, 15);
            this.label13.TabIndex = 13;
            this.label13.Text = "Data Bits";
            this.label13.Visible = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(312, 76);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 15);
            this.label14.TabIndex = 14;
            this.label14.Text = "Stop Bits";
            this.label14.Visible = false;
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(340, 19);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.Enabled = false;
            this.btnDownload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDownload.Location = new System.Drawing.Point(211, 19);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 23);
            this.btnDownload.TabIndex = 1;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.BtnDownload_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnDownload);
            this.groupBox4.Controls.Add(this.btnClose);
            this.groupBox4.Controls.Add(this.btnUpload);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox4.Location = new System.Drawing.Point(109, 401);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox4.Size = new System.Drawing.Size(636, 65);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            // 
            // groupBoxFile
            // 
            this.groupBoxFile.Controls.Add(this.btnConfigure);
            this.groupBoxFile.Controls.Add(this.txtMacID);
            this.groupBoxFile.Controls.Add(this.lblethernetFile);
            this.groupBoxFile.Controls.Add(this.btnethernet);
            this.groupBoxFile.Controls.Add(this.txtBoxEthernetFile);
            this.groupBoxFile.Controls.Add(this.lblFontPath);
            this.groupBoxFile.Controls.Add(this.lblLadPath);
            this.groupBoxFile.Controls.Add(this.BtnFontFileBrowse);
            this.groupBoxFile.Controls.Add(this.txtFontFilePath);
            this.groupBoxFile.Controls.Add(this.BtnLadderBrowse);
            this.groupBoxFile.Controls.Add(this.txtLadderFilePath);
            this.groupBoxFile.Controls.Add(this.lblAppPath);
            this.groupBoxFile.Controls.Add(this.lblFirmPath);
            this.groupBoxFile.Controls.Add(this.btnApplicationBrowse);
            this.groupBoxFile.Controls.Add(this.txtAppFilePath);
            this.groupBoxFile.Controls.Add(this.btnFirmwareBrowse);
            this.groupBoxFile.Controls.Add(this.txtFirmwareFilePath);
            this.groupBoxFile.Location = new System.Drawing.Point(210, 216);
            this.groupBoxFile.Name = "groupBoxFile";
            this.groupBoxFile.Size = new System.Drawing.Size(495, 188);
            this.groupBoxFile.TabIndex = 43;
            this.groupBoxFile.TabStop = false;
            this.groupBoxFile.Text = "File Download";
            // 
            // btnConfigure
            // 
            this.btnConfigure.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnConfigure.Location = new System.Drawing.Point(408, 133);
            this.btnConfigure.Name = "btnConfigure";
            this.btnConfigure.Size = new System.Drawing.Size(75, 23);
            this.btnConfigure.TabIndex = 45;
            this.btnConfigure.Text = "Configure";
            this.btnConfigure.UseVisualStyleBackColor = true;
            this.btnConfigure.Visible = false;
            this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // txtMacID
            // 
            this.txtMacID.Location = new System.Drawing.Point(100, 162);
            this.txtMacID.Name = "txtMacID";
            this.txtMacID.Size = new System.Drawing.Size(220, 20);
            this.txtMacID.TabIndex = 44;
            this.txtMacID.Text = "00:1A:C2:7B:00:47";
            this.txtMacID.Visible = false;
            // 
            // lblethernetFile
            // 
            this.lblethernetFile.AutoSize = true;
            this.lblethernetFile.Location = new System.Drawing.Point(16, 135);
            this.lblethernetFile.Name = "lblethernetFile";
            this.lblethernetFile.Size = new System.Drawing.Size(66, 13);
            this.lblethernetFile.TabIndex = 42;
            this.lblethernetFile.Text = "Ethernet File";
            this.lblethernetFile.Visible = false;
            // 
            // btnethernet
            // 
            this.btnethernet.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnethernet.Location = new System.Drawing.Point(325, 133);
            this.btnethernet.Name = "btnethernet";
            this.btnethernet.Size = new System.Drawing.Size(75, 23);
            this.btnethernet.TabIndex = 41;
            this.btnethernet.Text = "Browse";
            this.btnethernet.UseVisualStyleBackColor = true;
            this.btnethernet.Visible = false;
            this.btnethernet.Click += new System.EventHandler(this.Btnethernet_Click);
            // 
            // txtBoxEthernetFile
            // 
            this.txtBoxEthernetFile.Location = new System.Drawing.Point(99, 135);
            this.txtBoxEthernetFile.Name = "txtBoxEthernetFile";
            this.txtBoxEthernetFile.Size = new System.Drawing.Size(220, 20);
            this.txtBoxEthernetFile.TabIndex = 40;
            this.txtBoxEthernetFile.Visible = false;
            // 
            // lblFontPath
            // 
            this.lblFontPath.AutoSize = true;
            this.lblFontPath.Location = new System.Drawing.Point(17, 104);
            this.lblFontPath.Name = "lblFontPath";
            this.lblFontPath.Size = new System.Drawing.Size(47, 13);
            this.lblFontPath.TabIndex = 39;
            this.lblFontPath.Text = "Font File";
            this.lblFontPath.Visible = false;
            // 
            // lblLadPath
            // 
            this.lblLadPath.AutoSize = true;
            this.lblLadPath.Location = new System.Drawing.Point(17, 75);
            this.lblLadPath.Name = "lblLadPath";
            this.lblLadPath.Size = new System.Drawing.Size(59, 13);
            this.lblLadPath.TabIndex = 38;
            this.lblLadPath.Text = "Ladder File";
            this.lblLadPath.Visible = false;
            // 
            // BtnFontFileBrowse
            // 
            this.BtnFontFileBrowse.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnFontFileBrowse.Location = new System.Drawing.Point(326, 102);
            this.BtnFontFileBrowse.Name = "BtnFontFileBrowse";
            this.BtnFontFileBrowse.Size = new System.Drawing.Size(75, 23);
            this.BtnFontFileBrowse.TabIndex = 37;
            this.BtnFontFileBrowse.Text = "Browse";
            this.BtnFontFileBrowse.UseVisualStyleBackColor = true;
            this.BtnFontFileBrowse.Visible = false;
            this.BtnFontFileBrowse.Click += new System.EventHandler(this.BtnFontFileBrowse_Click);
            // 
            // txtFontFilePath
            // 
            this.txtFontFilePath.Location = new System.Drawing.Point(100, 104);
            this.txtFontFilePath.Name = "txtFontFilePath";
            this.txtFontFilePath.Size = new System.Drawing.Size(220, 20);
            this.txtFontFilePath.TabIndex = 36;
            this.txtFontFilePath.Visible = false;
            // 
            // BtnLadderBrowse
            // 
            this.BtnLadderBrowse.Location = new System.Drawing.Point(326, 73);
            this.BtnLadderBrowse.Name = "BtnLadderBrowse";
            this.BtnLadderBrowse.Size = new System.Drawing.Size(75, 23);
            this.BtnLadderBrowse.TabIndex = 35;
            this.BtnLadderBrowse.Text = "Browse";
            this.BtnLadderBrowse.UseVisualStyleBackColor = true;
            this.BtnLadderBrowse.Visible = false;
            this.BtnLadderBrowse.Click += new System.EventHandler(this.BtnLadderBrowse_Click_1);
            // 
            // txtLadderFilePath
            // 
            this.txtLadderFilePath.Location = new System.Drawing.Point(100, 75);
            this.txtLadderFilePath.Name = "txtLadderFilePath";
            this.txtLadderFilePath.Size = new System.Drawing.Size(220, 20);
            this.txtLadderFilePath.TabIndex = 34;
            this.txtLadderFilePath.Visible = false;
            // 
            // lblAppPath
            // 
            this.lblAppPath.AutoSize = true;
            this.lblAppPath.Location = new System.Drawing.Point(17, 46);
            this.lblAppPath.Name = "lblAppPath";
            this.lblAppPath.Size = new System.Drawing.Size(78, 13);
            this.lblAppPath.TabIndex = 33;
            this.lblAppPath.Text = "Application File";
            this.lblAppPath.Visible = false;
            // 
            // lblFirmPath
            // 
            this.lblFirmPath.AutoSize = true;
            this.lblFirmPath.Location = new System.Drawing.Point(17, 17);
            this.lblFirmPath.Name = "lblFirmPath";
            this.lblFirmPath.Size = new System.Drawing.Size(68, 13);
            this.lblFirmPath.TabIndex = 32;
            this.lblFirmPath.Text = "Firmware File";
            this.lblFirmPath.Visible = false;
            // 
            // btnApplicationBrowse
            // 
            this.btnApplicationBrowse.Location = new System.Drawing.Point(326, 44);
            this.btnApplicationBrowse.Name = "btnApplicationBrowse";
            this.btnApplicationBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnApplicationBrowse.TabIndex = 31;
            this.btnApplicationBrowse.Text = "Browse";
            this.btnApplicationBrowse.UseVisualStyleBackColor = true;
            this.btnApplicationBrowse.Visible = false;
            this.btnApplicationBrowse.Click += new System.EventHandler(this.BtnApplicationBrowse_Click_1);
            // 
            // txtAppFilePath
            // 
            this.txtAppFilePath.Location = new System.Drawing.Point(100, 46);
            this.txtAppFilePath.Name = "txtAppFilePath";
            this.txtAppFilePath.Size = new System.Drawing.Size(220, 20);
            this.txtAppFilePath.TabIndex = 30;
            this.txtAppFilePath.Visible = false;
            // 
            // btnFirmwareBrowse
            // 
            this.btnFirmwareBrowse.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnFirmwareBrowse.Location = new System.Drawing.Point(326, 15);
            this.btnFirmwareBrowse.Name = "btnFirmwareBrowse";
            this.btnFirmwareBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnFirmwareBrowse.TabIndex = 29;
            this.btnFirmwareBrowse.Text = "Browse";
            this.btnFirmwareBrowse.UseVisualStyleBackColor = true;
            this.btnFirmwareBrowse.Visible = false;
            this.btnFirmwareBrowse.Click += new System.EventHandler(this.BtnFirmwareBrowse_Click_1);
            // 
            // txtFirmwareFilePath
            // 
            this.txtFirmwareFilePath.Location = new System.Drawing.Point(100, 17);
            this.txtFirmwareFilePath.Name = "txtFirmwareFilePath";
            this.txtFirmwareFilePath.Size = new System.Drawing.Size(220, 20);
            this.txtFirmwareFilePath.TabIndex = 28;
            this.txtFirmwareFilePath.Visible = false;
            // 
            // chbxMacID
            // 
            this.chbxMacID.AutoSize = true;
            this.chbxMacID.Location = new System.Drawing.Point(584, 198);
            this.chbxMacID.Name = "chbxMacID";
            this.chbxMacID.Size = new System.Drawing.Size(58, 17);
            this.chbxMacID.TabIndex = 43;
            this.chbxMacID.Text = "MacID";
            this.chbxMacID.UseVisualStyleBackColor = true;
            this.chbxMacID.CheckedChanged += new System.EventHandler(this.chbxMacID_CheckedChanged);
            // 
            // chkBoxFontFile
            // 
            this.chkBoxFontFile.AutoSize = true;
            this.chkBoxFontFile.Location = new System.Drawing.Point(670, 198);
            this.chkBoxFontFile.Name = "chkBoxFontFile";
            this.chkBoxFontFile.Size = new System.Drawing.Size(47, 17);
            this.chkBoxFontFile.TabIndex = 20;
            this.chkBoxFontFile.Text = "Font";
            this.chkBoxFontFile.UseVisualStyleBackColor = true;
            this.chkBoxFontFile.Visible = false;
            this.chkBoxFontFile.CheckedChanged += new System.EventHandler(this.chkBoxFontFile_CheckedChanged);
            this.chkBoxFontFile.CheckStateChanged += new System.EventHandler(this.ChkBoxFontFile_CheckStateChanged);
            // 
            // chkBoxApplicationFile
            // 
            this.chkBoxApplicationFile.AutoSize = true;
            this.chkBoxApplicationFile.Location = new System.Drawing.Point(267, 198);
            this.chkBoxApplicationFile.Name = "chkBoxApplicationFile";
            this.chkBoxApplicationFile.Size = new System.Drawing.Size(78, 17);
            this.chkBoxApplicationFile.TabIndex = 19;
            this.chkBoxApplicationFile.Text = "Application";
            this.chkBoxApplicationFile.UseVisualStyleBackColor = true;
            this.chkBoxApplicationFile.CheckedChanged += new System.EventHandler(this.chkBoxApplicationFile_CheckedChanged);
            this.chkBoxApplicationFile.CheckStateChanged += new System.EventHandler(this.ChkBoxApplicationFile_CheckStateChanged);
            // 
            // chkBoxLadderFile
            // 
            this.chkBoxLadderFile.AutoSize = true;
            this.chkBoxLadderFile.Location = new System.Drawing.Point(584, 198);
            this.chkBoxLadderFile.Name = "chkBoxLadderFile";
            this.chkBoxLadderFile.Size = new System.Drawing.Size(59, 17);
            this.chkBoxLadderFile.TabIndex = 18;
            this.chkBoxLadderFile.Text = "Ladder";
            this.chkBoxLadderFile.UseVisualStyleBackColor = true;
            this.chkBoxLadderFile.Visible = false;
            this.chkBoxLadderFile.CheckedChanged += new System.EventHandler(this.chkBoxLadderFile_CheckedChanged);
            this.chkBoxLadderFile.CheckStateChanged += new System.EventHandler(this.ChkBoxLadderFile_CheckStateChanged);
            // 
            // chkBoxFirmawareFile
            // 
            this.chkBoxFirmawareFile.AutoSize = true;
            this.chkBoxFirmawareFile.Location = new System.Drawing.Point(177, 198);
            this.chkBoxFirmawareFile.Name = "chkBoxFirmawareFile";
            this.chkBoxFirmawareFile.Size = new System.Drawing.Size(68, 17);
            this.chkBoxFirmawareFile.TabIndex = 17;
            this.chkBoxFirmawareFile.Text = "Firmware";
            this.chkBoxFirmawareFile.CheckedChanged += new System.EventHandler(this.chkBoxFirmawareFile_CheckedChanged);
            this.chkBoxFirmawareFile.CheckStateChanged += new System.EventHandler(this.CmbBoxFirmawareFile_CheckStateChanged);
            // 
            // chkboxEthernetSettings
            // 
            this.chkboxEthernetSettings.AutoSize = true;
            this.chkboxEthernetSettings.Location = new System.Drawing.Point(360, 198);
            this.chkboxEthernetSettings.Name = "chkboxEthernetSettings";
            this.chkboxEthernetSettings.Size = new System.Drawing.Size(107, 17);
            this.chkboxEthernetSettings.TabIndex = 45;
            this.chkboxEthernetSettings.Text = "Ethernet Settings";
            this.chkboxEthernetSettings.UseVisualStyleBackColor = true;
            this.chkboxEthernetSettings.CheckedChanged += new System.EventHandler(this.chkboxEthernetSettings_CheckedChanged);
            this.chkboxEthernetSettings.CheckStateChanged += new System.EventHandler(this.ChkboxEthernetSettings_CheckStateChanged);
            // 
            // checkBoxRTCSync
            // 
            this.checkBoxRTCSync.AutoSize = true;
            this.checkBoxRTCSync.Location = new System.Drawing.Point(486, 198);
            this.checkBoxRTCSync.Name = "checkBoxRTCSync";
            this.checkBoxRTCSync.Size = new System.Drawing.Size(75, 17);
            this.checkBoxRTCSync.TabIndex = 46;
            this.checkBoxRTCSync.Text = "Sync RTC";
            this.checkBoxRTCSync.UseVisualStyleBackColor = true;
            this.checkBoxRTCSync.CheckedChanged += new System.EventHandler(this.checkBoxRTCSync_CheckedChanged);
            // 
            // panelUpload
            // 
            this.panelUpload.Location = new System.Drawing.Point(145, 215);
            this.panelUpload.Name = "panelUpload";
            this.panelUpload.Size = new System.Drawing.Size(588, 234);
            this.panelUpload.TabIndex = 47;
            // 
            // Download
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(745, 494);
            this.Controls.Add(this.checkBoxRTCSync);
            this.Controls.Add(this.chkboxEthernetSettings);
            this.Controls.Add(this.chbxMacID);
            this.Controls.Add(this.groupBoxFile);
            this.Controls.Add(this.chkBoxFirmawareFile);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.chkBoxLadderFile);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.chkBoxApplicationFile);
            this.Controls.Add(this.chkBoxFontFile);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panelUpload);
            this.Controls.Add(this.grpBoxCOMPort);
            this.Controls.Add(this.grpBoxUSB);
            this.Controls.Add(this.grpBoxEthernet);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Download";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Download to device";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Download_FormClosing);
            this.Load += new System.EventHandler(this.Download_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.grpBoxUSB.ResumeLayout(false);
            this.grpBoxUSB.PerformLayout();
            this.grpBoxEthernet.ResumeLayout(false);
            this.grpBoxEthernet.PerformLayout();
            this.grpBoxCOMPort.ResumeLayout(false);
            this.grpBoxCOMPort.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBoxFile.ResumeLayout(false);
            this.groupBoxFile.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.GroupBox groupBox10;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.CheckBox cmbxFont;
		private System.Windows.Forms.CheckBox cmbxApplication;
		private System.Windows.Forms.CheckBox cmbxLadder;
		private System.Windows.Forms.CheckBox cmbxFirmare;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.RichTextBox rtxtDataArea;
		private System.Windows.Forms.GroupBox grpBoxUSB;
		private System.Windows.Forms.GroupBox grpBoxEthernet;
		private System.Windows.Forms.TextBox txtBoxPortNumber;
		private System.Windows.Forms.TextBox txtBoxIPAddress;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox grpBoxCOMPort;
		private System.Windows.Forms.ComboBox cmbPortName;
		private System.Windows.Forms.ComboBox cmbStopBit;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ComboBox cmbDataBits;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.ComboBox cmbParity;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.ComboBox cmbBaudRate;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Button btnDownload;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBoxFile;
		private System.Windows.Forms.CheckBox chkBoxFontFile;
		private System.Windows.Forms.CheckBox chkBoxApplicationFile;
		private System.Windows.Forms.CheckBox chkBoxLadderFile;
		private System.Windows.Forms.CheckBox chkBoxFirmawareFile;
		private System.Windows.Forms.Label lblFontPath;
		private System.Windows.Forms.Label lblLadPath;
		private System.Windows.Forms.Button BtnFontFileBrowse;
		private System.Windows.Forms.TextBox txtFontFilePath;
		private System.Windows.Forms.Button BtnLadderBrowse;
		private System.Windows.Forms.TextBox txtLadderFilePath;
		private System.Windows.Forms.Label lblAppPath;
		private System.Windows.Forms.Label lblFirmPath;
		private System.Windows.Forms.Button btnApplicationBrowse;
		private System.Windows.Forms.TextBox txtAppFilePath;
		private System.Windows.Forms.Button btnFirmwareBrowse;
		private System.Windows.Forms.TextBox txtFirmwareFilePath;
		private System.Windows.Forms.CheckBox chkboxEthernetSettings;
		private System.Windows.Forms.Label lblethernetFile;
		private System.Windows.Forms.Button btnethernet;
		private System.Windows.Forms.TextBox txtBoxEthernetFile;
        private System.Windows.Forms.TextBox txtMacID;
        private System.Windows.Forms.CheckBox chbxMacID;
        private System.Windows.Forms.Button btnConfigure;
        private System.Windows.Forms.CheckBox checkBoxRTCSync;
        private System.Windows.Forms.ComboBox cmbxUSBPortDetails;
        private System.Windows.Forms.Label lblPortDetails;
        private System.Windows.Forms.Panel panelUpload;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}