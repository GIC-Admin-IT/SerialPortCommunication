using SerialPortCommunication.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortCommunication
{
    public partial class EthernetConfiguration : Form
    {
        public int[] IPAddressInInt;
        public int[] SubnetMaskInInt;
        public int DownloadPort;
        public int MonitoringPort;
        public int[] DefaultGatewayInInt;
        public int DHCP;

		public delegate void __getEthernetConfiguredFilePath(string filepath);
		public event __getEthernetConfiguredFilePath _getEthernetConfiguredFilePath;
		public EthernetConfiguration()
        {
            InitializeComponent();

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
			string IPAddress = txtIPAddress.Text;
			string[] IPAddress_SpliteArr = txtIPAddress.Text.ToString().Split('.');
			int IPAddress_SpliteArr0 = Convert.ToInt32(IPAddress_SpliteArr[0]);
			int IPAddress_SpliteArr1 = Convert.ToInt32(IPAddress_SpliteArr[1]);
			int IPAddress_SpliteArr2 = Convert.ToInt32(IPAddress_SpliteArr[2]);
			int IPAddress_SpliteArr3 = Convert.ToInt32(IPAddress_SpliteArr[3]);

			IPAddressInInt = new[] { IPAddress_SpliteArr0, IPAddress_SpliteArr1, IPAddress_SpliteArr2, IPAddress_SpliteArr3 };

			string SubnetMask = txtSubnetMask.Text;
			string[] SubnetMask_SpliteArr = txtSubnetMask.Text.ToString().Split('.');
			int SubnetMask_SpliteArr0 = Convert.ToInt32(SubnetMask_SpliteArr[0]);
			int SubnetMask_SpliteArr1 = Convert.ToInt32(SubnetMask_SpliteArr[1]);
			int SubnetMask_SpliteArr2 = Convert.ToInt32(SubnetMask_SpliteArr[2]);
			int SubnetMask_SpliteArr3 = Convert.ToInt32(SubnetMask_SpliteArr[3]);

			SubnetMaskInInt = new[] { SubnetMask_SpliteArr0, SubnetMask_SpliteArr1, SubnetMask_SpliteArr2, SubnetMask_SpliteArr3 };

			string DefaultGateway = txtDefaultGateway.Text;
			string[] DefaultGateway_SpliteArr = txtDefaultGateway.Text.ToString().Split('.');
			int DefaultGateway_SpliteArr0 = Convert.ToInt32(DefaultGateway_SpliteArr[0]);
			int DefaultGateway_SpliteArr1 = Convert.ToInt32(DefaultGateway_SpliteArr[1]);
			int DefaultGateway_SpliteArr2 = Convert.ToInt32(DefaultGateway_SpliteArr[2]);
			int DefaultGateway_SpliteArr3 = Convert.ToInt32(DefaultGateway_SpliteArr[3]);

			DefaultGatewayInInt = new[] { DefaultGateway_SpliteArr0, DefaultGateway_SpliteArr1, DefaultGateway_SpliteArr2, DefaultGateway_SpliteArr3 };

			DHCP = checkBoxDHCP.Checked == true ? 1 : 0;
			DownloadPort = Convert.ToInt32(txtDownloadPort.Text);
			MonitoringPort = Convert.ToInt32(txtMonitoringport.Text);
			//IPAddress = IPAddressInInt;
			//SubnetMask = SubnetMaskInInt;
			//DownloadPort = Convert.ToInt32(txtDownloadPort.Text);
			//MonitoringPort = Convert.ToInt32(txtMonitoringport.Text);
			//DefaultGateway = DefaultGatewayInInt;


			WriteEthernetConfigFile();


			MessageBox.Show("Ethenet Configuration File Saved", "File Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
			this.Close();
		}

		public void WriteEthernetConfigFile()
		{
			byte[] buffer1 = new byte[4];

			string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "EthernetSetting");

			FileStream pFileStream = new FileStream(filePath, FileMode.Create);

			

			pFileStream.Write(ObjectToByteArray(DHCP), 0, 1);       //DHCP

			pFileStream.Write(ObjectToByteArray(IPAddressInInt[0]), 0, 1);       //IP Address 0

			pFileStream.Write(ObjectToByteArray(IPAddressInInt[1]), 0, 1);       //IP Address 1

			pFileStream.Write(ObjectToByteArray(IPAddressInInt[2]), 0, 1);       //IP Address 2

			pFileStream.Write(ObjectToByteArray(IPAddressInInt[3]), 0, 1);       //IP Address 3


			pFileStream.Write(ObjectToByteArray(SubnetMaskInInt[0]), 0, 1);       //Subnet Mask 0

			pFileStream.Write(ObjectToByteArray(SubnetMaskInInt[1]), 0, 1);       //Subnet Mask 1

			pFileStream.Write(ObjectToByteArray(SubnetMaskInInt[2]), 0, 1);       //Subnet Mask 2

			pFileStream.Write(ObjectToByteArray(SubnetMaskInInt[3]), 0, 1);       //Subnet Mask 3


			pFileStream.Write(ObjectToByteArray(DefaultGatewayInInt[0]), 0, 1);   //Default Gateway 0

			pFileStream.Write(ObjectToByteArray(DefaultGatewayInInt[1]), 0, 1);   //Default Gateway 1

			pFileStream.Write(ObjectToByteArray(DefaultGatewayInInt[2]), 0, 1);   //Default Gateway 2

			pFileStream.Write(ObjectToByteArray(DefaultGatewayInInt[3]), 0, 1);   //Default Gateway 3


			pFileStream.Write(ObjectToByteArray(DownloadPort), 0, 2);         //DownloadPort

			pFileStream.Write(ObjectToByteArray(MonitoringPort), 0, 2);       //Monitoring Port


			_getEthernetConfiguredFilePath(filePath);

			pFileStream.Close();

			pFileStream.Dispose();

		}
		byte[] ObjectToByteArray(int value)
		{
			return BitConverter.GetBytes(value);


			byte[] intBytes = BitConverter.GetBytes(value);
			Array.Reverse(intBytes);
			byte[] result = intBytes;

			return result;
		}

        private void btnClose_Click(object sender, EventArgs e)
        {
			this.Close();
        }       

        private void EthernetConfiguration_Load(object sender, EventArgs e)
        {
			txtIPAddress.Text = Settings.Default.IP_Address;
			txtSubnetMask.Text = Settings.Default.SubnetMask;
			txtDownloadPort.Text = Settings.Default.Port_Number.ToString();
			txtDefaultGateway.Text = Settings.Default.DefaultGateway;
			txtMonitoringport.Text = Settings.Default.MonitoringPort.ToString();
		}

        private void EthernetConfiguration_FormClosed_1(object sender, FormClosedEventArgs e)
        {
			Settings.Default.IP_Address = txtIPAddress.Text;
			Settings.Default.SubnetMask = txtSubnetMask.Text;
			Settings.Default.Port_Number = (txtDownloadPort.Text);
			Settings.Default.DefaultGateway = txtDefaultGateway.Text;
			Settings.Default.MonitoringPort = (txtMonitoringport.Text);
			Settings.Default.Save();
		}
    }
}
