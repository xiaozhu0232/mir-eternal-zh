using System;
using System.Collections.Generic;
using System.Linq;

namespace Newtonsoft.Json.Schema;

[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public class JsonSchemaResolver
{
	public IList<JsonSchema> LoadedSchemas { get; protected set; }

	public JsonSchemaResolver()
	{
		LoadedSchemas = new List<JsonSchema>();
	}

	public virtual JsonSchema GetSchema(string reference)
	{
		JsonSchema jsonSchema = LoadedSchemas.SingleOrDefault((JsonSchema s) => string.Equals(s.Id, reference, StringComparison.Ordinal));
		if (jsonSchema == null)
		{
			jsonSchema = LoadedSchemas.SingleOrDefault((JsonSchema s) => string.Equals(s.Location, reference, StringComparison.Ordinal));
		}
		return jsonSchema;
	}
}
