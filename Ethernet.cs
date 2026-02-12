using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Supporting_DLL;

namespace SerialPortCommunication
{
    class Ethernet : Device
    {
        static IPAddress _ipaddress;
        IPEndPoint _ipEndPoint;
        int _portNo;

        Socket _socket;

        string _ethernetFWFilePath;
        string _ethernetAppFilePath;
        string _ethernetFontFilePath;
        string _ethernetLadderFilePath;
        private string _ethernetSettingFilePath;
        private string _ethernetMacIDFilepath;
        private byte[] _ethernetFrmDcryptionCRC;
        private long _ethernetFrmDcryptionSize;
        private bool _ethernetIsResetFirmwareEncryptionMode;


        //private const int FRAMEWITHHEADERS = 262;
        //private const int FRAMEDATA = 256;

        int _modelID = -1;

        private const int FRAMEWITHHEADERS = 1030;
        private const int FRAMEDATA = 1024;


        private byte[] WriteBuffer = new byte[262];

        private byte[] ReadBuffer = new byte[262];

        int _result;

        public override int Initialize(CommunicationParameters pComParam)
        {
            _ipaddress = IPAddress.Parse(pComParam.IPAddress);

            _ipEndPoint = new IPEndPoint(_ipaddress, pComParam.PortNumber);

            _portNo = pComParam.PortNumber;

            _ethernetFWFilePath = pComParam.firmwareFilePath;

            _ethernetAppFilePath = pComParam.applicationFilePath;

            _ethernetFontFilePath = pComParam.fontFilePath;

            _ethernetLadderFilePath = pComParam.ladderFilePath;

            _ethernetSettingFilePath = pComParam.ethernetFilePath;

            _ethernetMacIDFilepath = pComParam.macIdFilePath;

            _deviceFWDownload = pComParam.isFirmware;

            _deviceAppDownload = pComParam.isApplication;

            _deviceFontDownload = pComParam.isFont;

            _deviceLadderDownload = pComParam.isLadder;

            _deviceEthernetSettingDownload = pComParam.isEthernetSettings;

            _deviceMacIdSettingDownload = pComParam.isMacId;

            _modelID = pComParam.modelID;

            _ethernetFrmDcryptionCRC = pComParam.FrmDcryptionCRC;

            _ethernetFrmDcryptionSize = pComParam.FrmDcryptionSize;

            _ethernetIsResetFirmwareEncryptionMode = pComParam.IsResetFirmwareEncryptionMode;


            return 0;
        }

        public override int Connect()
        {
            try
            {
                _socket = new Socket(_ipaddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                _socket.ReceiveTimeout = 5000;

                _socket.SendTimeout = 5000;

                _socket.Connect(_ipaddress, _portNo);

                return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public override int SendFile()
        {
            ErrorCode result = ReadData();

            string msg = "";

            if (result == ErrorCode.Success)
            {
                msg = "File sent successfully";
                OnProgressEvent("100", 0, CurrentDownload.MSG_DOWNLOAD_SUCCESS);

            }
            else
            {
                msg = "Failed to send file,\nError Code: " + result;
                OnProgressEvent("0", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

            }

            //MessageBox.Show(msg);

            return (int)result;
            //return -1;
        }

        public ErrorCode ReadData()
        {
            int RETRYCOUNT = 0;
            int MAXRETRY = 4;

            OnProgressEvent("-10", 1, CurrentDownload.None);//Connecting device

            string check = "GIC_SYNC";

            Array.Clear(WriteBuffer, 0, WriteBuffer.Length);
            WriteBuffer = ASCIIEncoding.ASCII.GetBytes(check);

            RETRYCOUNT = 0;
            int bootVersionNo = 0;

            while (checkThreadClosing())
            {
                try
                {
                    Array.Resize(ref WriteBuffer, 20);
                    _result = _socket.Send(WriteBuffer, 0, 20, SocketFlags.None);

                    Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
                    _result = _socket.Receive(ReadBuffer, 0, 16, SocketFlags.None);
                }
                catch (Exception ex)
                {                    
                    RETRYCOUNT++;

                    if (RETRYCOUNT >= MAXRETRY)
                    {
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        OnProgressEvent(((int)ErrorCode.CommunicationTermineted).ToString(), 0, CurrentDownload.None);

                        return ErrorCode.CommunicationTermineted;

                    }

                    continue;
                }

                Thread.Sleep(100);

                check = Encoding.UTF8.GetString(ReadBuffer, 0, 8);

                if (check != "GIC_SYNC")
                {
                    RETRYCOUNT++;

                    if (RETRYCOUNT >= MAXRETRY)
                    {
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        //OnProgressEvent(((int)ErrorCode.AcknowledgementError).ToString(), 0, CurrentDownload.None);

                        return ErrorCode.AcknowledgementError;
                    }

                    continue;
                }
                else
                {
                    int modelID = (ReadBuffer[8] + (ReadBuffer[9] << 8));

                    bootVersionNo = ReadBuffer[12];

                    if (bootVersionNo >= 15 && _deviceFWDownload)
                        _deviceFWEncyptionDownload = true;

                    if (modelID != _modelID)
                    {
                        string deviceModelId = Enum.GetName(typeof(CONSTANTS.ProjectDescription), _modelID);
                        string softwareModelId = Enum.GetName(typeof(CONSTANTS.ProjectDescription), modelID);

                        System.Windows.Forms.MessageBox.Show("Device " + deviceModelId + " Does Not Matched With Current " + softwareModelId + " Project");

                        _socket.Disconnect(false);
                        _socket.Dispose();
                        OnProgressEvent("-6", 1, CurrentDownload.None);//Product mismatched

                        return ErrorCode.ProductMismatched;
                    }

                    Thread.Sleep(5);

                    break;
                }
            }

            //  OnProgressEvent(ASCIIEncoding.ASCII.GetString(ReadBuffer), 1);

            check = "DOWNLOAD_INFO";

            Array.Clear(WriteBuffer, 0, WriteBuffer.Length);
            WriteBuffer = ASCIIEncoding.ASCII.GetBytes(check);

            Array.Resize(ref WriteBuffer, 35);
            WriteBuffer[check.Length] = (byte)((_deviceFWDownload ? 1 : 0) + (_deviceAppDownload ? 2 : 0) + (_deviceLadderDownload ? 4 : 0) + (_deviceFontDownload ? 8 : 0) + (_deviceEthernetSettingDownload ? 16 : 0)
                                        + (_deviceMacIdSettingDownload ? 32 : 0) + (_deviceFWEncyptionDownload ? 128 : 0)); // communication type bit number 1:Firmware 2:application 3:font 4:ladder 
            WriteBuffer[check.Length + 1] = 0; // communication type heigher byte
            WriteBuffer[check.Length + 2] = 1; // ack required
            WriteBuffer[check.Length + 3] = 0; //*1byte for timeout if no ack

            while (checkThreadClosing())
            {
                try
                {
                    _result = _socket.Send(WriteBuffer, 0, 35, SocketFlags.None);
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    _socket.Disconnect(false);
                    _socket.Dispose();
                   
                    return ErrorCode.CommunicationTermineted;
                }

                try
                {
                    Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
                    _result = _socket.Receive(ReadBuffer, 0, 5, SocketFlags.None);
                }
                catch (Exception ex)
                {
                   
                    RETRYCOUNT++;

                    if (RETRYCOUNT == MAXRETRY)
                    {
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        return ErrorCode.CommunicationTermineted;
                    }

                    continue;
                }

                check = Encoding.UTF8.GetString(ReadBuffer, 0, 5);

                if (check != "READY")
                {
                    RETRYCOUNT++;

                    if (RETRYCOUNT == MAXRETRY)
                    {
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        return ErrorCode.CommunicationTermineted;
                    }

                    continue;
                }
                else
                {
                    Thread.Sleep(5);

                    break;
                }
            }


            for (int iCount = 0; ; iCount++)
            {
                string filename;
                int downloadInProgress = 1;
                string DWNL = "FW_DWNL";
                if (_deviceFWDownload)
                {
                    filename = _ethernetFWFilePath;

                    if (bootVersionNo >= 15 && !_ethernetIsResetFirmwareEncryptionMode)
                    {
                        _ethernetFWFilePath += "_encrypted";
                        filename = _ethernetFWFilePath;
                    }

                    if (!File.Exists(filename))
                    {
                        return ErrorCode.FileNotExists;
                    }
                    downloadInProgress = 1;
                    DWNL = "FW_DWNL";
                }
                else if (_deviceAppDownload)
                {
                    filename = _ethernetAppFilePath;
                    if (!File.Exists(filename))
                    {
                        return ErrorCode.FileNotExists;
                    }
                    downloadInProgress = 2;
                    DWNL = "AP_DWNL";
                }
                else if (_deviceFontDownload)
                {
                    filename = _ethernetFontFilePath;
                    if (!File.Exists(filename))
                    {
                        return ErrorCode.FileNotExists;
                    }
                    downloadInProgress = 3;
                    DWNL = "FN_DWNL";
                }
                else if (_deviceLadderDownload)
                {
                    filename = _ethernetLadderFilePath;
                    if (!File.Exists(filename))
                    {
                        return ErrorCode.FileNotExists;
                    }
                    downloadInProgress = 4;
                    DWNL = "LD_DWNL";
                }
                else if (_deviceEthernetSettingDownload)
                {
                    filename = _ethernetSettingFilePath;
                    if (!File.Exists(filename))
                    {
                        return ErrorCode.FileNotExists;
                    }
                    downloadInProgress = 5;
                    DWNL = "ES_DWNL";
                }
                else if (_deviceMacIdSettingDownload)
                {
                    filename = _ethernetMacIDFilepath;
                    if (!File.Exists(filename))
                    {
                        return ErrorCode.FileNotExists;
                    }
                    downloadInProgress = 6;
                    DWNL = "MA_DWNL";
                }
                else if (_deviceFWEncyptionDownload)
                {
                    filename = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "EncryptionKeyFile");

                    if (!File.Exists(filename))
                    {
                        return ErrorCode.FileNotExists;
                    }
                    downloadInProgress = 8;
                    DWNL = "EF_DWNL";
                }

                else
                    break;

                Array.Clear(WriteBuffer, 0, WriteBuffer.Length);
                WriteBuffer = ASCIIEncoding.ASCII.GetBytes(DWNL);

                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

                BinaryReader sr = new BinaryReader(fs);

                sr.BaseStream.Seek(0, 0);

                byte[] fileLength = BitConverter.GetBytes(fs.Length);

                byte[] abc = new byte[fs.Length];

                sr.Read(abc, 0, (int)fs.Length);

                byte[] arrCRCTotal = CalculateCRC(abc, 0, (int)fs.Length);

                sr.BaseStream.Seek(0, 0);

                Array.Resize(ref WriteBuffer, 20);
                WriteBuffer[DWNL.Length] = fileLength[0]; //Firware size
                WriteBuffer[DWNL.Length + 1] = fileLength[1];
                WriteBuffer[DWNL.Length + 2] = fileLength[2];
                WriteBuffer[DWNL.Length + 3] = fileLength[3];

                if (DWNL == "AP_DWNL" || DWNL == "FW_DWNL")
                {
                    long _socketReceiveTimeOut = 350000;
                    _socket.ReceiveTimeout = ((int)_socketReceiveTimeOut) + 10000;
                }
                else
                    _socket.ReceiveTimeout = 2000;

                while (checkThreadClosing())
                {
                    try
                    {
                        _result = _socket.Send(WriteBuffer, 0, 20, SocketFlags.None);
                    }
                    catch (Exception ex)
                    {                       
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        sr.Close();
                        fs.Close();

                        return ErrorCode.CommunicationTermineted;
                    }

                    try
                    {
                        _result = _socket.Receive(ReadBuffer, 0, 13, SocketFlags.None);

                        check = Encoding.UTF8.GetString(ReadBuffer, 0, 13);

                        if (check != "FW_DWNL_START" && check != "AP_DWNL_START" && check != "FN_DWNL_START" &&
                            check != "LD_DWNL_START" && check != "ES_DWNL_START" && check != "MA_DWNL_START" && check != "EF_DWNL_START")
                        {
                            RETRYCOUNT++;

                            if (RETRYCOUNT == MAXRETRY)
                            {
                                _socket.Disconnect(false);
                                _socket.Dispose();

                               
                                return ErrorCode.CommunicationTermineted;
                            }
                                                       
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        RETRYCOUNT++;

                        if (RETRYCOUNT == MAXRETRY)
                        {
                            _socket.Disconnect(false);
                            _socket.Dispose();

                            sr.Close();
                            fs.Close();

                            
                            return ErrorCode.CommunicationTermineted;
                        }                        
                        continue;
                    }
                }

                int RetryCnt = 0;
                int MAX_RETRIES = 4;

                Array.Clear(WriteBuffer, 0, WriteBuffer.Length);
                Array.Resize(ref WriteBuffer, FRAMEWITHHEADERS);//ToDo declare constants

                _socket.SendTimeout = 5000;
                _socket.ReceiveTimeout = 5000;
                double Privious = 0;

                for (int i = 0; i < fs.Length / FRAMEDATA; i++)
                {
                    if (checkThreadClosing())
                    {
                        Array.Clear(WriteBuffer, 0, WriteBuffer.Length);
                        Array.Clear(ReadBuffer, 0, ReadBuffer.Length);

                        double per = ((100 * (i + 1)) / (fs.Length / FRAMEDATA));

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

                                case "MA_DWNL":

                                    OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.MacID);
                                    break;

                                case "RT_DWNL":

                                    OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Rtc_Sync);
                                    break;
                                case "EF_DWNL":

                                    OnProgressEvent(((int)per).ToString(), 0, CurrentDownload.Firmware_Encryption_key);
                                    break;
                            }
                        }

                        byte[] arrFrameNo = BitConverter.GetBytes(i + 1);
                        WriteBuffer[0] = arrFrameNo[0];
                        WriteBuffer[1] = arrFrameNo[1];
                        WriteBuffer[2] = arrFrameNo[2];
                        WriteBuffer[3] = arrFrameNo[3];

                        sr.Read(WriteBuffer, 4, FRAMEDATA);

                        byte[] arrCRC = CalculateCRC(WriteBuffer, 4, FRAMEDATA);

                        WriteBuffer[1028] = arrCRC[0];//ToDo Change index
                        WriteBuffer[1029] = arrCRC[1];

                        while (checkThreadClosing())
                        {
                            Array.Clear(ReadBuffer, 0, ReadBuffer.Length);

                            try
                            {
                                _result = _socket.Send(WriteBuffer, 0, FRAMEWITHHEADERS, SocketFlags.None);
                            }
                            catch (Exception ex)
                            {
                                _socket.Disconnect(false);
                                _socket.Dispose();

                                sr.Close();
                                fs.Close();
                                
                                return ErrorCode.WriteTimeoutError;
                            }

                            try
                            {
                                _result = _socket.Receive(ReadBuffer, 0, 1, SocketFlags.None);

                                if (ReadBuffer[0] == 0xA1)
                                {
                                    RetryCnt = 0;

                                    break;
                                }
                                else if (ReadBuffer[0] == 0xCC)
                                {
                                    RetryCnt++;

                                    if (RetryCnt == MAXRETRY)
                                    {
                                        _socket.Disconnect(false);
                                        _socket.Dispose();

                                        sr.Close();
                                        fs.Close();
                                        
                                        return ErrorCode.CRCMissmatched;
                                    }
                                    else
                                    {
                                      
                                        continue;
                                    }
                                }
                                else if (ReadBuffer[0] == 0xEE)
                                {
                                    _socket.Disconnect(false);
                                    _socket.Dispose();

                                    sr.Close();
                                    fs.Close();

                                    return ErrorCode.PageError;
                                }
                            }
                            catch (Exception ex)
                            {
                                RetryCnt++;
                              
                                if (RetryCnt == MAXRETRY)
                                {
                                    _socket.Disconnect(false);
                                    _socket.Dispose();

                                    sr.Close();
                                    fs.Close();

                                   
                                    return ErrorCode.AcknowledgementError;
                                }
                            }
                        }
                    }
                    else
                    {
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        sr.Close();
                        fs.Close();

                        OnProgressEvent("-11", 0, CurrentDownload.MSG_DOWNLOAD_FAILED);

                       
                        return ErrorCode.CommunicationTermineted;
                    }
                }


                //Here

                int noOfBytes = (int)(fs.Length - sr.BaseStream.Position);

                if (noOfBytes > 0)
                {
                    Array.Clear(WriteBuffer, 0, WriteBuffer.Length);
                    Array.Resize(ref WriteBuffer, (noOfBytes + 2 + 4));

                    if (noOfBytes < 3) // added due to firmware required.(if we pass the 1 bytes crc then firmware will send cc hence added)
                        Array.Resize(ref WriteBuffer, (noOfBytes + 2 + 4 + 2));

                    byte[] arrFrameNo = BitConverter.GetBytes(fs.Length / FRAMEDATA + 1);
                    WriteBuffer[0] = arrFrameNo[0];
                    WriteBuffer[1] = arrFrameNo[1];
                    WriteBuffer[2] = arrFrameNo[2];
                    WriteBuffer[3] = arrFrameNo[3];

                    sr.Read(WriteBuffer, 4, noOfBytes);

                    if (noOfBytes < 3)
                    {
                        if (noOfBytes == 1)
                        {
                            WriteBuffer[5] = 255; //TxBuffer[4] is data which is noofBytes
                            WriteBuffer[6] = 255;
                        }
                        else
                        {
                            WriteBuffer[6] = 255;//TxBuffer[4] and TxBuffer[5] are data which is noofBytes
                            WriteBuffer[7] = 255;

                        }


                        noOfBytes += 2;
                    }

                    byte[] arrCRC = CalculateCRC(WriteBuffer, 4, noOfBytes);
                    WriteBuffer[WriteBuffer.Length - 2] = arrCRC[0];//ToDo Change index
                    WriteBuffer[WriteBuffer.Length - 1] = arrCRC[1];

                    try
                    {
                        _result = _socket.Send(WriteBuffer, 0, (noOfBytes + 2 + 4), SocketFlags.None);
                    }
                    catch (Exception ex)
                    {
                       
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        sr.Close();
                        fs.Close();

                        return ErrorCode.CommunicationTermineted;
                    }

                    try
                    {
                        Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
                        _result = _socket.Receive(ReadBuffer, 0, 1, SocketFlags.None);
                    }
                    catch (Exception ex)
                    {
                       
                        _socket.Disconnect(false);
                        _socket.Dispose();

                        return ErrorCode.CommunicationTermineted;
                    }

                    check = Encoding.UTF8.GetString(ReadBuffer, 0, 1);

                    if (ReadBuffer[0] == 0xA1)
                    {
                        RetryCnt = 0;
                    }
                    else if (ReadBuffer[0] == 0xCC)
                    {
                        //RetryCnt++;

                        //REMAINING NAC LOGIC

                        for (RetryCnt = 1; RetryCnt < 4; RetryCnt++)
                        {

                            //Change Shubham
                            //Below code commneted and added in try catch

                            //_result = _socket.Send(WriteBuffer, 0, (noOfBytes + 2 + 4), SocketFlags.None);

                            //Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
                            //_result = _socket.Receive(ReadBuffer, 0, 1, SocketFlags.None);

                            try
                            {
                                _result = _socket.Send(WriteBuffer, 0, (noOfBytes + 2 + 4), SocketFlags.None);

                                Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
                                _result = _socket.Receive(ReadBuffer, 0, 1, SocketFlags.None);
                            }
                            catch (Exception ex)
                            {                                
                                _socket.Disconnect(false);
                                _socket.Dispose();
                                //GauriJ Started
                                //OnProgressEvent(((int)ErrorCode.CommunicationTermineted).ToString(), 0, CurrentDownload.None);
                                return ErrorCode.CommunicationTermineted;
                            }

                            if (RetryCnt == 3)
                            {
                                _socket.Disconnect(false);
                                _socket.Dispose();
                                //GauriJ Started
                                //OnProgressEvent(((int)ErrorCode.CommunicationTermineted).ToString(), 0, CurrentDownload.None);
                                return ErrorCode.CommunicationTermineted;
                                //GauriJ Ended
                            }
                            else if (_result != 1)
                            {
                                _socket.Disconnect(false);
                                _socket.Dispose();
                                return ErrorCode.ReadTimeoutError;
                            }
                            else if (ReadBuffer[0] == 0xA1)
                            {
                                break;
                            }
                        }

                        //return ErrorCode.CommunicationTermineted;
                    }
                    else if (ReadBuffer[0] == 0xEE)
                    {
                        _socket.Disconnect(false);
                        _socket.Dispose();
                        //GauriJ Started
                        //OnProgressEvent(((int)ErrorCode.PageError).ToString(), 0, CurrentDownload.None);
                        return ErrorCode.PageError;
                        //GauriJ Ended
                    }

                }

                if (downloadInProgress == 1)
                    check = "FW_FINISH";
                else if (downloadInProgress == 2)
                    check = "AP_FINISH";
                else if (downloadInProgress == 3)
                    check = "FN_FINISH";
                else if (downloadInProgress == 4)
                    check = "LD_FINISH";
                else if (downloadInProgress == 5)
                    check = "ES_FINISH";
                else if (downloadInProgress == 6)
                    check = "MA_FINISH";
                else if (downloadInProgress == 7)
                    check = "RT_FINISH";
                else if (downloadInProgress == 8)
                    check = "EF_FINISH";

                Array.Clear(WriteBuffer, 0, WriteBuffer.Length);
                WriteBuffer = ASCIIEncoding.ASCII.GetBytes(check);
                Array.Resize(ref WriteBuffer, 20);

                WriteBuffer[check.Length] = fileLength[0]; //Firware size
                WriteBuffer[check.Length + 1] = fileLength[1];
                WriteBuffer[check.Length + 2] = fileLength[2];
                WriteBuffer[check.Length + 3] = fileLength[3];

                WriteBuffer[check.Length + 4] = arrCRCTotal[0]; // 2byte crc
                WriteBuffer[check.Length + 5] = arrCRCTotal[1];

                if (bootVersionNo >= 15 && DWNL == "FW_DWNL" && !_ethernetIsResetFirmwareEncryptionMode)
                {
                    WriteBuffer[check.Length + 4] = _ethernetFrmDcryptionCRC[0]; // 2byte crc
                    WriteBuffer[check.Length + 5] = _ethernetFrmDcryptionCRC[1];
                }

                WriteBuffer[check.Length + 6] = BitConverter.GetBytes(_ethernetFrmDcryptionSize)[0];
                WriteBuffer[check.Length + 7] = BitConverter.GetBytes(_ethernetFrmDcryptionSize)[1];
                WriteBuffer[check.Length + 8] = BitConverter.GetBytes(_ethernetFrmDcryptionSize)[2];
                WriteBuffer[check.Length + 9] = BitConverter.GetBytes(_ethernetFrmDcryptionSize)[3];


                //Change Shubham
                //Below code commneted and added in try catch
                //_result = _socket.Send(WriteBuffer, 0, WriteBuffer.Length, SocketFlags.None);

                try
                {
                    _result = _socket.Send(WriteBuffer, 0, WriteBuffer.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {

                    return ErrorCode.CommunicationTermineted;
                }

                try
                {

                    Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
                    _result = _socket.Receive(ReadBuffer, 0, 1, SocketFlags.None);
                }
                catch (Exception ex)
                {
                   
                    //sr.Close();
                    //fs.Close();

                    //return ErrorCode.CommunicationTermineted;
                }

                check = Encoding.UTF8.GetString(ReadBuffer, 0, 1);

                if (ReadBuffer[0] != 0xA1)
                {
                    _socket.Disconnect(false);
                    _socket.Dispose();


                    //return ErrorCode.CommunicationTermineted;
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
                else if (downloadInProgress == 8)
                    _deviceFWEncyptionDownload = false;
            }

            try
            {
                _socket.Disconnect(false);
                _socket.Dispose();
            }
            catch (Exception ex)
            {
               
            }

            OnProgressEvent("-12", 0, CurrentDownload.MSG_DOWNLOAD_SUCCESS);

            return ErrorCode.Success;
        }

        internal bool checkThreadClosing()
        {
            return _deviceShouldThreadExit;
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

    }
}
