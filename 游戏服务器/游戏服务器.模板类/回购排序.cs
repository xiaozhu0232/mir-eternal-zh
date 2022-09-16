using System.Collections.Generic;
using 游戏服务器.数据类;

namespace 游戏服务器.模板类;

public sealed class 回购排序 : IComparer<物品数据>
{
	public int Compare(物品数据 a, 物品数据 b)
	{
		return b.回购编号.CompareTo(a.回购编号);
	}

	static 回购排序()
	{
	}
}
