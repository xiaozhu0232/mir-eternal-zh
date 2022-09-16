using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Agreement.JPake;

public class JPakeRound2Payload
{
	private readonly string participantId;

	private readonly BigInteger a;

	private readonly BigInteger[] knowledgeProofForX2s;

	public virtual string ParticipantId => participantId;

	public virtual BigInteger A => a;

	public virtual BigInteger[] KnowledgeProofForX2s
	{
		get
		{
			BigInteger[] array = new BigInteger[knowledgeProofForX2s.Length];
			Array.Copy(knowledgeProofForX2s, array, knowledgeProofForX2s.Length);
			return array;
		}
	}

	public JPakeRound2Payload(string participantId, BigInteger a, BigInteger[] knowledgeProofForX2s)
	{
		JPakeUtilities.ValidateNotNull(participantId, "participantId");
		JPakeUtilities.ValidateNotNull(a, "a");
		JPakeUtilities.ValidateNotNull(knowledgeProofForX2s, "knowledgeProofForX2s");
		this.participantId = participantId;
		this.a = a;
		this.knowledgeProofForX2s = new BigInteger[knowledgeProofForX2s.Length];
		knowledgeProofForX2s.CopyTo(this.knowledgeProofForX2s, 0);
	}
}
