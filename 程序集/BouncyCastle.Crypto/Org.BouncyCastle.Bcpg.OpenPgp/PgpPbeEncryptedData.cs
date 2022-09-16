using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp;

public class PgpPbeEncryptedData : PgpEncryptedData
{
	private readonly SymmetricKeyEncSessionPacket keyData;

	internal PgpPbeEncryptedData(SymmetricKeyEncSessionPacket keyData, InputStreamPacket encData)
		: base(encData)
	{
		this.keyData = keyData;
	}

	public override Stream GetInputStream()
	{
		return encData.GetInputStream();
	}

	public Stream GetDataStream(char[] passPhrase)
	{
		return DoGetDataStream(PgpUtilities.EncodePassPhrase(passPhrase, utf8: false), clearPassPhrase: true);
	}

	public Stream GetDataStreamUtf8(char[] passPhrase)
	{
		return DoGetDataStream(PgpUtilities.EncodePassPhrase(passPhrase, utf8: true), clearPassPhrase: true);
	}

	public Stream GetDataStreamRaw(byte[] rawPassPhrase)
	{
		return DoGetDataStream(rawPassPhrase, clearPassPhrase: false);
	}

	internal Stream DoGetDataStream(byte[] rawPassPhrase, bool clearPassPhrase)
	{
		try
		{
			SymmetricKeyAlgorithmTag symmetricKeyAlgorithmTag = keyData.EncAlgorithm;
			KeyParameter parameters = PgpUtilities.DoMakeKeyFromPassPhrase(symmetricKeyAlgorithmTag, keyData.S2k, rawPassPhrase, clearPassPhrase);
			byte[] secKeyData = keyData.GetSecKeyData();
			if (secKeyData != null && secKeyData.Length > 0)
			{
				IBufferedCipher cipher = CipherUtilities.GetCipher(PgpUtilities.GetSymmetricCipherName(symmetricKeyAlgorithmTag) + "/CFB/NoPadding");
				cipher.Init(forEncryption: false, new ParametersWithIV(parameters, new byte[cipher.GetBlockSize()]));
				byte[] array = cipher.DoFinal(secKeyData);
				symmetricKeyAlgorithmTag = (SymmetricKeyAlgorithmTag)array[0];
				parameters = ParameterUtilities.CreateKeyParameter(PgpUtilities.GetSymmetricCipherName(symmetricKeyAlgorithmTag), array, 1, array.Length - 1);
			}
			IBufferedCipher bufferedCipher = CreateStreamCipher(symmetricKeyAlgorithmTag);
			byte[] array2 = new byte[bufferedCipher.GetBlockSize()];
			bufferedCipher.Init(forEncryption: false, new ParametersWithIV(parameters, array2));
			encStream = BcpgInputStream.Wrap(new CipherStream(encData.GetInputStream(), bufferedCipher, null));
			if (encData is SymmetricEncIntegrityPacket)
			{
				truncStream = new TruncatedStream(encStream);
				string digestName = PgpUtilities.GetDigestName(HashAlgorithmTag.Sha1);
				IDigest digest = DigestUtilities.GetDigest(digestName);
				encStream = new DigestStream(truncStream, digest, null);
			}
			if (Streams.ReadFully(encStream, array2, 0, array2.Length) < array2.Length)
			{
				throw new EndOfStreamException("unexpected end of stream.");
			}
			int num = encStream.ReadByte();
			int num2 = encStream.ReadByte();
			if (num < 0 || num2 < 0)
			{
				throw new EndOfStreamException("unexpected end of stream.");
			}
			bool flag = array2[array2.Length - 2] == (byte)num && array2[array2.Length - 1] == (byte)num2;
			bool flag2 = num == 0 && num2 == 0;
			if (!flag && !flag2)
			{
				throw new PgpDataValidationException("quick check failed.");
			}
			return encStream;
		}
		catch (PgpException ex)
		{
			throw ex;
		}
		catch (Exception exception)
		{
			throw new PgpException("Exception creating cipher", exception);
		}
	}

	private IBufferedCipher CreateStreamCipher(SymmetricKeyAlgorithmTag keyAlgorithm)
	{
		string text = ((encData is SymmetricEncIntegrityPacket) ? "CFB" : "OpenPGPCFB");
		string algorithm = PgpUtilities.GetSymmetricCipherName(keyAlgorithm) + "/" + text + "/NoPadding";
		return CipherUtilities.GetCipher(algorithm);
	}
}
