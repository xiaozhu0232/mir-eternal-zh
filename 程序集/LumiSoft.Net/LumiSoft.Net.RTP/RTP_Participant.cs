using System;
using System.Collections.Generic;

namespace LumiSoft.Net.RTP;

public abstract class RTP_Participant
{
	private string m_CNAME = "";

	private List<RTP_Source> m_pSources;

	private object m_pTag;

	public string CNAME => m_CNAME;

	public RTP_Source[] Sources => m_pSources.ToArray();

	public object Tag
	{
		get
		{
			return m_pTag;
		}
		set
		{
			m_pTag = value;
		}
	}

	public event EventHandler Removed;

	public event EventHandler<RTP_SourceEventArgs> SourceAdded;

	public event EventHandler<RTP_SourceEventArgs> SourceRemoved;

	public RTP_Participant(string cname)
	{
		if (cname == null)
		{
			throw new ArgumentNullException("cname");
		}
		if (cname == string.Empty)
		{
			throw new ArgumentException("Argument 'cname' value must be specified.");
		}
		m_CNAME = cname;
		m_pSources = new List<RTP_Source>();
	}

	internal void Dispose()
	{
		m_pSources = null;
		this.Removed = null;
		this.SourceAdded = null;
		this.SourceRemoved = null;
	}

	internal void EnsureSource(RTP_Source source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (m_pSources.Contains(source))
		{
			return;
		}
		m_pSources.Add(source);
		OnSourceAdded(source);
		source.Disposing += delegate
		{
			if (m_pSources.Remove(source))
			{
				OnSourceRemoved(source);
				if (m_pSources.Count == 0)
				{
					OnRemoved();
					Dispose();
				}
			}
		};
	}

	private void OnRemoved()
	{
		if (this.Removed != null)
		{
			this.Removed(this, new EventArgs());
		}
	}

	private void OnSourceAdded(RTP_Source source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (this.SourceAdded != null)
		{
			this.SourceAdded(this, new RTP_SourceEventArgs(source));
		}
	}

	private void OnSourceRemoved(RTP_Source source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (this.SourceRemoved != null)
		{
			this.SourceRemoved(this, new RTP_SourceEventArgs(source));
		}
	}
}
