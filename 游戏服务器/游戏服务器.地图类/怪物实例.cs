using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using 游戏服务器.模板类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public sealed class 怪物实例 : 地图对象
{
	public byte 宠物等级;

	public 游戏怪物 对象模板;

	public int 复活间隔;

	public 对象仇恨 对象仇恨;

	public Point[] 出生范围;

	public 地图实例 出生地图;

	public 游戏技能 普通攻击技能;

	public 游戏技能 概率触发技能;

	public 游戏技能 进入战斗技能;

	public 游戏技能 退出战斗技能;

	public 游戏技能 死亡释放技能;

	public 游戏技能 移动释放技能;

	public 游戏技能 出生释放技能;

	public bool 禁止复活 { get; set; }

	public bool 尸体消失 { get; set; }

	public DateTime 攻击时间 { get; set; }

	public DateTime 漫游时间 { get; set; }

	public DateTime 复活时间 { get; set; }

	public DateTime 消失时间 { get; set; }

	public DateTime 存活时间 { get; set; }

	public override int 处理间隔 => 10;

	public override DateTime 忙碌时间
	{
		get
		{
			return base.忙碌时间;
		}
		set
		{
			if (base.忙碌时间 < value)
			{
				硬直时间 = value;
				base.忙碌时间 = value;
			}
		}
	}

	public override DateTime 硬直时间
	{
		get
		{
			return base.硬直时间;
		}
		set
		{
			if (base.硬直时间 < value)
			{
				base.硬直时间 = value;
			}
		}
	}

	public override int 当前体力
	{
		get
		{
			return base.当前体力;
		}
		set
		{
			value = 计算类.数值限制(0, value, this[游戏对象属性.最大体力]);
			if (base.当前体力 != value)
			{
				base.当前体力 = value;
				发送封包(new 同步对象体力
				{
					对象编号 = 地图编号,
					当前体力 = 当前体力,
					体力上限 = this[游戏对象属性.最大体力]
				});
			}
		}
	}

	public override 地图实例 当前地图
	{
		get
		{
			return base.当前地图;
		}
		set
		{
			if (当前地图 != value)
			{
				base.当前地图?.移除对象(this);
				base.当前地图 = value;
				base.当前地图.添加对象(this);
			}
		}
	}

	public override 游戏方向 当前方向
	{
		get
		{
			return base.当前方向;
		}
		set
		{
			if (当前方向 != value)
			{
				base.当前方向 = value;
				发送封包(new 对象转动方向
				{
					转向耗时 = 100,
					对象编号 = 地图编号,
					对象朝向 = (ushort)value
				});
			}
		}
	}

	public override byte 当前等级 => 对象模板.怪物等级;

	public override string 对象名字 => 对象模板.怪物名字;

	public override 游戏对象类型 对象类型 => 游戏对象类型.怪物;

	public override 技能范围类型 对象体型 => 对象模板.怪物体型;

	public override int this[游戏对象属性 属性]
	{
		get
		{
			return base[属性];
		}
		set
		{
			base[属性] = value;
		}
	}

	public 怪物种族分类 怪物种族 => 对象模板.怪物分类;

	public 怪物级别分类 怪物级别 => 对象模板.怪物级别;

	public List<怪物掉落> 怪物掉落 => 对象模板.怪物掉落物品;

	public ushort 模板编号 => 对象模板.怪物编号;

	public int 怪物经验 => 对象模板.怪物提供经验;

	public int 仇恨范围
	{
		get
		{
			if (当前地图.地图编号 == 80)
			{
				return 25;
			}
			return 对象模板.怪物仇恨范围;
		}
	}

	public int 移动间隔 => 对象模板.怪物移动间隔;

	public int 切换间隔 => 5000;

	public int 漫游间隔 => 对象模板.怪物漫游间隔;

	public int 仇恨时长 => 对象模板.怪物仇恨时间;

	public int 尸体保留 => 对象模板.尸体保留时长;

	public bool 怪物禁止移动 => 对象模板.怪物禁止移动;

	public bool 可被技能推动 => 对象模板.可被技能推动;

	public bool 可见隐身目标 => 对象模板.可见隐身目标;

	public bool 可被技能控制 => 对象模板.可被技能控制;

	public bool 可被技能诱惑 => 对象模板.可被技能诱惑;

	public float 基础诱惑概率 => 对象模板.基础诱惑概率;

	public bool 主动攻击目标 => 对象模板.主动攻击目标;

	public 怪物实例(宠物实例 对应宠物)
	{
		地图编号 = ++地图处理网关.对象编号;
		对象模板 = 对应宠物.对象模板;
		当前地图 = 对应宠物.当前地图;
		当前坐标 = 对应宠物.当前坐标;
		当前方向 = 对应宠物.当前方向;
		宠物等级 = 对应宠物.宠物等级;
		禁止复活 = true;
		对象仇恨 = new 对象仇恨();
		存活时间 = 主程.当前时间.AddHours(2.0);
		base.恢复时间 = 主程.当前时间.AddSeconds(5.0);
		攻击时间 = 主程.当前时间.AddSeconds(1.0);
		漫游时间 = 主程.当前时间.AddMilliseconds(漫游间隔);
		属性加成[this] = 对应宠物.基础属性;
		更新对象属性();
		当前体力 = Math.Min(对应宠物.当前体力, this[游戏对象属性.最大体力]);
		string text = 对象模板.普通攻击技能;
		if (text != null && text.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.普通攻击技能, out 普通攻击技能);
		}
		string text2 = 对象模板.概率触发技能;
		if (text2 != null && text2.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.概率触发技能, out 概率触发技能);
		}
		string text3 = 对象模板.进入战斗技能;
		if (text3 != null && text3.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.进入战斗技能, out 进入战斗技能);
		}
		string text4 = 对象模板.退出战斗技能;
		if (text4 != null && text4.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.退出战斗技能, out 退出战斗技能);
		}
		string text5 = 对象模板.死亡释放技能;
		if (text5 != null && text5.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.死亡释放技能, out 死亡释放技能);
		}
		string text6 = 对象模板.移动释放技能;
		if (text6 != null && text6.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.移动释放技能, out 移动释放技能);
		}
		string text7 = 对象模板.出生释放技能;
		if (text7 != null && text7.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.出生释放技能, out 出生释放技能);
		}
		对应宠物.自身死亡处理(null, 技能击杀: false);
		对应宠物.删除对象();
		对象死亡 = false;
		base.战斗姿态 = false;
		阻塞网格 = true;
		绑定网格();
		更新邻居时处理();
		地图处理网关.添加地图对象(this);
	}

	public 怪物实例(游戏怪物 对应模板, 地图实例 出生地图, int 复活间隔, Point[] 出生范围, bool 禁止复活, bool 立即刷新)
	{
		对象模板 = 对应模板;
		this.出生地图 = 出生地图;
		当前地图 = 出生地图;
		this.复活间隔 = 复活间隔;
		this.出生范围 = 出生范围;
		this.禁止复活 = 禁止复活;
		地图编号 = ++地图处理网关.对象编号;
		属性加成[this] = 对应模板.基础属性;
		string text = 对象模板.普通攻击技能;
		if (text != null && text.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.普通攻击技能, out 普通攻击技能);
		}
		string text2 = 对象模板.概率触发技能;
		if (text2 != null && text2.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.概率触发技能, out 概率触发技能);
		}
		string text3 = 对象模板.进入战斗技能;
		if (text3 != null && text3.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.进入战斗技能, out 进入战斗技能);
		}
		string text4 = 对象模板.退出战斗技能;
		if (text4 != null && text4.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.退出战斗技能, out 退出战斗技能);
		}
		string text5 = 对象模板.死亡释放技能;
		if (text5 != null && text5.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.死亡释放技能, out 死亡释放技能);
		}
		string text6 = 对象模板.移动释放技能;
		if (text6 != null && text6.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.移动释放技能, out 移动释放技能);
		}
		string text7 = 对象模板.出生释放技能;
		if (text7 != null && text7.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.出生释放技能, out 出生释放技能);
		}
		地图处理网关.添加地图对象(this);
		if (!禁止复活)
		{
			当前地图.固定怪物总数++;
			主窗口.更新地图数据(当前地图, "固定怪物总数", 当前地图.固定怪物总数);
		}
		if (!立即刷新)
		{
			复活时间 = 主程.当前时间.AddMilliseconds(复活间隔);
			阻塞网格 = false;
			尸体消失 = true;
			对象死亡 = true;
			次要对象 = true;
			地图处理网关.添加次要对象(this);
		}
		else
		{
			怪物复活处理(计算复活: false);
		}
	}

	public override void 处理对象数据()
	{
		if (主程.当前时间 < base.预约时间)
		{
			return;
		}
		if (!禁止复活 || !(主程.当前时间 >= 存活时间))
		{
			if (对象死亡)
			{
				if (!尸体消失 && 主程.当前时间 >= 消失时间)
				{
					if (禁止复活)
					{
						删除对象();
					}
					else
					{
						尸体消失 = true;
						清空邻居时处理();
						解绑网格();
					}
				}
				if (!禁止复活 && 主程.当前时间 >= 复活时间)
				{
					清空邻居时处理();
					解绑网格();
					怪物复活处理(计算复活: true);
				}
			}
			else
			{
				foreach (KeyValuePair<ushort, Buff数据> item in Buff列表.ToList())
				{
					轮询Buff时处理(item.Value);
				}
				foreach (技能实例 item2 in 技能任务.ToList())
				{
					item2.处理任务();
				}
				if (主程.当前时间 > base.恢复时间)
				{
					if (!检查状态(游戏对象状态.中毒状态))
					{
						当前体力 += this[游戏对象属性.体力恢复];
					}
					base.恢复时间 = 主程.当前时间.AddSeconds(5.0);
				}
				if (主程.当前时间 > base.治疗时间 && base.治疗次数 > 0)
				{
					base.治疗次数--;
					base.治疗时间 = 主程.当前时间.AddMilliseconds(500.0);
					当前体力 += base.治疗基数;
				}
				if (主程.当前时间 > 忙碌时间 && 主程.当前时间 > 硬直时间)
				{
					if (进入战斗技能 != null && !base.战斗姿态 && 对象仇恨.仇恨列表.Count != 0)
					{
						new 技能实例(this, 进入战斗技能, null, base.动作编号++, 当前地图, 当前坐标, null, 当前坐标, null);
						base.战斗姿态 = true;
						base.脱战时间 = 主程.当前时间.AddSeconds(10.0);
					}
					else if (退出战斗技能 != null && base.战斗姿态 && 对象仇恨.仇恨列表.Count == 0 && 主程.当前时间 > base.脱战时间)
					{
						new 技能实例(this, 退出战斗技能, null, base.动作编号++, 当前地图, 当前坐标, null, 当前坐标, null);
						base.战斗姿态 = false;
					}
					else if (对象模板.脱战自动石化 && !base.战斗姿态 && 对象仇恨.仇恨列表.Count != 0)
					{
						base.战斗姿态 = true;
						移除Buff时处理(对象模板.石化状态编号);
						base.脱战时间 = 主程.当前时间.AddSeconds(10.0);
					}
					else if (对象模板.脱战自动石化 && base.战斗姿态 && 对象仇恨.仇恨列表.Count == 0 && 主程.当前时间 > base.脱战时间)
					{
						base.战斗姿态 = false;
						添加Buff时处理(对象模板.石化状态编号, this);
					}
					else if ((怪物级别 == 怪物级别分类.头目首领) ? 更新最近仇恨() : 更新对象仇恨())
					{
						怪物智能攻击();
					}
					else
					{
						怪物随机漫游();
					}
				}
			}
		}
		else
		{
			删除对象();
		}
		base.处理对象数据();
	}

	public override void 自身死亡处理(地图对象 对象, bool 技能击杀)
	{
		foreach (技能实例 item in 技能任务)
		{
			item.技能中断();
		}
		base.自身死亡处理(对象, 技能击杀);
		if (死亡释放技能 != null && 对象 != null)
		{
			new 技能实例(this, 死亡释放技能, null, base.动作编号++, 当前地图, 当前坐标, null, 当前坐标, null).处理任务();
		}
		if (当前地图.副本地图 || !禁止复活)
		{
			当前地图.存活怪物总数--;
			主窗口.更新地图数据(当前地图, "存活怪物总数", -1);
		}
		尸体消失 = false;
		消失时间 = 主程.当前时间.AddMilliseconds(尸体保留);
		复活时间 = 主程.当前时间.AddMilliseconds(Math.Max(复活间隔, 尸体保留 + 5000));
		if (对象 is 宠物实例 宠物实例2)
		{
			宠物实例2.宠物经验增加();
		}
		if (更新怪物归属(out var 归属玩家))
		{
			if (当前地图.地图编号 != 80)
			{
				HashSet<角色数据> 物品归属 = ((归属玩家.所属队伍 == null) ? new HashSet<角色数据> { 归属玩家.角色数据 } : new HashSet<角色数据>(归属玩家.所属队伍.队伍成员));
				float num = 计算类.收益衰减(归属玩家.当前等级, 当前等级);
				int num2 = 0;
				int num3 = 0;
				if (num < 1f)
				{
					foreach (怪物掉落 item2 in 怪物掉落)
					{
						if (!游戏物品.检索表.TryGetValue(item2.物品名字, out var value) || 计算类.计算概率(num) || (归属玩家.本期特权 == 0 && 怪物级别 != 怪物级别分类.头目首领 && value.物品分类 != 物品使用分类.可用药剂 && 计算类.计算概率(0.5f)) || (归属玩家.本期特权 == 3 && 怪物级别 != 怪物级别分类.头目首领 && value.物品分类 != 物品使用分类.可用药剂 && 计算类.计算概率(0.25f)))
						{
							continue;
						}
						int num4 = Math.Max(1, item2.掉落概率 - (int)Math.Round((decimal)item2.掉落概率 * 自定义类.怪物额外爆率));
						if (主程.随机数.Next(num4) != num4 / 2)
						{
							continue;
						}
						int num5 = 主程.随机数.Next(item2.最小数量, item2.最大数量 + 1);
						if (num5 == 0)
						{
							continue;
						}
						if (value.物品持久 != 0)
						{
							for (int i = 0; i < num5; i++)
							{
								new 物品实例(value, null, 当前地图, 当前坐标, 物品归属, 1);
							}
							当前地图.怪物掉落次数 += num5;
							num3++;
							对象模板.掉落统计[value] = ((!对象模板.掉落统计.ContainsKey(value)) ? 0 : 对象模板.掉落统计[value]) + num5;
						}
						else
						{
							new 物品实例(value, null, 当前地图, 当前坐标, 物品归属, num5);
							if (value.物品编号 == 1)
							{
								当前地图.金币掉落总数 += num5;
								num2 = num5;
							}
							对象模板.掉落统计[value] = ((!对象模板.掉落统计.ContainsKey(value)) ? 0 : 对象模板.掉落统计[value]) + num5;
						}
						if (value.贵重物品)
						{
							网络服务网关.发送公告("[" + 对象名字 + "] 被 [" + 归属玩家.对象名字 + "] 击杀, 掉落了[" + value.物品名字 + "]");
						}
					}
				}
				if (num2 > 0)
				{
					主窗口.更新地图数据(当前地图, "金币掉落总数", num2);
				}
				if (num3 > 0)
				{
					主窗口.更新地图数据(当前地图, "怪物掉落次数", num3);
				}
				if (num2 > 0 || num3 > 0)
				{
					主窗口.更新掉落统计(对象模板, 对象模板.掉落统计.ToList());
				}
				if (归属玩家.所属队伍 == null)
				{
					归属玩家.玩家增加经验(this, 怪物经验);
				}
				else
				{
					List<玩家实例> list = new List<玩家实例> { 归属玩家 };
					foreach (地图对象 item3 in 重要邻居)
					{
						if (item3 != 归属玩家 && item3 is 玩家实例 玩家实例2 && 玩家实例2.所属队伍 == 归属玩家.所属队伍)
						{
							list.Add(玩家实例2);
						}
					}
					float num6 = (float)怪物经验 * (1f + (float)(list.Count - 1) * 0.2f);
					float num7 = list.Sum((玩家实例 x) => x.当前等级);
					foreach (玩家实例 item4 in list)
					{
						item4.玩家增加经验(this, (int)(num6 * (float)(int)item4.当前等级 / num7));
					}
				}
			}
			else
			{
				int num8 = 0;
				if (游戏物品.检索表.TryGetValue("强效金创药", out var value2))
				{
					int num9 = ((怪物级别 != 怪物级别分类.普通怪物) ? 1 : 15);
					int num10 = Math.Max(1, num9 - (int)Math.Round((decimal)num9 * 自定义类.怪物额外爆率));
					if (主程.随机数.Next(num10) == num10 / 2)
					{
						num8++;
						new 物品实例(value2, null, 当前地图, 当前坐标, new HashSet<角色数据>(), 1);
					}
				}
				if (游戏物品.检索表.TryGetValue("强效魔法药", out var value3))
				{
					int num11 = ((怪物级别 != 怪物级别分类.普通怪物) ? 1 : 20);
					int num12 = Math.Max(1, num11 - (int)Math.Round((decimal)num11 * 自定义类.怪物额外爆率));
					if (主程.随机数.Next(num12) == num12 / 2)
					{
						num8++;
						new 物品实例(value3, null, 当前地图, 当前坐标, new HashSet<角色数据>(), 1);
					}
				}
				if (游戏物品.检索表.TryGetValue("疗伤药", out var value4))
				{
					int num13 = ((怪物级别 != 怪物级别分类.普通怪物) ? 1 : 100);
					int num14 = Math.Max(1, num13 - (int)Math.Round((decimal)num13 * 自定义类.怪物额外爆率));
					if (主程.随机数.Next(num14) == num14 / 2)
					{
						num8++;
						new 物品实例(value4, null, 当前地图, 当前坐标, new HashSet<角色数据>(), 1);
					}
				}
				if (游戏物品.检索表.TryGetValue("祝福油", out var value5))
				{
					int num15 = ((怪物级别 == 怪物级别分类.普通怪物) ? 1000 : ((怪物级别 == 怪物级别分类.精英干将) ? 50 : 10));
					int num16 = Math.Max(1, num15 - (int)Math.Round((decimal)num15 * 自定义类.怪物额外爆率));
					if (主程.随机数.Next(num16) == num16 / 2)
					{
						num8++;
						new 物品实例(value5, null, 当前地图, 当前坐标, new HashSet<角色数据>(), 1);
						网络服务网关.发送公告("[" + 对象名字 + "] 被 [" + 归属玩家.对象名字 + "] 击杀, 掉落了[祝福油]");
					}
				}
				if (num8 > 0)
				{
					主窗口.更新地图数据(当前地图, "怪物掉落次数", num8);
				}
				foreach (玩家实例 item5 in 当前地图.玩家列表)
				{
					item5.玩家增加经验(this, (int)((float)怪物经验 * 1.5f));
				}
			}
		}
		Buff列表.Clear();
		次要对象 = true;
		地图处理网关.添加次要对象(this);
		if (激活对象)
		{
			激活对象 = false;
			地图处理网关.移除激活对象(this);
		}
	}

	public void 怪物随机漫游()
	{
		if (怪物禁止移动 || 主程.当前时间 < 漫游时间)
		{
			return;
		}
		if (能否走动())
		{
			Point point = 计算类.前方坐标(当前坐标, 计算类.随机方向(), 1);
			if (当前地图.能否通行(point))
			{
				忙碌时间 = 主程.当前时间.AddMilliseconds(行走耗时);
				行走时间 = 主程.当前时间.AddMilliseconds(行走耗时 + 移动间隔);
				当前方向 = 计算类.计算方向(当前坐标, point);
				自身移动时处理(point);
				if (!对象死亡)
				{
					发送封包(new 对象角色走动
					{
						对象编号 = 地图编号,
						移动坐标 = 当前坐标,
						移动速度 = base.行走速度
					});
				}
			}
		}
		漫游时间 = 主程.当前时间.AddMilliseconds(漫游间隔 + 主程.随机数.Next(5000));
	}

	public void 怪物智能攻击()
	{
		base.脱战时间 = 主程.当前时间.AddSeconds(10.0);
		游戏技能 游戏技能 = null;
		if (概率触发技能 == null || (冷却记录.ContainsKey(概率触发技能.自身技能编号 | 0x1000000) && !(主程.当前时间 > 冷却记录[概率触发技能.自身技能编号 | 0x1000000])) || !计算类.计算概率(概率触发技能.计算触发概率))
		{
			if (普通攻击技能 == null || (冷却记录.ContainsKey(普通攻击技能.自身技能编号 | 0x1000000) && !(主程.当前时间 > 冷却记录[普通攻击技能.自身技能编号 | 0x1000000])))
			{
				return;
			}
			游戏技能 = 普通攻击技能;
		}
		else
		{
			游戏技能 = 概率触发技能;
		}
		if (检查状态(游戏对象状态.忙绿状态 | 游戏对象状态.麻痹状态 | 游戏对象状态.失神状态))
		{
			return;
		}
		if (网格距离(对象仇恨.当前目标) <= 游戏技能.技能最远距离)
		{
			if (!游戏技能.需要正向走位 || 计算类.直线方向(当前坐标, 对象仇恨.当前目标.当前坐标))
			{
				if (!(主程.当前时间 > 攻击时间))
				{
					if (!怪物禁止移动 && 能否转动())
					{
						当前方向 = 计算类.计算方向(当前坐标, 对象仇恨.当前目标.当前坐标);
					}
				}
				else
				{
					new 技能实例(this, 游戏技能, null, base.动作编号++, 当前地图, 当前坐标, 对象仇恨.当前目标, 对象仇恨.当前目标.当前坐标, null);
					攻击时间 = 主程.当前时间.AddMilliseconds(计算类.数值限制(0, 10 - this[游戏对象属性.攻击速度], 10) * 500);
				}
			}
			else
			{
				if (怪物禁止移动 || !能否走动())
				{
					return;
				}
				游戏方向 方向 = 计算类.正向方向(当前坐标, 对象仇恨.当前目标.当前坐标);
				Point point = default(Point);
				for (int i = 0; i < 8; i++)
				{
					if (!当前地图.能否通行(point = 计算类.前方坐标(当前坐标, 方向, 1)))
					{
						方向 = 计算类.旋转方向(方向, (主程.随机数.Next(2) != 0) ? 1 : (-1));
						continue;
					}
					当前方向 = 计算类.计算方向(当前坐标, point);
					忙碌时间 = 主程.当前时间.AddMilliseconds(行走耗时);
					行走时间 = 主程.当前时间.AddMilliseconds(行走耗时 + 移动间隔);
					自身移动时处理(point);
					if (!对象死亡)
					{
						发送封包(new 对象角色走动
						{
							对象编号 = 地图编号,
							移动坐标 = point,
							移动速度 = base.行走速度
						});
					}
					break;
				}
			}
		}
		else
		{
			if (怪物禁止移动 || !能否走动())
			{
				return;
			}
			游戏方向 方向2 = 计算类.计算方向(当前坐标, 对象仇恨.当前目标.当前坐标);
			Point point2 = default(Point);
			for (int j = 0; j < 8; j++)
			{
				if (!当前地图.能否通行(point2 = 计算类.前方坐标(当前坐标, 方向2, 1)))
				{
					方向2 = 计算类.旋转方向(方向2, (主程.随机数.Next(2) != 0) ? 1 : (-1));
					continue;
				}
				忙碌时间 = 主程.当前时间.AddMilliseconds(行走耗时);
				行走时间 = 主程.当前时间.AddMilliseconds(行走耗时 + 移动间隔);
				当前方向 = 计算类.计算方向(当前坐标, point2);
				发送封包(new 对象角色走动
				{
					对象编号 = 地图编号,
					移动坐标 = point2,
					移动速度 = base.行走速度
				});
				自身移动时处理(point2);
				break;
			}
		}
	}

	public void 怪物复活处理(bool 计算复活)
	{
		if (当前地图.副本地图 || !禁止复活)
		{
			当前地图.存活怪物总数++;
			主窗口.更新地图数据(当前地图, "存活怪物总数", 1);
			if (计算复活)
			{
				当前地图.怪物复活次数++;
				主窗口.更新地图数据(当前地图, "怪物复活次数", 1);
			}
		}
		更新对象属性();
		当前地图 = 出生地图;
		当前方向 = 计算类.随机方向();
		当前体力 = this[游戏对象属性.最大体力];
		当前坐标 = 出生范围[主程.随机数.Next(0, 出生范围.Length)];
		Point point = 当前坐标;
		for (int i = 0; i < 100; i++)
		{
			if (!当前地图.空间阻塞(point = 计算类.螺旋坐标(当前坐标, i)))
			{
				当前坐标 = point;
				break;
			}
		}
		攻击时间 = 主程.当前时间.AddSeconds(1.0);
		base.恢复时间 = 主程.当前时间.AddMilliseconds(主程.随机数.Next(5000));
		漫游时间 = 主程.当前时间.AddMilliseconds(主程.随机数.Next(5000) + 漫游间隔);
		对象仇恨 = new 对象仇恨();
		次要对象 = false;
		对象死亡 = false;
		base.战斗姿态 = false;
		阻塞网格 = true;
		绑定网格();
		更新邻居时处理();
		if (!激活对象)
		{
			if (对象模板.脱战自动石化)
			{
				添加Buff时处理(对象模板.石化状态编号, this);
			}
			if (退出战斗技能 != null)
			{
				new 技能实例(this, 退出战斗技能, null, base.动作编号++, 当前地图, 当前坐标, null, 当前坐标, null).处理任务();
			}
		}
	}

	public void 怪物诱惑处理()
	{
		Buff列表.Clear();
		尸体消失 = true;
		对象死亡 = true;
		阻塞网格 = false;
		if (!禁止复活)
		{
			清空邻居时处理();
			解绑网格();
			复活时间 = 主程.当前时间.AddMilliseconds(复活间隔);
			次要对象 = true;
			地图处理网关.添加次要对象(this);
			激活对象 = false;
			地图处理网关.移除激活对象(this);
		}
		else
		{
			删除对象();
		}
	}

	public void 怪物沉睡处理()
	{
		if (激活对象)
		{
			激活对象 = false;
			技能任务.Clear();
			地图处理网关.移除激活对象(this);
		}
		if (禁止复活 && !次要对象)
		{
			次要对象 = true;
			技能任务.Clear();
			地图处理网关.添加次要对象(this);
		}
	}

	public void 怪物激活处理()
	{
		if (!激活对象)
		{
			次要对象 = false;
			激活对象 = true;
			地图处理网关.添加激活对象(this);
			int num = (int)Math.Max(0.0, (主程.当前时间 - base.恢复时间).TotalSeconds / 5.0);
			base.当前体力 = Math.Min(this[游戏对象属性.最大体力], 当前体力 + num * this[游戏对象属性.体力恢复]);
			base.恢复时间 = base.恢复时间.AddSeconds(5.0);
			攻击时间 = 主程.当前时间.AddSeconds(1.0);
			漫游时间 = 主程.当前时间.AddMilliseconds(主程.随机数.Next(5000) + 漫游间隔);
		}
	}

	public bool 更新对象仇恨()
	{
		if (对象仇恨.仇恨列表.Count == 0)
		{
			return false;
		}
		if (对象仇恨.当前目标 != null)
		{
			if (!对象仇恨.当前目标.对象死亡)
			{
				if (!邻居列表.Contains(对象仇恨.当前目标))
				{
					对象仇恨.移除仇恨(对象仇恨.当前目标);
				}
				else if (!对象仇恨.仇恨列表.ContainsKey(对象仇恨.当前目标))
				{
					对象仇恨.移除仇恨(对象仇恨.当前目标);
				}
				else if (网格距离(对象仇恨.当前目标) > 仇恨范围 && 主程.当前时间 > 对象仇恨.仇恨列表[对象仇恨.当前目标].仇恨时间)
				{
					对象仇恨.移除仇恨(对象仇恨.当前目标);
				}
				else if (网格距离(对象仇恨.当前目标) <= 仇恨范围)
				{
					对象仇恨.仇恨列表[对象仇恨.当前目标].仇恨时间 = 主程.当前时间.AddMilliseconds(仇恨时长);
				}
			}
			else
			{
				对象仇恨.移除仇恨(对象仇恨.当前目标);
			}
		}
		else
		{
			对象仇恨.切换时间 = default(DateTime);
		}
		if (对象仇恨.切换时间 < 主程.当前时间 && 对象仇恨.切换仇恨(this))
		{
			对象仇恨.切换时间 = 主程.当前时间.AddMilliseconds(切换间隔);
		}
		if (对象仇恨.当前目标 != null)
		{
			return true;
		}
		return 更新对象仇恨();
	}

	public bool 更新最近仇恨()
	{
		if (对象仇恨.仇恨列表.Count == 0)
		{
			return false;
		}
		if (对象仇恨.当前目标 == null)
		{
			对象仇恨.切换时间 = default(DateTime);
		}
		else if (对象仇恨.当前目标.对象死亡)
		{
			对象仇恨.移除仇恨(对象仇恨.当前目标);
		}
		else if (!邻居列表.Contains(对象仇恨.当前目标))
		{
			对象仇恨.移除仇恨(对象仇恨.当前目标);
		}
		else if (对象仇恨.仇恨列表.ContainsKey(对象仇恨.当前目标))
		{
			if (网格距离(对象仇恨.当前目标) > 仇恨范围 && 主程.当前时间 > 对象仇恨.仇恨列表[对象仇恨.当前目标].仇恨时间)
			{
				对象仇恨.移除仇恨(对象仇恨.当前目标);
			}
			else if (网格距离(对象仇恨.当前目标) <= 仇恨范围)
			{
				对象仇恨.仇恨列表[对象仇恨.当前目标].仇恨时间 = 主程.当前时间.AddMilliseconds(仇恨时长);
			}
		}
		else
		{
			对象仇恨.移除仇恨(对象仇恨.当前目标);
		}
		if (对象仇恨.切换时间 < 主程.当前时间 && 对象仇恨.最近仇恨(this))
		{
			对象仇恨.切换时间 = 主程.当前时间.AddMilliseconds(切换间隔);
		}
		if (对象仇恨.当前目标 == null)
		{
			return 更新对象仇恨();
		}
		return true;
	}

	public void 清空怪物仇恨()
	{
		对象仇恨.当前目标 = null;
		对象仇恨.仇恨列表.Clear();
	}

	public bool 更新怪物归属(out 玩家实例 归属玩家)
	{
		foreach (KeyValuePair<地图对象, 对象仇恨.仇恨详情> item in 对象仇恨.仇恨列表.ToList())
		{
			if (item.Key is 宠物实例 宠物实例2)
			{
				if (item.Value.仇恨数值 > 0)
				{
					对象仇恨.添加仇恨(宠物实例2.宠物主人, item.Value.仇恨时间, item.Value.仇恨数值);
				}
				对象仇恨.移除仇恨(item.Key);
			}
			else if (!(item.Key is 玩家实例))
			{
				对象仇恨.移除仇恨(item.Key);
			}
		}
		地图对象 地图对象2 = (from x in 对象仇恨.仇恨列表.Keys.ToList()
			orderby 对象仇恨.仇恨列表[x].仇恨数值 descending
			select x).FirstOrDefault();
		object obj = ((地图对象2 == null || !(地图对象2 is 玩家实例 玩家实例2)) ? null : 玩家实例2);
		归属玩家 = (玩家实例)obj;
		return 归属玩家 != null;
	}

	static 怪物实例()
	{
	}
}
