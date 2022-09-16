using System;

namespace LumiSoft.Net;

public interface IAsyncOP
{
	AsyncOP_State State { get; }

	Exception Error { get; }
}
