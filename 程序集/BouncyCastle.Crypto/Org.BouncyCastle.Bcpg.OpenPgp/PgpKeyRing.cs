using System.Collections;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public abstract class PgpKeyRing : PgpObject
{
	internal PgpKeyRing()
	{
	}

	internal static TrustPacket ReadOptionalTrustPacket(BcpgInputStream bcpgInput)
	{
		if (bcpgInput.NextPacketTag() != PacketTag.Trust)
		{
			return null;
		}
		return (TrustPacket)bcpgInput.ReadPacket();
	}

	internal static IList ReadSignaturesAndTrust(BcpgInputStream bcpgInput)
	{
		try
		{
			IList list = Platform.CreateArrayList();
			while (bcpgInput.NextPacketTag() == PacketTag.Signature)
			{
				SignaturePacket sigPacket = (SignaturePacket)bcpgInput.ReadPacket();
				TrustPacket trustPacket = ReadOptionalTrustPacket(bcpgInput);
				list.Add(new PgpSignature(sigPacket, trustPacket));
			}
			return list;
		}
		catch (PgpException ex)
		{
			throw new IOException("can't create signature object: " + ex.Message, ex);
		}
	}

	internal static void ReadUserIDs(BcpgInputStream bcpgInput, out IList ids, out IList idTrusts, out IList idSigs)
	{
		ids = Platform.CreateArrayList();
		idTrusts = Platform.CreateArrayList();
		idSigs = Platform.CreateArrayList();
		while (bcpgInput.NextPacketTag() == PacketTag.UserId || bcpgInput.NextPacketTag() == PacketTag.UserAttribute)
		{
			Packet packet = bcpgInput.ReadPacket();
			if (packet is UserIdPacket)
			{
				UserIdPacket userIdPacket = (UserIdPacket)packet;
				ids.Add(userIdPacket.GetId());
			}
			else
			{
				UserAttributePacket userAttributePacket = (UserAttributePacket)packet;
				ids.Add(new PgpUserAttributeSubpacketVector(userAttributePacket.GetSubpackets()));
			}
			idTrusts.Add(ReadOptionalTrustPacket(bcpgInput));
			idSigs.Add(ReadSignaturesAndTrust(bcpgInput));
		}
	}
}
