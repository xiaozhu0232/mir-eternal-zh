using System;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.Utilities;

internal static class ValidationUtils
{
	public static void ArgumentNotNull([NotNull] object? value, string parameterName)
	{
		if (value == null)
		{
			throw new ArgumentNullException(parameterName);
		}
	}
}
