using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace 游戏服务器.数据类;

public sealed class 哈希监视器<T> : IEnumerable<T>, IEnumerable
{
	public delegate void 更改委托(List<T> 更改列表);

	private readonly HashSet<T> v;

	private readonly 游戏数据 对应数据;

	public int Count => v.Count;

	public ISet<T> ISet => v;

	public event 更改委托 更改事件;

	public 哈希监视器(游戏数据 数据)
	{
		v = new HashSet<T>();
		对应数据 = 数据;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return v.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)v).GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
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

	public void Clear()
	{
		if (v.Count > 0)
		{
			v.Clear();
			this.更改事件?.Invoke(v.ToList());
			设置状态();
		}
	}

	public bool Add(T Tv)
	{
		if (v.Add(Tv))
		{
			this.更改事件?.Invoke(v.ToList());
			设置状态();
			return true;
		}
		return false;
	}

	public bool Remove(T Tv)
	{
		if (v.Remove(Tv))
		{
			this.更改事件?.Invoke(v.ToList());
			设置状态();
			return true;
		}
		return false;
	}

	public void QuietlyAdd(T Tv)
	{
		v.Add(Tv);
	}

	public bool Contains(T Tv)
	{
		return v.Contains(Tv);
	}

	static 哈希监视器()
	{
	}
}
