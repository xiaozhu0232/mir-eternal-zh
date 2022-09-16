using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class TlsClientProtocol : TlsProtocol
{
	protected TlsClient mTlsClient = null;

	internal TlsClientContextImpl mTlsClientContext = null;

	protected byte[] mSelectedSessionID = null;

	protected TlsKeyExchange mKeyExchange = null;

	protected TlsAuthentication mAuthentication = null;

	protected CertificateStatus mCertificateStatus = null;

	protected CertificateRequest mCertificateRequest = null;

	protected override TlsContext Context => mTlsClientContext;

	internal override AbstractTlsContext ContextAdmin => mTlsClientContext;

	protected override TlsPeer Peer => mTlsClient;

	public TlsClientProtocol(Stream stream, SecureRandom secureRandom)
		: base(stream, secureRandom)
	{
	}

	public TlsClientProtocol(Stream input, Stream output, SecureRandom secureRandom)
		: base(input, output, secureRandom)
	{
	}

	public TlsClientProtocol(SecureRandom secureRandom)
		: base(secureRandom)
	{
	}

	public virtual void Connect(TlsClient tlsClient)
	{
		if (tlsClient == null)
		{
			throw new ArgumentNullException("tlsClient");
		}
		if (mTlsClient != null)
		{
			throw new InvalidOperationException("'Connect' can only be called once");
		}
		mTlsClient = tlsClient;
		mSecurityParameters = new SecurityParameters();
		mSecurityParameters.entity = 1;
		mTlsClientContext = new TlsClientContextImpl(mSecureRandom, mSecurityParameters);
		mSecurityParameters.clientRandom = TlsProtocol.CreateRandomBlock(tlsClient.ShouldUseGmtUnixTime(), mTlsClientContext.NonceRandomGenerator);
		mTlsClient.Init(mTlsClientContext);
		mRecordStream.Init(mTlsClientContext);
		tlsClient.NotifyCloseHandle(this);
		TlsSession sessionToResume = tlsClient.GetSessionToResume();
		if (sessionToResume != null && sessionToResume.IsResumable)
		{
			SessionParameters sessionParameters = sessionToResume.ExportSessionParameters();
			if (sessionParameters != null && sessionParameters.IsExtendedMasterSecret)
			{
				mTlsSession = sessionToResume;
				mSessionParameters = sessionParameters;
			}
		}
		SendClientHelloMessage();
		mConnectionState = 1;
		BlockForHandshake();
	}

	protected override void CleanupHandshake()
	{
		base.CleanupHandshake();
		mSelectedSessionID = null;
		mKeyExchange = null;
		mAuthentication = null;
		mCertificateStatus = null;
		mCertificateRequest = null;
	}

	protected override void HandleHandshakeMessage(byte type, MemoryStream buf)
	{
		if (mResumedSession)
		{
			if (type != 20 || mConnectionState != 2)
			{
				throw new TlsFatalAlert(10);
			}
			ProcessFinishedMessage(buf);
			mConnectionState = 15;
			SendChangeCipherSpecMessage();
			SendFinishedMessage();
			mConnectionState = 13;
			CompleteHandshake();
			return;
		}
		switch (type)
		{
		case 11:
			switch (mConnectionState)
			{
			case 2:
			case 3:
				if (mConnectionState == 2)
				{
					HandleSupplementalData(null);
				}
				mPeerCertificate = Certificate.Parse(buf);
				TlsProtocol.AssertEmpty(buf);
				if (mPeerCertificate == null || mPeerCertificate.IsEmpty)
				{
					mAllowCertificateStatus = false;
				}
				mKeyExchange.ProcessServerCertificate(mPeerCertificate);
				mAuthentication = mTlsClient.GetAuthentication();
				mAuthentication.NotifyServerCertificate(mPeerCertificate);
				mConnectionState = 4;
				break;
			default:
				throw new TlsFatalAlert(10);
			}
			break;
		case 22:
		{
			short num = mConnectionState;
			if (num == 4)
			{
				if (!mAllowCertificateStatus)
				{
					throw new TlsFatalAlert(10);
				}
				mCertificateStatus = CertificateStatus.Parse(buf);
				TlsProtocol.AssertEmpty(buf);
				mConnectionState = 5;
				break;
			}
			throw new TlsFatalAlert(10);
		}
		case 20:
			switch (mConnectionState)
			{
			case 13:
			case 14:
				if (mConnectionState == 13 && mExpectSessionTicket)
				{
					throw new TlsFatalAlert(10);
				}
				ProcessFinishedMessage(buf);
				mConnectionState = 15;
				CompleteHandshake();
				break;
			default:
				throw new TlsFatalAlert(10);
			}
			break;
		case 2:
		{
			short num = mConnectionState;
			if (num == 1)
			{
				ReceiveServerHelloMessage(buf);
				mConnectionState = 2;
				mRecordStream.NotifyHelloComplete();
				ApplyMaxFragmentLengthExtension();
				if (mResumedSession)
				{
					mSecurityParameters.masterSecret = Arrays.Clone(mSessionParameters.MasterSecret);
					mRecordStream.SetPendingConnectionState(Peer.GetCompression(), Peer.GetCipher());
					break;
				}
				InvalidateSession();
				if (mSelectedSessionID.Length > 0)
				{
					mTlsSession = new TlsSessionImpl(mSelectedSessionID, null);
				}
				break;
			}
			throw new TlsFatalAlert(10);
		}
		case 23:
		{
			short num = mConnectionState;
			if (num == 2)
			{
				HandleSupplementalData(TlsProtocol.ReadSupplementalDataMessage(buf));
				break;
			}
			throw new TlsFatalAlert(10);
		}
		case 14:
			switch (mConnectionState)
			{
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			{
				if (mConnectionState < 3)
				{
					HandleSupplementalData(null);
				}
				if (mConnectionState < 4)
				{
					mKeyExchange.SkipServerCredentials();
					mAuthentication = null;
				}
				if (mConnectionState < 6)
				{
					mKeyExchange.SkipServerKeyExchange();
				}
				TlsProtocol.AssertEmpty(buf);
				mConnectionState = 8;
				mRecordStream.HandshakeHash.SealHashAlgorithms();
				IList clientSupplementalData = mTlsClient.GetClientSupplementalData();
				if (clientSupplementalData != null)
				{
					SendSupplementalDataMessage(clientSupplementalData);
				}
				mConnectionState = 9;
				TlsCredentials tlsCredentials = null;
				if (mCertificateRequest == null)
				{
					mKeyExchange.SkipClientCredentials();
				}
				else
				{
					tlsCredentials = mAuthentication.GetClientCredentials(mCertificateRequest);
					if (tlsCredentials == null)
					{
						mKeyExchange.SkipClientCredentials();
						SendCertificateMessage(Certificate.EmptyChain);
					}
					else
					{
						mKeyExchange.ProcessClientCredentials(tlsCredentials);
						SendCertificateMessage(tlsCredentials.Certificate);
					}
				}
				mConnectionState = 10;
				SendClientKeyExchangeMessage();
				mConnectionState = 11;
				if (TlsUtilities.IsSsl(Context))
				{
					TlsProtocol.EstablishMasterSecret(Context, mKeyExchange);
				}
				TlsHandshakeHash tlsHandshakeHash = mRecordStream.PrepareToFinish();
				mSecurityParameters.sessionHash = TlsProtocol.GetCurrentPrfHash(Context, tlsHandshakeHash, null);
				if (!TlsUtilities.IsSsl(Context))
				{
					TlsProtocol.EstablishMasterSecret(Context, mKeyExchange);
				}
				mRecordStream.SetPendingConnectionState(Peer.GetCompression(), Peer.GetCipher());
				if (tlsCredentials != null && tlsCredentials is TlsSignerCredentials)
				{
					TlsSignerCredentials tlsSignerCredentials = (TlsSignerCredentials)tlsCredentials;
					SignatureAndHashAlgorithm signatureAndHashAlgorithm = TlsUtilities.GetSignatureAndHashAlgorithm(Context, tlsSignerCredentials);
					byte[] hash = ((signatureAndHashAlgorithm != null) ? tlsHandshakeHash.GetFinalHash(signatureAndHashAlgorithm.Hash) : mSecurityParameters.SessionHash);
					byte[] signature = tlsSignerCredentials.GenerateCertificateSignature(hash);
					DigitallySigned certificateVerify = new DigitallySigned(signatureAndHashAlgorithm, signature);
					SendCertificateVerifyMessage(certificateVerify);
					mConnectionState = 12;
				}
				SendChangeCipherSpecMessage();
				SendFinishedMessage();
				mConnectionState = 13;
				break;
			}
			default:
				throw new TlsFatalAlert(10);
			}
			break;
		case 12:
			switch (mConnectionState)
			{
			case 2:
			case 3:
			case 4:
			case 5:
				if (mConnectionState < 3)
				{
					HandleSupplementalData(null);
				}
				if (mConnectionState < 4)
				{
					mKeyExchange.SkipServerCredentials();
					mAuthentication = null;
				}
				mKeyExchange.ProcessServerKeyExchange(buf);
				TlsProtocol.AssertEmpty(buf);
				mConnectionState = 6;
				break;
			default:
				throw new TlsFatalAlert(10);
			}
			break;
		case 13:
			switch (mConnectionState)
			{
			case 4:
			case 5:
			case 6:
				if (mConnectionState != 6)
				{
					mKeyExchange.SkipServerKeyExchange();
				}
				if (mAuthentication == null)
				{
					throw new TlsFatalAlert(40);
				}
				mCertificateRequest = CertificateRequest.Parse(Context, buf);
				TlsProtocol.AssertEmpty(buf);
				mKeyExchange.ValidateCertificateRequest(mCertificateRequest);
				TlsUtilities.TrackHashAlgorithms(mRecordStream.HandshakeHash, mCertificateRequest.SupportedSignatureAlgorithms);
				mConnectionState = 7;
				break;
			default:
				throw new TlsFatalAlert(10);
			}
			break;
		case 4:
		{
			short num = mConnectionState;
			if (num == 13)
			{
				if (!mExpectSessionTicket)
				{
					throw new TlsFatalAlert(10);
				}
				InvalidateSession();
				ReceiveNewSessionTicketMessage(buf);
				mConnectionState = 14;
				break;
			}
			throw new TlsFatalAlert(10);
		}
		case 0:
			TlsProtocol.AssertEmpty(buf);
			if (mConnectionState == 16)
			{
				RefuseRenegotiation();
			}
			break;
		default:
			throw new TlsFatalAlert(10);
		}
	}

	protected virtual void HandleSupplementalData(IList serverSupplementalData)
	{
		mTlsClient.ProcessServerSupplementalData(serverSupplementalData);
		mConnectionState = 3;
		mKeyExchange = mTlsClient.GetKeyExchange();
		mKeyExchange.Init(Context);
	}

	protected virtual void ReceiveNewSessionTicketMessage(MemoryStream buf)
	{
		NewSessionTicket newSessionTicket = NewSessionTicket.Parse(buf);
		TlsProtocol.AssertEmpty(buf);
		mTlsClient.NotifyNewSessionTicket(newSessionTicket);
	}

	protected virtual void ReceiveServerHelloMessage(MemoryStream buf)
	{
		ProtocolVersion protocolVersion = TlsUtilities.ReadVersion(buf);
		if (protocolVersion.IsDtls)
		{
			throw new TlsFatalAlert(47);
		}
		if (!protocolVersion.Equals(mRecordStream.ReadVersion))
		{
			throw new TlsFatalAlert(47);
		}
		ProtocolVersion clientVersion = Context.ClientVersion;
		if (!protocolVersion.IsEqualOrEarlierVersionOf(clientVersion))
		{
			throw new TlsFatalAlert(47);
		}
		mRecordStream.SetWriteVersion(protocolVersion);
		ContextAdmin.SetServerVersion(protocolVersion);
		mTlsClient.NotifyServerVersion(protocolVersion);
		mSecurityParameters.serverRandom = TlsUtilities.ReadFully(32, buf);
		mSelectedSessionID = TlsUtilities.ReadOpaque8(buf);
		if (mSelectedSessionID.Length > 32)
		{
			throw new TlsFatalAlert(47);
		}
		mTlsClient.NotifySessionID(mSelectedSessionID);
		mResumedSession = mSelectedSessionID.Length > 0 && mTlsSession != null && Arrays.AreEqual(mSelectedSessionID, mTlsSession.SessionID);
		int num = TlsUtilities.ReadUint16(buf);
		if (!Arrays.Contains(mOfferedCipherSuites, num) || num == 0 || CipherSuite.IsScsv(num) || !TlsUtilities.IsValidCipherSuiteForVersion(num, Context.ServerVersion))
		{
			throw new TlsFatalAlert(47);
		}
		mTlsClient.NotifySelectedCipherSuite(num);
		byte b = TlsUtilities.ReadUint8(buf);
		if (!Arrays.Contains(mOfferedCompressionMethods, b))
		{
			throw new TlsFatalAlert(47);
		}
		mTlsClient.NotifySelectedCompressionMethod(b);
		mServerExtensions = TlsProtocol.ReadExtensions(buf);
		mSecurityParameters.extendedMasterSecret = !TlsUtilities.IsSsl(mTlsClientContext) && TlsExtensionsUtilities.HasExtendedMasterSecretExtension(mServerExtensions);
		if (!mSecurityParameters.IsExtendedMasterSecret && (mResumedSession || mTlsClient.RequiresExtendedMasterSecret()))
		{
			throw new TlsFatalAlert(40);
		}
		if (mServerExtensions != null)
		{
			foreach (object key in mServerExtensions.Keys)
			{
				int num2 = (int)key;
				if (num2 != 65281)
				{
					if (TlsUtilities.GetExtensionData(mClientExtensions, num2) == null)
					{
						throw new TlsFatalAlert(110);
					}
					_ = mResumedSession;
				}
			}
		}
		byte[] extensionData = TlsUtilities.GetExtensionData(mServerExtensions, 65281);
		if (extensionData != null)
		{
			mSecureRenegotiation = true;
			if (!Arrays.ConstantTimeAreEqual(extensionData, TlsProtocol.CreateRenegotiationInfo(TlsUtilities.EmptyBytes)))
			{
				throw new TlsFatalAlert(40);
			}
		}
		mTlsClient.NotifySecureRenegotiation(mSecureRenegotiation);
		IDictionary dictionary = mClientExtensions;
		IDictionary dictionary2 = mServerExtensions;
		if (mResumedSession)
		{
			if (num != mSessionParameters.CipherSuite || b != mSessionParameters.CompressionAlgorithm)
			{
				throw new TlsFatalAlert(47);
			}
			dictionary = null;
			dictionary2 = mSessionParameters.ReadServerExtensions();
		}
		mSecurityParameters.cipherSuite = num;
		mSecurityParameters.compressionAlgorithm = b;
		if (dictionary2 != null && dictionary2.Count > 0)
		{
			bool flag = TlsExtensionsUtilities.HasEncryptThenMacExtension(dictionary2);
			if (flag && !TlsUtilities.IsBlockCipherSuite(num))
			{
				throw new TlsFatalAlert(47);
			}
			mSecurityParameters.encryptThenMac = flag;
			mSecurityParameters.maxFragmentLength = ProcessMaxFragmentLengthExtension(dictionary, dictionary2, 47);
			mSecurityParameters.truncatedHMac = TlsExtensionsUtilities.HasTruncatedHMacExtension(dictionary2);
			mAllowCertificateStatus = !mResumedSession && TlsUtilities.HasExpectedEmptyExtensionData(dictionary2, 5, 47);
			mExpectSessionTicket = !mResumedSession && TlsUtilities.HasExpectedEmptyExtensionData(dictionary2, 35, 47);
		}
		if (dictionary != null)
		{
			mTlsClient.ProcessServerExtensions(dictionary2);
		}
		mSecurityParameters.prfAlgorithm = TlsProtocol.GetPrfAlgorithm(Context, mSecurityParameters.CipherSuite);
		mSecurityParameters.verifyDataLength = 12;
	}

	protected virtual void SendCertificateVerifyMessage(DigitallySigned certificateVerify)
	{
		HandshakeMessage handshakeMessage = new HandshakeMessage(15);
		certificateVerify.Encode(handshakeMessage);
		handshakeMessage.WriteToRecordStream(this);
	}

	protected virtual void SendClientHelloMessage()
	{
		mRecordStream.SetWriteVersion(mTlsClient.ClientHelloRecordLayerVersion);
		ProtocolVersion clientVersion = mTlsClient.ClientVersion;
		if (clientVersion.IsDtls)
		{
			throw new TlsFatalAlert(80);
		}
		ContextAdmin.SetClientVersion(clientVersion);
		byte[] array = TlsUtilities.EmptyBytes;
		if (mTlsSession != null)
		{
			array = mTlsSession.SessionID;
			if (array == null || array.Length > 32)
			{
				array = TlsUtilities.EmptyBytes;
			}
		}
		bool isFallback = mTlsClient.IsFallback;
		mOfferedCipherSuites = mTlsClient.GetCipherSuites();
		mOfferedCompressionMethods = mTlsClient.GetCompressionMethods();
		if (array.Length > 0 && mSessionParameters != null && (!mSessionParameters.IsExtendedMasterSecret || !Arrays.Contains(mOfferedCipherSuites, mSessionParameters.CipherSuite) || !Arrays.Contains(mOfferedCompressionMethods, mSessionParameters.CompressionAlgorithm)))
		{
			array = TlsUtilities.EmptyBytes;
		}
		mClientExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(mTlsClient.GetClientExtensions());
		if (!clientVersion.IsSsl)
		{
			TlsExtensionsUtilities.AddExtendedMasterSecretExtension(mClientExtensions);
		}
		HandshakeMessage handshakeMessage = new HandshakeMessage(1);
		TlsUtilities.WriteVersion(clientVersion, handshakeMessage);
		handshakeMessage.Write(mSecurityParameters.ClientRandom);
		TlsUtilities.WriteOpaque8(array, handshakeMessage);
		byte[] extensionData = TlsUtilities.GetExtensionData(mClientExtensions, 65281);
		bool flag = null == extensionData;
		bool flag2 = !Arrays.Contains(mOfferedCipherSuites, 255);
		if (flag && flag2)
		{
			mOfferedCipherSuites = Arrays.Append(mOfferedCipherSuites, 255);
		}
		if (isFallback && !Arrays.Contains(mOfferedCipherSuites, 22016))
		{
			mOfferedCipherSuites = Arrays.Append(mOfferedCipherSuites, 22016);
		}
		TlsUtilities.WriteUint16ArrayWithUint16Length(mOfferedCipherSuites, handshakeMessage);
		TlsUtilities.WriteUint8ArrayWithUint8Length(mOfferedCompressionMethods, handshakeMessage);
		TlsProtocol.WriteExtensions(handshakeMessage, mClientExtensions);
		handshakeMessage.WriteToRecordStream(this);
	}

	protected virtual void SendClientKeyExchangeMessage()
	{
		HandshakeMessage handshakeMessage = new HandshakeMessage(16);
		mKeyExchange.GenerateClientKeyExchange(handshakeMessage);
		handshakeMessage.WriteToRecordStream(this);
	}
}
