using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using 游戏服务器.模板类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public static class 地图处理网关
{
	public static int 对象表计数;

	public static List<地图对象> 次要对象表;

	public static List<地图对象> 对象备份表;

	public static Dictionary<int, 地图对象> 激活对象表;

	public static Dictionary<int, 地图对象> 地图对象表;

	public static Dictionary<int, 玩家实例> 玩家对象表;

	public static Dictionary<int, 宠物实例> 宠物对象表;

	public static Dictionary<int, 怪物实例> 怪物对象表;

	public static Dictionary<int, 守卫实例> 守卫对象表;

	public static Dictionary<int, 物品实例> 物品对象表;

	public static Dictionary<int, 陷阱实例> 陷阱对象表;

	public static Dictionary<int, 地图实例> 地图实例表;

	public static HashSet<地图实例> 副本实例表;

	private static ConcurrentQueue<地图实例> 副本移除表;

	private static ConcurrentQueue<地图对象> 添加激活表;

	private static ConcurrentQueue<地图对象> 移除激活表;

	public static int 对象编号;

	public static int 陷阱编号;

	public static int 物品编号;
	
	private static DateTime 沙城处理计时;

	public static Point 沙城城门坐标;

	public static Point 皇宫下门坐标;

	public static Point 皇宫下门出口;

	public static Point 皇宫下门入口;

	public static Point 皇宫左门坐标;

	public static Point 皇宫左门出口;

	public static Point 皇宫左门入口;

	public static Point 皇宫上门坐标;

	public static Point 皇宫上门出口;

	public static Point 皇宫上门入口;

	public static Point 皇宫出口点一;

	public static Point 皇宫出口点二;

	public static Point 皇宫正门入口;

	public static Point 皇宫正门出口;

	public static Point 皇宫入口点左;

	public static Point 皇宫入口点中;

	public static Point 皇宫入口点右;

	public static Point 八卦坛坐标上;

	public static Point 八卦坛坐标下;

	public static Point 八卦坛坐标左;

	public static Point 八卦坛坐标右;

	public static Point 八卦坛坐标中;

	public static 地图实例 沙城地图;

	public static 怪物实例 沙城城门;

	public static 怪物实例 下方宫门;

	public static 怪物实例 上方宫门;

	public static 怪物实例 左方宫门;

	public static 守卫实例 上方法阵;

	public static 守卫实例 下方法阵;

	public static 守卫实例 左方法阵;

	public static 守卫实例 右方法阵;

	public static 守卫实例 八卦坛激活法阵;

	public static 行会数据 八卦坛激活行会;

	public static DateTime 八卦坛激活计时;

	public static 地图区域 皇宫随机区域;

	public static 地图区域 外城复活区域;

	public static 地图区域 内城复活区域;

	public static 地图区域 守方传送区域;

	public static byte 沙城节点;

	public static DateTime 通知时间;

	public static HashSet<行会数据> 攻城行会;

	private static void 沙城处理()
	{
		if (主程.当前时间 < 沙城处理计时)
		{
			return;
		}
		沙城处理计时 = 主程.当前时间.AddMilliseconds(50.0);
		if (沙城地图 == null)
		{
			if (!地图实例表.TryGetValue(2433, out 沙城地图) || !游戏Buff.数据表.TryGetValue(22300, out var value) || !游戏怪物.数据表.TryGetValue("沙巴克城门", out var value2) || !游戏怪物.数据表.TryGetValue("沙巴克宫门", out var value3) || (皇宫随机区域 = 沙城地图.地图区域.FirstOrDefault((地图区域 O) => O.区域名字 == "沙巴克-皇宫随机区域")) == null || (外城复活区域 = 沙城地图.地图区域.FirstOrDefault((地图区域 O) => O.区域名字 == "沙巴克-外城复活区域")) == null || (内城复活区域 = 沙城地图.地图区域.FirstOrDefault((地图区域 O) => O.区域名字 == "沙巴克-内城复活区域")) == null || (守方传送区域 = 沙城地图.地图区域.FirstOrDefault((地图区域 O) => O.区域名字 == "沙巴克-守方传送区域")) == null)
			{
				沙城地图 = null;
				return;
			}
			沙城城门 = new 怪物实例(value2, 沙城地图, int.MaxValue, new Point[1] { 沙城城门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.右上,
				存活时间 = DateTime.MaxValue
			};
			上方宫门 = new 怪物实例(value3, 沙城地图, int.MaxValue, new Point[1] { 皇宫上门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.右下,
				存活时间 = DateTime.MaxValue
			};
			下方宫门 = new 怪物实例(value3, 沙城地图, int.MaxValue, new Point[1] { 皇宫下门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.右下,
				存活时间 = DateTime.MaxValue
			};
			左方宫门 = new 怪物实例(value3, 沙城地图, int.MaxValue, new Point[1] { 皇宫左门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.左下,
				存活时间 = DateTime.MaxValue
			};
			沙城城门.添加Buff时处理(value.Buff编号, 沙城城门);
			上方宫门.添加Buff时处理(value.Buff编号, 上方宫门);
			下方宫门.添加Buff时处理(value.Buff编号, 下方宫门);
			左方宫门.添加Buff时处理(value.Buff编号, 左方宫门);
		}
		foreach (地图对象 item in 沙城地图[皇宫下门坐标].ToList())
		{
			if (!item.对象死亡 && item is 玩家实例 玩家实例2 && 主程.当前时间 > 玩家实例2.忙碌时间)
			{
				玩家实例2.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫下门入口);
			}
		}
		foreach (地图对象 item2 in 沙城地图[皇宫上门坐标].ToList())
		{
			if (!item2.对象死亡 && item2 is 玩家实例 玩家实例3 && 主程.当前时间 > 玩家实例3.忙碌时间)
			{
				玩家实例3.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫上门入口);
			}
		}
		foreach (地图对象 item3 in 沙城地图[皇宫左门坐标].ToList())
		{
			if (!item3.对象死亡 && item3 is 玩家实例 玩家实例4 && 主程.当前时间 > 玩家实例4.忙碌时间)
			{
				玩家实例4.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫左门入口);
			}
		}
		foreach (地图对象 item4 in 沙城地图[皇宫出口点一].ToList())
		{
			if (!item4.对象死亡 && item4 is 玩家实例 玩家实例5 && 主程.当前时间 > 玩家实例5.忙碌时间)
			{
				玩家实例5.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫正门出口);
			}
		}
		foreach (地图对象 item5 in 沙城地图[皇宫出口点二].ToList())
		{
			if (!item5.对象死亡 && item5 is 玩家实例 玩家实例6 && 主程.当前时间 > 玩家实例6.忙碌时间)
			{
				玩家实例6.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫正门出口);
			}
		}
		foreach (地图对象 item6 in 沙城地图[皇宫入口点左].ToList())
		{
			if (!item6.对象死亡 && item6 is 玩家实例 玩家实例7 && 主程.当前时间 > 玩家实例7.忙碌时间 && 玩家实例7.所属行会 != null && 玩家实例7.所属行会 == 系统数据.数据.占领行会.V)
			{
				玩家实例7.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫正门入口);
			}
		}
		foreach (地图对象 item7 in 沙城地图[皇宫入口点中].ToList())
		{
			if (!item7.对象死亡 && item7 is 玩家实例 玩家实例8 && 主程.当前时间 > 玩家实例8.忙碌时间 && 玩家实例8.所属行会 != null && 玩家实例8.所属行会 == 系统数据.数据.占领行会.V)
			{
				玩家实例8.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫正门入口);
			}
		}
		foreach (地图对象 item8 in 沙城地图[皇宫入口点右].ToList())
		{
			if (!item8.对象死亡 && item8 is 玩家实例 玩家实例9 && 主程.当前时间 > 玩家实例9.忙碌时间 && 玩家实例9.所属行会 != null && 玩家实例9.所属行会 == 系统数据.数据.占领行会.V)
			{
				玩家实例9.玩家切换地图(沙城地图, 地图区域类型.未知区域, 皇宫正门入口);
			}
		}
		if (沙城节点 == 0)
		{
			foreach (KeyValuePair<DateTime, 行会数据> item9 in 系统数据.数据.申请行会.ToList())
			{
				if (item9.Key.Date < 主程.当前时间.Date)
				{
					系统数据.数据.申请行会.Remove(item9.Key);
				}
			}
			if (系统数据.数据.申请行会.Count == 0 || 主程.当前时间.Hour + 1 != 自定义类.攻沙开始时间 || 主程.当前时间.Minute != 50 )
			{
				return;
			}
			{
				foreach (KeyValuePair<DateTime, 行会数据> item10 in 系统数据.数据.申请行会)
				{
					if (item10.Key.Date == 主程.当前时间.Date)
					{
						网络服务网关.发送公告("沙巴克攻城战将在十分钟后开始, 请做好准备", 滚动播报: true);
						沙城节点++;
						break;
					}
				}
				return;
			}
		}
		if (沙城节点 == 1)
		{
			if (主程.当前时间.Hour != 自定义类.攻沙开始时间)
			{
				return;
			}
			foreach (KeyValuePair<DateTime, 行会数据> item11 in 系统数据.数据.申请行会.ToList())
			{
				if (item11.Key.Date == 主程.当前时间.Date)
				{
					攻城行会.Add(item11.Value);
					系统数据.数据.申请行会.Remove(item11.Key);
				}
			}
			if (攻城行会.Count == 0)
			{
				沙城节点 = 0;
				return;
			}
			沙城城门.移除Buff时处理(22300);
			下方宫门.移除Buff时处理(22300);
			上方宫门.移除Buff时处理(22300);
			左方宫门.移除Buff时处理(22300);
			foreach (玩家实例 item12 in 沙城地图.玩家列表)
			{
				if (item12.所属行会 == null || item12.所属行会 != 系统数据.数据.占领行会.V)
				{
					item12.玩家切换地图(沙城地图, 地图区域类型.未知区域, 外城复活区域.随机坐标);
				}
			}
			if (地图实例表.TryGetValue(2849, out var value4))
			{
				foreach (玩家实例 item13 in value4.玩家列表.ToList())
				{
					if (item13.所属行会 == null || item13.所属行会 != 系统数据.数据.占领行会.V)
					{
						item13.玩家切换地图(item13.复活地图, 地图区域类型.复活区域);
					}
				}
			}
			if (系统数据.数据.占领行会.V != null)
			{
				行会数据 v = 系统数据.数据.占领行会.V;
				foreach (行会数据 item14 in 攻城行会)
				{
					if (item14.结盟行会.Remove(v))
					{
						v.结盟行会.Remove(item14);
						item14.发送封包(new 删除外交公告
						{
							外交类型 = 1,
							行会编号 = v.行会编号
						});
						v.发送封包(new 删除外交公告
						{
							外交类型 = 1,
							行会编号 = item14.行会编号
						});
					}
					if (!item14.敌对行会.ContainsKey(v))
					{
						item14.敌对行会.Add(v, 主程.当前时间.AddHours(1.0));
						v.敌对行会.Add(item14, 主程.当前时间.AddHours(1.0));
						item14.发送封包(new 添加外交公告
						{
							外交类型 = 2,
							行会编号 = v.行会编号,
							行会名字 = v.行会名字.V,
							行会等级 = v.行会等级.V,
							行会人数 = (byte)v.行会成员.Count,
							外交时间 = (int)(item14.敌对行会[v] - 主程.当前时间).TotalSeconds
						});
						v.发送封包(new 添加外交公告
						{
							外交类型 = 2,
							行会编号 = item14.行会编号,
							行会名字 = item14.行会名字.V,
							行会等级 = item14.行会等级.V,
							行会人数 = (byte)item14.行会成员.Count,
							外交时间 = (int)(v.敌对行会[item14] - 主程.当前时间).TotalSeconds
						});
					}
					if (item14.敌对行会[v] < 主程.当前时间.AddHours(1.0))
					{
						item14.敌对行会[v] = 主程.当前时间.AddHours(1.0);
						v.敌对行会[item14] = 主程.当前时间.AddHours(1.0);
						item14.发送封包(new 添加外交公告
						{
							外交类型 = 2,
							行会编号 = v.行会编号,
							行会名字 = v.行会名字.V,
							行会等级 = v.行会等级.V,
							行会人数 = (byte)v.行会成员.Count,
							外交时间 = (int)(item14.敌对行会[v] - 主程.当前时间).TotalSeconds
						});
						v.发送封包(new 添加外交公告
						{
							外交类型 = 2,
							行会编号 = item14.行会编号,
							行会名字 = item14.行会名字.V,
							行会等级 = item14.行会等级.V,
							行会人数 = (byte)item14.行会成员.Count,
							外交时间 = (int)(v.敌对行会[item14] - 主程.当前时间).TotalSeconds
						});
					}
				}
			}
			网络服务网关.发送公告("沙巴克攻城战开始", 滚动播报: true);
			沙城节点++;
		}
		else if (沙城节点 == 2)
		{
			if (沙城城门.对象死亡 && 沙城城门.出生地图 != null)
			{
				网络服务网关.发送公告("沙巴克城门已经被攻破", 滚动播报: true);
				沙城城门.出生地图 = null;
			}
			if (八卦坛激活行会 == null)
			{
				行会数据 行会数据 = null;
				bool flag = true;
				if (沙城地图[八卦坛坐标上].FirstOrDefault((地图对象 O) => !O.对象死亡 && O is 玩家实例) == null)
				{
					flag = false;
				}
				if (flag && 沙城地图[八卦坛坐标下].FirstOrDefault((地图对象 O) => !O.对象死亡 && O is 玩家实例) == null)
				{
					flag = false;
				}
				if (flag && 沙城地图[八卦坛坐标左].FirstOrDefault((地图对象 O) => !O.对象死亡 && O is 玩家实例) == null)
				{
					flag = false;
				}
				if (flag && 沙城地图[八卦坛坐标右].FirstOrDefault((地图对象 O) => !O.对象死亡 && O is 玩家实例) == null)
				{
					flag = false;
				}
				if (行会数据 == null && flag)
				{
					foreach (地图对象 item15 in 沙城地图[八卦坛坐标上])
					{
						if (!item15.对象死亡 && item15 is 玩家实例 玩家实例10)
						{
							if (玩家实例10.所属行会 == null)
							{
								flag = false;
								break;
							}
							if (!攻城行会.Contains(玩家实例10.所属行会))
							{
								flag = false;
								break;
							}
							if (行会数据 == null)
							{
								行会数据 = 玩家实例10.所属行会;
							}
							else if (行会数据 != 玩家实例10.所属行会)
							{
								flag = false;
								break;
							}
						}
					}
				}
				if (行会数据 != null && flag)
				{
					foreach (地图对象 item16 in 沙城地图[八卦坛坐标下])
					{
						if (!item16.对象死亡 && item16 is 玩家实例 玩家实例11 && (玩家实例11.所属行会 == null || 行会数据 != 玩家实例11.所属行会))
						{
							flag = false;
							break;
						}
					}
				}
				if (行会数据 != null && flag)
				{
					foreach (地图对象 item17 in 沙城地图[八卦坛坐标左])
					{
						if (!item17.对象死亡 && item17 is 玩家实例 玩家实例12 && (玩家实例12.所属行会 == null || 行会数据 != 玩家实例12.所属行会))
						{
							flag = false;
							break;
						}
					}
				}
				if (行会数据 != null && flag)
				{
					foreach (地图对象 item18 in 沙城地图[八卦坛坐标右])
					{
						if (!item18.对象死亡 && item18 is 玩家实例 玩家实例13 && (玩家实例13.所属行会 == null || 行会数据 != 玩家实例13.所属行会))
						{
							flag = false;
							break;
						}
					}
				}
				if (行会数据 != null && flag && 攻城行会.Contains(行会数据))
				{
					if (八卦坛激活计时 == DateTime.MaxValue)
					{
						八卦坛激活计时 = 主程.当前时间.AddSeconds(10.0);
					}
					else if (主程.当前时间 > 八卦坛激活计时)
					{
						八卦坛激活行会 = 行会数据;
						八卦坛激活法阵 = new 守卫实例(地图守卫.数据表[6123], 沙城地图, 游戏方向.左方, 八卦坛坐标中);
						网络服务网关.发送公告($"沙巴克八卦坛传送点已经被行会[{行会数据}]成功激活", 滚动播报: true);
					}
				}
				else
				{
					八卦坛激活计时 = DateTime.MaxValue;
				}
			}
			bool flag2 = true;
			行会数据 行会数据2 = null;
			foreach (Point item19 in 皇宫随机区域.范围坐标)
			{
				foreach (地图对象 item20 in 沙城地图[item19])
				{
					if (!item20.对象死亡 && item20 is 玩家实例 玩家实例14)
					{
						if (玩家实例14.所属行会 == null || !攻城行会.Contains(玩家实例14.所属行会))
						{
							flag2 = false;
							break;
						}
						if (行会数据2 == null)
						{
							行会数据2 = 玩家实例14.所属行会;
						}
						else if (行会数据2 != 玩家实例14.所属行会)
						{
							flag2 = false;
							break;
						}
					}
				}
				if (!flag2)
				{
					break;
				}
			}
			if (flag2 && 行会数据2 != null)
			{
				网络服务网关.发送封包(new 同步占领行会
				{
					行会编号 = 行会数据2.行会编号
				});
				系统数据.数据.占领行会.V = 行会数据2;
				系统数据.数据.占领时间.V = 主程.当前时间;
				foreach (KeyValuePair<角色数据, 行会职位> item21 in 行会数据2.行会成员)
				{
					item21.Key.攻沙日期.V = 主程.当前时间;
				}
				网络服务网关.发送公告($"沙巴克攻城战已经结束, [{行会数据2}]成为新的沙巴克行会", 滚动播报: true);
				八卦坛激活计时 = 主程.当前时间.AddMinutes(5.0);
				沙城节点++;
			}
			else
			{
				if (主程.当前时间.Hour < 自定义类.攻沙结束时间)
				{
					return;
				}
				if (系统数据.数据.占领行会.V == null)
				{
					网络服务网关.发送公告("沙巴克攻城战已经结束, 沙巴克仍然为无主之地", 滚动播报: true);
				}
				else
				{
					网络服务网关.发送公告($"沙巴克攻城战已经结束, 沙巴克仍然被[{系统数据.数据.占领行会.V.行会名字}]行会占领", 滚动播报: true);
				}
				if (系统数据.数据.占领行会.V == null)
				{
					foreach (KeyValuePair<角色数据, 行会职位> item22 in 系统数据.数据.占领行会.V.行会成员)
					{
						item22.Key.攻沙日期.V = 主程.当前时间;
					}
				}
				八卦坛激活计时 = 主程.当前时间.AddMinutes(5.0);
				沙城节点++;
			}
		}
		else if (沙城节点 == 3 && 主程.当前时间 > 八卦坛激活计时)
		{
			沙城城门?.删除对象();
			上方宫门?.删除对象();
			下方宫门?.删除对象();
			左方宫门?.删除对象();
			沙城城门 = new 怪物实例(沙城城门.对象模板, 沙城地图, int.MaxValue, new Point[1] { 沙城城门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.右上,
				存活时间 = DateTime.MaxValue
			};
			上方宫门 = new 怪物实例(上方宫门.对象模板, 沙城地图, int.MaxValue, new Point[1] { 皇宫上门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.右下,
				存活时间 = DateTime.MaxValue
			};
			下方宫门 = new 怪物实例(下方宫门.对象模板, 沙城地图, int.MaxValue, new Point[1] { 皇宫下门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.右下,
				存活时间 = DateTime.MaxValue
			};
			左方宫门 = new 怪物实例(左方宫门.对象模板, 沙城地图, int.MaxValue, new Point[1] { 皇宫左门坐标 }, 禁止复活: true, 立即刷新: true)
			{
				当前方向 = 游戏方向.左下,
				存活时间 = DateTime.MaxValue
			};
			沙城城门.添加Buff时处理(22300, 沙城城门);
			上方宫门.添加Buff时处理(22300, 上方宫门);
			下方宫门.添加Buff时处理(22300, 下方宫门);
			左方宫门.添加Buff时处理(22300, 左方宫门);
			八卦坛激活行会 = null;
			八卦坛激活计时 = DateTime.MaxValue;
			八卦坛激活法阵?.删除对象();
			八卦坛激活法阵 = null;
			攻城行会.Clear();
			沙城节点 = 0;
		}
	}

	public static void 处理数据()
	{
		foreach (KeyValuePair<int, 地图对象> item in 激活对象表)
		{
			item.Value?.处理对象数据();
		}
		if (对象表计数 >= 次要对象表.Count)
		{
			对象表计数 = 0;
			次要对象表 = 对象备份表;
			对象备份表 = new List<地图对象>();
		}
		for (int i = 0; i < 100; i++)
		{
			if (对象表计数 >= 次要对象表.Count)
			{
				break;
			}
			if (次要对象表[对象表计数].次要对象)
			{
				次要对象表[对象表计数].处理对象数据();
				对象备份表.Add(次要对象表[对象表计数]);
			}
			对象表计数++;
		}
		while (!移除激活表.IsEmpty)
		{
			if (移除激活表.TryDequeue(out var result) && !result.激活对象)
			{
				激活对象表.Remove(result.地图编号);
			}
		}
		while (!添加激活表.IsEmpty)
		{
			if (添加激活表.TryDequeue(out var result2) && result2.激活对象 && !激活对象表.ContainsKey(result2.地图编号))
			{
				激活对象表.Add(result2.地图编号, result2);
			}
		}
		if (主程.当前时间.Minute == 55 && 主程.当前时间.Hour != 通知时间.Hour)
		{
			if (主程.当前时间.Hour + 1 == 自定义类.武斗场时间一 || 主程.当前时间.Hour + 1 == 自定义类.武斗场时间二)
			{
				网络服务网关.发送公告("经验武斗场将在五分钟后开启, 想要参加的勇士请做好准备", 滚动播报: true);
			}
			通知时间 = 主程.当前时间;
		}
		foreach (地图实例 item2 in 副本实例表)
		{
			if (item2.副本关闭)
			{
				副本移除表.Enqueue(item2);
			}
			else
			{
				item2.处理数据();
			}
		}
		while (!副本移除表.IsEmpty)
		{
			if (副本移除表.TryDequeue(out var result3))
			{
				副本实例表.Remove(result3);
			}
		}
		沙城处理();
	}

	public static void 开启地图()
	{
		次要对象表 = new List<地图对象>();
		对象备份表 = new List<地图对象>();
		副本实例表 = new HashSet<地图实例>();
		副本移除表 = new ConcurrentQueue<地图实例>();
		添加激活表 = new ConcurrentQueue<地图对象>();
		移除激活表 = new ConcurrentQueue<地图对象>();
		激活对象表 = new Dictionary<int, 地图对象>();
		地图对象表 = new Dictionary<int, 地图对象>();
		地图实例表 = new Dictionary<int, 地图实例>();
		玩家对象表 = new Dictionary<int, 玩家实例>();
		怪物对象表 = new Dictionary<int, 怪物实例>();
		宠物对象表 = new Dictionary<int, 宠物实例>();
		守卫对象表 = new Dictionary<int, 守卫实例>();
		物品对象表 = new Dictionary<int, 物品实例>();
		陷阱对象表 = new Dictionary<int, 陷阱实例>();
		foreach (游戏地图 value3 in 游戏地图.数据表.Values)
		{
			地图实例表.Add(value3.地图编号 * 16 + 1, new 地图实例(value3, 16777217));
		}
		foreach (地形数据 value4 in 地形数据.数据表.Values)
		{
			foreach (地图实例 value5 in 地图实例表.Values)
			{
				if (value5.地图编号 != value4.地图编号)
				{
					continue;
				}
				value5.地形数据 = value4;
				value5.地图对象 = new HashSet<地图对象>[value5.地图大小.X, value5.地图大小.Y];
				for (int i = 0; i < value5.地图大小.X; i++)
				{
					for (int j = 0; j < value5.地图大小.Y; j++)
					{
						value5.地图对象[i, j] = new HashSet<地图对象>();
					}
				}
			}
		}
		foreach (地图区域 item in 地图区域.数据表)
		{
			foreach (地图实例 value6 in 地图实例表.Values)
			{
				if (value6.地图编号 == item.所处地图)
				{
					if (item.区域类型 == 地图区域类型.复活区域)
					{
						value6.复活区域 = item;
					}
					if (item.区域类型 == 地图区域类型.红名区域)
					{
						value6.红名区域 = item;
					}
					if (item.区域类型 == 地图区域类型.传送区域)
					{
						value6.传送区域 = item;
					}
					value6.地图区域.Add(item);
					break;
				}
			}
		}
		foreach (传送法阵 item2 in 传送法阵.数据表)
		{
			foreach (地图实例 value7 in 地图实例表.Values)
			{
				if (value7.地图编号 == item2.所处地图)
				{
					value7.法阵列表.Add(item2.法阵编号, item2);
				}
			}
		}
		foreach (守卫刷新 item3 in 守卫刷新.数据表)
		{
			foreach (地图实例 value8 in 地图实例表.Values)
			{
				if (value8.地图编号 == item3.所处地图)
				{
					value8.守卫区域.Add(item3);
				}
			}
		}
		foreach (怪物刷新 item4 in 怪物刷新.数据表)
		{
			foreach (地图实例 value9 in 地图实例表.Values)
			{
				if (value9.地图编号 == item4.所处地图)
				{
					value9.怪物区域.Add(item4);
				}
			}
		}
		foreach (地图实例 value10 in 地图实例表.Values)
		{
			if (!value10.副本地图)
			{
				foreach (怪物刷新 item5 in value10.怪物区域)
				{
					if (item5.刷新列表 == null)
					{
						continue;
					}
					Point[] 出生范围 = item5.范围坐标.ToArray();
					刷新信息[] 刷新列表 = item5.刷新列表;
					foreach (刷新信息 刷新信息 in 刷新列表)
					{
						if (游戏怪物.数据表.TryGetValue(刷新信息.怪物名字, out var value))
						{
							主窗口.添加怪物数据(value);
							int 复活间隔 = 刷新信息.复活间隔 * 60 * 1000;
							for (int l = 0; l < 刷新信息.刷新数量; l++)
							{
								new 怪物实例(value, value10, 复活间隔, 出生范围, 禁止复活: false, 立即刷新: true);
							}
						}
					}
				}
				foreach (守卫刷新 item6 in value10.守卫区域)
				{
					if (地图守卫.数据表.TryGetValue(item6.守卫编号, out var value2))
					{
						new 守卫实例(value2, value10, item6.所处方向, item6.所处坐标);
					}
				}
			}
			else
			{
				value10.固定怪物总数 = (uint)value10.怪物区域.Sum((怪物刷新 O) => O.刷新列表.Sum((刷新信息 X) => X.刷新数量));
			}
			主窗口.添加地图数据(value10);
		}
	}

	public static void 清理物品()
	{
		foreach (物品实例 value in 物品对象表.Values)
		{
			value.物品数据?.删除数据();
		}
		foreach (KeyValuePair<int, 游戏商店> item in 游戏商店.数据表)
		{
			foreach (物品数据 item2 in item.Value.回购列表)
			{
				item2.删除数据();
			}
		}
	}

	public static void 添加地图对象(地图对象 当前对象)
	{
		地图对象表.Add(当前对象.地图编号, 当前对象);
		游戏对象类型 对象类型 = 当前对象.对象类型;
		if (对象类型 <= 游戏对象类型.Npcc)
		{
			switch (对象类型)
			{
			case 游戏对象类型.玩家:
				玩家对象表.Add(当前对象.地图编号, (玩家实例)当前对象);
				return;
			case 游戏对象类型.宠物:
				宠物对象表.Add(当前对象.地图编号, (宠物实例)当前对象);
				return;
			case 游戏对象类型.怪物:
				怪物对象表.Add(当前对象.地图编号, (怪物实例)当前对象);
				return;
			case (游戏对象类型)3:
				return;
			}
			if (对象类型 == 游戏对象类型.Npcc)
			{
				守卫对象表.Add(当前对象.地图编号, (守卫实例)当前对象);
			}
		}
		else
		{
			switch (对象类型)
			{
			case 游戏对象类型.物品:
				物品对象表.Add(当前对象.地图编号, (物品实例)当前对象);
				break;
			case 游戏对象类型.陷阱:
				陷阱对象表.Add(当前对象.地图编号, (陷阱实例)当前对象);
				break;
			}
		}
	}

	public static void 移除地图对象(地图对象 当前对象)
	{
		地图对象表.Remove(当前对象.地图编号);
		游戏对象类型 对象类型 = 当前对象.对象类型;
		if (对象类型 <= 游戏对象类型.Npcc)
		{
			switch (对象类型)
			{
			case 游戏对象类型.玩家:
				玩家对象表.Remove(当前对象.地图编号);
				return;
			case 游戏对象类型.宠物:
				宠物对象表.Remove(当前对象.地图编号);
				return;
			case 游戏对象类型.怪物:
				怪物对象表.Remove(当前对象.地图编号);
				return;
			case (游戏对象类型)3:
				return;
			}
			if (对象类型 == 游戏对象类型.Npcc)
			{
				守卫对象表.Remove(当前对象.地图编号);
			}
		}
		else
		{
			switch (对象类型)
			{
			case 游戏对象类型.物品:
				物品对象表.Remove(当前对象.地图编号);
				break;
			case 游戏对象类型.陷阱:
				陷阱对象表.Remove(当前对象.地图编号);
				break;
			}
		}
	}

	public static void 添加激活对象(地图对象 当前对象)
	{
		添加激活表.Enqueue(当前对象);
	}

	public static void 移除激活对象(地图对象 当前对象)
	{
		移除激活表.Enqueue(当前对象);
	}

	public static void 添加次要对象(地图对象 当前对象)
	{
		对象备份表.Add(当前对象);
	}

	public static 地图实例 分配地图(int 地图编号)
	{
		if (地图实例表.TryGetValue(地图编号 * 16 + 1, out var value))
		{
			return value;
		}
		return null;
	}

	static 地图处理网关()
	{
		对象编号 = 268435456;
		陷阱编号 = 1073741824;
		物品编号 = 1342177280;
		沙城城门坐标 = new Point(1020, 506);
		皇宫下门坐标 = new Point(1079, 557);
		皇宫下门出口 = new Point(1078, 556);
		皇宫下门入口 = new Point(1265, 773);
		皇宫左门坐标 = new Point(1082, 557);
		皇宫左门出口 = new Point(1083, 556);
		皇宫左门入口 = new Point(1266, 773);
		皇宫上门坐标 = new Point(1071, 565);
		皇宫上门出口 = new Point(1070, 564);
		皇宫上门入口 = new Point(1254, 784);
		皇宫出口点一 = new Point(1257, 777);
		皇宫出口点二 = new Point(1258, 776);
		皇宫正门入口 = new Point(1258, 777);
		皇宫正门出口 = new Point(1074, 560);
		皇宫入口点左 = new Point(1076, 560);
		皇宫入口点中 = new Point(1075, 561);
		皇宫入口点右 = new Point(1074, 562);
		八卦坛坐标上 = new Point(1059, 591);
		八卦坛坐标下 = new Point(1054, 586);
		八卦坛坐标左 = new Point(1059, 586);
		八卦坛坐标右 = new Point(1054, 591);
		八卦坛坐标中 = new Point(1056, 588);
		八卦坛激活计时 = DateTime.MaxValue;
		攻城行会 = new HashSet<行会数据>();
	}
}
