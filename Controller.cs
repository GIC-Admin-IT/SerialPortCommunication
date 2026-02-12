using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Supporting_DLL;

namespace SerialPortCommunication
{
    public class Controller
    {
        //Download dbobj = new Download();
        Thread thread;
        Download dbobj;

        Device deviceObj;
        bool _uploadsuccess = false;

        public delegate CONSTANTS.BinaryMemoryCalculation __saveBinaryFile(string applicationFilePath);
        public event __saveBinaryFile _saveBinaryFile;

        public delegate List<string> __getProjectModelIdForFirmawarFilePath();
        public event __getProjectModelIdForFirmawarFilePath _getProjectModelIdForFirmawarFilePath;

        public delegate bool __checkIsEthernetModel();
        public event __checkIsEthernetModel _checkIsEthernetModel;

        public delegate List<string> __GetAlarmDetails();
        public event __GetAlarmDetails _GetAlarmDetails;

        public delegate bool __checkThreadAlive();
        public event __checkThreadAlive _checkThreadAlive;

        public delegate void __killTheProcess();
        public event __killTheProcess _killTheProcess;

        public delegate int __uploadDataInSoftware();
        public event __uploadDataInSoftware _uploadDataInSoftware;

        public Form init(string callFrom, bool isDownload, ref bool _pUploadSuccess, Form pObjView)
        {
            dbobj = new Download();

            if (callFrom == "Utility")
                dbobj.GroupboxHide(true);
            else
                dbobj.GroupboxHide(false);

            dbobj._connectUIserialToController += FrmObj__connectUIserialToController;
            //frmObj._connectUIserialToController += FrmObj__connectUIserialToController;

            //deviceObj._showDownloadProgress += DeviceObj__showDownloadProgress;

            dbobj._saveBinaryFile += SaveBinaryFile;

            dbobj._getProjectModelIdForFirmawarFilePath += Dbobj__getProjectModelIdForFirmawarFilePath;

            dbobj._checkIsEthernetModel += Dbobj__checkIsEthernetModel;

            dbobj._GetAlarmDetails += Dbobj__GetAlarmDetails;

            dbobj._checkThreadAlive += Dbobj__checkThreadAlive;

            dbobj._killTheProcess += Dbobj__killTheProcess;

            dbobj.init(callFrom, isDownload, _pUploadSuccess, pObjView);

            _pUploadSuccess = _uploadsuccess;
            return dbobj;

            //return frmObj;
        }

        private void Dbobj__killTheProcess()
        {
            deviceObj._deviceShouldThreadExit = false;
        }

        private bool Dbobj__checkThreadAlive()
        {
            if (thread != null)
                return thread.IsAlive;
            else
                return false;

        }

        private bool Dbobj__checkIsEthernetModel()
        {
            return _checkIsEthernetModel();
        }

        private List<string> Dbobj__GetAlarmDetails()
        {
            return _GetAlarmDetails();
        }

        private List<string> Dbobj__getProjectModelIdForFirmawarFilePath()
        {
            return _getProjectModelIdForFirmawarFilePath();
        }

        private CONSTANTS.BinaryMemoryCalculation SaveBinaryFile(string applicationFilePath)
        {
            return _saveBinaryFile(applicationFilePath);
        }

        private void DeviceObj__showDownloadProgress(string pstrPersentage, int isProgressOrDebug, CurrentDownload pCurrentDownload)
        {
            dbobj.SetText(pstrPersentage, pCurrentDownload);

        }

        private int FrmObj__connectUIserialToController(CommunicationParameters obj)
        {
            //if (obj.Mode == CommunicationParameters.CommunicationMode.Serial)
            //{
            //    deviceObj = new Serial();
            //}
            //else if (obj.Mode == CommunicationParameters.CommunicationMode.Ethernet)
            //{
            //    deviceObj = new Ethernet();
            //}
            //else                                //USB
            //{
            //    deviceObj = new USB();
            //}

            if (obj.Mode == CommunicationParameters.CommunicationMode.Ethernet)
            {
                deviceObj = new Ethernet();
            }
            else 
            {
                deviceObj = new Serial();
            }


            AppData.ResultStatus = false;
            thread = new Thread(() => ConnectDownloadAndUpload(deviceObj, obj));
            deviceObj._deviceShouldThreadExit = true;
            thread.Priority = ThreadPriority.AboveNormal;
            thread.Start();

            Console.WriteLine("Thread Created");

            return 0;
        }

        private void ConnectDownloadAndUpload(Device deviceObj, CommunicationParameters obj)
        {
            deviceObj._showDownloadProgress += DeviceObj__showDownloadProgress;
            
            if (deviceObj.Initialize(obj) == 0)
            {
                Console.WriteLine("######################################################### Intialization done in controller class #########################################################");

                if (deviceObj.Connect() == 0)
                {
                    Console.WriteLine("#########################################################  Connect done in controller class #########################################################");

                    if (obj._isDownload == true)
                    {
                        if (deviceObj.SendFile() == (int)ErrorCode.Success)
                        {
                            deviceObj = null;
                            
                            AppData.ResultStatus = true;
                            if (AppData.DownloadMACID)
                            {
                                MessageBox.Show("Device MacID : " + AppData.DownloadedMacID);
                            }
                        }
                        
                        //dbobj.Close();

                        Application.Exit();

                    }
                    else
                    {
                        Console.WriteLine("#########################################################  Upload ReceiveFile start in controller class #########################################################");

                        if (deviceObj.ReceiveFile() == 0)
                        {
                            _uploadsuccess = true;
                            deviceObj = null;

                        }
                    }
                }
            }

            //dbobj.RefreshUI();
        }

        ///GauriJ - Changes Ended

    }
}
