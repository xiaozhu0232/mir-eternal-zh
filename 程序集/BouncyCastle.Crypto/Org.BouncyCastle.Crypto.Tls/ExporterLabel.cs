namespace Org.BouncyCastle.Crypto.Tls;

public abstract class ExporterLabel
{
	public const string client_finished = "client finished";

	public const string server_finished = "server finished";

	public const string master_secret = "master secret";

	public const string key_expansion = "key expansion";

	public const string client_EAP_encryption = "client EAP encryption";

	public const string ttls_keying_material = "ttls keying material";

	public const string ttls_challenge = "ttls challenge";

	public const string dtls_srtp = "EXTRACTOR-dtls_srtp";

	public static readonly string extended_master_secret = "extended master secret";
}
