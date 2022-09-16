using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json;

public class JsonSerializerSettings
{
	internal const ReferenceLoopHandling DefaultReferenceLoopHandling = ReferenceLoopHandling.Error;

	internal const MissingMemberHandling DefaultMissingMemberHandling = MissingMemberHandling.Ignore;

	internal const NullValueHandling DefaultNullValueHandling = NullValueHandling.Include;

	internal const DefaultValueHandling DefaultDefaultValueHandling = DefaultValueHandling.Include;

	internal const ObjectCreationHandling DefaultObjectCreationHandling = ObjectCreationHandling.Auto;

	internal const PreserveReferencesHandling DefaultPreserveReferencesHandling = PreserveReferencesHandling.None;

	internal const ConstructorHandling DefaultConstructorHandling = ConstructorHandling.Default;

	internal const TypeNameHandling DefaultTypeNameHandling = TypeNameHandling.None;

	internal const MetadataPropertyHandling DefaultMetadataPropertyHandling = MetadataPropertyHandling.Default;

	internal static readonly StreamingContext DefaultContext;

	internal const Formatting DefaultFormatting = Formatting.None;

	internal const DateFormatHandling DefaultDateFormatHandling = DateFormatHandling.IsoDateFormat;

	internal const DateTimeZoneHandling DefaultDateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;

	internal const DateParseHandling DefaultDateParseHandling = DateParseHandling.DateTime;

	internal const FloatParseHandling DefaultFloatParseHandling = FloatParseHandling.Double;

	internal const FloatFormatHandling DefaultFloatFormatHandling = FloatFormatHandling.String;

	internal const StringEscapeHandling DefaultStringEscapeHandling = StringEscapeHandling.Default;

	internal const TypeNameAssemblyFormatHandling DefaultTypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;

	internal static readonly CultureInfo DefaultCulture;

	internal const bool DefaultCheckAdditionalContent = false;

	internal const string DefaultDateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

	internal const int DefaultMaxDepth = 64;

	internal Formatting? _formatting;

	internal DateFormatHandling? _dateFormatHandling;

	internal DateTimeZoneHandling? _dateTimeZoneHandling;

	internal DateParseHandling? _dateParseHandling;

	internal FloatFormatHandling? _floatFormatHandling;

	internal FloatParseHandling? _floatParseHandling;

	internal StringEscapeHandling? _stringEscapeHandling;

	internal CultureInfo? _culture;

	internal bool? _checkAdditionalContent;

	internal int? _maxDepth;

	internal bool _maxDepthSet;

	internal string? _dateFormatString;

	internal bool _dateFormatStringSet;

	internal TypeNameAssemblyFormatHandling? _typeNameAssemblyFormatHandling;

	internal DefaultValueHandling? _defaultValueHandling;

	internal PreserveReferencesHandling? _preserveReferencesHandling;

	internal NullValueHandling? _nullValueHandling;

	internal ObjectCreationHandling? _objectCreationHandling;

	internal MissingMemberHandling? _missingMemberHandling;

	internal ReferenceLoopHandling? _referenceLoopHandling;

	internal StreamingContext? _context;

	internal ConstructorHandling? _constructorHandling;

	internal TypeNameHandling? _typeNameHandling;

	internal MetadataPropertyHandling? _metadataPropertyHandling;

	public ReferenceLoopHandling ReferenceLoopHandling
	{
		get
		{
			return _referenceLoopHandling.GetValueOrDefault();
		}
		set
		{
			_referenceLoopHandling = value;
		}
	}

	public MissingMemberHandling MissingMemberHandling
	{
		get
		{
			return _missingMemberHandling.GetValueOrDefault();
		}
		set
		{
			_missingMemberHandling = value;
		}
	}

	public ObjectCreationHandling ObjectCreationHandling
	{
		get
		{
			return _objectCreationHandling.GetValueOrDefault();
		}
		set
		{
			_objectCreationHandling = value;
		}
	}

	public NullValueHandling NullValueHandling
	{
		get
		{
			return _nullValueHandling.GetValueOrDefault();
		}
		set
		{
			_nullValueHandling = value;
		}
	}

	public DefaultValueHandling DefaultValueHandling
	{
		get
		{
			return _defaultValueHandling.GetValueOrDefault();
		}
		set
		{
			_defaultValueHandling = value;
		}
	}

	public IList<JsonConverter> Converters { get; set; }

	public PreserveReferencesHandling PreserveReferencesHandling
	{
		get
		{
			return _preserveReferencesHandling.GetValueOrDefault();
		}
		set
		{
			_preserveReferencesHandling = value;
		}
	}

	public TypeNameHandling TypeNameHandling
	{
		get
		{
			return _typeNameHandling.GetValueOrDefault();
		}
		set
		{
			_typeNameHandling = value;
		}
	}

	public MetadataPropertyHandling MetadataPropertyHandling
	{
		get
		{
			return _metadataPropertyHandling.GetValueOrDefault();
		}
		set
		{
			_metadataPropertyHandling = value;
		}
	}

	[Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
	public FormatterAssemblyStyle TypeNameAssemblyFormat
	{
		get
		{
			return (FormatterAssemblyStyle)TypeNameAssemblyFormatHandling;
		}
		set
		{
			TypeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
		}
	}

	public TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
	{
		get
		{
			return _typeNameAssemblyFormatHandling.GetValueOrDefault();
		}
		set
		{
			_typeNameAssemblyFormatHandling = value;
		}
	}

	public ConstructorHandling ConstructorHandling
	{
		get
		{
			return _constructorHandling.GetValueOrDefault();
		}
		set
		{
			_constructorHandling = value;
		}
	}

	public IContractResolver? ContractResolver { get; set; }

	public IEqualityComparer? EqualityComparer { get; set; }

	[Obsolete("ReferenceResolver property is obsolete. Use the ReferenceResolverProvider property to set the IReferenceResolver: settings.ReferenceResolverProvider = () => resolver")]
	public IReferenceResolver? ReferenceResolver
	{
		get
		{
			return ReferenceResolverProvider?.Invoke();
		}
		set
		{
			IReferenceResolver value2 = value;
			ReferenceResolverProvider = ((value2 != null) ? ((Func<IReferenceResolver>)(() => value2)) : null);
		}
	}

	public Func<IReferenceResolver?>? ReferenceResolverProvider { get; set; }

	public ITraceWriter? TraceWriter { get; set; }

	[Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
	public SerializationBinder? Binder
	{
		get
		{
			if (SerializationBinder == null)
			{
				return null;
			}
			if (SerializationBinder is SerializationBinderAdapter serializationBinderAdapter)
			{
				return serializationBinderAdapter.SerializationBinder;
			}
			throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
		}
		set
		{
			SerializationBinder = ((value == null) ? null : new SerializationBinderAdapter(value));
		}
	}

	public ISerializationBinder? SerializationBinder { get; set; }

	public EventHandler<ErrorEventArgs>? Error { get; set; }

	public StreamingContext Context
	{
		get
		{
			return _context ?? DefaultContext;
		}
		set
		{
			_context = value;
		}
	}

	public string DateFormatString
	{
		get
		{
			return _dateFormatString ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
		}
		set
		{
			_dateFormatString = value;
			_dateFormatStringSet = true;
		}
	}

	public int? MaxDepth
	{
		get
		{
			if (!_maxDepthSet)
			{
				return 64;
			}
			return _maxDepth;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("Value must be positive.", "value");
			}
			_maxDepth = value;
			_maxDepthSet = true;
		}
	}

	public Formatting Formatting
	{
		get
		{
			return _formatting.GetValueOrDefault();
		}
		set
		{
			_formatting = value;
		}
	}

	public DateFormatHandling DateFormatHandling
	{
		get
		{
			return _dateFormatHandling.GetValueOrDefault();
		}
		set
		{
			_dateFormatHandling = value;
		}
	}

	public DateTimeZoneHandling DateTimeZoneHandling
	{
		get
		{
			return _dateTimeZoneHandling ?? DateTimeZoneHandling.RoundtripKind;
		}
		set
		{
			_dateTimeZoneHandling = value;
		}
	}

	public DateParseHandling DateParseHandling
	{
		get
		{
			return _dateParseHandling ?? DateParseHandling.DateTime;
		}
		set
		{
			_dateParseHandling = value;
		}
	}

	public FloatFormatHandling FloatFormatHandling
	{
		get
		{
			return _floatFormatHandling.GetValueOrDefault();
		}
		set
		{
			_floatFormatHandling = value;
		}
	}

	public FloatParseHandling FloatParseHandling
	{
		get
		{
			return _floatParseHandling.GetValueOrDefault();
		}
		set
		{
			_floatParseHandling = value;
		}
	}

	public StringEscapeHandling StringEscapeHandling
	{
		get
		{
			return _stringEscapeHandling.GetValueOrDefault();
		}
		set
		{
			_stringEscapeHandling = value;
		}
	}

	public CultureInfo Culture
	{
		get
		{
			return _culture ?? DefaultCulture;
		}
		set
		{
			_culture = value;
		}
	}

	public bool CheckAdditionalContent
	{
		get
		{
			return _checkAdditionalContent.GetValueOrDefault();
		}
		set
		{
			_checkAdditionalContent = value;
		}
	}

	static JsonSerializerSettings()
	{
		DefaultContext = default(StreamingContext);
		DefaultCulture = CultureInfo.InvariantCulture;
	}

	[DebuggerStepThrough]
	public JsonSerializerSettings()
	{
		Converters = new List<JsonConverter>();
	}
}
