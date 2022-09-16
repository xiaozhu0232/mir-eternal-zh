using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls;

public class CertificateStatusRequest
{
	protected readonly byte mStatusType;

	protected readonly object mRequest;

	public virtual byte StatusType => mStatusType;

	public virtual object Request => mRequest;

	public CertificateStatusRequest(byte statusType, object request)
	{
		if (!IsCorrectType(statusType, request))
		{
			throw new ArgumentException("not an instance of the correct type", "request");
		}
		mStatusType = statusType;
		mRequest = request;
	}

	public virtual OcspStatusRequest GetOcspStatusRequest()
	{
		if (!IsCorrectType(1, mRequest))
		{
			throw new InvalidOperationException("'request' is not an OCSPStatusRequest");
		}
		return (OcspStatusRequest)mRequest;
	}

	public virtual void Encode(Stream output)
	{
		TlsUtilities.WriteUint8(mStatusType, output);
		byte b = mStatusType;
		if (b == 1)
		{
			((OcspStatusRequest)mRequest).Encode(output);
			return;
		}
		throw new TlsFatalAlert(80);
	}

	public static CertificateStatusRequest Parse(Stream input)
	{
		byte b = TlsUtilities.ReadUint8(input);
		byte b2 = b;
		if (b2 == 1)
		{
			object request = OcspStatusRequest.Parse(input);
			return new CertificateStatusRequest(b, request);
		}
		throw new TlsFatalAlert(50);
	}

	protected static bool IsCorrectType(byte statusType, object request)
	{
		byte b = statusType;
		if (b == 1)
		{
			return request is OcspStatusRequest;
		}
		throw new ArgumentException("unsupported CertificateStatusType", "statusType");
	}
}
