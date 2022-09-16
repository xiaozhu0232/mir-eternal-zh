using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.X509;

public class X509AttrCertParser
{
	private static readonly PemParser PemAttrCertParser = new PemParser("ATTRIBUTE CERTIFICATE");

	private Asn1Set sData;

	private int sDataObjectCount;

	private Stream currentStream;

	private IX509AttributeCertificate ReadDerCertificate(Asn1InputStream dIn)
	{
		Asn1Sequence asn1Sequence = (Asn1Sequence)dIn.ReadObject();
		if (asn1Sequence.Count > 1 && asn1Sequence[0] is DerObjectIdentifier && asn1Sequence[0].Equals(PkcsObjectIdentifiers.SignedData))
		{
			sData = SignedData.GetInstance(Asn1Sequence.GetInstance((Asn1TaggedObject)asn1Sequence[1], explicitly: true)).Certificates;
			return GetCertificate();
		}
		return new X509V2AttributeCertificate(AttributeCertificate.GetInstance(asn1Sequence));
	}

	private IX509AttributeCertificate GetCertificate()
	{
		if (sData != null)
		{
			while (sDataObjectCount < sData.Count)
			{
				object obj = sData[sDataObjectCount++];
				if (obj is Asn1TaggedObject && ((Asn1TaggedObject)obj).TagNo == 2)
				{
					return new X509V2AttributeCertificate(AttributeCertificate.GetInstance(Asn1Sequence.GetInstance((Asn1TaggedObject)obj, explicitly: false)));
				}
			}
		}
		return null;
	}

	private IX509AttributeCertificate ReadPemCertificate(Stream inStream)
	{
		Asn1Sequence asn1Sequence = PemAttrCertParser.ReadPemObject(inStream);
		if (asn1Sequence != null)
		{
			return new X509V2AttributeCertificate(AttributeCertificate.GetInstance(asn1Sequence));
		}
		return null;
	}

	public IX509AttributeCertificate ReadAttrCert(byte[] input)
	{
		return ReadAttrCert(new MemoryStream(input, writable: false));
	}

	public ICollection ReadAttrCerts(byte[] input)
	{
		return ReadAttrCerts(new MemoryStream(input, writable: false));
	}

	public IX509AttributeCertificate ReadAttrCert(Stream inStream)
	{
		if (inStream == null)
		{
			throw new ArgumentNullException("inStream");
		}
		if (!inStream.CanRead)
		{
			throw new ArgumentException("inStream must be read-able", "inStream");
		}
		if (currentStream == null)
		{
			currentStream = inStream;
			sData = null;
			sDataObjectCount = 0;
		}
		else if (currentStream != inStream)
		{
			currentStream = inStream;
			sData = null;
			sDataObjectCount = 0;
		}
		try
		{
			if (sData != null)
			{
				if (sDataObjectCount != sData.Count)
				{
					return GetCertificate();
				}
				sData = null;
				sDataObjectCount = 0;
				return null;
			}
			PushbackStream pushbackStream = new PushbackStream(inStream);
			int num = pushbackStream.ReadByte();
			if (num < 0)
			{
				return null;
			}
			pushbackStream.Unread(num);
			if (num != 48)
			{
				return ReadPemCertificate(pushbackStream);
			}
			return ReadDerCertificate(new Asn1InputStream(pushbackStream));
		}
		catch (Exception ex)
		{
			throw new CertificateException(ex.ToString());
		}
	}

	public ICollection ReadAttrCerts(Stream inStream)
	{
		IList list = Platform.CreateArrayList();
		IX509AttributeCertificate value;
		while ((value = ReadAttrCert(inStream)) != null)
		{
			list.Add(value);
		}
		return list;
	}
}
