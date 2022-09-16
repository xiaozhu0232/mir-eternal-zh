namespace Newtonsoft.Json.Utilities;

internal static class JsonTokenUtils
{
	internal static bool IsEndToken(JsonToken token)
	{
		if ((uint)(token - 13) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsStartToken(JsonToken token)
	{
		if ((uint)(token - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPrimitiveToken(JsonToken token)
	{
		if ((uint)(token - 7) <= 5u || (uint)(token - 16) <= 1u)
		{
			return true;
		}
		return false;
	}
}
