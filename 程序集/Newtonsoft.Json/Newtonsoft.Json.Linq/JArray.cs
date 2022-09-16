using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

public class JArray : JContainer, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable
{
	private readonly List<JToken> _values = new List<JToken>();

	protected override IList<JToken> ChildrenTokens => _values;

	public override JTokenType Type => JTokenType.Array;

	public override JToken? this[object key]
	{
		get
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int))
			{
				throw new ArgumentException("Accessed JArray values with invalid key value: {0}. Int32 array index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			return GetItem((int)key);
		}
		set
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int))
			{
				throw new ArgumentException("Set JArray values with invalid key value: {0}. Int32 array index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			SetItem((int)key, value);
		}
	}

	public JToken this[int index]
	{
		get
		{
			return GetItem(index);
		}
		set
		{
			SetItem(index, value);
		}
	}

	public bool IsReadOnly => false;

	public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		for (int i = 0; i < _values.Count; i++)
		{
			await _values[i].WriteToAsync(writer, cancellationToken, converters).ConfigureAwait(continueOnCapturedContext: false);
		}
		await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public new static Task<JArray> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return LoadAsync(reader, null, cancellationToken);
	}

	public new static async Task<JArray> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (reader.TokenType == JsonToken.None && !(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader.");
		}
		await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.TokenType != JsonToken.StartArray)
		{
			throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JArray a = new JArray();
		a.SetLineInfo(reader as IJsonLineInfo, settings);
		await a.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return a;
	}

	public JArray()
	{
	}

	public JArray(JArray other)
		: base(other)
	{
	}

	public JArray(params object[] content)
		: this((object)content)
	{
	}

	public JArray(object content)
	{
		Add(content);
	}

	internal override bool DeepEquals(JToken node)
	{
		if (node is JArray container)
		{
			return ContentsEqual(container);
		}
		return false;
	}

	internal override JToken CloneToken()
	{
		return new JArray(this);
	}

	public new static JArray Load(JsonReader reader)
	{
		return Load(reader, null);
	}

	public new static JArray Load(JsonReader reader, JsonLoadSettings? settings)
	{
		if (reader.TokenType == JsonToken.None && !reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader.");
		}
		reader.MoveToContent();
		if (reader.TokenType != JsonToken.StartArray)
		{
			throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JArray jArray = new JArray();
		jArray.SetLineInfo(reader as IJsonLineInfo, settings);
		jArray.ReadTokenFrom(reader, settings);
		return jArray;
	}

	public new static JArray Parse(string json)
	{
		return Parse(json, null);
	}

	public new static JArray Parse(string json, JsonLoadSettings? settings)
	{
		using JsonReader jsonReader = new JsonTextReader(new StringReader(json));
		JArray result = Load(jsonReader, settings);
		while (jsonReader.Read())
		{
		}
		return result;
	}

	public new static JArray FromObject(object o)
	{
		return FromObject(o, JsonSerializer.CreateDefault());
	}

	public new static JArray FromObject(object o, JsonSerializer jsonSerializer)
	{
		JToken jToken = JToken.FromObjectInternal(o, jsonSerializer);
		if (jToken.Type != JTokenType.Array)
		{
			throw new ArgumentException("Object serialized to {0}. JArray instance expected.".FormatWith(CultureInfo.InvariantCulture, jToken.Type));
		}
		return (JArray)jToken;
	}

	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		writer.WriteStartArray();
		for (int i = 0; i < _values.Count; i++)
		{
			_values[i].WriteTo(writer, converters);
		}
		writer.WriteEndArray();
	}

	internal override int IndexOfItem(JToken? item)
	{
		if (item == null)
		{
			return -1;
		}
		return _values.IndexOfReference(item);
	}

	internal override void MergeItem(object content, JsonMergeSettings? settings)
	{
		IEnumerable enumerable = ((IsMultiContent(content) || content is JArray) ? ((IEnumerable)content) : null);
		if (enumerable != null)
		{
			JContainer.MergeEnumerableContent(this, enumerable, settings);
		}
	}

	public int IndexOf(JToken item)
	{
		return IndexOfItem(item);
	}

	public void Insert(int index, JToken item)
	{
		InsertItem(index, item, skipParentCheck: false);
	}

	public void RemoveAt(int index)
	{
		RemoveItemAt(index);
	}

	public IEnumerator<JToken> GetEnumerator()
	{
		return Children().GetEnumerator();
	}

	public void Add(JToken item)
	{
		Add((object?)item);
	}

	public void Clear()
	{
		ClearItems();
	}

	public bool Contains(JToken item)
	{
		return ContainsItem(item);
	}

	public void CopyTo(JToken[] array, int arrayIndex)
	{
		CopyItemsTo(array, arrayIndex);
	}

	public bool Remove(JToken item)
	{
		return RemoveItem(item);
	}

	internal override int GetDeepHashCode()
	{
		return ContentsHashCode();
	}
}
