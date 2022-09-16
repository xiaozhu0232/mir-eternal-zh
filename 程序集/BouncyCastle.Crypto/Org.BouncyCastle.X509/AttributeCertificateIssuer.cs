using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.X509;

public class AttributeCertificateIssuer : IX509Selector, ICloneable
{
	internal readonly Asn1Encodable form;

	public AttributeCertificateIssuer(AttCertIssuer issuer)
	{
		form = issuer.Issuer;
	}

	public AttributeCertificateIssuer(X509Name principal)
	{
		form = new V2Form(new GeneralNames(new GeneralName(principal)));
	}

	private object[] GetNames()
	{
		GeneralNames generalNames = ((!(form is V2Form)) ? ((GeneralNames)form) : ((V2Form)form).IssuerName);
		GeneralName[] names = generalNames.GetNames();
		int num = 0;
		for (int i = 0; i != names.Length; i++)
		{
			if (names[i].TagNo == 4)
			{
				num++;
			}
		}
		object[] array = new object[num];
		int num2 = 0;
		for (int j = 0; j != names.Length; j++)
		{
			if (names[j].TagNo == 4)
			{
				array[num2++] = X509Name.GetInstance(names[j].Name);
			}
		}
		return array;
	}

	public X509Name[] GetPrincipals()
	{
		object[] names = GetNames();
		int num = 0;
		for (int i = 0; i != names.Length; i++)
		{
			if (names[i] is X509Name)
			{
				num++;
			}
		}
		X509Name[] array = new X509Name[num];
		int num2 = 0;
		for (int j = 0; j != names.Length; j++)
		{
			if (names[j] is X509Name)
			{
				array[num2++] = (X509Name)names[j];
			}
		}
		return array;
	}

	private bool MatchesDN(X509Name subject, GeneralNames targets)
	{
		GeneralName[] names = targets.GetNames();
		for (int i = 0; i != names.Length; i++)
		{
			GeneralName generalName = names[i];
			if (generalName.TagNo != 4)
			{
				continue;
			}
			try
			{
				if (X509Name.GetInstance(generalName.Name).Equivalent(subject))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
		}
		return false;
	}

	public object Clone()
	{
		return new AttributeCertificateIssuer(AttCertIssuer.GetInstance(form));
	}

	public bool Match(X509Certificate x509Cert)
	{
		if (form is V2Form)
		{
			V2Form v2Form = (V2Form)form;
			if (v2Form.BaseCertificateID != null)
			{
				if (v2Form.BaseCertificateID.Serial.Value.Equals(x509Cert.SerialNumber))
				{
					return MatchesDN(x509Cert.IssuerDN, v2Form.BaseCertificateID.Issuer);
				}
				return false;
			}
			return MatchesDN(x509Cert.SubjectDN, v2Form.IssuerName);
		}
		return MatchesDN(x509Cert.SubjectDN, (GeneralNames)form);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is AttributeCertificateIssuer))
		{
			return false;
		}
		AttributeCertificateIssuer attributeCertificateIssuer = (AttributeCertificateIssuer)obj;
		return form.Equals(attributeCertificateIssuer.form);
	}

	public override int GetHashCode()
	{
		return form.GetHashCode();
	}

	public bool Match(object obj)
	{
		if (!(obj is X509Certificate))
		{
			return false;
		}
		return Match((X509Certificate)obj);
	}
}
