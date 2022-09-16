using System;
using System.Collections.Concurrent;

namespace Newtonsoft.Json.Utilities;

internal class ThreadSafeStore<TKey, TValue>
{
	private readonly ConcurrentDictionary<TKey, TValue> _concurrentStore;

	private readonly Func<TKey, TValue> _creator;

	public ThreadSafeStore(Func<TKey, TValue> creator)
	{
		ValidationUtils.ArgumentNotNull(creator, "creator");
		_creator = creator;
		_concurrentStore = new ConcurrentDictionary<TKey, TValue>();
	}

	public TValue Get(TKey key)
	{
		return _concurrentStore.GetOrAdd(key, _creator);
	}
}
