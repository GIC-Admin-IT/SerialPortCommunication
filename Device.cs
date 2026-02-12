using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections;
using System.Threading;

namespace SerialPortCommunication
{
	class Device
	{
		Thread thread;

		protected bool _deviceAppDownload = false;
		protected bool _deviceFWDownload = false;
		protected bool _deviceFontDownload = false;
		protected bool _deviceLadderDownload = false;
		protected bool _deviceEthernetSettingDownload = false;
		protected bool _deviceMacIdSettingDownload = false;
		protected bool _deviceRTCSyncDownload = false;

		protected bool _deviceFWEncyptionDownload = false;

		protected bool _deviceAppUpload = false;
		protected bool _deviceLadderUpload = false;
		protected bool _deviceAlaramUpload = false;
		protected bool _deviceDataLoggerUpload = false;

		public volatile bool _deviceShouldThreadExit = false;

		public delegate void __showDownloadProgress(string pstrPersentage, int isProgressOrDebug, CurrentDownload pCurrentDownload);
		public event __showDownloadProgress _showDownloadProgress;

		///Progress bar delegate event

		public virtual int Connect() { return -1; }
		public virtual int Initialize(CommunicationParameters pComParam) { return -1; }
		public virtual int SendFile() { return -1; }
		public virtual int uploadFile() { return -1; }
		public virtual int ReceiveFile() { return -1; }

		protected void OnProgressEvent(string pStrProgress, int isProgressOrDebug, CurrentDownload pCurrentDownload)
		{
            //thread = new Thread(() => _showDownloadProgress(pStrProgress, isProgressOrDebug, pCurrentDownload));
            //thread.Start();

            //_showDownloadProgress(pStrProgress, isProgressOrDebug);

            _showDownloadProgress(pStrProgress, isProgressOrDebug, pCurrentDownload);
		}

		internal int connectUIserialToController(ArrayList arr)
		{
			return -1;
		}
	}

	public enum ErrorCode
	{
		NoError = 0,
		AcknowledgementError = 1, //-1
		Success,        //-2
		ReadTimeoutError, //-3
		WriteTimeoutError,  //-4
		CommunicationTermineted, //-5
		ProductMismatched, //-6
		PageError, //-7
		FileNotExists,  //-8
		NoResponseFromDevice ,//-9
		CRCMissmatched//-9
	}

	public enum CurrentDownload
	{
		None = 0,
		Firmware,
		Application,
		Ladder,
		Font,
		Rtc_Sync,
		Ethernet_Settings,
		MacID,
		MSG_DOWNLOAD_SUCCESS,
		MSG_DOWNLOAD_FAILED,
		MSG_PORT_ACCESS_DENIED,
		MSG_PORT_ERROR,
		MSG_COMMINICATION_SOMETHING_WRONG,
		Firmware_Encryption_key

	}

	public struct CommunicationParameters
	{
		public CommunicationMode Mode;
		public String PortName;
		public int PortNumber;
		public int BaudRate;
		public Parity Parity;
		public int DataBit;
		public StopBits StopBit;
		public string IPAddress;
		public int SubnetMask;
		public int Gateway;
		public byte[] MacID;

		public bool isApplication;
		public bool isFirmware;
		public bool isFont;
		public bool isLadder;
		public bool isMacId;
		public bool isEthernetSettings;
		public bool isDataLog;
		public bool isRTCSync;


		public string applicationFilePath;
		public string RTCSyncFilePath;
		public string firmwareFilePath;
		public string ethernetFilePath;
		public string fontFilePath;
		public string ladderFilePath;
		public string macIdFilePath;

		public bool _isStandAloneUtility;

		public bool _isDownload;

		public int modelID;

		public bool Acknowledgement;
		public enum CommunicationMode
		{
			Serial,
			USB,
			Ethernet
		}

		public byte[] FrmDcryptionCRC;
		public long FrmDcryptionSize;
		public bool IsResetFirmwareEncryptionMode;
	}
}
