namespace LumiSoft.Net.DNS;

public enum DNS_QType
{
	A = 1,
	NS = 2,
	CNAME = 5,
	SOA = 6,
	PTR = 12,
	HINFO = 13,
	MX = 15,
	TXT = 16,
	AAAA = 28,
	SRV = 33,
	NAPTR = 35,
	SPF = 99,
	ANY = 255
}
