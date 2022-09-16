using System;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls;

internal class TlsSessionImpl : TlsSession
{
	internal readonly byte[] mSessionID;

	internal readonly SessionParameters mSessionParameters;

	internal bool mResumable;

	public virtual byte[] SessionID
	{
		get
		{
			lock (this)
			{
				return mSessionID;
			}
		}
	}

	public virtual bool IsResumable
	{
		get
		{
			lock (this)
			{
				return mResumable;
			}
		}
	}

	internal TlsSessionImpl(byte[] sessionID, SessionParameters sessionParameters)
	{
		if (sessionID == null)
		{
			throw new ArgumentNullException("sessionID");
		}
		if (sessionID.Length > 32)
		{
			throw new ArgumentException("cannot be longer than 32 bytes", "sessionID");
		}
		mSessionID = Arrays.Clone(sessionID);
		mSessionParameters = sessionParameters;
		mResumable = sessionID.Length > 0 && sessionParameters != null && sessionParameters.IsExtendedMasterSecret;
	}

	public virtual SessionParameters ExportSessionParameters()
	{
		lock (this)
		{
			return (mSessionParameters == null) ? null : mSessionParameters.Copy();
		}
	}

	public virtual void Invalidate()
	{
		lock (this)
		{
			mResumable = false;
		}
	}
}
