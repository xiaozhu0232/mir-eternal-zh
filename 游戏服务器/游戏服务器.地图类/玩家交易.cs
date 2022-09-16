using System;
using System.Collections.Generic;
using System.Linq;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public sealed class 玩家交易
{
	public 玩家实例 交易申请方;

	public 玩家实例 交易接收方;

	public byte 申请方状态;

	public byte 接收方状态;

	public int 申请方金币;

	public int 接收方金币;

	public Dictionary<byte, 物品数据> 申请方物品;

	public Dictionary<byte, 物品数据> 接收方物品;

	public 玩家交易(玩家实例 申请方, 玩家实例 接收方)
	{
		申请方物品 = new Dictionary<byte, 物品数据>();
		接收方物品 = new Dictionary<byte, 物品数据>();
		交易申请方 = 申请方;
		交易接收方 = 接收方;
		申请方状态 = 1;
		接收方状态 = 2;
		发送封包(new 交易状态改变
		{
			对象编号 = 交易申请方.地图编号,
			交易状态 = 申请方状态,
			对象等级 = 交易申请方.当前等级
		});
		发送封包(new 交易状态改变
		{
			对象编号 = 交易接收方.地图编号,
			交易状态 = 接收方状态,
			对象等级 = 交易接收方.当前等级
		});
	}

	public void 结束交易()
	{
		交易申请方.网络连接?.发送封包(new 交易状态改变
		{
			对象编号 = 交易申请方.地图编号,
			交易状态 = 0,
			对象等级 = 交易申请方.当前等级
		});
		交易接收方.网络连接?.发送封包(new 交易状态改变
		{
			对象编号 = 交易接收方.地图编号,
			交易状态 = 0,
			对象等级 = 交易接收方.当前等级
		});
		交易申请方.当前交易 = (交易接收方.当前交易 = null);
	}

	public void 交换物品()
	{
		if (接收方金币 > 0)
		{
			交易接收方.金币数量 -= (int)Math.Ceiling((float)接收方金币 * 1.04f);
			交易接收方.角色数据.转出金币.V += 接收方金币;
		}
		if (申请方金币 > 0)
		{
			交易申请方.金币数量 -= (int)Math.Ceiling((float)申请方金币 * 1.04f);
			交易申请方.角色数据.转出金币.V += 申请方金币;
		}
		foreach (物品数据 value in 接收方物品.Values)
		{
			if (value.物品编号 != 80207)
			{
				if (value.物品编号 == 80209)
				{
					交易接收方.角色数据.转出金币.V += 5000000L;
				}
			}
			else
			{
				交易接收方.角色数据.转出金币.V += 1000000L;
			}
			交易接收方.角色背包.Remove(value.物品位置.V);
			交易接收方.网络连接?.发送封包(new 删除玩家物品
			{
				背包类型 = 1,
				物品位置 = value.物品位置.V
			});
		}
		foreach (物品数据 value2 in 申请方物品.Values)
		{
			if (value2.物品编号 != 80207)
			{
				if (value2.物品编号 == 80209)
				{
					交易申请方.角色数据.转出金币.V += 5000000L;
				}
			}
			else
			{
				交易申请方.角色数据.转出金币.V += 1000000L;
			}
			交易申请方.角色背包.Remove(value2.物品位置.V);
			交易申请方.网络连接?.发送封包(new 删除玩家物品
			{
				背包类型 = 1,
				物品位置 = value2.物品位置.V
			});
		}
		foreach (物品数据 value3 in 申请方物品.Values)
		{
			byte b = 0;
			while (b < 交易接收方.背包大小)
			{
				if (交易接收方.角色背包.ContainsKey(b))
				{
					b = (byte)(b + 1);
					continue;
				}
				交易接收方.角色背包.Add(b, value3);
				value3.物品容器.V = 1;
				value3.物品位置.V = b;
				交易接收方.网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = value3.字节描述()
				});
				break;
			}
		}
		foreach (物品数据 value4 in 接收方物品.Values)
		{
			byte b2 = 0;
			while (b2 < 交易申请方.背包大小)
			{
				if (交易申请方.角色背包.ContainsKey(b2))
				{
					b2 = (byte)(b2 + 1);
					continue;
				}
				交易申请方.角色背包.Add(b2, value4);
				value4.物品容器.V = 1;
				value4.物品位置.V = b2;
				交易申请方.网络连接?.发送封包(new 玩家物品变动
				{
					物品描述 = value4.字节描述()
				});
				break;
			}
		}
		if (申请方金币 > 0)
		{
			交易接收方.金币数量 += 申请方金币;
		}
		if (接收方金币 > 0)
		{
			交易申请方.金币数量 += 接收方金币;
		}
		更改状态(6);
		结束交易();
	}

	public void 更改状态(byte 状态, 玩家实例 玩家 = null)
	{
		if (玩家 != null)
		{
			if (玩家 == 交易申请方)
			{
				申请方状态 = 状态;
				发送封包(new 交易状态改变
				{
					对象编号 = 玩家.地图编号,
					交易状态 = 玩家.交易状态,
					对象等级 = 玩家.当前等级
				});
			}
			else if (玩家 != 交易接收方)
			{
				结束交易();
			}
			else
			{
				接收方状态 = 状态;
				发送封包(new 交易状态改变
				{
					对象编号 = 玩家.地图编号,
					交易状态 = 玩家.交易状态,
					对象等级 = 玩家.当前等级
				});
			}
		}
		else
		{
			申请方状态 = (接收方状态 = 状态);
			发送封包(new 交易状态改变
			{
				对象编号 = 交易申请方.地图编号,
				交易状态 = 申请方状态,
				对象等级 = 交易申请方.当前等级
			});
			发送封包(new 交易状态改变
			{
				对象编号 = 交易接收方.地图编号,
				交易状态 = 接收方状态,
				对象等级 = 交易接收方.当前等级
			});
		}
	}

	public void 放入金币(玩家实例 玩家, int 数量)
	{
		if (玩家 != 交易申请方)
		{
			if (玩家 == 交易接收方)
			{
				接收方金币 = 数量;
				发送封包(new 放入交易金币
				{
					对象编号 = 玩家.地图编号,
					金币数量 = 数量
				});
			}
			else
			{
				结束交易();
			}
		}
		else
		{
			申请方金币 = 数量;
			发送封包(new 放入交易金币
			{
				对象编号 = 玩家.地图编号,
				金币数量 = 数量
			});
		}
	}

	public void 放入物品(玩家实例 玩家, 物品数据 物品, byte 位置)
	{
		if (玩家 != 交易申请方)
		{
			if (玩家 != 交易接收方)
			{
				结束交易();
				return;
			}
			接收方物品.Add(位置, 物品);
			发送封包(new 放入交易物品
			{
				对象编号 = 玩家.地图编号,
				放入位置 = 位置,
				放入物品 = 1,
				物品描述 = 物品.字节描述()
			});
		}
		else
		{
			申请方物品.Add(位置, 物品);
			发送封包(new 放入交易物品
			{
				对象编号 = 玩家.地图编号,
				放入位置 = 位置,
				放入物品 = 1,
				物品描述 = 物品.字节描述()
			});
		}
	}

	public bool 背包已满(out 玩家实例 玩家)
	{
		玩家 = null;
		if (交易申请方.背包剩余 < 接收方物品.Count)
		{
			玩家 = 交易申请方;
			return true;
		}
		if (交易接收方.背包剩余 < 申请方物品.Count)
		{
			玩家 = 交易接收方;
			return true;
		}
		return false;
	}

	public bool 金币重复(玩家实例 玩家)
	{
		if (玩家 != 交易申请方)
		{
			if (玩家 != 交易接收方)
			{
				return true;
			}
			return 接收方金币 != 0;
		}
		return 申请方金币 != 0;
	}

	public bool 物品重复(玩家实例 玩家, 物品数据 物品)
	{
		if (玩家 == 交易申请方)
		{
			return 申请方物品.Values.FirstOrDefault((物品数据 O) => O == 物品) != null;
		}
		if (玩家 != 交易接收方)
		{
			return true;
		}
		return 接收方物品.Values.FirstOrDefault((物品数据 O) => O == 物品) != null;
	}

	public bool 物品重复(玩家实例 玩家, byte 位置)
	{
		if (玩家 == 交易申请方)
		{
			return 申请方物品.ContainsKey(位置);
		}
		if (玩家 == 交易接收方)
		{
			return 接收方物品.ContainsKey(位置);
		}
		return true;
	}

	public byte 对方状态(玩家实例 玩家)
	{
		if (玩家 != 交易接收方)
		{
			if (玩家 != 交易申请方)
			{
				return 0;
			}
			return 接收方状态;
		}
		return 申请方状态;
	}

	public void 发送封包(游戏封包 封包)
	{
		交易接收方.网络连接?.发送封包(封包);
		交易申请方.网络连接?.发送封包(封包);
	}

	public 玩家实例 对方玩家(玩家实例 玩家)
	{
		if (玩家 == 交易接收方)
		{
			return 交易申请方;
		}
		return 交易接收方;
	}

	public Dictionary<byte, 物品数据> 对方物品(玩家实例 玩家)
	{
		if (玩家 == 交易接收方)
		{
			return 申请方物品;
		}
		if (玩家 == 交易申请方)
		{
			return 接收方物品;
		}
		return null;
	}

	static 玩家交易()
	{
	}
}
