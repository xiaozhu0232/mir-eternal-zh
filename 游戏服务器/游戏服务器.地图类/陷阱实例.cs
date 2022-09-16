using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using 游戏服务器.模板类;
using 游戏服务器.网络类;

namespace 游戏服务器.地图类;

public sealed class 陷阱实例 : 地图对象
{
	public byte 陷阱等级;

	public ushort 陷阱编号;

	public DateTime 放置时间;

	public DateTime 消失时间;

	public DateTime 触发时间;

	public 地图对象 陷阱来源;

	public 技能陷阱 陷阱模板;

	public HashSet<地图对象> 被动触发列表;

	public byte 陷阱移动次数;

	public 游戏技能 被动触发技能;

	public 游戏技能 主动触发技能;

	public ushort 陷阱分组编号 => 陷阱模板.分组编号;

	public ushort 主动触发间隔 => 陷阱模板.主动触发间隔;

	public ushort 主动触发延迟 => 陷阱模板.主动触发延迟;

	public ushort 陷阱剩余时间 => (ushort)Math.Ceiling((消失时间 - 主程.当前时间).TotalMilliseconds / 62.5);

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

	public override int 处理间隔 => 10;

	public override byte 当前等级
	{
		get
		{
			return 陷阱来源.当前等级;
		}
		set
		{
			陷阱来源.当前等级 = value;
		}
	}

	public override bool 阻塞网格
	{
		get
		{
			return false;
		}
		set
		{
			base.阻塞网格 = value;
		}
	}

	public override bool 能被命中 => false;

	public override string 对象名字 => 陷阱模板.陷阱名字;

	public override 游戏对象类型 对象类型 => 游戏对象类型.陷阱;

	public override 技能范围类型 对象体型 => 陷阱模板.陷阱体型;

	public override Dictionary<游戏对象属性, int> 当前属性 => base.当前属性;

	public 陷阱实例(地图对象 来源, 技能陷阱 模板, 地图实例 地图, Point 坐标)
	{
		陷阱来源 = 来源;
		陷阱模板 = 模板;
		当前地图 = 地图;
		当前坐标 = 坐标;
		行走时间 = 主程.当前时间;
		放置时间 = 主程.当前时间;
		陷阱编号 = 模板.陷阱编号;
		当前方向 = 陷阱来源.当前方向;
		被动触发列表 = new HashSet<地图对象>();
		消失时间 = 放置时间 + TimeSpan.FromMilliseconds(陷阱模板.陷阱持续时间);
		触发时间 = 放置时间 + TimeSpan.FromMilliseconds((int)陷阱模板.主动触发延迟);
		if (来源 is 玩家实例 玩家实例2)
		{
			if (陷阱模板.绑定等级 != 0 && 玩家实例2.主体技能表.TryGetValue(陷阱模板.绑定等级, out var v))
			{
				陷阱等级 = v.技能等级.V;
			}
			if (陷阱模板.持续时间延长 && 陷阱模板.技能等级延时)
			{
				消失时间 += TimeSpan.FromMilliseconds(陷阱等级 * 陷阱模板.每级延长时间);
			}
			if (陷阱模板.持续时间延长 && 陷阱模板.角色属性延时)
			{
				消失时间 += TimeSpan.FromMilliseconds((float)玩家实例2[陷阱模板.绑定角色属性] * 陷阱模板.属性延时系数);
			}
			if (陷阱模板.持续时间延长 && 陷阱模板.特定铭文延时 && 玩家实例2.主体技能表.TryGetValue((ushort)(陷阱模板.特定铭文技能 / 10), out var v2) && v2.铭文编号 == 陷阱模板.特定铭文技能 % 10)
			{
				消失时间 += TimeSpan.FromMilliseconds(陷阱模板.铭文延长时间);
			}
		}
		主动触发技能 = ((陷阱模板.主动触发技能 == null || !游戏技能.数据表.ContainsKey(陷阱模板.主动触发技能)) ? null : 游戏技能.数据表[陷阱模板.主动触发技能]);
		被动触发技能 = ((陷阱模板.被动触发技能 == null || !游戏技能.数据表.ContainsKey(陷阱模板.被动触发技能)) ? null : 游戏技能.数据表[陷阱模板.被动触发技能]);
		地图编号 = ++地图处理网关.陷阱编号;
		绑定网格();
		更新邻居时处理();
		地图处理网关.添加地图对象(this);
		激活对象 = true;
		地图处理网关.添加激活对象(this);
	}

	public override void 处理对象数据()
	{
		if (主程.当前时间 < base.预约时间)
		{
			return;
		}
		if (!(主程.当前时间 > 消失时间))
		{
			foreach (技能实例 item in 技能任务.ToList())
			{
				item.处理任务();
			}
			if (主动触发技能 != null && 主程.当前时间 > 触发时间)
			{
				主动触发陷阱();
			}
			if (陷阱模板.陷阱能否移动 && 陷阱移动次数 < 陷阱模板.限制移动次数 && 主程.当前时间 > 行走时间)
			{
				if (陷阱模板.当前方向移动)
				{
					自身移动时处理(计算类.前方坐标(当前坐标, 当前方向, 1));
					发送封包(new 陷阱移动位置
					{
						陷阱编号 = 地图编号,
						移动坐标 = 当前坐标,
						移动高度 = 当前高度,
						移动速度 = 陷阱模板.陷阱移动速度
					});
				}
				if (被动触发技能 != null)
				{
					Point[] array = 计算类.技能范围(当前坐标, 当前方向, 对象体型);
					foreach (Point 坐标 in array)
					{
						foreach (地图对象 item2 in 当前地图[坐标].ToList())
						{
							被动触发陷阱(item2);
						}
					}
				}
				陷阱移动次数++;
				行走时间 = 行走时间.AddMilliseconds(陷阱模板.陷阱移动速度 * 60);
			}
		}
		else
		{
			陷阱消失处理();
		}
		base.处理对象数据();
	}

	public void 被动触发陷阱(地图对象 对象)
	{
		if (!(主程.当前时间 > 消失时间) && 被动触发技能 != null && !对象.对象死亡 && (对象.对象类型 & 陷阱模板.被动限定类型) != 0 && 对象.特定类型(陷阱来源, 陷阱模板.被动指定类型) && (陷阱来源.对象关系(对象) & 陷阱模板.被动限定关系) != 0 && (!陷阱模板.禁止重复触发 || 被动触发列表.Add(对象)))
		{
			new 技能实例(this, 被动触发技能, null, 0, 当前地图, 当前坐标, 对象, 对象.当前坐标, null);
		}
	}

	public void 主动触发陷阱()
	{
		if (!(主程.当前时间 > 消失时间))
		{
			new 技能实例(this, 主动触发技能, null, 0, 当前地图, 当前坐标, null, 当前坐标, null);
			触发时间 += TimeSpan.FromMilliseconds((int)主动触发间隔);
		}
	}

	public void 陷阱消失处理()
	{
		删除对象();
	}

	static 陷阱实例()
	{
	}
}
