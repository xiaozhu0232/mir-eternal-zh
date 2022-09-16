using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.Field;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class TlsEccUtilities
{
	private static readonly string[] CurveNames = new string[28]
	{
		"sect163k1", "sect163r1", "sect163r2", "sect193r1", "sect193r2", "sect233k1", "sect233r1", "sect239k1", "sect283k1", "sect283r1",
		"sect409k1", "sect409r1", "sect571k1", "sect571r1", "secp160k1", "secp160r1", "secp160r2", "secp192k1", "secp192r1", "secp224k1",
		"secp224r1", "secp256k1", "secp256r1", "secp384r1", "secp521r1", "brainpoolP256r1", "brainpoolP384r1", "brainpoolP512r1"
	};

	public static void AddSupportedEllipticCurvesExtension(IDictionary extensions, int[] namedCurves)
	{
		extensions[10] = CreateSupportedEllipticCurvesExtension(namedCurves);
	}

	public static void AddSupportedPointFormatsExtension(IDictionary extensions, byte[] ecPointFormats)
	{
		extensions[11] = CreateSupportedPointFormatsExtension(ecPointFormats);
	}

	public static int[] GetSupportedEllipticCurvesExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 10);
		if (extensionData != null)
		{
			return ReadSupportedEllipticCurvesExtension(extensionData);
		}
		return null;
	}

	public static byte[] GetSupportedPointFormatsExtension(IDictionary extensions)
	{
		byte[] extensionData = TlsUtilities.GetExtensionData(extensions, 11);
		if (extensionData != null)
		{
			return ReadSupportedPointFormatsExtension(extensionData);
		}
		return null;
	}

	public static byte[] CreateSupportedEllipticCurvesExtension(int[] namedCurves)
	{
		if (namedCurves == null || namedCurves.Length < 1)
		{
			throw new TlsFatalAlert(80);
		}
		return TlsUtilities.EncodeUint16ArrayWithUint16Length(namedCurves);
	}

	public static byte[] CreateSupportedPointFormatsExtension(byte[] ecPointFormats)
	{
		if (ecPointFormats == null || !Arrays.Contains(ecPointFormats, 0))
		{
			ecPointFormats = Arrays.Append(ecPointFormats, 0);
		}
		return TlsUtilities.EncodeUint8ArrayWithUint8Length(ecPointFormats);
	}

	public static int[] ReadSupportedEllipticCurvesExtension(byte[] extensionData)
	{
		if (extensionData == null)
		{
			throw new ArgumentNullException("extensionData");
		}
		MemoryStream memoryStream = new MemoryStream(extensionData, writable: false);
		int num = TlsUtilities.ReadUint16(memoryStream);
		if (num < 2 || ((uint)num & (true ? 1u : 0u)) != 0)
		{
			throw new TlsFatalAlert(50);
		}
		int[] result = TlsUtilities.ReadUint16Array(num / 2, memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		return result;
	}

	public static byte[] ReadSupportedPointFormatsExtension(byte[] extensionData)
	{
		byte[] array = TlsUtilities.DecodeUint8ArrayWithUint8Length(extensionData);
		if (!Arrays.Contains(array, 0))
		{
			throw new TlsFatalAlert(47);
		}
		return array;
	}

	public static string GetNameOfNamedCurve(int namedCurve)
	{
		if (!IsSupportedNamedCurve(namedCurve))
		{
			return null;
		}
		return CurveNames[namedCurve - 1];
	}

	public static ECDomainParameters GetParametersForNamedCurve(int namedCurve)
	{
		string nameOfNamedCurve = GetNameOfNamedCurve(namedCurve);
		if (nameOfNamedCurve == null)
		{
			return null;
		}
		X9ECParameters byName = CustomNamedCurves.GetByName(nameOfNamedCurve);
		if (byName == null)
		{
			byName = ECNamedCurveTable.GetByName(nameOfNamedCurve);
			if (byName == null)
			{
				return null;
			}
		}
		return new ECDomainParameters(byName.Curve, byName.G, byName.N, byName.H, byName.GetSeed());
	}

	public static bool HasAnySupportedNamedCurves()
	{
		return CurveNames.Length > 0;
	}

	public static bool ContainsEccCipherSuites(int[] cipherSuites)
	{
		for (int i = 0; i < cipherSuites.Length; i++)
		{
			if (IsEccCipherSuite(cipherSuites[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsEccCipherSuite(int cipherSuite)
	{
		switch (cipherSuite)
		{
		case 49153:
		case 49154:
		case 49155:
		case 49156:
		case 49157:
		case 49158:
		case 49159:
		case 49160:
		case 49161:
		case 49162:
		case 49163:
		case 49164:
		case 49165:
		case 49166:
		case 49167:
		case 49168:
		case 49169:
		case 49170:
		case 49171:
		case 49172:
		case 49173:
		case 49174:
		case 49175:
		case 49176:
		case 49177:
		case 49187:
		case 49188:
		case 49189:
		case 49190:
		case 49191:
		case 49192:
		case 49193:
		case 49194:
		case 49195:
		case 49196:
		case 49197:
		case 49198:
		case 49199:
		case 49200:
		case 49201:
		case 49202:
		case 49203:
		case 49204:
		case 49205:
		case 49206:
		case 49207:
		case 49208:
		case 49209:
		case 49210:
		case 49211:
		case 49266:
		case 49267:
		case 49268:
		case 49269:
		case 49270:
		case 49271:
		case 49272:
		case 49273:
		case 49286:
		case 49287:
		case 49288:
		case 49289:
		case 49290:
		case 49291:
		case 49292:
		case 49293:
		case 49306:
		case 49307:
		case 49324:
		case 49325:
		case 49326:
		case 49327:
		case 52392:
		case 52393:
		case 52396:
			return true;
		default:
			return false;
		}
	}

	public static bool AreOnSameCurve(ECDomainParameters a, ECDomainParameters b)
	{
		return a?.Equals(b) ?? false;
	}

	public static bool IsSupportedNamedCurve(int namedCurve)
	{
		if (namedCurve > 0)
		{
			return namedCurve <= CurveNames.Length;
		}
		return false;
	}

	public static bool IsCompressionPreferred(byte[] ecPointFormats, byte compressionFormat)
	{
		if (ecPointFormats == null)
		{
			return false;
		}
		foreach (byte b in ecPointFormats)
		{
			if (b == 0)
			{
				return false;
			}
			if (b == compressionFormat)
			{
				return true;
			}
		}
		return false;
	}

	public static byte[] SerializeECFieldElement(int fieldSize, BigInteger x)
	{
		return BigIntegers.AsUnsignedByteArray((fieldSize + 7) / 8, x);
	}

	public static byte[] SerializeECPoint(byte[] ecPointFormats, ECPoint point)
	{
		ECCurve curve = point.Curve;
		bool compressed = false;
		if (ECAlgorithms.IsFpCurve(curve))
		{
			compressed = IsCompressionPreferred(ecPointFormats, 1);
		}
		else if (ECAlgorithms.IsF2mCurve(curve))
		{
			compressed = IsCompressionPreferred(ecPointFormats, 2);
		}
		return point.GetEncoded(compressed);
	}

	public static byte[] SerializeECPublicKey(byte[] ecPointFormats, ECPublicKeyParameters keyParameters)
	{
		return SerializeECPoint(ecPointFormats, keyParameters.Q);
	}

	public static BigInteger DeserializeECFieldElement(int fieldSize, byte[] encoding)
	{
		int num = (fieldSize + 7) / 8;
		if (encoding.Length != num)
		{
			throw new TlsFatalAlert(50);
		}
		return new BigInteger(1, encoding);
	}

	public static ECPoint DeserializeECPoint(byte[] ecPointFormats, ECCurve curve, byte[] encoding)
	{
		if (encoding == null || encoding.Length < 1)
		{
			throw new TlsFatalAlert(47);
		}
		byte b;
		switch (encoding[0])
		{
		case 2:
		case 3:
			if (ECAlgorithms.IsF2mCurve(curve))
			{
				b = 2;
				break;
			}
			if (ECAlgorithms.IsFpCurve(curve))
			{
				b = 1;
				break;
			}
			throw new TlsFatalAlert(47);
		case 4:
			b = 0;
			break;
		default:
			throw new TlsFatalAlert(47);
		}
		if (b != 0 && (ecPointFormats == null || !Arrays.Contains(ecPointFormats, b)))
		{
			throw new TlsFatalAlert(47);
		}
		return curve.DecodePoint(encoding);
	}

	public static ECPublicKeyParameters DeserializeECPublicKey(byte[] ecPointFormats, ECDomainParameters curve_params, byte[] encoding)
	{
		try
		{
			ECPoint q = DeserializeECPoint(ecPointFormats, curve_params.Curve, encoding);
			return new ECPublicKeyParameters(q, curve_params);
		}
		catch (Exception alertCause)
		{
			throw new TlsFatalAlert(47, alertCause);
		}
	}

	public static byte[] CalculateECDHBasicAgreement(ECPublicKeyParameters publicKey, ECPrivateKeyParameters privateKey)
	{
		ECDHBasicAgreement eCDHBasicAgreement = new ECDHBasicAgreement();
		eCDHBasicAgreement.Init(privateKey);
		BigInteger n = eCDHBasicAgreement.CalculateAgreement(publicKey);
		return BigIntegers.AsUnsignedByteArray(eCDHBasicAgreement.GetFieldSize(), n);
	}

	public static AsymmetricCipherKeyPair GenerateECKeyPair(SecureRandom random, ECDomainParameters ecParams)
	{
		ECKeyPairGenerator eCKeyPairGenerator = new ECKeyPairGenerator();
		eCKeyPairGenerator.Init(new ECKeyGenerationParameters(ecParams, random));
		return eCKeyPairGenerator.GenerateKeyPair();
	}

	public static ECPrivateKeyParameters GenerateEphemeralClientKeyExchange(SecureRandom random, byte[] ecPointFormats, ECDomainParameters ecParams, Stream output)
	{
		AsymmetricCipherKeyPair asymmetricCipherKeyPair = GenerateECKeyPair(random, ecParams);
		ECPublicKeyParameters eCPublicKeyParameters = (ECPublicKeyParameters)asymmetricCipherKeyPair.Public;
		WriteECPoint(ecPointFormats, eCPublicKeyParameters.Q, output);
		return (ECPrivateKeyParameters)asymmetricCipherKeyPair.Private;
	}

	internal static ECPrivateKeyParameters GenerateEphemeralServerKeyExchange(SecureRandom random, int[] namedCurves, byte[] ecPointFormats, Stream output)
	{
		int num = -1;
		if (namedCurves == null)
		{
			num = 23;
		}
		else
		{
			foreach (int num2 in namedCurves)
			{
				if (NamedCurve.IsValid(num2) && IsSupportedNamedCurve(num2))
				{
					num = num2;
					break;
				}
			}
		}
		ECDomainParameters eCDomainParameters = null;
		if (num >= 0)
		{
			eCDomainParameters = GetParametersForNamedCurve(num);
		}
		else if (Arrays.Contains(namedCurves, 65281))
		{
			eCDomainParameters = GetParametersForNamedCurve(23);
		}
		else if (Arrays.Contains(namedCurves, 65282))
		{
			eCDomainParameters = GetParametersForNamedCurve(10);
		}
		if (eCDomainParameters == null)
		{
			throw new TlsFatalAlert(80);
		}
		if (num < 0)
		{
			WriteExplicitECParameters(ecPointFormats, eCDomainParameters, output);
		}
		else
		{
			WriteNamedECParameters(num, output);
		}
		return GenerateEphemeralClientKeyExchange(random, ecPointFormats, eCDomainParameters, output);
	}

	public static ECPublicKeyParameters ValidateECPublicKey(ECPublicKeyParameters key)
	{
		return key;
	}

	public static int ReadECExponent(int fieldSize, Stream input)
	{
		BigInteger bigInteger = ReadECParameter(input);
		if (bigInteger.BitLength < 32)
		{
			int intValue = bigInteger.IntValue;
			if (intValue > 0 && intValue < fieldSize)
			{
				return intValue;
			}
		}
		throw new TlsFatalAlert(47);
	}

	public static BigInteger ReadECFieldElement(int fieldSize, Stream input)
	{
		return DeserializeECFieldElement(fieldSize, TlsUtilities.ReadOpaque8(input));
	}

	public static BigInteger ReadECParameter(Stream input)
	{
		return new BigInteger(1, TlsUtilities.ReadOpaque8(input));
	}

	public static ECDomainParameters ReadECParameters(int[] namedCurves, byte[] ecPointFormats, Stream input)
	{
		try
		{
			switch (TlsUtilities.ReadUint8(input))
			{
			case 1:
			{
				CheckNamedCurve(namedCurves, 65281);
				BigInteger bigInteger3 = ReadECParameter(input);
				BigInteger a2 = ReadECFieldElement(bigInteger3.BitLength, input);
				BigInteger b3 = ReadECFieldElement(bigInteger3.BitLength, input);
				byte[] encoding2 = TlsUtilities.ReadOpaque8(input);
				BigInteger bigInteger4 = ReadECParameter(input);
				BigInteger bigInteger5 = ReadECParameter(input);
				ECCurve curve2 = new FpCurve(bigInteger3, a2, b3, bigInteger4, bigInteger5);
				ECPoint g2 = DeserializeECPoint(ecPointFormats, curve2, encoding2);
				return new ECDomainParameters(curve2, g2, bigInteger4, bigInteger5);
			}
			case 2:
			{
				CheckNamedCurve(namedCurves, 65282);
				int num = TlsUtilities.ReadUint16(input);
				byte b = TlsUtilities.ReadUint8(input);
				if (!ECBasisType.IsValid(b))
				{
					throw new TlsFatalAlert(47);
				}
				int num2 = ReadECExponent(num, input);
				int k = -1;
				int k2 = -1;
				if (b == 2)
				{
					k = ReadECExponent(num, input);
					k2 = ReadECExponent(num, input);
				}
				BigInteger a = ReadECFieldElement(num, input);
				BigInteger b2 = ReadECFieldElement(num, input);
				byte[] encoding = TlsUtilities.ReadOpaque8(input);
				BigInteger bigInteger = ReadECParameter(input);
				BigInteger bigInteger2 = ReadECParameter(input);
				ECCurve curve = ((b == 2) ? new F2mCurve(num, num2, k, k2, a, b2, bigInteger, bigInteger2) : new F2mCurve(num, num2, a, b2, bigInteger, bigInteger2));
				ECPoint g = DeserializeECPoint(ecPointFormats, curve, encoding);
				return new ECDomainParameters(curve, g, bigInteger, bigInteger2);
			}
			case 3:
			{
				int namedCurve = TlsUtilities.ReadUint16(input);
				if (!NamedCurve.RefersToASpecificNamedCurve(namedCurve))
				{
					throw new TlsFatalAlert(47);
				}
				CheckNamedCurve(namedCurves, namedCurve);
				return GetParametersForNamedCurve(namedCurve);
			}
			default:
				throw new TlsFatalAlert(47);
			}
		}
		catch (Exception alertCause)
		{
			throw new TlsFatalAlert(47, alertCause);
		}
	}

	private static void CheckNamedCurve(int[] namedCurves, int namedCurve)
	{
		if (namedCurves != null && !Arrays.Contains(namedCurves, namedCurve))
		{
			throw new TlsFatalAlert(47);
		}
	}

	public static void WriteECExponent(int k, Stream output)
	{
		BigInteger x = BigInteger.ValueOf(k);
		WriteECParameter(x, output);
	}

	public static void WriteECFieldElement(ECFieldElement x, Stream output)
	{
		TlsUtilities.WriteOpaque8(x.GetEncoded(), output);
	}

	public static void WriteECFieldElement(int fieldSize, BigInteger x, Stream output)
	{
		TlsUtilities.WriteOpaque8(SerializeECFieldElement(fieldSize, x), output);
	}

	public static void WriteECParameter(BigInteger x, Stream output)
	{
		TlsUtilities.WriteOpaque8(BigIntegers.AsUnsignedByteArray(x), output);
	}

	public static void WriteExplicitECParameters(byte[] ecPointFormats, ECDomainParameters ecParameters, Stream output)
	{
		ECCurve curve = ecParameters.Curve;
		if (ECAlgorithms.IsFpCurve(curve))
		{
			TlsUtilities.WriteUint8(1, output);
			WriteECParameter(curve.Field.Characteristic, output);
		}
		else
		{
			if (!ECAlgorithms.IsF2mCurve(curve))
			{
				throw new ArgumentException("'ecParameters' not a known curve type");
			}
			IPolynomialExtensionField polynomialExtensionField = (IPolynomialExtensionField)curve.Field;
			int[] exponentsPresent = polynomialExtensionField.MinimalPolynomial.GetExponentsPresent();
			TlsUtilities.WriteUint8(2, output);
			int i = exponentsPresent[exponentsPresent.Length - 1];
			TlsUtilities.CheckUint16(i);
			TlsUtilities.WriteUint16(i, output);
			if (exponentsPresent.Length == 3)
			{
				TlsUtilities.WriteUint8(1, output);
				WriteECExponent(exponentsPresent[1], output);
			}
			else
			{
				if (exponentsPresent.Length != 5)
				{
					throw new ArgumentException("Only trinomial and pentomial curves are supported");
				}
				TlsUtilities.WriteUint8(2, output);
				WriteECExponent(exponentsPresent[1], output);
				WriteECExponent(exponentsPresent[2], output);
				WriteECExponent(exponentsPresent[3], output);
			}
		}
		WriteECFieldElement(curve.A, output);
		WriteECFieldElement(curve.B, output);
		TlsUtilities.WriteOpaque8(SerializeECPoint(ecPointFormats, ecParameters.G), output);
		WriteECParameter(ecParameters.N, output);
		WriteECParameter(ecParameters.H, output);
	}

	public static void WriteECPoint(byte[] ecPointFormats, ECPoint point, Stream output)
	{
		TlsUtilities.WriteOpaque8(SerializeECPoint(ecPointFormats, point), output);
	}

	public static void WriteNamedECParameters(int namedCurve, Stream output)
	{
		if (!NamedCurve.RefersToASpecificNamedCurve(namedCurve))
		{
			throw new TlsFatalAlert(80);
		}
		TlsUtilities.WriteUint8(3, output);
		TlsUtilities.CheckUint16(namedCurve);
		TlsUtilities.WriteUint16(namedCurve, output);
	}
}
