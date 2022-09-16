using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class Dstu7624Engine : IBlockCipher
{
	private const int ROUNDS_128 = 10;

	private const int ROUNDS_256 = 14;

	private const int ROUNDS_512 = 18;

	private const ulong mdsMatrix = 290207332435296513uL;

	private const ulong mdsInvMatrix = 14616231584692868525uL;

	private ulong[] internalState;

	private ulong[] workingKey;

	private ulong[][] roundKeys;

	private int wordsInBlock;

	private int wordsInKey;

	private int roundsAmount;

	private bool forEncryption;

	private static readonly byte[] S0 = new byte[256]
	{
		168, 67, 95, 6, 107, 117, 108, 89, 113, 223,
		135, 149, 23, 240, 216, 9, 109, 243, 29, 203,
		201, 77, 44, 175, 121, 224, 151, 253, 111, 75,
		69, 57, 62, 221, 163, 79, 180, 182, 154, 14,
		31, 191, 21, 225, 73, 210, 147, 198, 146, 114,
		158, 97, 209, 99, 250, 238, 244, 25, 213, 173,
		88, 164, 187, 161, 220, 242, 131, 55, 66, 228,
		122, 50, 156, 204, 171, 74, 143, 110, 4, 39,
		46, 231, 226, 90, 150, 22, 35, 43, 194, 101,
		102, 15, 188, 169, 71, 65, 52, 72, 252, 183,
		106, 136, 165, 83, 134, 249, 91, 219, 56, 123,
		195, 30, 34, 51, 36, 40, 54, 199, 178, 59,
		142, 119, 186, 245, 20, 159, 8, 85, 155, 76,
		254, 96, 92, 218, 24, 70, 205, 125, 33, 176,
		63, 27, 137, 255, 235, 132, 105, 58, 157, 215,
		211, 112, 103, 64, 181, 222, 93, 48, 145, 177,
		120, 17, 1, 229, 0, 104, 152, 160, 197, 2,
		166, 116, 45, 11, 162, 118, 179, 190, 206, 189,
		174, 233, 138, 49, 28, 236, 241, 153, 148, 170,
		246, 38, 47, 239, 232, 140, 53, 3, 212, 127,
		251, 5, 193, 94, 144, 32, 61, 130, 247, 234,
		10, 13, 126, 248, 80, 26, 196, 7, 87, 184,
		60, 98, 227, 200, 172, 82, 100, 16, 208, 217,
		19, 12, 18, 41, 81, 185, 207, 214, 115, 141,
		129, 84, 192, 237, 78, 68, 167, 42, 133, 37,
		230, 202, 124, 139, 86, 128
	};

	private static readonly byte[] S1 = new byte[256]
	{
		206, 187, 235, 146, 234, 203, 19, 193, 233, 58,
		214, 178, 210, 144, 23, 248, 66, 21, 86, 180,
		101, 28, 136, 67, 197, 92, 54, 186, 245, 87,
		103, 141, 49, 246, 100, 88, 158, 244, 34, 170,
		117, 15, 2, 177, 223, 109, 115, 77, 124, 38,
		46, 247, 8, 93, 68, 62, 159, 20, 200, 174,
		84, 16, 216, 188, 26, 107, 105, 243, 189, 51,
		171, 250, 209, 155, 104, 78, 22, 149, 145, 238,
		76, 99, 142, 91, 204, 60, 25, 161, 129, 73,
		123, 217, 111, 55, 96, 202, 231, 43, 72, 253,
		150, 69, 252, 65, 18, 13, 121, 229, 137, 140,
		227, 32, 48, 220, 183, 108, 74, 181, 63, 151,
		212, 98, 45, 6, 164, 165, 131, 95, 42, 218,
		201, 0, 126, 162, 85, 191, 17, 213, 156, 207,
		14, 10, 61, 81, 125, 147, 27, 254, 196, 71,
		9, 134, 11, 143, 157, 106, 7, 185, 176, 152,
		24, 50, 113, 75, 239, 59, 112, 160, 228, 64,
		255, 195, 169, 230, 120, 249, 139, 70, 128, 30,
		56, 225, 184, 168, 224, 12, 35, 118, 29, 37,
		36, 5, 241, 110, 148, 40, 154, 132, 232, 163,
		79, 119, 211, 133, 226, 82, 242, 130, 80, 122,
		47, 116, 83, 179, 97, 175, 57, 53, 222, 205,
		31, 153, 172, 173, 114, 44, 221, 208, 135, 190,
		94, 166, 236, 4, 198, 3, 52, 251, 219, 89,
		182, 194, 1, 240, 90, 237, 167, 102, 33, 127,
		138, 39, 199, 192, 41, 215
	};

	private static readonly byte[] S2 = new byte[256]
	{
		147, 217, 154, 181, 152, 34, 69, 252, 186, 106,
		223, 2, 159, 220, 81, 89, 74, 23, 43, 194,
		148, 244, 187, 163, 98, 228, 113, 212, 205, 112,
		22, 225, 73, 60, 192, 216, 92, 155, 173, 133,
		83, 161, 122, 200, 45, 224, 209, 114, 166, 44,
		196, 227, 118, 120, 183, 180, 9, 59, 14, 65,
		76, 222, 178, 144, 37, 165, 215, 3, 17, 0,
		195, 46, 146, 239, 78, 18, 157, 125, 203, 53,
		16, 213, 79, 158, 77, 169, 85, 198, 208, 123,
		24, 151, 211, 54, 230, 72, 86, 129, 143, 119,
		204, 156, 185, 226, 172, 184, 47, 21, 164, 124,
		218, 56, 30, 11, 5, 214, 20, 110, 108, 126,
		102, 253, 177, 229, 96, 175, 94, 51, 135, 201,
		240, 93, 109, 63, 136, 141, 199, 247, 29, 233,
		236, 237, 128, 41, 39, 207, 153, 168, 80, 15,
		55, 36, 40, 48, 149, 210, 62, 91, 64, 131,
		179, 105, 87, 31, 7, 28, 138, 188, 32, 235,
		206, 142, 171, 238, 49, 162, 115, 249, 202, 58,
		26, 251, 13, 193, 254, 250, 242, 111, 189, 150,
		221, 67, 82, 182, 8, 243, 174, 190, 25, 137,
		50, 38, 176, 234, 75, 100, 132, 130, 107, 245,
		121, 191, 1, 95, 117, 99, 27, 35, 61, 104,
		42, 101, 232, 145, 246, 255, 19, 88, 241, 71,
		10, 127, 197, 167, 231, 97, 90, 6, 70, 68,
		66, 4, 160, 219, 57, 134, 84, 170, 140, 52,
		33, 139, 248, 12, 116, 103
	};

	private static readonly byte[] S3 = new byte[256]
	{
		104, 141, 202, 77, 115, 75, 78, 42, 212, 82,
		38, 179, 84, 30, 25, 31, 34, 3, 70, 61,
		45, 74, 83, 131, 19, 138, 183, 213, 37, 121,
		245, 189, 88, 47, 13, 2, 237, 81, 158, 17,
		242, 62, 85, 94, 209, 22, 60, 102, 112, 93,
		243, 69, 64, 204, 232, 148, 86, 8, 206, 26,
		58, 210, 225, 223, 181, 56, 110, 14, 229, 244,
		249, 134, 233, 79, 214, 133, 35, 207, 50, 153,
		49, 20, 174, 238, 200, 72, 211, 48, 161, 146,
		65, 177, 24, 196, 44, 113, 114, 68, 21, 253,
		55, 190, 95, 170, 155, 136, 216, 171, 137, 156,
		250, 96, 234, 188, 98, 12, 36, 166, 168, 236,
		103, 32, 219, 124, 40, 221, 172, 91, 52, 126,
		16, 241, 123, 143, 99, 160, 5, 154, 67, 119,
		33, 191, 39, 9, 195, 159, 182, 215, 41, 194,
		235, 192, 164, 139, 140, 29, 251, 255, 193, 178,
		151, 46, 248, 101, 246, 117, 7, 4, 73, 51,
		228, 217, 185, 208, 66, 199, 108, 144, 0, 142,
		111, 80, 1, 197, 218, 71, 63, 205, 105, 162,
		226, 122, 167, 198, 147, 15, 10, 6, 230, 43,
		150, 163, 28, 175, 106, 18, 132, 57, 231, 176,
		130, 247, 254, 157, 135, 92, 129, 53, 222, 180,
		165, 252, 128, 239, 203, 187, 107, 118, 186, 90,
		125, 120, 11, 149, 227, 173, 116, 152, 59, 54,
		100, 109, 220, 240, 89, 169, 76, 23, 127, 145,
		184, 201, 87, 27, 224, 97
	};

	private static readonly byte[] T0 = new byte[256]
	{
		164, 162, 169, 197, 78, 201, 3, 217, 126, 15,
		210, 173, 231, 211, 39, 91, 227, 161, 232, 230,
		124, 42, 85, 12, 134, 57, 215, 141, 184, 18,
		111, 40, 205, 138, 112, 86, 114, 249, 191, 79,
		115, 233, 247, 87, 22, 172, 80, 192, 157, 183,
		71, 113, 96, 196, 116, 67, 108, 31, 147, 119,
		220, 206, 32, 140, 153, 95, 68, 1, 245, 30,
		135, 94, 97, 44, 75, 29, 129, 21, 244, 35,
		214, 234, 225, 103, 241, 127, 254, 218, 60, 7,
		83, 106, 132, 156, 203, 2, 131, 51, 221, 53,
		226, 89, 90, 152, 165, 146, 100, 4, 6, 16,
		77, 28, 151, 8, 49, 238, 171, 5, 175, 121,
		160, 24, 70, 109, 252, 137, 212, 199, 255, 240,
		207, 66, 145, 248, 104, 10, 101, 142, 182, 253,
		195, 239, 120, 76, 204, 158, 48, 46, 188, 11,
		84, 26, 166, 187, 38, 128, 72, 148, 50, 125,
		167, 63, 174, 34, 61, 102, 170, 246, 0, 93,
		189, 74, 224, 59, 180, 23, 139, 159, 118, 176,
		36, 154, 37, 99, 219, 235, 122, 62, 92, 179,
		177, 41, 242, 202, 88, 110, 216, 168, 47, 117,
		223, 20, 251, 19, 73, 136, 178, 236, 228, 52,
		45, 150, 198, 58, 237, 149, 14, 229, 133, 107,
		64, 33, 155, 9, 25, 43, 82, 222, 69, 163,
		250, 81, 194, 181, 209, 144, 185, 243, 55, 193,
		13, 186, 65, 17, 56, 123, 190, 208, 213, 105,
		54, 200, 98, 27, 130, 143
	};

	private static readonly byte[] T1 = new byte[256]
	{
		131, 242, 42, 235, 233, 191, 123, 156, 52, 150,
		141, 152, 185, 105, 140, 41, 61, 136, 104, 6,
		57, 17, 76, 14, 160, 86, 64, 146, 21, 188,
		179, 220, 111, 248, 38, 186, 190, 189, 49, 251,
		195, 254, 128, 97, 225, 122, 50, 210, 112, 32,
		161, 69, 236, 217, 26, 93, 180, 216, 9, 165,
		85, 142, 55, 118, 169, 103, 16, 23, 54, 101,
		177, 149, 98, 89, 116, 163, 80, 47, 75, 200,
		208, 143, 205, 212, 60, 134, 18, 29, 35, 239,
		244, 83, 25, 53, 230, 127, 94, 214, 121, 81,
		34, 20, 247, 30, 74, 66, 155, 65, 115, 45,
		193, 92, 166, 162, 224, 46, 211, 40, 187, 201,
		174, 106, 209, 90, 48, 144, 132, 249, 178, 88,
		207, 126, 197, 203, 151, 228, 22, 108, 250, 176,
		109, 31, 82, 153, 13, 78, 3, 145, 194, 77,
		100, 119, 159, 221, 196, 73, 138, 154, 36, 56,
		167, 87, 133, 199, 124, 125, 231, 246, 183, 172,
		39, 70, 222, 223, 59, 215, 158, 43, 11, 213,
		19, 117, 240, 114, 182, 157, 27, 1, 63, 68,
		229, 135, 253, 7, 241, 171, 148, 24, 234, 252,
		58, 130, 95, 5, 84, 219, 0, 139, 227, 72,
		12, 202, 120, 137, 10, 255, 62, 91, 129, 238,
		113, 226, 218, 44, 184, 181, 204, 110, 168, 107,
		173, 96, 198, 8, 4, 2, 232, 245, 79, 164,
		243, 192, 206, 67, 37, 28, 33, 51, 15, 175,
		71, 237, 102, 99, 147, 170
	};

	private static readonly byte[] T2 = new byte[256]
	{
		69, 212, 11, 67, 241, 114, 237, 164, 194, 56,
		230, 113, 253, 182, 58, 149, 80, 68, 75, 226,
		116, 107, 30, 17, 90, 198, 180, 216, 165, 138,
		112, 163, 168, 250, 5, 217, 151, 64, 201, 144,
		152, 143, 220, 18, 49, 44, 71, 106, 153, 174,
		200, 127, 249, 79, 93, 150, 111, 244, 179, 57,
		33, 218, 156, 133, 158, 59, 240, 191, 239, 6,
		238, 229, 95, 32, 16, 204, 60, 84, 74, 82,
		148, 14, 192, 40, 246, 86, 96, 162, 227, 15,
		236, 157, 36, 131, 126, 213, 124, 235, 24, 215,
		205, 221, 120, 255, 219, 161, 9, 208, 118, 132,
		117, 187, 29, 26, 47, 176, 254, 214, 52, 99,
		53, 210, 42, 89, 109, 77, 119, 231, 142, 97,
		207, 159, 206, 39, 245, 128, 134, 199, 166, 251,
		248, 135, 171, 98, 63, 223, 72, 0, 20, 154,
		189, 91, 4, 146, 2, 37, 101, 76, 83, 12,
		242, 41, 175, 23, 108, 65, 48, 233, 147, 85,
		247, 172, 104, 38, 196, 125, 202, 122, 62, 160,
		55, 3, 193, 54, 105, 102, 8, 22, 167, 188,
		197, 211, 34, 183, 19, 70, 50, 232, 87, 136,
		43, 129, 178, 78, 100, 28, 170, 145, 88, 46,
		155, 92, 27, 81, 115, 66, 35, 1, 110, 243,
		13, 190, 61, 10, 45, 31, 103, 51, 25, 123,
		94, 234, 222, 139, 203, 169, 140, 141, 173, 73,
		130, 228, 186, 195, 21, 209, 224, 137, 252, 177,
		185, 181, 7, 121, 184, 225
	};

	private static readonly byte[] T3 = new byte[256]
	{
		178, 182, 35, 17, 167, 136, 197, 166, 57, 143,
		196, 232, 115, 34, 67, 195, 130, 39, 205, 24,
		81, 98, 45, 247, 92, 14, 59, 253, 202, 155,
		13, 15, 121, 140, 16, 76, 116, 28, 10, 142,
		124, 148, 7, 199, 94, 20, 161, 33, 87, 80,
		78, 169, 128, 217, 239, 100, 65, 207, 60, 238,
		46, 19, 41, 186, 52, 90, 174, 138, 97, 51,
		18, 185, 85, 168, 21, 5, 246, 3, 6, 73,
		181, 37, 9, 22, 12, 42, 56, 252, 32, 244,
		229, 127, 215, 49, 43, 102, 111, 255, 114, 134,
		240, 163, 47, 120, 0, 188, 204, 226, 176, 241,
		66, 180, 48, 95, 96, 4, 236, 165, 227, 139,
		231, 29, 191, 132, 123, 230, 129, 248, 222, 216,
		210, 23, 206, 75, 71, 214, 105, 108, 25, 153,
		154, 1, 179, 133, 177, 249, 89, 194, 55, 233,
		200, 160, 237, 79, 137, 104, 109, 213, 38, 145,
		135, 88, 189, 201, 152, 220, 117, 192, 118, 245,
		103, 107, 126, 235, 82, 203, 209, 91, 159, 11,
		219, 64, 146, 26, 250, 172, 228, 225, 113, 31,
		101, 141, 151, 158, 149, 144, 93, 183, 193, 175,
		84, 251, 2, 224, 53, 187, 58, 77, 173, 44,
		61, 86, 8, 27, 74, 147, 106, 171, 184, 122,
		242, 125, 218, 63, 254, 62, 190, 234, 170, 68,
		198, 208, 54, 72, 112, 150, 119, 36, 83, 223,
		243, 131, 40, 50, 69, 30, 164, 211, 162, 70,
		110, 156, 221, 99, 212, 157
	};

	public virtual string AlgorithmName => "DSTU7624";

	public virtual bool IsPartialBlockOkay => false;

	public Dstu7624Engine(int blockSizeBits)
	{
		if (blockSizeBits != 128 && blockSizeBits != 256 && blockSizeBits != 512)
		{
			throw new ArgumentException("unsupported block length: only 128/256/512 are allowed");
		}
		wordsInBlock = blockSizeBits / 64;
		internalState = new ulong[wordsInBlock];
	}

	public virtual void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("Invalid parameter passed to Dstu7624Engine Init");
		}
		this.forEncryption = forEncryption;
		byte[] key = ((KeyParameter)parameters).GetKey();
		int num = key.Length << 3;
		int num2 = wordsInBlock << 6;
		if (num != 128 && num != 256 && num != 512)
		{
			throw new ArgumentException("unsupported key length: only 128/256/512 are allowed");
		}
		if (num != num2 && num != 2 * num2)
		{
			throw new ArgumentException("Unsupported key length");
		}
		switch (num)
		{
		case 128:
			roundsAmount = 10;
			break;
		case 256:
			roundsAmount = 14;
			break;
		case 512:
			roundsAmount = 18;
			break;
		}
		wordsInKey = num / 64;
		roundKeys = new ulong[roundsAmount + 1][];
		for (int i = 0; i < roundKeys.Length; i++)
		{
			roundKeys[i] = new ulong[wordsInBlock];
		}
		workingKey = new ulong[wordsInKey];
		if (key.Length != wordsInKey * 8)
		{
			throw new ArgumentException("Invalid key parameter passed to Dstu7624Engine Init");
		}
		Pack.LE_To_UInt64(key, 0, workingKey);
		ulong[] array = new ulong[wordsInBlock];
		WorkingKeyExpandKT(workingKey, array);
		WorkingKeyExpandEven(workingKey, array);
		WorkingKeyExpandOdd();
	}

	private void WorkingKeyExpandKT(ulong[] workingKey, ulong[] tempKeys)
	{
		ulong[] array = new ulong[wordsInBlock];
		ulong[] array2 = new ulong[wordsInBlock];
		internalState = new ulong[wordsInBlock];
		ulong[] array3;
		(array3 = internalState)[0] = array3[0] + (ulong)(wordsInBlock + wordsInKey + 1);
		if (wordsInBlock == wordsInKey)
		{
			Array.Copy(workingKey, 0, array, 0, array.Length);
			Array.Copy(workingKey, 0, array2, 0, array2.Length);
		}
		else
		{
			Array.Copy(workingKey, 0, array, 0, wordsInBlock);
			Array.Copy(workingKey, wordsInBlock, array2, 0, wordsInBlock);
		}
		for (int i = 0; i < internalState.Length; i++)
		{
			ulong[] array4 = (array3 = internalState);
			int num = i;
			nint num2 = num;
			array4[num] = array3[num2] + array[i];
		}
		EncryptionRound();
		for (int j = 0; j < internalState.Length; j++)
		{
			ulong[] array5 = (array3 = internalState);
			int num3 = j;
			nint num2 = num3;
			array5[num3] = array3[num2] ^ array2[j];
		}
		EncryptionRound();
		for (int k = 0; k < internalState.Length; k++)
		{
			ulong[] array6 = (array3 = internalState);
			int num4 = k;
			nint num2 = num4;
			array6[num4] = array3[num2] + array[k];
		}
		EncryptionRound();
		Array.Copy(internalState, 0, tempKeys, 0, wordsInBlock);
	}

	private void WorkingKeyExpandEven(ulong[] workingKey, ulong[] tempKey)
	{
		ulong[] array = new ulong[wordsInKey];
		ulong[] array2 = new ulong[wordsInBlock];
		int num = 0;
		Array.Copy(workingKey, 0, array, 0, wordsInKey);
		ulong num2 = 281479271743489uL;
		while (true)
		{
			for (int i = 0; i < wordsInBlock; i++)
			{
				array2[i] = tempKey[i] + num2;
			}
			for (int j = 0; j < wordsInBlock; j++)
			{
				internalState[j] = array[j] + array2[j];
			}
			EncryptionRound();
			for (int k = 0; k < wordsInBlock; k++)
			{
				ulong[] array3;
				ulong[] array4 = (array3 = internalState);
				int num3 = k;
				nint num4 = num3;
				array4[num3] = array3[num4] ^ array2[k];
			}
			EncryptionRound();
			for (int l = 0; l < wordsInBlock; l++)
			{
				ulong[] array3;
				ulong[] array5 = (array3 = internalState);
				int num5 = l;
				nint num4 = num5;
				array5[num5] = array3[num4] + array2[l];
			}
			Array.Copy(internalState, 0, roundKeys[num], 0, wordsInBlock);
			if (roundsAmount == num)
			{
				break;
			}
			if (wordsInKey != wordsInBlock)
			{
				num += 2;
				num2 <<= 1;
				for (int m = 0; m < wordsInBlock; m++)
				{
					array2[m] = tempKey[m] + num2;
				}
				for (int n = 0; n < wordsInBlock; n++)
				{
					internalState[n] = array[wordsInBlock + n] + array2[n];
				}
				EncryptionRound();
				for (int num6 = 0; num6 < wordsInBlock; num6++)
				{
					ulong[] array3;
					ulong[] array6 = (array3 = internalState);
					int num7 = num6;
					nint num4 = num7;
					array6[num7] = array3[num4] ^ array2[num6];
				}
				EncryptionRound();
				for (int num8 = 0; num8 < wordsInBlock; num8++)
				{
					ulong[] array3;
					ulong[] array7 = (array3 = internalState);
					int num9 = num8;
					nint num4 = num9;
					array7[num9] = array3[num4] + array2[num8];
				}
				Array.Copy(internalState, 0, roundKeys[num], 0, wordsInBlock);
				if (roundsAmount == num)
				{
					break;
				}
			}
			num += 2;
			num2 <<= 1;
			ulong num10 = array[0];
			for (int num11 = 1; num11 < array.Length; num11++)
			{
				array[num11 - 1] = array[num11];
			}
			array[array.Length - 1] = num10;
		}
	}

	private void WorkingKeyExpandOdd()
	{
		for (int i = 1; i < roundsAmount; i += 2)
		{
			RotateLeft(roundKeys[i - 1], roundKeys[i]);
		}
	}

	public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (workingKey == null)
		{
			throw new InvalidOperationException("Dstu7624Engine not initialised");
		}
		Check.DataLength(input, inOff, GetBlockSize(), "input buffer too short");
		Check.OutputLength(output, outOff, GetBlockSize(), "output buffer too short");
		if (forEncryption)
		{
			int num = wordsInBlock;
			if (num == 2)
			{
				EncryptBlock_128(input, inOff, output, outOff);
			}
			else
			{
				Pack.LE_To_UInt64(input, inOff, internalState);
				AddRoundKey(0);
				int num2 = 0;
				while (true)
				{
					EncryptionRound();
					if (++num2 == roundsAmount)
					{
						break;
					}
					XorRoundKey(num2);
				}
				AddRoundKey(roundsAmount);
				Pack.UInt64_To_LE(internalState, output, outOff);
			}
		}
		else
		{
			int num = wordsInBlock;
			if (num == 2)
			{
				DecryptBlock_128(input, inOff, output, outOff);
			}
			else
			{
				Pack.LE_To_UInt64(input, inOff, internalState);
				SubRoundKey(roundsAmount);
				int num3 = roundsAmount;
				while (true)
				{
					DecryptionRound();
					if (--num3 == 0)
					{
						break;
					}
					XorRoundKey(num3);
				}
				SubRoundKey(0);
				Pack.UInt64_To_LE(internalState, output, outOff);
			}
		}
		return GetBlockSize();
	}

	private void EncryptionRound()
	{
		SubBytes();
		ShiftRows();
		MixColumns();
	}

	private void DecryptionRound()
	{
		MixColumnsInv();
		InvShiftRows();
		InvSubBytes();
	}

	private void DecryptBlock_128(byte[] input, int inOff, byte[] output, int outOff)
	{
		ulong num = Pack.LE_To_UInt64(input, inOff);
		ulong num2 = Pack.LE_To_UInt64(input, inOff + 8);
		ulong[] array = roundKeys[roundsAmount];
		num -= array[0];
		num2 -= array[1];
		int num3 = roundsAmount;
		while (true)
		{
			num = MixColumnInv(num);
			num2 = MixColumnInv(num2);
			uint num4 = (uint)num;
			uint num5 = (uint)(num >> 32);
			uint num6 = (uint)num2;
			uint num7 = (uint)(num2 >> 32);
			byte b = T0[num4 & 0xFF];
			byte b2 = T1[(num4 >> 8) & 0xFF];
			byte b3 = T2[(num4 >> 16) & 0xFF];
			byte b4 = T3[num4 >> 24];
			num4 = (uint)(b | (b2 << 8) | (b3 << 16) | (b4 << 24));
			byte b5 = T0[num7 & 0xFF];
			byte b6 = T1[(num7 >> 8) & 0xFF];
			byte b7 = T2[(num7 >> 16) & 0xFF];
			byte b8 = T3[num7 >> 24];
			num7 = (uint)(b5 | (b6 << 8) | (b7 << 16) | (b8 << 24));
			num = num4 | ((ulong)num7 << 32);
			byte b9 = T0[num6 & 0xFF];
			byte b10 = T1[(num6 >> 8) & 0xFF];
			byte b11 = T2[(num6 >> 16) & 0xFF];
			byte b12 = T3[num6 >> 24];
			num6 = (uint)(b9 | (b10 << 8) | (b11 << 16) | (b12 << 24));
			byte b13 = T0[num5 & 0xFF];
			byte b14 = T1[(num5 >> 8) & 0xFF];
			byte b15 = T2[(num5 >> 16) & 0xFF];
			byte b16 = T3[num5 >> 24];
			num5 = (uint)(b13 | (b14 << 8) | (b15 << 16) | (b16 << 24));
			num2 = num6 | ((ulong)num5 << 32);
			if (--num3 == 0)
			{
				break;
			}
			array = roundKeys[num3];
			num ^= array[0];
			num2 ^= array[1];
		}
		array = roundKeys[0];
		num -= array[0];
		num2 -= array[1];
		Pack.UInt64_To_LE(num, output, outOff);
		Pack.UInt64_To_LE(num2, output, outOff + 8);
	}

	private void EncryptBlock_128(byte[] input, int inOff, byte[] output, int outOff)
	{
		ulong num = Pack.LE_To_UInt64(input, inOff);
		ulong num2 = Pack.LE_To_UInt64(input, inOff + 8);
		ulong[] array = roundKeys[0];
		num += array[0];
		num2 += array[1];
		int num3 = 0;
		while (true)
		{
			uint num4 = (uint)num;
			uint num5 = (uint)(num >> 32);
			uint num6 = (uint)num2;
			uint num7 = (uint)(num2 >> 32);
			byte b = S0[num4 & 0xFF];
			byte b2 = S1[(num4 >> 8) & 0xFF];
			byte b3 = S2[(num4 >> 16) & 0xFF];
			byte b4 = S3[num4 >> 24];
			num4 = (uint)(b | (b2 << 8) | (b3 << 16) | (b4 << 24));
			byte b5 = S0[num7 & 0xFF];
			byte b6 = S1[(num7 >> 8) & 0xFF];
			byte b7 = S2[(num7 >> 16) & 0xFF];
			byte b8 = S3[num7 >> 24];
			num7 = (uint)(b5 | (b6 << 8) | (b7 << 16) | (b8 << 24));
			num = num4 | ((ulong)num7 << 32);
			byte b9 = S0[num6 & 0xFF];
			byte b10 = S1[(num6 >> 8) & 0xFF];
			byte b11 = S2[(num6 >> 16) & 0xFF];
			byte b12 = S3[num6 >> 24];
			num6 = (uint)(b9 | (b10 << 8) | (b11 << 16) | (b12 << 24));
			byte b13 = S0[num5 & 0xFF];
			byte b14 = S1[(num5 >> 8) & 0xFF];
			byte b15 = S2[(num5 >> 16) & 0xFF];
			byte b16 = S3[num5 >> 24];
			num5 = (uint)(b13 | (b14 << 8) | (b15 << 16) | (b16 << 24));
			num2 = num6 | ((ulong)num5 << 32);
			num = MixColumn(num);
			num2 = MixColumn(num2);
			if (++num3 == roundsAmount)
			{
				break;
			}
			array = roundKeys[num3];
			num ^= array[0];
			num2 ^= array[1];
		}
		array = roundKeys[roundsAmount];
		num += array[0];
		num2 += array[1];
		Pack.UInt64_To_LE(num, output, outOff);
		Pack.UInt64_To_LE(num2, output, outOff + 8);
	}

	private void SubBytes()
	{
		for (int i = 0; i < wordsInBlock; i++)
		{
			ulong num = internalState[i];
			uint num2 = (uint)num;
			uint num3 = (uint)(num >> 32);
			byte b = S0[num2 & 0xFF];
			byte b2 = S1[(num2 >> 8) & 0xFF];
			byte b3 = S2[(num2 >> 16) & 0xFF];
			byte b4 = S3[num2 >> 24];
			num2 = (uint)(b | (b2 << 8) | (b3 << 16) | (b4 << 24));
			byte b5 = S0[num3 & 0xFF];
			byte b6 = S1[(num3 >> 8) & 0xFF];
			byte b7 = S2[(num3 >> 16) & 0xFF];
			byte b8 = S3[num3 >> 24];
			num3 = (uint)(b5 | (b6 << 8) | (b7 << 16) | (b8 << 24));
			internalState[i] = num2 | ((ulong)num3 << 32);
		}
	}

	private void InvSubBytes()
	{
		for (int i = 0; i < wordsInBlock; i++)
		{
			ulong num = internalState[i];
			uint num2 = (uint)num;
			uint num3 = (uint)(num >> 32);
			byte b = T0[num2 & 0xFF];
			byte b2 = T1[(num2 >> 8) & 0xFF];
			byte b3 = T2[(num2 >> 16) & 0xFF];
			byte b4 = T3[num2 >> 24];
			num2 = (uint)(b | (b2 << 8) | (b3 << 16) | (b4 << 24));
			byte b5 = T0[num3 & 0xFF];
			byte b6 = T1[(num3 >> 8) & 0xFF];
			byte b7 = T2[(num3 >> 16) & 0xFF];
			byte b8 = T3[num3 >> 24];
			num3 = (uint)(b5 | (b6 << 8) | (b7 << 16) | (b8 << 24));
			internalState[i] = num2 | ((ulong)num3 << 32);
		}
	}

	private void ShiftRows()
	{
		switch (wordsInBlock)
		{
		case 2:
		{
			ulong num15 = internalState[0];
			ulong num16 = internalState[1];
			ulong num17 = (num15 ^ num16) & 0xFFFFFFFF00000000uL;
			num15 ^= num17;
			num16 ^= num17;
			internalState[0] = num15;
			internalState[1] = num16;
			break;
		}
		case 4:
		{
			ulong num10 = internalState[0];
			ulong num11 = internalState[1];
			ulong num12 = internalState[2];
			ulong num13 = internalState[3];
			ulong num14 = (num10 ^ num12) & 0xFFFFFFFF00000000uL;
			num10 ^= num14;
			num12 ^= num14;
			num14 = (num11 ^ num13) & 0xFFFFFFFF0000uL;
			num11 ^= num14;
			num13 ^= num14;
			num14 = (num10 ^ num11) & 0xFFFF0000FFFF0000uL;
			num10 ^= num14;
			num11 ^= num14;
			num14 = (num12 ^ num13) & 0xFFFF0000FFFF0000uL;
			num12 ^= num14;
			num13 ^= num14;
			internalState[0] = num10;
			internalState[1] = num11;
			internalState[2] = num12;
			internalState[3] = num13;
			break;
		}
		case 8:
		{
			ulong num = internalState[0];
			ulong num2 = internalState[1];
			ulong num3 = internalState[2];
			ulong num4 = internalState[3];
			ulong num5 = internalState[4];
			ulong num6 = internalState[5];
			ulong num7 = internalState[6];
			ulong num8 = internalState[7];
			ulong num9 = (num ^ num5) & 0xFFFFFFFF00000000uL;
			num ^= num9;
			num5 ^= num9;
			num9 = (num2 ^ num6) & 0xFFFFFFFF000000uL;
			num2 ^= num9;
			num6 ^= num9;
			num9 = (num3 ^ num7) & 0xFFFFFFFF0000uL;
			num3 ^= num9;
			num7 ^= num9;
			num9 = (num4 ^ num8) & 0xFFFFFFFF00uL;
			num4 ^= num9;
			num8 ^= num9;
			num9 = (num ^ num3) & 0xFFFF0000FFFF0000uL;
			num ^= num9;
			num3 ^= num9;
			num9 = (num2 ^ num4) & 0xFFFF0000FFFF00uL;
			num2 ^= num9;
			num4 ^= num9;
			num9 = (num5 ^ num7) & 0xFFFF0000FFFF0000uL;
			num5 ^= num9;
			num7 ^= num9;
			num9 = (num6 ^ num8) & 0xFFFF0000FFFF00uL;
			num6 ^= num9;
			num8 ^= num9;
			num9 = (num ^ num2) & 0xFF00FF00FF00FF00uL;
			num ^= num9;
			num2 ^= num9;
			num9 = (num3 ^ num4) & 0xFF00FF00FF00FF00uL;
			num3 ^= num9;
			num4 ^= num9;
			num9 = (num5 ^ num6) & 0xFF00FF00FF00FF00uL;
			num5 ^= num9;
			num6 ^= num9;
			num9 = (num7 ^ num8) & 0xFF00FF00FF00FF00uL;
			num7 ^= num9;
			num8 ^= num9;
			internalState[0] = num;
			internalState[1] = num2;
			internalState[2] = num3;
			internalState[3] = num4;
			internalState[4] = num5;
			internalState[5] = num6;
			internalState[6] = num7;
			internalState[7] = num8;
			break;
		}
		default:
			throw new InvalidOperationException("unsupported block length: only 128/256/512 are allowed");
		}
	}

	private void InvShiftRows()
	{
		switch (wordsInBlock)
		{
		case 2:
		{
			ulong num15 = internalState[0];
			ulong num16 = internalState[1];
			ulong num17 = (num15 ^ num16) & 0xFFFFFFFF00000000uL;
			num15 ^= num17;
			num16 ^= num17;
			internalState[0] = num15;
			internalState[1] = num16;
			break;
		}
		case 4:
		{
			ulong num10 = internalState[0];
			ulong num11 = internalState[1];
			ulong num12 = internalState[2];
			ulong num13 = internalState[3];
			ulong num14 = (num10 ^ num11) & 0xFFFF0000FFFF0000uL;
			num10 ^= num14;
			num11 ^= num14;
			num14 = (num12 ^ num13) & 0xFFFF0000FFFF0000uL;
			num12 ^= num14;
			num13 ^= num14;
			num14 = (num10 ^ num12) & 0xFFFFFFFF00000000uL;
			num10 ^= num14;
			num12 ^= num14;
			num14 = (num11 ^ num13) & 0xFFFFFFFF0000uL;
			num11 ^= num14;
			num13 ^= num14;
			internalState[0] = num10;
			internalState[1] = num11;
			internalState[2] = num12;
			internalState[3] = num13;
			break;
		}
		case 8:
		{
			ulong num = internalState[0];
			ulong num2 = internalState[1];
			ulong num3 = internalState[2];
			ulong num4 = internalState[3];
			ulong num5 = internalState[4];
			ulong num6 = internalState[5];
			ulong num7 = internalState[6];
			ulong num8 = internalState[7];
			ulong num9 = (num ^ num2) & 0xFF00FF00FF00FF00uL;
			num ^= num9;
			num2 ^= num9;
			num9 = (num3 ^ num4) & 0xFF00FF00FF00FF00uL;
			num3 ^= num9;
			num4 ^= num9;
			num9 = (num5 ^ num6) & 0xFF00FF00FF00FF00uL;
			num5 ^= num9;
			num6 ^= num9;
			num9 = (num7 ^ num8) & 0xFF00FF00FF00FF00uL;
			num7 ^= num9;
			num8 ^= num9;
			num9 = (num ^ num3) & 0xFFFF0000FFFF0000uL;
			num ^= num9;
			num3 ^= num9;
			num9 = (num2 ^ num4) & 0xFFFF0000FFFF00uL;
			num2 ^= num9;
			num4 ^= num9;
			num9 = (num5 ^ num7) & 0xFFFF0000FFFF0000uL;
			num5 ^= num9;
			num7 ^= num9;
			num9 = (num6 ^ num8) & 0xFFFF0000FFFF00uL;
			num6 ^= num9;
			num8 ^= num9;
			num9 = (num ^ num5) & 0xFFFFFFFF00000000uL;
			num ^= num9;
			num5 ^= num9;
			num9 = (num2 ^ num6) & 0xFFFFFFFF000000uL;
			num2 ^= num9;
			num6 ^= num9;
			num9 = (num3 ^ num7) & 0xFFFFFFFF0000uL;
			num3 ^= num9;
			num7 ^= num9;
			num9 = (num4 ^ num8) & 0xFFFFFFFF00uL;
			num4 ^= num9;
			num8 ^= num9;
			internalState[0] = num;
			internalState[1] = num2;
			internalState[2] = num3;
			internalState[3] = num4;
			internalState[4] = num5;
			internalState[5] = num6;
			internalState[6] = num7;
			internalState[7] = num8;
			break;
		}
		default:
			throw new InvalidOperationException("unsupported block length: only 128/256/512 are allowed");
		}
	}

	private void AddRoundKey(int round)
	{
		ulong[] array = roundKeys[round];
		for (int i = 0; i < wordsInBlock; i++)
		{
			ulong[] array2;
			ulong[] array3 = (array2 = internalState);
			int num = i;
			nint num2 = num;
			array3[num] = array2[num2] + array[i];
		}
	}

	private void SubRoundKey(int round)
	{
		ulong[] array = roundKeys[round];
		for (int i = 0; i < wordsInBlock; i++)
		{
			ulong[] array2;
			ulong[] array3 = (array2 = internalState);
			int num = i;
			nint num2 = num;
			array3[num] = array2[num2] - array[i];
		}
	}

	private void XorRoundKey(int round)
	{
		ulong[] array = roundKeys[round];
		for (int i = 0; i < wordsInBlock; i++)
		{
			ulong[] array2;
			ulong[] array3 = (array2 = internalState);
			int num = i;
			nint num2 = num;
			array3[num] = array2[num2] ^ array[i];
		}
	}

	private static ulong MixColumn(ulong c)
	{
		ulong num = MulX(c);
		ulong num2 = Rotate(8, c) ^ c;
		num2 ^= Rotate(16, num2);
		num2 ^= Rotate(48, c);
		ulong x = MulX2(num2 ^ c ^ num);
		return num2 ^ Rotate(32, x) ^ Rotate(40, num) ^ Rotate(48, num);
	}

	private void MixColumns()
	{
		for (int i = 0; i < wordsInBlock; i++)
		{
			internalState[i] = MixColumn(internalState[i]);
		}
	}

	private static ulong MixColumnInv(ulong c)
	{
		ulong num = c;
		num ^= Rotate(8, num);
		num ^= Rotate(32, num);
		num ^= Rotate(48, c);
		ulong num2 = num ^ c;
		ulong num3 = Rotate(48, c);
		ulong num4 = Rotate(56, c);
		ulong n = num2 ^ num4;
		ulong num5 = Rotate(56, num2);
		num5 ^= MulX(n);
		ulong num6 = Rotate(16, num2) ^ c;
		num6 ^= Rotate(40, MulX(num5) ^ c);
		ulong num7 = num2 ^ num3;
		num7 ^= MulX(num6);
		ulong num8 = Rotate(16, num);
		num8 ^= MulX(num7);
		ulong num9 = num2 ^ Rotate(24, c) ^ num3 ^ num4;
		num9 ^= MulX(num8);
		ulong num10 = Rotate(32, num2) ^ c ^ num4;
		num10 ^= MulX(num9);
		return num ^ MulX(Rotate(40, num10));
	}

	private void MixColumnsInv()
	{
		for (int i = 0; i < wordsInBlock; i++)
		{
			internalState[i] = MixColumnInv(internalState[i]);
		}
	}

	private static ulong MulX(ulong n)
	{
		return ((n & 0x7F7F7F7F7F7F7F7FL) << 1) ^ (((n & 0x8080808080808080uL) >> 7) * 29);
	}

	private static ulong MulX2(ulong n)
	{
		return ((n & 0x3F3F3F3F3F3F3F3FL) << 2) ^ (((n & 0x8080808080808080uL) >> 6) * 29) ^ (((n & 0x4040404040404040L) >> 6) * 29);
	}

	private static ulong Rotate(int n, ulong x)
	{
		return (x >> n) | (x << -n);
	}

	private void RotateLeft(ulong[] x, ulong[] z)
	{
		switch (wordsInBlock)
		{
		case 2:
		{
			ulong num13 = x[0];
			ulong num14 = x[1];
			z[0] = (num13 >> 56) | (num14 << 8);
			z[1] = (num14 >> 56) | (num13 << 8);
			break;
		}
		case 4:
		{
			ulong num9 = x[0];
			ulong num10 = x[1];
			ulong num11 = x[2];
			ulong num12 = x[3];
			z[0] = (num10 >> 24) | (num11 << 40);
			z[1] = (num11 >> 24) | (num12 << 40);
			z[2] = (num12 >> 24) | (num9 << 40);
			z[3] = (num9 >> 24) | (num10 << 40);
			break;
		}
		case 8:
		{
			ulong num = x[0];
			ulong num2 = x[1];
			ulong num3 = x[2];
			ulong num4 = x[3];
			ulong num5 = x[4];
			ulong num6 = x[5];
			ulong num7 = x[6];
			ulong num8 = x[7];
			z[0] = (num3 >> 24) | (num4 << 40);
			z[1] = (num4 >> 24) | (num5 << 40);
			z[2] = (num5 >> 24) | (num6 << 40);
			z[3] = (num6 >> 24) | (num7 << 40);
			z[4] = (num7 >> 24) | (num8 << 40);
			z[5] = (num8 >> 24) | (num << 40);
			z[6] = (num >> 24) | (num2 << 40);
			z[7] = (num2 >> 24) | (num3 << 40);
			break;
		}
		default:
			throw new InvalidOperationException("unsupported block length: only 128/256/512 are allowed");
		}
	}

	public virtual int GetBlockSize()
	{
		return wordsInBlock << 3;
	}

	public virtual void Reset()
	{
		Array.Clear(internalState, 0, internalState.Length);
	}
}
