using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.X509.Extension;

public class X509ExtensionUtilities
{
	public static Asn1Object FromExtensionValue(Asn1OctetString extensionValue)
	{
		return Asn1Object.FromByteArray(extensionValue.GetOctets());
	}

	public static ICollection GetIssuerAlternativeNames(X509Certificate cert)
	{
		Asn1OctetString extensionValue = cert.GetExtensionValue(X509Extensions.IssuerAlternativeName);
		return GetAlternativeName(extensionValue);
	}

	public static ICollection GetSubjectAlternativeNames(X509Certificate cert)
	{
		Asn1OctetString extensionValue = cert.GetExtensionValue(X509Extensions.SubjectAlternativeName);
		return GetAlternativeName(extensionValue);
	}

	private static ICollection GetAlternativeName(Asn1OctetString extVal)
	{
		IList list = Platform.CreateArrayList();
		if (extVal != null)
		{
			try
			{
				Asn1Sequence instance = Asn1Sequence.GetInstance(FromExtensionValue(extVal));
				foreach (Asn1Encodable item in instance)
				{
					IList list2 = Platform.CreateArrayList();
					GeneralName instance2 = GeneralName.GetInstance(item);
					list2.Add(instance2.TagNo);
					switch (instance2.TagNo)
					{
					case 0:
					case 3:
					case 5:
						list2.Add(instance2.Name.ToAsn1Object());
						break;
					case 4:
						list2.Add(X509Name.GetInstance(instance2.Name).ToString());
						break;
					case 1:
					case 2:
					case 6:
						list2.Add(((IAsn1String)instance2.Name).GetString());
						break;
					case 8:
						list2.Add(DerObjectIdentifier.GetInstance(instance2.Name).Id);
						break;
					case 7:
						list2.Add(Asn1OctetString.GetInstance(instance2.Name).GetOctets());
						break;
					default:
						throw new IOException("Bad tag number: " + instance2.TagNo);
					}
					list.Add(list2);
				}
				return list;
			}
			catch (Exception ex)
			{
				throw new CertificateParsingException(ex.Message);
			}
		}
		return list;
	}
}
