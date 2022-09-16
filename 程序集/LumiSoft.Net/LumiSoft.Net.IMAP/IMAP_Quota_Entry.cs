using System;

namespace LumiSoft.Net.IMAP;

public class IMAP_Quota_Entry
{
	private string m_ResourceName = "";

	private long m_CurrentUsage;

	private long m_MaxUsage;

	public string ResourceName => m_ResourceName;

	public long CurrentUsage => m_CurrentUsage;

	public long MaxUsage => m_MaxUsage;

	public IMAP_Quota_Entry(string resourceName, long currentUsage, long maxUsage)
	{
		if (resourceName == null)
		{
			throw new ArgumentNullException("resourceName");
		}
		if (resourceName == string.Empty)
		{
			throw new ArgumentException("Argument 'resourceName' value must be specified.", "resourceName");
		}
		m_ResourceName = resourceName;
		m_CurrentUsage = currentUsage;
		m_MaxUsage = maxUsage;
	}
}
