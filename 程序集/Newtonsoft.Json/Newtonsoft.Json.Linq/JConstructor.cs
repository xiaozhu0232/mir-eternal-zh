using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

public class JConstructor : JContainer
{
	private string? _name;

	private readonly List<JToken> _values = new List<JToken>();

	protected override IList<JToken> ChildrenTokens => _values;

	public string? Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public override JTokenType Type => JTokenType.Constructor;

	public override JToken? this[object key]
	{
		get
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int index))
			{
				throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			return GetItem(index);
		}
		set
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int index))
			{
				throw new ArgumentException("Set JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			SetItem(index, value);
		}
	}

	public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		await writer.WriteStartConstructorAsync(_name ?? string.Empty, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		for (int i = 0; i < _values.Count; i++)
		{
			await _values[i].WriteToAsync(writer, cancellationToken, converters).ConfigureAwait(continueOnCapturedContext: false);
		}
		await writer.WriteEndConstructorAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public new static Task<JConstructor> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return LoadAsync(reader, null, cancellationToken);
	}

	public new static async Task<JConstructor> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (reader.TokenType == JsonToken.None && !(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
		}
		await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.TokenType != JsonToken.StartConstructor)
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JConstructor c = new JConstructor((string)reader.Value);
		c.SetLineInfo(reader as IJsonLineInfo, settings);
		await c.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return c;
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
		if (content is JConstructor jConstructor)
		{
			if (jConstructor.Name != null)
			{
				Name = jConstructor.Name;
			}
			JContainer.MergeEnumerableContent(this, jConstructor, settings);
		}
	}

	public JConstructor()
	{
	}

	public JConstructor(JConstructor other)
		: base(other)
	{
		_name = other.Name;
	}

	public JConstructor(string name, params object[] content)
		: this(name, (object)content)
	{
	}

	public JConstructor(string name, object content)
		: this(name)
	{
		Add(content);
	}

	public JConstructor(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Constructor name cannot be empty.", "name");
		}
		_name = name;
	}

	internal override bool DeepEquals(JToken node)
	{
		if (node is JConstructor jConstructor && _name == jConstructor.Name)
		{
			return ContentsEqual(jConstructor);
		}
		return false;
	}

	internal override JToken CloneToken()
	{
		return new JConstructor(this);
	}

	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		writer.WriteStartConstructor(_name);
		int count = _values.Count;
		for (int i = 0; i < count; i++)
		{
			_values[i].WriteTo(writer, converters);
		}
		writer.WriteEndConstructor();
	}

	internal override int GetDeepHashCode()
	{
		return (_name?.GetHashCode() ?? 0) ^ ContentsHashCode();
	}

	public new static JConstructor Load(JsonReader reader)
	{
		return Load(reader, null);
	}

	public new static JConstructor Load(JsonReader reader, JsonLoadSettings? settings)
	{
		if (reader.TokenType == JsonToken.None && !reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
		}
		reader.MoveToContent();
		if (reader.TokenType != JsonToken.StartConstructor)
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JConstructor jConstructor = new JConstructor((string)reader.Value);
		jConstructor.SetLineInfo(reader as IJsonLineInfo, settings);
		jConstructor.ReadTokenFrom(reader, settings);
		return jConstructor;
	}
}
