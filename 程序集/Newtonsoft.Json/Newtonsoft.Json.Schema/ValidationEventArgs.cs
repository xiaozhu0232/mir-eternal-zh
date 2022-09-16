using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema;

[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public class ValidationEventArgs : EventArgs
{
	private readonly JsonSchemaException _ex;

	public JsonSchemaException Exception => _ex;

	public string Path => _ex.Path;

	public string Message => _ex.Message;

	internal ValidationEventArgs(JsonSchemaException ex)
	{
		ValidationUtils.ArgumentNotNull(ex, "ex");
		_ex = ex;
	}
}
