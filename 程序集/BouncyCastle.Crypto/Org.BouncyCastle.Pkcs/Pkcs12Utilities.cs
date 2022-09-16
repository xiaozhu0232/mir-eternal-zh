using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Pkcs;

public class Pkcs12Utilities
{
	public static byte[] ConvertToDefiniteLength(byte[] berPkcs12File)
	{
		Pfx pfx = new Pfx(Asn1Sequence.GetInstance(Asn1Object.FromByteArray(berPkcs12File)));
		return pfx.GetEncoded("DER");
	}

	public static byte[] ConvertToDefiniteLength(byte[] berPkcs12File, char[] passwd)
	{
		Pfx pfx = new Pfx(Asn1Sequence.GetInstance(Asn1Object.FromByteArray(berPkcs12File)));
		ContentInfo authSafe = pfx.AuthSafe;
		Asn1OctetString instance = Asn1OctetString.GetInstance(authSafe.Content);
		Asn1Object asn1Object = Asn1Object.FromByteArray(instance.GetOctets());
		authSafe = new ContentInfo(authSafe.ContentType, new DerOctetString(asn1Object.GetEncoded("DER")));
		MacData macData = pfx.MacData;
		try
		{
			int intValue = macData.IterationCount.IntValue;
			byte[] octets = Asn1OctetString.GetInstance(authSafe.Content).GetOctets();
			byte[] digest = Pkcs12Store.CalculatePbeMac(macData.Mac.AlgorithmID.Algorithm, macData.GetSalt(), intValue, passwd, wrongPkcs12Zero: false, octets);
			AlgorithmIdentifier algID = new AlgorithmIdentifier(macData.Mac.AlgorithmID.Algorithm, DerNull.Instance);
			DigestInfo digInfo = new DigestInfo(algID, digest);
			macData = new MacData(digInfo, macData.GetSalt(), intValue);
		}
		catch (Exception ex)
		{
			throw new IOException("error constructing MAC: " + ex.ToString());
		}
		pfx = new Pfx(authSafe, macData);
		return pfx.GetEncoded("DER");
	}
}
