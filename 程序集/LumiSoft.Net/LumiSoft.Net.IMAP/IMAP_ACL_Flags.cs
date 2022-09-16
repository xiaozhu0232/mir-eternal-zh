namespace LumiSoft.Net.IMAP;

public enum IMAP_ACL_Flags
{
	None = 0,
	l = 1,
	r = 2,
	s = 4,
	w = 8,
	i = 16,
	p = 32,
	c = 64,
	d = 128,
	a = 256,
	All = 65535
}
