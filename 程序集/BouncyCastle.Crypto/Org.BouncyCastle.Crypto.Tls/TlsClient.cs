using System.Collections;

namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsClient : TlsPeer
{
	ProtocolVersion ClientHelloRecordLayerVersion { get; }

	ProtocolVersion ClientVersion { get; }

	bool IsFallback { get; }

	void Init(TlsClientContext context);

	TlsSession GetSessionToResume();

	int[] GetCipherSuites();

	byte[] GetCompressionMethods();

	IDictionary GetClientExtensions();

	void NotifyServerVersion(ProtocolVersion selectedVersion);

	void NotifySessionID(byte[] sessionID);

	void NotifySelectedCipherSuite(int selectedCipherSuite);

	void NotifySelectedCompressionMethod(byte selectedCompressionMethod);

	void ProcessServerExtensions(IDictionary serverExtensions);

	void ProcessServerSupplementalData(IList serverSupplementalData);

	TlsKeyExchange GetKeyExchange();

	TlsAuthentication GetAuthentication();

	IList GetClientSupplementalData();

	void NotifyNewSessionTicket(NewSessionTicket newSessionTicket);
}
