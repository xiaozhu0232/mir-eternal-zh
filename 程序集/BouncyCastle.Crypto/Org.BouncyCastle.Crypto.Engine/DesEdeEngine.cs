using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines;

public class DesEdeEngine : DesEngine
{
	private int[] workingKey1;

	private int[] workingKey2;

	private int[] workingKey3;

	private bool forEncryption;

	public override string AlgorithmName => "DESede";

	public override void Init(bool forEncryption, ICipherParameters parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("invalid parameter passed to DESede init - " + Platform.GetTypeName(parameters));
		}
		byte[] key = ((KeyParameter)parameters).GetKey();
		if (key.Length != 24 && key.Length != 16)
		{
			throw new ArgumentException("key size must be 16 or 24 bytes.");
		}
		this.forEncryption = forEncryption;
		byte[] array = new byte[8];
		Array.Copy(key, 0, array, 0, array.Length);
		workingKey1 = DesEngine.GenerateWorkingKey(forEncryption, array);
		byte[] array2 = new byte[8];
		Array.Copy(key, 8, array2, 0, array2.Length);
		workingKey2 = DesEngine.GenerateWorkingKey(!forEncryption, array2);
		if (key.Length == 24)
		{
			byte[] array3 = new byte[8];
			Array.Copy(key, 16, array3, 0, array3.Length);
			workingKey3 = DesEngine.GenerateWorkingKey(forEncryption, array3);
		}
		else
		{
			workingKey3 = workingKey1;
		}
	}

	public override int GetBlockSize()
	{
		return 8;
	}

	public override int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
	{
		if (workingKey1 == null)
		{
			throw new InvalidOperationException("DESede engine not initialised");
		}
		Check.DataLength(input, inOff, 8, "input buffer too short");
		Check.OutputLength(output, outOff, 8, "output buffer too short");
		byte[] array = new byte[8];
		if (forEncryption)
		{
			DesEngine.DesFunc(workingKey1, input, inOff, array, 0);
			DesEngine.DesFunc(workingKey2, array, 0, array, 0);
			DesEngine.DesFunc(workingKey3, array, 0, output, outOff);
		}
		else
		{
			DesEngine.DesFunc(workingKey3, input, inOff, array, 0);
			DesEngine.DesFunc(workingKey2, array, 0, array, 0);
			DesEngine.DesFunc(workingKey1, array, 0, output, outOff);
		}
		return 8;
	}

	public override void Reset()
	{
	}
}
