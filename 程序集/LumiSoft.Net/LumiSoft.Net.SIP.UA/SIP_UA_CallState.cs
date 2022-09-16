using System;

namespace LumiSoft.Net.SIP.UA;

[Obsolete("Use SIP stack instead.")]
public enum SIP_UA_CallState
{
	WaitingForStart,
	Calling,
	Ringing,
	Queued,
	WaitingToAccept,
	Active,
	Terminating,
	Terminated,
	Disposed
}
