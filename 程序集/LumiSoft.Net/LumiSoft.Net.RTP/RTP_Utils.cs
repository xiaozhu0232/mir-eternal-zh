using System;
using System.Net;

namespace LumiSoft.Net.RTP;

public class RTP_Utils
{
	public static uint GenerateSSRC()
	{
		return (uint)new Random().Next(100000, int.MaxValue);
	}

	public static string GenerateCNAME()
	{
		return Environment.UserName + "@" + Dns.GetHostName() + "." + Guid.NewGuid().ToString().Substring(0, 8);
	}

	public static uint DateTimeToNTP32(DateTime value)
	{
		return (uint)((DateTimeToNTP64(value) >> 16) & 0xFFFFFFFFu);
	}

	public static ulong DateTimeToNTP64(DateTime value)
	{
		TimeSpan timeSpan = value.ToUniversalTime() - new DateTime(1900, 1, 1, 0, 0, 0);
		return ((ulong)(timeSpan.TotalMilliseconds % 1000.0) << 32) | (uint)(timeSpan.Milliseconds << 22);
	}
}
