using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using LumiSoft.Net.IO;
using LumiSoft.Net.TCP;

namespace LumiSoft.Net.FTP.Client;

public class FTP_Client : TCP_Client
{
	private class DataConnection : IDisposable
	{
		private FTP_Client m_pOwner;

		private Socket m_pSocket;

		private int m_ActivePort = -1;

		private FTP_TransferMode m_TransferMode;

		private DateTime m_LastActivity;

		private bool m_IsActive;

		public IPEndPoint LocalEndPoint => (IPEndPoint)m_pSocket.LocalEndPoint;

		public DateTime LastActivity => m_LastActivity;

		public bool IsActive => m_IsActive;

		public DataConnection(FTP_Client owner)
		{
			m_pOwner = owner;
			CreateSocket();
		}

		public void Dispose()
		{
			if (m_pSocket != null)
			{
				m_pSocket.Close();
				m_pSocket = null;
			}
			m_pOwner = null;
		}

		public void SwitchToActive()
		{
			m_pSocket.Listen(1);
			m_TransferMode = FTP_TransferMode.Active;
			m_pOwner.LogAddText("FTP data channel switched to Active mode, listening FTP server connect to '" + m_pSocket.LocalEndPoint.ToString() + "'.");
		}

		public void SwitchToPassive(IPEndPoint remoteEP)
		{
			if (remoteEP == null)
			{
				throw new ArgumentNullException("remoteEP");
			}
			m_pOwner.LogAddText("FTP data channel switched to Passive mode, connecting to FTP server '" + remoteEP.ToString() + "'.");
			m_pSocket.Connect(remoteEP);
			m_TransferMode = FTP_TransferMode.Passive;
			m_pOwner.LogAddText("FTP Passive data channel established, localEP='" + m_pSocket.LocalEndPoint.ToString() + "' remoteEP='" + m_pSocket.RemoteEndPoint.ToString() + "'.");
		}

		public void ReadAll(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			m_IsActive = true;
			try
			{
				if (m_TransferMode == FTP_TransferMode.Active)
				{
					using (NetworkStream source = WaitFtpServerToConnect(20))
					{
						long size = TransferStream(source, stream);
						m_pOwner.LogAddRead(size, "Data connection readed " + size + " bytes.");
						return;
					}
				}
				if (m_TransferMode == FTP_TransferMode.Passive)
				{
					using (NetworkStream source2 = new NetworkStream(m_pSocket, ownsSocket: true))
					{
						long size2 = TransferStream(source2, stream);
						m_pOwner.LogAddRead(size2, "Data connection readed " + size2 + " bytes.");
						return;
					}
				}
			}
			finally
			{
				m_IsActive = false;
				CleanUpSocket();
			}
		}

		public void WriteAll(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			try
			{
				if (m_TransferMode == FTP_TransferMode.Active)
				{
					using (NetworkStream target = WaitFtpServerToConnect(20))
					{
						long size = TransferStream(stream, target);
						m_pOwner.LogAddWrite(size, "Data connection wrote " + size + " bytes.");
						return;
					}
				}
				if (m_TransferMode == FTP_TransferMode.Passive)
				{
					using (NetworkStream target2 = new NetworkStream(m_pSocket, ownsSocket: true))
					{
						long size2 = TransferStream(stream, target2);
						m_pOwner.LogAddWrite(size2, "Data connection wrote " + size2 + " bytes.");
						return;
					}
				}
			}
			finally
			{
				m_IsActive = false;
				CleanUpSocket();
			}
		}

		private NetworkStream WaitFtpServerToConnect(int waitTime)
		{
			try
			{
				m_pOwner.LogAddText("FTP Active data channel waiting FTP server connect to '" + m_pSocket.LocalEndPoint.ToString() + "'.");
				DateTime now = DateTime.Now;
				while (!m_pSocket.Poll(0, SelectMode.SelectRead))
				{
					Thread.Sleep(50);
					if (now.AddSeconds(waitTime) < DateTime.Now)
					{
						m_pOwner.LogAddText("FTP server didn't connect during expected time.");
						throw new IOException("FTP server didn't connect during expected time.");
					}
				}
				Socket socket = m_pSocket.Accept();
				m_pOwner.LogAddText("FTP Active data channel established, localEP='" + socket.LocalEndPoint.ToString() + "' remoteEP='" + socket.RemoteEndPoint.ToString() + "'.");
				return new NetworkStream(socket, ownsSocket: true);
			}
			finally
			{
				CleanUpSocket();
			}
		}

		private void CreateSocket()
		{
			if (m_pOwner.LocalEndPoint.Address.AddressFamily == AddressFamily.InterNetwork)
			{
				m_pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}
			else
			{
				m_pSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
			}
			m_pSocket.SendTimeout = m_pOwner.Timeout;
			m_pSocket.ReceiveTimeout = m_pOwner.Timeout;
			int num = 0;
			if (m_pOwner.DataPortRange == null)
			{
				num = 0;
			}
			else
			{
				if (m_ActivePort == -1 || m_ActivePort + 1 > m_pOwner.DataPortRange.End)
				{
					m_ActivePort = m_pOwner.DataPortRange.Start;
				}
				else
				{
					m_ActivePort++;
				}
				num = m_ActivePort;
			}
			if (m_pOwner.DataIP == null || m_pOwner.DataIP == IPAddress.Any)
			{
				m_pSocket.Bind(new IPEndPoint(m_pOwner.LocalEndPoint.Address, num));
			}
			else
			{
				m_pSocket.Bind(new IPEndPoint(m_pOwner.DataIP, num));
			}
		}

		public void CleanUpSocket()
		{
			if (m_pSocket != null)
			{
				m_pSocket.Close();
			}
			CreateSocket();
		}

		private long TransferStream(Stream source, Stream target)
		{
			long num = 0L;
			byte[] array = new byte[32000];
			while (true)
			{
				int num2 = source.Read(array, 0, array.Length);
				if (num2 == 0)
				{
					break;
				}
				target.Write(array, 0, num2);
				num += num2;
				m_LastActivity = DateTime.Now;
			}
			return num;
		}
	}

	private FTP_TransferMode m_TransferMode = FTP_TransferMode.Passive;

	private IPAddress m_pDataConnectionIP;

	private PortRange m_pDataPortRange;

	private string m_GreetingText = "";

	private List<string> m_pExtCapabilities;

	private GenericIdentity m_pAuthdUserIdentity;

	private DataConnection m_pDataConnection;

	public FTP_TransferMode TransferMode
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_TransferMode;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_TransferMode = value;
		}
	}

	public IPAddress DataIP
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pDataConnectionIP;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_pDataConnectionIP = value;
			if (IsConnected)
			{
				m_pDataConnection.CleanUpSocket();
			}
		}
	}

	public PortRange DataPortRange
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return m_pDataPortRange;
		}
		set
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			m_pDataPortRange = value;
			if (IsConnected)
			{
				m_pDataConnection.CleanUpSocket();
			}
		}
	}

	public string GreetingText
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_GreetingText;
		}
	}

	public string[] ExtenededCapabilities
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_pExtCapabilities.ToArray();
		}
	}

	public override GenericIdentity AuthenticatedUserIdentity
	{
		get
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!IsConnected)
			{
				throw new InvalidOperationException("You must connect first.");
			}
			return m_pAuthdUserIdentity;
		}
	}

	public override void Dispose()
	{
		lock (this)
		{
			base.Dispose();
			m_pDataConnectionIP = null;
		}
	}

	public override void Disconnect()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("FTP client is not connected.");
		}
		try
		{
			WriteLine("QUIT");
		}
		catch
		{
		}
		try
		{
			base.Disconnect();
		}
		catch
		{
		}
		m_pExtCapabilities = null;
		m_pAuthdUserIdentity = null;
		if (m_pDataConnection != null)
		{
			m_pDataConnection.Dispose();
			m_pDataConnection = null;
		}
	}

	public void Reinitialize()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		WriteLine("REIN");
		string[] array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
	}

	public void Authenticate(string userName, string password)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (base.IsAuthenticated)
		{
			throw new InvalidOperationException("Session is already authenticated.");
		}
		if (string.IsNullOrEmpty(userName))
		{
			throw new ArgumentNullException("userName");
		}
		if (password == null)
		{
			password = "";
		}
		WriteLine("USER " + userName);
		string[] array = ReadResponse();
		if (array[0].StartsWith("331"))
		{
			WriteLine("PASS " + password);
			array = ReadResponse();
			if (!array[0].StartsWith("230"))
			{
				throw new FTP_ClientException(array[0]);
			}
			m_pAuthdUserIdentity = new GenericIdentity(userName, "ftp-user/pass");
			return;
		}
		throw new FTP_ClientException(array[0]);
	}

	public void Noop()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		WriteLine("NOOP");
		string[] array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
	}

	public void Abort()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		WriteLine("ABOR");
		string text = ReadLine();
		if (!text.StartsWith("2"))
		{
			throw new FTP_ClientException(text);
		}
	}

	public string GetCurrentDir()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		WriteLine("PWD");
		string[] array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
		StringReader stringReader = new StringReader(array[0]);
		stringReader.ReadWord();
		return stringReader.ReadWord();
	}

	public void SetCurrentDir(string path)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argumnet 'path' must be specified.");
		}
		WriteLine("CWD " + path);
		string[] array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
	}

	public FTP_ListItem[] GetList()
	{
		return GetList(null);
	}

	public FTP_ListItem[] GetList(string path)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pDataConnection.IsActive)
		{
			throw new InvalidOperationException("There is already active read/write operation on data connection.");
		}
		List<FTP_ListItem> list = new List<FTP_ListItem>();
		SetTransferType(TransferType.Binary);
		if (m_TransferMode == FTP_TransferMode.Passive)
		{
			Pasv();
		}
		else
		{
			Port();
		}
		bool flag = false;
		foreach (string pExtCapability in m_pExtCapabilities)
		{
			if (pExtCapability.ToLower().StartsWith("mlsd"))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (string.IsNullOrEmpty(path))
			{
				WriteLine("MLSD");
			}
			else
			{
				WriteLine("MLSD " + path);
			}
			string[] array = ReadResponse();
			if (!array[0].StartsWith("1"))
			{
				throw new FTP_ClientException(array[0]);
			}
			MemoryStream memoryStream = new MemoryStream();
			m_pDataConnection.ReadAll(memoryStream);
			array = ReadResponse();
			if (!array[0].StartsWith("2"))
			{
				throw new FTP_ClientException(array[0]);
			}
			byte[] buffer = new byte[8000];
			memoryStream.Position = 0L;
			SmartStream smartStream = new SmartStream(memoryStream, owner: true);
			while (true)
			{
				SmartStream.ReadLineAsyncOP readLineAsyncOP = new SmartStream.ReadLineAsyncOP(buffer, SizeExceededAction.JunkAndThrowException);
				smartStream.ReadLine(readLineAsyncOP, async: false);
				if (readLineAsyncOP.Error != null)
				{
					throw readLineAsyncOP.Error;
				}
				string lineUtf = readLineAsyncOP.LineUtf8;
				if (lineUtf == null)
				{
					break;
				}
				string[] array2 = lineUtf.Substring(0, lineUtf.LastIndexOf(';')).Split(';');
				string name = lineUtf.Substring(lineUtf.LastIndexOf(';') + 1).Trim();
				string text = "";
				long size = 0L;
				DateTime modified = DateTime.MinValue;
				string[] array3 = array2;
				for (int i = 0; i < array3.Length; i++)
				{
					string[] array4 = array3[i].Split('=');
					if (array4[0].ToLower() == "type")
					{
						text = array4[1].ToLower();
					}
					else if (array4[0].ToLower() == "size")
					{
						size = Convert.ToInt64(array4[1]);
					}
					else if (array4[0].ToLower() == "modify")
					{
						modified = DateTime.ParseExact(array4[1], "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
					}
				}
				if (text == "dir")
				{
					list.Add(new FTP_ListItem(name, 0L, modified, isDir: true));
				}
				else if (text == "file")
				{
					list.Add(new FTP_ListItem(name, size, modified, isDir: false));
				}
			}
		}
		else
		{
			if (string.IsNullOrEmpty(path))
			{
				WriteLine("LIST");
			}
			else
			{
				WriteLine("LIST " + path);
			}
			string[] array5 = ReadResponse();
			if (!array5[0].StartsWith("1"))
			{
				throw new FTP_ClientException(array5[0]);
			}
			MemoryStream memoryStream2 = new MemoryStream();
			m_pDataConnection.ReadAll(memoryStream2);
			array5 = ReadResponse();
			if (!array5[0].StartsWith("2"))
			{
				throw new FTP_ClientException(array5[0]);
			}
			memoryStream2.Position = 0L;
			SmartStream smartStream2 = new SmartStream(memoryStream2, owner: true);
			string[] formats = new string[2] { "M-d-yy h:mmtt", "MM-dd-yy HH:mm" };
			string[] formats2 = new string[2] { "MMM d H:mm", "MMM d yyyy" };
			SmartStream.ReadLineAsyncOP readLineAsyncOP2 = new SmartStream.ReadLineAsyncOP(new byte[8000], SizeExceededAction.JunkAndThrowException);
			while (true)
			{
				smartStream2.ReadLine(readLineAsyncOP2, async: false);
				if (readLineAsyncOP2.Error != null)
				{
					throw readLineAsyncOP2.Error;
				}
				if (readLineAsyncOP2.BytesInBuffer == 0)
				{
					break;
				}
				string lineUtf2 = readLineAsyncOP2.LineUtf8;
				string text2 = "unix";
				if (lineUtf2 != null)
				{
					StringReader stringReader = new StringReader(lineUtf2);
					if (DateTime.TryParseExact(stringReader.ReadWord() + " " + stringReader.ReadWord(), formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var _))
					{
						text2 = "win";
					}
				}
				try
				{
					if (text2 == "win")
					{
						StringReader stringReader2 = new StringReader(lineUtf2);
						DateTime modified2 = DateTime.ParseExact(stringReader2.ReadWord() + " " + stringReader2.ReadWord(), formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
						stringReader2.ReadToFirstChar();
						if (stringReader2.StartsWith("<dir>", case_sensitive: false))
						{
							stringReader2.ReadSpecifiedLength(5);
							stringReader2.ReadToFirstChar();
							list.Add(new FTP_ListItem(stringReader2.ReadToEnd(), 0L, modified2, isDir: true));
						}
						else
						{
							long size2 = Convert.ToInt64(stringReader2.ReadWord());
							stringReader2.ReadToFirstChar();
							list.Add(new FTP_ListItem(stringReader2.ReadToEnd(), size2, modified2, isDir: false));
						}
						continue;
					}
					StringReader stringReader3 = new StringReader(lineUtf2);
					string text3 = stringReader3.ReadWord();
					stringReader3.ReadWord();
					stringReader3.ReadWord();
					stringReader3.ReadWord();
					long size3 = Convert.ToInt64(stringReader3.ReadWord());
					DateTime modified3 = DateTime.ParseExact(stringReader3.ReadWord() + " " + stringReader3.ReadWord() + " " + stringReader3.ReadWord(), formats2, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
					stringReader3.ReadToFirstChar();
					string text4 = stringReader3.ReadToEnd();
					if (text4 != "." && text4 != "..")
					{
						if (text3.StartsWith("d"))
						{
							list.Add(new FTP_ListItem(text4, 0L, modified3, isDir: true));
						}
						else
						{
							list.Add(new FTP_ListItem(text4, size3, modified3, isDir: false));
						}
					}
				}
				catch
				{
				}
			}
		}
		return list.ToArray();
	}

	public void GetFile(string path, string storePath)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pDataConnection.IsActive)
		{
			throw new InvalidOperationException("There is already active read/write operation on data connection.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		if (storePath == null)
		{
			throw new ArgumentNullException("storePath");
		}
		if (storePath == "")
		{
			throw new ArgumentException("Argument 'storePath' value must be specified.");
		}
		using FileStream stream = File.Create(storePath);
		GetFile(path, stream);
	}

	public void GetFile(string path, Stream stream)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pDataConnection.IsActive)
		{
			throw new InvalidOperationException("There is already active read/write operation on data connection.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SetTransferType(TransferType.Binary);
		if (m_TransferMode == FTP_TransferMode.Passive)
		{
			Pasv();
		}
		else
		{
			Port();
		}
		WriteLine("RETR " + path);
		string[] array = ReadResponse();
		if (!array[0].StartsWith("1"))
		{
			throw new FTP_ClientException(array[0]);
		}
		m_pDataConnection.ReadAll(stream);
		array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
	}

	public void AppendToFile(string path, Stream stream)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pDataConnection.IsActive)
		{
			throw new InvalidOperationException("There is already active read/write operation on data connection.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SetTransferType(TransferType.Binary);
		if (m_TransferMode == FTP_TransferMode.Passive)
		{
			Pasv();
		}
		else
		{
			Port();
		}
		WriteLine("APPE " + path);
		string[] array = ReadResponse();
		if (!array[0].StartsWith("1"))
		{
			throw new FTP_ClientException(array[0]);
		}
		m_pDataConnection.WriteAll(stream);
		array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
	}

	public void StoreFile(string path, string sourcePath)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pDataConnection.IsActive)
		{
			throw new InvalidOperationException("There is already active read/write operation on data connection.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		if (sourcePath == null)
		{
			throw new ArgumentNullException("sourcePath");
		}
		if (sourcePath == "")
		{
			throw new ArgumentException("Argument 'sourcePath' value must be specified.");
		}
		using FileStream stream = File.OpenRead(sourcePath);
		StoreFile(path, stream);
	}

	public void StoreFile(string path, Stream stream)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (m_pDataConnection.IsActive)
		{
			throw new InvalidOperationException("There is already active read/write operation on data connection.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SetTransferType(TransferType.Binary);
		if (m_TransferMode == FTP_TransferMode.Passive)
		{
			Pasv();
		}
		else
		{
			Port();
		}
		WriteLine("STOR " + path);
		string[] array = ReadResponse();
		if (!array[0].StartsWith("1"))
		{
			throw new FTP_ClientException(array[0]);
		}
		m_pDataConnection.WriteAll(stream);
		array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
	}

	public void DeleteFile(string path)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		WriteLine("DELE " + path);
		string text = ReadLine();
		if (!text.StartsWith("250"))
		{
			throw new FTP_ClientException(text);
		}
	}

	public void Rename(string fromPath, string toPath)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (fromPath == null)
		{
			throw new ArgumentNullException("fromPath");
		}
		if (fromPath == "")
		{
			throw new ArgumentException("Argument 'fromPath' value must be specified.");
		}
		if (toPath == null)
		{
			throw new ArgumentNullException("toPath");
		}
		if (toPath == "")
		{
			throw new ArgumentException("Argument 'toPath' value must be specified.");
		}
		WriteLine("RNFR " + fromPath);
		string text = ReadLine();
		if (!text.StartsWith("350"))
		{
			throw new FTP_ClientException(text);
		}
		WriteLine("RNTO " + toPath);
		text = ReadLine();
		if (!text.StartsWith("250"))
		{
			throw new FTP_ClientException(text);
		}
	}

	public void CreateDirectory(string path)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		WriteLine("MKD " + path);
		string text = ReadLine();
		if (!text.StartsWith("257"))
		{
			throw new FTP_ClientException(text);
		}
	}

	public void DeleteDirectory(string path)
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (!IsConnected)
		{
			throw new InvalidOperationException("You must connect first.");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path == "")
		{
			throw new ArgumentException("Argument 'path' value must be specified.");
		}
		WriteLine("RMD " + path);
		string text = ReadLine();
		if (!text.StartsWith("250"))
		{
			throw new FTP_ClientException(text);
		}
	}

	private void SetTransferType(TransferType type)
	{
		switch (type)
		{
		case TransferType.Ascii:
			WriteLine("TYPE A");
			break;
		case TransferType.Binary:
			WriteLine("TYPE I");
			break;
		default:
			throw new ArgumentException("Not supported argument 'type' value '" + type.ToString() + "'.");
		}
		string[] array = ReadResponse();
		if (!array[0].StartsWith("2"))
		{
			throw new FTP_ClientException(array[0]);
		}
	}

	private void Port()
	{
		string[] array = null;
		IPAddress[] hostAddresses = Dns.GetHostAddresses("");
		foreach (IPAddress iPAddress in hostAddresses)
		{
			if (iPAddress.AddressFamily == m_pDataConnection.LocalEndPoint.AddressFamily)
			{
				WriteLine("PORT " + iPAddress.ToString().Replace(".", ",") + "," + (m_pDataConnection.LocalEndPoint.Port >> 8) + "," + (m_pDataConnection.LocalEndPoint.Port & 0xFF));
				array = ReadResponse();
				if (array[0].StartsWith("2"))
				{
					m_pDataConnection.SwitchToActive();
					return;
				}
			}
		}
		throw new FTP_ClientException(array[0]);
	}

	private void Pasv()
	{
		WriteLine("PASV");
		string[] array = ReadResponse();
		if (!array[0].StartsWith("227"))
		{
			throw new FTP_ClientException(array[0]);
		}
		string[] array2 = array[0].Substring(array[0].IndexOf("(") + 1, array[0].IndexOf(")") - array[0].IndexOf("(") - 1).Split(',');
		m_pDataConnection.SwitchToPassive(new IPEndPoint(IPAddress.Parse(array2[0] + "." + array2[1] + "." + array2[2] + "." + array2[3]), (Convert.ToInt32(array2[4]) << 8) | Convert.ToInt32(array2[5])));
	}

	private string[] ReadResponse()
	{
		List<string> list = new List<string>();
		string text = ReadLine();
		if (text == null)
		{
			throw new Exception("Remote host disconnected connection unexpectedly.");
		}
		if (text.Length >= 4 && text[3] == '-')
		{
			string text2 = text.Substring(0, 3);
			list.Add(text);
			while (true)
			{
				text = ReadLine();
				if (text == null)
				{
					throw new Exception("Remote host disconnected connection unexpectedly.");
				}
				if (text.StartsWith(text2 + " "))
				{
					break;
				}
				list.Add(text);
			}
			list.Add(text);
		}
		else
		{
			list.Add(text);
		}
		return list.ToArray();
	}

	protected override void OnConnected()
	{
		m_pDataConnection = new DataConnection(this);
		string text = ReadLine();
		if (text.StartsWith("220"))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(text.Substring(4));
			while (text.StartsWith("220-"))
			{
				text = ReadLine();
				stringBuilder.AppendLine(text.Substring(4));
			}
			m_GreetingText = stringBuilder.ToString();
			WriteLine("FEAT");
			text = ReadLine();
			m_pExtCapabilities = new List<string>();
			if (text.StartsWith("211"))
			{
				text = ReadLine();
				while (text.StartsWith(" "))
				{
					m_pExtCapabilities.Add(text.Trim());
					text = ReadLine();
				}
			}
			return;
		}
		throw new FTP_ClientException(text);
	}
}
