using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class TlsExtensionsUtilities
{
	public static IDictionary EnsureExtensionsInitialised(IDictionary extensions)
	{
		if (extensions != null)
		{
			return extensions;
		}
		return Platform.CreateHashtable();
	}

	public static void AddClientCertificateTypeExtensionClient(IDictionary extensions, byte[] certificateTypes)
	{
		extensions[19] = CreateCertificateTypeExtensionClient(certificateTypes);
	}

	public static void AddClientCertificateTypeExtensionServer(IDictionary extensions, byte certificateType)
	{
		extensions[19] = CreateCertificateTypeExtensionServer(certificateType);
	}

	public static void AddEncryptThenMacExtension(IDictionary extensions)
	{
		extensions[22] = CreateEncryptThenMacExtension();
	}

	public static void AddExtendedMasterSecretExtension(IDictionary extensions)
	{
		extensions[23] = CreateExtendedMasterSecretExtension();
	}

	public static void AddHeartbeatExtension(IDictionary extensions, HeartbeatExtension heartbeatExtension)
	{
		extensions[15] = CreateHeartbeatExtension(heartbeatExtension);
	}

	public static void AddMaxFragmentLengthExtension(IDictionary extensions, byte maxFragmentLength)
	{
		extensions[1] = CreateMaxFragmentLengthExtension(maxFragmentLength);
	}

	public static void AddPaddingExtension(IDictionary extensions, int dataLength)
	{
		extensions[21] = CreatePaddingExtension(dataLength);
	}

	public static void AddServerCertificateTypeExtensionClient(IDictionary extensions, byte[] certificateTypes)
	{
		extensions[20] = CreateCertificateTypeExtensionClient(certificateTypes);
	}

	public static void AddServerCertificateTypeExtensionServer(IDictionary extensions, byte certificateType)
	{
		extensions[20] = CreateCertificateTypeExtensionServer(certificateType);
	}

	public static void AddServerNameExtension(IDictionary extensions, ServerNameList serverNameList)
	{
		extensions[0] = CreateServerNameExtension(serverNameList);
	}

	public static void AddStatusRequestExtension(IDictionary extensions, CertificateStatusRequest statusRequest)
	{
		extensions[5] = CreateStatusRequestExtension(statusRequest);
	}

	public static void AddTruncatedHMacExtension(IDictionary extensions)
	{
		extensions[4] = CreateTruncatedHMacExtension();
	}

	public static byte[] GetClientCertificateTypeExtensionClient(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 19);
		if (extensionData != null)
		{
			return ReadCertificateTypeExtensionClient(extensionData);
		}
		return null;
	}

	public static short GetClientCertificateTypeExtensionServer(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 19);
		if (extensionData != null)
		{
			return ReadCertificateTypeExtensionServer(extensionData);
		}
		return -1;
	}

	public static HeartbeatExtension GetHeartbeatExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 15);
		if (extensionData != null)
		{
			return ReadHeartbeatExtension(extensionData);
		}
		return null;
	}

	public static short GetMaxFragmentLengthExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 1);
		if (extensionData != null)
		{
			return ReadMaxFragmentLengthExtension(extensionData);
		}
		return -1;
	}

	public static int GetPaddingExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 21);
		if (extensionData != null)
		{
			return ReadPaddingExtension(extensionData);
		}
		return -1;
	}

	public static byte[] GetServerCertificateTypeExtensionClient(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 20);
		if (extensionData != null)
		{
			return ReadCertificateTypeExtensionClient(extensionData);
		}
		return null;
	}

	public static short GetServerCertificateTypeExtensionServer(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 20);
		if (extensionData != null)
		{
			return ReadCertificateTypeExtensionServer(extensionData);
		}
		return -1;
	}

	public static ServerNameList GetServerNameExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 0);
		if (extensionData != null)
		{
			return ReadServerNameExtension(extensionData);
		}
		return null;
	}

	public static CertificateStatusRequest GetStatusRequestExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 5);
		if (extensionData != null)
		{
			return ReadStatusRequestExtension(extensionData);
		}
		return null;
	}

	public static bool HasEncryptThenMacExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 22);
		if (extensionData != null)
		{
			return ReadEncryptThenMacExtension(extensionData);
		}
		return false;
	}

	public static bool HasExtendedMasterSecretExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 23);
		if (extensionData != null)
		{
			return ReadExtendedMasterSecretExtension(extensionData);
		}
		return false;
	}

	public static bool HasTruncatedHMacExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 4);
		if (extensionData != null)
		{
			return ReadTruncatedHMacExtension(extensionData);
		}
		return false;
	}

	public static byte[] CreateCertificateTypeExtensionClient(byte[] certificateTypes)
	{
		if (certificateTypes == null || certificateTypes.Length < 1 || certificateTypes.Length > 255)
		{
			throw new TlsFatalAlert(80);
		}
		return TlsUtilities.EncodeUint8ArrayWithUint8Length(certificateTypes);
	}

	public static byte[] CreateCertificateTypeExtensionServer(byte certificateType)
	{
		return TlsUtilities.EncodeUint8(certificateType);
	}

	public static byte[] CreateEmptyExtensionData()
	{
		return TlsUtilities.EmptyBytes;
	}

	public static byte[] CreateEncryptThenMacExtension()
	{
		return CreateEmptyExtensionData();
	}

	public static byte[] CreateExtendedMasterSecretExtension()
	{
		return CreateEmptyExtensionData();
	}

	public static byte[] CreateHeartbeatExtension(HeartbeatExtension heartbeatExtension)
	{
		if (heartbeatExtension == null)
		{
			throw new TlsFatalAlert(80);
		}
		MemoryStream memoryStream = new MemoryStream();
		heartbeatExtension.Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public static byte[] CreateMaxFragmentLengthExtension(byte maxFragmentLength)
	{
		return TlsUtilities.EncodeUint8(maxFragmentLength);
	}

	public static byte[] CreatePaddingExtension(int dataLength)
	{
		TlsUtilities.CheckUint16(dataLength);
		return new byte[dataLength];
	}

	public static byte[] CreateServerNameExtension(ServerNameList serverNameList)
	{
		if (serverNameList == null)
		{
			throw new TlsFatalAlert(80);
		}
		MemoryStream memoryStream = new MemoryStream();
		serverNameList.Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public static byte[] CreateStatusRequestExtension(CertificateStatusRequest statusRequest)
	{
		if (statusRequest == null)
		{
			throw new TlsFatalAlert(80);
		}
		MemoryStream memoryStream = new MemoryStream();
		statusRequest.Encode(memoryStream);
		return memoryStream.ToArray();
	}

	public static byte[] CreateTruncatedHMacExtension()
	{
		return CreateEmptyExtensionData();
	}

	private static bool ReadEmptyExtensionData(byte[] extensionData)
	{
		if (extensionData == null)
		{
			throw new ArgumentNullException("extensionData");
		}
		if (extensionData.Length != 0)
		{
			throw new TlsFatalAlert(47);
		}
		return true;
	}

	public static byte[] ReadCertificateTypeExtensionClient(byte[] extensionData)
	{
		byte[] array = TlsUtilities.DecodeUint8ArrayWithUint8Length(extensionData);
		if (array.Length < 1)
		{
			throw new TlsFatalAlert(50);
		}
		return array;
	}

	public static byte ReadCertificateTypeExtensionServer(byte[] extensionData)
	{
		return TlsUtilities.DecodeUint8(extensionData);
	}

	public static bool ReadEncryptThenMacExtension(byte[] extensionData)
	{
		return ReadEmptyExtensionData(extensionData);
	}

	public static bool ReadExtendedMasterSecretExtension(byte[] extensionData)
	{
		return ReadEmptyExtensionData(extensionData);
	}

	public static HeartbeatExtension ReadHeartbeatExtension(byte[] extensionData)
	{
		if (extensionData == null)
		{
			throw new ArgumentNullException("extensionData");
		}
		MemoryStream memoryStream = new MemoryStream(extensionData, writable: false);
		HeartbeatExtension result = HeartbeatExtension.Parse(memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		return result;
	}

	public static byte ReadMaxFragmentLengthExtension(byte[] extensionData)
	{
		return TlsUtilities.DecodeUint8(extensionData);
	}

	public static int ReadPaddingExtension(byte[] extensionData)
	{
		if (extensionData == null)
		{
			throw new ArgumentNullException("extensionData");
		}
		for (int i = 0; i < extensionData.Length; i++)
		{
			if (extensionData[i] != 0)
			{
				throw new TlsFatalAlert(47);
			}
		}
		return extensionData.Length;
	}

	public static ServerNameList ReadServerNameExtension(byte[] extensionData)
	{
		if (extensionData == null)
		{
			throw new ArgumentNullException("extensionData");
		}
		MemoryStream memoryStream = new MemoryStream(extensionData, writable: false);
		ServerNameList result = ServerNameList.Parse(memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		return result;
	}

	public static CertificateStatusRequest ReadStatusRequestExtension(byte[] extensionData)
	{
		if (extensionData == null)
		{
			throw new ArgumentNullException("extensionData");
		}
		MemoryStream memoryStream = new MemoryStream(extensionData, writable: false);
		CertificateStatusRequest result = CertificateStatusRequest.Parse(memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		return result;
	}

	public static bool ReadTruncatedHMacExtension(byte[] extensionData)
	{
		return ReadEmptyExtensionData(extensionData);
	}
}
