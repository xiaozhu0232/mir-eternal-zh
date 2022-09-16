using System;

namespace LumiSoft.Net.IMAP;

[Obsolete("Use Fetch(bool uid,IMAP_t_SeqSet seqSet,IMAP_t_Fetch_i[] items,EventHandler<EventArgs<IMAP_r_u>> callback) intead.")]
public abstract class IMAP_Fetch_DataItem
{
}
