using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class OcspStatusRequest
{
	protected readonly IList mResponderIDList;

	protected readonly X509Extensions mRequestExtensions;

	public virtual IList ResponderIDList => mResponderIDList;

	public virtual X509Extensions RequestExtensions => mRequestExtensions;

	public OcspStatusRequest(IList responderIDList, X509Extensions requestExtensions)
	{
		mResponderIDList = responderIDList;
		mRequestExtensions = requestExtensions;
	}

	public virtual void Encode(Stream output)
	{
		if (mResponderIDList == null || mResponderIDList.Count < 1)
		{
			TlsUtilities.WriteUint16(0, output);
		}
		else
		{
			MemoryStream memoryStream = new MemoryStream();
			for (int i = 0; i < mResponderIDList.Count; i++)
			{
				ResponderID responderID = (ResponderID)mResponderIDList[i];
				byte[] encoded = responderID.GetEncoded("DER");
				TlsUtilities.WriteOpaque16(encoded, memoryStream);
			}
			TlsUtilities.CheckUint16(memoryStream.Length);
			TlsUtilities.WriteUint16((int)memoryStream.Length, output);
			Streams.WriteBufTo(memoryStream, output);
		}
		if (mRequestExtensions == null)
		{
			TlsUtilities.WriteUint16(0, output);
			return;
		}
		byte[] encoded2 = mRequestExtensions.GetEncoded("DER");
		TlsUtilities.CheckUint16(encoded2.Length);
		TlsUtilities.WriteUint16(encoded2.Length, output);
		output.Write(encoded2, 0, encoded2.Length);
	}

	public static OcspStatusRequest Parse(Stream input)
	{
		IList list = Platform.CreateArrayList();
		int num = TlsUtilities.ReadUint16(input);
		if (num > 0)
		{
			byte[] buffer = TlsUtilities.ReadFully(num, input);
			MemoryStream memoryStream = new MemoryStream(buffer, writable: false);
			do
			{
				byte[] encoding = TlsUtilities.ReadOpaque16(memoryStream);
				ResponderID instance = ResponderID.GetInstance(TlsUtilities.ReadDerObject(encoding));
				list.Add(instance);
			}
			while (memoryStream.Position < memoryStream.Length);
		}
		X509Extensions requestExtensions = null;
		int num2 = TlsUtilities.ReadUint16(input);
		if (num2 > 0)
		{
			byte[] encoding2 = TlsUtilities.ReadFully(num2, input);
			requestExtensions = X509Extensions.GetInstance(TlsUtilities.ReadDerObject(encoding2));
		}
		return new OcspStatusRequest(list, requestExtensions);
	}
}
