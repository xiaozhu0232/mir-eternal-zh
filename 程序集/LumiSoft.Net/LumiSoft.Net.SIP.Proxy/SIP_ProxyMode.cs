using System;

namespace LumiSoft.Net.SIP.Proxy;

[Flags]
public enum SIP_ProxyMode
{
	Registrar = 1,
	Presence = 2,
	Stateless = 4,
	Statefull = 8,
	B2BUA = 0x10
}
