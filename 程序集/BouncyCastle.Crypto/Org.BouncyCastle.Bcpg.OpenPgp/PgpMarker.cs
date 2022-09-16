using System.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpMarker : PgpObject
{
	private readonly MarkerPacket data;

	public PgpMarker(BcpgInputStream bcpgInput)
	{
		Packet packet = bcpgInput.ReadPacket();
		if (!(packet is MarkerPacket))
		{
			throw new IOException("unexpected packet in stream: " + packet);
		}
		data = (MarkerPacket)packet;
	}
}
