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
        private string _serialSystemRTCFilepath;

        private bool _isDownload = false;
        private Mutex syncData = new Mutex();

        string _serialAlarmFilePath;
        string _serialDataLogFilePath;

        private bool _isStandAloneUtility;

        int _modelID = 0;

        volatile int bytesToRead = 0;

        private const int FRAMEWITHHEADERSFORUPLOAD = 262; //for test
        private const int FRAMEDATAFORUPLOAD = 256;


        int _result;

        private const int FRAMEWITHHEADERS = 262;
        private const int FRAMEDATA = 256;
        System.Threading.Timer readTimeOutTimer; //= new System.Threading.Timer(readTimeoutTimerCallback);

        private void readTimeoutTimerCallback(object state)
        {
            ReadTimeout = true;
        }

        void enableReadTimoutTimer(bool status, int time = 1000)
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
            _serialComPort.BaudRate = pComParam.BaudRate; //SK to adopt respective changes
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

            _deviceFWDownload = pComParam.isFirmware;
            _deviceAppDownload = pComParam.isApplication;
            _deviceFontDownload = pComParam.isFont;
            _deviceLadderDownload = pComParam.isLadder;
            _deviceMacIdSettingDownload = pComParam.isMacId;
            _deviceEthernetSettingDownload = pComParam.isEthernetSettings;
            _deviceRTCSyncDownload = pComParam.isRTCSync;

            _deviceAppUpload = pComParam.isApplication;

            _isStandAloneUtility = pComParam._isStandAloneUtility;

            _modelID = pComParam.modelID;

            _isDownload = pComParam._isDownload;

            Console.WriteLine("######################################################### Intialization done in serial class #########################################################");

            return 0;
        }

        public override int Connect()
        {
            _serialComPort.Handshake = Handshake.None;
            _serialComPort.ReadTimeout = 10000;
            _serialComPort.WriteTimeout = 3000;

            try
            {
                if (_serialComPort != null)
                {
                    if (_serialComPort.IsOpen)
                        _serialComPort.Close();

                    _serialComPort.Open();
                }


                //This if is added because Upload File logic is written by Synchronous Method
                // if (_isDownload)
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
            catch (System.IO.IOException)
            {
                MessageBox.Show("Something went wrong while connecting.\nPlease try again.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Console.WriteLine("######################################################### Connect done in serial class #########################################################");


            return 0;
        }
        internal bool checkThreadClosing()
        {
            return _deviceShouldThreadExit;
        }
        public override int SendFile()
        {
            ErrorCode res = ReadData();
            string msg = "";

            if (res == ErrorCode.Success)
            {
                msg = "File sent successfully";
                OnProgressEvent("100", 0, CurrentDownload.MSG_DOWNLOAD_SUCCESS);
                Console.WriteLine("######################################################### Send file done in serial class #########################################################");


            }
            else
            {
                msg = "Failed to send file,\nError Code: 0x" + (byte)res;
                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);
            }
            if (_serialComPort != null)
                if (_serialComPort.IsOpen)
                    _serialComPort.Close();


            return (int)res;

            //return -1;
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            byte[] tempBuffer = new byte[8];
            if (serialPort.BytesToRead < bytesToRead) return;
            int noBytesToRead = serialPort.BytesToRead;
            //Array.Resize(ref tempBuffer, bytesToRead > serialPort.BytesToRead ? bytesToRead : serialPort.BytesToRead);
            Array.Resize(ref tempBuffer, bytesToRead);
            Array.Clear(tempBuffer, 0, tempBuffer.Length);



            try
            {
                Console.WriteLine("---->>> Before Read, serialPort.BytesToRead: " + serialPort.BytesToRead + " bytesToRead: " + bytesToRead + "tempBuffer.Length: " + tempBuffer.Length);

                int bytesReceived = serialPort.Read(tempBuffer, 0, serialPort.BytesToRead);
                int bytesReceivedBackup = 0;

                Console.WriteLine("---->>> After  Read, serialPort.BytesToRead: " + serialPort.BytesToRead + " bytesToRead: " + bytesToRead + "tempBuffer.Length: " + tempBuffer.Length + "bytesReceived: " + bytesReceived);

                Console.WriteLine(bytesReceived);


                while (bytesReceived < bytesToRead)
                {
                    Console.WriteLine("---->>> BeforeWRead, serialPort.BytesToRead: " + serialPort.BytesToRead + " bytesToRead: " + bytesToRead + "tempBuffer.Length: " + tempBuffer.Length);
                    bytesReceived += serialPort.Read(tempBuffer, bytesReceived, bytesToRead - bytesReceived);
                    Console.WriteLine("---->>> AfterW Read, serialPort.BytesToRead: " + serialPort.BytesToRead + " bytesToRead: " + bytesToRead + "tempBuffer.Length: " + tempBuffer.Length + "bytesReceived: " + bytesReceived);

                    if (bytesReceived != bytesReceivedBackup)
                    {
                        Console.WriteLine("ab");
                    }
                    bytesReceivedBackup = bytesReceived;
                }

                if (!_isDownload)
                {
                    string abc = "";
                    for (int i = 0; i < 4 && tempBuffer.Length > i; i++)
                    {
                        abc += tempBuffer[i] + " ";
                    }
                    Console.WriteLine("@@@@@@@@@@@  abc=" + abc);
                }

                ReadTimeout = false;
            }
            catch (TimeoutException)
            {
                Console.WriteLine("-TimeOut ERR>> After Read, serialPort.BytesToRead: " + serialPort.BytesToRead + " bytesToRead: " + bytesToRead + "tempBuffer.Length: " + tempBuffer.Length);
                ReadTimeout = true;
            }

            syncData.WaitOne();
            Array.Resize(ref RxBuffer, bytesToRead);
            Array.Clear(RxBuffer, 0, RxBuffer.Length);
            tempBuffer.CopyTo(RxBuffer, 0);
            syncData.ReleaseMutex();

            DataReceivedFlag = true;
        }

        public ErrorCode ReadData()
        {
            int retryCount = 0;
            OnProgressEvent(0.ToString(), 1, CurrentDownload.None);
            try
            {

                string strSync = "";

                retryCount = 0;
                while (true)
                {
                    strSync = "GIC_SYNC";//Add 12 reserved bytes here
                    byte[] data = ASCIIEncoding.ASCII.GetBytes(strSync);
                    Array.Resize(ref data, 20);

                    Array.Clear(TxBuffer, 0, TxBuffer.Length);
                    Array.Resize(ref TxBuffer, data.Length);
                    TxBuffer = data;
                    TxBuffer[strSync.Length] = 35;

                    _serialComPort.DiscardInBuffer();
                    _serialComPort.DiscardOutBuffer();

                    _serialComPort.RtsEnable = false;
                    _serialComPort.DtrEnable = false;
                    Thread.Sleep(1);
                    _serialComPort.RtsEnable = true;
                    _serialComPort.DtrEnable = true;
                    Thread.Sleep(1);

                    ReadTimeout = false;
                    DataReceivedFlag = false;

                    _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
                    Console.WriteLine("######################################################### GIC SYNC Write in serial class #########################################################");


                    bytesToRead = 16;//change it to 10 as per communication with GA

                    if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                    {
                        retryCount++;

                        if (retryCount >= 3)
                        {
                            _serialComPort.DtrEnable = false;
                            _serialComPort.RtsEnable = false;
                            _serialComPort.DiscardInBuffer();
                            _serialComPort.DiscardOutBuffer();
                            _serialComPort.Close();
                            _serialComPort.Dispose();

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

                //   OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);

                //Thread.Sleep(5);//KV added

                retryCount = 0;
                string receivedData = ASCIIEncoding.ASCII.GetString(RxBuffer);
                //rtxtDataArea.AppendText(receivedData);.
                string compareDataFromRxBuffer = receivedData.Substring(0, 8);
                int modelID = (RxBuffer[8] + (RxBuffer[9] << 8));
                int firmwareVersion = (RxBuffer[10] + (RxBuffer[11] << 8));

                Array.Clear(RxBuffer, 0, RxBuffer.Length);

                if (_isStandAloneUtility == true)
                {
                    string filePath = "";
                    if (_deviceFWDownload)
                        filePath = _serialFWFilePath;
                    else if (_deviceAppDownload)
                        filePath = _serialAppFilePath;

                    if (filePath != "")
                    {
                        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        fs.Seek(fs.Length - 8, SeekOrigin.Begin);
                        byte[] productId = new byte[2];
                        fs.Read(productId, 0, productId.Length);
                        fs.Close();

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
                    Console.WriteLine("######################################################### GIC SYNC responce and model Id is matched in serial class #########################################################");


                    retryCount = 0;
                    while (true)
                    {
                        string DOWNLOAD_INFO_0_Frame = "DOWNLOAD_INFO";
                        TxBuffer = ASCIIEncoding.ASCII.GetBytes(DOWNLOAD_INFO_0_Frame);

                        Array.Resize(ref TxBuffer, 35);

                        //TxBuffer[DOWNLOAD_INFO_0_Frame.Length] = (byte)((_deviceFWDownload ? 1 : 0) + (_deviceAppDownload ? 2 : 0) + (_deviceFontDownload ? 4 : 0) + (_ethernetSettingDownload ? 16 : 0)); // communication type bit number 1:Firmware 2:application 3:font 4:ladder 
                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length] = (byte)((_deviceFWDownload ? 1 : 0) + (_deviceAppDownload ? 2 : 0) + (_deviceLadderDownload ? 4 : 0) + (_deviceFontDownload ? 8 : 0)
                            + (_deviceEthernetSettingDownload ? 16 : 0) + (_deviceMacIdSettingDownload ? 32 : 0) + (_deviceRTCSyncDownload ? 64 : 0)); // communication type bit number 1:Firmware 2:application 3:font 4:ladder 

                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length + 1] = 0; // communication type heigher byte
                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length + 2] = 1; // ack required
                        TxBuffer[DOWNLOAD_INFO_0_Frame.Length + 3] = 0; //*1byte for timeout if no ack

                        ReadTimeout = false;
                        DataReceivedFlag = false;

                        _serialComPort.DiscardInBuffer();
                        _serialComPort.DiscardOutBuffer();
                        _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
                        Console.WriteLine("######################################################### DOWNLOAD_INFO written in serial class #########################################################");


                        bytesToRead = 5;
                        //Thread.Sleep(2000);//KV added
                        if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                        {
                            retryCount++;
                            if (retryCount == 3)
                            {
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                _serialComPort.Close();
                                _serialComPort.Dispose();

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
                    Array.Clear(RxBuffer, 0, RxBuffer.Length);
                    string compareDataFromRxBuffer1 = receivedData1.Substring(0, 5);
                    if ("READY" == compareDataFromRxBuffer1)//ToDo substring
                    {
                        Console.WriteLine("######################################################### got responce for DOWNLOAD_INFO in serial class #########################################################");

                        for (int iCount = 0; ; iCount++)
                        {
                            string filename;
                            int downloadInProgress = 1;
                            string DWNL = "FW_DWNL";
                            if (_deviceFWDownload)
                            {
                                filename = _serialFWFilePath;
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
                            else
                                break;

                            byte[] TxBuffer = ASCIIEncoding.ASCII.GetBytes(DWNL);//ToDo remove var
                                                                                 //ToDo try to remove fs or sr, use any one
                            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);//Get file name from UI browse dialog and pass file name in commParam struct in initialize()
                                                                                                     //If filename is == null show message and return.

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

                                _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);

                                Console.WriteLine("######################################################### AP_DWNL/FW_DWNL write in serial class #########################################################");

                                bytesToRead = 13;
                                //Thread.Sleep(1);//KV added
                                if (WaitForresponse(DWNL) == ErrorCode.ReadTimeoutError)
                                {
                                    retryCount++;
                                    if (retryCount >= 3)
                                    {
                                        _serialComPort.DiscardInBuffer();
                                        _serialComPort.DiscardOutBuffer();
                                        _serialComPort.Close();
                                        _serialComPort.Dispose();
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
                            Array.Clear(RxBuffer, 0, RxBuffer.Length);

                            int RetryCnt = 0;
                            int MAX_RETRIES = 4;

                            string compareDataFromRxBuffer2 = receivedData2.Substring(0, 13);
                            if ("FW_DWNL_START" == compareDataFromRxBuffer2 || "AP_DWNL_START" == compareDataFromRxBuffer2 || "FN_DWNL_START" == compareDataFromRxBuffer2
                                || "LD_DWNL_START" == compareDataFromRxBuffer2 || "ES_DWNL_START" == compareDataFromRxBuffer2 || "MA_DWNL_START" == compareDataFromRxBuffer2
                                || "RT_DWNL_START" == compareDataFromRxBuffer2)
                            {
                                Console.WriteLine("######################################################### got the responce for AP_DWNL/FW_DWNL in serial class #########################################################");

                                Array.Clear(TxBuffer, 0, TxBuffer.Length);
                                Array.Resize(ref TxBuffer, FRAMEWITHHEADERS);//ToDo declare constants

                                OnProgressEvent((0).ToString(), 0, CurrentDownload.None);

                                double Privious = 0;


                                for (int i = 0; i < fs.Length / FRAMEDATA; i++)
                                {

                                    Console.WriteLine("######################################################### Frame un for loop stated, " + i + " write in serial class #########################################################");

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

                                            TxBuffer[260] = arrCRC[0];//ToDo Change index
                                            TxBuffer[261] = arrCRC[1];
                                        }
                                        try
                                        {
                                            _serialComPort.DiscardInBuffer();
                                            _serialComPort.DiscardOutBuffer();
                                            ReadTimeout = false;
                                            DataReceivedFlag = false;

                                            _serialComPort.Write(TxBuffer, 0, FRAMEWITHHEADERS);
                                        }
                                        catch (TimeoutException)
                                        {
                                            OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                            return ErrorCode.WriteTimeoutError;
                                        }
                                        bytesToRead = 1;
                                        if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                        {
                                            OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                            return ErrorCode.ReadTimeoutError;
                                        }
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
                                //CHECK
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                _serialComPort.Close();
                                _serialComPort.Dispose();
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
                                    TxBuffer[5] = 255;
                                    TxBuffer[6] = 255;

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
                                    _serialComPort.Write(TxBuffer, 0, noOfBytes + 2 + 4);

                                    Console.WriteLine("Written All Bytes");
                                }
                                catch (TimeoutException)
                                {
                                    _serialComPort.DiscardInBuffer();
                                    _serialComPort.DiscardOutBuffer();
                                    _serialComPort.Close();
                                    _serialComPort.Dispose();
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.WriteTimeoutError;
                                }
                                bytesToRead = 1;
                                if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                {
                                    _serialComPort.DiscardInBuffer();
                                    _serialComPort.DiscardOutBuffer();
                                    _serialComPort.Close();
                                    _serialComPort.Dispose();
                                    OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                    return ErrorCode.ReadTimeoutError;
                                }

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
                                            _serialComPort.DiscardInBuffer();
                                            _serialComPort.DiscardOutBuffer();
                                            _serialComPort.Close();
                                            _serialComPort.Dispose();
                                            OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                            return ErrorCode.ReadTimeoutError;
                                        }
                                        if (RetryCnt == 3)  //"WRITE FRAME 3 TIMES IF CC RECEIVED"
                                        {
                                            _serialComPort.DiscardInBuffer();
                                            _serialComPort.DiscardOutBuffer();
                                            _serialComPort.Close();
                                            _serialComPort.Dispose();
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
                                    _serialComPort.DiscardInBuffer();
                                    _serialComPort.DiscardOutBuffer();
                                    _serialComPort.Close();
                                    _serialComPort.Dispose();
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

                            TxBuffer = ASCIIEncoding.ASCII.GetBytes(strSync);
                            Array.Resize(ref TxBuffer, 20);

                            TxBuffer[strSync.Length] = fileLength[0]; //Firware size
                            TxBuffer[strSync.Length + 1] = fileLength[1];
                            TxBuffer[strSync.Length + 2] = fileLength[2];
                            TxBuffer[strSync.Length + 3] = fileLength[3];

                            TxBuffer[strSync.Length + 4] = arrCRCTotal[0]; // 2byte crc
                            TxBuffer[strSync.Length + 5] = arrCRCTotal[1];

                            try
                            {
                                ReadTimeout = false;
                                DataReceivedFlag = false;
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
                            }
                            catch (TimeoutException)
                            {
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                _serialComPort.Close();
                                _serialComPort.Dispose();
                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                return ErrorCode.WriteTimeoutError;
                            }
                            bytesToRead = 1;
                            if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                            {
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                _serialComPort.Close();
                                _serialComPort.Dispose();
                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                return ErrorCode.ReadTimeoutError;
                            }
                            //      OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);
                            if (RxBuffer[0] != 0xA1)
                            {
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                _serialComPort.Close();
                                _serialComPort.Dispose();
                                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);


                                return ErrorCode.AcknowledgementError;
                            }

                            sr.Close();
                            fs.Close();

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
                        }

                        Thread.Sleep(2000); // this delay is required for port close at device side for handling ex 

                        if (_serialComPort.IsOpen == true)
                        {
                            Console.WriteLine("Caught");

                            _serialComPort.DiscardInBuffer();
                            _serialComPort.DiscardOutBuffer();
                            _serialComPort.Close();
                            _serialComPort.Dispose();
                        }


                        //ToDo: check if virtual com is still exist then discard and close otherwise ignore.
                        //_serialComPort.DiscardInBuffer();
                        //_serialComPort.DiscardOutBuffer();
                        //_serialComPort.Close();
                        //_serialComPort.Dispose();
                        return ErrorCode.Success;
                    }
                }
                else
                {
                    if (_serialComPort.IsOpen)
                        _serialComPort.DiscardInBuffer();
                    _serialComPort.DiscardOutBuffer();
                    _serialComPort.Close();
                    _serialComPort.Dispose();

                    string bn = Enum.GetName(typeof(CONSTANTS.ProjectDescription), _modelID);
                    string bn1 = Enum.GetName(typeof(CONSTANTS.ProjectDescription), modelID);

                    System.Windows.Forms.MessageBox.Show("Device " + bn + " Does Not Matched With Current " + bn1 + " Project");

                    //int modelID1 = (RxBuffer[8] + (RxBuffer[9] << 8));
                    return ErrorCode.ProductMismatched;
                    //return ErrorCode.ProductMismatched + _modelID + modelID1;
                }
            }
            catch (IOException ex1)
            {
                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                return ErrorCode.NoResponseFromDevice;
            }
            catch (Exception ex)
            {
                if (_serialComPort.IsOpen)
                {
                    _serialComPort.DiscardInBuffer();
                    _serialComPort.DiscardOutBuffer();
                    _serialComPort = null;
                    //_serialComPort.Dispose();
                }

                return ErrorCode.AcknowledgementError;
            }


            _serialComPort.DiscardInBuffer();
            _serialComPort.DiscardOutBuffer();
            _serialComPort.Close();
            _serialComPort.Dispose();
            return ErrorCode.Success;
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
                //enableReadTimoutTimer(true, (int)(ApplicationFileLength / 655 + 30000));
                enableReadTimoutTimer(true, 300000);

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

        public override int ReceiveFile()
        {
            ErrorCode res = WriteData();

            string msg = "";

            if (res == ErrorCode.Success)
            {
                msg = "File received successfully";
                OnProgressEvent("100", 0, CurrentDownload.MSG_DOWNLOAD_SUCCESS);
                return 0;
            }
            else
            {
                msg = "Failed to receive file,\nError Code: 0x" + (byte)res;
                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);
            }

            return (int)res;

        }

        public ErrorCode WriteData()  ///GauriJ 29-07-22 upload RxBuffer ,repalce write withh read
		{
            int RETRYCOUNT = 0;
            int MAXRETRY = 4;
            // Sychronization_Frame
            int recivedBytes = 0, retryCount = 0;

            Console.WriteLine("#########################################################  Upload WriteData start in Serial class #########################################################");


            string check = "GIC_SYNC";                      // "GIC_SYNC"
            int TotalApplicationSize = 0;
            byte[] TotalApplicationCRC = new byte[2];
            try
            {
                string strSync = "";

                retryCount = 0;
                while (true)
                {
                    strSync = "GIC_SYNC";//Add 12 reserved bytes here
                    byte[] data = ASCIIEncoding.ASCII.GetBytes(strSync);
                    Array.Resize(ref data, 20);

                    Array.Clear(TxBuffer, 0, TxBuffer.Length);
                    Array.Resize(ref TxBuffer, data.Length);
                    TxBuffer = data;
                    TxBuffer[strSync.Length] = 5;

                    _serialComPort.DiscardInBuffer();
                    _serialComPort.DiscardOutBuffer();

                    _serialComPort.RtsEnable = false;
                    _serialComPort.DtrEnable = false;
                    Thread.Sleep(1);
                    _serialComPort.RtsEnable = true;
                    _serialComPort.DtrEnable = true;
                    Thread.Sleep(1);

                    ReadTimeout = false;
                    DataReceivedFlag = false;
                    Console.WriteLine("######################################################### before GIC_SYNC write in WriteData in Serial class #########################################################");
                    int i = 0;
                    for (; i < 3; i++)
                    {
                        try
                        {
                            _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
                            break;
                        }
                        catch (Exception e)
                        {
                            _serialComPort.Close();
                            Thread.Sleep(1);
                            _serialComPort.Open();
                            Console.WriteLine("####################EXCEPTIONNNNNNNNNNNNNN #####################################" + e.Message);
                        }
                    }
                    if (i >= 3)
                    {
                        Console.WriteLine("############ STILL EXCEPTION EVEN AFTER RETRY ############################################# GIC_SYNC in upload write in serial class #########################################################");
                        return ErrorCode.CommunicationTermineted;
                    }
                    Console.WriteLine("######################################################### GIC_SYNC in upload write in serial class #########################################################");


                    bytesToRead = 16;//change it to 10 as per communication with GA

                    //Thread.Sleep(100);//KV added


                    //if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
                    if (WaitForresponse() == ErrorCode.ReadTimeoutError)

                    {
                        retryCount++;

                        if (retryCount >= 3)
                        {
                            _serialComPort.DiscardInBuffer();
                            _serialComPort.DiscardOutBuffer();
                            _serialComPort.Close();
                            _serialComPort.Dispose();

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

                retryCount = 0;
                string receivedData = ASCIIEncoding.ASCII.GetString(RxBuffer);
                string compareDataFromRxBuffer = receivedData.Substring(0, 8);
                if (strSync == compareDataFromRxBuffer)
                {
                    Console.WriteLine("######################################################### Got the responce for GIC_SYNC in upload in serial class #########################################################");

                    while (true)
                    {
                        // UPLOAD_INFO
                        check = "UPLD_INFO";

                        Array.Clear(TxBuffer, 0, TxBuffer.Length);
                        //	Array.Resize(ref TxBuffer, 35);
                        //Upload Info 11 length
                        TxBuffer = ASCIIEncoding.ASCII.GetBytes(check);

                        Array.Resize(ref TxBuffer, 13); //UPLD_INFO 9byte + 2+ 1 + 1 = 13
                                                        //TxBuffer[check.Length] = (byte)((_deviceAppUpload ? 1 : 0) + (_deviceLadderUpload ? 2 : 0) + (_deviceAlaramUpload ? 4 : 0) + (_deviceDataLoggerUpload ? 8 : 0)); // communication type bit number  1:application 2:Ladder 3:Alaram 4:DataLogger 
                        TxBuffer[check.Length] = (byte)((_deviceAppDownload ? 1 : 0) + (_deviceLadderUpload ? 2 : 0) + (_deviceAlaramUpload ? 4 : 0) + (_deviceDataLoggerUpload ? 8 : 0)); // communication type bit number  1:application 2:Ladder 3:Alaram 4:DataLogger 
                        TxBuffer[check.Length + 1] = 0; // communication type heigher byte
                        TxBuffer[check.Length + 2] = 1; // ack required
                        TxBuffer[check.Length + 3] = 0; //*1byte for timeout if no ack

                        //HERE


                        ReadTimeout = false;
                        DataReceivedFlag = false;
                        _serialComPort.DiscardInBuffer();
                        _serialComPort.DiscardOutBuffer();

                        _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
                        //Thread.Sleep(100);
                        Console.WriteLine("######################################################### UPLD_INFO write in upload in serial class #########################################################");


                        bytesToRead = 12;
                        //Thread.Sleep(2000);//KV added
                        //if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
                        if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                        {
                            retryCount++;
                            if (retryCount == 3)
                            {
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                _serialComPort.Close();
                                _serialComPort.Dispose();
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

                    string receivedData1 = ASCIIEncoding.ASCII.GetString(RxBuffer);
                    Array.Clear(RxBuffer, 0, RxBuffer.Length);
                    string compareDataFromRxBuffer1 = receivedData1.Substring(0, 5);
                    if ("READY" == compareDataFromRxBuffer1)//ToDo substring
                    {
                        Console.WriteLine("######################################################### Got the responce for UPLD_INFO in upload in serial class #########################################################");

                        for (int iCount = 0; ; iCount++)
                        {
                            string filename;
                            int uploadInProgress = 1;
                            string UPLD = "AP_UPLD_00000";
                            if (_deviceAppUpload)
                            {
                                filename = _serialAppFilePath;

                                uploadInProgress = 1;
                                UPLD = "AP_UPLD_00000";
                            }

                            else if (_deviceLadderUpload)
                            {
                                filename = _serialLadderFilePath;

                                uploadInProgress = 2;
                                UPLD = "LD_UPLD_00000";
                            }
                            else if (_deviceAlaramUpload)
                            {
                                filename = _serialAlarmFilePath;

                                uploadInProgress = 3;
                                UPLD = "AL_UPLD_00000";
                            }
                            else if (_deviceDataLoggerUpload)
                            {
                                filename = _serialDataLogFilePath;

                                uploadInProgress = 4;
                                UPLD = "DL_UPLD_00000";
                            }
                            else
                                break;

                            TxBuffer = ASCIIEncoding.ASCII.GetBytes(UPLD);

                            FileStream fs = new FileStream(filename, FileMode.Create);


                            while (true)
                            {
                                _serialComPort.DiscardInBuffer();
                                _serialComPort.DiscardOutBuffer();
                                ReadTimeout = false;
                                DataReceivedFlag = false;
                                Array.Resize(ref TxBuffer, 19); // 13 + 4 size + 2 crc
                                _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
                                Console.WriteLine("######################################################### AP_UPLD_00000 write in upload in serial class #########################################################");


                                Thread.Sleep(1);
                                bytesToRead = 19;

                                // if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
                                if (WaitForresponse() == ErrorCode.ReadTimeoutError)

                                {
                                    retryCount++;
                                    if (retryCount == 3)
                                    {
                                        _serialComPort.DiscardInBuffer();
                                        _serialComPort.DiscardOutBuffer();
                                        _serialComPort.Close();
                                        _serialComPort.Dispose();

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
                            Thread.Sleep(5);

                            retryCount = 0;
                            string receivedData2 = ASCIIEncoding.ASCII.GetString(RxBuffer);

                            int RetryCnt = 0;
                            int MAX_RETRIES = 4;
                            //      recivedBytes = _serialComPort.Read(RxBuffer, 0, RxBuffer.Length); // (13() + 6) [AP_UPLD_START + (4 Bytes = Size 2 Byte = CRC)]

                            TotalApplicationSize = BitConverter.ToInt32(RxBuffer, 13);                                    //Y1
                                                                                                                          //  TotalApplicationCRC = BitConverter.GetBytes(Convert.ToUInt16(Encoding.UTF8.GetString(RxBuffer, 17, 2)));
                            TotalApplicationCRC[0] = RxBuffer[17];
                            TotalApplicationCRC[1] = RxBuffer[18];

                            Array.Clear(RxBuffer, 0, RxBuffer.Length);
                            Array.Resize(ref RxBuffer, 13);
                            string compareDataFromRxBuffer2 = receivedData2.Substring(0, 13);

                            if ("AP_UPLD_START" == compareDataFromRxBuffer2 || "LD_UPLD_START" == compareDataFromRxBuffer2 ||
                                 "AL_UPLD_START" == compareDataFromRxBuffer2 || "DL_UPLD_START" == compareDataFromRxBuffer2)
                            {
                                Console.WriteLine("######################################################### Got the responce for AP_UPLD_00000 in upload in serial class #########################################################");

                                /// check from here , application upload befor loop
                                Array.Clear(TxBuffer, 0, TxBuffer.Length);
                                TxBuffer[0] = 0xA1;

                                bytesToRead = FRAMEWITHHEADERSFORUPLOAD;

                                _serialComPort.Write(TxBuffer, 0, 1);

                                double Privious = 0;
                                for (int i = 0; i < TotalApplicationSize / FRAMEDATAFORUPLOAD; i++)
                                {
                                    _serialComPort.ReadTimeout = 1000;

                                    Console.WriteLine("######################################################### Frame in for loop stated in upload funtion serial class, " + i + " write in serial class #########################################################");

                                    double per = ((100 * (i + 1)) / (TotalApplicationSize / FRAMEDATAFORUPLOAD));

                                    if (Privious != per)
                                    {
                                        Privious = per;
                                        switch (UPLD)
                                        {
                                            case "AP_UPLD_00000":

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
                                        }

                                    }

                                    while (true)
                                    {
                                        ReadTimeout = false;
                                        DataReceivedFlag = false;

                                        Thread.Sleep(1);
                                        bytesToRead = FRAMEWITHHEADERSFORUPLOAD;

                                        // if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)

                                        if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                        {
                                            Console.WriteLine("----Retry upload frame");
                                            retryCount++;
                                            if (retryCount == 3)
                                            {
                                                _serialComPort.DiscardInBuffer();
                                                _serialComPort.DiscardOutBuffer();
                                                _serialComPort.Close();
                                                _serialComPort.Dispose();

                                                //OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);
                                                Console.WriteLine("----upload receive frame error .... returning");
                                                return ErrorCode.ReadTimeoutError;
                                            }

                                            TxBuffer[0] = 0xCC;
                                            _serialComPort.Write(TxBuffer, 0, 1); // for CRC mismatch

                                            continue;
                                            //goto c;
                                        }
                                        else
                                        {
                                            retryCount = 0;
                                            break;
                                        }
                                    }

                                    syncData.WaitOne();

                                    byte[] arrCRC = CalculateCRC(RxBuffer, 4, FRAMEWITHHEADERSFORUPLOAD - 6);

                                    if (arrCRC[0] == RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 2] && arrCRC[1] == RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 1])//ToDo : GJ verify
                                    {
                                        fs.Write(RxBuffer, 4, FRAMEDATAFORUPLOAD);

                                        TxBuffer[0] = 0xA1;
                                        Console.WriteLine("#################################### before ACK##################### Frame in for loop stated in upload funtion serial class, " + i + " write in serial class #########################################################");

                                        _serialComPort.DiscardInBuffer();
                                        _serialComPort.DiscardOutBuffer();

                                        _serialComPort.Write(TxBuffer, 0, 1); // for Ack
                                        Console.WriteLine("#################################### After ACK##################### Frame in for loop stated in upload funtion serial class, " + i + " write in serial class #########################################################");

                                    }
                                    else if (arrCRC[0] != RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 2] && arrCRC[1] != RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 1])
                                    {
                                        Console.WriteLine("Alela CRC:" + RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 2].ToString() + " * " + RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 1].ToString());
                                        Console.WriteLine("Apla CRC:" + arrCRC[0].ToString() + " * " + arrCRC[1].ToString());
                                        Console.WriteLine("################################# CRC MISMATCH ######################## Frame in for loop stated in upload funtion serial class, " + i + " write in serial class #########################################################");

                                        //CRC Mismatch so retry

                                        RetryCnt++;

                                        TxBuffer[0] = 0XCC;
                                        _serialComPort.Write(TxBuffer, 0, 1); // CRC mismatch

                                        if (RetryCnt == MAX_RETRIES)
                                        {
                                            TxBuffer[0] = 0XEE;
                                            _serialComPort.Write(TxBuffer, 0, 1);

                                            _serialComPort.DiscardInBuffer();
                                            _serialComPort.DiscardOutBuffer();
                                            _serialComPort.Close();
                                            _serialComPort.Dispose();
                                            syncData.ReleaseMutex();
                                            return ErrorCode.CommunicationTermineted;
                                        }
                                        i--;
                                    }
                                    syncData.ReleaseMutex();
                                }

                                int noOfBytes = (TotalApplicationSize % FRAMEDATAFORUPLOAD);
                                byte[] test;

                                if (noOfBytes > 0)
                                {
                                    int LastFrameSize = noOfBytes + 6;
                                    while (true)
                                    {
                                        _serialComPort.DiscardInBuffer();
                                        _serialComPort.DiscardOutBuffer();
                                        ReadTimeout = false;
                                        DataReceivedFlag = false;

                                        //Thread.Sleep(1);
                                        bytesToRead = LastFrameSize;
                                        test = new byte[LastFrameSize];
                                        //if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
                                        if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                        {
                                            retryCount++;
                                            if (retryCount == 3)
                                            {
                                                _serialComPort.DiscardInBuffer();
                                                _serialComPort.DiscardOutBuffer();
                                                _serialComPort.Close();
                                                _serialComPort.Dispose();

                                                //		OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                                                return ErrorCode.ReadTimeoutError;
                                            }

                                            TxBuffer[0] = 0xCC;
                                            _serialComPort.Write(TxBuffer, 0, 1); // for CRC mismatch

                                            continue;
                                            //goto c;

                                        }
                                        else
                                        {

                                            retryCount = 0;
                                            Array.Copy(RxBuffer, test, RxBuffer.Length);
                                            _serialComPort.DiscardInBuffer();
                                            _serialComPort.DiscardOutBuffer();
                                            break;
                                        }
                                    }
                                    //_serialComPort.Read(RxBuffer, 0, noOfBytes);

                                    byte[] arrCRC = CalculateCRC(test, 4, LastFrameSize - 6);

                                    if (arrCRC[0] == test[LastFrameSize - 2] && arrCRC[1] == test[LastFrameSize - 1])
                                    {
                                        fs.Write(test, 4, LastFrameSize - 6);
                                        TxBuffer[0] = 0xA1;
                                        _serialComPort.Write(TxBuffer, 0, 1); // for Ack
                                        fs.Close();
                                    }
                                }

                                Thread.Sleep(1);
                                bytesToRead = 9;

                                while (true)
                                {
                                    _serialComPort.DiscardInBuffer();
                                    _serialComPort.DiscardOutBuffer();
                                    ReadTimeout = false;
                                    DataReceivedFlag = false;

                                    Thread.Sleep(0);
                                    bytesToRead = 9;

                                    //if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
                                    if (WaitForresponse() == ErrorCode.ReadTimeoutError)
                                    {
                                        retryCount++;
                                        if (retryCount == 3)
                                        {
                                            _serialComPort.DiscardInBuffer();
                                            _serialComPort.DiscardOutBuffer();
                                            _serialComPort.Close();
                                            _serialComPort.Dispose();

                                            //		OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

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

                                receivedData = ASCIIEncoding.ASCII.GetString(RxBuffer);




                            }
                            Thread.Sleep(5);


                            //    OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);
                            Console.WriteLine("COMPLETED");
                            fs.Close();

                            if (uploadInProgress == 1)
                                _deviceAppUpload = false;
                            else if (uploadInProgress == 3)
                                _deviceLadderUpload = false;
                            else if (uploadInProgress == 4)
                                _deviceAlaramUpload = false;
                            else if (uploadInProgress == 5)
                                _deviceDataLoggerUpload = false;
                        }

                        _serialComPort.DiscardInBuffer();
                        _serialComPort.DiscardOutBuffer();
                        _serialComPort.Close();
                        _serialComPort.Dispose();

                        return ErrorCode.Success;
                    }
                }
                else
                {
                    if (_serialComPort.IsOpen)
                        _serialComPort.DiscardInBuffer();

                    _serialComPort.DiscardOutBuffer();
                    _serialComPort.Close();
                    _serialComPort.Dispose();

                    //Return Unsuccess
                }
            }

            catch (Exception ex)
            {
                if (_serialComPort.IsOpen)
                    _serialComPort.DiscardInBuffer();
                _serialComPort.DiscardOutBuffer();
                _serialComPort.Close();
                _serialComPort.Dispose();

                return ErrorCode.AcknowledgementError;
            }

            _serialComPort.DiscardInBuffer();
            _serialComPort.DiscardOutBuffer();
            _serialComPort.Close();
            _serialComPort.Dispose();
            return ErrorCode.Success;
        }

        //      public ErrorCode WriteData()  ///GauriJ 29-07-22 upload RxBuffer ,repalce write withh read
        //{
        //          int RETRYCOUNT = 0;
        //          int MAXRETRY = 4;
        //          // Sychronization_Frame
        //          int recivedBytes = 0, retryCount = 0;

        //          Console.WriteLine("#########################################################  Upload WriteData start in Serial class #########################################################");


        //          string check = "GIC_SYNC";                      // "GIC_SYNC"
        //          int TotalApplicationSize = 0;
        //          byte[] TotalApplicationCRC = new byte[2];
        //          try
        //          {
        //              string strSync = "";

        //              retryCount = 0;
        //              while (true)
        //              {
        //                  strSync = "GIC_SYNC";//Add 12 reserved bytes here
        //                  byte[] data = ASCIIEncoding.ASCII.GetBytes(strSync);
        //                  Array.Resize(ref data, 20);

        //                  Array.Clear(TxBuffer, 0, TxBuffer.Length);
        //                  Array.Resize(ref TxBuffer, data.Length);
        //                  TxBuffer = data;
        //                  TxBuffer[strSync.Length] = 5;

        //                  _serialComPort.DiscardInBuffer();
        //                  _serialComPort.DiscardOutBuffer();

        //                  _serialComPort.RtsEnable = false;
        //                  _serialComPort.DtrEnable = false;
        //                  Thread.Sleep(1);
        //                  _serialComPort.RtsEnable = true;
        //                  _serialComPort.DtrEnable = true;
        //                  Thread.Sleep(1);

        //                  ReadTimeout = false;
        //                  DataReceivedFlag = false;
        //                  Console.WriteLine("######################################################### before GIC_SYNC write in WriteData in Serial class #########################################################");
        //                  int i = 0;
        //                  for (; i < 3; i++)
        //                  {
        //                      try
        //                      {
        //                          _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
        //                          break;
        //                      }
        //                      catch (Exception e)
        //                      {
        //                          _serialComPort.Close();
        //                          Thread.Sleep(1);
        //                          _serialComPort.Open();
        //                          Console.WriteLine("####################EXCEPTIONNNNNNNNNNNNNN #####################################" + e.Message);
        //                      }
        //                  }
        //                  if(i >= 3)
        //                  {
        //                      Console.WriteLine("############ STILL EXCEPTION EVEN AFTER RETRY ############################################# GIC_SYNC in upload write in serial class #########################################################");
        //                      return ErrorCode.CommunicationTermineted;
        //                  }
        //                  Console.WriteLine("######################################################### GIC_SYNC in upload write in serial class #########################################################");


        //                  bytesToRead = 16;//change it to 10 as per communication with GA

        //                  //Thread.Sleep(100);//KV added


        //                  if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
        //                  //if (WaitForresponse() == ErrorCode.ReadTimeoutError)

        //                  {
        //                      retryCount++;

        //                      if (retryCount >= 3)
        //                      {
        //                          _serialComPort.DiscardInBuffer();
        //                          _serialComPort.DiscardOutBuffer();
        //                          _serialComPort.Close();
        //                          _serialComPort.Dispose();

        //                          return ErrorCode.NoResponseFromDevice;
        //                      }
        //                  }
        //                  else
        //                  {
        //                      _serialComPort.DiscardInBuffer();
        //                      _serialComPort.DiscardOutBuffer();
        //                      break;
        //                  }
        //              }

        //              retryCount = 0;
        //              string receivedData = ASCIIEncoding.ASCII.GetString(RxBuffer);
        //              string compareDataFromRxBuffer = receivedData.Substring(0, 8);
        //              if (strSync == compareDataFromRxBuffer)
        //              {
        //                  Console.WriteLine("######################################################### Got the responce for GIC_SYNC in upload in serial class #########################################################");

        //                  while (true)
        //                  {
        //                      // UPLOAD_INFO
        //                      check = "UPLD_INFO";

        //                      Array.Clear(TxBuffer, 0, TxBuffer.Length);
        //                      //	Array.Resize(ref TxBuffer, 35);
        //                      //Upload Info 11 length
        //                      TxBuffer = ASCIIEncoding.ASCII.GetBytes(check);

        //                      Array.Resize(ref TxBuffer, 13); //UPLD_INFO 9byte + 2+ 1 + 1 = 13
        //                                                      //TxBuffer[check.Length] = (byte)((_deviceAppUpload ? 1 : 0) + (_deviceLadderUpload ? 2 : 0) + (_deviceAlaramUpload ? 4 : 0) + (_deviceDataLoggerUpload ? 8 : 0)); // communication type bit number  1:application 2:Ladder 3:Alaram 4:DataLogger 
        //                      TxBuffer[check.Length] = (byte)((_deviceAppDownload ? 1 : 0) + (_deviceLadderUpload ? 2 : 0) + (_deviceAlaramUpload ? 4 : 0) + (_deviceDataLoggerUpload ? 8 : 0)); // communication type bit number  1:application 2:Ladder 3:Alaram 4:DataLogger 
        //                      TxBuffer[check.Length + 1] = 0; // communication type heigher byte
        //                      TxBuffer[check.Length + 2] = 1; // ack required
        //                      TxBuffer[check.Length + 3] = 0; //*1byte for timeout if no ack

        //                      //HERE


        //                      ReadTimeout = false;
        //                      DataReceivedFlag = false;
        //                      _serialComPort.DiscardInBuffer();
        //                      _serialComPort.DiscardOutBuffer();

        //                      _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
        //                      //Thread.Sleep(100);
        //                      Console.WriteLine("######################################################### UPLD_INFO write in upload in serial class #########################################################");


        //                      bytesToRead = 12;
        //                      //Thread.Sleep(2000);//KV added
        //                      if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
        //                      //if (WaitForresponse() == ErrorCode.ReadTimeoutError)
        //                      {
        //                          retryCount++;
        //                          if (retryCount == 3)
        //                          {
        //                              _serialComPort.DiscardInBuffer();
        //                              _serialComPort.DiscardOutBuffer();
        //                              _serialComPort.Close();
        //                              _serialComPort.Dispose();
        //                              return ErrorCode.ReadTimeoutError;
        //                          }
        //                          continue;
        //                      }
        //                      else
        //                      {
        //                          _serialComPort.DiscardInBuffer();
        //                          _serialComPort.DiscardOutBuffer();
        //                          break;
        //                      }
        //                  }

        //                  string receivedData1 = ASCIIEncoding.ASCII.GetString(RxBuffer);
        //                  Array.Clear(RxBuffer, 0, RxBuffer.Length);
        //                  string compareDataFromRxBuffer1 = receivedData1.Substring(0, 5);
        //                  if ("READY" == compareDataFromRxBuffer1)//ToDo substring
        //                  {
        //                      Console.WriteLine("######################################################### Got the responce for UPLD_INFO in upload in serial class #########################################################");

        //                      for (int iCount = 0; ; iCount++)
        //                      {
        //                          string filename;
        //                          int uploadInProgress = 1;
        //                          string UPLD = "AP_UPLD_00000";
        //                          if (_deviceAppUpload)
        //                          {
        //                              filename = _serialAppFilePath;

        //                              uploadInProgress = 1;
        //                              UPLD = "AP_UPLD_00000";
        //                          }

        //                          else if (_deviceLadderUpload)
        //                          {
        //                              filename = _serialLadderFilePath;

        //                              uploadInProgress = 2;
        //                              UPLD = "LD_UPLD_00000";
        //                          }
        //                          else if (_deviceAlaramUpload)
        //                          {
        //                              filename = _serialAlarmFilePath;

        //                              uploadInProgress = 3;
        //                              UPLD = "AL_UPLD_00000";
        //                          }
        //                          else if (_deviceDataLoggerUpload)
        //                          {
        //                              filename = _serialDataLogFilePath;

        //                              uploadInProgress = 4;
        //                              UPLD = "DL_UPLD_00000";
        //                          }
        //                          else
        //                              break;

        //                          TxBuffer = ASCIIEncoding.ASCII.GetBytes(UPLD);

        //                          FileStream fs = new FileStream(filename, FileMode.Create);


        //                          while (true)
        //                          {
        //                              _serialComPort.DiscardInBuffer();
        //                              _serialComPort.DiscardOutBuffer();
        //                              ReadTimeout = false;
        //                              DataReceivedFlag = false;
        //                              Array.Resize(ref TxBuffer, 19); // 13 + 4 size + 2 crc
        //                              _serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
        //                              Console.WriteLine("######################################################### AP_UPLD_00000 write in upload in serial class #########################################################");


        //                              Thread.Sleep(1);
        //                              bytesToRead = 19;

        //                              if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
        //                              //if (WaitForresponse() == ErrorCode.ReadTimeoutError)

        //                              {
        //                                  retryCount++;
        //                                  if (retryCount == 3)
        //                                  {
        //                                      _serialComPort.DiscardInBuffer();
        //                                      _serialComPort.DiscardOutBuffer();
        //                                      _serialComPort.Close();
        //                                      _serialComPort.Dispose();

        //                                      return ErrorCode.ReadTimeoutError;
        //                                  }
        //                                  continue;
        //                                  //goto c;
        //                              }
        //                              else
        //                              {
        //                                  _serialComPort.DiscardInBuffer();
        //                                  _serialComPort.DiscardOutBuffer();
        //                                  break;
        //                              }
        //                          }
        //                          Thread.Sleep(5);

        //                          retryCount = 0;
        //                          string receivedData2 = ASCIIEncoding.ASCII.GetString(RxBuffer);

        //                          int RetryCnt = 0;
        //                          int MAX_RETRIES = 4;
        //                          //      recivedBytes = _serialComPort.Read(RxBuffer, 0, RxBuffer.Length); // (13() + 6) [AP_UPLD_START + (4 Bytes = Size 2 Byte = CRC)]

        //                          TotalApplicationSize = BitConverter.ToInt32(RxBuffer, 13);                                    //Y1
        //                                                                                                                        //  TotalApplicationCRC = BitConverter.GetBytes(Convert.ToUInt16(Encoding.UTF8.GetString(RxBuffer, 17, 2)));


        //                          TotalApplicationCRC[0] = RxBuffer[17];
        //                          TotalApplicationCRC[1] = RxBuffer[18];

        //                          Array.Clear(RxBuffer, 0, RxBuffer.Length);
        //                          Array.Resize(ref RxBuffer, 13);
        //                          string compareDataFromRxBuffer2 = receivedData2.Substring(0, 13);

        //                          if ("AP_UPLD_START" == compareDataFromRxBuffer2 || "LD_UPLD_START" == compareDataFromRxBuffer2 ||
        //                               "AL_UPLD_START" == compareDataFromRxBuffer2 || "DL_UPLD_START" == compareDataFromRxBuffer2)
        //                          {

        //                              Console.WriteLine("######################################################### Got the responce for AP_UPLD_00000 in upload in serial class #########################################################");


        //                              /// check from here , application upload befor loop
        //                              Array.Clear(TxBuffer, 0, TxBuffer.Length);
        //                              TxBuffer[0] = 0xA1;

        //                              _serialComPort.Write(TxBuffer, 0, 1);

        //                              double Privious = 0;
        //                              for (int i = 0; i < TotalApplicationSize / FRAMEDATAFORUPLOAD; i++)
        //                              {
        //                                  Console.WriteLine("######################################################### Frame in for loop stated in upload funtion serial class, " + i + " write in serial class #########################################################");

        //                                  double per = ((100 * (i + 1)) / (TotalApplicationSize / FRAMEDATAFORUPLOAD));

        //                                  if (Privious != per)
        //                                  {
        //                                      Privious = per;
        //                                      switch (UPLD)
        //                                      {
        //                                          case "AP_UPLD_00000":

        //                                              OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Application);
        //                                              break;

        //                                          case "FW_DWNL":

        //                                              OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Firmware);
        //                                              break;

        //                                          case "FN_DWNL":

        //                                              OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Font);
        //                                              break;

        //                                          case "LD_DWNL":

        //                                              OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Ladder);
        //                                              break;

        //                                          case "ES_DWNL":

        //                                              OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Ethernet_Settings);
        //                                              break;
        //                                      }

        //                                  }

        //                                  while (true)
        //                                  {
        //                                      _serialComPort.DiscardInBuffer();
        //                                      _serialComPort.DiscardOutBuffer();
        //                                      ReadTimeout = false;
        //                                      DataReceivedFlag = false;

        //                                      Thread.Sleep(3);
        //                                      bytesToRead = FRAMEWITHHEADERSFORUPLOAD;

        //                                      if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
        //                                      //if (WaitForresponse() == ErrorCode.ReadTimeoutError)
        //                                      {
        //                                          Console.WriteLine("----Retry upload frame");
        //                                          retryCount++;
        //                                          if (retryCount == 3)
        //                                          {
        //                                              _serialComPort.DiscardInBuffer();
        //                                              _serialComPort.DiscardOutBuffer();
        //                                              _serialComPort.Close();
        //                                              _serialComPort.Dispose();

        //                                              //		OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);
        //                                              Console.WriteLine("----upload receive frame error .... returning");
        //                                              return ErrorCode.ReadTimeoutError;
        //                                          }
        //                                          continue;
        //                                          //goto c;
        //                                      }
        //                                      else
        //                                      {
        //                                          _serialComPort.DiscardInBuffer();
        //                                          _serialComPort.DiscardOutBuffer();
        //                                          break;
        //                                      }
        //                                  }

        //                                  byte[] arrCRC = CalculateCRC(RxBuffer, 0, FRAMEWITHHEADERSFORUPLOAD - 2);

        //                                  if (arrCRC[0] == RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 2] && arrCRC[1] == RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 1])//ToDo : GJ verify
        //                                  {
        //                                      fs.Write(RxBuffer, 4, FRAMEDATAFORUPLOAD);

        //                                      _serialComPort.DiscardInBuffer();
        //                                      _serialComPort.DiscardOutBuffer();

        //                                      TxBuffer[0] = 0xA1;
        //                                      _serialComPort.Write(TxBuffer, 0, 1); // for Ack
        //                                  }
        //                                  else if (arrCRC[0] != RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 2] && arrCRC[1] != RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 1])
        //                                  {
        //                                      Console.WriteLine("Alela CRC:" + RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 2].ToString() + " * " + RxBuffer[FRAMEWITHHEADERSFORUPLOAD - 1].ToString());
        //                                      Console.WriteLine("Apla CRC:" + arrCRC[0].ToString() + " * " + arrCRC[1].ToString());

        //                                      //CRC Mismatch so retry

        //                                      RetryCnt++;

        //                                      TxBuffer[0] = 0XCC;
        //                                      _serialComPort.Write(TxBuffer, 0, 1); // CRC mismatch

        //                                      if (RetryCnt == MAX_RETRIES)
        //                                      {
        //                                          TxBuffer[0] = 0XEE;
        //                                          _serialComPort.Write(TxBuffer, 0, 1);

        //                                          _serialComPort.DiscardInBuffer();
        //                                          _serialComPort.DiscardOutBuffer();
        //                                          _serialComPort.Close();
        //                                          _serialComPort.Dispose();
        //                                          return ErrorCode.CommunicationTermineted;
        //                                      }
        //                                      i--;
        //                                  }
        //                              }

        //                              int noOfBytes = (TotalApplicationSize % FRAMEDATAFORUPLOAD);
        //                              byte[] test;

        //                              if (noOfBytes > 0)
        //                              {
        //                                  int LastFrameSize = noOfBytes + 6;
        //                                  while (true)
        //                                  {
        //                                      _serialComPort.DiscardInBuffer();
        //                                      _serialComPort.DiscardOutBuffer();
        //                                      ReadTimeout = false;
        //                                      DataReceivedFlag = false;

        //                                      //Thread.Sleep(1);
        //                                      bytesToRead = LastFrameSize;
        //                                      test = new byte[LastFrameSize];
        //                                      if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
        //                                      //if (WaitForresponse() == ErrorCode.ReadTimeoutError)
        //                                      {
        //                                          retryCount++;
        //                                          if (retryCount == 3)
        //                                          {
        //                                              _serialComPort.DiscardInBuffer();
        //                                              _serialComPort.DiscardOutBuffer();
        //                                              _serialComPort.Close();
        //                                              _serialComPort.Dispose();

        //                                              //		OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

        //                                              return ErrorCode.ReadTimeoutError;
        //                                          }
        //                                          continue;
        //                                          //goto c;
        //                                      }
        //                                      else
        //                                      {

        //                                          Array.Copy(RxBuffer, test, RxBuffer.Length);
        //                                          _serialComPort.DiscardInBuffer();
        //                                          _serialComPort.DiscardOutBuffer();
        //                                          break;
        //                                      }
        //                                  }
        //                                  //_serialComPort.Read(RxBuffer, 0, noOfBytes);

        //                                  byte[] arrCRC = CalculateCRC(test, 0, LastFrameSize - 2);

        //                                  if (arrCRC[0] == test[LastFrameSize - 2] && arrCRC[1] == test[LastFrameSize - 1])
        //                                  {
        //                                      fs.Write(test, 4, LastFrameSize - 6);
        //                                      TxBuffer[0] = 0xA1;
        //                                      _serialComPort.Write(TxBuffer, 0, 1); // for Ack
        //                                      fs.Close();
        //                                  }
        //                              }

        //                              Thread.Sleep(1);
        //                              bytesToRead = 9;

        //                              while (true)
        //                              {
        //                                  _serialComPort.DiscardInBuffer();
        //                                  _serialComPort.DiscardOutBuffer();
        //                                  ReadTimeout = false;
        //                                  DataReceivedFlag = false;

        //                                  Thread.Sleep(0);
        //                                  bytesToRead = 9;

        //                                  if (WaitForresponseUPLD(bytesToRead) == ErrorCode.ReadTimeoutError)
        //                                  //if (WaitForresponse() == ErrorCode.ReadTimeoutError)
        //                                  {
        //                                      retryCount++;
        //                                      if (retryCount == 3)
        //                                      {
        //                                          _serialComPort.DiscardInBuffer();
        //                                          _serialComPort.DiscardOutBuffer();
        //                                          _serialComPort.Close();
        //                                          _serialComPort.Dispose();

        //                                          //		OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

        //                                          return ErrorCode.ReadTimeoutError;
        //                                      }
        //                                      continue;
        //                                      //goto c;
        //                                  }
        //                                  else
        //                                  {
        //                                      _serialComPort.DiscardInBuffer();
        //                                      _serialComPort.DiscardOutBuffer();
        //                                      break;
        //                                  }
        //                              }

        //                              receivedData = ASCIIEncoding.ASCII.GetString(RxBuffer);




        //                          }
        //                          Thread.Sleep(5);


        //                          //    OnProgressEvent(ASCIIEncoding.ASCII.GetString(RxBuffer), 1);
        //                          Console.WriteLine("COMPLETED");
        //                          fs.Close();

        //                          if (uploadInProgress == 1)
        //                              _deviceAppUpload = false;
        //                          else if (uploadInProgress == 3)
        //                              _deviceLadderUpload = false;
        //                          else if (uploadInProgress == 4)
        //                              _deviceAlaramUpload = false;
        //                          else if (uploadInProgress == 5)
        //                              _deviceDataLoggerUpload = false;
        //                      }

        //                      _serialComPort.DiscardInBuffer();
        //                      _serialComPort.DiscardOutBuffer();
        //                      _serialComPort.Close();
        //                      _serialComPort.Dispose();

        //                      return ErrorCode.Success;
        //                  }
        //              }
        //              else
        //              {
        //                  if (_serialComPort.IsOpen)
        //                      _serialComPort.DiscardInBuffer();

        //                  _serialComPort.DiscardOutBuffer();
        //                  _serialComPort.Close();
        //                  _serialComPort.Dispose();

        //                  //Return Unsuccess
        //              }
        //          }

        //          catch (Exception ex)
        //          {
        //              if (_serialComPort.IsOpen)
        //                  _serialComPort.DiscardInBuffer();
        //              _serialComPort.DiscardOutBuffer();
        //              _serialComPort.Close();
        //              _serialComPort.Dispose();

        //              return ErrorCode.AcknowledgementError;
        //          }

        //          _serialComPort.DiscardInBuffer();
        //          _serialComPort.DiscardOutBuffer();
        //          _serialComPort.Close();
        //          _serialComPort.Dispose();
        //          return ErrorCode.Success;
        //      }


        private ErrorCode WaitForresponseUPLD(int pByteToRead, string DWNL = "")
        {
            //	enableReadTimoutTimer(true, (int)(ApplicationFileLength / 655 + 10000));
            //else
            //	enableReadTimoutTimer(true);
            //ReadTimeout = false;
            //DataReceivedFlag = false;

            while (!DataReceivedFlag)
            {
                Thread.Sleep(0);

                SerialPortDataReceivedUPLD(_serialComPort, null, pByteToRead);
            }
            //enableReadTimoutTimer(false);


            return ReadTimeout ? ErrorCode.ReadTimeoutError : ErrorCode.Success;
        }

        private void SerialPortDataReceivedUPLD(object sender, SerialDataReceivedEventArgs e, int pBytesToRead)
        {
            SerialPort serialPort = (SerialPort)sender;
            int noBytesToRead = pBytesToRead;
            Array.Clear(RxBuffer, 0, RxBuffer.Length);
            Array.Resize(ref RxBuffer, bytesToRead);

            try
            {
                int bytesReceived = serialPort.Read(RxBuffer, 0, bytesToRead);
                int bytesReceivedBackup = 0;

                Console.WriteLine("##############################" + bytesReceived);


                while (bytesReceived < bytesToRead)
                {
                    bytesReceived += serialPort.Read(RxBuffer, bytesReceived, bytesToRead - bytesReceived);
                    if (bytesReceived != bytesReceivedBackup)
                    {
                        Console.WriteLine("ab");
                    }
                    bytesReceivedBackup = bytesReceived;
                    Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@" + bytesReceived);
                }

                ReadTimeout = false;
            }
            catch (TimeoutException)
            {
                ReadTimeout = true;
            }

            DataReceivedFlag = true;
        }

    }
}
