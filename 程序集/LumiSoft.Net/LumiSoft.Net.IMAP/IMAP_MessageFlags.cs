using System;

namespace LumiSoft.Net.IMAP;

[Obsolete("Use IMAP_t_MsgFlags instead.")]
public enum IMAP_MessageFlags
{
	None = 0,
	Seen = 2,
	Answered = 4,
	Flagged = 8,
	Deleted = 0x10,
	Draft = 0x20,
	Recent = 0x40
}
