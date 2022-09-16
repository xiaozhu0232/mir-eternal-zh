using System;

namespace Newtonsoft.Json.Schema;

[Flags]
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public enum JsonSchemaType
{
	None = 0,
	String = 1,
	Float = 2,
	Integer = 4,
	Boolean = 8,
	Object = 0x10,
	Array = 0x20,
	Null = 0x40,
	Any = 0x7F
}
