using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities;

internal static class AsyncUtils
{
	public static readonly Task<bool> False = Task.FromResult(result: false);

	public static readonly Task<bool> True = Task.FromResult(result: true);

	internal static readonly Task CompletedTask = Task.Delay(0);

	internal static Task<bool> ToAsync(this bool value)
	{
		if (!value)
		{
			return False;
		}
		return True;
	}

	public static Task? CancelIfRequestedAsync(this CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return null;
		}
		return cancellationToken.FromCanceled();
	}

	public static Task<T>? CancelIfRequestedAsync<T>(this CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return null;
		}
		return cancellationToken.FromCanceled<T>();
	}

	public static Task FromCanceled(this CancellationToken cancellationToken)
	{
		return new Task(delegate
		{
		}, cancellationToken);
	}

	public static Task<T> FromCanceled<T>(this CancellationToken cancellationToken)
	{
		return new Task<T>(() => default(T), cancellationToken);
	}

	public static Task WriteAsync(this TextWriter writer, char value, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return writer.WriteAsync(value);
		}
		return cancellationToken.FromCanceled();
	}

	public static Task WriteAsync(this TextWriter writer, string? value, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return writer.WriteAsync(value);
		}
		return cancellationToken.FromCanceled();
	}

	public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return writer.WriteAsync(value, start, count);
		}
		return cancellationToken.FromCanceled();
	}

	public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			return reader.ReadAsync(buffer, index, count);
		}
		return cancellationToken.FromCanceled<int>();
	}

	public static bool IsCompletedSucessfully(this Task task)
	{
		return task.Status == TaskStatus.RanToCompletion;
	}
}
