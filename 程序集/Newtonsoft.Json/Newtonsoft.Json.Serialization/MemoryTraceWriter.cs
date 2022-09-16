using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Newtonsoft.Json.Serialization;

public class MemoryTraceWriter : ITraceWriter
{
	private readonly Queue<string> _traceMessages;

	private readonly object _lock;

	public TraceLevel LevelFilter { get; set; }

	public MemoryTraceWriter()
	{
		LevelFilter = TraceLevel.Verbose;
		_traceMessages = new Queue<string>();
		_lock = new object();
	}

	public void Trace(TraceLevel level, string message, Exception? ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
		stringBuilder.Append(" ");
		stringBuilder.Append(level.ToString("g"));
		stringBuilder.Append(" ");
		stringBuilder.Append(message);
		string item = stringBuilder.ToString();
		lock (_lock)
		{
			if (_traceMessages.Count >= 1000)
			{
				_traceMessages.Dequeue();
			}
			_traceMessages.Enqueue(item);
		}
	}

	public IEnumerable<string> GetTraceMessages()
	{
		return _traceMessages;
	}

	public override string ToString()
	{
		lock (_lock)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string traceMessage in _traceMessages)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(traceMessage);
			}
			return stringBuilder.ToString();
		}
	}
}
