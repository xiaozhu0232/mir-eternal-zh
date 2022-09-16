using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.IsisMtt;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix;

public class PkixCertPathValidatorUtilities
{
	private static readonly PkixCrlUtilities CrlUtilities = new PkixCrlUtilities();

	internal static readonly string ANY_POLICY = "2.5.29.32.0";

	internal static readonly string CRL_NUMBER = X509Extensions.CrlNumber.Id;

	internal static readonly int KEY_CERT_SIGN = 5;

	internal static readonly int CRL_SIGN = 6;

	internal static readonly string[] crlReasons = new string[11]
	{
		"unspecified", "keyCompromise", "cACompromise", "affiliationChanged", "superseded", "cessationOfOperation", "certificateHold", "unknown", "removeFromCRL", "privilegeWithdrawn",
		"aACompromise"
	};

	internal static TrustAnchor FindTrustAnchor(X509Certificate cert, ISet trustAnchors)
	{
		IEnumerator enumerator = trustAnchors.GetEnumerator();
		TrustAnchor trustAnchor = null;
		AsymmetricKeyParameter asymmetricKeyParameter = null;
		Exception ex = null;
		X509CertStoreSelector x509CertStoreSelector = new X509CertStoreSelector();
		try
		{
			x509CertStoreSelector.Subject = GetIssuerPrincipal(cert);
		}
		catch (IOException innerException)
		{
			throw new Exception("Cannot set subject search criteria for trust anchor.", innerException);
		}
		while (enumerator.MoveNext() && trustAnchor == null)
		{
			trustAnchor = (TrustAnchor)enumerator.Current;
			if (trustAnchor.TrustedCert != null)
			{
				if (x509CertStoreSelector.Match(trustAnchor.TrustedCert))
				{
					asymmetricKeyParameter = trustAnchor.TrustedCert.GetPublicKey();
				}
				else
				{
					trustAnchor = null;
				}
			}
			else if (trustAnchor.CAName != null && trustAnchor.CAPublicKey != null)
			{
				try
				{
					X509Name issuerPrincipal = GetIssuerPrincipal(cert);
					X509Name other = new X509Name(trustAnchor.CAName);
					if (issuerPrincipal.Equivalent(other, inOrder: true))
					{
						asymmetricKeyParameter = trustAnchor.CAPublicKey;
					}
					else
					{
						trustAnchor = null;
					}
				}
				catch (InvalidParameterException)
				{
					trustAnchor = null;
				}
			}
			else
			{
				trustAnchor = null;
			}
			if (asymmetricKeyParameter != null)
			{
				try
				{
					cert.Verify(asymmetricKeyParameter);
				}
				catch (Exception ex3)
				{
					ex = ex3;
					trustAnchor = null;
				}
			}
		}
		if (trustAnchor == null && ex != null)
		{
			throw new Exception("TrustAnchor found but certificate validation failed.", ex);
		}
		return trustAnchor;
	}

	internal static bool IsIssuerTrustAnchor(X509Certificate cert, ISet trustAnchors)
	{
		try
		{
			return FindTrustAnchor(cert, trustAnchors) != null;
		}
		catch (Exception)
		{
			return false;
		}
	}

	internal static void AddAdditionalStoresFromAltNames(X509Certificate cert, PkixParameters pkixParams)
	{
		if (cert.GetIssuerAlternativeNames() == null)
		{
			return;
		}
		IEnumerator enumerator = cert.GetIssuerAlternativeNames().GetEnumerator();
		while (enumerator.MoveNext())
		{
			IList list = (IList)enumerator.Current;
			if (list[0].Equals(6))
			{
				string location = (string)list[1];
				AddAdditionalStoreFromLocation(location, pkixParams);
			}
		}
	}

	internal static DateTime GetValidDate(PkixParameters paramsPKIX)
	{
		return paramsPKIX.Date?.Value ?? DateTime.UtcNow;
	}

	internal static X509Name GetIssuerPrincipal(object cert)
	{
		if (cert is X509Certificate)
		{
			return ((X509Certificate)cert).IssuerDN;
		}
		return ((IX509AttributeCertificate)cert).Issuer.GetPrincipals()[0];
	}

	internal static bool IsSelfIssued(X509Certificate cert)
	{
		return cert.SubjectDN.Equivalent(cert.IssuerDN, inOrder: true);
	}

	internal static AlgorithmIdentifier GetAlgorithmIdentifier(AsymmetricKeyParameter key)
	{
		try
		{
			SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key);
			return subjectPublicKeyInfo.AlgorithmID;
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Subject public key cannot be decoded.", cause);
		}
	}

	internal static bool IsAnyPolicy(ISet policySet)
	{
		if (policySet != null && !policySet.Contains(ANY_POLICY))
		{
			return policySet.Count == 0;
		}
		return true;
	}

	internal static void AddAdditionalStoreFromLocation(string location, PkixParameters pkixParams)
	{
		if (!pkixParams.IsAdditionalLocationsEnabled)
		{
			return;
		}
		try
		{
			if (Platform.StartsWith(location, "ldap://"))
			{
				location = location.Substring(7);
				int num = location.IndexOf('/');
				if (num != -1)
				{
					_ = "ldap://" + location.Substring(0, num);
				}
				else
				{
					_ = "ldap://" + location;
				}
				throw Platform.CreateNotImplementedException("LDAP cert/CRL stores");
			}
		}
		catch (Exception)
		{
			throw new Exception("Exception adding X.509 stores.");
		}
	}

	private static BigInteger GetSerialNumber(object cert)
	{
		if (cert is X509Certificate)
		{
			return ((X509Certificate)cert).SerialNumber;
		}
		return ((X509V2AttributeCertificate)cert).SerialNumber;
	}

	internal static ISet GetQualifierSet(Asn1Sequence qualifiers)
	{
		ISet set = new HashSet();
		if (qualifiers == null)
		{
			return set;
		}
		foreach (Asn1Encodable qualifier in qualifiers)
		{
			try
			{
				set.Add(PolicyQualifierInfo.GetInstance(qualifier.ToAsn1Object()));
			}
			catch (IOException cause)
			{
				throw new PkixCertPathValidatorException("Policy qualifier info cannot be decoded.", cause);
			}
		}
		return set;
	}

	internal static PkixPolicyNode RemovePolicyNode(PkixPolicyNode validPolicyTree, IList[] policyNodes, PkixPolicyNode _node)
	{
		PkixPolicyNode parent = _node.Parent;
		if (validPolicyTree == null)
		{
			return null;
		}
		if (parent == null)
		{
			for (int i = 0; i < policyNodes.Length; i++)
			{
				policyNodes[i] = Platform.CreateArrayList();
			}
			return null;
		}
		parent.RemoveChild(_node);
		RemovePolicyNodeRecurse(policyNodes, _node);
		return validPolicyTree;
	}

	private static void RemovePolicyNodeRecurse(IList[] policyNodes, PkixPolicyNode _node)
	{
		policyNodes[_node.Depth].Remove(_node);
		if (!_node.HasChildren)
		{
			return;
		}
		foreach (PkixPolicyNode child in _node.Children)
		{
			RemovePolicyNodeRecurse(policyNodes, child);
		}
	}

	internal static void PrepareNextCertB1(int i, IList[] policyNodes, string id_p, IDictionary m_idp, X509Certificate cert)
	{
		bool flag = false;
		IEnumerator enumerator = policyNodes[i].GetEnumerator();
		while (enumerator.MoveNext())
		{
			PkixPolicyNode pkixPolicyNode = (PkixPolicyNode)enumerator.Current;
			if (pkixPolicyNode.ValidPolicy.Equals(id_p))
			{
				flag = true;
				pkixPolicyNode.ExpectedPolicies = (ISet)m_idp[id_p];
				break;
			}
		}
		if (flag)
		{
			return;
		}
		enumerator = policyNodes[i].GetEnumerator();
		while (enumerator.MoveNext())
		{
			PkixPolicyNode pkixPolicyNode2 = (PkixPolicyNode)enumerator.Current;
			if (!ANY_POLICY.Equals(pkixPolicyNode2.ValidPolicy))
			{
				continue;
			}
			ISet policyQualifiers = null;
			Asn1Sequence asn1Sequence = null;
			try
			{
				asn1Sequence = Asn1Sequence.GetInstance(GetExtensionValue(cert, X509Extensions.CertificatePolicies));
			}
			catch (Exception innerException)
			{
				throw new Exception("Certificate policies cannot be decoded.", innerException);
			}
			IEnumerator enumerator2 = asn1Sequence.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				PolicyInformation policyInformation = null;
				try
				{
					policyInformation = PolicyInformation.GetInstance(enumerator2.Current);
				}
				catch (Exception innerException2)
				{
					throw new Exception("Policy information cannot be decoded.", innerException2);
				}
				if (ANY_POLICY.Equals(policyInformation.PolicyIdentifier.Id))
				{
					try
					{
						policyQualifiers = GetQualifierSet(policyInformation.PolicyQualifiers);
					}
					catch (PkixCertPathValidatorException cause)
					{
						throw new PkixCertPathValidatorException("Policy qualifier info set could not be built.", cause);
					}
					break;
				}
			}
			bool critical = false;
			ISet criticalExtensionOids = cert.GetCriticalExtensionOids();
			if (criticalExtensionOids != null)
			{
				critical = criticalExtensionOids.Contains(X509Extensions.CertificatePolicies.Id);
			}
			PkixPolicyNode parent = pkixPolicyNode2.Parent;
			if (ANY_POLICY.Equals(parent.ValidPolicy))
			{
				PkixPolicyNode pkixPolicyNode3 = new PkixPolicyNode(Platform.CreateArrayList(), i, (ISet)m_idp[id_p], parent, policyQualifiers, id_p, critical);
				parent.AddChild(pkixPolicyNode3);
				policyNodes[i].Add(pkixPolicyNode3);
			}
			break;
		}
	}

	internal static PkixPolicyNode PrepareNextCertB2(int i, IList[] policyNodes, string id_p, PkixPolicyNode validPolicyTree)
	{
		int num = 0;
		foreach (PkixPolicyNode item in Platform.CreateArrayList(policyNodes[i]))
		{
			if (item.ValidPolicy.Equals(id_p))
			{
				PkixPolicyNode parent = item.Parent;
				parent.RemoveChild(item);
				policyNodes[i].RemoveAt(num);
				for (int num2 = i - 1; num2 >= 0; num2--)
				{
					IList list = policyNodes[num2];
					for (int j = 0; j < list.Count; j++)
					{
						PkixPolicyNode pkixPolicyNode2 = (PkixPolicyNode)list[j];
						if (!pkixPolicyNode2.HasChildren)
						{
							validPolicyTree = RemovePolicyNode(validPolicyTree, policyNodes, pkixPolicyNode2);
							if (validPolicyTree == null)
							{
								break;
							}
						}
					}
				}
			}
			else
			{
				num++;
			}
		}
		return validPolicyTree;
	}

	internal static void GetCertStatus(DateTime validDate, X509Crl crl, object cert, CertStatus certStatus)
	{
		X509Crl x509Crl = null;
		try
		{
			x509Crl = new X509Crl(CertificateList.GetInstance((Asn1Sequence)Asn1Object.FromByteArray(crl.GetEncoded())));
		}
		catch (Exception innerException)
		{
			throw new Exception("Bouncy Castle X509Crl could not be created.", innerException);
		}
		X509CrlEntry revokedCertificate = x509Crl.GetRevokedCertificate(GetSerialNumber(cert));
		if (revokedCertificate == null)
		{
			return;
		}
		X509Name issuerPrincipal = GetIssuerPrincipal(cert);
		if (!issuerPrincipal.Equivalent(revokedCertificate.GetCertificateIssuer(), inOrder: true) && !issuerPrincipal.Equivalent(crl.IssuerDN, inOrder: true))
		{
			return;
		}
		int num = 0;
		if (revokedCertificate.HasExtensions)
		{
			try
			{
				Asn1Object extensionValue = GetExtensionValue(revokedCertificate, X509Extensions.ReasonCode);
				DerEnumerated instance = DerEnumerated.GetInstance(extensionValue);
				if (instance != null)
				{
					num = instance.IntValueExact;
				}
			}
			catch (Exception innerException2)
			{
				throw new Exception("Reason code CRL entry extension could not be decoded.", innerException2);
			}
		}
		DateTime revocationDate = revokedCertificate.RevocationDate;
		if (validDate.Ticks < revocationDate.Ticks)
		{
			switch (num)
			{
			default:
				return;
			case 0:
			case 1:
			case 2:
			case 10:
				break;
			}
		}
		certStatus.Status = num;
		certStatus.RevocationDate = new DateTimeObject(revocationDate);
	}

	internal static AsymmetricKeyParameter GetNextWorkingKey(IList certs, int index)
	{
		X509Certificate x509Certificate = (X509Certificate)certs[index];
		AsymmetricKeyParameter publicKey = x509Certificate.GetPublicKey();
		if (!(publicKey is DsaPublicKeyParameters))
		{
			return publicKey;
		}
		DsaPublicKeyParameters dsaPublicKeyParameters = (DsaPublicKeyParameters)publicKey;
		if (dsaPublicKeyParameters.Parameters != null)
		{
			return dsaPublicKeyParameters;
		}
		for (int i = index + 1; i < certs.Count; i++)
		{
			X509Certificate x509Certificate2 = (X509Certificate)certs[i];
			publicKey = x509Certificate2.GetPublicKey();
			if (!(publicKey is DsaPublicKeyParameters))
			{
				throw new PkixCertPathValidatorException("DSA parameters cannot be inherited from previous certificate.");
			}
			DsaPublicKeyParameters dsaPublicKeyParameters2 = (DsaPublicKeyParameters)publicKey;
			if (dsaPublicKeyParameters2.Parameters != null)
			{
				DsaParameters parameters = dsaPublicKeyParameters2.Parameters;
				try
				{
					return new DsaPublicKeyParameters(dsaPublicKeyParameters.Y, parameters);
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}
			}
		}
		throw new PkixCertPathValidatorException("DSA parameters cannot be inherited from previous certificate.");
	}

	internal static DateTime GetValidCertDateFromValidityModel(PkixParameters paramsPkix, PkixCertPath certPath, int index)
	{
		if (paramsPkix.ValidityModel != 1)
		{
			return GetValidDate(paramsPkix);
		}
		if (index <= 0)
		{
			return GetValidDate(paramsPkix);
		}
		if (index - 1 == 0)
		{
			DerGeneralizedTime derGeneralizedTime = null;
			try
			{
				X509Certificate x509Certificate = (X509Certificate)certPath.Certificates[index - 1];
				Asn1OctetString extensionValue = x509Certificate.GetExtensionValue(IsisMttObjectIdentifiers.IdIsisMttATDateOfCertGen);
				derGeneralizedTime = DerGeneralizedTime.GetInstance(extensionValue);
			}
			catch (ArgumentException)
			{
				throw new Exception("Date of cert gen extension could not be read.");
			}
			if (derGeneralizedTime != null)
			{
				try
				{
					return derGeneralizedTime.ToDateTime();
				}
				catch (ArgumentException innerException)
				{
					throw new Exception("Date from date of cert gen extension could not be parsed.", innerException);
				}
			}
		}
		return ((X509Certificate)certPath.Certificates[index - 1]).NotBefore;
	}

	internal static ICollection FindCertificates(X509CertStoreSelector certSelect, IList certStores)
	{
		ISet set = new HashSet();
		foreach (IX509Store certStore in certStores)
		{
			try
			{
				foreach (X509Certificate match in certStore.GetMatches(certSelect))
				{
					set.Add(match);
				}
			}
			catch (Exception innerException)
			{
				throw new Exception("Problem while picking certificates from X.509 store.", innerException);
			}
		}
		return set;
	}

	internal static void GetCrlIssuersFromDistributionPoint(DistributionPoint dp, ICollection issuerPrincipals, X509CrlStoreSelector selector, PkixParameters pkixParams)
	{
		IList list = Platform.CreateArrayList();
		if (dp.CrlIssuer != null)
		{
			GeneralName[] names = dp.CrlIssuer.GetNames();
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i].TagNo == 4)
				{
					try
					{
						list.Add(X509Name.GetInstance(names[i].Name.ToAsn1Object()));
					}
					catch (IOException innerException)
					{
						throw new Exception("CRL issuer information from distribution point cannot be decoded.", innerException);
					}
				}
			}
		}
		else
		{
			if (dp.DistributionPointName == null)
			{
				throw new Exception("CRL issuer is omitted from distribution point but no distributionPoint field present.");
			}
			IEnumerator enumerator = issuerPrincipals.GetEnumerator();
			while (enumerator.MoveNext())
			{
				list.Add((X509Name)enumerator.Current);
			}
		}
		selector.Issuers = list;
	}

	internal static ISet GetCompleteCrls(DistributionPoint dp, object cert, DateTime currentDate, PkixParameters paramsPKIX)
	{
		X509CrlStoreSelector x509CrlStoreSelector = new X509CrlStoreSelector();
		try
		{
			ISet set = new HashSet();
			if (cert is X509V2AttributeCertificate)
			{
				set.Add(((X509V2AttributeCertificate)cert).Issuer.GetPrincipals()[0]);
			}
			else
			{
				set.Add(GetIssuerPrincipal(cert));
			}
			GetCrlIssuersFromDistributionPoint(dp, set, x509CrlStoreSelector, paramsPKIX);
		}
		catch (Exception innerException)
		{
			throw new Exception("Could not get issuer information from distribution point.", innerException);
		}
		if (cert is X509Certificate)
		{
			x509CrlStoreSelector.CertificateChecking = (X509Certificate)cert;
		}
		else if (cert is X509V2AttributeCertificate)
		{
			x509CrlStoreSelector.AttrCertChecking = (IX509AttributeCertificate)cert;
		}
		x509CrlStoreSelector.CompleteCrlEnabled = true;
		ISet set2 = CrlUtilities.FindCrls(x509CrlStoreSelector, paramsPKIX, currentDate);
		if (set2.IsEmpty)
		{
			if (cert is IX509AttributeCertificate)
			{
				IX509AttributeCertificate iX509AttributeCertificate = (IX509AttributeCertificate)cert;
				throw new Exception(string.Concat("No CRLs found for issuer \"", iX509AttributeCertificate.Issuer.GetPrincipals()[0], "\""));
			}
			X509Certificate x509Certificate = (X509Certificate)cert;
			throw new Exception(string.Concat("No CRLs found for issuer \"", x509Certificate.IssuerDN, "\""));
		}
		return set2;
	}

	internal static ISet GetDeltaCrls(DateTime currentDate, PkixParameters paramsPKIX, X509Crl completeCRL)
	{
		X509CrlStoreSelector x509CrlStoreSelector = new X509CrlStoreSelector();
		try
		{
			IList list = Platform.CreateArrayList();
			list.Add(completeCRL.IssuerDN);
			x509CrlStoreSelector.Issuers = list;
		}
		catch (IOException innerException)
		{
			throw new Exception("Cannot extract issuer from CRL.", innerException);
		}
		BigInteger bigInteger = null;
		try
		{
			Asn1Object extensionValue = GetExtensionValue(completeCRL, X509Extensions.CrlNumber);
			if (extensionValue != null)
			{
				bigInteger = DerInteger.GetInstance(extensionValue).PositiveValue;
			}
		}
		catch (Exception innerException2)
		{
			throw new Exception("CRL number extension could not be extracted from CRL.", innerException2);
		}
		byte[] issuingDistributionPoint = null;
		try
		{
			Asn1Object extensionValue2 = GetExtensionValue(completeCRL, X509Extensions.IssuingDistributionPoint);
			if (extensionValue2 != null)
			{
				issuingDistributionPoint = extensionValue2.GetDerEncoded();
			}
		}
		catch (Exception innerException3)
		{
			throw new Exception("Issuing distribution point extension value could not be read.", innerException3);
		}
		x509CrlStoreSelector.MinCrlNumber = bigInteger?.Add(BigInteger.One);
		x509CrlStoreSelector.IssuingDistributionPoint = issuingDistributionPoint;
		x509CrlStoreSelector.IssuingDistributionPointEnabled = true;
		x509CrlStoreSelector.MaxBaseCrlNumber = bigInteger;
		ISet set = CrlUtilities.FindCrls(x509CrlStoreSelector, paramsPKIX, currentDate);
		ISet set2 = new HashSet();
		foreach (X509Crl item in set)
		{
			if (isDeltaCrl(item))
			{
				set2.Add(item);
			}
		}
		return set2;
	}

	private static bool isDeltaCrl(X509Crl crl)
	{
		ISet criticalExtensionOids = crl.GetCriticalExtensionOids();
		return criticalExtensionOids.Contains(X509Extensions.DeltaCrlIndicator.Id);
	}

	internal static ICollection FindCertificates(X509AttrCertStoreSelector certSelect, IList certStores)
	{
		ISet set = new HashSet();
		foreach (IX509Store certStore in certStores)
		{
			try
			{
				foreach (X509V2AttributeCertificate match in certStore.GetMatches(certSelect))
				{
					set.Add(match);
				}
			}
			catch (Exception innerException)
			{
				throw new Exception("Problem while picking certificates from X.509 store.", innerException);
			}
		}
		return set;
	}

	internal static void AddAdditionalStoresFromCrlDistributionPoint(CrlDistPoint crldp, PkixParameters pkixParams)
	{
		if (crldp == null)
		{
			return;
		}
		DistributionPoint[] array = null;
		try
		{
			array = crldp.GetDistributionPoints();
		}
		catch (Exception innerException)
		{
			throw new Exception("Distribution points could not be read.", innerException);
		}
		for (int i = 0; i < array.Length; i++)
		{
			DistributionPointName distributionPointName = array[i].DistributionPointName;
			if (distributionPointName == null || distributionPointName.PointType != 0)
			{
				continue;
			}
			GeneralName[] names = GeneralNames.GetInstance(distributionPointName.Name).GetNames();
			for (int j = 0; j < names.Length; j++)
			{
				if (names[j].TagNo == 6)
				{
					string @string = DerIA5String.GetInstance(names[j].Name).GetString();
					AddAdditionalStoreFromLocation(@string, pkixParams);
				}
			}
		}
	}

	internal static bool ProcessCertD1i(int index, IList[] policyNodes, DerObjectIdentifier pOid, ISet pq)
	{
		IList list = policyNodes[index - 1];
		for (int i = 0; i < list.Count; i++)
		{
			PkixPolicyNode pkixPolicyNode = (PkixPolicyNode)list[i];
			ISet expectedPolicies = pkixPolicyNode.ExpectedPolicies;
			if (expectedPolicies.Contains(pOid.Id))
			{
				ISet set = new HashSet();
				set.Add(pOid.Id);
				PkixPolicyNode pkixPolicyNode2 = new PkixPolicyNode(Platform.CreateArrayList(), index, set, pkixPolicyNode, pq, pOid.Id, critical: false);
				pkixPolicyNode.AddChild(pkixPolicyNode2);
				policyNodes[index].Add(pkixPolicyNode2);
				return true;
			}
		}
		return false;
	}

	internal static void ProcessCertD1ii(int index, IList[] policyNodes, DerObjectIdentifier _poid, ISet _pq)
	{
		IList list = policyNodes[index - 1];
		for (int i = 0; i < list.Count; i++)
		{
			PkixPolicyNode pkixPolicyNode = (PkixPolicyNode)list[i];
			if (ANY_POLICY.Equals(pkixPolicyNode.ValidPolicy))
			{
				ISet set = new HashSet();
				set.Add(_poid.Id);
				PkixPolicyNode pkixPolicyNode2 = new PkixPolicyNode(Platform.CreateArrayList(), index, set, pkixPolicyNode, _pq, _poid.Id, critical: false);
				pkixPolicyNode.AddChild(pkixPolicyNode2);
				policyNodes[index].Add(pkixPolicyNode2);
				break;
			}
		}
	}

	internal static ICollection FindIssuerCerts(X509Certificate cert, PkixBuilderParameters pkixParams)
	{
		X509CertStoreSelector x509CertStoreSelector = new X509CertStoreSelector();
		ISet set = new HashSet();
		try
		{
			x509CertStoreSelector.Subject = cert.IssuerDN;
		}
		catch (IOException innerException)
		{
			throw new Exception("Subject criteria for certificate selector to find issuer certificate could not be set.", innerException);
		}
		try
		{
			set.AddAll(FindCertificates(x509CertStoreSelector, pkixParams.GetStores()));
			set.AddAll(FindCertificates(x509CertStoreSelector, pkixParams.GetAdditionalStores()));
			return set;
		}
		catch (Exception innerException2)
		{
			throw new Exception("Issuer certificate cannot be searched.", innerException2);
		}
	}

	internal static Asn1Object GetExtensionValue(IX509Extension ext, DerObjectIdentifier oid)
	{
		Asn1OctetString extensionValue = ext.GetExtensionValue(oid);
		if (extensionValue == null)
		{
			return null;
		}
		return X509ExtensionUtilities.FromExtensionValue(extensionValue);
	}
}
