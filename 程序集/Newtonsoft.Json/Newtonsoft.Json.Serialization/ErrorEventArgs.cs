using System;

namespace Newtonsoft.Json.Serialization;

public class ErrorEventArgs : EventArgs
{
	public object? CurrentObject { get; }

	public ErrorContext ErrorContext { get; }

	public ErrorEventArgs(object? currentObject, ErrorContext errorContext)
	{
		CurrentObject = currentObject;
		ErrorContext = errorContext;
	}
}
