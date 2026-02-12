using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortCommunication
{
    public static class AppData
    {
        public static string CommunicationType= "USB";
        public static string IsEthernet="true";
        public static string IPAddress="192.168.0.11";
        public static string Port="COM5";
        public static string DownloadFirmware="false";
        public static string DownloadLadder;
        public static bool DownloadEthernetSetting=false;
        public static bool DownloadApplicationFile = false;
        public static bool DownloadRTC = false;
        public static bool DownloadMACID = false;
        public static string MACIDFilePath="";

        public static bool DownloadLadderSetting=false;
        public static string EthernetSettingFilePath;
        public static string FirmwarePath;
        public static string AppPath;
        public static bool ResultStatus = false;
        public static string Product;
        public static int ModelID=22;
        public static string DownloadedMacID;
        static AppData() 
        {
            
        }
    }
}
