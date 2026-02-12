using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortCommunication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            try
            {
                AppData.Port = Environment.GetEnvironmentVariable("Port");
                AppData.DownloadFirmware = Environment.GetEnvironmentVariable("DownloadFirmware");
                AppData.FirmwarePath = Environment.GetEnvironmentVariable("FirmwarePath");
                AppData.AppPath = Environment.GetEnvironmentVariable("AppPath");
                AppData.IPAddress = Environment.GetEnvironmentVariable("IPAddress");
                AppData.CommunicationType = Environment.GetEnvironmentVariable("CommunicationType");
                AppData.DownloadMACID = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadMACID"));
                AppData.MACIDFilePath = Environment.GetEnvironmentVariable("MACIDFilePath");
                AppData.ModelID = Convert.ToInt32(Environment.GetEnvironmentVariable("ModelID"));
                AppData.DownloadEthernetSetting = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadEthernetSetting"));
                AppData.EthernetSettingFilePath = Environment.GetEnvironmentVariable("EthernetSettingFilePath");
                AppData.DownloadApplicationFile = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadApplicationFile"));
                AppData.DownloadRTC = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadRTC"));

 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Controller objController = new Controller();

            bool isDownload = true;//To Check with Kapil Sir
            bool uploadSuccess = false;
            try
            {
                Application.Run(objController.init("Utility", isDownload, ref uploadSuccess, null));
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.ToString());
            }

            return Convert.ToInt32(AppData.ResultStatus);
        }
    }
}
