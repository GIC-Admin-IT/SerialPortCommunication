using Microsoft.Win32;
using SerialPortCommunication.Properties;
using Supporting_DLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml.Linq;
using static Supporting_DLL.CONSTANTS;
using Serilog;

namespace SerialPortCommunication
{
    public partial class Download : Form
    {
        public delegate int __connectUIserialToController(CommunicationParameters obj);
        public event __connectUIserialToController _connectUIserialToController;

        public delegate CONSTANTS.BinaryMemoryCalculation __saveBinaryFile(string applicationFilePath);
        public event __saveBinaryFile _saveBinaryFile;

        public delegate List<string> __GetAlarmDetails();
        public event __GetAlarmDetails _GetAlarmDetails;

        public delegate List<string> __getProjectModelIdForFirmawarFilePath();
        public event __getProjectModelIdForFirmawarFilePath _getProjectModelIdForFirmawarFilePath;

        public delegate bool __checkIsEthernetModel();
        public event __checkIsEthernetModel _checkIsEthernetModel;

        public delegate bool __checkThreadAlive();
        public event __checkThreadAlive _checkThreadAlive;

        public delegate void __killTheProcess();
        public event __killTheProcess _killTheProcess;



        delegate void SetTextCallback(string text, CurrentDownload pCurrentDownload);

        //SerialPort ComPort = new SerialPort();

        bool _standAloneUtility = false;
        bool _standAloneUtilityThroughCommand = true;
        bool _isDownload = false;
        string AckFromXML;
        bool macIDVisibilityFromxml = false;

        bool _downloadInProgress;

        bool IsEthernetModel = false;

        int _portNo;
        public int PortNo { get => _portNo; set => _portNo = value; }

        private bool downloadInProgress
        {
            get { return _downloadInProgress; }
            set
            {
                _downloadInProgress = value;
                // btnClose.Enabled = !value;
                // btnDownload.Enabled = !value;
            }
        }


        List<Tuple<string, string>> _listOfUSBPorts = new List<Tuple<string, string>>();
        public List<Tuple<string, string>> ListOfUSBPorts { get => _listOfUSBPorts; set => _listOfUSBPorts = value; }

        public Download()
        {
            InitializeComponent();
            groupBoxFile.Visible = true;
            listBox1.SelectedIndex = 0;
        }

        public Form init(string callFrom, bool isDownload, bool _pUloadSuccess, Form pObjView)
        {
            _isDownload = isDownload;

            if (callFrom == "Utility1")
                _standAloneUtility = true;
            else
            {
                if (isDownload)
                {
                    _standAloneUtility = false;
                    panelUpload.Visible = false;
                    this.Text = "Download To Device";
                    btnDownload.Visible = true;
                    btnUpload.Visible = false;
                    DialogResult d = this.ShowDialog(pObjView);

                }
                else
                {
                    _standAloneUtility = false;
                    panelUpload.Visible = true;
                    this.Text = "Upload From Device";
                    btnUpload.Visible = true;
                    btnDownload.Visible = false;
                    this.ShowDialog(pObjView);
                }

            }

            return this;
        }


        byte[] ObjectToByteArray(int value)
        {
            return BitConverter.GetBytes(value);

            byte[] intBytes = BitConverter.GetBytes(value);
            Array.Reverse(intBytes);
            byte[] result = intBytes;

            return result;
        }
        private bool updatePorts()
        {
            bool isHavePorts = true;

            string[] ports = SerialPort.GetPortNames();

            ports = ports.Distinct().ToArray();

            if (ports.Length == 0 || ports == null)
            {
                isHavePorts = false;
                return isHavePorts;
            }

            cmbPortName.Items.Clear();
            List<string> _USBPortList = new List<string>();

            using (ManagementClass i_Entity = new ManagementClass("Win32_PnPEntity"))
            {
                foreach (ManagementObject i_Inst in i_Entity.GetInstances())
                {
                    Object o_Guid = i_Inst.GetPropertyValue("ClassGuid");
                    if (o_Guid == null || o_Guid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                        continue; // Skip all devices except device class "PORTS"

                    String s_Caption = i_Inst.GetPropertyValue("Caption").ToString();
                    String s_Manufact = i_Inst.GetPropertyValue("Manufacturer").ToString();
                    String s_DeviceID = i_Inst.GetPropertyValue("PnpDeviceID").ToString();
                    String s_RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + s_DeviceID + "\\Device Parameters";
                    String s_PortName = Registry.GetValue(s_RegPath, "PortName", "").ToString();
                    if (s_DeviceID.StartsWith("U"))
                    {
                        _USBPortList.Add(s_PortName);
                    }

                    int s32_Pos = s_Caption.IndexOf(" (COM");
                    if (s32_Pos > 0) // remove COM port from description
                        s_Caption = s_Caption.Substring(0, s32_Pos);

                    Console.WriteLine("Port Name:    " + s_PortName);
                    Console.WriteLine("Description:  " + s_Caption);
                    Console.WriteLine("Manufacturer: " + s_Manufact);
                    Console.WriteLine("Device ID:    " + s_DeviceID);
                    Console.WriteLine("-----------------------------------");
                }
            }

            List<string> _comPortList = new List<string>();

            foreach (string port in ports)
            {
                _comPortList.Add(port);
            }

            _comPortList = _comPortList.Except(_USBPortList).ToList();

            for (int i = 0; i < _comPortList.Count; i++)
            {
                cmbPortName.Items.Add(_comPortList[i]);
            }

            //foreach (string port in ports)
            //{
            //    cmbPortName.Items.Add(port);
            //}

            return isHavePorts;
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbxUSBPortDetails.Items.Clear();
            ListOfUSBPorts.Clear();
            if (listBox1.SelectedIndex == 0)
            {
                btnDownload.Enabled = true;

                cmbBaudRate.Enabled = false;
                cmbParity.Enabled = false;
                cmbStopBit.Enabled = false;
                cmbDataBits.Enabled = false;
                label11.Enabled = false;
                label12.Enabled = false;
                label13.Enabled = false;
                label14.Enabled = false;

                grpBoxEthernet.Visible = false;
                grpBoxUSB.Visible = false;
                grpBoxCOMPort.Visible = true;
                //groupBoxFile.Visible = true;
                checkBoxRTCSync.Visible = true;
                checkBoxRTCSync.Enabled = false;
                //chbxMacID.Visible = true;

            }
            else if (listBox1.SelectedIndex == 1)  //USB
            {

                if (cmbxUSBPortDetails.Text == "")
                    btnDownload.Enabled = false;
                else
                    btnDownload.Enabled = true;

                ShowHideDownloadButton();


                checkBoxRTCSync.Enabled = true;

                checkBoxRTCSync.Visible = true;
                using (ManagementClass i_Entity = new ManagementClass("Win32_PnPEntity"))
                {
                    foreach (ManagementObject i_Inst in i_Entity.GetInstances())
                    {
                        Object o_Guid = i_Inst.GetPropertyValue("ClassGuid");
                        if (o_Guid == null || o_Guid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                            continue; // Skip all devices except device class "PORTS"

                        String s_Caption = i_Inst.GetPropertyValue("Caption").ToString();
                        String s_Manufact = i_Inst.GetPropertyValue("Manufacturer").ToString();
                        String s_DeviceID = i_Inst.GetPropertyValue("PnpDeviceID").ToString();
                        String s_RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + s_DeviceID + "\\Device Parameters";
                        String s_PortName = Registry.GetValue(s_RegPath, "PortName", "").ToString();
                        if (s_DeviceID.StartsWith("U"))
                        {
                            cmbxUSBPortDetails.Items.Add(s_PortName);
                            Tuple<string, string> UsbPort = new Tuple<string, string>(s_PortName, s_DeviceID);
                            ListOfUSBPorts.Add(UsbPort);
                        }



                        if (cmbxUSBPortDetails.Items.Count > 0)
                            cmbxUSBPortDetails.SelectedIndex = 0;
                        int s32_Pos = s_Caption.IndexOf(" (COM");
                        if (s32_Pos > 0) // remove COM port from description
                            s_Caption = s_Caption.Substring(0, s32_Pos);

                        Console.WriteLine("Port Name:    " + s_PortName);
                        Console.WriteLine("Description:  " + s_Caption);
                        Console.WriteLine("Manufacturer: " + s_Manufact);
                        Console.WriteLine("Device ID:    " + s_DeviceID);
                        Console.WriteLine("-----------------------------------");
                    }
                }

                grpBoxEthernet.Visible = false;
                grpBoxCOMPort.Visible = false;
                grpBoxUSB.Visible = true;
                checkBoxRTCSync.Visible = true;
                //chbxMacID.Visible = true;
                //groupBoxFile.Visible = true;

            }
            else if (listBox1.SelectedIndex == 2)
            {
                btnDownload.Enabled = true;

                grpBoxCOMPort.Visible = false;
                grpBoxUSB.Visible = false;
                grpBoxEthernet.Visible = true;
                //groupBoxFile.Visible = true;
                checkBoxRTCSync.Visible = false;
                checkBoxRTCSync.Visible = false;
                //chbxMacID.Visible = false;
            }
        }
        private void PaintBorderlessGroupBox(object sender, PaintEventArgs p)
        {
            GroupBox box = (GroupBox)sender;
            p.Graphics.Clear(SystemColors.Control);
            //p.Graphics.DrawString(box.Text, box.Font, Brushes.Black, 0, 0);
        }

        public void SetPrerequisites()
        {
            if (Convert.ToBoolean(AppData.DownloadFirmware))
            {
                chkBoxFirmawareFile.Checked = true;
            }
            if (Convert.ToBoolean(AppData.DownloadEthernetSetting))
            {
                chkboxEthernetSettings.Checked = true;
            }
            if (Convert.ToBoolean(AppData.DownloadApplicationFile))
            {
                chkBoxApplicationFile.Checked = true;
            }
            if (Convert.ToBoolean(AppData.DownloadRTC))
            {
                checkBoxRTCSync.Checked = true;
            }
            if (Convert.ToBoolean(AppData.DownloadMACID))
            {
                chbxMacID.Checked = true;
            }

            listBox1.SelectedItem = AppData.CommunicationType;

            txtFirmwareFilePath.Enabled = false;
            txtBoxEthernetFile.Enabled = false;
            txtAppFilePath.Enabled = false;
            btnApplicationBrowse.Enabled = false;
            btnFirmwareBrowse.Enabled = false;

            //listBox1.SelectedItem = "Serial";
            //chkBoxFirmawareFile.Checked = true;

            BtnDownload_Click(this, null);
        }

        private void Download_Load(object sender, EventArgs e)
        {
            //Log.Information("Download_Load: StandAloneUtility={StandAlone}, IsDownload={IsDownload}, IP={IP}, Port={Port}",
            //_standAloneUtility, _isDownload, txtBoxIPAddress.Text, txtBoxPortNumber.Text);


            // IsEthernetModel = _checkIsEthernetModel();

            //if (IsEthernetModel == false)
            //{
            //    listBox1.Items.Remove("Ethernet");
            //    chkboxEthernetSettings.Enabled = false;
            //}

            PortNo = Convert.ToInt32(txtBoxPortNumber.Text); //ethernet port no set to 5000

            string filePath1 = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Production" + ".xml");

            //XDocument doc1 = XDocument.Load(filePath1);

            //XElement xel = doc1.Element("Manufacturer").Elements("MacId").SingleOrDefault();


            //string macIDVisibility = xel.Value;
            //if (macIDVisibility == "Yes")
            //{
            //    chbxMacID.Visible = true;
            //    macIDVisibilityFromxml = true;
            //}
            //else
            //{
            //    chbxMacID.Visible = false;
            //    macIDVisibilityFromxml = false;
            //}

            cmbxFirmare.Checked = true;

            groupBox10.Paint += PaintBorderlessGroupBox;
            groupBox4.Paint += PaintBorderlessGroupBox;

            bool isHavePorts = updatePorts();

            CheckForIllegalCrossThreadCalls = false;

            if (isHavePorts == true && cmbPortName.Items.Count > 0)
                cmbPortName.SelectedIndex = 0;

            cmbBaudRate.SelectedIndex = 7;
            cmbParity.SelectedIndex = 0;
            cmbDataBits.SelectedIndex = 1;
            cmbStopBit.SelectedIndex = 0;

            if (_standAloneUtility == false)
            {
                ReadEthernetConfigFile();
            }

            string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "AckFile.xml");

            if (File.Exists(filePath))
            {
                XDocument doc = XDocument.Load(filePath);
                var _AckFromXML = from elements in doc.Elements("Device").Elements("ACK") select elements;

                AckFromXML = _AckFromXML.FirstOrDefault().Value.ToString();
            }
            else
            {
                AckFromXML = "No";

            }

            txtBoxIPAddress.Text = Settings.Default.IP_Address;
            txtBoxPortNumber.Text = Settings.Default.Port_Number;

            SetPrerequisites();



        }

        public void ReadEthernetConfigFile()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "EthernetSetting");

            if (File.Exists(filePath))
            {
                FileStream pFileStream = new FileStream(filePath, FileMode.Open);

                byte[] buffer1 = new byte[4];

                int[] _ipArr = new int[4];

                pFileStream.Read(buffer1, 0, 1);        //DHCP

                Array.Clear(buffer1, 0, 4);
                pFileStream.Read(buffer1, 0, 1);
                _ipArr[0] = BitConverter.ToInt32(buffer1, 0);                                    //IP [0]

                Array.Clear(buffer1, 0, 4);
                pFileStream.Read(buffer1, 0, 1);
                _ipArr[1] = BitConverter.ToInt32(buffer1, 0);                                    //IP [0]

                Array.Clear(buffer1, 0, 4);
                pFileStream.Read(buffer1, 0, 1);
                _ipArr[2] = BitConverter.ToInt32(buffer1, 0);                                    //IP [0]

                Array.Clear(buffer1, 0, 4);
                pFileStream.Read(buffer1, 0, 1);
                _ipArr[3] = BitConverter.ToInt32(buffer1, 0);                                    //IP [0]

                pFileStream.Read(buffer1, 0, 4);

                Array.Clear(buffer1, 0, 4);
                pFileStream.Read(buffer1, 0, 2);
                int _dwnlPort = BitConverter.ToInt32(buffer1, 0);                                    //IP [0]

                for (int i = 0; i < _ipArr.Length; i++)
                {
                    if (i > 0)
                        txtBoxIPAddress.Text += "." + _ipArr[i].ToString();
                    else
                        txtBoxIPAddress.Text += _ipArr[i].ToString();

                }

                txtBoxPortNumber.Text = _dwnlPort.ToString();

                pFileStream.Close();
            }
        }

        string ethernetFilePath;

        internal int AddNeWMacIDSettings()
        {
            StreamReader oStreamReader = new StreamReader(AppData.MACIDFilePath);
            var res = oStreamReader.ReadToEnd().Replace("\r\n", "\n").Split('\n').ToList(); //text= new StreamReader("MacID.txt");
            oStreamReader.Close();
            oStreamReader.Dispose();
            string[] MacID_SpliteArr = res.ElementAt(0).Split(':');

            int MacID_SpliteArr0 = Convert.ToInt32(MacID_SpliteArr[0].Replace('O', '0'), 16);
            int MacID_SpliteArr1 = Convert.ToInt32(MacID_SpliteArr[1].Replace('O', '0'), 16);
            int MacID_SpliteArr2 = Convert.ToInt32(MacID_SpliteArr[2].Replace('O', '0'), 16);
            int MacID_SpliteArr3 = Convert.ToInt32(MacID_SpliteArr[3].Replace('O', '0'), 16);
            int MacID_SpliteArr4 = Convert.ToInt32(MacID_SpliteArr[4].Replace('O', '0'), 16);
            int MacID_SpliteArr5 = Convert.ToInt32(MacID_SpliteArr[5].Replace('O', '0'), 16);

            byte[] MacIDInByte = new[] { (byte)MacID_SpliteArr0, (byte)MacID_SpliteArr1, (byte)MacID_SpliteArr2, (byte)MacID_SpliteArr3, (byte)MacID_SpliteArr4, (byte)MacID_SpliteArr5 };

            //communicationParameterObj.MacID = MacIDInByte;

            byte[] buffer1 = new byte[6];

            //string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), AppData.MACIDFilePath);
            string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MacID");

            FileStream pFileStream = new FileStream(filePath, FileMode.Create);

            pFileStream.Write(MacIDInByte, 0, 6);       //MacID 0
            pFileStream.Close();
            pFileStream.Dispose();
            AppData.DownloadedMacID = res.ElementAt(0);
            res.RemoveAt(0);

            FileStream pFileStream1 = new FileStream(AppData.MACIDFilePath, FileMode.Create);

            StreamWriter oStreamWriter = new StreamWriter(pFileStream1);

            foreach (string item in res)
            {
                oStreamWriter.WriteLine(item);
            }
            oStreamWriter.Close();
            oStreamWriter.Dispose();
            return 0;


        }


        private void BtnDownload_Click(object sender, EventArgs e)
        {
            btnDownload.Enabled = false;
            listBox1.Enabled = false;
            //chkboxfirmawarefile.enabled = false;
            //chkboxapplicationfile.enabled = false;
            //chkboxethernetsettings.enabled = false;
            //chbxmacid.enabled = false;
            //checkboxrtcsync.enabled = false;
            //chkboxfontfile.enabled = false;
            //chkboxladderfile.enabled = false;
            chkBoxFirmawareFile.Enabled = true;
            chkBoxApplicationFile.Enabled = true;
            chkboxEthernetSettings.Enabled = true;
            chbxMacID.Enabled = true;
            checkBoxRTCSync.Enabled = true;
            chkBoxFontFile.Enabled = true;
            chkBoxLadderFile.Enabled = true;
            grpBoxUSB.Enabled = true;
            grpBoxEthernet.Enabled = true;
            grpBoxCOMPort.Enabled = true;

            grpBoxUSB.Enabled = false;
            grpBoxEthernet.Enabled = false;
            grpBoxCOMPort.Enabled = false;
            cmbPortName.Text = AppData.Port;
            CommunicationParameters communicationParameterObj = new CommunicationParameters();

            string applicationFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "ApplicationFile");

            string firmwareFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "FirmwareFile");

            string fontFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "FontFile");

            string ladderFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "LadderFile");

            string ethernetFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "EthernetSetting");

            string macIdFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MacID");

            //if (listBox1.SelectedIndex == 1)
            //	MessageBox.Show("");
            if (AckFromXML == "Yes")
                communicationParameterObj.Acknowledgement = true;
            else
                communicationParameterObj.Acknowledgement = false;

            if (listBox1.SelectedItem.ToString() == "Serial")
            {
                communicationParameterObj.Mode = CommunicationParameters.CommunicationMode.Serial;
                communicationParameterObj.PortName = cmbPortName.Text;
                communicationParameterObj.BaudRate = Convert.ToInt32(cmbBaudRate.SelectedItem.ToString());
                communicationParameterObj.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
                communicationParameterObj.DataBit = Convert.ToInt32(cmbDataBits.Text);
                communicationParameterObj.StopBit = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBit.Text);
            }
            else if (listBox1.SelectedItem.ToString() == "USB")
            {
                communicationParameterObj.Mode = CommunicationParameters.CommunicationMode.USB;
                communicationParameterObj.PortName = "COM5";//cmbPortName.Text;
                communicationParameterObj.BaudRate = Convert.ToInt32(cmbBaudRate.SelectedItem.ToString());
                communicationParameterObj.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
                communicationParameterObj.DataBit = Convert.ToInt32(cmbDataBits.Text);
                communicationParameterObj.StopBit = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBit.Text);

                if (ListOfUSBPorts.Count == 0)
                {
                    MessageBox.Show("USB Device not recognized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                communicationParameterObj.PortName = AppData.Port;//ListOfUSBPorts[cmbxUSBPortDetails.SelectedIndex].Item1;
                cmbPortName.Text = AppData.Port;
            }
            else if (listBox1.SelectedItem.ToString() == "Ethernet")
            {
                communicationParameterObj.Mode = CommunicationParameters.CommunicationMode.Ethernet;
                communicationParameterObj.IPAddress = txtBoxIPAddress.Text;
                communicationParameterObj.PortNumber = Convert.ToInt32(txtBoxPortNumber.Text);
            }

            communicationParameterObj.applicationFilePath = applicationFilePath;
            communicationParameterObj.firmwareFilePath = firmwareFilePath;
            communicationParameterObj.fontFilePath = fontFilePath;
            communicationParameterObj.ladderFilePath = ladderFilePath;
            communicationParameterObj.ethernetFilePath = ethernetFilePath;
            communicationParameterObj.macIdFilePath = macIdFilePath;

            communicationParameterObj.isFirmware = chkBoxFirmawareFile.Checked;
            communicationParameterObj.isApplication = chkBoxApplicationFile.Checked;
            communicationParameterObj.isFont = chkBoxFontFile.Checked;
            communicationParameterObj.isLadder = chkBoxLadderFile.Checked;
            communicationParameterObj.isMacId = chbxMacID.Checked;
            communicationParameterObj.isEthernetSettings = chkboxEthernetSettings.Checked;
            communicationParameterObj.isRTCSync = checkBoxRTCSync.Checked;

            //if (chbxMacID.Checked && _standAloneUtility == true)
            //{
            //    if (txtBoxEthernetFile.Text == "")
            //    {
            //        MessageBox.Show("select ethernet file..");
            //        return;
            //    }
            //    else
            //    {
            //        if (chbxMacID.Checked)
            //        {
            //            if (txtMacID.Text.Length < 17)
            //            {
            //                MessageBox.Show("Please Enter Valid MAC ID", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //                return;
            //            }
            //            string MacID = txtMacID.Text;

            //            string[] MacID_SpliteArr = txtMacID.Text.ToString().Split(':');

            //            int MacID_SpliteArr0 = Convert.ToInt32(MacID_SpliteArr[0], 16);
            //            int MacID_SpliteArr1 = Convert.ToInt32(MacID_SpliteArr[1], 16);
            //            int MacID_SpliteArr2 = Convert.ToInt32(MacID_SpliteArr[2], 16);
            //            int MacID_SpliteArr3 = Convert.ToInt32(MacID_SpliteArr[3], 16);
            //            int MacID_SpliteArr4 = Convert.ToInt32(MacID_SpliteArr[4], 16);
            //            int MacID_SpliteArr5 = Convert.ToInt32(MacID_SpliteArr[5], 16);

            //            byte[] MacIDInByte = new[] { (byte)MacID_SpliteArr0, (byte)MacID_SpliteArr1, (byte)MacID_SpliteArr2, (byte)MacID_SpliteArr3, (byte)MacID_SpliteArr4, (byte)MacID_SpliteArr5 };

            //            communicationParameterObj.MacID = MacIDInByte;

            //            byte[] buffer1 = new byte[6];

            //            string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MacID");

            //            FileStream pFileStream = new FileStream(filePath, FileMode.Create);

            //            pFileStream.Write(MacIDInByte, 0, 6);       //MacID 0

            //            pFileStream.Dispose();
            //            pFileStream.Close();

            //            communicationParameterObj.macIdFilePath = filePath;
            //        }

            //        communicationParameterObj.macIdFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MacID");
            //    }
            //}

            if (chbxMacID.Checked && (_standAloneUtility == true || _standAloneUtilityThroughCommand))
            {

                if (chbxMacID.Checked)
                {
                    AddNeWMacIDSettings();
                    string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MacID");

                    communicationParameterObj.macIdFilePath = filePath;
                }

                communicationParameterObj.macIdFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "MacID");

            }

            if (checkBoxRTCSync.Checked)
            {
                byte[] buffer1 = new byte[4];

                string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "RTCSyncFile");

                if (File.Exists(filePath))
                    File.Delete(filePath);

                FileStream pFileStream = new FileStream(filePath, FileMode.CreateNew);

                DateTime now = DateTime.Now;

                int day = now.Day;
                int month = now.Month;
                int year = now.Year - 2000;

                int sec = now.Second;
                int min = now.Minute;
                int hr = now.Hour;

                pFileStream.Write(ObjectToByteArray(sec), 0, 1);
                pFileStream.Write(ObjectToByteArray(min), 0, 1);
                pFileStream.Write(ObjectToByteArray(hr), 0, 1);
                pFileStream.Write(ObjectToByteArray(day), 0, 1);
                pFileStream.Write(ObjectToByteArray(month), 0, 1);
                pFileStream.Write(ObjectToByteArray(year), 0, 1);


                pFileStream.Close();
                pFileStream.Dispose();

                communicationParameterObj.RTCSyncFilePath = filePath;

            }



            if (chkBoxApplicationFile.Checked && _standAloneUtility == true)
            {
                if (txtAppFilePath.Text == "")
                {
                    MessageBox.Show("select application file..");
                    return;
                }
                else
                {
                    communicationParameterObj.applicationFilePath = txtAppFilePath.Text;
                }
            }
            if (chkBoxFirmawareFile.Checked && _standAloneUtility == true)
            {
                if (txtFirmwareFilePath.Text == "")
                {
                    MessageBox.Show("select firware file..");
                    return;
                }
                else
                {
                    communicationParameterObj.firmwareFilePath = txtFirmwareFilePath.Text;
                }
            }
            if (chkBoxFontFile.Checked && _standAloneUtility == true)
            {
                if (txtFontFilePath.Text == "")
                {
                    MessageBox.Show("select font file..");
                    return;
                }
                else
                {
                    communicationParameterObj.fontFilePath = txtFontFilePath.Text;
                }
            }

            if (chkBoxLadderFile.Checked && _standAloneUtility == true)
            {
                if (txtLadderFilePath.Text == "")
                {
                    MessageBox.Show("select ladder file..");
                    return;
                }
                else
                {
                    communicationParameterObj.ladderFilePath = txtLadderFilePath.Text;
                }
            }

            if (chkboxEthernetSettings.Checked && (_standAloneUtility || _standAloneUtilityThroughCommand))
            {
                if (_standAloneUtilityThroughCommand)
                {
                    communicationParameterObj.ethernetFilePath = AppData.EthernetSettingFilePath;
                }
                else
                {
                    if (txtBoxEthernetFile.Text == "")
                    {
                        MessageBox.Show("select ethernet file..");
                        return;
                    }
                    else
                    {
                        communicationParameterObj.ethernetFilePath = txtBoxEthernetFile.Text;
                    }
                }
            }


            if (chkBoxApplicationFile.Checked == true && (_standAloneUtility || _standAloneUtilityThroughCommand))
            {
                applicationFilePath = AppData.AppPath;
                communicationParameterObj.applicationFilePath = AppData.AppPath;
                //            CONSTANTS.BinaryMemoryCalculation res = _saveBinaryFile(applicationFilePath);

                //            long dataLogSize = 0;

                //for (int i = 0; i < res.dataLogSize.Count; i++)
                //                dataLogSize += res.dataLogSize[i].Item2 * 64;

                //if (res.totalApplicationSize + dataLogSize > 16777216)
                //{
                //	long diff = (res.totalApplicationSize + dataLogSize) - 16777216;

                //	double extraSize = Convert.ToDouble((diff / 1024) / 1024);

                //	MessageBox.Show("Please reduce application size by " + extraSize.ToString() + " MB", "File Size Exceed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //	RefreshUI();

                //	return;
                //}
            }


            if (chkBoxFirmawareFile.Checked == true && (_standAloneUtility || _standAloneUtilityThroughCommand))
            {

                if (_standAloneUtilityThroughCommand)
                {
                    communicationParameterObj.firmwareFilePath = AppData.FirmwarePath; //Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));
                    communicationParameterObj.modelID = Convert.ToInt32(AppData.ModelID);
                    communicationParameterObj.IPAddress = AppData.IPAddress;
                    txtBoxIPAddress.Text = AppData.IPAddress;


                    if (!File.Exists(AppData.EncryptionFilePath))
                    {
                        MessageBox.Show("Encryption File is missing.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        Environment.ExitCode = 0; 
                        Application.Exit();

                        return;
                    }



                    //string path = Directory.GetParent(communicationParameterObj.firmwareFilePath).FullName + "\\";
                    string path = communicationParameterObj.firmwareFilePath;

                    //Log.Information("Firmware file path: {Path}", path);


                    //byte[] key = GenerateRandomBytes(16); // AES-128 requires a 16-byte key
                    //byte[] iv = GenerateRandomBytes(16);  // IV should also be 16 bytes

                    // WriteEncryptionKeyFile(key, iv, path); // check path with sagar

                    //WriteEncryptionKeyFile(key, iv, path, communicationParameterObj.IsResetFirmwareEncryptionMode);

                    string rootPath = AppData.EncryptionFilePath;

                    byte[] key = new byte[16];
                    byte[] iv = new byte[16];

                    getKeyIV(rootPath, key, iv);

                    //EncryptFile(communicationParameterObj.firmwareFilePath,communicationParameterObj.firmwareFilePath + "_encrypted", key, iv);

                    DecryptFile(communicationParameterObj.firmwareFilePath, Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Firmware_decrypted"), key, iv);


                    string filenameForDecryptedFirmwareCRC = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Firmware_decrypted");

                    //filenameForDecryptedFirmwareCRC += "_decrypted";

                    try
                    {
                        // Read the entire file as a byte array
                        byte[] fileBytes = File.ReadAllBytes(filenameForDecryptedFirmwareCRC);

                        //Log.Information("Decrypted firmware file size: {Size} bytes", fileBytes.Length);

                        communicationParameterObj.FrmDcryptionSize = fileBytes.Length;

                        // Call your CRC calculation function
                        communicationParameterObj.FrmDcryptionCRC = CalculateCRC(fileBytes, 0, (int)fileBytes.Length);

                        // Log.Information("Decrypted firmware CRC: {CRC-0}", communicationParameterObj.FrmDcryptionCRC[0]);
                        // Log.Information("Decrypted firmware CRC: {CRC-1}", communicationParameterObj.FrmDcryptionCRC[1]);

                        if (File.Exists(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Firmware_decrypted")))
                        {
                            File.Delete(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Firmware_decrypted"));

                            //  Log.Information("Decrypted firmware file deleted: {Path}", Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Firmware_decrypted"));
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }

                    //
                }
                else
                {

                    List<string> projectProductIdAndModelId = _getProjectModelIdForFirmawarFilePath();

                    // ****************** IMP ****************** //
                    // Here the LIST will Hold 0th Index For The Product Id and 1th Index will Hold the Model Id

                    string projectProductId = Enum.GetName(typeof(CONSTANTS.ProjectProductId), Convert.ToInt32(projectProductIdAndModelId[0]));

                    communicationParameterObj.firmwareFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));

                    communicationParameterObj.firmwareFilePath += "\\FirmWareFiles\\" + projectProductId + "\\" + projectProductIdAndModelId[0];

                    communicationParameterObj.modelID = Convert.ToInt32(projectProductIdAndModelId[0]);
                }
            }

            if (_standAloneUtility == false)
            {
                //List<string> projectProductIdAndModelId = _getProjectModelIdForFirmawarFilePath();

                // ****************** IMP ****************** //
                // Here the LIST will Hold 0th Index For The Product Id and 1th Index will Hold the Model Id

                //string projectProductId = Enum.GetName(typeof(CONSTANTS.ProjectProductId), Convert.ToInt32(projectProductIdAndModelId[0]));

                //communicationParameterObj.firmwareFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));

                //communicationParameterObj.firmwareFilePath += "\\FirmWareFiles\\" + projectProductId + "\\" + projectProductIdAndModelId[1].ToString();

                //communicationParameterObj.modelID = Convert.ToInt32(projectProductIdAndModelId[1]);
                communicationParameterObj.firmwareFilePath = AppData.FirmwarePath; //Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));

                //communicationParameterObj.firmwareFilePath += "\\FirmWareFiles\\" + AppData.Product + "\\" + AppData.ModelID.ToString();

                communicationParameterObj.modelID = Convert.ToInt32(AppData.ModelID);
            }

            communicationParameterObj._isDownload = _isDownload;

            communicationParameterObj._isStandAloneUtility = _standAloneUtility;

            //Log.Information("BtnDownload_Click: Mode={Mode}, Port={Port}, Baud={Baud}, Firmware={Firmware}, App={App}",
            //listBox1.SelectedItem, cmbPortName.Text, cmbBaudRate.SelectedItem, chkBoxFirmawareFile.Checked, chkBoxApplicationFile.Checked);

            communicationParameterObj.modelID = Convert.ToInt32(AppData.ModelID);
            communicationParameterObj.encryptionFilePath = AppData.EncryptionFilePath;

            _connectUIserialToController(communicationParameterObj);
        }

        private void getKeyIV(string rootPath, byte[] key, byte[] iv)
        {
            FileStream pFileStream = new FileStream(rootPath, FileMode.Open);

            byte[] buffer1 = new byte[128];


            Array.Clear(buffer1, 0, 128);
            pFileStream.Read(buffer1, 0, 128);

            int index = 2;
            Array.Copy(buffer1, index, key, 0, 4);
            index += 12;
            Array.Copy(buffer1, index, key, 4, 4);
            index += 12;
            Array.Copy(buffer1, index, key, 8, 4);
            index += 12;
            Array.Copy(buffer1, index, key, 12, 4);
            index += 12;

            Array.Copy(buffer1, index, iv, 0, 4);
            index += 12;
            Array.Copy(buffer1, index, iv, 4, 4);
            index += 12;
            Array.Copy(buffer1, index, iv, 8, 4);
            index += 12;
            Array.Copy(buffer1, index, iv, 12, 4);

            pFileStream.Close();
            pFileStream.Dispose();
        }

        static byte[] GenerateRandomBytes(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);
                return data;
            }
        }

        static void DecryptFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            // Log.Information("DecryptFile: Starting decryption. InputFile={InputFile}, OutputFile={OutputFile}", inputFile, outputFile);

            var binFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)));

            var files = binFiles.Where(f => f.Contains("_decrypted")).ToList();
            foreach (var binFile in files)
                File.Delete(binFile);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                using (var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                using (var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(outputStream);
                }
            }

            // Log.Information("DecryptFile: Decryption completed successfully. OutputFile={OutputFile}", outputFile);
        }


        static void EncryptFile(string inputFile, string outputFile, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor())
                using (var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                {
                    inputStream.CopyTo(cryptoStream);
                }
            }
        }

        private void WriteEncryptionKeyFile(byte[] pKey, byte[] pIv, string pPath, bool pIsResetFirmwareEncryptionMode)
        {

            FileStream fileStream;

            using (fileStream = new FileStream(Path.Combine(pPath + "EncryptionKeyFile"), FileMode.Create))
            {

                byte[] keyChunk1 = new byte[4];
                byte[] keyChunk2 = new byte[4];
                byte[] keyChunk3 = new byte[4];
                byte[] keyChunk4 = new byte[4];

                Array.Copy(pKey, 0, keyChunk1, 0, Math.Min(4, pKey.Length));
                Array.Copy(pKey, 4, keyChunk2, 0, Math.Min(4, pKey.Length - 4));
                Array.Copy(pKey, 8, keyChunk3, 0, Math.Min(4, pKey.Length - 8));
                Array.Copy(pKey, 12, keyChunk4, 0, Math.Min(4, pKey.Length - 12));

                byte[] IvChunk1 = new byte[4];
                byte[] IvChunk2 = new byte[4];
                byte[] IvChunk3 = new byte[4];
                byte[] IvChunk4 = new byte[4];

                Array.Copy(pIv, 0, IvChunk1, 0, Math.Min(4, pKey.Length));
                Array.Copy(pIv, 4, IvChunk2, 0, Math.Min(4, pKey.Length - 4));
                Array.Copy(pIv, 8, IvChunk3, 0, Math.Min(4, pKey.Length - 8));
                Array.Copy(pIv, 12, IvChunk4, 0, Math.Min(4, pKey.Length - 12));

                if (pIsResetFirmwareEncryptionMode)
                {
                    fileStream.WriteByte(0);//Identifier
                    fileStream.WriteByte(0);//Identifier
                }
                else
                {

                    fileStream.WriteByte(69);//Identifier
                    fileStream.WriteByte(70);//Identifier
                }

                fileStream.Write(keyChunk1, 0, keyChunk1.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes

                fileStream.Write(keyChunk2, 0, keyChunk2.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes

                fileStream.Write(keyChunk3, 0, keyChunk3.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes

                fileStream.Write(keyChunk4, 0, keyChunk4.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes


                fileStream.Write(IvChunk1, 0, IvChunk1.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes

                fileStream.Write(IvChunk2, 0, IvChunk2.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes

                fileStream.Write(IvChunk3, 0, IvChunk3.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes

                fileStream.Write(IvChunk4, 0, IvChunk4.Length);//Key

                fileStream.Write(GenerateRandomBytes(8), 0, 8);//DummyBytes

                fileStream.Write(GenerateRandomBytes(30), 0, 30);//DummyBytes

                fileStream.Close();

                fileStream.Dispose();
            }
        }




        public byte[] CalculateCRC(byte[] _buffer, int startingIndex, int lengthOfBytes)
        {
            byte[] _crcValues = new byte[2];
            byte _btmCRCHi;    //higher byte of CRC initialized.
            byte _btmCRCLo;    //Lower byte of CRC initialized.
            int _imIndex;
            int _imCounter;
            byte _btmByte;
            _btmCRCHi = 0xFF;
            _btmCRCLo = 0xFF;
            _imCounter = startingIndex;
            while (true)
            {
                _btmByte = _buffer[_imCounter];
                _imIndex = _btmCRCLo ^ _btmByte;
                _btmCRCLo = (byte)((_btmCRCHi) ^ (_auchCRCHi[_imIndex]));
                _btmCRCHi = _auchCRCLo[_imIndex];
                _imCounter++;
                if (_imCounter > lengthOfBytes + startingIndex - 1)
                {
                    _crcValues[0] = _btmCRCHi;   // Higher byte is stored in the buffer
                    _crcValues[1] = _btmCRCLo;   // Lower byte is stored in the buffer
                    break;
                }
            }
            return _crcValues;
        }
        //Table of CRC values for high–order byte
        private byte[] _auchCRCHi = new byte[]{0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81,
                                      0x40, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0,
                                      0x80, 0x41, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81, 0x40, 0x1,
                                      0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x1, 0xC0, 0x80, 0x41,
                                      0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81,
                                      0x40, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x1, 0xC0,
                                      0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x1,
                                      0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40,
                                      0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81,
                                      0x40, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0,
                                      0x80, 0x41, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81, 0x40, 0x1,
                                      0xC0, 0x80, 0x41, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41,
                                      0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81,
                                      0x40, 0x1, 0xC0, 0x80, 0x41, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0,
                                      0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x1,
                                      0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40, 0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41,
                                      0x0, 0xC1, 0x81, 0x40, 0x1, 0xC0, 0x80, 0x41, 0x1, 0xC0, 0x80, 0x41, 0x0, 0xC1, 0x81, 0x40};
        // Table of CRC values for low–order byte
        private byte[] _auchCRCLo = new byte[] {0x0, 0xC0, 0xC1, 0x1, 0xC3, 0x3, 0x2, 0xC2, 0xC6, 0x6, 0x7, 0xC7, 0x5, 0xC5, 0xC4,
                                       0x4, 0xCC, 0xC, 0xD, 0xCD, 0xF, 0xCF, 0xCE, 0xE, 0xA, 0xCA, 0xCB, 0xB, 0xC9, 0x9,
                                       0x8, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD,
                                       0x1D, 0x1C, 0xDC, 0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
                                       0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7,
                                       0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
                                       0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE,
                                       0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
                                       0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2,
                                       0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
                                       0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB,
                                       0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
                                       0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0, 0x50, 0x90, 0x91,
                                       0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
                                       0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88,
                                       0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
                                       0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80, 0x40};


        internal void SetText(string text, CurrentDownload pCurrentDownload)
        {
            if (this.rtxtDataArea.InvokeRequired)
            {
                //rtxtDataArea.ForeColor = Color.Green;
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                int val = 0;
                int.TryParse(text, out val);

                if (val < 0)
                {
                    // btnDownload.Enabled = true;

                    listBox1.Enabled = true;
                    chkBoxFirmawareFile.Enabled = true;
                    chkBoxApplicationFile.Enabled = true;
                    chkboxEthernetSettings.Enabled = true;
                    chbxMacID.Enabled = true;
                    checkBoxRTCSync.Enabled = true;
                    chkBoxFontFile.Enabled = true;
                    chkBoxLadderFile.Enabled = true;
                    grpBoxUSB.Enabled = true;
                    grpBoxEthernet.Enabled = true;
                    grpBoxCOMPort.Enabled = true;

                    if (val == -1)
                    {
                        toolStripStatusLabel2.Text = "Communication Error"; // "Acknowledgement Error";
                        downloadInProgress = false;
                    }
                    else if (val == -2)
                    {
                        toolStripStatusLabel2.Text = "Success";
                        downloadInProgress = false;
                    }
                    else if (val == -3)
                    {
                        toolStripStatusLabel2.Text = "Read Timeout Error"; // "PageError";
                        downloadInProgress = false;
                    }
                    else if (val == -4)
                    {
                        toolStripStatusLabel2.Text = "Write Timeout Error"; // "PageError";
                        downloadInProgress = false;
                    }
                    else if (val == -5)
                    {
                        toolStripStatusLabel2.Text = "Communication terminated!";
                        downloadInProgress = false;
                    }
                    else if (val == -6)
                    {
                        toolStripStatusLabel1.Text = "Product Mismatched!";
                        downloadInProgress = false;
                    }
                    else if (val == -7)
                    {
                        toolStripStatusLabel2.Text = "Communication Error"; // "PageError";
                        downloadInProgress = false;
                    }
                    else if (val == -8)
                    {
                        toolStripStatusLabel2.Text = "File Not Exists"; // "PageError";
                        downloadInProgress = false;
                    }
                    else if (val == -9)
                    {
                        toolStripStatusLabel2.Text = "No Response From Device"; // "PageError";
                        downloadInProgress = false;
                    }
                    else if (val == -10)
                    {
                        toolStripStatusLabel1.Text = "Connecting to Device"; // "connecting";
                        toolStripProgressBar1.Value = 0;
                        toolStripStatusLabel2.Text = 0 + " %";

                        downloadInProgress = false;
                        btnDownload.Enabled = false;
                        // btnClose.Enabled = false;

                        chkBoxFirmawareFile.Enabled = false;
                        chkBoxApplicationFile.Enabled = false;
                        chkboxEthernetSettings.Enabled = false;
                        chbxMacID.Enabled = false;
                        checkBoxRTCSync.Enabled = false;
                        chkBoxFontFile.Enabled = false;
                        chkBoxLadderFile.Enabled = false;

                        listBox1.Enabled = false;
                        grpBoxUSB.Enabled = false;
                        grpBoxEthernet.Enabled = false;
                        grpBoxCOMPort.Enabled = false;


                    }
                    else if (val == -11)
                    {
                        toolStripStatusLabel1.Text = "Communication terminated"; // "connecting";
                        downloadInProgress = false;
                        btnDownload.Enabled = false;
                        btnClose.Enabled = false;

                    }
                    else if (val == -12)
                    {
                        btnDownload.Enabled = true;

                        toolStripStatusLabel1.Text = "Completed";

                    }
                }
                else if (pCurrentDownload == CurrentDownload.MSG_DOWNLOAD_FAILED || pCurrentDownload == CurrentDownload.MSG_DOWNLOAD_SUCCESS)
                {
                    btnClose.Enabled = true;
                    chkBoxFirmawareFile.Enabled = true;
                    chkBoxApplicationFile.Enabled = true;
                    //chkboxEthernetSettings.Enabled = true;
                    chbxMacID.Enabled = true;
                    checkBoxRTCSync.Enabled = true;
                    chkBoxFontFile.Enabled = true;
                    chkBoxLadderFile.Enabled = true;

                    listBox1.Enabled = true;
                    grpBoxUSB.Enabled = true;
                    grpBoxEthernet.Enabled = true;
                    grpBoxCOMPort.Enabled = true;
                }
                else
                {
                    toolStripProgressBar1.Value = int.Parse(text);

                    toolStripStatusLabel2.Text = text + " %";
                    btnClose.Enabled = true;

                    if (_isDownload)
                        toolStripStatusLabel1.Text = pCurrentDownload + " download in progress...";
                    else
                        toolStripStatusLabel1.Text = pCurrentDownload + " upload in progress...";

                    downloadInProgress = true;
                }

                if (text == "0" && pCurrentDownload == CurrentDownload.MSG_DOWNLOAD_FAILED)
                {
                    //toolStripProgressBar1.Value = 0;
                    btnDownload.Enabled = true;
                    toolStripStatusLabel1.Text = "Download Failed.";
                    downloadInProgress = false;
                }
                else if (text == "0")
                {
                    toolStripStatusLabel1.Text = "Ready";
                    toolStripProgressBar1.Value = 0;

                    toolStripStatusLabel2.Text = 0 + " %";


                    downloadInProgress = true;
                }
                else if (text == "100")
                {
                    // toolStripStatusLabel1.Text = "Completed";

                    //if (_isDownload)
                    //    toolStripStatusLabel1.Text = pCurrentDownload + " download completed.";
                    //else
                    //    toolStripStatusLabel1.Text = pCurrentDownload + " upload in completed.";

                    downloadInProgress = false;
                }
                else
                {

                }

                statusStrip1.Refresh();
            }
        }

        internal void SetDebugText(string text, CurrentDownload pCurrentDownload)
        {
            if (this.rtxtDataArea.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetDebugText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.rtxtDataArea.AppendText("\n");
                this.rtxtDataArea.AppendText(text);

                statusStrip1.Refresh();

                //Refresh();
            }
        }

        public void GroupboxHide(bool fileGrpBox)
        {
            if (fileGrpBox == true)
                groupBoxFile.Visible = true;
            else
                groupBoxFile.Visible = false;
        }

        private void BtnFirmwareBrowse_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                txtFirmwareFilePath.Text = path;
            }
        }


        private void BtnApplicationBrowse_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                txtAppFilePath.Text = path;
            }
        }

        private void BtnLadderBrowse_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                txtLadderFilePath.Text = path;
            }
        }

        private void BtnFontFileBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                txtFontFilePath.Text = path;
            }
        }
        private void CmbBoxFirmawareFile_CheckStateChanged(object sender, EventArgs e)
        {
            lblFirmPath.Visible = chkBoxFirmawareFile.Checked;
            txtFirmwareFilePath.Visible = chkBoxFirmawareFile.Checked;
            btnFirmwareBrowse.Visible = chkBoxFirmawareFile.Checked;
        }

        private void ChkBoxApplicationFile_CheckStateChanged(object sender, EventArgs e)
        {
            lblAppPath.Visible = chkBoxApplicationFile.Checked;
            txtAppFilePath.Visible = chkBoxApplicationFile.Checked;
            btnApplicationBrowse.Visible = chkBoxApplicationFile.Checked;
        }

        private void ChkBoxLadderFile_CheckStateChanged(object sender, EventArgs e)
        {
            lblLadPath.Visible = chkBoxLadderFile.Checked;
            txtLadderFilePath.Visible = chkBoxLadderFile.Checked;
            BtnLadderBrowse.Visible = chkBoxLadderFile.Checked;
        }

        private void ChkBoxFontFile_CheckStateChanged(object sender, EventArgs e)
        {
            lblFontPath.Visible = chkBoxFontFile.Checked;
            txtFontFilePath.Visible = chkBoxFontFile.Checked;
            BtnFontFileBrowse.Visible = chkBoxFontFile.Checked;
        }

        private void Btnethernet_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;
                txtBoxEthernetFile.Text = path;
            }
        }

        private void ChkboxEthernetSettings_CheckStateChanged(object sender, EventArgs e)
        {
            lblethernetFile.Visible = chkboxEthernetSettings.Checked;
            txtBoxEthernetFile.Visible = chkboxEthernetSettings.Checked;
            btnethernet.Visible = chkboxEthernetSettings.Checked;
            btnConfigure.Visible = chkboxEthernetSettings.Checked;
        }

        private void chbxMacID_CheckedChanged(object sender, EventArgs e)
        {
            //txtMacID.Visible = chbxMacID.Checked;
            ShowHideDownloadButton();
        }

        private void btnConfigure_Click(object sender, EventArgs e)
        {
            EthernetConfiguration EthernetConfigurationObj = new EthernetConfiguration();
            EthernetConfigurationObj._getEthernetConfiguredFilePath += EthernetConfigurationObj__getEthernetConfiguredFilePath1;
            EthernetConfigurationObj.ShowDialog();

            txtBoxEthernetFile.Text = ethernetFilePath;
        }

        private void EthernetConfigurationObj__getEthernetConfiguredFilePath1(string filepath)
        {
            ethernetFilePath = filepath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        bool isclosed = false;
        private void Download_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (btnClose.Enabled)
            {
                Application.Exit();
                //this.Close();

            }
            else
            {
                e.Cancel = true;
            }

            Settings.Default.IP_Address = txtBoxIPAddress.Text;
            Settings.Default.Port_Number = txtBoxPortNumber.Text;
            Settings.Default.Save();
        }

        private void chkBoxFirmawareFile_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideDownloadButton();
        }

        private void chkBoxApplicationFile_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideDownloadButton();
        }

        private void chkBoxLadderFile_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideDownloadButton();
        }

        private void chkBoxFontFile_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideDownloadButton();
        }

        private void chkboxEthernetSettings_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideDownloadButton();
        }

        private void ShowHideDownloadButton()
        {
            if (cmbPortName.Items.Count == 0 && listBox1.SelectedIndex == 0)
            {
                btnDownload.Enabled = false;
            }
            else
            {
                if (chkboxEthernetSettings.Checked || chkBoxFontFile.Checked || chkBoxLadderFile.Checked ||
                     chkBoxApplicationFile.Checked || chkBoxFirmawareFile.Checked || chbxMacID.Checked)
                {
                    btnDownload.Enabled = true;
                    listBox1.Enabled = true;
                    chkBoxFirmawareFile.Enabled = true;
                    chkBoxApplicationFile.Enabled = true;
                    if (IsEthernetModel)
                        chkboxEthernetSettings.Enabled = true;
                    chbxMacID.Enabled = true;
                    checkBoxRTCSync.Enabled = true;
                    chkBoxFontFile.Enabled = true;
                    chkBoxLadderFile.Enabled = true;
                    grpBoxUSB.Enabled = true;
                    grpBoxEthernet.Enabled = true;
                    grpBoxCOMPort.Enabled = true;
                    checkBoxRTCSync.Enabled = false;

                }
                else if (checkBoxRTCSync.Checked)
                {
                    btnDownload.Enabled = true;
                    listBox1.Enabled = true;
                    chkBoxFirmawareFile.Enabled = true;
                    chkBoxApplicationFile.Enabled = true;
                    chkboxEthernetSettings.Enabled = true;
                    chbxMacID.Enabled = false;
                    checkBoxRTCSync.Enabled = true;
                    chkBoxFontFile.Enabled = true;
                    chkBoxLadderFile.Enabled = true;
                    grpBoxUSB.Enabled = true;
                    grpBoxEthernet.Enabled = true;
                    grpBoxCOMPort.Enabled = true;

                }
                else
                {
                    btnDownload.Enabled = false;
                    if (listBox1.SelectedIndex != 0)
                        checkBoxRTCSync.Enabled = true;

                }
            }
        }

        /// <summary>
        ///To Enable All Controls For Next Operation (Hapyy/Worst Condition)
        /// </summary>
        internal void RefreshUI()
        {
            checkBoxRTCSync.Enabled = true;
            listBox1.Enabled = true;
            chkBoxFirmawareFile.Enabled = true;
            chkBoxApplicationFile.Enabled = true;
            chkboxEthernetSettings.Enabled = true;
            chbxMacID.Enabled = false;
            checkBoxRTCSync.Enabled = true;
            chkBoxFontFile.Enabled = true;
            chkBoxLadderFile.Enabled = true;
            grpBoxUSB.Enabled = true;
            grpBoxEthernet.Enabled = true;
            grpBoxCOMPort.Enabled = true;
        }

        private void checkBoxRTCSync_CheckedChanged(object sender, EventArgs e)
        {
            ShowHideDownloadButton();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            CommunicationParameters communicationParameterObj = new CommunicationParameters();

            if (listBox1.SelectedItem.ToString() == "Serial")
            {
                communicationParameterObj.Mode = CommunicationParameters.CommunicationMode.Serial;
                communicationParameterObj.PortName = cmbPortName.Text;
                communicationParameterObj.BaudRate = Convert.ToInt32(cmbBaudRate.SelectedItem.ToString());
                communicationParameterObj.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
                communicationParameterObj.DataBit = Convert.ToInt32(cmbDataBits.Text);
                communicationParameterObj.StopBit = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBit.Text);
            }
            else if (listBox1.SelectedItem.ToString() == "USB")
            {
                communicationParameterObj.Mode = CommunicationParameters.CommunicationMode.Serial;

                communicationParameterObj.BaudRate = Convert.ToInt32(cmbBaudRate.SelectedItem.ToString());
                communicationParameterObj.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
                communicationParameterObj.DataBit = Convert.ToInt32(cmbDataBits.Text);
                communicationParameterObj.StopBit = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBit.Text);

                if (ListOfUSBPorts.Count == 0)
                {
                    MessageBox.Show("USB Device not recognized.", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    return;
                }

                communicationParameterObj.PortName = ListOfUSBPorts[cmbxUSBPortDetails.SelectedIndex].Item1;
            }
            else if (listBox1.SelectedItem.ToString() == "Ethernet")
            {
                communicationParameterObj.Mode = CommunicationParameters.CommunicationMode.Ethernet;
                communicationParameterObj.IPAddress = txtBoxIPAddress.Text;
                communicationParameterObj.PortNumber = Convert.ToInt32(txtBoxPortNumber.Text);
            }

            if (chkBoxApplicationFile.Checked == true)
            {

            }

            string applicationFilePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "ApplicationFileUPLD");

            communicationParameterObj.applicationFilePath = applicationFilePath;

            communicationParameterObj.isFirmware = chkBoxFirmawareFile.Checked;
            communicationParameterObj.isApplication = chkBoxApplicationFile.Checked;
            communicationParameterObj.isFont = chkBoxFontFile.Checked;
            communicationParameterObj.isLadder = chkBoxLadderFile.Checked;
            communicationParameterObj.isMacId = chbxMacID.Checked;
            communicationParameterObj.isEthernetSettings = chkboxEthernetSettings.Checked;
            communicationParameterObj.isRTCSync = checkBoxRTCSync.Checked;



            communicationParameterObj._isDownload = _isDownload;

            communicationParameterObj._isStandAloneUtility = _standAloneUtility;

            _connectUIserialToController(communicationParameterObj);

        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult result;
            bool ProcessInProgress = _checkThreadAlive();
            if (ProcessInProgress)
            {
                result = MessageBox.Show("Do you want stop current process?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _killTheProcess();
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }

        }


        private void txtBoxIPAddress_Leave(object sender, EventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtBoxIPAddress.Text,
               "^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
            {
                MessageBox.Show("Please enter valid Ip address", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBoxIPAddress.Text = "192.168.0.11";
            }
        }


        private void txtBoxPortNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            RestrictChar(e);
        }
        private void RestrictChar(KeyPressEventArgs e)
        {
            // Allow digits, backspace, and delete keys
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != 0x7f)
            {
                e.Handled = true; // Handle the keypress event to prevent the character from being entered
            }
        }

        private void txtBoxPortNumber_Leave(object sender, EventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtBoxPortNumber.Text,
               "^(10[2-9][4-9]|[1-5][0-9]{3}|6[0-4][0-9]{2}|65[0-4][0-9]|655[0-35])$"))
            {
                MessageBox.Show("Port number value out of range.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBoxPortNumber.Text = "1024";
            }
        }

        private void cmbPortName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

