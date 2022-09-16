using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace 游戏服务器.数据类;

public sealed class 列表监视器<T> : IEnumerable<T>, IEnumerable
{
	public delegate void 更改委托(List<T> 更改列表);

	private List<T> v;

	private readonly 游戏数据 对应数据;

	public T Last
	{
		get
		{
			if (v.Count != 0)
			{
				return v.Last();
			}
			return default(T);
		}
	}

	public T this[int 索引]
	{
		get
		{
			if (索引 >= v.Count)
			{
				return default(T);
			}
			return v[索引];
		}
		set
		{
			if (索引 < v.Count)
			{
				v[索引] = value;
				this.更改事件?.Invoke(v.ToList());
				设置状态();
			}
		}
	}

	public IList IList => v;

	public int Count => v.Count;

	public event 更改委托 更改事件;

	public 列表监视器(游戏数据 数据)
	{
		v = new List<T>();
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

	public List<T> GetRange(int index, int count)
	{
		return v.GetRange(index, count);
	}

	public void Add(T Tv)
	{
		v.Add(Tv);
		this.更改事件?.Invoke(v.ToList());
		设置状态();
	}

	public void Insert(int index, T Tv)
	{
		v.Insert(index, Tv);
		this.更改事件?.Invoke(v.ToList());
		设置状态();
	}

	public void Remove(T Tv)
	{
		if (v.Remove(Tv))
		{
			this.更改事件?.Invoke(v.ToList());
			设置状态();
		}
	}

	public void RemoveAt(int i)
	{
		if (v.Count > i)
		{
			v.RemoveAt(i);
			this.更改事件?.Invoke(v.ToList());
			设置状态();
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

	public void SetValue(List<T> Lv)
	{
		v = Lv;
		this.更改事件?.Invoke(v.ToList());
		设置状态();
	}

	public void QuietlyAdd(T Tv)
	{
		v.Add(Tv);
	}

	static 列表监视器()
	{
	}
}
