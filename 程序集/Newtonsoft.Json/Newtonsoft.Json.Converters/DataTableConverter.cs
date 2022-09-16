using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

public class DataTableConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		DataTable obj = (DataTable)value;
		DefaultContractResolver defaultContractResolver = serializer.ContractResolver as DefaultContractResolver;
		writer.WriteStartArray();
		foreach (DataRow row in obj.Rows)
		{
			writer.WriteStartObject();
			foreach (DataColumn column in row.Table.Columns)
			{
				object obj2 = row[column];
				if (serializer.NullValueHandling != NullValueHandling.Ignore || (obj2 != null && obj2 != DBNull.Value))
				{
					writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName(column.ColumnName) : column.ColumnName);
					serializer.Serialize(writer, obj2);
				}
			}
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		DataTable dataTable = existingValue as DataTable;
		if (dataTable == null)
		{
			dataTable = ((objectType == typeof(DataTable)) ? new DataTable() : ((DataTable)Activator.CreateInstance(objectType)));
		}
		if (reader.TokenType == JsonToken.PropertyName)
		{
			dataTable.TableName = (string)reader.Value;
			reader.ReadAndAssert();
			if (reader.TokenType == JsonToken.Null)
			{
				return dataTable;
			}
		}
		if (reader.TokenType != JsonToken.StartArray)
		{
			throw JsonSerializationException.Create(reader, "Unexpected JSON token when reading DataTable. Expected StartArray, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		reader.ReadAndAssert();
		while (reader.TokenType != JsonToken.EndArray)
		{
			CreateRow(reader, dataTable, serializer);
			reader.ReadAndAssert();
		}
		return dataTable;
	}

	private static void CreateRow(JsonReader reader, DataTable dt, JsonSerializer serializer)
	{
		DataRow dataRow = dt.NewRow();
		reader.ReadAndAssert();
		while (reader.TokenType == JsonToken.PropertyName)
		{
			string text = (string)reader.Value;
			reader.ReadAndAssert();
			DataColumn dataColumn = dt.Columns[text];
			if (dataColumn == null)
			{
				Type columnDataType = GetColumnDataType(reader);
				dataColumn = new DataColumn(text, columnDataType);
				dt.Columns.Add(dataColumn);
			}
			if (dataColumn.DataType == typeof(DataTable))
			{
				if (reader.TokenType == JsonToken.StartArray)
				{
					reader.ReadAndAssert();
				}
				DataTable dataTable = new DataTable();
				while (reader.TokenType != JsonToken.EndArray)
				{
					CreateRow(reader, dataTable, serializer);
					reader.ReadAndAssert();
				}
				dataRow[text] = dataTable;
			}
			else if (!dataColumn.DataType.IsArray || !(dataColumn.DataType != typeof(byte[])))
			{
				object obj2 = (dataRow[text] = ((reader.Value != null) ? (serializer.Deserialize(reader, dataColumn.DataType) ?? DBNull.Value) : DBNull.Value));
			}
			else
			{
				if (reader.TokenType == JsonToken.StartArray)
				{
					reader.ReadAndAssert();
				}
				List<object> list = new List<object>();
				while (reader.TokenType != JsonToken.EndArray)
				{
					list.Add(reader.Value);
					reader.ReadAndAssert();
				}
				Array array = Array.CreateInstance(dataColumn.DataType.GetElementType(), list.Count);
				((ICollection)list).CopyTo(array, 0);
				dataRow[text] = array;
			}
			reader.ReadAndAssert();
		}
		dataRow.EndEdit();
		dt.Rows.Add(dataRow);
	}

	private static Type GetColumnDataType(JsonReader reader)
	{
		JsonToken tokenType = reader.TokenType;
		switch (tokenType)
		{
		case JsonToken.Integer:
		case JsonToken.Float:
		case JsonToken.String:
		case JsonToken.Boolean:
		case JsonToken.Date:
		case JsonToken.Bytes:
			return reader.ValueType;
		case JsonToken.Null:
		case JsonToken.Undefined:
		case JsonToken.EndArray:
			return typeof(string);
		case JsonToken.StartArray:
			reader.ReadAndAssert();
			if (reader.TokenType == JsonToken.StartObject)
			{
				return typeof(DataTable);
			}
			return GetColumnDataType(reader).MakeArrayType();
		default:
			throw JsonSerializationException.Create(reader, "Unexpected JSON token when reading DataTable: {0}".FormatWith(CultureInfo.InvariantCulture, tokenType));
		}
	}

	public override bool CanConvert(Type valueType)
	{
		return typeof(DataTable).IsAssignableFrom(valueType);
	}
}
