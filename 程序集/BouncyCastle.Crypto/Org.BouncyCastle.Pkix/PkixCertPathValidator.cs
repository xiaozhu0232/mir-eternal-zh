using System;
using System.Collections;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix;

public class PkixCertPathValidator
{
	public virtual PkixCertPathValidatorResult Validate(PkixCertPath certPath, PkixParameters paramsPkix)
	{
		if (paramsPkix.GetTrustAnchors() == null)
		{
			throw new ArgumentException("trustAnchors is null, this is not allowed for certification path validation.", "parameters");
		}
		IList certificates = certPath.Certificates;
		int count = certificates.Count;
		if (certificates.Count == 0)
		{
			throw new PkixCertPathValidatorException("Certification path is empty.", null, certPath, 0);
		}
		ISet initialPolicies = paramsPkix.GetInitialPolicies();
		TrustAnchor trustAnchor;
		try
		{
			trustAnchor = PkixCertPathValidatorUtilities.FindTrustAnchor((X509Certificate)certificates[certificates.Count - 1], paramsPkix.GetTrustAnchors());
			if (trustAnchor == null)
			{
				throw new PkixCertPathValidatorException("Trust anchor for certification path not found.", null, certPath, -1);
			}
			CheckCertificate(trustAnchor.TrustedCert);
		}
		catch (Exception ex)
		{
			throw new PkixCertPathValidatorException(ex.Message, ex.InnerException, certPath, certificates.Count - 1);
		}
		int num = 0;
		IList[] array = new IList[count + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Platform.CreateArrayList();
		}
		ISet set = new HashSet();
		set.Add(Rfc3280CertPathUtilities.ANY_POLICY);
		PkixPolicyNode pkixPolicyNode = new PkixPolicyNode(Platform.CreateArrayList(), 0, set, null, new HashSet(), Rfc3280CertPathUtilities.ANY_POLICY, critical: false);
		array[0].Add(pkixPolicyNode);
		PkixNameConstraintValidator nameConstraintValidator = new PkixNameConstraintValidator();
		ISet acceptablePolicies = new HashSet();
		int explicitPolicy = ((!paramsPkix.IsExplicitPolicyRequired) ? (count + 1) : 0);
		int inhibitAnyPolicy = ((!paramsPkix.IsAnyPolicyInhibited) ? (count + 1) : 0);
		int policyMapping = ((!paramsPkix.IsPolicyMappingInhibited) ? (count + 1) : 0);
		X509Certificate x509Certificate = trustAnchor.TrustedCert;
		X509Name workingIssuerName;
		AsymmetricKeyParameter asymmetricKeyParameter;
		try
		{
			if (x509Certificate != null)
			{
				workingIssuerName = x509Certificate.SubjectDN;
				asymmetricKeyParameter = x509Certificate.GetPublicKey();
			}
			else
			{
				workingIssuerName = new X509Name(trustAnchor.CAName);
				asymmetricKeyParameter = trustAnchor.CAPublicKey;
			}
		}
		catch (ArgumentException cause)
		{
			throw new PkixCertPathValidatorException("Subject of trust anchor could not be (re)encoded.", cause, certPath, -1);
		}
		try
		{
			PkixCertPathValidatorUtilities.GetAlgorithmIdentifier(asymmetricKeyParameter);
		}
		catch (PkixCertPathValidatorException cause2)
		{
			throw new PkixCertPathValidatorException("Algorithm identifier of public key of trust anchor could not be read.", cause2, certPath, -1);
		}
		int maxPathLength = count;
		X509CertStoreSelector targetCertConstraints = paramsPkix.GetTargetCertConstraints();
		if (targetCertConstraints != null && !targetCertConstraints.Match((X509Certificate)certificates[0]))
		{
			throw new PkixCertPathValidatorException("Target certificate in certification path does not match targetConstraints.", null, certPath, 0);
		}
		IList certPathCheckers = paramsPkix.GetCertPathCheckers();
		IEnumerator enumerator = certPathCheckers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			((PkixCertPathChecker)enumerator.Current).Init(forward: false);
		}
		X509Certificate x509Certificate2 = null;
		for (num = certificates.Count - 1; num >= 0; num--)
		{
			int num2 = count - num;
			x509Certificate2 = (X509Certificate)certificates[num];
			try
			{
				CheckCertificate(x509Certificate2);
			}
			catch (Exception ex2)
			{
				throw new PkixCertPathValidatorException(ex2.Message, ex2.InnerException, certPath, num);
			}
			Rfc3280CertPathUtilities.ProcessCertA(certPath, paramsPkix, num, asymmetricKeyParameter, workingIssuerName, x509Certificate);
			Rfc3280CertPathUtilities.ProcessCertBC(certPath, num, nameConstraintValidator);
			pkixPolicyNode = Rfc3280CertPathUtilities.ProcessCertD(certPath, num, acceptablePolicies, pkixPolicyNode, array, inhibitAnyPolicy);
			pkixPolicyNode = Rfc3280CertPathUtilities.ProcessCertE(certPath, num, pkixPolicyNode);
			Rfc3280CertPathUtilities.ProcessCertF(certPath, num, pkixPolicyNode, explicitPolicy);
			if (num2 != count)
			{
				if (x509Certificate2 != null && x509Certificate2.Version == 1)
				{
					if (num2 != 1 || !x509Certificate2.Equals(trustAnchor.TrustedCert))
					{
						throw new PkixCertPathValidatorException("Version 1 certificates can't be used as CA ones.", null, certPath, num);
					}
				}
				else
				{
					Rfc3280CertPathUtilities.PrepareNextCertA(certPath, num);
					pkixPolicyNode = Rfc3280CertPathUtilities.PrepareCertB(certPath, num, array, pkixPolicyNode, policyMapping);
					Rfc3280CertPathUtilities.PrepareNextCertG(certPath, num, nameConstraintValidator);
					explicitPolicy = Rfc3280CertPathUtilities.PrepareNextCertH1(certPath, num, explicitPolicy);
					policyMapping = Rfc3280CertPathUtilities.PrepareNextCertH2(certPath, num, policyMapping);
					inhibitAnyPolicy = Rfc3280CertPathUtilities.PrepareNextCertH3(certPath, num, inhibitAnyPolicy);
					explicitPolicy = Rfc3280CertPathUtilities.PrepareNextCertI1(certPath, num, explicitPolicy);
					policyMapping = Rfc3280CertPathUtilities.PrepareNextCertI2(certPath, num, policyMapping);
					inhibitAnyPolicy = Rfc3280CertPathUtilities.PrepareNextCertJ(certPath, num, inhibitAnyPolicy);
					Rfc3280CertPathUtilities.PrepareNextCertK(certPath, num);
					maxPathLength = Rfc3280CertPathUtilities.PrepareNextCertL(certPath, num, maxPathLength);
					maxPathLength = Rfc3280CertPathUtilities.PrepareNextCertM(certPath, num, maxPathLength);
					Rfc3280CertPathUtilities.PrepareNextCertN(certPath, num);
					ISet criticalExtensionOids = x509Certificate2.GetCriticalExtensionOids();
					if (criticalExtensionOids != null)
					{
						criticalExtensionOids = new HashSet(criticalExtensionOids);
						criticalExtensionOids.Remove(X509Extensions.KeyUsage.Id);
						criticalExtensionOids.Remove(X509Extensions.CertificatePolicies.Id);
						criticalExtensionOids.Remove(X509Extensions.PolicyMappings.Id);
						criticalExtensionOids.Remove(X509Extensions.InhibitAnyPolicy.Id);
						criticalExtensionOids.Remove(X509Extensions.IssuingDistributionPoint.Id);
						criticalExtensionOids.Remove(X509Extensions.DeltaCrlIndicator.Id);
						criticalExtensionOids.Remove(X509Extensions.PolicyConstraints.Id);
						criticalExtensionOids.Remove(X509Extensions.BasicConstraints.Id);
						criticalExtensionOids.Remove(X509Extensions.SubjectAlternativeName.Id);
						criticalExtensionOids.Remove(X509Extensions.NameConstraints.Id);
					}
					else
					{
						criticalExtensionOids = new HashSet();
					}
					Rfc3280CertPathUtilities.PrepareNextCertO(certPath, num, criticalExtensionOids, certPathCheckers);
					x509Certificate = x509Certificate2;
					workingIssuerName = x509Certificate.SubjectDN;
					try
					{
						asymmetricKeyParameter = PkixCertPathValidatorUtilities.GetNextWorkingKey(certPath.Certificates, num);
					}
					catch (PkixCertPathValidatorException cause3)
					{
						throw new PkixCertPathValidatorException("Next working key could not be retrieved.", cause3, certPath, num);
					}
					PkixCertPathValidatorUtilities.GetAlgorithmIdentifier(asymmetricKeyParameter);
				}
			}
		}
		explicitPolicy = Rfc3280CertPathUtilities.WrapupCertA(explicitPolicy, x509Certificate2);
		explicitPolicy = Rfc3280CertPathUtilities.WrapupCertB(certPath, num + 1, explicitPolicy);
		ISet criticalExtensionOids2 = x509Certificate2.GetCriticalExtensionOids();
		if (criticalExtensionOids2 != null)
		{
			criticalExtensionOids2 = new HashSet(criticalExtensionOids2);
			criticalExtensionOids2.Remove(X509Extensions.KeyUsage.Id);
			criticalExtensionOids2.Remove(X509Extensions.CertificatePolicies.Id);
			criticalExtensionOids2.Remove(X509Extensions.PolicyMappings.Id);
			criticalExtensionOids2.Remove(X509Extensions.InhibitAnyPolicy.Id);
			criticalExtensionOids2.Remove(X509Extensions.IssuingDistributionPoint.Id);
			criticalExtensionOids2.Remove(X509Extensions.DeltaCrlIndicator.Id);
			criticalExtensionOids2.Remove(X509Extensions.PolicyConstraints.Id);
			criticalExtensionOids2.Remove(X509Extensions.BasicConstraints.Id);
			criticalExtensionOids2.Remove(X509Extensions.SubjectAlternativeName.Id);
			criticalExtensionOids2.Remove(X509Extensions.NameConstraints.Id);
			criticalExtensionOids2.Remove(X509Extensions.CrlDistributionPoints.Id);
		}
		else
		{
			criticalExtensionOids2 = new HashSet();
		}
		Rfc3280CertPathUtilities.WrapupCertF(certPath, num + 1, certPathCheckers, criticalExtensionOids2);
		PkixPolicyNode pkixPolicyNode2 = Rfc3280CertPathUtilities.WrapupCertG(certPath, paramsPkix, initialPolicies, num + 1, array, pkixPolicyNode, acceptablePolicies);
		if (explicitPolicy > 0 || pkixPolicyNode2 != null)
		{
			return new PkixCertPathValidatorResult(trustAnchor, pkixPolicyNode2, x509Certificate2.GetPublicKey());
		}
		throw new PkixCertPathValidatorException("Path processing failed on policy.", null, certPath, num);
	}

	internal static void CheckCertificate(X509Certificate cert)
	{
		try
		{
			TbsCertificateStructure.GetInstance(cert.CertificateStructure.TbsCertificate);
		}
		catch (CertificateEncodingException innerException)
		{
			throw new Exception("unable to process TBSCertificate", innerException);
		}
	}
}
