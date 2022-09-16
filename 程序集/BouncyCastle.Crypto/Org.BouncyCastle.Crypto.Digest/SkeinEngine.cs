using System;
using System.Collections;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Digests;

public class SkeinEngine : IMemoable
{
	private class Configuration
	{
		private byte[] bytes = new byte[32];

		public byte[] Bytes => bytes;

		public Configuration(long outputSizeBits)
		{
			bytes[0] = 83;
			bytes[1] = 72;
			bytes[2] = 65;
			bytes[3] = 51;
			bytes[4] = 1;
			bytes[5] = 0;
			ThreefishEngine.WordToBytes((ulong)outputSizeBits, bytes, 8);
		}
	}

	public class Parameter
	{
		private int type;

		private byte[] value;

		public int Type => type;

		public byte[] Value => value;

		public Parameter(int type, byte[] value)
		{
			this.type = type;
			this.value = value;
		}
	}

	private class UbiTweak
	{
		private const ulong LOW_RANGE = 18446744069414584320uL;

		private const ulong T1_FINAL = 9223372036854775808uL;

		private const ulong T1_FIRST = 4611686018427387904uL;

		private ulong[] tweak = new ulong[2];

		private bool extendedPosition;

		public uint Type
		{
			get
			{
				return (uint)((tweak[1] >> 56) & 0x3F);
			}
			set
			{
				tweak[1] = (tweak[1] & 0xFFFFFFC000000000uL) | (((ulong)value & 0x3FuL) << 56);
			}
		}

		public bool First
		{
			get
			{
				return (tweak[1] & 0x4000000000000000L) != 0;
			}
			set
			{
				if (value)
				{
					ulong[] array;
					(array = tweak)[1] = array[1] | 0x4000000000000000uL;
				}
				else
				{
					ulong[] array;
					(array = tweak)[1] = array[1] & 0xBFFFFFFFFFFFFFFFuL;
				}
			}
		}

		public bool Final
		{
			get
			{
				return (tweak[1] & 0x8000000000000000uL) != 0;
			}
			set
			{
				if (value)
				{
					ulong[] array;
					(array = tweak)[1] = array[1] | 0x8000000000000000uL;
				}
				else
				{
					ulong[] array;
					(array = tweak)[1] = array[1] & 0x7FFFFFFFFFFFFFFFuL;
				}
			}
		}

		public UbiTweak()
		{
			Reset();
		}

		public void Reset(UbiTweak tweak)
		{
			this.tweak = Arrays.Clone(tweak.tweak, this.tweak);
			extendedPosition = tweak.extendedPosition;
		}

		public void Reset()
		{
			tweak[0] = 0uL;
			tweak[1] = 0uL;
			extendedPosition = false;
			First = true;
		}

		public void AdvancePosition(int advance)
		{
			if (extendedPosition)
			{
				ulong[] array = new ulong[3]
				{
					tweak[0] & 0xFFFFFFFFu,
					(tweak[0] >> 32) & 0xFFFFFFFFu,
					tweak[1] & 0xFFFFFFFFu
				};
				ulong num = (ulong)advance;
				for (int i = 0; i < array.Length; i++)
				{
					num = (array[i] = num + array[i]) >> 32;
				}
				tweak[0] = ((array[1] & 0xFFFFFFFFu) << 32) | (array[0] & 0xFFFFFFFFu);
				tweak[1] = (tweak[1] & 0xFFFFFFFF00000000uL) | (array[2] & 0xFFFFFFFFu);
			}
			else
			{
				ulong num2 = tweak[0];
				num2 += (uint)advance;
				tweak[0] = num2;
				if (num2 > 18446744069414584320uL)
				{
					extendedPosition = true;
				}
			}
		}

		public ulong[] GetWords()
		{
			return tweak;
		}

		public override string ToString()
		{
			return Type + " first: " + First + ", final: " + Final;
		}
	}

	private class UBI
	{
		private readonly UbiTweak tweak = new UbiTweak();

		private readonly SkeinEngine engine;

		private byte[] currentBlock;

		private int currentOffset;

		private ulong[] message;

		public UBI(SkeinEngine engine, int blockSize)
		{
			this.engine = engine;
			currentBlock = new byte[blockSize];
			message = new ulong[currentBlock.Length / 8];
		}

		public void Reset(UBI ubi)
		{
			currentBlock = Arrays.Clone(ubi.currentBlock, currentBlock);
			currentOffset = ubi.currentOffset;
			message = Arrays.Clone(ubi.message, message);
			tweak.Reset(ubi.tweak);
		}

		public void Reset(int type)
		{
			tweak.Reset();
			tweak.Type = (uint)type;
			currentOffset = 0;
		}

		public void Update(byte[] value, int offset, int len, ulong[] output)
		{
			int num = 0;
			while (len > num)
			{
				if (currentOffset == currentBlock.Length)
				{
					ProcessBlock(output);
					tweak.First = false;
					currentOffset = 0;
				}
				int num2 = System.Math.Min(len - num, currentBlock.Length - currentOffset);
				Array.Copy(value, offset + num, currentBlock, currentOffset, num2);
				num += num2;
				currentOffset += num2;
				tweak.AdvancePosition(num2);
			}
		}

		private void ProcessBlock(ulong[] output)
		{
			engine.threefish.Init(forEncryption: true, engine.chain, tweak.GetWords());
			for (int i = 0; i < message.Length; i++)
			{
				message[i] = ThreefishEngine.BytesToWord(currentBlock, i * 8);
			}
			engine.threefish.ProcessBlock(message, output);
			for (int j = 0; j < output.Length; j++)
			{
				ulong[] array;
				ulong[] array2 = (array = output);
				int num = j;
				nint num2 = num;
				array2[num] = array[num2] ^ message[j];
			}
		}

		public void DoFinal(ulong[] output)
		{
			for (int i = currentOffset; i < currentBlock.Length; i++)
			{
				currentBlock[i] = 0;
			}
			tweak.Final = true;
			ProcessBlock(output);
		}
	}

	public const int SKEIN_256 = 256;

	public const int SKEIN_512 = 512;

	public const int SKEIN_1024 = 1024;

	private const int PARAM_TYPE_KEY = 0;

	private const int PARAM_TYPE_CONFIG = 4;

	private const int PARAM_TYPE_MESSAGE = 48;

	private const int PARAM_TYPE_OUTPUT = 63;

	private static readonly IDictionary INITIAL_STATES;

	private readonly ThreefishEngine threefish;

	private readonly int outputSizeBytes;

	private ulong[] chain;

	private ulong[] initialState;

	private byte[] key;

	private Parameter[] preMessageParameters;

	private Parameter[] postMessageParameters;

	private readonly UBI ubi;

	private readonly byte[] singleByte = new byte[1];

	public int OutputSize => outputSizeBytes;

	public int BlockSize => threefish.GetBlockSize();

	static SkeinEngine()
	{
		INITIAL_STATES = Platform.CreateHashtable();
		InitialState(256, 128, new ulong[4] { 16217771249220022880uL, 9817190399063458076uL, 1155188648486244218uL, 14769517481627992514uL });
		InitialState(256, 160, new ulong[4] { 1450197650740764312uL, 3081844928540042640uL, 15310647011875280446uL, 3301952811952417661uL });
		InitialState(256, 224, new ulong[4] { 14270089230798940683uL, 9758551101254474012uL, 11082101768697755780uL, 4056579644589979102uL });
		InitialState(256, 256, new ulong[4] { 18202890402666165321uL, 3443677322885453875uL, 12915131351309911055uL, 7662005193972177513uL });
		InitialState(512, 128, new ulong[8] { 12158729379475595090uL, 2204638249859346602uL, 3502419045458743507uL, 13617680570268287068uL, 983504137758028059uL, 1880512238245786339uL, 11730851291495443074uL, 7602827311880509485uL });
		InitialState(512, 160, new ulong[8] { 2934123928682216849uL, 14047033351726823311uL, 1684584802963255058uL, 5744138295201861711uL, 2444857010922934358uL, 15638910433986703544uL, 13325156239043941114uL, 118355523173251694uL });
		InitialState(512, 224, new ulong[8] { 14758403053642543652uL, 14674518637417806319uL, 10145881904771976036uL, 4146387520469897396uL, 1106145742801415120uL, 7455425944880474941uL, 11095680972475339753uL, 11397762726744039159uL });
		InitialState(512, 384, new ulong[8] { 11814849197074935647uL, 12753905853581818532uL, 11346781217370868990uL, 15535391162178797018uL, 2000907093792408677uL, 9140007292425499655uL, 6093301768906360022uL, 2769176472213098488uL });
		InitialState(512, 512, new ulong[8] { 5261240102383538638uL, 978932832955457283uL, 10363226125605772238uL, 11107378794354519217uL, 6752626034097301424uL, 16915020251879818228uL, 11029617608758768931uL, 12544957130904423475uL });
	}

	private static void InitialState(int blockSize, int outputSize, ulong[] state)
	{
		INITIAL_STATES.Add(VariantIdentifier(blockSize / 8, outputSize / 8), state);
	}

	private static int VariantIdentifier(int blockSizeBytes, int outputSizeBytes)
	{
		return (outputSizeBytes << 16) | blockSizeBytes;
	}

	public SkeinEngine(int blockSizeBits, int outputSizeBits)
	{
		if (outputSizeBits % 8 != 0)
		{
			throw new ArgumentException("Output size must be a multiple of 8 bits. :" + outputSizeBits);
		}
		outputSizeBytes = outputSizeBits / 8;
		threefish = new ThreefishEngine(blockSizeBits);
		ubi = new UBI(this, threefish.GetBlockSize());
	}

	public SkeinEngine(SkeinEngine engine)
		: this(engine.BlockSize * 8, engine.OutputSize * 8)
	{
		CopyIn(engine);
	}

	private void CopyIn(SkeinEngine engine)
	{
		ubi.Reset(engine.ubi);
		chain = Arrays.Clone(engine.chain, chain);
		initialState = Arrays.Clone(engine.initialState, initialState);
		key = Arrays.Clone(engine.key, key);
		preMessageParameters = Clone(engine.preMessageParameters, preMessageParameters);
		postMessageParameters = Clone(engine.postMessageParameters, postMessageParameters);
	}

	private static Parameter[] Clone(Parameter[] data, Parameter[] existing)
	{
		if (data == null)
		{
			return null;
		}
		if (existing == null || existing.Length != data.Length)
		{
			existing = new Parameter[data.Length];
		}
		Array.Copy(data, 0, existing, 0, existing.Length);
		return existing;
	}

	public IMemoable Copy()
	{
		return new SkeinEngine(this);
	}

	public void Reset(IMemoable other)
	{
		SkeinEngine skeinEngine = (SkeinEngine)other;
		if (BlockSize != skeinEngine.BlockSize || outputSizeBytes != skeinEngine.outputSizeBytes)
		{
			throw new MemoableResetException("Incompatible parameters in provided SkeinEngine.");
		}
		CopyIn(skeinEngine);
	}

	public void Init(SkeinParameters parameters)
	{
		chain = null;
		key = null;
		preMessageParameters = null;
		postMessageParameters = null;
		if (parameters != null)
		{
			byte[] array = parameters.GetKey();
			if (array.Length < 16)
			{
				throw new ArgumentException("Skein key must be at least 128 bits.");
			}
			InitParams(parameters.GetParameters());
		}
		CreateInitialState();
		UbiInit(48);
	}

	private void InitParams(IDictionary parameters)
	{
		IEnumerator enumerator = parameters.Keys.GetEnumerator();
		IList list = Platform.CreateArrayList();
		IList list2 = Platform.CreateArrayList();
		while (enumerator.MoveNext())
		{
			int num = (int)enumerator.Current;
			byte[] value = (byte[])parameters[num];
			if (num == 0)
			{
				key = value;
			}
			else if (num < 48)
			{
				list.Add(new Parameter(num, value));
			}
			else
			{
				list2.Add(new Parameter(num, value));
			}
		}
		preMessageParameters = new Parameter[list.Count];
		list.CopyTo(preMessageParameters, 0);
		Array.Sort((Array)preMessageParameters);
		postMessageParameters = new Parameter[list2.Count];
		list2.CopyTo(postMessageParameters, 0);
		Array.Sort((Array)postMessageParameters);
	}

	private void CreateInitialState()
	{
		ulong[] array = (ulong[])INITIAL_STATES[VariantIdentifier(BlockSize, OutputSize)];
		if (key == null && array != null)
		{
			chain = Arrays.Clone(array);
		}
		else
		{
			chain = new ulong[BlockSize / 8];
			if (key != null)
			{
				UbiComplete(0, key);
			}
			UbiComplete(4, new Configuration(outputSizeBytes * 8).Bytes);
		}
		if (preMessageParameters != null)
		{
			for (int i = 0; i < preMessageParameters.Length; i++)
			{
				Parameter parameter = preMessageParameters[i];
				UbiComplete(parameter.Type, parameter.Value);
			}
		}
		initialState = Arrays.Clone(chain);
	}

	public void Reset()
	{
		Array.Copy(initialState, 0, chain, 0, chain.Length);
		UbiInit(48);
	}

	private void UbiComplete(int type, byte[] value)
	{
		UbiInit(type);
		ubi.Update(value, 0, value.Length, chain);
		UbiFinal();
	}

	private void UbiInit(int type)
	{
		ubi.Reset(type);
	}

	private void UbiFinal()
	{
		ubi.DoFinal(chain);
	}

	private void CheckInitialised()
	{
		if (ubi == null)
		{
			throw new ArgumentException("Skein engine is not initialised.");
		}
	}

	public void Update(byte inByte)
	{
		singleByte[0] = inByte;
		Update(singleByte, 0, 1);
	}

	public void Update(byte[] inBytes, int inOff, int len)
	{
		CheckInitialised();
		ubi.Update(inBytes, inOff, len, chain);
	}

	public int DoFinal(byte[] outBytes, int outOff)
	{
		CheckInitialised();
		if (outBytes.Length < outOff + outputSizeBytes)
		{
			throw new DataLengthException("Output buffer is too short to hold output");
		}
		UbiFinal();
		if (postMessageParameters != null)
		{
			for (int i = 0; i < postMessageParameters.Length; i++)
			{
				Parameter parameter = postMessageParameters[i];
				UbiComplete(parameter.Type, parameter.Value);
			}
		}
		int blockSize = BlockSize;
		int num = (outputSizeBytes + blockSize - 1) / blockSize;
		for (int j = 0; j < num; j++)
		{
			int outputBytes = System.Math.Min(blockSize, outputSizeBytes - j * blockSize);
			Output((ulong)j, outBytes, outOff + j * blockSize, outputBytes);
		}
		Reset();
		return outputSizeBytes;
	}

	private void Output(ulong outputSequence, byte[] outBytes, int outOff, int outputBytes)
	{
		byte[] array = new byte[8];
		ThreefishEngine.WordToBytes(outputSequence, array, 0);
		ulong[] array2 = new ulong[chain.Length];
		UbiInit(63);
		ubi.Update(array, 0, array.Length, array2);
		ubi.DoFinal(array2);
		int num = (outputBytes + 8 - 1) / 8;
		for (int i = 0; i < num; i++)
		{
			int num2 = System.Math.Min(8, outputBytes - i * 8);
			if (num2 == 8)
			{
				ThreefishEngine.WordToBytes(array2[i], outBytes, outOff + i * 8);
				continue;
			}
			ThreefishEngine.WordToBytes(array2[i], array, 0);
			Array.Copy(array, 0, outBytes, outOff + i * 8, num2);
		}
	}
}
