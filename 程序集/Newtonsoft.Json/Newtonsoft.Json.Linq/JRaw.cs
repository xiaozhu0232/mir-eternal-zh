using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Linq;

public class JRaw : JValue
{
	public static async Task<JRaw> CreateAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		using StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
		using JsonTextWriter jsonWriter = new JsonTextWriter(sw);
		await jsonWriter.WriteTokenSyncReadingAsync(reader, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new JRaw(sw.ToString());
	}

	public JRaw(JRaw other)
		: base(other)
	{
	}

	public JRaw(object? rawJson)
		: base(rawJson, JTokenType.Raw)
	{
	}

	public static JRaw Create(JsonReader reader)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		using JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
		jsonTextWriter.WriteToken(reader);
		return new JRaw(stringWriter.ToString());
	}

	internal override JToken CloneToken()
	{
		return new JRaw(this);
	}
}
