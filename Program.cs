using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

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
            // configure Serilog
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var logsDir = System.IO.Path.Combine(exeDir, "Logs");
            System.IO.Directory.CreateDirectory(logsDir);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    System.IO.Path.Combine(logsDir, "app_.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();


            try
            {
                var dictionary = Environment.GetEnvironmentVariables();
                Log.Information($"Total {dictionary.Count} variables found in Env.");
                //foreach (var key in dictionary.Keys)
                //{
                //    Log.Information("Key: {Key} , Value: {Value}", key, dictionary[key]);
                //}
                AppData.Port = Environment.GetEnvironmentVariable("Port");
                AppData.DownloadFirmware = Environment.GetEnvironmentVariable("DownloadFirmware");
                AppData.FirmwarePath = Environment.GetEnvironmentVariable("FirmwarePath");
                Log.Information("program .cs Firmware Path: {FirmwarePath}", AppData.FirmwarePath);


                AppData.EncryptionFilePath = Environment.GetEnvironmentVariable("EncryptionFilePath");
                Log.Information(" program .cs Encryption File Path: {EncryptionFilePath}", AppData.EncryptionFilePath);

                AppData.AppPath = Environment.GetEnvironmentVariable("AppPath");

                Log.Information(" program .cs AppPath File Path: {AppPath}", AppData.AppPath);

                AppData.IPAddress = Environment.GetEnvironmentVariable("IPAddress");
                AppData.CommunicationType = Environment.GetEnvironmentVariable("CommunicationType");
                AppData.DownloadMACID = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadMACID"));
                AppData.MACIDFilePath = Environment.GetEnvironmentVariable("MACIDFilePath");
                AppData.ModelID = Convert.ToInt32(Environment.GetEnvironmentVariable("ModelID"));
                AppData.DownloadEthernetSetting = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadEthernetSetting"));
                AppData.EthernetSettingFilePath = Environment.GetEnvironmentVariable("EthernetSettingFilePath");
                AppData.DownloadApplicationFile = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadApplicationFile"));
                AppData.DownloadRTC = Convert.ToBoolean(Environment.GetEnvironmentVariable("DownloadRTC"));

                Log.Information("Application starting");

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");

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
