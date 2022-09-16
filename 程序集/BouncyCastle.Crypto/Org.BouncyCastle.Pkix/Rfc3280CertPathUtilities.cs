using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix;

public class Rfc3280CertPathUtilities
{
	private static readonly PkixCrlUtilities CrlUtilities = new PkixCrlUtilities();

	internal static readonly string ANY_POLICY = "2.5.29.32.0";

	internal static readonly int KEY_CERT_SIGN = 5;

	internal static readonly int CRL_SIGN = 6;

	internal static readonly string[] CrlReasons = new string[11]
	{
		"unspecified", "keyCompromise", "cACompromise", "affiliationChanged", "superseded", "cessationOfOperation", "certificateHold", "unknown", "removeFromCRL", "privilegeWithdrawn",
		"aACompromise"
	};

	internal static void ProcessCrlB2(DistributionPoint dp, object cert, X509Crl crl)
	{
		IssuingDistributionPoint issuingDistributionPoint = null;
		try
		{
			issuingDistributionPoint = IssuingDistributionPoint.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(crl, X509Extensions.IssuingDistributionPoint));
		}
		catch (Exception innerException)
		{
			throw new Exception("0 Issuing distribution point extension could not be decoded.", innerException);
		}
		if (issuingDistributionPoint == null)
		{
			return;
		}
		if (issuingDistributionPoint.DistributionPoint != null)
		{
			DistributionPointName distributionPoint = IssuingDistributionPoint.GetInstance(issuingDistributionPoint).DistributionPoint;
			IList list = Platform.CreateArrayList();
			if (distributionPoint.PointType == 0)
			{
				GeneralName[] names = GeneralNames.GetInstance(distributionPoint.Name).GetNames();
				for (int i = 0; i < names.Length; i++)
				{
					list.Add(names[i]);
				}
			}
			if (distributionPoint.PointType == 1)
			{
				Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector();
				try
				{
					IEnumerator enumerator = Asn1Sequence.GetInstance(Asn1Object.FromByteArray(crl.IssuerDN.GetEncoded())).GetEnumerator();
					while (enumerator.MoveNext())
					{
						asn1EncodableVector.Add((Asn1Encodable)enumerator.Current);
					}
				}
				catch (IOException innerException2)
				{
					throw new Exception("Could not read CRL issuer.", innerException2);
				}
				asn1EncodableVector.Add(distributionPoint.Name);
				list.Add(new GeneralName(X509Name.GetInstance(new DerSequence(asn1EncodableVector))));
			}
			bool flag = false;
			if (dp.DistributionPointName != null)
			{
				distributionPoint = dp.DistributionPointName;
				GeneralName[] array = null;
				if (distributionPoint.PointType == 0)
				{
					array = GeneralNames.GetInstance(distributionPoint.Name).GetNames();
				}
				if (distributionPoint.PointType == 1)
				{
					if (dp.CrlIssuer != null)
					{
						array = dp.CrlIssuer.GetNames();
					}
					else
					{
						array = new GeneralName[1];
						try
						{
							array[0] = new GeneralName(PkixCertPathValidatorUtilities.GetIssuerPrincipal(cert));
						}
						catch (IOException innerException3)
						{
							throw new Exception("Could not read certificate issuer.", innerException3);
						}
					}
					for (int j = 0; j < array.Length; j++)
					{
						IEnumerator enumerator2 = Asn1Sequence.GetInstance(array[j].Name.ToAsn1Object()).GetEnumerator();
						Asn1EncodableVector asn1EncodableVector2 = new Asn1EncodableVector();
						while (enumerator2.MoveNext())
						{
							asn1EncodableVector2.Add((Asn1Encodable)enumerator2.Current);
						}
						asn1EncodableVector2.Add(distributionPoint.Name);
						array[j] = new GeneralName(X509Name.GetInstance(new DerSequence(asn1EncodableVector2)));
					}
				}
				if (array != null)
				{
					for (int k = 0; k < array.Length; k++)
					{
						if (list.Contains(array[k]))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					throw new Exception("No match for certificate CRL issuing distribution point name to cRLIssuer CRL distribution point.");
				}
			}
			else
			{
				if (dp.CrlIssuer == null)
				{
					throw new Exception("Either the cRLIssuer or the distributionPoint field must be contained in DistributionPoint.");
				}
				GeneralName[] names2 = dp.CrlIssuer.GetNames();
				for (int l = 0; l < names2.Length; l++)
				{
					if (list.Contains(names2[l]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw new Exception("No match for certificate CRL issuing distribution point name to cRLIssuer CRL distribution point.");
				}
			}
		}
		BasicConstraints basicConstraints = null;
		try
		{
			basicConstraints = BasicConstraints.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue((IX509Extension)cert, X509Extensions.BasicConstraints));
		}
		catch (Exception innerException4)
		{
			throw new Exception("Basic constraints extension could not be decoded.", innerException4);
		}
		if (issuingDistributionPoint.OnlyContainsUserCerts && basicConstraints != null && basicConstraints.IsCA())
		{
			throw new Exception("CA Cert CRL only contains user certificates.");
		}
		if (issuingDistributionPoint.OnlyContainsCACerts && (basicConstraints == null || !basicConstraints.IsCA()))
		{
			throw new Exception("End CRL only contains CA certificates.");
		}
		if (issuingDistributionPoint.OnlyContainsAttributeCerts)
		{
			throw new Exception("onlyContainsAttributeCerts boolean is asserted.");
		}
	}

	internal static void ProcessCertBC(PkixCertPath certPath, int index, PkixNameConstraintValidator nameConstraintValidator)
	{
		IList certificates = certPath.Certificates;
		X509Certificate x509Certificate = (X509Certificate)certificates[index];
		int count = certificates.Count;
		int num = count - index;
		if (PkixCertPathValidatorUtilities.IsSelfIssued(x509Certificate) && num < count)
		{
			return;
		}
		X509Name subjectDN = x509Certificate.SubjectDN;
		Asn1Sequence instance;
		try
		{
			instance = Asn1Sequence.GetInstance(subjectDN.GetEncoded());
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Exception extracting subject name when checking subtrees.", cause, certPath, index);
		}
		try
		{
			nameConstraintValidator.CheckPermittedDN(instance);
			nameConstraintValidator.CheckExcludedDN(instance);
		}
		catch (PkixNameConstraintValidatorException cause2)
		{
			throw new PkixCertPathValidatorException("Subtree check for certificate subject failed.", cause2, certPath, index);
		}
		GeneralNames generalNames = null;
		try
		{
			generalNames = GeneralNames.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(x509Certificate, X509Extensions.SubjectAlternativeName));
		}
		catch (Exception cause3)
		{
			throw new PkixCertPathValidatorException("Subject alternative name extension could not be decoded.", cause3, certPath, index);
		}
		IList valueList = X509Name.GetInstance(instance).GetValueList(X509Name.EmailAddress);
		foreach (string item in valueList)
		{
			GeneralName name2 = new GeneralName(1, item);
			try
			{
				nameConstraintValidator.checkPermitted(name2);
				nameConstraintValidator.checkExcluded(name2);
			}
			catch (PkixNameConstraintValidatorException cause4)
			{
				throw new PkixCertPathValidatorException("Subtree check for certificate subject alternative email failed.", cause4, certPath, index);
			}
		}
		if (generalNames == null)
		{
			return;
		}
		GeneralName[] array = null;
		try
		{
			array = generalNames.GetNames();
		}
		catch (Exception cause5)
		{
			throw new PkixCertPathValidatorException("Subject alternative name contents could not be decoded.", cause5, certPath, index);
		}
		GeneralName[] array2 = array;
		foreach (GeneralName name3 in array2)
		{
			try
			{
				nameConstraintValidator.checkPermitted(name3);
				nameConstraintValidator.checkExcluded(name3);
			}
			catch (PkixNameConstraintValidatorException cause6)
			{
				throw new PkixCertPathValidatorException("Subtree check for certificate subject alternative name failed.", cause6, certPath, index);
			}
		}
	}

	internal static void PrepareNextCertA(PkixCertPath certPath, int index)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		Asn1Sequence asn1Sequence = null;
		try
		{
			asn1Sequence = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.PolicyMappings));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Policy mappings extension could not be decoded.", cause, certPath, index);
		}
		if (asn1Sequence == null)
		{
			return;
		}
		Asn1Sequence asn1Sequence2 = asn1Sequence;
		for (int i = 0; i < asn1Sequence2.Count; i++)
		{
			DerObjectIdentifier derObjectIdentifier = null;
			DerObjectIdentifier derObjectIdentifier2 = null;
			try
			{
				Asn1Sequence instance = Asn1Sequence.GetInstance(asn1Sequence2[i]);
				derObjectIdentifier = DerObjectIdentifier.GetInstance(instance[0]);
				derObjectIdentifier2 = DerObjectIdentifier.GetInstance(instance[1]);
			}
			catch (Exception cause2)
			{
				throw new PkixCertPathValidatorException("Policy mappings extension contents could not be decoded.", cause2, certPath, index);
			}
			if (ANY_POLICY.Equals(derObjectIdentifier.Id))
			{
				throw new PkixCertPathValidatorException("IssuerDomainPolicy is anyPolicy", null, certPath, index);
			}
			if (ANY_POLICY.Equals(derObjectIdentifier2.Id))
			{
				throw new PkixCertPathValidatorException("SubjectDomainPolicy is anyPolicy,", null, certPath, index);
			}
		}
	}

	internal static PkixPolicyNode ProcessCertD(PkixCertPath certPath, int index, ISet acceptablePolicies, PkixPolicyNode validPolicyTree, IList[] policyNodes, int inhibitAnyPolicy)
	{
		IList certificates = certPath.Certificates;
		X509Certificate x509Certificate = (X509Certificate)certificates[index];
		int count = certificates.Count;
		int num = count - index;
		Asn1Sequence asn1Sequence = null;
		try
		{
			asn1Sequence = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(x509Certificate, X509Extensions.CertificatePolicies));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Could not read certificate policies extension from certificate.", cause, certPath, index);
		}
		if (asn1Sequence != null && validPolicyTree != null)
		{
			ISet set = new HashSet();
			foreach (Asn1Encodable item in asn1Sequence)
			{
				PolicyInformation instance = PolicyInformation.GetInstance(item.ToAsn1Object());
				DerObjectIdentifier policyIdentifier = instance.PolicyIdentifier;
				set.Add(policyIdentifier.Id);
				if (!ANY_POLICY.Equals(policyIdentifier.Id))
				{
					ISet set2 = null;
					try
					{
						set2 = PkixCertPathValidatorUtilities.GetQualifierSet(instance.PolicyQualifiers);
					}
					catch (PkixCertPathValidatorException cause2)
					{
						throw new PkixCertPathValidatorException("Policy qualifier info set could not be build.", cause2, certPath, index);
					}
					if (!PkixCertPathValidatorUtilities.ProcessCertD1i(num, policyNodes, policyIdentifier, set2))
					{
						PkixCertPathValidatorUtilities.ProcessCertD1ii(num, policyNodes, policyIdentifier, set2);
					}
				}
			}
			if (acceptablePolicies.IsEmpty || acceptablePolicies.Contains(ANY_POLICY))
			{
				acceptablePolicies.Clear();
				acceptablePolicies.AddAll(set);
			}
			else
			{
				ISet set3 = new HashSet();
				foreach (object acceptablePolicy in acceptablePolicies)
				{
					if (set.Contains(acceptablePolicy))
					{
						set3.Add(acceptablePolicy);
					}
				}
				acceptablePolicies.Clear();
				acceptablePolicies.AddAll(set3);
			}
			if (inhibitAnyPolicy > 0 || (num < count && PkixCertPathValidatorUtilities.IsSelfIssued(x509Certificate)))
			{
				foreach (Asn1Encodable item2 in asn1Sequence)
				{
					PolicyInformation instance2 = PolicyInformation.GetInstance(item2.ToAsn1Object());
					if (!ANY_POLICY.Equals(instance2.PolicyIdentifier.Id))
					{
						continue;
					}
					ISet qualifierSet = PkixCertPathValidatorUtilities.GetQualifierSet(instance2.PolicyQualifiers);
					IList list = policyNodes[num - 1];
					for (int i = 0; i < list.Count; i++)
					{
						PkixPolicyNode pkixPolicyNode = (PkixPolicyNode)list[i];
						IEnumerator enumerator2 = pkixPolicyNode.ExpectedPolicies.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							object current2 = enumerator2.Current;
							string text;
							if (current2 is string)
							{
								text = (string)current2;
							}
							else
							{
								if (!(current2 is DerObjectIdentifier))
								{
									continue;
								}
								text = ((DerObjectIdentifier)current2).Id;
							}
							bool flag = false;
							foreach (PkixPolicyNode child in pkixPolicyNode.Children)
							{
								if (text.Equals(child.ValidPolicy))
								{
									flag = true;
								}
							}
							if (!flag)
							{
								ISet set4 = new HashSet();
								set4.Add(text);
								PkixPolicyNode pkixPolicyNode3 = new PkixPolicyNode(Platform.CreateArrayList(), num, set4, pkixPolicyNode, qualifierSet, text, critical: false);
								pkixPolicyNode.AddChild(pkixPolicyNode3);
								policyNodes[num].Add(pkixPolicyNode3);
							}
						}
					}
					break;
				}
			}
			PkixPolicyNode pkixPolicyNode4 = validPolicyTree;
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				IList list2 = policyNodes[num2];
				for (int j = 0; j < list2.Count; j++)
				{
					PkixPolicyNode pkixPolicyNode5 = (PkixPolicyNode)list2[j];
					if (!pkixPolicyNode5.HasChildren)
					{
						pkixPolicyNode4 = PkixCertPathValidatorUtilities.RemovePolicyNode(pkixPolicyNode4, policyNodes, pkixPolicyNode5);
						if (pkixPolicyNode4 == null)
						{
							break;
						}
					}
				}
			}
			ISet criticalExtensionOids = x509Certificate.GetCriticalExtensionOids();
			if (criticalExtensionOids != null)
			{
				bool isCritical = criticalExtensionOids.Contains(X509Extensions.CertificatePolicies.Id);
				IList list3 = policyNodes[num];
				for (int k = 0; k < list3.Count; k++)
				{
					PkixPolicyNode pkixPolicyNode6 = (PkixPolicyNode)list3[k];
					pkixPolicyNode6.IsCritical = isCritical;
				}
			}
			return pkixPolicyNode4;
		}
		return null;
	}

	internal static void ProcessCrlB1(DistributionPoint dp, object cert, X509Crl crl)
	{
		Asn1Object extensionValue = PkixCertPathValidatorUtilities.GetExtensionValue(crl, X509Extensions.IssuingDistributionPoint);
		bool flag = false;
		if (extensionValue != null && IssuingDistributionPoint.GetInstance(extensionValue).IsIndirectCrl)
		{
			flag = true;
		}
		byte[] encoded = crl.IssuerDN.GetEncoded();
		bool flag2 = false;
		if (dp.CrlIssuer != null)
		{
			GeneralName[] names = dp.CrlIssuer.GetNames();
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i].TagNo != 4)
				{
					continue;
				}
				try
				{
					if (Arrays.AreEqual(names[i].Name.ToAsn1Object().GetEncoded(), encoded))
					{
						flag2 = true;
					}
				}
				catch (IOException innerException)
				{
					throw new Exception("CRL issuer information from distribution point cannot be decoded.", innerException);
				}
			}
			if (flag2 && !flag)
			{
				throw new Exception("Distribution point contains cRLIssuer field but CRL is not indirect.");
			}
			if (!flag2)
			{
				throw new Exception("CRL issuer of CRL does not match CRL issuer of distribution point.");
			}
		}
		else if (crl.IssuerDN.Equivalent(PkixCertPathValidatorUtilities.GetIssuerPrincipal(cert), inOrder: true))
		{
			flag2 = true;
		}
		if (!flag2)
		{
			throw new Exception("Cannot find matching CRL issuer for certificate.");
		}
	}

	internal static ReasonsMask ProcessCrlD(X509Crl crl, DistributionPoint dp)
	{
		IssuingDistributionPoint issuingDistributionPoint = null;
		try
		{
			issuingDistributionPoint = IssuingDistributionPoint.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(crl, X509Extensions.IssuingDistributionPoint));
		}
		catch (Exception innerException)
		{
			throw new Exception("issuing distribution point extension could not be decoded.", innerException);
		}
		if (issuingDistributionPoint != null && issuingDistributionPoint.OnlySomeReasons != null && dp.Reasons != null)
		{
			return new ReasonsMask(dp.Reasons.IntValue).Intersect(new ReasonsMask(issuingDistributionPoint.OnlySomeReasons.IntValue));
		}
		if ((issuingDistributionPoint == null || issuingDistributionPoint.OnlySomeReasons == null) && dp.Reasons == null)
		{
			return ReasonsMask.AllReasons;
		}
		ReasonsMask reasonsMask = null;
		reasonsMask = ((dp.Reasons != null) ? new ReasonsMask(dp.Reasons.IntValue) : ReasonsMask.AllReasons);
		ReasonsMask reasonsMask2 = null;
		reasonsMask2 = ((issuingDistributionPoint != null) ? new ReasonsMask(issuingDistributionPoint.OnlySomeReasons.IntValue) : ReasonsMask.AllReasons);
		return reasonsMask.Intersect(reasonsMask2);
	}

	internal static ISet ProcessCrlF(X509Crl crl, object cert, X509Certificate defaultCRLSignCert, AsymmetricKeyParameter defaultCRLSignKey, PkixParameters paramsPKIX, IList certPathCerts)
	{
		X509CertStoreSelector x509CertStoreSelector = new X509CertStoreSelector();
		try
		{
			x509CertStoreSelector.Subject = crl.IssuerDN;
		}
		catch (IOException innerException)
		{
			throw new Exception("Subject criteria for certificate selector to find issuer certificate for CRL could not be set.", innerException);
		}
		IList list = Platform.CreateArrayList();
		try
		{
			CollectionUtilities.AddRange(list, PkixCertPathValidatorUtilities.FindCertificates(x509CertStoreSelector, paramsPKIX.GetStores()));
			CollectionUtilities.AddRange(list, PkixCertPathValidatorUtilities.FindCertificates(x509CertStoreSelector, paramsPKIX.GetAdditionalStores()));
		}
		catch (Exception innerException2)
		{
			throw new Exception("Issuer certificate for CRL cannot be searched.", innerException2);
		}
		list.Add(defaultCRLSignCert);
		IEnumerator enumerator = list.GetEnumerator();
		IList list2 = Platform.CreateArrayList();
		IList list3 = Platform.CreateArrayList();
		while (enumerator.MoveNext())
		{
			X509Certificate x509Certificate = (X509Certificate)enumerator.Current;
			if (x509Certificate.Equals(defaultCRLSignCert))
			{
				list2.Add(x509Certificate);
				list3.Add(defaultCRLSignKey);
				continue;
			}
			try
			{
				PkixCertPathBuilder pkixCertPathBuilder = new PkixCertPathBuilder();
				x509CertStoreSelector = new X509CertStoreSelector();
				x509CertStoreSelector.Certificate = x509Certificate;
				PkixParameters pkixParameters = (PkixParameters)paramsPKIX.Clone();
				pkixParameters.SetTargetCertConstraints(x509CertStoreSelector);
				PkixBuilderParameters instance = PkixBuilderParameters.GetInstance(pkixParameters);
				if (certPathCerts.Contains(x509Certificate))
				{
					instance.IsRevocationEnabled = false;
				}
				else
				{
					instance.IsRevocationEnabled = true;
				}
				IList certificates = pkixCertPathBuilder.Build(instance).CertPath.Certificates;
				list2.Add(x509Certificate);
				list3.Add(PkixCertPathValidatorUtilities.GetNextWorkingKey(certificates, 0));
			}
			catch (PkixCertPathBuilderException innerException3)
			{
				throw new Exception("CertPath for CRL signer failed to validate.", innerException3);
			}
			catch (PkixCertPathValidatorException innerException4)
			{
				throw new Exception("Public key of issuer certificate of CRL could not be retrieved.", innerException4);
			}
		}
		ISet set = new HashSet();
		Exception ex = null;
		for (int i = 0; i < list2.Count; i++)
		{
			X509Certificate x509Certificate2 = (X509Certificate)list2[i];
			bool[] keyUsage = x509Certificate2.GetKeyUsage();
			if (keyUsage != null && (keyUsage.Length < 7 || !keyUsage[CRL_SIGN]))
			{
				ex = new Exception("Issuer certificate key usage extension does not permit CRL signing.");
			}
			else
			{
				set.Add(list3[i]);
			}
		}
		if (set.Count == 0 && ex == null)
		{
			throw new Exception("Cannot find a valid issuer certificate.");
		}
		if (set.Count == 0 && ex != null)
		{
			throw ex;
		}
		return set;
	}

	internal static AsymmetricKeyParameter ProcessCrlG(X509Crl crl, ISet keys)
	{
		Exception innerException = null;
		foreach (AsymmetricKeyParameter key in keys)
		{
			try
			{
				crl.Verify(key);
				return key;
			}
			catch (Exception ex)
			{
				innerException = ex;
			}
		}
		throw new Exception("Cannot verify CRL.", innerException);
	}

	internal static X509Crl ProcessCrlH(ISet deltaCrls, AsymmetricKeyParameter key)
	{
		Exception ex = null;
		foreach (X509Crl deltaCrl in deltaCrls)
		{
			try
			{
				deltaCrl.Verify(key);
				return deltaCrl;
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
		}
		if (ex != null)
		{
			throw new Exception("Cannot verify delta CRL.", ex);
		}
		return null;
	}

	private static void CheckCrl(DistributionPoint dp, PkixParameters paramsPKIX, X509Certificate cert, DateTime validDate, X509Certificate defaultCRLSignCert, AsymmetricKeyParameter defaultCRLSignKey, CertStatus certStatus, ReasonsMask reasonMask, IList certPathCerts)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (validDate.Ticks > utcNow.Ticks)
		{
			throw new Exception("Validation time is in future.");
		}
		ISet completeCrls = PkixCertPathValidatorUtilities.GetCompleteCrls(dp, cert, utcNow, paramsPKIX);
		bool flag = false;
		Exception ex = null;
		IEnumerator enumerator = completeCrls.GetEnumerator();
		while (enumerator.MoveNext() && certStatus.Status == 11 && !reasonMask.IsAllReasons)
		{
			try
			{
				X509Crl x509Crl = (X509Crl)enumerator.Current;
				ReasonsMask reasonsMask = ProcessCrlD(x509Crl, dp);
				if (!reasonsMask.HasNewReasons(reasonMask))
				{
					continue;
				}
				ISet keys = ProcessCrlF(x509Crl, cert, defaultCRLSignCert, defaultCRLSignKey, paramsPKIX, certPathCerts);
				AsymmetricKeyParameter key = ProcessCrlG(x509Crl, keys);
				X509Crl x509Crl2 = null;
				if (paramsPKIX.IsUseDeltasEnabled)
				{
					ISet deltaCrls = PkixCertPathValidatorUtilities.GetDeltaCrls(utcNow, paramsPKIX, x509Crl);
					x509Crl2 = ProcessCrlH(deltaCrls, key);
				}
				if (paramsPKIX.ValidityModel != 1 && cert.NotAfter.Ticks < x509Crl.ThisUpdate.Ticks)
				{
					throw new Exception("No valid CRL for current time found.");
				}
				ProcessCrlB1(dp, cert, x509Crl);
				ProcessCrlB2(dp, cert, x509Crl);
				ProcessCrlC(x509Crl2, x509Crl, paramsPKIX);
				ProcessCrlI(validDate, x509Crl2, cert, certStatus, paramsPKIX);
				ProcessCrlJ(validDate, x509Crl, cert, certStatus);
				if (certStatus.Status == 8)
				{
					certStatus.Status = 11;
				}
				reasonMask.AddReasons(reasonsMask);
				ISet criticalExtensionOids = x509Crl.GetCriticalExtensionOids();
				if (criticalExtensionOids != null)
				{
					criticalExtensionOids = new HashSet(criticalExtensionOids);
					criticalExtensionOids.Remove(X509Extensions.IssuingDistributionPoint.Id);
					criticalExtensionOids.Remove(X509Extensions.DeltaCrlIndicator.Id);
					if (!criticalExtensionOids.IsEmpty)
					{
						throw new Exception("CRL contains unsupported critical extensions.");
					}
				}
				if (x509Crl2 != null)
				{
					criticalExtensionOids = x509Crl2.GetCriticalExtensionOids();
					if (criticalExtensionOids != null)
					{
						criticalExtensionOids = new HashSet(criticalExtensionOids);
						criticalExtensionOids.Remove(X509Extensions.IssuingDistributionPoint.Id);
						criticalExtensionOids.Remove(X509Extensions.DeltaCrlIndicator.Id);
						if (!criticalExtensionOids.IsEmpty)
						{
							throw new Exception("Delta CRL contains unsupported critical extension.");
						}
					}
				}
				flag = true;
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
		}
		if (!flag)
		{
			throw ex;
		}
	}

	protected static void CheckCrls(PkixParameters paramsPKIX, X509Certificate cert, DateTime validDate, X509Certificate sign, AsymmetricKeyParameter workingPublicKey, IList certPathCerts)
	{
		Exception ex = null;
		CrlDistPoint crlDistPoint = null;
		try
		{
			crlDistPoint = CrlDistPoint.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(cert, X509Extensions.CrlDistributionPoints));
		}
		catch (Exception innerException)
		{
			throw new Exception("CRL distribution point extension could not be read.", innerException);
		}
		try
		{
			PkixCertPathValidatorUtilities.AddAdditionalStoresFromCrlDistributionPoint(crlDistPoint, paramsPKIX);
		}
		catch (Exception innerException2)
		{
			throw new Exception("No additional CRL locations could be decoded from CRL distribution point extension.", innerException2);
		}
		CertStatus certStatus = new CertStatus();
		ReasonsMask reasonsMask = new ReasonsMask();
		bool flag = false;
		if (crlDistPoint != null)
		{
			DistributionPoint[] array = null;
			try
			{
				array = crlDistPoint.GetDistributionPoints();
			}
			catch (Exception innerException3)
			{
				throw new Exception("Distribution points could not be read.", innerException3);
			}
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (certStatus.Status != 11)
					{
						break;
					}
					if (reasonsMask.IsAllReasons)
					{
						break;
					}
					PkixParameters paramsPKIX2 = (PkixParameters)paramsPKIX.Clone();
					try
					{
						CheckCrl(array[i], paramsPKIX2, cert, validDate, sign, workingPublicKey, certStatus, reasonsMask, certPathCerts);
						flag = true;
					}
					catch (Exception ex2)
					{
						ex = ex2;
					}
				}
			}
		}
		if (certStatus.Status == 11 && !reasonsMask.IsAllReasons)
		{
			try
			{
				X509Name instance;
				try
				{
					instance = X509Name.GetInstance(cert.IssuerDN.GetEncoded());
				}
				catch (Exception innerException4)
				{
					throw new Exception("Issuer from certificate for CRL could not be reencoded.", innerException4);
				}
				DistributionPoint dp = new DistributionPoint(new DistributionPointName(0, new GeneralNames(new GeneralName(4, instance))), null, null);
				PkixParameters paramsPKIX3 = (PkixParameters)paramsPKIX.Clone();
				CheckCrl(dp, paramsPKIX3, cert, validDate, sign, workingPublicKey, certStatus, reasonsMask, certPathCerts);
				flag = true;
			}
			catch (Exception ex3)
			{
				ex = ex3;
			}
		}
		if (!flag)
		{
			throw ex;
		}
		if (certStatus.Status != 11)
		{
			string text = certStatus.RevocationDate.Value.ToString("ddd MMM dd HH:mm:ss K yyyy");
			string text2 = "Certificate revocation after " + text;
			text2 = text2 + ", reason: " + CrlReasons[certStatus.Status];
			throw new Exception(text2);
		}
		if (!reasonsMask.IsAllReasons && certStatus.Status == 11)
		{
			certStatus.Status = 12;
		}
		if (certStatus.Status == 12)
		{
			throw new Exception("Certificate status could not be determined.");
		}
	}

	internal static PkixPolicyNode PrepareCertB(PkixCertPath certPath, int index, IList[] policyNodes, PkixPolicyNode validPolicyTree, int policyMapping)
	{
		IList certificates = certPath.Certificates;
		X509Certificate x509Certificate = (X509Certificate)certificates[index];
		int count = certificates.Count;
		int num = count - index;
		Asn1Sequence asn1Sequence = null;
		try
		{
			asn1Sequence = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(x509Certificate, X509Extensions.PolicyMappings));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Policy mappings extension could not be decoded.", cause, certPath, index);
		}
		PkixPolicyNode pkixPolicyNode = validPolicyTree;
		if (asn1Sequence != null)
		{
			Asn1Sequence asn1Sequence2 = asn1Sequence;
			IDictionary dictionary = Platform.CreateHashtable();
			ISet set = new HashSet();
			for (int i = 0; i < asn1Sequence2.Count; i++)
			{
				Asn1Sequence asn1Sequence3 = (Asn1Sequence)asn1Sequence2[i];
				string id = ((DerObjectIdentifier)asn1Sequence3[0]).Id;
				string id2 = ((DerObjectIdentifier)asn1Sequence3[1]).Id;
				if (!dictionary.Contains(id))
				{
					ISet set2 = new HashSet();
					set2.Add(id2);
					dictionary[id] = set2;
					set.Add(id);
				}
				else
				{
					ISet set2 = (ISet)dictionary[id];
					set2.Add(id2);
				}
			}
			IEnumerator enumerator = set.GetEnumerator();
			while (enumerator.MoveNext())
			{
				string text = (string)enumerator.Current;
				if (policyMapping > 0)
				{
					bool flag = false;
					IEnumerator enumerator2 = policyNodes[num].GetEnumerator();
					while (enumerator2.MoveNext())
					{
						PkixPolicyNode pkixPolicyNode2 = (PkixPolicyNode)enumerator2.Current;
						if (pkixPolicyNode2.ValidPolicy.Equals(text))
						{
							flag = true;
							pkixPolicyNode2.ExpectedPolicies = (ISet)dictionary[text];
							break;
						}
					}
					if (flag)
					{
						continue;
					}
					enumerator2 = policyNodes[num].GetEnumerator();
					while (enumerator2.MoveNext())
					{
						PkixPolicyNode pkixPolicyNode3 = (PkixPolicyNode)enumerator2.Current;
						if (!ANY_POLICY.Equals(pkixPolicyNode3.ValidPolicy))
						{
							continue;
						}
						ISet policyQualifiers = null;
						Asn1Sequence asn1Sequence4 = null;
						try
						{
							asn1Sequence4 = (Asn1Sequence)PkixCertPathValidatorUtilities.GetExtensionValue(x509Certificate, X509Extensions.CertificatePolicies);
						}
						catch (Exception cause2)
						{
							throw new PkixCertPathValidatorException("Certificate policies extension could not be decoded.", cause2, certPath, index);
						}
						foreach (Asn1Encodable item in asn1Sequence4)
						{
							PolicyInformation policyInformation = null;
							try
							{
								policyInformation = PolicyInformation.GetInstance(item.ToAsn1Object());
							}
							catch (Exception cause3)
							{
								throw new PkixCertPathValidatorException("Policy information could not be decoded.", cause3, certPath, index);
							}
							if (ANY_POLICY.Equals(policyInformation.PolicyIdentifier.Id))
							{
								try
								{
									policyQualifiers = PkixCertPathValidatorUtilities.GetQualifierSet(policyInformation.PolicyQualifiers);
								}
								catch (PkixCertPathValidatorException cause4)
								{
									throw new PkixCertPathValidatorException("Policy qualifier info set could not be decoded.", cause4, certPath, index);
								}
								break;
							}
						}
						bool critical = false;
						ISet criticalExtensionOids = x509Certificate.GetCriticalExtensionOids();
						if (criticalExtensionOids != null)
						{
							critical = criticalExtensionOids.Contains(X509Extensions.CertificatePolicies.Id);
						}
						PkixPolicyNode parent = pkixPolicyNode3.Parent;
						if (ANY_POLICY.Equals(parent.ValidPolicy))
						{
							PkixPolicyNode pkixPolicyNode4 = new PkixPolicyNode(Platform.CreateArrayList(), num, (ISet)dictionary[text], parent, policyQualifiers, text, critical);
							parent.AddChild(pkixPolicyNode4);
							policyNodes[num].Add(pkixPolicyNode4);
						}
						break;
					}
				}
				else
				{
					if (policyMapping > 0)
					{
						continue;
					}
					foreach (PkixPolicyNode item2 in Platform.CreateArrayList(policyNodes[num]))
					{
						if (!item2.ValidPolicy.Equals(text))
						{
							continue;
						}
						item2.Parent.RemoveChild(item2);
						for (int num2 = num - 1; num2 >= 0; num2--)
						{
							foreach (PkixPolicyNode item3 in Platform.CreateArrayList(policyNodes[num2]))
							{
								if (!item3.HasChildren)
								{
									pkixPolicyNode = PkixCertPathValidatorUtilities.RemovePolicyNode(pkixPolicyNode, policyNodes, item3);
									if (pkixPolicyNode == null)
									{
										break;
									}
								}
							}
						}
					}
				}
			}
		}
		return pkixPolicyNode;
	}

	internal static ISet[] ProcessCrlA1ii(DateTime currentDate, PkixParameters paramsPKIX, X509Certificate cert, X509Crl crl)
	{
		ISet set = new HashSet();
		X509CrlStoreSelector x509CrlStoreSelector = new X509CrlStoreSelector();
		x509CrlStoreSelector.CertificateChecking = cert;
		try
		{
			IList list = Platform.CreateArrayList();
			list.Add(crl.IssuerDN);
			x509CrlStoreSelector.Issuers = list;
		}
		catch (IOException ex)
		{
			throw new Exception("Cannot extract issuer from CRL." + ex, ex);
		}
		x509CrlStoreSelector.CompleteCrlEnabled = true;
		ISet set2 = CrlUtilities.FindCrls(x509CrlStoreSelector, paramsPKIX, currentDate);
		if (paramsPKIX.IsUseDeltasEnabled)
		{
			try
			{
				set.AddAll(PkixCertPathValidatorUtilities.GetDeltaCrls(currentDate, paramsPKIX, crl));
			}
			catch (Exception innerException)
			{
				throw new Exception("Exception obtaining delta CRLs.", innerException);
			}
		}
		return new ISet[2] { set2, set };
	}

	internal static ISet ProcessCrlA1i(DateTime currentDate, PkixParameters paramsPKIX, X509Certificate cert, X509Crl crl)
	{
		ISet set = new HashSet();
		if (paramsPKIX.IsUseDeltasEnabled)
		{
			CrlDistPoint crlDistPoint = null;
			try
			{
				crlDistPoint = CrlDistPoint.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(cert, X509Extensions.FreshestCrl));
			}
			catch (Exception innerException)
			{
				throw new Exception("Freshest CRL extension could not be decoded from certificate.", innerException);
			}
			if (crlDistPoint == null)
			{
				try
				{
					crlDistPoint = CrlDistPoint.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(crl, X509Extensions.FreshestCrl));
				}
				catch (Exception innerException2)
				{
					throw new Exception("Freshest CRL extension could not be decoded from CRL.", innerException2);
				}
			}
			if (crlDistPoint != null)
			{
				try
				{
					PkixCertPathValidatorUtilities.AddAdditionalStoresFromCrlDistributionPoint(crlDistPoint, paramsPKIX);
				}
				catch (Exception innerException3)
				{
					throw new Exception("No new delta CRL locations could be added from Freshest CRL extension.", innerException3);
				}
				try
				{
					set.AddAll(PkixCertPathValidatorUtilities.GetDeltaCrls(currentDate, paramsPKIX, crl));
					return set;
				}
				catch (Exception innerException4)
				{
					throw new Exception("Exception obtaining delta CRLs.", innerException4);
				}
			}
		}
		return set;
	}

	internal static void ProcessCertF(PkixCertPath certPath, int index, PkixPolicyNode validPolicyTree, int explicitPolicy)
	{
		if (explicitPolicy <= 0 && validPolicyTree == null)
		{
			throw new PkixCertPathValidatorException("No valid policy tree found when one expected.", null, certPath, index);
		}
	}

	internal static void ProcessCertA(PkixCertPath certPath, PkixParameters paramsPKIX, int index, AsymmetricKeyParameter workingPublicKey, X509Name workingIssuerName, X509Certificate sign)
	{
		IList certificates = certPath.Certificates;
		X509Certificate x509Certificate = (X509Certificate)certificates[index];
		try
		{
			x509Certificate.Verify(workingPublicKey);
		}
		catch (GeneralSecurityException cause)
		{
			throw new PkixCertPathValidatorException("Could not validate certificate signature.", cause, certPath, index);
		}
		try
		{
			x509Certificate.CheckValidity(PkixCertPathValidatorUtilities.GetValidCertDateFromValidityModel(paramsPKIX, certPath, index));
		}
		catch (CertificateExpiredException ex)
		{
			throw new PkixCertPathValidatorException("Could not validate certificate: " + ex.Message, ex, certPath, index);
		}
		catch (CertificateNotYetValidException ex2)
		{
			throw new PkixCertPathValidatorException("Could not validate certificate: " + ex2.Message, ex2, certPath, index);
		}
		catch (Exception cause2)
		{
			throw new PkixCertPathValidatorException("Could not validate time of certificate.", cause2, certPath, index);
		}
		if (paramsPKIX.IsRevocationEnabled)
		{
			try
			{
				CheckCrls(paramsPKIX, x509Certificate, PkixCertPathValidatorUtilities.GetValidCertDateFromValidityModel(paramsPKIX, certPath, index), sign, workingPublicKey, certificates);
			}
			catch (Exception ex3)
			{
				Exception ex4 = ex3.InnerException;
				if (ex4 == null)
				{
					ex4 = ex3;
				}
				throw new PkixCertPathValidatorException(ex3.Message, ex4, certPath, index);
			}
		}
		X509Name issuerPrincipal = PkixCertPathValidatorUtilities.GetIssuerPrincipal(x509Certificate);
		if (!issuerPrincipal.Equivalent(workingIssuerName, inOrder: true))
		{
			throw new PkixCertPathValidatorException(string.Concat("IssuerName(", issuerPrincipal, ") does not match SubjectName(", workingIssuerName, ") of signing certificate."), null, certPath, index);
		}
	}

	internal static int PrepareNextCertI1(PkixCertPath certPath, int index, int explicitPolicy)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		Asn1Sequence asn1Sequence = null;
		try
		{
			asn1Sequence = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.PolicyConstraints));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Policy constraints extension cannot be decoded.", cause, certPath, index);
		}
		if (asn1Sequence != null)
		{
			IEnumerator enumerator = asn1Sequence.GetEnumerator();
			while (enumerator.MoveNext())
			{
				try
				{
					Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(enumerator.Current);
					if (instance.TagNo == 0)
					{
						int intValueExact = DerInteger.GetInstance(instance, isExplicit: false).IntValueExact;
						if (intValueExact < explicitPolicy)
						{
							return intValueExact;
						}
						return explicitPolicy;
					}
				}
				catch (ArgumentException cause2)
				{
					throw new PkixCertPathValidatorException("Policy constraints extension contents cannot be decoded.", cause2, certPath, index);
				}
			}
		}
		return explicitPolicy;
	}

	internal static int PrepareNextCertI2(PkixCertPath certPath, int index, int policyMapping)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		Asn1Sequence asn1Sequence = null;
		try
		{
			asn1Sequence = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.PolicyConstraints));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Policy constraints extension cannot be decoded.", cause, certPath, index);
		}
		if (asn1Sequence != null)
		{
			IEnumerator enumerator = asn1Sequence.GetEnumerator();
			while (enumerator.MoveNext())
			{
				try
				{
					Asn1TaggedObject instance = Asn1TaggedObject.GetInstance(enumerator.Current);
					if (instance.TagNo == 1)
					{
						int intValueExact = DerInteger.GetInstance(instance, isExplicit: false).IntValueExact;
						if (intValueExact < policyMapping)
						{
							return intValueExact;
						}
						return policyMapping;
					}
				}
				catch (ArgumentException cause2)
				{
					throw new PkixCertPathValidatorException("Policy constraints extension contents cannot be decoded.", cause2, certPath, index);
				}
			}
		}
		return policyMapping;
	}

	internal static void PrepareNextCertG(PkixCertPath certPath, int index, PkixNameConstraintValidator nameConstraintValidator)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		NameConstraints nameConstraints = null;
		try
		{
			Asn1Sequence instance = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.NameConstraints));
			if (instance != null)
			{
				nameConstraints = new NameConstraints(instance);
			}
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Name constraints extension could not be decoded.", cause, certPath, index);
		}
		if (nameConstraints == null)
		{
			return;
		}
		Asn1Sequence permittedSubtrees = nameConstraints.PermittedSubtrees;
		if (permittedSubtrees != null)
		{
			try
			{
				nameConstraintValidator.IntersectPermittedSubtree(permittedSubtrees);
			}
			catch (Exception cause2)
			{
				throw new PkixCertPathValidatorException("Permitted subtrees cannot be build from name constraints extension.", cause2, certPath, index);
			}
		}
		Asn1Sequence excludedSubtrees = nameConstraints.ExcludedSubtrees;
		if (excludedSubtrees == null)
		{
			return;
		}
		IEnumerator enumerator = excludedSubtrees.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GeneralSubtree instance2 = GeneralSubtree.GetInstance(enumerator.Current);
				nameConstraintValidator.AddExcludedSubtree(instance2);
			}
		}
		catch (Exception cause3)
		{
			throw new PkixCertPathValidatorException("Excluded subtrees cannot be build from name constraints extension.", cause3, certPath, index);
		}
	}

	internal static int PrepareNextCertJ(PkixCertPath certPath, int index, int inhibitAnyPolicy)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		DerInteger derInteger = null;
		try
		{
			derInteger = DerInteger.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.InhibitAnyPolicy));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Inhibit any-policy extension cannot be decoded.", cause, certPath, index);
		}
		if (derInteger != null)
		{
			int intValueExact = derInteger.IntValueExact;
			if (intValueExact < inhibitAnyPolicy)
			{
				return intValueExact;
			}
		}
		return inhibitAnyPolicy;
	}

	internal static void PrepareNextCertK(PkixCertPath certPath, int index)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		BasicConstraints basicConstraints = null;
		try
		{
			basicConstraints = BasicConstraints.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.BasicConstraints));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Basic constraints extension cannot be decoded.", cause, certPath, index);
		}
		if (basicConstraints != null)
		{
			if (!basicConstraints.IsCA())
			{
				throw new PkixCertPathValidatorException("Not a CA certificate");
			}
			return;
		}
		throw new PkixCertPathValidatorException("Intermediate certificate lacks BasicConstraints");
	}

	internal static int PrepareNextCertL(PkixCertPath certPath, int index, int maxPathLength)
	{
		IList certificates = certPath.Certificates;
		X509Certificate cert = (X509Certificate)certificates[index];
		if (!PkixCertPathValidatorUtilities.IsSelfIssued(cert))
		{
			if (maxPathLength <= 0)
			{
				throw new PkixCertPathValidatorException("Max path length not greater than zero", null, certPath, index);
			}
			return maxPathLength - 1;
		}
		return maxPathLength;
	}

	internal static int PrepareNextCertM(PkixCertPath certPath, int index, int maxPathLength)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		BasicConstraints basicConstraints = null;
		try
		{
			basicConstraints = BasicConstraints.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.BasicConstraints));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Basic constraints extension cannot be decoded.", cause, certPath, index);
		}
		if (basicConstraints != null)
		{
			BigInteger pathLenConstraint = basicConstraints.PathLenConstraint;
			if (pathLenConstraint != null)
			{
				int intValue = pathLenConstraint.IntValue;
				if (intValue < maxPathLength)
				{
					return intValue;
				}
			}
		}
		return maxPathLength;
	}

	internal static void PrepareNextCertN(PkixCertPath certPath, int index)
	{
		IList certificates = certPath.Certificates;
		X509Certificate x509Certificate = (X509Certificate)certificates[index];
		bool[] keyUsage = x509Certificate.GetKeyUsage();
		if (keyUsage != null && !keyUsage[KEY_CERT_SIGN])
		{
			throw new PkixCertPathValidatorException("Issuer certificate keyusage extension is critical and does not permit key signing.", null, certPath, index);
		}
	}

	internal static void PrepareNextCertO(PkixCertPath certPath, int index, ISet criticalExtensions, IList pathCheckers)
	{
		IList certificates = certPath.Certificates;
		X509Certificate cert = (X509Certificate)certificates[index];
		IEnumerator enumerator = pathCheckers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			try
			{
				((PkixCertPathChecker)enumerator.Current).Check(cert, criticalExtensions);
			}
			catch (PkixCertPathValidatorException ex)
			{
				throw new PkixCertPathValidatorException(ex.Message, ex.InnerException, certPath, index);
			}
		}
		if (!criticalExtensions.IsEmpty)
		{
			throw new PkixCertPathValidatorException("Certificate has unsupported critical extension.", null, certPath, index);
		}
	}

	internal static int PrepareNextCertH1(PkixCertPath certPath, int index, int explicitPolicy)
	{
		IList certificates = certPath.Certificates;
		X509Certificate cert = (X509Certificate)certificates[index];
		if (!PkixCertPathValidatorUtilities.IsSelfIssued(cert) && explicitPolicy != 0)
		{
			return explicitPolicy - 1;
		}
		return explicitPolicy;
	}

	internal static int PrepareNextCertH2(PkixCertPath certPath, int index, int policyMapping)
	{
		IList certificates = certPath.Certificates;
		X509Certificate cert = (X509Certificate)certificates[index];
		if (!PkixCertPathValidatorUtilities.IsSelfIssued(cert) && policyMapping != 0)
		{
			return policyMapping - 1;
		}
		return policyMapping;
	}

	internal static int PrepareNextCertH3(PkixCertPath certPath, int index, int inhibitAnyPolicy)
	{
		IList certificates = certPath.Certificates;
		X509Certificate cert = (X509Certificate)certificates[index];
		if (!PkixCertPathValidatorUtilities.IsSelfIssued(cert) && inhibitAnyPolicy != 0)
		{
			return inhibitAnyPolicy - 1;
		}
		return inhibitAnyPolicy;
	}

	internal static int WrapupCertA(int explicitPolicy, X509Certificate cert)
	{
		if (!PkixCertPathValidatorUtilities.IsSelfIssued(cert) && explicitPolicy != 0)
		{
			explicitPolicy--;
		}
		return explicitPolicy;
	}

	internal static int WrapupCertB(PkixCertPath certPath, int index, int explicitPolicy)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		Asn1Sequence asn1Sequence = null;
		try
		{
			asn1Sequence = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.PolicyConstraints));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Policy constraints could not be decoded.", cause, certPath, index);
		}
		if (asn1Sequence != null)
		{
			IEnumerator enumerator = asn1Sequence.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Asn1TaggedObject asn1TaggedObject = (Asn1TaggedObject)enumerator.Current;
				if (asn1TaggedObject.TagNo == 0)
				{
					int intValueExact;
					try
					{
						intValueExact = DerInteger.GetInstance(asn1TaggedObject, isExplicit: false).IntValueExact;
					}
					catch (Exception cause2)
					{
						throw new PkixCertPathValidatorException("Policy constraints requireExplicitPolicy field could not be decoded.", cause2, certPath, index);
					}
					if (intValueExact == 0)
					{
						return 0;
					}
				}
			}
		}
		return explicitPolicy;
	}

	internal static void WrapupCertF(PkixCertPath certPath, int index, IList pathCheckers, ISet criticalExtensions)
	{
		IList certificates = certPath.Certificates;
		X509Certificate cert = (X509Certificate)certificates[index];
		IEnumerator enumerator = pathCheckers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			try
			{
				((PkixCertPathChecker)enumerator.Current).Check(cert, criticalExtensions);
			}
			catch (PkixCertPathValidatorException cause)
			{
				throw new PkixCertPathValidatorException("Additional certificate path checker failed.", cause, certPath, index);
			}
		}
		if (!criticalExtensions.IsEmpty)
		{
			throw new PkixCertPathValidatorException("Certificate has unsupported critical extension", null, certPath, index);
		}
	}

	internal static PkixPolicyNode WrapupCertG(PkixCertPath certPath, PkixParameters paramsPKIX, ISet userInitialPolicySet, int index, IList[] policyNodes, PkixPolicyNode validPolicyTree, ISet acceptablePolicies)
	{
		int count = certPath.Certificates.Count;
		if (validPolicyTree == null)
		{
			if (paramsPKIX.IsExplicitPolicyRequired)
			{
				throw new PkixCertPathValidatorException("Explicit policy requested but none available.", null, certPath, index);
			}
			return null;
		}
		if (PkixCertPathValidatorUtilities.IsAnyPolicy(userInitialPolicySet))
		{
			if (paramsPKIX.IsExplicitPolicyRequired)
			{
				if (acceptablePolicies.IsEmpty)
				{
					throw new PkixCertPathValidatorException("Explicit policy requested but none available.", null, certPath, index);
				}
				ISet set = new HashSet();
				foreach (IList list in policyNodes)
				{
					for (int j = 0; j < list.Count; j++)
					{
						PkixPolicyNode pkixPolicyNode = (PkixPolicyNode)list[j];
						if (!ANY_POLICY.Equals(pkixPolicyNode.ValidPolicy))
						{
							continue;
						}
						foreach (object child in pkixPolicyNode.Children)
						{
							set.Add(child);
						}
					}
				}
				foreach (PkixPolicyNode item in set)
				{
					string validPolicy = item.ValidPolicy;
					acceptablePolicies.Contains(validPolicy);
				}
				if (validPolicyTree != null)
				{
					for (int num = count - 1; num >= 0; num--)
					{
						IList list2 = policyNodes[num];
						for (int k = 0; k < list2.Count; k++)
						{
							PkixPolicyNode pkixPolicyNode3 = (PkixPolicyNode)list2[k];
							if (!pkixPolicyNode3.HasChildren)
							{
								validPolicyTree = PkixCertPathValidatorUtilities.RemovePolicyNode(validPolicyTree, policyNodes, pkixPolicyNode3);
							}
						}
					}
				}
			}
			return validPolicyTree;
		}
		ISet set2 = new HashSet();
		foreach (IList list3 in policyNodes)
		{
			for (int m = 0; m < list3.Count; m++)
			{
				PkixPolicyNode pkixPolicyNode4 = (PkixPolicyNode)list3[m];
				if (!ANY_POLICY.Equals(pkixPolicyNode4.ValidPolicy))
				{
					continue;
				}
				foreach (PkixPolicyNode child2 in pkixPolicyNode4.Children)
				{
					if (!ANY_POLICY.Equals(child2.ValidPolicy))
					{
						set2.Add(child2);
					}
				}
			}
		}
		IEnumerator enumerator2 = set2.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			PkixPolicyNode pkixPolicyNode6 = (PkixPolicyNode)enumerator2.Current;
			string validPolicy2 = pkixPolicyNode6.ValidPolicy;
			if (!userInitialPolicySet.Contains(validPolicy2))
			{
				validPolicyTree = PkixCertPathValidatorUtilities.RemovePolicyNode(validPolicyTree, policyNodes, pkixPolicyNode6);
			}
		}
		if (validPolicyTree != null)
		{
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				IList list4 = policyNodes[num2];
				for (int n = 0; n < list4.Count; n++)
				{
					PkixPolicyNode pkixPolicyNode7 = (PkixPolicyNode)list4[n];
					if (!pkixPolicyNode7.HasChildren)
					{
						validPolicyTree = PkixCertPathValidatorUtilities.RemovePolicyNode(validPolicyTree, policyNodes, pkixPolicyNode7);
					}
				}
			}
		}
		return validPolicyTree;
	}

	internal static void ProcessCrlC(X509Crl deltaCRL, X509Crl completeCRL, PkixParameters pkixParams)
	{
		if (deltaCRL == null)
		{
			return;
		}
		IssuingDistributionPoint issuingDistributionPoint = null;
		try
		{
			issuingDistributionPoint = IssuingDistributionPoint.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(completeCRL, X509Extensions.IssuingDistributionPoint));
		}
		catch (Exception innerException)
		{
			throw new Exception("000 Issuing distribution point extension could not be decoded.", innerException);
		}
		if (pkixParams.IsUseDeltasEnabled)
		{
			if (!deltaCRL.IssuerDN.Equivalent(completeCRL.IssuerDN, inOrder: true))
			{
				throw new Exception("Complete CRL issuer does not match delta CRL issuer.");
			}
			IssuingDistributionPoint issuingDistributionPoint2 = null;
			try
			{
				issuingDistributionPoint2 = IssuingDistributionPoint.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(deltaCRL, X509Extensions.IssuingDistributionPoint));
			}
			catch (Exception innerException2)
			{
				throw new Exception("Issuing distribution point extension from delta CRL could not be decoded.", innerException2);
			}
			if (!object.Equals(issuingDistributionPoint, issuingDistributionPoint2))
			{
				throw new Exception("Issuing distribution point extension from delta CRL and complete CRL does not match.");
			}
			Asn1Object asn1Object = null;
			try
			{
				asn1Object = PkixCertPathValidatorUtilities.GetExtensionValue(completeCRL, X509Extensions.AuthorityKeyIdentifier);
			}
			catch (Exception innerException3)
			{
				throw new Exception("Authority key identifier extension could not be extracted from complete CRL.", innerException3);
			}
			Asn1Object asn1Object2 = null;
			try
			{
				asn1Object2 = PkixCertPathValidatorUtilities.GetExtensionValue(deltaCRL, X509Extensions.AuthorityKeyIdentifier);
			}
			catch (Exception innerException4)
			{
				throw new Exception("Authority key identifier extension could not be extracted from delta CRL.", innerException4);
			}
			if (asn1Object == null)
			{
				throw new Exception("CRL authority key identifier is null.");
			}
			if (asn1Object2 == null)
			{
				throw new Exception("Delta CRL authority key identifier is null.");
			}
			if (!asn1Object.Equals(asn1Object2))
			{
				throw new Exception("Delta CRL authority key identifier does not match complete CRL authority key identifier.");
			}
		}
	}

	internal static void ProcessCrlI(DateTime validDate, X509Crl deltacrl, object cert, CertStatus certStatus, PkixParameters pkixParams)
	{
		if (pkixParams.IsUseDeltasEnabled && deltacrl != null)
		{
			PkixCertPathValidatorUtilities.GetCertStatus(validDate, deltacrl, cert, certStatus);
		}
	}

	internal static void ProcessCrlJ(DateTime validDate, X509Crl completecrl, object cert, CertStatus certStatus)
	{
		if (certStatus.Status == 11)
		{
			PkixCertPathValidatorUtilities.GetCertStatus(validDate, completecrl, cert, certStatus);
		}
	}

	internal static PkixPolicyNode ProcessCertE(PkixCertPath certPath, int index, PkixPolicyNode validPolicyTree)
	{
		IList certificates = certPath.Certificates;
		X509Certificate ext = (X509Certificate)certificates[index];
		Asn1Sequence asn1Sequence = null;
		try
		{
			asn1Sequence = Asn1Sequence.GetInstance(PkixCertPathValidatorUtilities.GetExtensionValue(ext, X509Extensions.CertificatePolicies));
		}
		catch (Exception cause)
		{
			throw new PkixCertPathValidatorException("Could not read certificate policies extension from certificate.", cause, certPath, index);
		}
		if (asn1Sequence == null)
		{
			validPolicyTree = null;
		}
		return validPolicyTree;
	}
}
