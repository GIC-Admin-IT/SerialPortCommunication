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
        private SerialPort _serialComPort = new SerialPort();

		private string _serialFWFilePath;
		private string _serialAppFilePath;
		private string _serialFontFilePath;
		private string _serialMacIDFilepath;
		private string _serialLadderFilePath;
		private string _serialSystemRTCFilepath;
		private string _serialEthernetSettingFilePath;

		private byte[] TxBuffer = new byte[8];
        private byte[] RxBuffer = new byte[8];

        private readonly int MAXRETRYFORSYNCFRAME = 3;

        public override int Initialize(CommunicationParameters pComParam)
        {
			if (_serialComPort.IsOpen)
				_serialComPort.Close();
			if (_serialComPort != null)
				_serialComPort = null;

			_serialComPort = new SerialPort();

			_serialComPort.PortName = pComParam.PortName;
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

			_deviceFWDownload = pComParam.isFirmware;
			_deviceAppDownload = pComParam.isApplication;
			_deviceFontDownload = pComParam.isFont;
			_deviceLadderDownload = pComParam.isLadder;
			_deviceMacIdSettingDownload = pComParam.isMacId;
			_deviceEthernetSettingDownload = pComParam.isEthernetSettings;
			_deviceRTCSyncDownload = pComParam.isRTCSync;

			_deviceAppUpload = pComParam.isApplication;

            _serialComPort.DataReceived += _serialComPort_DataReceived;

			//_isStandAloneUtility = pComParam._isStandAloneUtility;
			//_modelID = pComParam.modelID;
			//_isDownload = pComParam._isDownload;

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

				//This if is added because Upload File logic is written by Synchronous Method
				//_serialComPort.DataReceived += new SerialDataReceivedEventHandler(SerialPortDataReceived);
			}
			catch (UnauthorizedAccessException)
			{
				OnProgressEvent("0", 0, CurrentDownload.MSG_PORT_ERROR);
				return -1;
				//MessageBox.Show("Access denied, please check if another process is not using the resource.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (ArgumentException)
			{
				OnProgressEvent("0", 0, CurrentDownload.MSG_COMMINICATION_SOMETHING_WRONG);
				return -1;
				//MessageBox.Show(".", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (System.IO.IOException)
			{
				OnProgressEvent("0", 0, CurrentDownload.MSG_PORT_ACCESS_DENIED);
				return -1;
				//MessageBox.Show("Something went wrong while connecting.\nPlease try again.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception e)
            {
				OnProgressEvent("0", 0, CurrentDownload.MSG_COMMINICATION_SOMETHING_WRONG);
				return -1;
			}
			return 0;
        }
		private void ClosePort()
		{
			try
			{
				_serialComPort.Close();
			}
			catch (Exception e)
			{
				MessageBox.Show("Error in port close!");
			}
		}
		public override int SendFile()
        {
			string strSync = "GIC_SYNC";//Add 12 reserved bytes here
			int retryCount = 0;
			ErrorCode communicationStatus =  ErrorCode.Success;

			OnProgressEvent(0.ToString(), 1, CurrentDownload.None);

			while (true)
			{
				retryCount++;
				if (retryCount > MAXRETRYFORSYNCFRAME)
				{
					communicationStatus = ErrorCode.NoResponseFromDevice;
					break;
				}
				else if (SendFrame(strSync, 12, 35) == 0) // Successfully Sync done
				{
					if (ReceiveFrame(16) == 0)
						break;
					else
						continue;
				}
                else		//Retry
                {
					continue;
				}
			}
			if(communicationStatus == ErrorCode.NoResponseFromDevice)
            {

            }

			ClosePort();
			return (int)communicationStatus;
        }
        private int SendFrame(string pStrToSend, int pIntFrameSize, int pIntNextFrameSize)
		{
			int retVal = 0;
			byte[] data = ASCIIEncoding.ASCII.GetBytes(pStrToSend);
			Array.Resize(ref data, pIntFrameSize);

			Array.Clear(TxBuffer, 0, TxBuffer.Length);
			Array.Resize(ref TxBuffer, data.Length);
			TxBuffer = data;
			TxBuffer[pStrToSend.Length] = Convert.ToByte(pIntNextFrameSize & 0xFF);

			_serialComPort.DiscardInBuffer();
			_serialComPort.DiscardOutBuffer();

			_serialComPort.RtsEnable = false;
			_serialComPort.DtrEnable = false;
			Thread.Sleep(1);
			_serialComPort.RtsEnable = true;
			_serialComPort.DtrEnable = true;
			Thread.Sleep(1);

			try
			{
				_serialComPort.Write(TxBuffer, 0, TxBuffer.Length);
			}
			catch(Exception e)
            {
				retVal = (int)ErrorCode.NoResponseFromDevice;
            }

			return retVal;
		}

		private void _serialComPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{

		}

		private int ReceiveFrame(int pIntBytesToRead)
		{
			int retVal = (int)ErrorCode.NoError;
			Array.Resize(ref RxBuffer, pIntBytesToRead);
			Array.Clear(RxBuffer, 0, RxBuffer.Length);

			try
			{
				Thread.Sleep(200);
				int bytesReceived = 0;// _serialComPort.Read(RxBuffer, 0, pIntBytesToRead);
				Console.WriteLine("##############################:" + _serialComPort.BytesToRead);

				while (bytesReceived < pIntBytesToRead)
				{
					Thread.Sleep(0);
					Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@:" + _serialComPort.ReadExisting());
					bytesReceived += _serialComPort.Read(RxBuffer, bytesReceived, 1);
				}
			}
			catch (TimeoutException)
			{
				retVal = (int)ErrorCode.NoResponseFromDevice;
			}
			return retVal;
		}
		public override int ReceiveFile()
        {
            return 0;
        }
    }
}