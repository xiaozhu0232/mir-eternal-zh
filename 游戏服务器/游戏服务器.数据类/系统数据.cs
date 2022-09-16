using System;
using System.Collections.Generic;
using System.IO;

namespace 游戏服务器.数据类;

public class 系统数据 : 游戏数据
{
	private sealed class 行会比较器 : IComparer<行会数据>
	{
		public int Compare(行会数据 x, 行会数据 y)
		{
			return x.行会成员.Count - y.行会成员.Count;
		}

		static 行会比较器()
		{
		}
	}

	private sealed class 等级比较器 : IComparer<角色数据>
	{
		public int Compare(角色数据 x, 角色数据 y)
		{
			if (x.角色等级 == y.角色等级)
			{
				return x.角色经验 - y.角色经验;
			}
			return x.角色等级 - y.角色等级;
		}

		static 等级比较器()
		{
		}
	}

	private sealed class 战力比较器 : IComparer<角色数据>
	{
		public int Compare(角色数据 x, 角色数据 y)
		{
			return x.角色战力 - y.角色战力;
		}

		static 战力比较器()
		{
		}
	}

	private sealed class 声望比较器 : IComparer<角色数据>
	{
		public int Compare(角色数据 x, 角色数据 y)
		{
			return x.师门声望 - y.师门声望;
		}

		static 声望比较器()
		{
		}
	}

	private sealed class PK值比较器 : IComparer<角色数据>
	{
		public int Compare(角色数据 x, 角色数据 y)
		{
			return x.角色PK值 - y.角色PK值;
		}

		static PK值比较器()
		{
		}
	}

	public readonly 字典监视器<string, DateTime> 网络封禁;

	public readonly 字典监视器<string, DateTime> 网卡封禁;

	public readonly 数据监视器<DateTime> 占领时间;

	public readonly 数据监视器<行会数据> 占领行会;

	public readonly 字典监视器<DateTime, 行会数据> 申请行会;

	public readonly 列表监视器<角色数据> 个人战力排名;

	public readonly 列表监视器<角色数据> 个人等级排名;

	public readonly 列表监视器<角色数据> 个人声望排名;

	public readonly 列表监视器<角色数据> 个人PK值排名;

	public readonly 列表监视器<角色数据> 战士战力排名;

	public readonly 列表监视器<角色数据> 法师战力排名;

	public readonly 列表监视器<角色数据> 道士战力排名;

	public readonly 列表监视器<角色数据> 刺客战力排名;

	public readonly 列表监视器<角色数据> 弓手战力排名;

	public readonly 列表监视器<角色数据> 龙枪战力排名;

	public readonly 列表监视器<角色数据> 战士等级排名;

	public readonly 列表监视器<角色数据> 法师等级排名;

	public readonly 列表监视器<角色数据> 道士等级排名;

	public readonly 列表监视器<角色数据> 刺客等级排名;

	public readonly 列表监视器<角色数据> 弓手等级排名;

	public readonly 列表监视器<角色数据> 龙枪等级排名;

	public readonly 列表监视器<行会数据> 行会人数排名;

	private static readonly 战力比较器 战力计算器;

	private static readonly 等级比较器 等级计算器;

	private static readonly 声望比较器 声望计算器;

	private static readonly PK值比较器 PK值计算器;

	private static readonly 行会比较器 行会计算器;

	public static 系统数据 数据 => 游戏数据网关.数据类型表[typeof(系统数据)].数据表[1] as 系统数据;

	public 系统数据()
	{
	}

	public 系统数据(int 索引)
	{
		数据索引.V = 索引;
		游戏数据网关.数据类型表[typeof(系统数据)].添加数据(this);
	}

	public void 更新战力(角色数据 角色)
	{
		更新榜单(个人战力排名, 6, 角色, 战力计算器);
		switch (角色.角色职业.V)
		{
		case 游戏对象职业.战士:
			更新榜单(战士战力排名, 7, 角色, 战力计算器);
			break;
		case 游戏对象职业.法师:
			更新榜单(法师战力排名, 8, 角色, 战力计算器);
			break;
		case 游戏对象职业.刺客:
			更新榜单(刺客战力排名, 10, 角色, 战力计算器);
			break;
		case 游戏对象职业.弓手:
			更新榜单(弓手战力排名, 11, 角色, 战力计算器);
			break;
		case 游戏对象职业.道士:
			更新榜单(道士战力排名, 9, 角色, 战力计算器);
			break;
		case 游戏对象职业.龙枪:
			更新榜单(龙枪战力排名, 37, 角色, 战力计算器);
			break;
		}
	}

	public void 更新等级(角色数据 角色)
	{
		更新榜单(个人等级排名, 0, 角色, 等级计算器);
		switch (角色.角色职业.V)
		{
		case 游戏对象职业.战士:
			更新榜单(战士等级排名, 1, 角色, 等级计算器);
			break;
		case 游戏对象职业.法师:
			更新榜单(法师等级排名, 2, 角色, 等级计算器);
			break;
		case 游戏对象职业.刺客:
			更新榜单(刺客等级排名, 4, 角色, 等级计算器);
			break;
		case 游戏对象职业.弓手:
			更新榜单(弓手等级排名, 5, 角色, 等级计算器);
			break;
		case 游戏对象职业.道士:
			更新榜单(道士等级排名, 3, 角色, 等级计算器);
			break;
		case 游戏对象职业.龙枪:
			更新榜单(龙枪等级排名, 36, 角色, 等级计算器);
			break;
		}
	}

	public void 更新声望(角色数据 角色)
	{
		更新榜单(个人声望排名, 14, 角色, 声望计算器);
	}

	public void 更新PK值(角色数据 角色)
	{
		更新榜单(个人PK值排名, 15, 角色, PK值计算器);
	}

	public void 更新行会(行会数据 行会)
	{
		int num = 行会.行会排名.V - 1;
		if (行会人数排名.Count >= 100)
		{
			if (num >= 0)
			{
				行会人数排名.RemoveAt(num);
				int num2 = 二分查找(行会人数排名, 行会, 行会计算器, 0, 行会人数排名.Count);
				行会人数排名.Insert(num2, 行会);
				for (int i = Math.Min(num, num2); i <= Math.Max(num, num2); i++)
				{
					行会人数排名[i].行会排名.V = i + 1;
				}
			}
			else if (行会计算器.Compare(行会, 行会人数排名.Last) > 0)
			{
				int num3 = 二分查找(行会人数排名, 行会, 行会计算器, 0, 行会人数排名.Count);
				行会人数排名.Insert(num3, 行会);
				for (int j = num3; j < 行会人数排名.Count; j++)
				{
					行会人数排名[j].行会排名.V = j + 1;
				}
				行会人数排名[100].行会排名.V = 0;
				行会人数排名.RemoveAt(100);
			}
		}
		else if (num >= 0)
		{
			行会人数排名.RemoveAt(num);
			int num4 = 二分查找(行会人数排名, 行会, 行会计算器, 0, 行会人数排名.Count);
			行会人数排名.Insert(num4, 行会);
			for (int k = Math.Min(num, num4); k <= Math.Max(num, num4); k++)
			{
				行会人数排名[k].行会排名.V = k + 1;
			}
		}
		else
		{
			int num5 = 二分查找(行会人数排名, 行会, 行会计算器, 0, 行会人数排名.Count);
			行会人数排名.Insert(num5, 行会);
			for (int l = num5; l < 行会人数排名.Count; l++)
			{
				行会人数排名[l].行会排名.V = l + 1;
			}
		}
	}

	public void 封禁网络(string 地址, DateTime 时间)
	{
		if (网络封禁.ContainsKey(地址))
		{
			网络封禁[地址] = 时间;
			主窗口.更新封禁数据(地址, 时间);
		}
		else
		{
			网络封禁[地址] = 时间;
			主窗口.添加封禁数据(地址, 时间);
		}
	}

	public void 封禁网卡(string 地址, DateTime 时间)
	{
		if (网卡封禁.ContainsKey(地址))
		{
			网卡封禁[地址] = 时间;
			主窗口.更新封禁数据(地址, 时间, 网络地址: false);
		}
		else
		{
			网卡封禁[地址] = 时间;
			主窗口.添加封禁数据(地址, 时间, 网络地址: false);
		}
	}

	public void 解封网络(string 地址)
	{
		if (网络封禁.Remove(地址))
		{
			主窗口.移除封禁数据(地址);
		}
	}

	public void 解封网卡(string 地址)
	{
		if (网卡封禁.Remove(地址))
		{
			主窗口.移除封禁数据(地址);
		}
	}

	public byte[] 沙城申请描述()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (KeyValuePair<DateTime, 行会数据> item in 申请行会)
		{
			binaryWriter.Write(item.Value.行会编号);
			binaryWriter.Write(计算类.时间转换(item.Key.AddDays(-1.0)));
		}
		return memoryStream.ToArray();
	}

	public override void 加载完成()
	{
		foreach (KeyValuePair<string, DateTime> item in 网络封禁)
		{
			主窗口.添加封禁数据(item.Key, item.Value);
		}
		foreach (KeyValuePair<string, DateTime> item2 in 网卡封禁)
		{
			主窗口.添加封禁数据(item2.Key, item2.Value, 网络地址: false);
		}
	}

	private static void 更新榜单(列表监视器<角色数据> 当前榜单, byte 当前类型, object 角色, IComparer<角色数据> 比较方法)
	{
		int num = ((角色数据)角色).当前排名[当前类型] - 1;
		if (当前榜单.Count < 300)
		{
			if (num >= 0)
			{
				当前榜单.RemoveAt(num);
				int num2 = 二分查找(当前榜单, 角色, 比较方法, 0, 当前榜单.Count);
				当前榜单.Insert(num2, (角色数据)角色);
				for (int i = Math.Min(num, num2); i <= Math.Max(num, num2); i++)
				{
					当前榜单[i].历史排名[当前类型] = 当前榜单[i].当前排名[当前类型];
					当前榜单[i].当前排名[当前类型] = i + 1;
				}
			}
			else
			{
				int num3 = 二分查找(当前榜单, 角色, 战力计算器, 0, 当前榜单.Count);
				当前榜单.Insert(num3, (角色数据)角色);
				for (int j = num3; j < 当前榜单.Count; j++)
				{
					当前榜单[j].历史排名[当前类型] = 当前榜单[j].当前排名[当前类型];
					当前榜单[j].当前排名[当前类型] = j + 1;
				}
			}
		}
		else if (num >= 0)
		{
			当前榜单.RemoveAt(num);
			int num4 = 二分查找(当前榜单, 角色, 比较方法, 0, 当前榜单.Count);
			当前榜单.Insert(num4, (角色数据)角色);
			for (int k = Math.Min(num, num4); k <= Math.Max(num, num4); k++)
			{
				当前榜单[k].历史排名[当前类型] = 当前榜单[k].当前排名[当前类型];
				当前榜单[k].当前排名[当前类型] = k + 1;
			}
		}
		else if (比较方法.Compare((角色数据)角色, 当前榜单.Last) > 0)
		{
			int num5 = 二分查找(当前榜单, 角色, 战力计算器, 0, 当前榜单.Count);
			当前榜单.Insert(num5, (角色数据)角色);
			for (int l = num5; l < 当前榜单.Count; l++)
			{
				当前榜单[l].历史排名[当前类型] = 当前榜单[l].当前排名[当前类型];
				当前榜单[l].当前排名[当前类型] = l + 1;
			}
			当前榜单[300].当前排名.Remove(当前类型);
			当前榜单.RemoveAt(300);
		}
	}

	private static int 二分查找(列表监视器<角色数据> 列表, object 元素, IComparer<角色数据> 比较器, int 起始位置, int 结束位置)
	{
		if (结束位置 >= 0 && 列表.Count != 0)
		{
			if (起始位置 >= 列表.Count)
			{
				return 列表.Count;
			}
			int num = (起始位置 + 结束位置) / 2;
			int num2 = 比较器.Compare(列表[num], (角色数据)元素);
			if (num2 == 0)
			{
				return num;
			}
			if (num2 > 0)
			{
				if (num + 1 >= 列表.Count)
				{
					return 列表.Count;
				}
				if (比较器.Compare(列表[num + 1], (角色数据)元素) <= 0)
				{
					return num + 1;
				}
				return 二分查找(列表, 元素, 比较器, num + 1, 结束位置);
			}
			if (num - 1 < 0)
			{
				return 0;
			}
			if (比较器.Compare(列表[num - 1], (角色数据)元素) >= 0)
			{
				return num;
			}
			return 二分查找(列表, 元素, 比较器, 起始位置, num - 1);
		}
		return 0;
	}

	private static int 二分查找(列表监视器<行会数据> 列表, object 元素, IComparer<行会数据> 比较器, int 起始位置, int 结束位置)
	{
		if (结束位置 < 0)
		{
			return 0;
		}
		if (起始位置 >= 列表.Count)
		{
			return 列表.Count;
		}
		int num = (起始位置 + 结束位置) / 2;
		int num2 = 比较器.Compare(列表[num], (行会数据)元素);
		if (num2 == 0)
		{
			return num;
		}
		if (num2 > 0)
		{
			if (num + 1 >= 列表.Count)
			{
				return 列表.Count;
			}
			if (比较器.Compare(列表[num + 1], (行会数据)元素) <= 0)
			{
				return num + 1;
			}
			return 二分查找(列表, 元素, 比较器, num + 1, 结束位置);
		}
		if (num - 1 < 0)
		{
			return 0;
		}
		if (比较器.Compare(列表[num - 1], (行会数据)元素) >= 0)
		{
			return num;
		}
		return 二分查找(列表, 元素, 比较器, 起始位置, num - 1);
	}

	static 系统数据()
	{
		战力计算器 = new 战力比较器();
		等级计算器 = new 等级比较器();
		声望计算器 = new 声望比较器();
		PK值计算器 = new PK值比较器();
		行会计算器 = new 行会比较器();
	}
}
