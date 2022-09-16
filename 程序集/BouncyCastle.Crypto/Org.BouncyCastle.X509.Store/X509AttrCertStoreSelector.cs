using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509.Store;

public class X509AttrCertStoreSelector : IX509Selector, ICloneable
{
	private IX509AttributeCertificate attributeCert;

	private DateTimeObject attributeCertificateValid;

	private AttributeCertificateHolder holder;

	private AttributeCertificateIssuer issuer;

	private BigInteger serialNumber;

	private ISet targetNames = new HashSet();

	private ISet targetGroups = new HashSet();

	public IX509AttributeCertificate AttributeCert
	{
		get
		{
			return attributeCert;
		}
		set
		{
			attributeCert = value;
		}
	}

	[Obsolete("Use AttributeCertificateValid instead")]
	public DateTimeObject AttribueCertificateValid
	{
		get
		{
			return attributeCertificateValid;
		}
		set
		{
			attributeCertificateValid = value;
		}
	}

	public DateTimeObject AttributeCertificateValid
	{
		get
		{
			return attributeCertificateValid;
		}
		set
		{
			attributeCertificateValid = value;
		}
	}

	public AttributeCertificateHolder Holder
	{
		get
		{
			return holder;
		}
		set
		{
			holder = value;
		}
	}

	public AttributeCertificateIssuer Issuer
	{
		get
		{
			return issuer;
		}
		set
		{
			issuer = value;
		}
	}

	public BigInteger SerialNumber
	{
		get
		{
			return serialNumber;
		}
		set
		{
			serialNumber = value;
		}
	}

	public X509AttrCertStoreSelector()
	{
	}

	private X509AttrCertStoreSelector(X509AttrCertStoreSelector o)
	{
		attributeCert = o.attributeCert;
		attributeCertificateValid = o.attributeCertificateValid;
		holder = o.holder;
		issuer = o.issuer;
		serialNumber = o.serialNumber;
		targetGroups = new HashSet(o.targetGroups);
		targetNames = new HashSet(o.targetNames);
	}

	public bool Match(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (!(obj is IX509AttributeCertificate iX509AttributeCertificate))
		{
			return false;
		}
		if (attributeCert != null && !attributeCert.Equals(iX509AttributeCertificate))
		{
			return false;
		}
		if (serialNumber != null && !iX509AttributeCertificate.SerialNumber.Equals(serialNumber))
		{
			return false;
		}
		if (holder != null && !iX509AttributeCertificate.Holder.Equals(holder))
		{
			return false;
		}
		if (issuer != null && !iX509AttributeCertificate.Issuer.Equals(issuer))
		{
			return false;
		}
		if (attributeCertificateValid != null && !iX509AttributeCertificate.IsValid(attributeCertificateValid.Value))
		{
			return false;
		}
		if (targetNames.Count > 0 || targetGroups.Count > 0)
		{
			Asn1OctetString extensionValue = iX509AttributeCertificate.GetExtensionValue(X509Extensions.TargetInformation);
			if (extensionValue != null)
			{
				TargetInformation instance;
				try
				{
					instance = TargetInformation.GetInstance(X509ExtensionUtilities.FromExtensionValue(extensionValue));
				}
				catch (Exception)
				{
					return false;
				}
				Targets[] targetsObjects = instance.GetTargetsObjects();
				if (targetNames.Count > 0)
				{
					bool flag = false;
					for (int i = 0; i < targetsObjects.Length; i++)
					{
						if (flag)
						{
							break;
						}
						Target[] targets = targetsObjects[i].GetTargets();
						for (int j = 0; j < targets.Length; j++)
						{
							GeneralName targetName = targets[j].TargetName;
							if (targetName != null && targetNames.Contains(targetName))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				if (targetGroups.Count > 0)
				{
					bool flag2 = false;
					for (int k = 0; k < targetsObjects.Length; k++)
					{
						if (flag2)
						{
							break;
						}
						Target[] targets2 = targetsObjects[k].GetTargets();
						for (int l = 0; l < targets2.Length; l++)
						{
							GeneralName targetGroup = targets2[l].TargetGroup;
							if (targetGroup != null && targetGroups.Contains(targetGroup))
							{
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public object Clone()
	{
		return new X509AttrCertStoreSelector(this);
	}

	public void AddTargetName(GeneralName name)
	{
		targetNames.Add(name);
	}

	public void AddTargetName(byte[] name)
	{
		AddTargetName(GeneralName.GetInstance(Asn1Object.FromByteArray(name)));
	}

	public void SetTargetNames(IEnumerable names)
	{
		targetNames = ExtractGeneralNames(names);
	}

	public IEnumerable GetTargetNames()
	{
		return new EnumerableProxy(targetNames);
	}

	public void AddTargetGroup(GeneralName group)
	{
		targetGroups.Add(group);
	}

	public void AddTargetGroup(byte[] name)
	{
		AddTargetGroup(GeneralName.GetInstance(Asn1Object.FromByteArray(name)));
	}

	public void SetTargetGroups(IEnumerable names)
	{
		targetGroups = ExtractGeneralNames(names);
	}

	public IEnumerable GetTargetGroups()
	{
		return new EnumerableProxy(targetGroups);
	}

	private ISet ExtractGeneralNames(IEnumerable names)
	{
		ISet set = new HashSet();
		if (names != null)
		{
			foreach (object name in names)
			{
				if (name is GeneralName)
				{
					set.Add(name);
				}
				else
				{
					set.Add(GeneralName.GetInstance(Asn1Object.FromByteArray((byte[])name)));
				}
			}
			return set;
		}
		return set;
	}
}
