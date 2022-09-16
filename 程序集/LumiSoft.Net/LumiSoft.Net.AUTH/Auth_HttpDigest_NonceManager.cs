using System;
using System.Collections.Generic;
using System.Timers;

namespace LumiSoft.Net.AUTH;

public class Auth_HttpDigest_NonceManager : IDisposable
{
	private class NonceEntry
	{
		private string m_Nonce = "";

		private DateTime m_CreateTime;

		public string Nonce => m_Nonce;

		public DateTime CreateTime => m_CreateTime;

		public NonceEntry(string nonce)
		{
			m_Nonce = nonce;
			m_CreateTime = DateTime.Now;
		}
	}

	private List<NonceEntry> m_pNonces;

	private int m_ExpireTime = 30;

	private Timer m_pTimer;

	public int ExpireTime
	{
		get
		{
			return m_ExpireTime;
		}
		set
		{
			if (value < 5)
			{
				throw new ArgumentException("Property ExpireTime value must be >= 5 !");
			}
			m_ExpireTime = value;
		}
	}

	public Auth_HttpDigest_NonceManager()
	{
		m_pNonces = new List<NonceEntry>();
		m_pTimer = new Timer(15000.0);
		m_pTimer.Elapsed += m_pTimer_Elapsed;
		m_pTimer.Enabled = true;
	}

	public void Dispose()
	{
		if (m_pNonces == null)
		{
			m_pNonces.Clear();
			m_pNonces = null;
		}
		if (m_pTimer != null)
		{
			m_pTimer.Dispose();
			m_pTimer = null;
		}
	}

	private void m_pTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		RemoveExpiredNonces();
	}

	public string CreateNonce()
	{
		string text = Guid.NewGuid().ToString().Replace("-", "");
		m_pNonces.Add(new NonceEntry(text));
		return text;
	}

	public bool NonceExists(string nonce)
	{
		lock (m_pNonces)
		{
			foreach (NonceEntry pNonce in m_pNonces)
			{
				if (pNonce.Nonce == nonce)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveNonce(string nonce)
	{
		lock (m_pNonces)
		{
			for (int i = 0; i < m_pNonces.Count; i++)
			{
				if (m_pNonces[i].Nonce == nonce)
				{
					m_pNonces.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private void RemoveExpiredNonces()
	{
		lock (m_pNonces)
		{
			for (int i = 0; i < m_pNonces.Count; i++)
			{
				if (m_pNonces[i].CreateTime.AddSeconds(m_ExpireTime) < DateTime.Now)
				{
					m_pNonces.RemoveAt(i);
					i--;
				}
			}
		}
	}
}
