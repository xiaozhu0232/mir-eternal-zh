using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using 游戏服务器.模板类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public sealed class 地图实例
{
	public readonly int 路线编号;

	public readonly 游戏地图 地图模板;

	public uint 固定怪物总数;

	public uint 存活怪物总数;

	public uint 怪物复活次数;

	public long 怪物掉落次数;

	public long 金币掉落总数;

	public bool 副本关闭;

	public byte 副本节点;

	public 守卫实例 副本守卫;

	public DateTime 节点计时;

	public int 刷怪记录;

	public List<怪物刷新> 怪物波数;

	public HashSet<地图对象>[,] 地图对象;

	public 地形数据 地形数据;

	public 地图区域 复活区域;

	public 地图区域 红名区域;

	public 地图区域 传送区域;

	public HashSet<地图区域> 地图区域;

	public HashSet<怪物刷新> 怪物区域;

	public HashSet<守卫刷新> 守卫区域;

	public HashSet<玩家实例> 玩家列表;

	public HashSet<宠物实例> 宠物列表;

	public HashSet<物品实例> 物品列表;

	public HashSet<地图对象> 对象列表;

	public Dictionary<byte, 传送法阵> 法阵列表;

	public byte 地图状态
	{
		get
		{
			if (玩家列表.Count < 200)
			{
				return 1;
			}
			if (玩家列表.Count < 500)
			{
				return 2;
			}
			return 3;
		}
	}

	public int 地图编号 => 地图模板.地图编号;

	public byte 限制等级 => 地图模板.限制等级;

	public byte 分线数量 => 1;

	public bool 下线传送 => 地图模板.下线传送;

	public byte 传送地图 => 地图模板.传送地图;

	public bool 副本地图 => 地图模板.副本地图;

	public Point 地图起点 => 地形数据.地图起点;

	public Point 地图终点 => 地形数据.地图终点;

	public Point 地图大小 => 地形数据.地图大小;

	public HashSet<地图对象> this[Point 坐标]
	{
		get
		{
			if (坐标越界(坐标))
			{
				return new HashSet<地图对象>();
			}
			if (地图对象[坐标.X - 地图起点.X, 坐标.Y - 地图起点.Y] == null)
			{
				return 地图对象[坐标.X - 地图起点.X, 坐标.Y - 地图起点.Y] = new HashSet<地图对象>();
			}
			return 地图对象[坐标.X - 地图起点.X, 坐标.Y - 地图起点.Y];
		}
	}

	public 地图实例(游戏地图 地图模板, int 路线编号 = 1)
	{
		地图区域 = new HashSet<地图区域>();
		怪物区域 = new HashSet<怪物刷新>();
		守卫区域 = new HashSet<守卫刷新>();
		玩家列表 = new HashSet<玩家实例>();
		宠物列表 = new HashSet<宠物实例>();
		物品列表 = new HashSet<物品实例>();
		对象列表 = new HashSet<地图对象>();
		法阵列表 = new Dictionary<byte, 传送法阵>();
		this.地图模板 = 地图模板;
		this.路线编号 = 路线编号;
	}

	public void 处理数据()
	{
		if (地图编号 != 80)
		{
			return;
		}
		if (玩家列表.Count != 0)
		{
			if (副本节点 > 5)
			{
				if (副本节点 > 5 + 怪物波数.Count)
				{
					if (副本节点 == 6 + 怪物波数.Count)
					{
						if (副本守卫.对象死亡)
						{
							副本节点 = 100;
							节点计时 = 主程.当前时间;
						}
						else if (存活怪物总数 == 0)
						{
							地图公告("所有怪物都已被击退, 大厅将在30秒后关闭");
							副本节点 = 110;
							节点计时 = 主程.当前时间.AddSeconds(30.0);
						}
					}
					else if (副本节点 <= 109)
					{
						if (主程.当前时间 > 节点计时)
						{
							地图公告("守卫已经死亡, 大厅即将关闭");
							副本节点 += 2;
							节点计时 = 主程.当前时间.AddSeconds(2.0);
						}
					}
					else
					{
						if (副本节点 < 110 || !(主程.当前时间 > 节点计时))
						{
							return;
						}
						foreach (玩家实例 item in 玩家列表.ToList())
						{
							if (item.对象死亡)
							{
								item.玩家请求复活();
							}
							else
							{
								item.玩家切换地图(地图处理网关.分配地图(item.重生地图), 地图区域类型.复活区域);
							}
						}
						foreach (宠物实例 item2 in 宠物列表.ToList())
						{
							if (item2.对象死亡)
							{
								item2.删除对象();
							}
							else
							{
								item2.宠物召回处理();
							}
						}
						foreach (物品实例 item3 in 物品列表)
						{
							item3.物品消失处理();
						}
						foreach (地图对象 item4 in 对象列表)
						{
							item4.删除对象();
						}
						副本关闭 = true;
					}
				}
				else if (副本守卫.对象死亡)
				{
					副本节点 = 100;
					节点计时 = 主程.当前时间;
				}
				else if (主程.当前时间 > 节点计时)
				{
					int num = 副本节点 - 6;
					怪物刷新 怪物刷新 = 怪物波数[num];
					int num2 = 刷怪记录 >> 16;
					int num3 = 刷怪记录 & 0xFFFF;
					刷新信息 刷新信息 = 怪物刷新.刷新列表[num2];
					if (刷怪记录 == 0)
					{
						地图公告($"第{num + 1}波怪物已经出现, 请注意防守");
					}
					if (游戏怪物.数据表.TryGetValue(刷新信息.怪物名字, out var value))
					{
						new 怪物实例(value, this, int.MaxValue, new Point[1]
						{
							new Point(995, 283)
						}, 禁止复活: true, 立即刷新: true).存活时间 = 主程.当前时间.AddMinutes(30.0);
					}
					if (++num3 >= 刷新信息.刷新数量)
					{
						num2++;
						num3 = 0;
					}
					if (num2 >= 怪物刷新.刷新列表.Length)
					{
						副本节点++;
						刷怪记录 = 0;
						节点计时 = 主程.当前时间.AddSeconds(60.0);
					}
					else
					{
						刷怪记录 = (num2 << 16) + num3;
						节点计时 = 主程.当前时间.AddSeconds(2.0);
					}
				}
			}
			else if (主程.当前时间 > 节点计时)
			{
				地图公告($"怪物将在{30 - 副本节点 * 5}秒后刷新, 请做好准备");
				副本节点++;
				节点计时 = 主程.当前时间.AddSeconds(5.0);
			}
		}
		else
		{
			副本节点 = 110;
		}
	}

	public void 添加对象(地图对象 对象)
	{
		switch (对象.对象类型)
		{
		default:
			对象列表.Add(对象);
			break;
		case 游戏对象类型.物品:
			物品列表.Add(对象 as 物品实例);
			break;
		case 游戏对象类型.宠物:
			宠物列表.Add(对象 as 宠物实例);
			break;
		case 游戏对象类型.玩家:
			玩家列表.Add(对象 as 玩家实例);
			break;
		}
	}

	public void 移除对象(地图对象 对象)
	{
		switch (对象.对象类型)
		{
		default:
			对象列表.Remove(对象);
			break;
		case 游戏对象类型.物品:
			物品列表.Remove(对象 as 物品实例);
			break;
		case 游戏对象类型.宠物:
			宠物列表.Remove(对象 as 宠物实例);
			break;
		case 游戏对象类型.玩家:
			玩家列表.Remove(对象 as 玩家实例);
			break;
		}
	}

	public void 地图公告(string 内容)
	{
		if (玩家列表.Count == 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(0);
		binaryWriter.Write(2415919107u);
		binaryWriter.Write(3);
		binaryWriter.Write(0);
		binaryWriter.Write(Encoding.UTF8.GetBytes(内容 + "\0"));
		byte[] 字节描述 = memoryStream.ToArray();
		foreach (玩家实例 item in 玩家列表)
		{
			item.网络连接?.发送封包(new 接收聊天消息
			{
				字节描述 = 字节描述
			});
		}
	}

	public override string ToString()
	{
		return 地图模板.ToString();
	}

	public Point 随机坐标(地图区域类型 区域)
	{
		return 区域 switch
		{
			地图区域类型.复活区域 => 复活区域.随机坐标, 
			地图区域类型.红名区域 => 红名区域.随机坐标, 
			地图区域类型.传送区域 => 传送区域.随机坐标, 
			地图区域类型.随机区域 => 地图区域.FirstOrDefault((地图区域 O) => O.区域类型 == 地图区域类型.随机区域)?.随机坐标 ?? default(Point), 
			_ => default(Point), 
		};
	}

	public Point 随机传送(Point 坐标)
	{
		foreach (地图区域 item in 地图区域)
		{
			if (item.范围坐标.Contains(坐标) && item.区域类型 == 地图区域类型.随机区域)
			{
				return item.随机坐标;
			}
		}
		return default(Point);
	}

	public bool 坐标越界(Point 坐标)
	{
		if (坐标.X < 地图起点.X || 坐标.Y < 地图起点.Y || 坐标.X >= 地图终点.X)
		{
			return true;
		}
		return 坐标.Y >= 地图终点.Y;
	}

	public bool 空间阻塞(Point 坐标)
	{
		if (安全区内(坐标))
		{
			return false;
		}
		foreach (地图对象 item in this[坐标])
		{
			if (item.阻塞网格)
			{
				return true;
			}
		}
		return false;
	}

	public int 阻塞数量(Point 坐标)
	{
		int num = 0;
		foreach (地图对象 item in this[坐标])
		{
			if (item.阻塞网格)
			{
				num++;
			}
		}
		return num;
	}

	public bool 地形阻塞(Point 坐标)
	{
		if (坐标越界(坐标))
		{
			return true;
		}
		return (地形数据[坐标] & 0x10000000) != 268435456;
	}

	public bool 能否通行(Point 坐标)
	{
		if (地形阻塞(坐标))
		{
			return false;
		}
		return !空间阻塞(坐标);
	}

	public ushort 地形高度(Point 坐标)
	{
		if (坐标越界(坐标))
		{
			return 0;
		}
		return (ushort)((地形数据[坐标] & 0xFFFF) - 30);
	}

	public bool 地形遮挡(Point 起点, Point 终点)
	{
		int num = 计算类.网格距离(起点, 终点);
		int num2 = 1;
		while (true)
		{
			if (num2 < num)
			{
				if (地形阻塞(计算类.前方坐标(起点, 终点, num2)))
				{
					break;
				}
				num2++;
				continue;
			}
			return false;
		}
		return true;
	}

	public bool 自由区内(Point 坐标)
	{
		if (!坐标越界(坐标))
		{
			return (地形数据[坐标] & 0x20000) == 131072;
		}
		return false;
	}

	public bool 安全区内(Point 坐标)
	{
		if (!坐标越界(坐标))
		{
			if ((地形数据[坐标] & 0x40000) == 262144)
			{
				return true;
			}
			return (地形数据[坐标] & 0x100000) == 1048576;
		}
		return false;
	}

	public bool 摆摊区内(Point 坐标)
	{
		if (坐标越界(坐标))
		{
			return false;
		}
		return (地形数据[坐标] & 0x100000) == 1048576;
	}

	public bool 掉落装备(Point 坐标, bool 红名)
	{
		if (地图处理网关.沙城节点 >= 2 && (地图编号 == 152 || 地图编号 == 178))
		{
			return false;
		}
		if (!坐标越界(坐标))
		{
			if ((地形数据[坐标] & 0x400000) != 4194304)
			{
				if (!((地形数据[坐标] & 0x800000) == 8388608 && 红名))
				{
					return false;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	static 地图实例()
	{
	}
}
