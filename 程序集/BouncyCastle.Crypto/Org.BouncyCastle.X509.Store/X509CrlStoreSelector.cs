using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509.Store;

public class X509CrlStoreSelector : IX509Selector, ICloneable
{
	private X509Certificate certificateChecking;

	private DateTimeObject dateAndTime;

	private ICollection issuers;

	private BigInteger maxCrlNumber;

	private BigInteger minCrlNumber;

	private IX509AttributeCertificate attrCertChecking;

	private bool completeCrlEnabled;

	private bool deltaCrlIndicatorEnabled;

	private byte[] issuingDistributionPoint;

	private bool issuingDistributionPointEnabled;

	private BigInteger maxBaseCrlNumber;

	public X509Certificate CertificateChecking
	{
		get
		{
			return certificateChecking;
		}
		set
		{
			certificateChecking = value;
		}
	}

	public DateTimeObject DateAndTime
	{
		get
		{
			return dateAndTime;
		}
		set
		{
			dateAndTime = value;
		}
	}

	public ICollection Issuers
	{
		get
		{
			return Platform.CreateArrayList(issuers);
		}
		set
		{
			issuers = Platform.CreateArrayList(value);
		}
	}

	public BigInteger MaxCrlNumber
	{
		get
		{
			return maxCrlNumber;
		}
		set
		{
			maxCrlNumber = value;
		}
	}

	public BigInteger MinCrlNumber
	{
		get
		{
			return minCrlNumber;
		}
		set
		{
			minCrlNumber = value;
		}
	}

	public IX509AttributeCertificate AttrCertChecking
	{
		get
		{
			return attrCertChecking;
		}
		set
		{
			attrCertChecking = value;
		}
	}

	public bool CompleteCrlEnabled
	{
		get
		{
			return completeCrlEnabled;
		}
		set
		{
			completeCrlEnabled = value;
		}
	}

	public bool DeltaCrlIndicatorEnabled
	{
		get
		{
			return deltaCrlIndicatorEnabled;
		}
		set
		{
			deltaCrlIndicatorEnabled = value;
		}
	}

	public byte[] IssuingDistributionPoint
	{
		get
		{
			return Arrays.Clone(issuingDistributionPoint);
		}
		set
		{
			issuingDistributionPoint = Arrays.Clone(value);
		}
	}

	public bool IssuingDistributionPointEnabled
	{
		get
		{
			return issuingDistributionPointEnabled;
		}
		set
		{
			issuingDistributionPointEnabled = value;
		}
	}

	public BigInteger MaxBaseCrlNumber
	{
		get
		{
			return maxBaseCrlNumber;
		}
		set
		{
			maxBaseCrlNumber = value;
		}
	}

	public X509CrlStoreSelector()
	{
	}

	public X509CrlStoreSelector(X509CrlStoreSelector o)
	{
		certificateChecking = o.CertificateChecking;
		dateAndTime = o.DateAndTime;
		issuers = o.Issuers;
		maxCrlNumber = o.MaxCrlNumber;
		minCrlNumber = o.MinCrlNumber;
		deltaCrlIndicatorEnabled = o.DeltaCrlIndicatorEnabled;
		completeCrlEnabled = o.CompleteCrlEnabled;
		maxBaseCrlNumber = o.MaxBaseCrlNumber;
		attrCertChecking = o.AttrCertChecking;
		issuingDistributionPointEnabled = o.IssuingDistributionPointEnabled;
		issuingDistributionPoint = o.IssuingDistributionPoint;
	}

	public virtual object Clone()
	{
		return new X509CrlStoreSelector(this);
	}

	public virtual bool Match(object obj)
	{
		if (!(obj is X509Crl x509Crl))
		{
			return false;
		}
		if (dateAndTime != null)
		{
			DateTime value = dateAndTime.Value;
			DateTime thisUpdate = x509Crl.ThisUpdate;
			DateTimeObject nextUpdate = x509Crl.NextUpdate;
			if (value.CompareTo((object)thisUpdate) < 0 || nextUpdate == null || value.CompareTo((object)nextUpdate.Value) >= 0)
			{
				return false;
			}
		}
		if (issuers != null)
		{
			X509Name issuerDN = x509Crl.IssuerDN;
			bool flag = false;
			foreach (X509Name issuer in issuers)
			{
				if (issuer.Equivalent(issuerDN, inOrder: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (maxCrlNumber != null || minCrlNumber != null)
		{
			Asn1OctetString extensionValue = x509Crl.GetExtensionValue(X509Extensions.CrlNumber);
			if (extensionValue == null)
			{
				return false;
			}
			BigInteger positiveValue = DerInteger.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue)).PositiveValue;
			if (maxCrlNumber != null && positiveValue.CompareTo(maxCrlNumber) > 0)
			{
				return false;
			}
			if (minCrlNumber != null && positiveValue.CompareTo(minCrlNumber) < 0)
			{
				return false;
			}
		}
		DerInteger derInteger = null;
		try
		{
			Asn1OctetString extensionValue2 = x509Crl.GetExtensionValue(X509Extensions.DeltaCrlIndicator);
			if (extensionValue2 != null)
			{
				derInteger = DerInteger.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue2));
			}
		}
		catch (Exception)
		{
			return false;
		}
		if (derInteger == null)
		{
			if (DeltaCrlIndicatorEnabled)
			{
				return false;
			}
		}
		else
		{
			if (CompleteCrlEnabled)
			{
				return false;
			}
			if (maxBaseCrlNumber != null && derInteger.PositiveValue.CompareTo(maxBaseCrlNumber) > 0)
			{
				return false;
			}
		}
		if (issuingDistributionPointEnabled)
		{
			Asn1OctetString extensionValue3 = x509Crl.GetExtensionValue(X509Extensions.IssuingDistributionPoint);
			if (issuingDistributionPoint == null)
			{
				if (extensionValue3 != null)
				{
					return false;
				}
			}
			else if (!Arrays.AreEqual(extensionValue3.GetOctets(), issuingDistributionPoint))
			{
				return false;
			}
		}
		return true;
	}
}
