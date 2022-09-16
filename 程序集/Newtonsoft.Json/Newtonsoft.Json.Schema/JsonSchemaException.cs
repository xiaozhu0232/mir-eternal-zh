using System;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Schema;

[Serializable]
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public class JsonSchemaException : JsonException
{
	public int LineNumber { get; }

	public int LinePosition { get; }

	public string Path { get; }

	public JsonSchemaException()
	{
	}

	public JsonSchemaException(string message)
		: base(message)
	{
	}

	public JsonSchemaException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public JsonSchemaException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal JsonSchemaException(string message, Exception innerException, string path, int lineNumber, int linePosition)
		: base(message, innerException)
	{
		Path = path;
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}
}
