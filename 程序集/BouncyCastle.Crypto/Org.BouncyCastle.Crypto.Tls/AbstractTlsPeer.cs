using System;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class AbstractTlsPeer : TlsPeer
{
	private volatile TlsCloseable mCloseHandle;

	public virtual void Cancel()
	{
		mCloseHandle?.Close();
	}

	public virtual void NotifyCloseHandle(TlsCloseable closeHandle)
	{
		mCloseHandle = closeHandle;
	}

	public virtual int GetHandshakeTimeoutMillis()
	{
		return 0;
	}

	public virtual bool RequiresExtendedMasterSecret()
	{
		return false;
	}

	public virtual bool ShouldUseGmtUnixTime()
	{
		return false;
	}

	public virtual void NotifySecureRenegotiation(bool secureRenegotiation)
	{
		if (!secureRenegotiation)
		{
			throw new TlsFatalAlert(40);
		}
	}

	public abstract TlsCompression GetCompression();

	public abstract TlsCipher GetCipher();

	public virtual void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause)
	{
	}

	public virtual void NotifyAlertReceived(byte alertLevel, byte alertDescription)
	{
	}

	public virtual void NotifyHandshakeComplete()
	{
	}
}
