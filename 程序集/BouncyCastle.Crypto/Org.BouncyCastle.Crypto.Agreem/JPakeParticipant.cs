using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Agreement.JPake;

public class JPakeParticipant
{
	public static readonly int STATE_INITIALIZED = 0;

	public static readonly int STATE_ROUND_1_CREATED = 10;

	public static readonly int STATE_ROUND_1_VALIDATED = 20;

	public static readonly int STATE_ROUND_2_CREATED = 30;

	public static readonly int STATE_ROUND_2_VALIDATED = 40;

	public static readonly int STATE_KEY_CALCULATED = 50;

	public static readonly int STATE_ROUND_3_CREATED = 60;

	public static readonly int STATE_ROUND_3_VALIDATED = 70;

	private string participantId;

	private char[] password;

	private IDigest digest;

	private readonly SecureRandom random;

	private readonly BigInteger p;

	private readonly BigInteger q;

	private readonly BigInteger g;

	private string partnerParticipantId;

	private BigInteger x1;

	private BigInteger x2;

	private BigInteger gx1;

	private BigInteger gx2;

	private BigInteger gx3;

	private BigInteger gx4;

	private BigInteger b;

	private int state;

	public virtual int State => state;

	public JPakeParticipant(string participantId, char[] password)
		: this(participantId, password, JPakePrimeOrderGroups.NIST_3072)
	{
	}

	public JPakeParticipant(string participantId, char[] password, JPakePrimeOrderGroup group)
		: this(participantId, password, group, new Sha256Digest(), new SecureRandom())
	{
	}

	public JPakeParticipant(string participantId, char[] password, JPakePrimeOrderGroup group, IDigest digest, SecureRandom random)
	{
		JPakeUtilities.ValidateNotNull(participantId, "participantId");
		JPakeUtilities.ValidateNotNull(password, "password");
		JPakeUtilities.ValidateNotNull(group, "p");
		JPakeUtilities.ValidateNotNull(digest, "digest");
		JPakeUtilities.ValidateNotNull(random, "random");
		if (password.Length == 0)
		{
			throw new ArgumentException("Password must not be empty.");
		}
		this.participantId = participantId;
		this.password = new char[password.Length];
		Array.Copy(password, this.password, password.Length);
		p = group.P;
		q = group.Q;
		g = group.G;
		this.digest = digest;
		this.random = random;
		state = STATE_INITIALIZED;
	}

	public virtual JPakeRound1Payload CreateRound1PayloadToSend()
	{
		if (state >= STATE_ROUND_1_CREATED)
		{
			throw new InvalidOperationException("Round 1 payload already created for " + participantId);
		}
		x1 = JPakeUtilities.GenerateX1(q, random);
		x2 = JPakeUtilities.GenerateX2(q, random);
		gx1 = JPakeUtilities.CalculateGx(p, g, x1);
		gx2 = JPakeUtilities.CalculateGx(p, g, x2);
		BigInteger[] knowledgeProofForX = JPakeUtilities.CalculateZeroKnowledgeProof(p, q, g, gx1, x1, participantId, digest, random);
		BigInteger[] knowledgeProofForX2 = JPakeUtilities.CalculateZeroKnowledgeProof(p, q, g, gx2, x2, participantId, digest, random);
		state = STATE_ROUND_1_CREATED;
		return new JPakeRound1Payload(participantId, gx1, gx2, knowledgeProofForX, knowledgeProofForX2);
	}

	public virtual void ValidateRound1PayloadReceived(JPakeRound1Payload round1PayloadReceived)
	{
		if (state >= STATE_ROUND_1_VALIDATED)
		{
			throw new InvalidOperationException("Validation already attempted for round 1 payload for " + participantId);
		}
		partnerParticipantId = round1PayloadReceived.ParticipantId;
		gx3 = round1PayloadReceived.Gx1;
		gx4 = round1PayloadReceived.Gx2;
		BigInteger[] knowledgeProofForX = round1PayloadReceived.KnowledgeProofForX1;
		BigInteger[] knowledgeProofForX2 = round1PayloadReceived.KnowledgeProofForX2;
		JPakeUtilities.ValidateParticipantIdsDiffer(participantId, round1PayloadReceived.ParticipantId);
		JPakeUtilities.ValidateGx4(gx4);
		JPakeUtilities.ValidateZeroKnowledgeProof(p, q, g, gx3, knowledgeProofForX, round1PayloadReceived.ParticipantId, digest);
		JPakeUtilities.ValidateZeroKnowledgeProof(p, q, g, gx4, knowledgeProofForX2, round1PayloadReceived.ParticipantId, digest);
		state = STATE_ROUND_1_VALIDATED;
	}

	public virtual JPakeRound2Payload CreateRound2PayloadToSend()
	{
		if (state >= STATE_ROUND_2_CREATED)
		{
			throw new InvalidOperationException("Round 2 payload already created for " + participantId);
		}
		if (state < STATE_ROUND_1_VALIDATED)
		{
			throw new InvalidOperationException("Round 1 payload must be validated prior to creating round 2 payload for " + participantId);
		}
		BigInteger gA = JPakeUtilities.CalculateGA(p, gx1, gx3, gx4);
		BigInteger s = JPakeUtilities.CalculateS(password);
		BigInteger bigInteger = JPakeUtilities.CalculateX2s(q, x2, s);
		BigInteger bigInteger2 = JPakeUtilities.CalculateA(p, q, gA, bigInteger);
		BigInteger[] knowledgeProofForX2s = JPakeUtilities.CalculateZeroKnowledgeProof(p, q, gA, bigInteger2, bigInteger, participantId, digest, random);
		state = STATE_ROUND_2_CREATED;
		return new JPakeRound2Payload(participantId, bigInteger2, knowledgeProofForX2s);
	}

	public virtual void ValidateRound2PayloadReceived(JPakeRound2Payload round2PayloadReceived)
	{
		if (state >= STATE_ROUND_2_VALIDATED)
		{
			throw new InvalidOperationException("Validation already attempted for round 2 payload for " + participantId);
		}
		if (state < STATE_ROUND_1_VALIDATED)
		{
			throw new InvalidOperationException("Round 1 payload must be validated prior to validation round 2 payload for " + participantId);
		}
		BigInteger ga = JPakeUtilities.CalculateGA(p, gx3, gx1, gx2);
		b = round2PayloadReceived.A;
		BigInteger[] knowledgeProofForX2s = round2PayloadReceived.KnowledgeProofForX2s;
		JPakeUtilities.ValidateParticipantIdsDiffer(participantId, round2PayloadReceived.ParticipantId);
		JPakeUtilities.ValidateParticipantIdsEqual(partnerParticipantId, round2PayloadReceived.ParticipantId);
		JPakeUtilities.ValidateGa(ga);
		JPakeUtilities.ValidateZeroKnowledgeProof(p, q, ga, b, knowledgeProofForX2s, round2PayloadReceived.ParticipantId, digest);
		state = STATE_ROUND_2_VALIDATED;
	}

	public virtual BigInteger CalculateKeyingMaterial()
	{
		if (state >= STATE_KEY_CALCULATED)
		{
			throw new InvalidOperationException("Key already calculated for " + participantId);
		}
		if (state < STATE_ROUND_2_VALIDATED)
		{
			throw new InvalidOperationException("Round 2 payload must be validated prior to creating key for " + participantId);
		}
		BigInteger s = JPakeUtilities.CalculateS(password);
		Array.Clear(password, 0, password.Length);
		password = null;
		BigInteger result = JPakeUtilities.CalculateKeyingMaterial(p, q, gx4, x2, s, b);
		x1 = null;
		x2 = null;
		b = null;
		state = STATE_KEY_CALCULATED;
		return result;
	}

	public virtual JPakeRound3Payload CreateRound3PayloadToSend(BigInteger keyingMaterial)
	{
		if (state >= STATE_ROUND_3_CREATED)
		{
			throw new InvalidOperationException("Round 3 payload already created for " + participantId);
		}
		if (state < STATE_KEY_CALCULATED)
		{
			throw new InvalidOperationException("Keying material must be calculated prior to creating round 3 payload for " + participantId);
		}
		BigInteger magTag = JPakeUtilities.CalculateMacTag(participantId, partnerParticipantId, gx1, gx2, gx3, gx4, keyingMaterial, digest);
		state = STATE_ROUND_3_CREATED;
		return new JPakeRound3Payload(participantId, magTag);
	}

	public virtual void ValidateRound3PayloadReceived(JPakeRound3Payload round3PayloadReceived, BigInteger keyingMaterial)
	{
		if (state >= STATE_ROUND_3_VALIDATED)
		{
			throw new InvalidOperationException("Validation already attempted for round 3 payload for " + participantId);
		}
		if (state < STATE_KEY_CALCULATED)
		{
			throw new InvalidOperationException("Keying material must be calculated prior to validating round 3 payload for " + participantId);
		}
		JPakeUtilities.ValidateParticipantIdsDiffer(participantId, round3PayloadReceived.ParticipantId);
		JPakeUtilities.ValidateParticipantIdsEqual(partnerParticipantId, round3PayloadReceived.ParticipantId);
		JPakeUtilities.ValidateMacTag(participantId, partnerParticipantId, gx1, gx2, gx3, gx4, keyingMaterial, digest, round3PayloadReceived.MacTag);
		gx1 = null;
		gx2 = null;
		gx3 = null;
		gx4 = null;
		state = STATE_ROUND_3_VALIDATED;
	}
}
