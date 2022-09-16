namespace Org.BouncyCastle.Crypto.Tls;

public interface TlsCipher
{
	int GetPlaintextLimit(int ciphertextLimit);

	byte[] EncodePlaintext(long seqNo, byte type, byte[] plaintext, int offset, int len);

	byte[] DecodeCiphertext(long seqNo, byte type, byte[] ciphertext, int offset, int len);
}
