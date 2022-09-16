namespace Org.BouncyCastle.Crypto.Engines;

public class VmpcKsa3Engine : VmpcEngine
{
	public override string AlgorithmName => "VMPC-KSA3";

	protected override void InitKey(byte[] keyBytes, byte[] ivBytes)
	{
		s = 0;
		P = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			P[i] = (byte)i;
		}
		for (int j = 0; j < 768; j++)
		{
			s = P[(s + P[j & 0xFF] + keyBytes[j % keyBytes.Length]) & 0xFF];
			byte b = P[j & 0xFF];
			P[j & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b;
		}
		for (int k = 0; k < 768; k++)
		{
			s = P[(s + P[k & 0xFF] + ivBytes[k % ivBytes.Length]) & 0xFF];
			byte b2 = P[k & 0xFF];
			P[k & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b2;
		}
		for (int l = 0; l < 768; l++)
		{
			s = P[(s + P[l & 0xFF] + keyBytes[l % keyBytes.Length]) & 0xFF];
			byte b3 = P[l & 0xFF];
			P[l & 0xFF] = P[s & 0xFF];
			P[s & 0xFF] = b3;
		}
		n = 0;
	}
}
