using System;
using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Kisa;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ntt;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security;

public sealed class WrapperUtilities
{
	private enum WrapAlgorithm
	{
		AESWRAP,
		CAMELLIAWRAP,
		DESEDEWRAP,
		RC2WRAP,
		SEEDWRAP,
		DESEDERFC3211WRAP,
		AESRFC3211WRAP,
		CAMELLIARFC3211WRAP
	}

	private class BufferedCipherWrapper : IWrapper
	{
		private readonly IBufferedCipher cipher;

		private bool forWrapping;

		public string AlgorithmName => cipher.AlgorithmName;

		public BufferedCipherWrapper(IBufferedCipher cipher)
		{
			this.cipher = cipher;
		}

		public void Init(bool forWrapping, ICipherParameters parameters)
		{
			this.forWrapping = forWrapping;
			cipher.Init(forWrapping, parameters);
		}

		public byte[] Wrap(byte[] input, int inOff, int length)
		{
			if (!forWrapping)
			{
				throw new InvalidOperationException("Not initialised for wrapping");
			}
			return cipher.DoFinal(input, inOff, length);
		}

		public byte[] Unwrap(byte[] input, int inOff, int length)
		{
			if (forWrapping)
			{
				throw new InvalidOperationException("Not initialised for unwrapping");
			}
			return cipher.DoFinal(input, inOff, length);
		}
	}

	private static readonly IDictionary algorithms;

	private WrapperUtilities()
	{
	}

	static WrapperUtilities()
	{
		algorithms = Platform.CreateHashtable();
		((WrapAlgorithm)Enums.GetArbitraryValue(typeof(WrapAlgorithm))).ToString();
		algorithms[NistObjectIdentifiers.IdAes128Wrap.Id] = "AESWRAP";
		algorithms[NistObjectIdentifiers.IdAes192Wrap.Id] = "AESWRAP";
		algorithms[NistObjectIdentifiers.IdAes256Wrap.Id] = "AESWRAP";
		algorithms[NttObjectIdentifiers.IdCamellia128Wrap.Id] = "CAMELLIAWRAP";
		algorithms[NttObjectIdentifiers.IdCamellia192Wrap.Id] = "CAMELLIAWRAP";
		algorithms[NttObjectIdentifiers.IdCamellia256Wrap.Id] = "CAMELLIAWRAP";
		algorithms[PkcsObjectIdentifiers.IdAlgCms3DesWrap.Id] = "DESEDEWRAP";
		algorithms["TDEAWRAP"] = "DESEDEWRAP";
		algorithms[PkcsObjectIdentifiers.IdAlgCmsRC2Wrap.Id] = "RC2WRAP";
		algorithms[KisaObjectIdentifiers.IdNpkiAppCmsSeedWrap.Id] = "SEEDWRAP";
	}

	public static IWrapper GetWrapper(DerObjectIdentifier oid)
	{
		return GetWrapper(oid.Id);
	}

	public static IWrapper GetWrapper(string algorithm)
	{
		string text = Platform.ToUpperInvariant(algorithm);
		string text2 = (string)algorithms[text];
		if (text2 == null)
		{
			text2 = text;
		}
		try
		{
			switch ((WrapAlgorithm)Enums.GetEnumValue(typeof(WrapAlgorithm), text2))
			{
			case WrapAlgorithm.AESWRAP:
				return new AesWrapEngine();
			case WrapAlgorithm.CAMELLIAWRAP:
				return new CamelliaWrapEngine();
			case WrapAlgorithm.DESEDEWRAP:
				return new DesEdeWrapEngine();
			case WrapAlgorithm.RC2WRAP:
				return new RC2WrapEngine();
			case WrapAlgorithm.SEEDWRAP:
				return new SeedWrapEngine();
			case WrapAlgorithm.DESEDERFC3211WRAP:
				return new Rfc3211WrapEngine(new DesEdeEngine());
			case WrapAlgorithm.AESRFC3211WRAP:
				return new Rfc3211WrapEngine(new AesEngine());
			case WrapAlgorithm.CAMELLIARFC3211WRAP:
				return new Rfc3211WrapEngine(new CamelliaEngine());
			}
		}
		catch (ArgumentException)
		{
		}
		IBufferedCipher cipher = CipherUtilities.GetCipher(algorithm);
		if (cipher != null)
		{
			return new BufferedCipherWrapper(cipher);
		}
		throw new SecurityUtilityException("Wrapper " + algorithm + " not recognised.");
	}

	public static string GetAlgorithmName(DerObjectIdentifier oid)
	{
		return (string)algorithms[oid.Id];
	}
}
