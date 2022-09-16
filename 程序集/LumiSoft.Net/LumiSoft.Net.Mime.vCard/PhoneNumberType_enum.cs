using System;

namespace LumiSoft.Net.Mime.vCard;

[Flags]
public enum PhoneNumberType_enum
{
	NotSpecified = 0,
	Preferred = 1,
	Home = 2,
	Msg = 4,
	Work = 8,
	Voice = 0x10,
	Fax = 0x20,
	Cellular = 0x40,
	Video = 0x80,
	Pager = 0x100,
	BBS = 0x200,
	Modem = 0x400,
	Car = 0x800,
	ISDN = 0x1000,
	PCS = 0x2000
}
