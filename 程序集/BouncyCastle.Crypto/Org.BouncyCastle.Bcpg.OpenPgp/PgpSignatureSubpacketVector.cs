using System;
using Org.BouncyCastle.Bcpg.Sig;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpSignatureSubpacketVector
{
	private readonly SignatureSubpacket[] packets;

	[Obsolete("Use 'Count' property instead")]
	public int Size => packets.Length;

	public int Count => packets.Length;

	internal PgpSignatureSubpacketVector(SignatureSubpacket[] packets)
	{
		this.packets = packets;
	}

	public SignatureSubpacket GetSubpacket(SignatureSubpacketTag type)
	{
		for (int i = 0; i != packets.Length; i++)
		{
			if (packets[i].SubpacketType == type)
			{
				return packets[i];
			}
		}
		return null;
	}

	public bool HasSubpacket(SignatureSubpacketTag type)
	{
		return GetSubpacket(type) != null;
	}

	public SignatureSubpacket[] GetSubpackets(SignatureSubpacketTag type)
	{
		int num = 0;
		for (int i = 0; i < packets.Length; i++)
		{
			if (packets[i].SubpacketType == type)
			{
				num++;
			}
		}
		SignatureSubpacket[] array = new SignatureSubpacket[num];
		int num2 = 0;
		for (int j = 0; j < packets.Length; j++)
		{
			if (packets[j].SubpacketType == type)
			{
				array[num2++] = packets[j];
			}
		}
		return array;
	}

	public NotationData[] GetNotationDataOccurrences()
	{
		SignatureSubpacket[] subpackets = GetSubpackets(SignatureSubpacketTag.NotationData);
		NotationData[] array = new NotationData[subpackets.Length];
		for (int i = 0; i < subpackets.Length; i++)
		{
			array[i] = (NotationData)subpackets[i];
		}
		return array;
	}

	[Obsolete("Use 'GetNotationDataOccurrences' instead")]
	public NotationData[] GetNotationDataOccurences()
	{
		return GetNotationDataOccurrences();
	}

	public long GetIssuerKeyId()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.IssuerKeyId);
		if (subpacket != null)
		{
			return ((IssuerKeyId)subpacket).KeyId;
		}
		return 0L;
	}

	public bool HasSignatureCreationTime()
	{
		return GetSubpacket(SignatureSubpacketTag.CreationTime) != null;
	}

	public DateTime GetSignatureCreationTime()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.CreationTime);
		if (subpacket == null)
		{
			throw new PgpException("SignatureCreationTime not available");
		}
		return ((SignatureCreationTime)subpacket).GetTime();
	}

	public long GetSignatureExpirationTime()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.ExpireTime);
		if (subpacket != null)
		{
			return ((SignatureExpirationTime)subpacket).Time;
		}
		return 0L;
	}

	public long GetKeyExpirationTime()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.KeyExpireTime);
		if (subpacket != null)
		{
			return ((KeyExpirationTime)subpacket).Time;
		}
		return 0L;
	}

	public int[] GetPreferredHashAlgorithms()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.PreferredHashAlgorithms);
		if (subpacket != null)
		{
			return ((PreferredAlgorithms)subpacket).GetPreferences();
		}
		return null;
	}

	public int[] GetPreferredSymmetricAlgorithms()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.PreferredSymmetricAlgorithms);
		if (subpacket != null)
		{
			return ((PreferredAlgorithms)subpacket).GetPreferences();
		}
		return null;
	}

	public int[] GetPreferredCompressionAlgorithms()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.PreferredCompressionAlgorithms);
		if (subpacket != null)
		{
			return ((PreferredAlgorithms)subpacket).GetPreferences();
		}
		return null;
	}

	public int GetKeyFlags()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.KeyFlags);
		if (subpacket != null)
		{
			return ((KeyFlags)subpacket).Flags;
		}
		return 0;
	}

	public string GetSignerUserId()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.SignerUserId);
		if (subpacket != null)
		{
			return ((SignerUserId)subpacket).GetId();
		}
		return null;
	}

	public bool IsPrimaryUserId()
	{
		return ((PrimaryUserId)GetSubpacket(SignatureSubpacketTag.PrimaryUserId))?.IsPrimaryUserId() ?? false;
	}

	public SignatureSubpacketTag[] GetCriticalTags()
	{
		int num = 0;
		for (int i = 0; i != packets.Length; i++)
		{
			if (packets[i].IsCritical())
			{
				num++;
			}
		}
		SignatureSubpacketTag[] array = new SignatureSubpacketTag[num];
		num = 0;
		for (int j = 0; j != packets.Length; j++)
		{
			if (packets[j].IsCritical())
			{
				array[num++] = packets[j].SubpacketType;
			}
		}
		return array;
	}

	public Features GetFeatures()
	{
		SignatureSubpacket subpacket = GetSubpacket(SignatureSubpacketTag.Features);
		if (subpacket == null)
		{
			return null;
		}
		return new Features(subpacket.IsCritical(), subpacket.IsLongLength(), subpacket.GetData());
	}

	internal SignatureSubpacket[] ToSubpacketArray()
	{
		return packets;
	}
}
