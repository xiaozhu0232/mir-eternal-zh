using System;

namespace Org.BouncyCastle.Asn1.Crmf;

public class SubsequentMessage : DerInteger
{
	public static readonly SubsequentMessage encrCert = new SubsequentMessage(0);

	public static readonly SubsequentMessage challengeResp = new SubsequentMessage(1);

	private SubsequentMessage(int value)
		: base(value)
	{
	}

	public static SubsequentMessage ValueOf(int value)
	{
		return value switch
		{
			0 => encrCert, 
			1 => challengeResp, 
			_ => throw new ArgumentException("unknown value: " + value, "value"), 
		};
	}
}
