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

public class X509CertificateParser
{
	private static readonly PemParser PemCertParser = new PemParser("CERTIFICATE");

	private Asn1Set sData;

	private int sDataObjectCount;

	private Stream currentStream;

	private X509Certificate ReadDerCertificate(Asn1InputStream dIn)
	{
		Asn1Sequence asn1Sequence = (Asn1Sequence)dIn.ReadObject();
		if (asn1Sequence.Count > 1 && asn1Sequence[0] is DerObjectIdentifier && asn1Sequence[0].Equals(PkcsObjectIdentifiers.SignedData))
		{
			sData = SignedData.GetInstance(Asn1Sequence.GetInstance((Asn1TaggedObject)asn1Sequence[1], explicitly: true)).Certificates;
			return GetCertificate();
		}
		return CreateX509Certificate(X509CertificateStructure.GetInstance(asn1Sequence));
	}

	private X509Certificate GetCertificate()
	{
		if (sData != null)
		{
			while (sDataObjectCount < sData.Count)
			{
				object obj = sData[sDataObjectCount++];
				if (obj is Asn1Sequence)
				{
					return CreateX509Certificate(X509CertificateStructure.GetInstance(obj));
				}
			}
		}
		return null;
	}

	private X509Certificate ReadPemCertificate(Stream inStream)
	{
		Asn1Sequence asn1Sequence = PemCertParser.ReadPemObject(inStream);
		if (asn1Sequence != null)
		{
			return CreateX509Certificate(X509CertificateStructure.GetInstance(asn1Sequence));
		}
		return null;
	}

	protected virtual X509Certificate CreateX509Certificate(X509CertificateStructure c)
	{
		return new X509Certificate(c);
	}

	public X509Certificate ReadCertificate(byte[] input)
	{
		return ReadCertificate(new MemoryStream(input, writable: false));
	}

	public ICollection ReadCertificates(byte[] input)
	{
		return ReadCertificates(new MemoryStream(input, writable: false));
	}

	public X509Certificate ReadCertificate(Stream inStream)
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
		catch (Exception exception)
		{
			throw new CertificateException("Failed to read certificate", exception);
		}
	}

	public ICollection ReadCertificates(Stream inStream)
	{
		IList list = Platform.CreateArrayList();
		X509Certificate value;
		while ((value = ReadCertificate(inStream)) != null)
		{
			list.Add(value);
		}
		return list;
	}
}
