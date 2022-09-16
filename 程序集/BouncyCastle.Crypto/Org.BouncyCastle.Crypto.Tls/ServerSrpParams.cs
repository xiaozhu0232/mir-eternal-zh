using System.IO;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class ServerSrpParams
{
	protected BigInteger m_N;

	protected BigInteger m_g;

	protected BigInteger m_B;

	protected byte[] m_s;

	public virtual BigInteger B => m_B;

	public virtual BigInteger G => m_g;

	public virtual BigInteger N => m_N;

	public virtual byte[] S => m_s;

	public ServerSrpParams(BigInteger N, BigInteger g, byte[] s, BigInteger B)
	{
		m_N = N;
		m_g = g;
		m_s = Arrays.Clone(s);
		m_B = B;
	}

	public virtual void Encode(Stream output)
	{
		TlsSrpUtilities.WriteSrpParameter(m_N, output);
		TlsSrpUtilities.WriteSrpParameter(m_g, output);
		TlsUtilities.WriteOpaque8(m_s, output);
		TlsSrpUtilities.WriteSrpParameter(m_B, output);
	}

	public static ServerSrpParams Parse(Stream input)
	{
		BigInteger n = TlsSrpUtilities.ReadSrpParameter(input);
		BigInteger g = TlsSrpUtilities.ReadSrpParameter(input);
		byte[] s = TlsUtilities.ReadOpaque8(input);
		BigInteger b = TlsSrpUtilities.ReadSrpParameter(input);
		return new ServerSrpParams(n, g, s, b);
	}
}
