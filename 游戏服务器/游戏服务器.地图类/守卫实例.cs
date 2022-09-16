using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using 游戏服务器.模板类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public sealed class 守卫实例 : 地图对象
{
	public 地图守卫 对象模板;

	public 对象仇恨 对象仇恨;

	public Point 出生坐标;

	public 游戏方向 出生方向;

	public 地图实例 出生地图;

	public 游戏技能 普攻技能;

	public bool 尸体消失 { get; set; }

	public DateTime 复活时间 { get; set; }

	public DateTime 消失时间 { get; set; }

	public DateTime 转移计时 { get; set; }

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
				DateTime dateTime2 = (硬直时间 = value);
				DateTime dateTime3 = dateTime2;
				dateTime2 = (base.忙碌时间 = dateTime3);
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

	public override byte 当前等级 => 对象模板.守卫等级;

	public override bool 能被命中
	{
		get
		{
			if (能否受伤)
			{
				return !对象死亡;
			}
			return false;
		}
	}

	public override string 对象名字 => 对象模板.守卫名字;

	public override 游戏对象类型 对象类型 => 游戏对象类型.Npcc;

	public override 技能范围类型 对象体型 => 技能范围类型.单体1x1;

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

	public int 仇恨范围 => 10;

	public ushort 模板编号 => 对象模板.守卫编号;

	public int 复活间隔 => 对象模板.复活间隔;

	public int 商店编号 => 对象模板.商店编号;

	public string 界面代码 => 对象模板.界面代码;

	public bool 能否受伤 => 对象模板.能否受伤;

	public bool 主动攻击目标 => 对象模板.主动攻击;

	public 守卫实例(地图守卫 对应模板, 地图实例 出生地图, 游戏方向 出生方向, Point 出生坐标)
	{
		对象模板 = 对应模板;
		this.出生地图 = 出生地图;
		当前地图 = 出生地图;
		this.出生方向 = 出生方向;
		this.出生坐标 = 出生坐标;
		地图编号 = ++地图处理网关.对象编号;
		属性加成[this] = new Dictionary<游戏对象属性, int> { [游戏对象属性.最大体力] = 9999 };
		string text = 对象模板.普攻技能;
		if (text != null && text.Length > 0)
		{
			游戏技能.数据表.TryGetValue(对象模板.普攻技能, out 普攻技能);
		}
		地图处理网关.添加地图对象(this);
		守卫复活处理();
	}

	public override void 处理对象数据()
	{
		if (主程.当前时间 < base.预约时间)
		{
			return;
		}
		if (!对象死亡)
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
					当前体力 += 5;
				}
				base.恢复时间 = 主程.当前时间.AddSeconds(5.0);
			}
			if (主动攻击目标 && 主程.当前时间 > 忙碌时间 && 主程.当前时间 > 硬直时间)
			{
				if (!更新对象仇恨())
				{
					if (对象仇恨.仇恨列表.Count == 0 && 能否转动())
					{
						当前方向 = 出生方向;
					}
				}
				else
				{
					守卫智能攻击();
				}
			}
			if (模板编号 == 6121 && 当前地图.地图编号 == 183 && 主程.当前时间 > 转移计时)
			{
				清空邻居时处理();
				解绑网格();
				当前坐标 = 当前地图.传送区域.随机坐标;
				绑定网格();
				更新邻居时处理();
				转移计时 = 主程.当前时间.AddMinutes(2.5);
			}
		}
		else
		{
			if (!尸体消失 && 主程.当前时间 >= 消失时间)
			{
				清空邻居时处理();
				解绑网格();
				尸体消失 = true;
			}
			if (主程.当前时间 >= 复活时间)
			{
				清空邻居时处理();
				解绑网格();
				守卫复活处理();
			}
		}
		base.处理对象数据();
	}

	public override void 自身死亡处理(地图对象 对象, bool 技能击杀)
	{
		base.自身死亡处理(对象, 技能击杀);
		消失时间 = 主程.当前时间.AddMilliseconds(10000.0);
		复活时间 = 主程.当前时间.AddMilliseconds((当前地图.地图编号 == 80) ? int.MaxValue : 60000);
		Buff列表.Clear();
		次要对象 = true;
		地图处理网关.添加次要对象(this);
		if (激活对象)
		{
			激活对象 = false;
			地图处理网关.移除激活对象(this);
		}
	}

	public void 守卫沉睡处理()
	{
		if (激活对象)
		{
			激活对象 = false;
			技能任务.Clear();
			地图处理网关.移除激活对象(this);
		}
	}

	public void 守卫激活处理()
	{
		if (!激活对象)
		{
			激活对象 = true;
			地图处理网关.添加激活对象(this);
			int num = (int)Math.Max(0.0, (主程.当前时间 - base.恢复时间).TotalSeconds / 5.0);
			base.当前体力 = Math.Min(this[游戏对象属性.最大体力], 当前体力 + num * this[游戏对象属性.体力恢复]);
			base.恢复时间 = base.恢复时间.AddSeconds(5.0);
		}
	}

	public void 守卫智能攻击()
	{
		if (!检查状态(游戏对象状态.麻痹状态 | 游戏对象状态.失神状态) && 普攻技能 != null)
		{
			if (网格距离(对象仇恨.当前目标) <= 普攻技能.技能最远距离)
			{
				new 技能实例(this, 普攻技能, null, base.动作编号++, 当前地图, 当前坐标, 对象仇恨.当前目标, 对象仇恨.当前目标.当前坐标, null);
			}
			else
			{
				对象仇恨.移除仇恨(对象仇恨.当前目标);
			}
		}
	}

	public void 守卫复活处理()
	{
		更新对象属性();
		次要对象 = false;
		对象死亡 = false;
		阻塞网格 = !对象模板.虚无状态;
		当前地图 = 出生地图;
		当前方向 = 出生方向;
		当前坐标 = 出生坐标;
		当前体力 = this[游戏对象属性.最大体力];
		base.恢复时间 = 主程.当前时间.AddMilliseconds(主程.随机数.Next(5000));
		对象仇恨 = new 对象仇恨();
		绑定网格();
		更新邻居时处理();
	}

	public bool 更新对象仇恨()
	{
		if (对象仇恨.仇恨列表.Count != 0)
		{
			if (对象仇恨.当前目标 == null)
			{
				return 对象仇恨.切换仇恨(this);
			}
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
				else if (网格距离(对象仇恨.当前目标) > 仇恨范围)
				{
					对象仇恨.移除仇恨(对象仇恨.当前目标);
				}
			}
			else
			{
				对象仇恨.移除仇恨(对象仇恨.当前目标);
			}
			if (对象仇恨.当前目标 != null)
			{
				return true;
			}
			return 更新对象仇恨();
		}
		return false;
	}

	public void 清空守卫仇恨()
	{
		对象仇恨.当前目标 = null;
		对象仇恨.仇恨列表.Clear();
	}

	static 守卫实例()
	{
	}
}
