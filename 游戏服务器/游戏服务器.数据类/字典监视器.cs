using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace 游戏服务器.数据类;

public sealed class 字典监视器<TK, TV> : IEnumerable<KeyValuePair<TK, TV>>, IEnumerable
{
	public delegate void 更改委托(List<KeyValuePair<TK, TV>> 更改字典);

	private readonly Dictionary<TK, TV> v;

	private readonly 游戏数据 对应数据;

	public TV this[TK key]
	{
		get
		{
			if (!v.TryGetValue(key, out var value))
			{
				return default(TV);
			}
			return value;
		}
		set
		{
			v[key] = value;
			this.更改事件?.Invoke(v.ToList());
			设置状态();
		}
	}

	public ICollection<TK> Keys => v.Keys;

	public ICollection<TV> Values => v.Values;

	public IDictionary IDictionary_0 => v;

	public int Count => v.Count;

	public event 更改委托 更改事件;

	public 字典监视器(游戏数据 数据)
	{
		v = new Dictionary<TK, TV>();
		对应数据 = 数据;
	}

	public bool ContainsKey(TK k)
	{
		return v.ContainsKey(k);
	}

	public bool TryGetValue(TK k, out TV v)
	{
		return this.v.TryGetValue(k, out v);
	}

	public void Add(TK key, TV value)
	{
		v.Add(key, value);
		this.更改事件?.Invoke(v.ToList());
		设置状态();
	}

	public bool Remove(TK key)
	{
		if (v.Remove(key))
		{
			this.更改事件?.Invoke(v.ToList());
			设置状态();
			return true;
		}
		return false;
	}

	public void Clear()
	{
		if (v.Count > 0)
		{
			v.Clear();
			this.更改事件?.Invoke(v.ToList());
			设置状态();
		}
	}

	public void QuietlyAdd(TK key, TV value)
	{
		v.Add(key, value);
	}

	public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
	{
		return v.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)v).GetEnumerator();
	}

	IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator()
	{
		return v.GetEnumerator();
	}

	public override string ToString()
	{
		return v?.Count.ToString();
	}

	private void 设置状态()
	{
		if (对应数据 != null)
		{
			对应数据.已经修改 = true;
			游戏数据网关.已经修改 = true;
		}
	}
}
