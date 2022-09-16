using System;

namespace LumiSoft.Net.IMAP;

public abstract class IMAP_t_orc
{
	public static IMAP_t_orc Parse(StringReader r)
	{
		if (r == null)
		{
			throw new ArgumentNullException("r");
		}
		r.ReadToFirstChar();
		if (r.StartsWith("[ALERT", case_sensitive: false))
		{
			return IMAP_t_orc_Alert.Parse(r);
		}
		if (r.StartsWith("[BADCHARSET", case_sensitive: false))
		{
			return IMAP_t_orc_BadCharset.Parse(r);
		}
		if (r.StartsWith("[CAPABILITY", case_sensitive: false))
		{
			return IMAP_t_orc_Capability.Parse(r);
		}
		if (r.StartsWith("[PARSE", case_sensitive: false))
		{
			return IMAP_t_orc_Parse.Parse(r);
		}
		if (r.StartsWith("[PERMANENTFLAGS", case_sensitive: false))
		{
			return IMAP_t_orc_PermanentFlags.Parse(r);
		}
		if (r.StartsWith("[READ-ONLY", case_sensitive: false))
		{
			return IMAP_t_orc_ReadOnly.Parse(r);
		}
		if (r.StartsWith("[READ-WRITE", case_sensitive: false))
		{
			return IMAP_t_orc_ReadWrite.Parse(r);
		}
		if (r.StartsWith("[TRYCREATE", case_sensitive: false))
		{
			return IMAP_t_orc_TryCreate.Parse(r);
		}
		if (r.StartsWith("[UIDNEXT", case_sensitive: false))
		{
			return IMAP_t_orc_UidNext.Parse(r);
		}
		if (r.StartsWith("[UIDVALIDITY", case_sensitive: false))
		{
			return IMAP_t_orc_UidValidity.Parse(r);
		}
		if (r.StartsWith("[UNSEEN", case_sensitive: false))
		{
			return IMAP_t_orc_Unseen.Parse(r);
		}
		if (r.StartsWith("[APPENDUID", case_sensitive: false))
		{
			return IMAP_t_orc_AppendUid.Parse(r);
		}
		if (r.StartsWith("[COPYUID", case_sensitive: false))
		{
			return IMAP_t_orc_CopyUid.Parse(r);
		}
		return IMAP_t_orc_Unknown.Parse(r);
	}
}
