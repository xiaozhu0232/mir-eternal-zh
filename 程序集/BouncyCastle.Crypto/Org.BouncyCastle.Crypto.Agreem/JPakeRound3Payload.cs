using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Agreement.JPake;

public class JPakeRound3Payload
{
	private readonly string participantId;

	private readonly BigInteger macTag;

	public virtual string ParticipantId => participantId;

	public virtual BigInteger MacTag => macTag;

	public JPakeRound3Payload(string participantId, BigInteger magTag)
	{
		this.participantId = participantId;
		macTag = magTag;
	}
}
