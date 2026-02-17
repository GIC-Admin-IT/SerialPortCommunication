using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Supporting_DLL;
using System.Diagnostics;
using Serilog;

namespace SerialPortCommunication
{
    class Serial : Device
    {
        volatile bool DataReceivedFlag = false;
        volatile bool ReadTimeout = false;
        private long ApplicationFileLength;
        private byte[] TxBuffer = new byte[8];
        private byte[] RxBuffer = new byte[8];
        private SerialPort _serialComPort = new SerialPort();
        private string _serialFWFilePath;
        private string _serialAppFilePath;
        private string _serialFontFilePath;
        private string _serialLadderFilePath;
        private string _serialEthernetSettingFilePath;
        private string _serialMacIDFilepath;
        private string _serialEncryptionFilePath;
        private string _serialSystemRTCFilepath;
        private long _serialFrmDcryptionSize;
        private byte[] _serialFrmDcryptionCRC;

        string _communicationMode;
        public string communicationMode { get => _communicationMode; set => _communicationMode = value; }

        private bool _isStandAloneUtility;

        int _modelID = 0;

        private int bytesToRead = 0;

        private byte[] WriteBuffer = new byte[262];
        private byte[] ReadBuffer = new byte[262];
        int _result;

        private const int FRAMEWITHHEADERS = 262;
        private const int FRAMEDATA = 256;
        System.Threading.Timer readTimeOutTimer; //= new System.Threading.Timer(readTimeoutTimerCallback);

        FileStream fs;

        private void readTimeoutTimerCallback(object state)
        {
            ReadTimeout = true;
        }

        void enableReadTimoutTimer(bool status, int time = 10000)
        {
            if (status)
            {
                readTimeOutTimer.Change(time, Timeout.Infinite);
            }
            else
            {
                readTimeOutTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public override int Initialize(CommunicationParameters pComParam)
        {
            readTimeOutTimer = new System.Threading.Timer(readTimeoutTimerCallback);

            if (_serialComPort.IsOpen)
                _serialComPort.Close();
            if (_serialComPort != null)
                _serialComPort = null;

            _serialComPort = new SerialPort();
            //updatePorts();
            _serialComPort.PortName = pComParam.PortName;

            if (pComParam.Mode == CommunicationParameters.CommunicationMode.Serial)
            {
                communicationMode = "Serial";
            }
            else
            {               
                communicationMode = "USB";
            }
            _serialComPort.BaudRate = pComParam.BaudRate;

            _serialComPort.Parity = pComParam.Parity;
            _serialComPort.DataBits = pComParam.DataBit;
            _serialComPort.StopBits = pComParam.StopBit;

            _serialAppFilePath = pComParam.applicationFilePath;
            _serialFWFilePath = pComParam.firmwareFilePath;
            _serialFontFilePath = pComParam.fontFilePath;
            _serialLadderFilePath = pComParam.ladderFilePath;
            _serialEthernetSettingFilePath = pComParam.ethernetFilePath;
            _serialMacIDFilepath = pComParam.macIdFilePath;
            _serialSystemRTCFilepath = pComParam.RTCSyncFilePath;
            _serialEncryptionFilePath= pComParam.encryptionFilePath;

           // Log.Information("Serial class Initialize method started with encryptionFilePath: " + pComParam.encryptionFilePath);

            _deviceFWDownload = pComParam.isFirmware;
            _deviceAppDownload = pComParam.isApplication;
            _deviceFontDownload = pComParam.isFont;
            _deviceLadderDownload = pComParam.isLadder;
            _deviceMacIdSettingDownload = pComParam.isMacId;
            _deviceEthernetSettingDownload = pComParam.isEthernetSettings;
            _deviceRTCSyncDownload = pComParam.isRTCSync;

            _isStandAloneUtility = pComParam._isStandAloneUtility;

            _modelID = pComParam.modelID;

          //  Log.Information("Serial class Initialize method started with Model ID: " + _modelID);

            _serialFrmDcryptionCRC = pComParam.FrmDcryptionCRC;
            _serialFrmDcryptionSize = pComParam.FrmDcryptionSize;

            return 0;
        }

        public override int Connect()
        {
            _serialComPort.Handshake = Handshake.None;
            _serialComPort.ReadTimeout = 10000;
            _serialComPort.WriteTimeout = 3000;

            try
            {
                _serialComPort.Open();
                _serialComPort.DataReceived += new SerialDataReceivedEventHandler(SerialPortDataReceived);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied, please check if another process is not using the resource.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException)
            {
                //MessageBox.Show(".", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                //MessageBox.Show("Something went wrong while connecting.\nPlease try again.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("Port is either busy or not detected.\nPlease try again.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return 0;
        }

        public override int SendFile()
        {
            ErrorCode res = ReadData();

            string msg = "";
            if(fs!= null)
            fs.Close();
            if (res == ErrorCode.Success)
            {
                msg = "File sent successfully";
                OnProgressEvent("100", 0, CurrentDownload.MSG_DOWNLOAD_SUCCESS);

            }
            else
            {
                msg = "Failed to send file,\nError Code: 0x" + (byte)res;
               OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);
            }

            return (int)res;

            //return -1;
        }
        public override int ReceiveFile()
        {
            return -1;
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            int noBytesToRead = serialPort.BytesToRead;
            Array.Clear(RxBuffer, 0, RxBuffer.Length);
            Array.Resize(ref RxBuffer, bytesToRead);

            try
            {
                int bytesReceived = serialPort.Read(RxBuffer, 0, bytesToRead);
                int bytesReceivedBackup = 0;

                Console.WriteLine(bytesReceived);


                while (bytesReceived < bytesToRead)
                {
                    bytesReceived += serialPort.Read(RxBuffer, bytesReceived, bytesToRead - bytesReceived);
                    if (bytesReceived != bytesReceivedBackup)
                    {
                        Console.WriteLine("ab");
                    }
                    bytesReceivedBackup = bytesReceived;
                }

                ReadTimeout = false;
            }
            catch (TimeoutException)
            {
                ReadTimeout = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Read exception");

            }

            DataReceivedFlag = true;
        }
        void kapsSleep(int pMicroSec)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < pMicroSec)
            {
                //int timeout = pMicroSec - (int)stopwatch.ElapsedMilliseconds;
                Thread.Sleep(0);
            }

            stopwatch.Stop();
        }


        public ErrorCode ReadData()
        {
            //Log.Information("Serial class ReadData method started");

            int retryCount = 0;
            OnProgressEvent("-10", 1, CurrentDownload.None);//Connecting device
            try
            {
                //a: if (retryCount > 2)
                //		return ErrorCode.ReadTimeoutError;

                string strSync = "";

                retryCount = 0;

                int _sendRetryCnt = 0;

                strSync = "GIC_SYNC";//Add 12 reserved bytes here
                byte[] data = ASCIIEncoding.ASCII.GetBytes(strSync);
                Array.Resize(ref data, 20);

                Array.Clear(TxBuffer, 0, TxBuffer.Length);
                Array.Resize(ref TxBuffer, data.Length);
                TxBuffer = data;
                TxBuffer[strSync.Length] = 35;

                _serialComPort.DiscardInBuffer();
                _serialComPort.DiscardOutBuffer();

                if (communicationMode == "USB")
                {
                    _serialComPort.RtsEnable = false;
                    _serialComPort.DtrEnable = false;
                    Thread.Sleep(1);
                    _serialComPort.RtsEnable = true;
                    _serialComPort.DtrEnable = true;
                }
                else
                {
                    _serialComPort.RtsEnable = false;
                    _serialComPort.DtrEnable = false;
                }

                Thread.Sleep(1);

                ReadTimeout = false;
                DataReceivedFlag = false;

                bytesToRead = 16;//change it to 10 as per communication with GA


                #region NEW CODE START

                while (true)
                {
                    try
                    {
                        _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);

                        //Log.Information("Serial class GIC_SYNC Frame Sent: " + BitConverter.ToString(TxBuffer));

                        if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                        {
                            retryCount++;

                            if (retryCount >= 3)
                            {
                                DiscardInOutSerialBUffers();

                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                return ErrorCode.NoResponseFromDevice;
                            }
                        }
                        else
                        {
                            _serialComPort.DiscardInBuffer();
                            _serialComPort.DiscardOutBuffer();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {                       
                    }

                    _sendRetryCnt++;

                    if (_sendRetryCnt >= 3)
                    {
                        OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                        return ErrorCode.NoResponseFromDevice;
                    }

                    if (_serialComPort.IsOpen)
                    {
                        _serialComPort.Close();
                    }

                    if (SerialPort.GetPortNames().Contains(_serialComPort.PortName))
                    {
                        _serialComPort.Open();
                    }
                }

                #endregion NEW CODE START

                #region OLD CODE SANTOSH 

                //while (true)
                //{
                //    strSync = "GIC_SYNC";//Add 12 reserved bytes here
                //    byte[] data = ASCIIEncoding.ASCII.GetBytes(strSync);
                //    Array.Resize(ref data, 20);

                //    Array.Clear(TxBuffer, 0, TxBuffer.Length);
                //    Array.Resize(ref TxBuffer, data.Length);
                //    TxBuffer = data;
                //    TxBuffer[strSync.Length] = 35;

                //    _serialComPort.DiscardInBuffer();
                //    _serialComPort.DiscardOutBuffer();

                //    if (communicationMode == "USB")
                //    {
                //        _serialComPort.RtsEnable = false;
                //        _serialComPort.DtrEnable = false;
                //        Thread.Sleep(1);
                //        _serialComPort.RtsEnable = true;
                //        _serialComPort.DtrEnable = true;
                //    }
                //    else
                //    {
                //        _serialComPort.RtsEnable = false;
                //        _serialComPort.DtrEnable = false;
                //    }

                //    Thread.Sleep(1);

                //    ReadTimeout = false;
                //    DataReceivedFlag = false;

                //    bytesToRead = 16;//change it to 10 as per communication with GA

                //    _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);

                //    if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                //    {
                //        retryCount++;

                //        if (retryCount >= 3)
                //        {
                //            //_serialComPort.DiscardInBuffer();
                //            //_serialComPort.DiscardOutBuffer();
                //            //_serialComPort.Close();
                //            //_serialComPort.Dispose();

                //            DiscardInOutSerialBUffers();

                //            OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                //            return ErrorCode.NoResponseFromDevice;
                //        }
                //    }
                //    else
                //    {
                //        _serialComPort.DiscardInBuffer();
                //        _serialComPort.DiscardOutBuffer();
                //        break;
                //    }
                //}

                #endregion OLD CODE SANTOSH 


                //   OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);

                //Thread.Sleep(5);//KV added

                retryCount = 0;
                string receivedData = ASCIIEncoding.ASCII.GetString(RxBuffer);

                //Log.Information("Serial class Received Data After GIC_SYNC Frame Sent: " + receivedData);

                //rtxtDataArea.AppendText(receivedData);.
                string compareDataFromRxBuffer = receivedData.Substring(0, 8);
                int modelID = (RxBuffer[8] + (RxBuffer[9] << 8));

               // Log.Information("Serial class Model ID from Device FW: " + modelID);

                int firmwareVersion = (RxBuffer[10] + (RxBuffer[11] << 8));
                bool _isBootDownload = (RxBuffer[14] + (RxBuffer[15] << 8)) == 0 ? true : false;

                int bootVersionNo = RxBuffer[12];

                //Log.Information("Serial class bootVersionNo: " + RxBuffer[12]);

                if (bootVersionNo >= 11 && _deviceFWDownload)
                    _deviceFWEncyptionDownload = true;

                Array.Clear(RxBuffer, 0, RxBuffer.Length);

                if (_isStandAloneUtility == true)
                {

                    //Log.Information("Serial class Model ID from Device: " + modelID+ "Stand "+ _isStandAloneUtility);

                    string filePath = "";
                    if (_deviceFWDownload)
                        filePath = _serialFWFilePath;
                    else if (_deviceAppDownload)
                        filePath = _serialAppFilePath;

                    if (filePath != "")
                    {
                        byte[] productId;
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            fs.Seek(fs.Length - 8, SeekOrigin.Begin);
                            productId = new byte[2];
                            fs.Read(productId, 0, productId.Length);
                            fs.Close();
                        } //FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);


                        //***************  IMPORTANT  ***************//
                        // Need To Check Model Id Is Greater Than 255//
                        _modelID = productId[0];
                    }

                    if (_deviceRTCSyncDownload == true && _deviceFWDownload == false && _deviceAppDownload == false
                                && _deviceEthernetSettingDownload == false && _deviceMacIdSettingDownload == false)
                    {
                        modelID = 0;
                        _modelID = 0;
                    }
                }

                if (strSync == compareDataFromRxBuffer && modelID == _modelID)
                {
                    //Log.Information("Serial class Model ID Matched: " + modelID);

                    retryCount = 0;
                    while (true)
                    {
                        string DOWNLOAD_INFO_0_Frame = "DOWNLOAD_INFO";
                        TxBuffer = ASCIIEncoding.ASCII.GetBytes(DOWNLOAD_INFO_0_Frame);

                        Array.Resize(ref TxBuffer, 35);

                        //TxBuffer[DOWNLOAD_INFO_0_Frame.Length] = (byte)((_deviceFWDownload ? 1 : 0) + (_deviceAppDownload ? 2 : 0) + (_deviceFontDownload ? 4 : 0) + (_ethernetSettingDownload ? 16 : 0)); // communication type bit number 1:Firmware 2:application 3:font 4:ladder 
                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length] = (byte)((_deviceFWDownload ? 1 : 0) + (_deviceAppDownload ? 2 : 0) + (_deviceLadderDownload ? 4 : 0) + (_deviceFontDownload ? 8 : 0)
                            + (_deviceEthernetSettingDownload ? 16 : 0) + (_deviceMacIdSettingDownload ? 32 : 0) + (_deviceRTCSyncDownload ? 64 : 0) + (_deviceFWEncyptionDownload ? 128 : 0)); // communication type bit number 1:Firmware 2:application 3:font 4:ladder 

                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length + 1] = 0; // communication type heigher byte
                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length + 2] = 1; // ack required
                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length + 3] = 0; //*1byte for timeout if no ack

                        ReadTimeout = false;
                        DataReceivedFlag = false;


                        _serialComPort.DiscardInBuffer();
                        _serialComPort.DiscardOutBuffer();

                        kapsSleep(10);
                        bytesToRead = 5;
                        _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);

                        //Log.Information("Serial class DOWNLOAD_INFO Frame Sent: " + BitConverter.ToString(TxBuffer));

                        //Thread.Sleep(2000);//KV added
                        if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                        {
                            retryCount++;
                            if (retryCount == 3)
                            {
                                //_serialComPort.DiscardInBuffer();
                                //_serialComPort.DiscardOutBuffer();
                                //_serialComPort.Close();
                                //_serialComPort.Dispose();

                                DiscardInOutSerialBUffers();

                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                return ErrorCode.ReadTimeoutError;
                            }
                            continue;
                        }
                        else
                        {
                            _serialComPort.DiscardInBuffer();
                            _serialComPort.DiscardOutBuffer();
                            break;
                        }
                    }

                    //     OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);
                    //Thread.Sleep(5);//KV added

                    string receivedData1 = ASCIIEncoding.ASCII.GetString(RxBuffer);

                    //Log.Information("Serial class Received Data After DOWNLOAD_INFO Frame Sent: " + receivedData1);

                    Array.Clear(RxBuffer, 0, RxBuffer.Length);
                    string compareDataFromRxBuffer1 = receivedData1.Substring(0, 5);
                    if ("READY" == compareDataFromRxBuffer1)//ToDo substring
                    {
                        for (int iCount = 0; ; iCount++)
                        {
                            string filename;
                            int downloadInProgress = 1;
                            string DWNL = "FW_DWNL";
                            if (_deviceFWDownload)
                            {
                                filename = _serialFWFilePath;

                                if (bootVersionNo >= 11)
                                {
                                    filename = _serialFWFilePath;

                                    Serilog.Log.Information("Serial class Serial class Encrypted FW File Path: " + filename);
                                }

                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 1;
                                DWNL = "FW_DWNL";
                            }
                            else if (_deviceAppDownload)
                            {
                                filename = _serialAppFilePath;
                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 2;
                                DWNL = "AP_DWNL";
                            }
                            else if (_deviceFontDownload)
                            {
                                filename = _serialFontFilePath;
                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 3;
                                DWNL = "FN_DWNL";
                            }
                            else if (_deviceLadderDownload)
                            {
                                filename = _serialLadderFilePath;
                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 4;
                                DWNL = "LD_DWNL";   //CHECK
                            }
                            else if (_deviceEthernetSettingDownload)
                            {
                                filename = _serialEthernetSettingFilePath;
                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 5;
                                DWNL = "ES_DWNL";
                            }
                            else if (_deviceMacIdSettingDownload)
                            {
                                filename = _serialMacIDFilepath;
                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 6;
                                DWNL = "MA_DWNL";
                            }
                            else if (_deviceRTCSyncDownload)
                            {
                                filename = _serialSystemRTCFilepath;
                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 7;
                                DWNL = "RT_DWNL";
                            }
                            else if (_deviceFWEncyptionDownload)
                            {
                                //Log.Information("Serial class Firmware Encryption Key File Path: " + _serialEncryptionFilePath);

                                filename = _serialEncryptionFilePath;

                                if (!File.Exists(filename))
                                {
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.FileNotExists;
                                }
                                downloadInProgress = 8;
                                DWNL = "EF_DWNL";
                            }
                            else

                                break;

                            byte[] TxBuffer = ASCIIEncoding.ASCII.GetBytes(DWNL);//ToDo remove var
                                                                                 //ToDo try to remove fs or sr, use any one
                            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                            {
                                ApplicationFileLength = fs.Length;

                                BinaryReader sr = new BinaryReader(fs);
                                sr.BaseStream.Seek(0, 0);

                                byte[] fileLength = BitConverter.GetBytes(fs.Length);
                                byte[] abc = new byte[fs.Length];
                                sr.Read(abc, 0, (int)fs.Length);

                                byte[] arrCRCTotal = CalculateCRC(abc, 0, (int)fs.Length);

                                sr.BaseStream.Seek(0, 0);
                                Array.Resize(ref TxBuffer, 20);
                                TxBuffer[DWNL.Length] = fileLength[0]; //Firware size
                                TxBuffer[DWNL.Length + 1] = fileLength[1];
                                TxBuffer[DWNL.Length + 2] = fileLength[2];
                                TxBuffer[DWNL.Length + 3] = fileLength[3];

                                retryCount = 0;

                                //ByteFW_DWNL[13] = 0; // 2byte crc
                                //ByteFW_DWNL[14] = 0;

                                //	c: 
                                retryCount = 0;
                                while (true)
                                {
                                    _serialComPort.DiscardInBuffer();
                                    _serialComPort.DiscardOutBuffer();
                                    ReadTimeout = false;
                                    DataReceivedFlag = false;

                                    bytesToRead = 13;
                                    _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);

                                   // Log.Information("Serial class " + DWNL + " Frame Sent: " + BitConverter.ToString(TxBuffer));

                                    //Thread.Sleep(1);//KV added
                                    if (WaitForresponse(DWNL) == ErrorCode.ReadTimeoutError)
                                    {
                                        retryCount++;
                                        if (retryCount >= 3)
                                        {
                                            //_serialComPort.DiscardInBuffer();
                                            //_serialComPort.DiscardOutBuffer();
                                            //_serialComPort.Close();
                                            //_serialComPort.Dispose();

                                            DiscardInOutSerialBUffers();

                                            OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                            return ErrorCode.ReadTimeoutError;
                                        }
                                        continue;
                                        //goto c;
                                    }
                                    else
                                    {
                                        _serialComPort.DiscardInBuffer();
                                        _serialComPort.DiscardOutBuffer();
                                        break;
                                    }
                                }

                                //     OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);
                                Thread.Sleep(5);//KV added

                                retryCount = 0;

                                string receivedData2 = ASCIIEncoding.ASCII.GetString(RxBuffer);

                                //Log.Information("Serial class Received Data After " + DWNL + " Frame Sent: " + receivedData2);

                                Array.Clear(RxBuffer, 0, RxBuffer.Length);

                                int RetryCnt = 0;
                                int MAX_RETRIES = 4;

                                string compareDataFromRxBuffer2 = receivedData2.Substring(0, 13);
                                if ("FW_DWNL_START" == compareDataFromRxBuffer2 || "AP_DWNL_START" == compareDataFromRxBuffer2 || "FN_DWNL_START" == compareDataFromRxBuffer2
                                    || "LD_DWNL_START" == compareDataFromRxBuffer2 || "ES_DWNL_START" == compareDataFromRxBuffer2 || "MA_DWNL_START" == compareDataFromRxBuffer2
                                    || "RT_DWNL_START" == compareDataFromRxBuffer2 || "EF_DWNL_START" == compareDataFromRxBuffer2)
                                {
                                    Array.Clear(TxBuffer, 0, TxBuffer.Length);
                                    Array.Resize(ref TxBuffer, FRAMEWITHHEADERS);//ToDo declare constants

                                    OnProgressEvent((0).ToString(), 0, CurrentDownload.None);

                                    double Privious = 0;

                                    for (int i = 0; i < fs.Length / FRAMEDATA; i++)
                                    {
                                        //Console.WriteLine("In For Loops");

                                        if (checkThreadClosing() == true)
                                        {
                                            double per = ((100 * (i + 1)) / (fs.Length / 256));

                                            if (Privious != per)
                                            {
                                                Privious = per;
                                                switch (DWNL)
                                                {
                                                    case "AP_DWNL":

                                                        OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Application);
                                                        break;

                                                    case "FW_DWNL":

                                                        OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Firmware);
                                                        break;

                                                    case "FN_DWNL":

                                                        OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Font);
                                                        break;

                                                    case "LD_DWNL":

                                                        OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Ladder);
                                                        break;

                                                    case "ES_DWNL":

                                                        OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Ethernet_Settings);
                                                        break;
                                                    case "EF_DWNL":

                                                        OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Firmware_Encryption_key);
                                                        break;
                                                }
                                            }

                                            //OnProgressEvent(((int)per).ToString(), 0,CurrentDownload.NONE);

                                            byte[] arrFrameNo = BitConverter.GetBytes(i + 1);
                                            TxBuffer[0] = arrFrameNo[0];
                                            TxBuffer[1] = arrFrameNo[1];
                                            TxBuffer[2] = arrFrameNo[2];
                                            TxBuffer[3] = arrFrameNo[3];

                                            if (RxBuffer[0] != 0xCC)
                                            {
                                                sr.Read(TxBuffer, 4, FRAMEDATA);
                                                byte[] arrCRC = CalculateCRC(TxBuffer, 4, FRAMEDATA);

                                                TxBuffer[FRAMEWITHHEADERS - 2] = arrCRC[0];//ToDo Change index
                                                TxBuffer[FRAMEWITHHEADERS - 1] = arrCRC[1];
                                            }
                                            else
                                            {
                                                byte[] arrCRC = CalculateCRC(TxBuffer, 4, FRAMEDATA);
                                                TxBuffer[FRAMEWITHHEADERS - 2] = arrCRC[0];//ToDo Change index
                                                TxBuffer[FRAMEWITHHEADERS - 1] = arrCRC[1];
                                            }
                                            try
                                            {
                                                _serialComPort.DiscardInBuffer();
                                                _serialComPort.DiscardOutBuffer();
                                                ReadTimeout = false;
                                                DataReceivedFlag = false;

                                                //if (_isBootDownload)
                                                kapsSleep(1);

                                                bytesToRead = 1;

                                                _serialComPort.Write(TxBuffer, 0, FRAMEWITHHEADERS);
                                                Console.WriteLine("FrameNumber - " + i + " " + TxBuffer.Length);
                                                if(DWNL == "EF_DWNL")
                                                Log.Information("Serial class Frame Sent for Frame Number: " + (i + 1) + " Data: " + BitConverter.ToString(TxBuffer));
                                            }
                                            catch (TimeoutException ex)
                                            {
                                              
                                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                return ErrorCode.WriteTimeoutError;
                                            }
                                            catch (Exception ex)
                                            {
                                               
                                                _serialComPort.DiscardInBuffer();
                                                _serialComPort.DiscardOutBuffer();
                                                _serialComPort.Close();
                                                _serialComPort.Dispose();

                                            }
                                            bytesToRead = 1;
                                            _serialComPort.ReadTimeout = 1000;
                                            if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                            {
                                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                return ErrorCode.ReadTimeoutError;
                                            }
                                            _serialComPort.ReadTimeout = 10000;

                                            //   OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);
                                            if (RxBuffer[0] == 0xA1)
                                            {
                                                RetryCnt = 0;
                                            }
                                            else if (RxBuffer[0] != 0xA1)
                                            {
                                                RetryCnt++;

                                                if (RxBuffer[0] == 0xCC)//CRC error
                                                {
                                                    if (RetryCnt == MAX_RETRIES)
                                                    {
                                                        _serialComPort.DiscardInBuffer();
                                                        _serialComPort.DiscardOutBuffer();
                                                        _serialComPort.Close();
                                                        _serialComPort.Dispose();
                                                        OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                        return ErrorCode.CommunicationTermineted;
                                                    }
                                                    // sr.po (TxBuffer, 4, FRAMEDATA);
                                                    i--;
                                                }
                                                else if (RxBuffer[0] == 0xEE)//Device error
                                                {
                                                    _serialComPort.DiscardInBuffer();
                                                    _serialComPort.DiscardOutBuffer();
                                                    _serialComPort.Close();
                                                    _serialComPort.Dispose();
                                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                    return ErrorCode.PageError;

                                                }
                                                else
                                                {
                                                    _serialComPort.DiscardInBuffer();
                                                    _serialComPort.DiscardOutBuffer();
                                                    _serialComPort.Close();
                                                    _serialComPort.Dispose();
                                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                    return ErrorCode.AcknowledgementError;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //_serialComPort.DiscardInBuffer();
                                            //_serialComPort.DiscardOutBuffer();
                                            //_serialComPort.Close();
                                            //_serialComPort.Dispose();

                                            DiscardInOutSerialBUffers();

                                            if (fs != null)
                                                fs.Close();
                                            OnProgressEvent("-11", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);
                                            return ErrorCode.AcknowledgementError;
                                        }
                                    }//Loop
                                }
                                else
                                {
                                    //CHECK
                                    //_serialComPort.DiscardInBuffer();
                                    //_serialComPort.DiscardOutBuffer();
                                    //_serialComPort.Close();
                                    //_serialComPort.Dispose();

                                    DiscardInOutSerialBUffers();


                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.AcknowledgementError;
                                }

                                int noOfBytes = (int)(fs.Length - sr.BaseStream.Position);

                                if (noOfBytes > 0)
                                {
                                    Array.Clear(TxBuffer, 0, TxBuffer.Length);
                                    Array.Resize(ref TxBuffer, noOfBytes + 2 + 4);

                                    if (noOfBytes < 3) // added due to firmware required.(if we pass the 1 bytes crc then firmware will send cc hence added)
                                        Array.Resize(ref TxBuffer, noOfBytes + 2 + 4 + 2);



                                    byte[] arrFrameNo = BitConverter.GetBytes(fs.Length / FRAMEDATA + 1);
                                    TxBuffer[0] = arrFrameNo[0];
                                    TxBuffer[1] = arrFrameNo[1];
                                    TxBuffer[2] = arrFrameNo[2];
                                    TxBuffer[3] = arrFrameNo[3];

                                    sr.Read(TxBuffer, 4, noOfBytes);

                                    if (noOfBytes < 3)
                                    {
                                        if (noOfBytes == 1)
                                        {
                                            TxBuffer[5] = 255; //TxBuffer[4] is data which is noofBytes
                                            TxBuffer[6] = 255;
                                        }
                                        else
                                        {
                                            TxBuffer[6] = 255;//TxBuffer[4] and TxBuffer[5] are data which is noofBytes
                                            TxBuffer[7] = 255;

                                        }


                                        noOfBytes += 2;
                                    }

                                    byte[] arrCRC = CalculateCRC(TxBuffer, 4, noOfBytes);
                                    TxBuffer[TxBuffer.Length - 2] = arrCRC[0];//ToDo Change index
                                    TxBuffer[TxBuffer.Length - 1] = arrCRC[1];

                                    try
                                    {
                                        ReadTimeout = false;
                                        DataReceivedFlag = false;
                                        _serialComPort.DiscardInBuffer();
                                        _serialComPort.DiscardOutBuffer();

                                        bytesToRead = 1;

                                        _serialComPort.ReadTimeout = 1000;

                                        _serialComPort.Write(TxBuffer, 0, noOfBytes + 2 + 4);

                                        //Log.Information("Serial class Last Frame Sent: " + BitConverter.ToString(TxBuffer));

                                        Console.WriteLine("Written All Bytes");
                                    }
                                    catch (TimeoutException ex)
                                    {
                                       
                                        //_serialComPort.DiscardInBuffer();
                                        //  _serialComPort.DiscardOutBuffer();
                                        //  _serialComPort.Close();
                                        //  _serialComPort.Dispose();

                                        DiscardInOutSerialBUffers();


                                        OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                        return ErrorCode.WriteTimeoutError;
                                    }
                                    bytesToRead = 1;
                                    if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                    {
                                        //_serialComPort.DiscardInBuffer();
                                        //_serialComPort.DiscardOutBuffer();
                                        //_serialComPort.Close();
                                        //_serialComPort.Dispose();

                                        DiscardInOutSerialBUffers();

                                        OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                        return ErrorCode.ReadTimeoutError;
                                    }
                                    _serialComPort.ReadTimeout = 10000;

                                    Console.WriteLine("Raed All Bytes");

                                    //  OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);

                                    if (RxBuffer[0] == 0xA1)
                                    {
                                        RetryCnt = 0;

                                    }
                                    else if (RxBuffer[0] == 0xCC)
                                    {
                                        //RetryCnt++;

                                        for (RetryCnt = 1; RetryCnt < 4; RetryCnt++)
                                        {
                                            ReadTimeout = false;
                                            DataReceivedFlag = false;
                                            _serialComPort.DiscardInBuffer();
                                            _serialComPort.DiscardOutBuffer();
                                            _serialComPort.Write(TxBuffer, 0, noOfBytes + 2 + 4);
                                            bytesToRead = 1;
                                            if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                            {
                                                //_serialComPort.DiscardInBuffer();
                                                //_serialComPort.DiscardOutBuffer();
                                                //_serialComPort.Close();
                                                //_serialComPort.Dispose();

                                                DiscardInOutSerialBUffers();


                                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                return ErrorCode.ReadTimeoutError;
                                            }
                                            if (RetryCnt == 3)  //"WRITE FRAME 3 TIMES IF CC RECEIVED"
                                            {
                                                //_serialComPort.DiscardInBuffer();
                                                //_serialComPort.DiscardOutBuffer();
                                                //_serialComPort.Close();
                                                //_serialComPort.Dispose();

                                                DiscardInOutSerialBUffers();


                                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                return ErrorCode.CommunicationTermineted;
                                            }
                                            if (RxBuffer[0] == 0xA1)
                                            {
                                                break;
                                            }
                                        }

                                        //return ErrorCode.CommunicationTermineted;
                                    }
                                    else if (RxBuffer[0] == 0xEE)
                                    {
                                        //_serialComPort.DiscardInBuffer();
                                        //_serialComPort.DiscardOutBuffer();
                                        //_serialComPort.Close();
                                        //_serialComPort.Dispose();

                                        DiscardInOutSerialBUffers();

                                        OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                        return ErrorCode.PageError;
                                    }
                                }

                                if (downloadInProgress == 1)
                                    strSync = "FW_FINISH";
                                else if (downloadInProgress == 2)
                                    strSync = "AP_FINISH";
                                else if (downloadInProgress == 3)
                                    strSync = "FN_FINISH";
                                else if (downloadInProgress == 4)
                                    strSync = "LD_FINISH";
                                else if (downloadInProgress == 5)
                                    strSync = "ES_FINISH";
                                else if (downloadInProgress == 6)
                                    strSync = "MA_FINISH";
                                else if (downloadInProgress == 7)
                                    strSync = "RT_FINISH";
                                else if (downloadInProgress == 8)
                                    strSync = "EF_FINISH";

                                TxBuffer = ASCIIEncoding.ASCII.GetBytes(strSync);
                                Array.Resize(ref TxBuffer, 20);

                                if (downloadInProgress == 1)
                                {
                                    TxBuffer[strSync.Length] = BitConverter.GetBytes(_serialFrmDcryptionSize)[0]; //Firware size
                                    TxBuffer[strSync.Length + 1] = BitConverter.GetBytes(_serialFrmDcryptionSize)[1]; ;
                                    TxBuffer[strSync.Length + 2] = BitConverter.GetBytes(_serialFrmDcryptionSize)[2]; ;
                                    TxBuffer[strSync.Length + 3] = BitConverter.GetBytes(_serialFrmDcryptionSize)[3]; ;

                                    //Serilog.Log.Information("Serial class Firmware decryption size - " + _serialFrmDcryptionSize);


                                    TxBuffer[strSync.Length + 4] = arrCRCTotal[0]; // 2byte crc
                                    TxBuffer[strSync.Length + 5] = arrCRCTotal[1];

                                    //Serilog.Log.Information("Serial class Firmware CRC - " + arrCRCTotal[0] + " " + arrCRCTotal[1]);

                                    if (bootVersionNo >= 11)
                                    {
                                        TxBuffer[strSync.Length + 4] = _serialFrmDcryptionCRC[0]; // 2byte crc
                                        TxBuffer[strSync.Length + 5] = _serialFrmDcryptionCRC[1];

                                        //Serilog.Log.Information("Serial class Firmware CRC - " + arrCRCTotal[0] + " " + arrCRCTotal[1]);

                                    }

                                    TxBuffer[strSync.Length + 6] = fileLength[0];
                                    TxBuffer[strSync.Length + 7] = fileLength[1];
                                    TxBuffer[strSync.Length + 8] = fileLength[2];
                                    TxBuffer[strSync.Length + 9] = fileLength[3];

                                    //Serilog.Log.Information("Serial class Firmware size - " + fileLength[0] + " " + fileLength[1] + " " + fileLength[2] + " " + fileLength[3]);

                                }
                                else
                                {
                                    TxBuffer[strSync.Length] = fileLength[0]; //Firware size
                                    TxBuffer[strSync.Length + 1] = fileLength[1];
                                    TxBuffer[strSync.Length + 2] = fileLength[2];
                                    TxBuffer[strSync.Length + 3] = fileLength[3];

                                    TxBuffer[strSync.Length + 4] = arrCRCTotal[0]; // 2byte crc
                                    TxBuffer[strSync.Length + 5] = arrCRCTotal[1];
                                }

                                try
                                {
                                    ReadTimeout = false;
                                    DataReceivedFlag = false;
                                    _serialComPort.DiscardInBuffer();
                                    _serialComPort.DiscardOutBuffer();
                                    _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);

                                    Log.Information("Serial class " + strSync + " Frame Sent: " + BitConverter.ToString(TxBuffer));
                                }
                                catch (TimeoutException ex)
                                {                                   
                                    //_serialComPort.DiscardInBuffer();
                                    //_serialComPort.DiscardOutBuffer();
                                    //_serialComPort.Close();
                                    //_serialComPort.Dispose();

                                    DiscardInOutSerialBUffers();

                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.WriteTimeoutError;
                                }
                                bytesToRead = 1;
                                if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                {
                                    DiscardInOutSerialBUffers();

                                    //if (_serialComPort.IsOpen)
                                    //{
                                    //    _serialComPort.DiscardInBuffer();
                                    //    _serialComPort.DiscardOutBuffer();
                                    //    _serialComPort.Close();
                                    //    _serialComPort.Dispose();
                                    //}
                                    // OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    //return ErrorCode.ReadTimeoutError;
                                }
                                //      OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);
                                if (RxBuffer[0] != 0xA1)
                                {
                                    //_serialComPort.DiscardInBuffer();
                                    //_serialComPort.DiscardOutBuffer();
                                    //_serialComPort.Close();
                                    //_serialComPort.Dispose();

                                    DiscardInOutSerialBUffers();

                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);


                                    return ErrorCode.AcknowledgementError;
                                }


                                sr.Close();
                                fs.Close();
                            }

                                    ;//Get file name from UI browse dialog and pass file name in commParam struct in initialize()
                                     //If filename is == null show message and return.



                            if (downloadInProgress == 1)
                                _deviceFWDownload = false;
                            else if (downloadInProgress == 2)
                                _deviceAppDownload = false;
                            else if (downloadInProgress == 3)
                                _deviceFontDownload = false;
                            else if (downloadInProgress == 4)
                                _deviceLadderDownload = false;
                            else if (downloadInProgress == 5)
                                _deviceEthernetSettingDownload = false;
                            else if (downloadInProgress == 6)
                                _deviceMacIdSettingDownload = false;
                            else if (downloadInProgress == 7)
                                _deviceRTCSyncDownload = false;
                            else if (downloadInProgress == 8)
                                _deviceFWEncyptionDownload = false;
                        }

                        Thread.Sleep(2000); // this delay is required for port close at device side for handling ex 

                        //if (_serialComPort.IsOpen == true)
                        //{
                        //    Console.WriteLine("Caught");

                        //    _serialComPort.DiscardInBuffer();
                        //    _serialComPort.DiscardOutBuffer();
                        //    _serialComPort.Close();
                        //    _serialComPort.Dispose();
                        //}

                        DiscardInOutSerialBUffers();


                        //ToDo: check if virtual com is still exist then discard and close otherwise ignore.
                        //_serialComPort.DiscardInBuffer();
                        //_serialComPort.DiscardOutBuffer();
                        //_serialComPort.Close();
                        //_serialComPort.Dispose();
                        OnProgressEvent("-12", 0, CurrentDownload.MSG_DOWNLOAD_SUCCESS);
                        return ErrorCode.Success;
                    }
                }
                else
                {
                    //if (_serialComPort.IsOpen)
                    //    _serialComPort.DiscardInBuffer();
                    //_serialComPort.DiscardOutBuffer();
                    //_serialComPort.Close();
                    //_serialComPort.Dispose();

                    DiscardInOutSerialBUffers();


                    string bn = Enum.GetName(typeof(CONSTANTS.ProjectDescription), _modelID);
                    string bn1 = Enum.GetName(typeof(CONSTANTS.ProjectDescription), modelID);

                    // System.Windows.Forms.MessageBox.Show("Device " + bn + " Does Not Matched With Current " + bn1 + " Project");

                    OnProgressEvent("-6", 1, CurrentDownload.None);//Product mismatched

                    //int modelID1 = (RxBuffer[8] + (RxBuffer[9] << 8));
                    return ErrorCode.ProductMismatched;
                    //return ErrorCode.ProductMismatched + _modelID + modelID1;
                }
            }
            catch (IOException ex)
            {
                // Calling the method for logged actions/ exceptions.
               
                DiscardInOutSerialBUffers();

                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                return ErrorCode.NoResponseFromDevice;
            }
            catch (Exception ex)
            {
                // Calling the method for logged actions/ exceptions.
                
                //if (_serialComPort.IsOpen)
                // {
                //     _serialComPort.DiscardInBuffer();
                //     _serialComPort.DiscardOutBuffer();
                //     _serialComPort.Close();
                //     _serialComPort.Dispose();
                // }

                DiscardInOutSerialBUffers();

                return ErrorCode.AcknowledgementError;
            }

            DiscardInOutSerialBUffers();

            //_serialComPort.DiscardInBuffer();
            //_serialComPort.DiscardOutBuffer();
            //_serialComPort.Close();
            //_serialComPort.Dispose();
            return ErrorCode.Success;
        }

        void DiscardInOutSerialBUffers()
        {
            try
            {
                if (_serialComPort.IsOpen)
                {
                    _serialComPort.DiscardInBuffer();
                    _serialComPort.DiscardOutBuffer();
                    _serialComPort.Close();
                    _serialComPort.Dispose();
                }
            }
            catch (Exception ex)
            {  
                MessageBox.Show(ex.StackTrace + " DiscardInOutSerialBUffers Exception");
            }
        }

        internal bool checkThreadClosing()
        {
            return _deviceShouldThreadExit;
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

        //ToDo KV to validate this function against the full file scanning at once
        public byte[] CalculateKapsCRC(byte[] _buffer, int startingIndex, int lengthOfBytes, byte lastHi, byte lastLow)
        {
            byte[] _crcValues = new byte[2];
            byte _btmCRCHi;    //higher byte of CRC initialized.
            byte _btmCRCLo;    //Lower byte of CRC initialized.
            int _imIndex;
            int _imCounter;
            byte _btmByte;
            _btmCRCHi = lastHi;
            _btmCRCLo = lastLow;
            _imCounter = startingIndex;
            while (true)
            {

                _btmByte = _buffer[_imCounter];
                _imIndex = _btmCRCLo ^ _btmByte;
                _btmCRCLo = (byte)((_btmCRCHi) ^ (_auchCRCHi[_imIndex]));
                _btmCRCHi = _auchCRCLo[_imIndex];
                _imCounter++;
                if (_imCounter > lengthOfBytes - 1)
                {
                    _crcValues[0] = _btmCRCHi;   // Higher byte is stored in the buffer
                    _crcValues[1] = _btmCRCLo;   // Lower byte is stored in the buffer
                    break;
                }
            }
            return _crcValues;
        }

        public byte GetCRC(byte[] data, int startIndex, int count)
        {
            byte tempByte = 0;

            for (int i = startIndex; i < count; i++)
            {
                if (i == 0)
                {
                    tempByte = data[i];
                }
                else
                {
                    tempByte = (byte)(tempByte ^ data[i]);
                }
            }

            return tempByte;
        }

        private ErrorCode WaitForresponse(string DWNL = "")
        {
            if (DWNL == "AP_DWNL") 
            {
//                enableReadTimoutTimer(true, (int)(ApplicationFileLength / 655 + 10000));
                enableReadTimoutTimer(true, 350000);

            }
            else
                enableReadTimoutTimer(true);
            //ReadTimeout = false;
            //DataReceivedFlag = false;

            while (!ReadTimeout && !DataReceivedFlag)
            {
                Thread.Sleep(0);
            }
            enableReadTimoutTimer(false);


            return ReadTimeout ? ErrorCode.ReadTimeoutError : ErrorCode.Success;
        }

    }
}
