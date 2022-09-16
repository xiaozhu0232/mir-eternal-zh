using System;
using System.Collections.Generic;

namespace 游戏服务器.地图类;

public sealed class 对象仇恨
{
	public sealed class 仇恨详情
	{
		public int 仇恨数值;

		public DateTime 仇恨时间;

		public 仇恨详情(DateTime 仇恨时间, int 仇恨数值)
		{
			this.仇恨数值 = 仇恨数值;
			this.仇恨时间 = 仇恨时间;
		}

		static 仇恨详情()
		{
		}
	}

	public 地图对象 当前目标;

	public DateTime 切换时间;

	public readonly Dictionary<地图对象, 仇恨详情> 仇恨列表;

	public 对象仇恨()
	{
		仇恨列表 = new Dictionary<地图对象, 仇恨详情>();
	}

	public bool 移除仇恨(地图对象 对象)
	{
		if (当前目标 == 对象)
		{
			当前目标 = null;
		}
		return 仇恨列表.Remove(对象);
	}

	public void 添加仇恨(地图对象 对象, DateTime 时间, int 仇恨数值)
	{
		if (!对象.对象死亡)
		{
			if (仇恨列表.TryGetValue(对象, out var value))
			{
				value.仇恨时间 = ((!(value.仇恨时间 < 时间)) ? value.仇恨时间 : 时间);
				value.仇恨数值 += 仇恨数值;
			}
			else
			{
				仇恨列表[对象] = new 仇恨详情(时间, 仇恨数值);
			}
		}
	}

	public bool 切换仇恨(地图对象 主人)
	{
		int num = int.MinValue;
		List<地图对象> list = new List<地图对象>();
		foreach (KeyValuePair<地图对象, 仇恨详情> item in 仇恨列表)
		{
			if (item.Value.仇恨数值 > num)
			{
				num = item.Value.仇恨数值;
				list = new List<地图对象> { item.Key };
			}
			else if (item.Value.仇恨数值 == num)
			{
				list.Add(item.Key);
			}
		}
		if (num == 0 && 当前目标 != null)
		{
			return true;
		}
		int num2 = int.MaxValue;
		地图对象 地图对象2 = null;
		foreach (地图对象 item2 in list)
		{
			int num3 = 主人.网格距离(item2);
			if (num3 < num2)
			{
				num2 = num3;
				地图对象2 = item2;
			}
		}
		if (地图对象2 is 玩家实例 玩家实例2)
		{
			玩家实例2.玩家获得仇恨(主人);
		}
		return (当前目标 = 地图对象2) != null;
	}

	public bool 最近仇恨(地图对象 主人)
	{
		int num = int.MaxValue;
		List<KeyValuePair<地图对象, 仇恨详情>> list = new List<KeyValuePair<地图对象, 仇恨详情>>();
		foreach (KeyValuePair<地图对象, 仇恨详情> item in 仇恨列表)
		{
			int num2 = 主人.网格距离(item.Key);
			if (num2 >= num)
			{
				if (num2 == num)
				{
					list.Add(item);
				}
			}
			else
			{
				num = num2;
				list = new List<KeyValuePair<地图对象, 仇恨详情>> { item };
			}
		}
		int num3 = int.MinValue;
		地图对象 地图对象2 = null;
		foreach (KeyValuePair<地图对象, 仇恨详情> item2 in list)
		{
			if (item2.Value.仇恨数值 > num3)
			{
				num3 = item2.Value.仇恨数值;
				地图对象2 = item2.Key;
			}
		}
		if (地图对象2 is 玩家实例 玩家实例2)
		{
			玩家实例2.玩家获得仇恨(主人);
		}
		return (当前目标 = 地图对象2) != null;
	}

	static 对象仇恨()
	{
	}
}
