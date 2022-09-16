using System;

namespace LumiSoft.Net.SMTP;

[Flags]
public enum SMTP_DSN_Notify
{
	NotSpecified = 0,
	Never = 0xFF,
	Success = 2,
	Failure = 4,
	Delay = 8
}
