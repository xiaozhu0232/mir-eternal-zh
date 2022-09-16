using System;

namespace Org.BouncyCastle.Asn1.X509;

public class X509CertificateStructure : Asn1Encodable
{
	private readonly TbsCertificateStructure tbsCert;

	private readonly AlgorithmIdentifier sigAlgID;

	private readonly DerBitString sig;

	public TbsCertificateStructure TbsCertificate => tbsCert;

	public int Version => tbsCert.Version;

	public DerInteger SerialNumber => tbsCert.SerialNumber;

	public X509Name Issuer => tbsCert.Issuer;

	public Time StartDate => tbsCert.StartDate;

	public Time EndDate => tbsCert.EndDate;

	public X509Name Subject => tbsCert.Subject;

	public SubjectPublicKeyInfo SubjectPublicKeyInfo => tbsCert.SubjectPublicKeyInfo;

	public AlgorithmIdentifier SignatureAlgorithm => sigAlgID;

	public DerBitString Signature => sig;

	public static X509CertificateStructure GetInstance(Asn1TaggedObject obj, bool explicitly)
	{
		return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
	}

	public static X509CertificateStructure GetInstance(object obj)
	{
		if (obj is X509CertificateStructure)
		{
			return (X509CertificateStructure)obj;
		}
		if (obj == null)
		{
			return null;
		}
		return new X509CertificateStructure(Asn1Sequence.GetInstance(obj));
	}

	public X509CertificateStructure(TbsCertificateStructure tbsCert, AlgorithmIdentifier sigAlgID, DerBitString sig)
	{
		if (tbsCert == null)
		{
			throw new ArgumentNullException("tbsCert");
		}
		if (sigAlgID == null)
		{
			throw new ArgumentNullException("sigAlgID");
		}
		if (sig == null)
		{
			throw new ArgumentNullException("sig");
		}
		this.tbsCert = tbsCert;
		this.sigAlgID = sigAlgID;
		this.sig = sig;
	}

	private X509CertificateStructure(Asn1Sequence seq)
	{
		if (seq.Count != 3)
		{
			throw new ArgumentException("sequence wrong size for a certificate", "seq");
		}
		tbsCert = TbsCertificateStructure.GetInstance(seq[0]);
		sigAlgID = AlgorithmIdentifier.GetInstance(seq[1]);
		sig = DerBitString.GetInstance(seq[2]);
	}

	public byte[] GetSignatureOctets()
	{
		return sig.GetOctets();
	}

	public override Asn1Object ToAsn1Object()
	{
		return new DerSequence(tbsCert, sigAlgID, sig);
	}
}
