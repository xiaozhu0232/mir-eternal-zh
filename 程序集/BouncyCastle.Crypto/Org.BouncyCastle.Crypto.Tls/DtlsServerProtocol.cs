using System;
using System.Collections;
using System.IO;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

public class DtlsServerProtocol : DtlsProtocol
{
	protected internal class ServerHandshakeState
	{
		internal TlsServer server = null;

		internal TlsServerContextImpl serverContext = null;

		internal TlsSession tlsSession = null;

		internal SessionParameters sessionParameters = null;

		internal SessionParameters.Builder sessionParametersBuilder = null;

		internal int[] offeredCipherSuites = null;

		internal byte[] offeredCompressionMethods = null;

		internal IDictionary clientExtensions = null;

		internal IDictionary serverExtensions = null;

		internal bool resumedSession = false;

		internal bool secure_renegotiation = false;

		internal bool allowCertificateStatus = false;

		internal bool expectSessionTicket = false;

		internal TlsKeyExchange keyExchange = null;

		internal TlsCredentials serverCredentials = null;

		internal CertificateRequest certificateRequest = null;

		internal short clientCertificateType = -1;

		internal Certificate clientCertificate = null;
	}

	protected bool mVerifyRequests = true;

	public virtual bool VerifyRequests
	{
		get
		{
			return mVerifyRequests;
		}
		set
		{
			mVerifyRequests = value;
		}
	}

	public DtlsServerProtocol(SecureRandom secureRandom)
		: base(secureRandom)
	{
	}

	public virtual DtlsTransport Accept(TlsServer server, DatagramTransport transport)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (transport == null)
		{
			throw new ArgumentNullException("transport");
		}
		SecurityParameters securityParameters = new SecurityParameters();
		securityParameters.entity = 0;
		ServerHandshakeState serverHandshakeState = new ServerHandshakeState();
		serverHandshakeState.server = server;
		serverHandshakeState.serverContext = new TlsServerContextImpl(mSecureRandom, securityParameters);
		securityParameters.serverRandom = TlsProtocol.CreateRandomBlock(server.ShouldUseGmtUnixTime(), serverHandshakeState.serverContext.NonceRandomGenerator);
		server.Init(serverHandshakeState.serverContext);
		DtlsRecordLayer dtlsRecordLayer = new DtlsRecordLayer(transport, serverHandshakeState.serverContext, server, 22);
		server.NotifyCloseHandle(dtlsRecordLayer);
		try
		{
			return ServerHandshake(serverHandshakeState, dtlsRecordLayer);
		}
		catch (TlsFatalAlert tlsFatalAlert)
		{
			AbortServerHandshake(serverHandshakeState, dtlsRecordLayer, tlsFatalAlert.AlertDescription);
			throw tlsFatalAlert;
		}
		catch (IOException ex)
		{
			AbortServerHandshake(serverHandshakeState, dtlsRecordLayer, 80);
			throw ex;
		}
		catch (Exception alertCause)
		{
			AbortServerHandshake(serverHandshakeState, dtlsRecordLayer, 80);
			throw new TlsFatalAlert(80, alertCause);
		}
		finally
		{
			securityParameters.Clear();
		}
	}

	internal virtual void AbortServerHandshake(ServerHandshakeState state, DtlsRecordLayer recordLayer, byte alertDescription)
	{
		recordLayer.Fail(alertDescription);
		InvalidateSession(state);
	}

	internal virtual DtlsTransport ServerHandshake(ServerHandshakeState state, DtlsRecordLayer recordLayer)
	{
		SecurityParameters securityParameters = state.serverContext.SecurityParameters;
		DtlsReliableHandshake dtlsReliableHandshake = new DtlsReliableHandshake(state.serverContext, recordLayer, state.server.GetHandshakeTimeoutMillis());
		DtlsReliableHandshake.Message message = dtlsReliableHandshake.ReceiveMessage();
		if (message.Type == 1)
		{
			ProcessClientHello(state, message.Body);
			byte[] body = GenerateServerHello(state);
			DtlsProtocol.ApplyMaxFragmentLengthExtension(recordLayer, securityParameters.maxFragmentLength);
			ProtocolVersion writeVersion = (recordLayer.ReadVersion = state.serverContext.ServerVersion);
			recordLayer.SetWriteVersion(writeVersion);
			dtlsReliableHandshake.SendMessage(2, body);
			dtlsReliableHandshake.NotifyHelloComplete();
			IList serverSupplementalData = state.server.GetServerSupplementalData();
			if (serverSupplementalData != null)
			{
				byte[] body2 = DtlsProtocol.GenerateSupplementalData(serverSupplementalData);
				dtlsReliableHandshake.SendMessage(23, body2);
			}
			state.keyExchange = state.server.GetKeyExchange();
			state.keyExchange.Init(state.serverContext);
			state.serverCredentials = state.server.GetCredentials();
			Certificate certificate = null;
			if (state.serverCredentials == null)
			{
				state.keyExchange.SkipServerCredentials();
			}
			else
			{
				state.keyExchange.ProcessServerCredentials(state.serverCredentials);
				certificate = state.serverCredentials.Certificate;
				byte[] body3 = DtlsProtocol.GenerateCertificate(certificate);
				dtlsReliableHandshake.SendMessage(11, body3);
			}
			if (certificate == null || certificate.IsEmpty)
			{
				state.allowCertificateStatus = false;
			}
			if (state.allowCertificateStatus)
			{
				CertificateStatus certificateStatus = state.server.GetCertificateStatus();
				if (certificateStatus != null)
				{
					byte[] body4 = GenerateCertificateStatus(state, certificateStatus);
					dtlsReliableHandshake.SendMessage(22, body4);
				}
			}
			byte[] array = state.keyExchange.GenerateServerKeyExchange();
			if (array != null)
			{
				dtlsReliableHandshake.SendMessage(12, array);
			}
			if (state.serverCredentials != null)
			{
				state.certificateRequest = state.server.GetCertificateRequest();
				if (state.certificateRequest != null)
				{
					if (TlsUtilities.IsTlsV12(state.serverContext) != (state.certificateRequest.SupportedSignatureAlgorithms != null))
					{
						throw new TlsFatalAlert(80);
					}
					state.keyExchange.ValidateCertificateRequest(state.certificateRequest);
					byte[] body5 = GenerateCertificateRequest(state, state.certificateRequest);
					dtlsReliableHandshake.SendMessage(13, body5);
					TlsUtilities.TrackHashAlgorithms(dtlsReliableHandshake.HandshakeHash, state.certificateRequest.SupportedSignatureAlgorithms);
				}
			}
			dtlsReliableHandshake.SendMessage(14, TlsUtilities.EmptyBytes);
			dtlsReliableHandshake.HandshakeHash.SealHashAlgorithms();
			message = dtlsReliableHandshake.ReceiveMessage();
			if (message.Type == 23)
			{
				ProcessClientSupplementalData(state, message.Body);
				message = dtlsReliableHandshake.ReceiveMessage();
			}
			else
			{
				state.server.ProcessClientSupplementalData(null);
			}
			if (state.certificateRequest == null)
			{
				state.keyExchange.SkipClientCredentials();
			}
			else if (message.Type == 11)
			{
				ProcessClientCertificate(state, message.Body);
				message = dtlsReliableHandshake.ReceiveMessage();
			}
			else
			{
				if (TlsUtilities.IsTlsV12(state.serverContext))
				{
					throw new TlsFatalAlert(10);
				}
				NotifyClientCertificate(state, Certificate.EmptyChain);
			}
			if (message.Type == 16)
			{
				ProcessClientKeyExchange(state, message.Body);
				TlsHandshakeHash tlsHandshakeHash = dtlsReliableHandshake.PrepareToFinish();
				securityParameters.sessionHash = TlsProtocol.GetCurrentPrfHash(state.serverContext, tlsHandshakeHash, null);
				TlsProtocol.EstablishMasterSecret(state.serverContext, state.keyExchange);
				recordLayer.InitPendingEpoch(state.server.GetCipher());
				if (ExpectCertificateVerifyMessage(state))
				{
					byte[] body6 = dtlsReliableHandshake.ReceiveMessageBody(15);
					ProcessCertificateVerify(state, body6, tlsHandshakeHash);
				}
				byte[] expected_verify_data = TlsUtilities.CalculateVerifyData(state.serverContext, "client finished", TlsProtocol.GetCurrentPrfHash(state.serverContext, dtlsReliableHandshake.HandshakeHash, null));
				ProcessFinished(dtlsReliableHandshake.ReceiveMessageBody(20), expected_verify_data);
				if (state.expectSessionTicket)
				{
					NewSessionTicket newSessionTicket = state.server.GetNewSessionTicket();
					byte[] body7 = GenerateNewSessionTicket(state, newSessionTicket);
					dtlsReliableHandshake.SendMessage(4, body7);
				}
				byte[] body8 = TlsUtilities.CalculateVerifyData(state.serverContext, "server finished", TlsProtocol.GetCurrentPrfHash(state.serverContext, dtlsReliableHandshake.HandshakeHash, null));
				dtlsReliableHandshake.SendMessage(20, body8);
				dtlsReliableHandshake.Finish();
				state.server.NotifyHandshakeComplete();
				return new DtlsTransport(recordLayer);
			}
			throw new TlsFatalAlert(10);
		}
		throw new TlsFatalAlert(10);
	}

	protected virtual void InvalidateSession(ServerHandshakeState state)
	{
		if (state.sessionParameters != null)
		{
			state.sessionParameters.Clear();
			state.sessionParameters = null;
		}
		if (state.tlsSession != null)
		{
			state.tlsSession.Invalidate();
			state.tlsSession = null;
		}
	}

	protected virtual byte[] GenerateCertificateRequest(ServerHandshakeState state, CertificateRequest certificateRequest)
	{
		MemoryStream memoryStream = new MemoryStream();
		certificateRequest.Encode(memoryStream);
		return memoryStream.ToArray();
	}

	protected virtual byte[] GenerateCertificateStatus(ServerHandshakeState state, CertificateStatus certificateStatus)
	{
		MemoryStream memoryStream = new MemoryStream();
		certificateStatus.Encode(memoryStream);
		return memoryStream.ToArray();
	}

	protected virtual byte[] GenerateNewSessionTicket(ServerHandshakeState state, NewSessionTicket newSessionTicket)
	{
		MemoryStream memoryStream = new MemoryStream();
		newSessionTicket.Encode(memoryStream);
		return memoryStream.ToArray();
	}

	protected virtual byte[] GenerateServerHello(ServerHandshakeState state)
	{
		SecurityParameters securityParameters = state.serverContext.SecurityParameters;
		MemoryStream memoryStream = new MemoryStream();
		ProtocolVersion serverVersion = state.server.GetServerVersion();
		if (!serverVersion.IsEqualOrEarlierVersionOf(state.serverContext.ClientVersion))
		{
			throw new TlsFatalAlert(80);
		}
		state.serverContext.SetServerVersion(serverVersion);
		TlsUtilities.WriteVersion(state.serverContext.ServerVersion, memoryStream);
		memoryStream.Write(securityParameters.ServerRandom, 0, securityParameters.ServerRandom.Length);
		TlsUtilities.WriteOpaque8(TlsUtilities.EmptyBytes, memoryStream);
		int selectedCipherSuite = state.server.GetSelectedCipherSuite();
		if (!Arrays.Contains(state.offeredCipherSuites, selectedCipherSuite) || selectedCipherSuite == 0 || CipherSuite.IsScsv(selectedCipherSuite) || !TlsUtilities.IsValidCipherSuiteForVersion(selectedCipherSuite, state.serverContext.ServerVersion))
		{
			throw new TlsFatalAlert(80);
		}
		DtlsProtocol.ValidateSelectedCipherSuite(selectedCipherSuite, 80);
		securityParameters.cipherSuite = selectedCipherSuite;
		byte selectedCompressionMethod = state.server.GetSelectedCompressionMethod();
		if (!Arrays.Contains(state.offeredCompressionMethods, selectedCompressionMethod))
		{
			throw new TlsFatalAlert(80);
		}
		securityParameters.compressionAlgorithm = selectedCompressionMethod;
		TlsUtilities.WriteUint16(selectedCipherSuite, memoryStream);
		TlsUtilities.WriteUint8(selectedCompressionMethod, memoryStream);
		state.serverExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(state.server.GetServerExtensions());
		if (state.secure_renegotiation)
		{
			byte[] extensionData = TlsUtilities.GetExtensionData(state.serverExtensions, 65281);
			if (null == extensionData)
			{
				state.serverExtensions[65281] = TlsProtocol.CreateRenegotiationInfo(TlsUtilities.EmptyBytes);
			}
		}
		if (securityParameters.IsExtendedMasterSecret)
		{
			TlsExtensionsUtilities.AddExtendedMasterSecretExtension(state.serverExtensions);
		}
		if (state.serverExtensions.Count > 0)
		{
			securityParameters.encryptThenMac = TlsExtensionsUtilities.HasEncryptThenMacExtension(state.serverExtensions);
			securityParameters.maxFragmentLength = DtlsProtocol.EvaluateMaxFragmentLengthExtension(state.resumedSession, state.clientExtensions, state.serverExtensions, 80);
			securityParameters.truncatedHMac = TlsExtensionsUtilities.HasTruncatedHMacExtension(state.serverExtensions);
			state.allowCertificateStatus = !state.resumedSession && TlsUtilities.HasExpectedEmptyExtensionData(state.serverExtensions, 5, 80);
			state.expectSessionTicket = !state.resumedSession && TlsUtilities.HasExpectedEmptyExtensionData(state.serverExtensions, 35, 80);
			TlsProtocol.WriteExtensions(memoryStream, state.serverExtensions);
		}
		securityParameters.prfAlgorithm = TlsProtocol.GetPrfAlgorithm(state.serverContext, securityParameters.CipherSuite);
		securityParameters.verifyDataLength = 12;
		return memoryStream.ToArray();
	}

	protected virtual void NotifyClientCertificate(ServerHandshakeState state, Certificate clientCertificate)
	{
		if (state.certificateRequest == null)
		{
			throw new InvalidOperationException();
		}
		if (state.clientCertificate != null)
		{
			throw new TlsFatalAlert(10);
		}
		state.clientCertificate = clientCertificate;
		if (clientCertificate.IsEmpty)
		{
			state.keyExchange.SkipClientCredentials();
		}
		else
		{
			state.clientCertificateType = TlsUtilities.GetClientCertificateType(clientCertificate, state.serverCredentials.Certificate);
			state.keyExchange.ProcessClientCertificate(clientCertificate);
		}
		state.server.NotifyClientCertificate(clientCertificate);
	}

	protected virtual void ProcessClientCertificate(ServerHandshakeState state, byte[] body)
	{
		MemoryStream memoryStream = new MemoryStream(body, writable: false);
		Certificate clientCertificate = Certificate.Parse(memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		NotifyClientCertificate(state, clientCertificate);
	}

	protected virtual void ProcessCertificateVerify(ServerHandshakeState state, byte[] body, TlsHandshakeHash prepareFinishHash)
	{
		if (state.certificateRequest == null)
		{
			throw new InvalidOperationException();
		}
		MemoryStream memoryStream = new MemoryStream(body, writable: false);
		TlsServerContextImpl serverContext = state.serverContext;
		DigitallySigned digitallySigned = DigitallySigned.Parse(serverContext, memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
		try
		{
			SignatureAndHashAlgorithm algorithm = digitallySigned.Algorithm;
			byte[] hash;
			if (TlsUtilities.IsTlsV12(serverContext))
			{
				TlsUtilities.VerifySupportedSignatureAlgorithm(state.certificateRequest.SupportedSignatureAlgorithms, algorithm);
				hash = prepareFinishHash.GetFinalHash(algorithm.Hash);
			}
			else
			{
				hash = serverContext.SecurityParameters.SessionHash;
			}
			X509CertificateStructure certificateAt = state.clientCertificate.GetCertificateAt(0);
			SubjectPublicKeyInfo subjectPublicKeyInfo = certificateAt.SubjectPublicKeyInfo;
			AsymmetricKeyParameter publicKey = PublicKeyFactory.CreateKey(subjectPublicKeyInfo);
			TlsSigner tlsSigner = TlsUtilities.CreateTlsSigner((byte)state.clientCertificateType);
			tlsSigner.Init(serverContext);
			if (!tlsSigner.VerifyRawSignature(algorithm, digitallySigned.Signature, publicKey, hash))
			{
				throw new TlsFatalAlert(51);
			}
		}
		catch (TlsFatalAlert tlsFatalAlert)
		{
			throw tlsFatalAlert;
		}
		catch (Exception alertCause)
		{
			throw new TlsFatalAlert(51, alertCause);
		}
	}

	protected virtual void ProcessClientHello(ServerHandshakeState state, byte[] body)
	{
		MemoryStream input = new MemoryStream(body, writable: false);
		ProtocolVersion protocolVersion = TlsUtilities.ReadVersion(input);
		if (!protocolVersion.IsDtls)
		{
			throw new TlsFatalAlert(47);
		}
		byte[] clientRandom = TlsUtilities.ReadFully(32, input);
		byte[] array = TlsUtilities.ReadOpaque8(input);
		if (array.Length > 32)
		{
			throw new TlsFatalAlert(47);
		}
		TlsUtilities.ReadOpaque8(input);
		int num = TlsUtilities.ReadUint16(input);
		if (num < 2 || ((uint)num & (true ? 1u : 0u)) != 0)
		{
			throw new TlsFatalAlert(50);
		}
		state.offeredCipherSuites = TlsUtilities.ReadUint16Array(num / 2, input);
		int num2 = TlsUtilities.ReadUint8(input);
		if (num2 < 1)
		{
			throw new TlsFatalAlert(47);
		}
		state.offeredCompressionMethods = TlsUtilities.ReadUint8Array(num2, input);
		state.clientExtensions = TlsProtocol.ReadExtensions(input);
		TlsServerContextImpl serverContext = state.serverContext;
		SecurityParameters securityParameters = serverContext.SecurityParameters;
		securityParameters.extendedMasterSecret = TlsExtensionsUtilities.HasExtendedMasterSecretExtension(state.clientExtensions);
		if (!securityParameters.IsExtendedMasterSecret && state.server.RequiresExtendedMasterSecret())
		{
			throw new TlsFatalAlert(40);
		}
		serverContext.SetClientVersion(protocolVersion);
		state.server.NotifyClientVersion(protocolVersion);
		state.server.NotifyFallback(Arrays.Contains(state.offeredCipherSuites, 22016));
		securityParameters.clientRandom = clientRandom;
		state.server.NotifyOfferedCipherSuites(state.offeredCipherSuites);
		state.server.NotifyOfferedCompressionMethods(state.offeredCompressionMethods);
		if (Arrays.Contains(state.offeredCipherSuites, 255))
		{
			state.secure_renegotiation = true;
		}
		byte[] extensionData = TlsUtilities.GetExtensionData(state.clientExtensions, 65281);
		if (extensionData != null)
		{
			state.secure_renegotiation = true;
			if (!Arrays.ConstantTimeAreEqual(extensionData, TlsProtocol.CreateRenegotiationInfo(TlsUtilities.EmptyBytes)))
			{
				throw new TlsFatalAlert(40);
			}
		}
		state.server.NotifySecureRenegotiation(state.secure_renegotiation);
		if (state.clientExtensions != null)
		{
			TlsExtensionsUtilities.GetPaddingExtension(state.clientExtensions);
			state.server.ProcessClientExtensions(state.clientExtensions);
		}
	}

	protected virtual void ProcessClientKeyExchange(ServerHandshakeState state, byte[] body)
	{
		MemoryStream memoryStream = new MemoryStream(body, writable: false);
		state.keyExchange.ProcessClientKeyExchange(memoryStream);
		TlsProtocol.AssertEmpty(memoryStream);
	}

	protected virtual void ProcessClientSupplementalData(ServerHandshakeState state, byte[] body)
	{
		MemoryStream input = new MemoryStream(body, writable: false);
		IList clientSupplementalData = TlsProtocol.ReadSupplementalDataMessage(input);
		state.server.ProcessClientSupplementalData(clientSupplementalData);
	}

	protected virtual bool ExpectCertificateVerifyMessage(ServerHandshakeState state)
	{
		if (state.clientCertificateType >= 0)
		{
			return TlsUtilities.HasSigningCapability((byte)state.clientCertificateType);
		}
		return false;
	}
}
