using System;

namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsPeer
{
	void NotifyCloseHandle(TlsCloseable closehandle);

	void Cancel();

	int GetHandshakeTimeoutMillis();

	bool RequiresExtendedMasterSecret();

	bool ShouldUseGmtUnixTime();

	void NotifySecureRenegotiation(bool secureRenegotiation);

	TlsCompression GetCompression();

	TlsCipher GetCipher();

	void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause);

	void NotifyAlertReceived(byte alertLevel, byte alertDescription);

	void NotifyHandshakeComplete();
}
