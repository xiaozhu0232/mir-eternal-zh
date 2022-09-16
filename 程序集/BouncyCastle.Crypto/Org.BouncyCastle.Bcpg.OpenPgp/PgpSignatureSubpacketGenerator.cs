using System;
using System.Collections;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpSignatureSubpacketGenerator
{
	private IList list = Platform.CreateArrayList();

	public void SetRevocable(bool isCritical, bool isRevocable)
	{
		list.Add(new Revocable(isCritical, isRevocable));
	}

	public void SetExportable(bool isCritical, bool isExportable)
	{
		list.Add(new Exportable(isCritical, isExportable));
	}

	public void SetFeature(bool isCritical, byte feature)
	{
		list.Add(new Features(isCritical, feature));
	}

	public void SetTrust(bool isCritical, int depth, int trustAmount)
	{
		list.Add(new TrustSignature(isCritical, depth, trustAmount));
	}

	public void SetKeyExpirationTime(bool isCritical, long seconds)
	{
		list.Add(new KeyExpirationTime(isCritical, seconds));
	}

	public void SetSignatureExpirationTime(bool isCritical, long seconds)
	{
		list.Add(new SignatureExpirationTime(isCritical, seconds));
	}

	public void SetSignatureCreationTime(bool isCritical, DateTime date)
	{
		list.Add(new SignatureCreationTime(isCritical, date));
	}

	public void SetPreferredHashAlgorithms(bool isCritical, int[] algorithms)
	{
		list.Add(new PreferredAlgorithms(SignatureSubpacketTag.PreferredHashAlgorithms, isCritical, algorithms));
	}

	public void SetPreferredSymmetricAlgorithms(bool isCritical, int[] algorithms)
	{
		list.Add(new PreferredAlgorithms(SignatureSubpacketTag.PreferredSymmetricAlgorithms, isCritical, algorithms));
	}

	public void SetPreferredCompressionAlgorithms(bool isCritical, int[] algorithms)
	{
		list.Add(new PreferredAlgorithms(SignatureSubpacketTag.PreferredCompressionAlgorithms, isCritical, algorithms));
	}

	public void SetKeyFlags(bool isCritical, int flags)
	{
		list.Add(new KeyFlags(isCritical, flags));
	}

	public void SetSignerUserId(bool isCritical, string userId)
	{
		if (userId == null)
		{
			throw new ArgumentNullException("userId");
		}
		list.Add(new SignerUserId(isCritical, userId));
	}

	public void SetSignerUserId(bool isCritical, byte[] rawUserId)
	{
		if (rawUserId == null)
		{
			throw new ArgumentNullException("rawUserId");
		}
		list.Add(new SignerUserId(isCritical, isLongLength: false, rawUserId));
	}

	public void SetEmbeddedSignature(bool isCritical, PgpSignature pgpSignature)
	{
		byte[] encoded = pgpSignature.GetEncoded();
		byte[] array = ((encoded.Length - 1 <= 256) ? new byte[encoded.Length - 2] : new byte[encoded.Length - 3]);
		Array.Copy(encoded, encoded.Length - array.Length, array, 0, array.Length);
		list.Add(new EmbeddedSignature(isCritical, isLongLength: false, array));
	}

	public void SetPrimaryUserId(bool isCritical, bool isPrimaryUserId)
	{
		list.Add(new PrimaryUserId(isCritical, isPrimaryUserId));
	}

	public void SetNotationData(bool isCritical, bool isHumanReadable, string notationName, string notationValue)
	{
		list.Add(new NotationData(isCritical, isHumanReadable, notationName, notationValue));
	}

	public void SetRevocationReason(bool isCritical, RevocationReasonTag reason, string description)
	{
		list.Add(new RevocationReason(isCritical, reason, description));
	}

	public void SetRevocationKey(bool isCritical, PublicKeyAlgorithmTag keyAlgorithm, byte[] fingerprint)
	{
		list.Add(new RevocationKey(isCritical, RevocationKeyTag.ClassDefault, keyAlgorithm, fingerprint));
	}

	public void SetIssuerKeyID(bool isCritical, long keyID)
	{
		list.Add(new IssuerKeyId(isCritical, keyID));
	}

	public PgpSignatureSubpacketVector Generate()
	{
		SignatureSubpacket[] array = new SignatureSubpacket[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = (SignatureSubpacket)list[i];
		}
		return new PgpSignatureSubpacketVector(array);
	}
}
