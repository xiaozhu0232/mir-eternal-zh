using System;

namespace Newtonsoft.Json.Linq;

public class JsonSelectSettings
{
	public TimeSpan? RegexMatchTimeout { get; set; }

	public bool ErrorWhenNoMatch { get; set; }
}
