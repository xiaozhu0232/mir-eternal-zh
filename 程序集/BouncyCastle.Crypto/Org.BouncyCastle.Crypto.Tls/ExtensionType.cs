using System;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class ExtensionType
{
	public const int server_name = 0;

	public const int max_fragment_length = 1;

	public const int client_certificate_url = 2;

	public const int trusted_ca_keys = 3;

	public const int truncated_hmac = 4;

	public const int status_request = 5;

	public const int user_mapping = 6;

	public const int client_authz = 7;

	public const int server_authz = 8;

	public const int cert_type = 9;

	public const int supported_groups = 10;

	[Obsolete("Use 'supported_groups' instead")]
	public const int elliptic_curves = 10;

	public const int ec_point_formats = 11;

	public const int srp = 12;

	public const int signature_algorithms = 13;

	public const int use_srtp = 14;

	public const int heartbeat = 15;

	public const int application_layer_protocol_negotiation = 16;

	public const int status_request_v2 = 17;

	public const int signed_certificate_timestamp = 18;

	public const int client_certificate_type = 19;

	public const int server_certificate_type = 20;

	public const int padding = 21;

	public const int encrypt_then_mac = 22;

	public const int extended_master_secret = 23;

	public const int cached_info = 25;

	public const int session_ticket = 35;

	public const int renegotiation_info = 65281;

	public static readonly int DRAFT_token_binding = 24;

	public static readonly int negotiated_ff_dhe_groups = 101;
}
