using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema;

[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public static class Extensions
{
	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static bool IsValid(this JToken source, JsonSchema schema)
	{
		bool valid = true;
		source.Validate(schema, delegate
		{
			valid = false;
		});
		return valid;
	}

	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static bool IsValid(this JToken source, JsonSchema schema, out IList<string> errorMessages)
	{
		IList<string> errors = new List<string>();
		source.Validate(schema, delegate(object sender, ValidationEventArgs args)
		{
			errors.Add(args.Message);
		});
		errorMessages = errors;
		return errorMessages.Count == 0;
	}

	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static void Validate(this JToken source, JsonSchema schema)
	{
		source.Validate(schema, null);
	}

	[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
	public static void Validate(this JToken source, JsonSchema schema, ValidationEventHandler validationEventHandler)
	{
		ValidationUtils.ArgumentNotNull(source, "source");
		ValidationUtils.ArgumentNotNull(schema, "schema");
		using JsonValidatingReader jsonValidatingReader = new JsonValidatingReader(source.CreateReader());
		jsonValidatingReader.Schema = schema;
		if (validationEventHandler != null)
		{
			jsonValidatingReader.ValidationEventHandler += validationEventHandler;
		}
		while (jsonValidatingReader.Read())
		{
		}
	}
}
