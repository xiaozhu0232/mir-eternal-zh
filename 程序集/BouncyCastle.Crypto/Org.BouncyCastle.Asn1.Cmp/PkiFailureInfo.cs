namespace Org.BouncyCastle.Asn1.Cmp;

public class PkiFailureInfo : DerBitString
{
	public const int BadAlg = 128;

	public const int BadMessageCheck = 64;

	public const int BadRequest = 32;

	public const int BadTime = 16;

	public const int BadCertId = 8;

	public const int BadDataFormat = 4;

	public const int WrongAuthority = 2;

	public const int IncorrectData = 1;

	public const int MissingTimeStamp = 32768;

	public const int BadPop = 16384;

	public const int CertRevoked = 8192;

	public const int CertConfirmed = 4096;

	public const int WrongIntegrity = 2048;

	public const int BadRecipientNonce = 1024;

	public const int TimeNotAvailable = 512;

	public const int UnacceptedPolicy = 256;

	public const int UnacceptedExtension = 8388608;

	public const int AddInfoNotAvailable = 4194304;

	public const int BadSenderNonce = 2097152;

	public const int BadCertTemplate = 1048576;

	public const int SignerNotTrusted = 524288;

	public const int TransactionIdInUse = 262144;

	public const int UnsupportedVersion = 131072;

	public const int NotAuthorized = 65536;

	public const int SystemUnavail = int.MinValue;

	public const int SystemFailure = 1073741824;

	public const int DuplicateCertReq = 536870912;

	public PkiFailureInfo(int info)
		: base(info)
	{
	}

	public PkiFailureInfo(DerBitString info)
		: base(info.GetBytes(), info.PadBits)
	{
	}

	public override string ToString()
	{
		return "PkiFailureInfo: 0x" + IntValue.ToString("X");
	}
}
