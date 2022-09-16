using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpEncryptedDataList : PgpObject
{
	private readonly IList list = Platform.CreateArrayList();

	private readonly InputStreamPacket data;

	public PgpEncryptedData this[int index] => (PgpEncryptedData)list[index];

	[Obsolete("Use 'Count' property instead")]
	public int Size => list.Count;

	public int Count => list.Count;

	public bool IsEmpty => list.Count == 0;

	public PgpEncryptedDataList(BcpgInputStream bcpgInput)
	{
		while (bcpgInput.NextPacketTag() == PacketTag.PublicKeyEncryptedSession || bcpgInput.NextPacketTag() == PacketTag.SymmetricKeyEncryptedSessionKey)
		{
			list.Add(bcpgInput.ReadPacket());
		}
		Packet packet = bcpgInput.ReadPacket();
		if (!(packet is InputStreamPacket))
		{
			throw new IOException("unexpected packet in stream: " + packet);
		}
		data = (InputStreamPacket)packet;
		for (int i = 0; i != list.Count; i++)
		{
			if (list[i] is SymmetricKeyEncSessionPacket)
			{
				list[i] = new PgpPbeEncryptedData((SymmetricKeyEncSessionPacket)list[i], data);
			}
			else
			{
				list[i] = new PgpPublicKeyEncryptedData((PublicKeyEncSessionPacket)list[i], data);
			}
		}
	}

	[Obsolete("Use 'object[index]' syntax instead")]
	public object Get(int index)
	{
		return this[index];
	}

	public IEnumerable GetEncryptedDataObjects()
	{
		return new EnumerableProxy(list);
	}
}
