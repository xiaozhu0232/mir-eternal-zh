using System;

namespace Org.BouncyCastle.X509.Store;

public class X509CertPairStoreSelector : IX509Selector, ICloneable
{
	private X509CertificatePair certPair;

	private X509CertStoreSelector forwardSelector;

	private X509CertStoreSelector reverseSelector;

	public X509CertificatePair CertPair
	{
		get
		{
			return certPair;
		}
		set
		{
			certPair = value;
		}
	}

	public X509CertStoreSelector ForwardSelector
	{
		get
		{
			return CloneSelector(forwardSelector);
		}
		set
		{
			forwardSelector = CloneSelector(value);
		}
	}

	public X509CertStoreSelector ReverseSelector
	{
		get
		{
			return CloneSelector(reverseSelector);
		}
		set
		{
			reverseSelector = CloneSelector(value);
		}
	}

	private static X509CertStoreSelector CloneSelector(X509CertStoreSelector s)
	{
		if (s != null)
		{
			return (X509CertStoreSelector)s.Clone();
		}
		return null;
	}

	public X509CertPairStoreSelector()
	{
	}

	private X509CertPairStoreSelector(X509CertPairStoreSelector o)
	{
		certPair = o.CertPair;
		forwardSelector = o.ForwardSelector;
		reverseSelector = o.ReverseSelector;
	}

	public bool Match(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (!(obj is X509CertificatePair x509CertificatePair))
		{
			return false;
		}
		if (certPair != null && !certPair.Equals(x509CertificatePair))
		{
			return false;
		}
		if (forwardSelector != null && !forwardSelector.Match(x509CertificatePair.Forward))
		{
			return false;
		}
		if (reverseSelector != null && !reverseSelector.Match(x509CertificatePair.Reverse))
		{
			return false;
		}
		return true;
	}

	public object Clone()
	{
		return new X509CertPairStoreSelector(this);
	}
}
