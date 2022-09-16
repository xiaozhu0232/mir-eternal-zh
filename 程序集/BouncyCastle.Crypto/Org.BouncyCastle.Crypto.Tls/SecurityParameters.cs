using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class SecurityParameters
{
	internal int entity = -1;

	internal int cipherSuite = -1;

	internal byte compressionAlgorithm = 0;

	internal int prfAlgorithm = -1;

	internal int verifyDataLength = -1;

	internal byte[] masterSecret = null;

	internal byte[] clientRandom = null;

	internal byte[] serverRandom = null;

	internal byte[] sessionHash = null;

	internal byte[] pskIdentity = null;

	internal byte[] srpIdentity = null;

	internal short maxFragmentLength = -1;

	internal bool truncatedHMac = false;

	internal bool encryptThenMac = false;

	internal bool extendedMasterSecret = false;

	public virtual int Entity => entity;

	public virtual int CipherSuite => cipherSuite;

	public virtual byte CompressionAlgorithm => compressionAlgorithm;

	public virtual int PrfAlgorithm => prfAlgorithm;

	public virtual int VerifyDataLength => verifyDataLength;

	public virtual byte[] MasterSecret => masterSecret;

	public virtual byte[] ClientRandom => clientRandom;

	public virtual byte[] ServerRandom => serverRandom;

	public virtual byte[] SessionHash => sessionHash;

	public virtual byte[] PskIdentity => pskIdentity;

	public virtual byte[] SrpIdentity => srpIdentity;

	public virtual bool IsExtendedMasterSecret => extendedMasterSecret;

	internal virtual void Clear()
	{
		if (masterSecret != null)
		{
			Arrays.Fill(masterSecret, 0);
			masterSecret = null;
		}
	}
}
