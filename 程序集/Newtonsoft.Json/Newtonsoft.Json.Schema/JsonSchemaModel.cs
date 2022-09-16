using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema;

[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
internal class JsonSchemaModel
{
	public bool Required { get; set; }

	public JsonSchemaType Type { get; set; }

	public int? MinimumLength { get; set; }

	public int? MaximumLength { get; set; }

	public double? DivisibleBy { get; set; }

	public double? Minimum { get; set; }

	public double? Maximum { get; set; }

	public bool ExclusiveMinimum { get; set; }

	public bool ExclusiveMaximum { get; set; }

	public int? MinimumItems { get; set; }

	public int? MaximumItems { get; set; }

	public IList<string> Patterns { get; set; }

	public IList<JsonSchemaModel> Items { get; set; }

	public IDictionary<string, JsonSchemaModel> Properties { get; set; }

	public IDictionary<string, JsonSchemaModel> PatternProperties { get; set; }

	public JsonSchemaModel AdditionalProperties { get; set; }

	public JsonSchemaModel AdditionalItems { get; set; }

	public bool PositionalItemsValidation { get; set; }

	public bool AllowAdditionalProperties { get; set; }

	public bool AllowAdditionalItems { get; set; }

	public bool UniqueItems { get; set; }

	public IList<JToken> Enum { get; set; }

	public JsonSchemaType Disallow { get; set; }

	public JsonSchemaModel()
	{
		Type = JsonSchemaType.Any;
		AllowAdditionalProperties = true;
		AllowAdditionalItems = true;
		Required = false;
	}

	public static JsonSchemaModel Create(IList<JsonSchema> schemata)
	{
		JsonSchemaModel jsonSchemaModel = new JsonSchemaModel();
		foreach (JsonSchema schematum in schemata)
		{
			Combine(jsonSchemaModel, schematum);
		}
		return jsonSchemaModel;
	}

	private static void Combine(JsonSchemaModel model, JsonSchema schema)
	{
		model.Required = model.Required || schema.Required.GetValueOrDefault();
		model.Type &= schema.Type ?? JsonSchemaType.Any;
		model.MinimumLength = MathUtils.Max(model.MinimumLength, schema.MinimumLength);
		model.MaximumLength = MathUtils.Min(model.MaximumLength, schema.MaximumLength);
		model.DivisibleBy = MathUtils.Max(model.DivisibleBy, schema.DivisibleBy);
		model.Minimum = MathUtils.Max(model.Minimum, schema.Minimum);
		model.Maximum = MathUtils.Max(model.Maximum, schema.Maximum);
		model.ExclusiveMinimum = model.ExclusiveMinimum || schema.ExclusiveMinimum.GetValueOrDefault();
		model.ExclusiveMaximum = model.ExclusiveMaximum || schema.ExclusiveMaximum.GetValueOrDefault();
		model.MinimumItems = MathUtils.Max(model.MinimumItems, schema.MinimumItems);
		model.MaximumItems = MathUtils.Min(model.MaximumItems, schema.MaximumItems);
		model.PositionalItemsValidation = model.PositionalItemsValidation || schema.PositionalItemsValidation;
		model.AllowAdditionalProperties = model.AllowAdditionalProperties && schema.AllowAdditionalProperties;
		model.AllowAdditionalItems = model.AllowAdditionalItems && schema.AllowAdditionalItems;
		model.UniqueItems = model.UniqueItems || schema.UniqueItems;
		if (schema.Enum != null)
		{
			if (model.Enum == null)
			{
				model.Enum = new List<JToken>();
			}
			model.Enum.AddRangeDistinct(schema.Enum, JToken.EqualityComparer);
		}
		model.Disallow |= schema.Disallow.GetValueOrDefault();
		if (schema.Pattern != null)
		{
			if (model.Patterns == null)
			{
				model.Patterns = new List<string>();
			}
			model.Patterns.AddDistinct(schema.Pattern);
		}
	}
}
