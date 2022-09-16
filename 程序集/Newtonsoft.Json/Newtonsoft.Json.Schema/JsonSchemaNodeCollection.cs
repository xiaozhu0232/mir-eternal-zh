using System;
using System.Collections.ObjectModel;

namespace Newtonsoft.Json.Schema;

[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
internal class JsonSchemaNodeCollection : KeyedCollection<string, JsonSchemaNode>
{
	protected override string GetKeyForItem(JsonSchemaNode item)
	{
		return item.Id;
	}
}
