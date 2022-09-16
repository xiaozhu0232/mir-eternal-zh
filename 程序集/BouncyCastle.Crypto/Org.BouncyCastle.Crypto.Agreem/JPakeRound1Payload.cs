using System;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Crypto.Agreement.JPake;

public class JPakeRound1Payload
{
	private readonly string participantId;

	private readonly BigInteger gx1;

	private readonly BigInteger gx2;

	private readonly BigInteger[] knowledgeProofForX1;

	private readonly BigInteger[] knowledgeProofForX2;

	public virtual string ParticipantId => participantId;

	public virtual BigInteger Gx1 => gx1;

	public virtual BigInteger Gx2 => gx2;

	public virtual BigInteger[] KnowledgeProofForX1
	{
		get
		{
			BigInteger[] array = new BigInteger[knowledgeProofForX1.Length];
			Array.Copy(knowledgeProofForX1, array, knowledgeProofForX1.Length);
			return array;
		}
	}

	public virtual BigInteger[] KnowledgeProofForX2
	{
		get
		{
			BigInteger[] array = new BigInteger[knowledgeProofForX2.Length];
			Array.Copy(knowledgeProofForX2, array, knowledgeProofForX2.Length);
			return array;
		}
	}

	public JPakeRound1Payload(string participantId, BigInteger gx1, BigInteger gx2, BigInteger[] knowledgeProofForX1, BigInteger[] knowledgeProofForX2)
	{
		JPakeUtilities.ValidateNotNull(participantId, "participantId");
		JPakeUtilities.ValidateNotNull(gx1, "gx1");
		JPakeUtilities.ValidateNotNull(gx2, "gx2");
		JPakeUtilities.ValidateNotNull(knowledgeProofForX1, "knowledgeProofForX1");
		JPakeUtilities.ValidateNotNull(knowledgeProofForX2, "knowledgeProofForX2");
		this.participantId = participantId;
		this.gx1 = gx1;
		this.gx2 = gx2;
		this.knowledgeProofForX1 = new BigInteger[knowledgeProofForX1.Length];
		Array.Copy(knowledgeProofForX1, this.knowledgeProofForX1, knowledgeProofForX1.Length);
		this.knowledgeProofForX2 = new BigInteger[knowledgeProofForX2.Length];
		Array.Copy(knowledgeProofForX2, this.knowledgeProofForX2, knowledgeProofForX2.Length);
	}
}
