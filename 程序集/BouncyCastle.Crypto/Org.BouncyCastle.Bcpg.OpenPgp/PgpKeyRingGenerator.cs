using System;
using System.Collections;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpKeyRingGenerator
{
	private IList keys = Platform.CreateArrayList();

	private string id;

	private SymmetricKeyAlgorithmTag encAlgorithm;

	private HashAlgorithmTag hashAlgorithm;

	private int certificationLevel;

	private byte[] rawPassPhrase;

	private bool useSha1;

	private PgpKeyPair masterKey;

	private PgpSignatureSubpacketVector hashedPacketVector;

	private PgpSignatureSubpacketVector unhashedPacketVector;

	private SecureRandom rand;

	[Obsolete("Use version taking an explicit 'useSha1' parameter instead")]
	public PgpKeyRingGenerator(int certificationLevel, PgpKeyPair masterKey, string id, SymmetricKeyAlgorithmTag encAlgorithm, char[] passPhrase, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, masterKey, id, encAlgorithm, passPhrase, useSha1: false, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpKeyRingGenerator(int certificationLevel, PgpKeyPair masterKey, string id, SymmetricKeyAlgorithmTag encAlgorithm, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, masterKey, id, encAlgorithm, utf8PassPhrase: false, passPhrase, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpKeyRingGenerator(int certificationLevel, PgpKeyPair masterKey, string id, SymmetricKeyAlgorithmTag encAlgorithm, bool utf8PassPhrase, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, masterKey, id, encAlgorithm, PgpUtilities.EncodePassPhrase(passPhrase, utf8PassPhrase), useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpKeyRingGenerator(int certificationLevel, PgpKeyPair masterKey, string id, SymmetricKeyAlgorithmTag encAlgorithm, byte[] rawPassPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
	{
		this.certificationLevel = certificationLevel;
		this.masterKey = masterKey;
		this.id = id;
		this.encAlgorithm = encAlgorithm;
		this.rawPassPhrase = rawPassPhrase;
		this.useSha1 = useSha1;
		hashedPacketVector = hashedPackets;
		unhashedPacketVector = unhashedPackets;
		this.rand = rand;
		keys.Add(new PgpSecretKey(certificationLevel, masterKey, id, encAlgorithm, rawPassPhrase, clearPassPhrase: false, useSha1, hashedPackets, unhashedPackets, rand));
	}

	public PgpKeyRingGenerator(int certificationLevel, PgpKeyPair masterKey, string id, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, masterKey, id, encAlgorithm, hashAlgorithm, utf8PassPhrase: false, passPhrase, useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpKeyRingGenerator(int certificationLevel, PgpKeyPair masterKey, string id, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, bool utf8PassPhrase, char[] passPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
		: this(certificationLevel, masterKey, id, encAlgorithm, hashAlgorithm, PgpUtilities.EncodePassPhrase(passPhrase, utf8PassPhrase), useSha1, hashedPackets, unhashedPackets, rand)
	{
	}

	public PgpKeyRingGenerator(int certificationLevel, PgpKeyPair masterKey, string id, SymmetricKeyAlgorithmTag encAlgorithm, HashAlgorithmTag hashAlgorithm, byte[] rawPassPhrase, bool useSha1, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, SecureRandom rand)
	{
		this.certificationLevel = certificationLevel;
		this.masterKey = masterKey;
		this.id = id;
		this.encAlgorithm = encAlgorithm;
		this.rawPassPhrase = rawPassPhrase;
		this.useSha1 = useSha1;
		hashedPacketVector = hashedPackets;
		unhashedPacketVector = unhashedPackets;
		this.rand = rand;
		this.hashAlgorithm = hashAlgorithm;
		keys.Add(new PgpSecretKey(certificationLevel, masterKey, id, encAlgorithm, hashAlgorithm, rawPassPhrase, clearPassPhrase: false, useSha1, hashedPackets, unhashedPackets, rand));
	}

	public void AddSubKey(PgpKeyPair keyPair)
	{
		AddSubKey(keyPair, hashedPacketVector, unhashedPacketVector);
	}

	public void AddSubKey(PgpKeyPair keyPair, HashAlgorithmTag hashAlgorithm)
	{
		AddSubKey(keyPair, hashedPacketVector, unhashedPacketVector, hashAlgorithm);
	}

	public void AddSubKey(PgpKeyPair keyPair, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets)
	{
		try
		{
			PgpSignatureGenerator pgpSignatureGenerator = new PgpSignatureGenerator(masterKey.PublicKey.Algorithm, HashAlgorithmTag.Sha1);
			pgpSignatureGenerator.InitSign(24, masterKey.PrivateKey);
			pgpSignatureGenerator.SetHashedSubpackets(hashedPackets);
			pgpSignatureGenerator.SetUnhashedSubpackets(unhashedPackets);
			IList list = Platform.CreateArrayList();
			list.Add(pgpSignatureGenerator.GenerateCertification(masterKey.PublicKey, keyPair.PublicKey));
			keys.Add(new PgpSecretKey(keyPair.PrivateKey, new PgpPublicKey(keyPair.PublicKey, null, list), encAlgorithm, rawPassPhrase, clearPassPhrase: false, useSha1, rand, isMasterKey: false));
		}
		catch (PgpException ex)
		{
			throw ex;
		}
		catch (Exception exception)
		{
			throw new PgpException("exception adding subkey: ", exception);
		}
	}

	public void AddSubKey(PgpKeyPair keyPair, PgpSignatureSubpacketVector hashedPackets, PgpSignatureSubpacketVector unhashedPackets, HashAlgorithmTag hashAlgorithm)
	{
		try
		{
			PgpSignatureGenerator pgpSignatureGenerator = new PgpSignatureGenerator(masterKey.PublicKey.Algorithm, hashAlgorithm);
			pgpSignatureGenerator.InitSign(24, masterKey.PrivateKey);
			pgpSignatureGenerator.SetHashedSubpackets(hashedPackets);
			pgpSignatureGenerator.SetUnhashedSubpackets(unhashedPackets);
			IList list = Platform.CreateArrayList();
			list.Add(pgpSignatureGenerator.GenerateCertification(masterKey.PublicKey, keyPair.PublicKey));
			keys.Add(new PgpSecretKey(keyPair.PrivateKey, new PgpPublicKey(keyPair.PublicKey, null, list), encAlgorithm, rawPassPhrase, clearPassPhrase: false, useSha1, rand, isMasterKey: false));
		}
		catch (PgpException)
		{
			throw;
		}
		catch (Exception exception)
		{
			throw new PgpException("exception adding subkey: ", exception);
		}
	}

	public PgpSecretKeyRing GenerateSecretKeyRing()
	{
		return new PgpSecretKeyRing(keys);
	}

	public PgpPublicKeyRing GeneratePublicKeyRing()
	{
		IList list = Platform.CreateArrayList();
		IEnumerator enumerator = keys.GetEnumerator();
		enumerator.MoveNext();
		PgpSecretKey pgpSecretKey = (PgpSecretKey)enumerator.Current;
		list.Add(pgpSecretKey.PublicKey);
		while (enumerator.MoveNext())
		{
			pgpSecretKey = (PgpSecretKey)enumerator.Current;
			PgpPublicKey pgpPublicKey = new PgpPublicKey(pgpSecretKey.PublicKey);
			pgpPublicKey.publicPk = new PublicSubkeyPacket(pgpPublicKey.Algorithm, pgpPublicKey.CreationTime, pgpPublicKey.publicPk.Key);
			list.Add(pgpPublicKey);
		}
		return new PgpPublicKeyRing(list);
	}
}
