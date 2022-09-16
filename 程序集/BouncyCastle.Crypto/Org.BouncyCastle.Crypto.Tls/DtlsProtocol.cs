using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public abstract class DtlsProtocol
{
	protected readonly SecureRandom mSecureRandom;

	protected DtlsProtocol(SecureRandom secureRandom)
	{
		if (secureRandom == null)
		{
			throw new ArgumentNullException("secureRandom");
		}
		mSecureRandom = secureRandom;
	}

	protected virtual void ProcessFinished(byte[] body, byte[] expected_verify_data)
	{
		MemoryStream memoryStream = new MemoryStream(body, writable: false);
		byte[] b = TlsUtilities.ReadFully(expected_verify_data.Length, memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		if (!Arrays.ConstantTimeAreEqual(expected_verify_data, b))
		{
			throw new TlsFatalAlert(40);
		}
	}

	internal static void ApplyMaxFragmentLengthExtension(DtlsRecordLayer recordLayer, short maxFragmentLength)
	{
		if (maxFragmentLength >= 0)
		{
			if (!MaxFragmentLength.IsValid((byte)maxFragmentLength))
			{
				throw new TlsFatalAlert(80);
			}
			int plaintextLimit = 1 << 8 + maxFragmentLength;
			recordLayer.SetPlaintextLimit(plaintextLimit);
		}
	}

	protected static short EvaluateMaxFragmentLengthExtension(bool resumedSession, IDictionary clientExtensions, IDictionary serverExtensions, byte alertDescription)
	{
		short maxFragmentLengthExtension = TlsExtensionsUtilities.GetMaxFragmentLengthExtension(serverExtensions);
		if (maxFragmentLengthExtension >= 0 && (!MaxFragmentLength.IsValid((byte)maxFragmentLengthExtension) || (!resumedSession && maxFragmentLengthExtension != TlsExtensionsUtilities.GetMaxFragmentLengthExtension(clientExtensions))))
		{
			throw new TlsFatalAlert(alertDescription);
		}
		return maxFragmentLengthExtension;
	}

	protected static byte[] GenerateCertificate(Certificate certificate)
	{
		MemoryStream memoryStream = new MemoryStream();
		certificate.Encode(memoryStream);
		return memoryStream.ToArray();
	}

	protected static byte[] GenerateSupplementalData(IList supplementalData)
	{
		MemoryStream memoryStream = new MemoryStream();
		TlsProtocol.WriteSupplementalData(memoryStream, supplementalData);
		return memoryStream.ToArray();
	}

	protected static void ValidateSelectedCipherSuite(int selectedCipherSuite, byte alertDescription)
	{
		switch (TlsUtilities.GetEncryptionAlgorithm(selectedCipherSuite))
		{
		case 1:
		case 2:
			throw new TlsFatalAlert(alertDescription);
		}
	}
}
