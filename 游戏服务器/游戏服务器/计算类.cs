using System;
using System.Collections.Generic;
using System.Drawing;
using 游戏服务器.地图类;
using 游戏服务器.模板类;

namespace 游戏服务器;

public static class 计算类
{
	public static readonly DateTime 系统相对时间;

	public static int 扩展背包(int 扩展次数, int 当前消耗 = 0, int 当前位置 = 1, int 累计消耗 = 0)
	{
		if (当前位置 <= 扩展次数)
		{
			if (当前位置 <= 1)
			{
				int num = 累计消耗;
				当前消耗 = 2000;
				累计消耗 = num + 2000;
			}
			else if (当前位置 <= 16)
			{
				累计消耗 += (当前消耗 += 1000);
			}
			else if (当前位置 != 17)
			{
				累计消耗 += (当前消耗 += 10000);
			}
			else
			{
				int num2 = 累计消耗;
				当前消耗 = 20000;
				累计消耗 = num2 + 20000;
			}
			return 扩展背包(扩展次数, 当前消耗, 当前位置 + 1, 累计消耗);
		}
		return 累计消耗;
	}

	public static int 资源背包(int 扩展次数, int 当前消耗 = 0, int 当前位置 = 1, int 累计消耗 = 0)
	{
		if (当前位置 > 扩展次数)
		{
			return 累计消耗;
		}
		if (当前位置 <= 1)
		{
			int num = 累计消耗;
			当前消耗 = 10000;
			累计消耗 = num + 10000;
		}
		else
		{
			累计消耗 += (当前消耗 += 10000);
		}
		return 资源背包(扩展次数, 当前消耗, 当前位置 + 1, 累计消耗);
	}

	public static int 扩展仓库(int 扩展次数, int 当前消耗 = 0, int 当前位置 = 1, int 累计消耗 = 0)
	{
		if (当前位置 <= 扩展次数)
		{
			if (当前位置 > 1)
			{
				if (当前位置 > 24)
				{
					if (当前位置 == 25)
					{
						int num = 累计消耗;
						当前消耗 = 30000;
						累计消耗 = num + 30000;
					}
					else
					{
						累计消耗 += (当前消耗 += 10000);
					}
				}
				else
				{
					累计消耗 += (当前消耗 += 1000);
				}
			}
			else
			{
				int num2 = 累计消耗;
				当前消耗 = 2000;
				累计消耗 = num2 + 2000;
			}
			return 扩展仓库(扩展次数, 当前消耗, 当前位置 + 1, 累计消耗);
		}
		return 累计消耗;
	}
	
	public static int 扩展资源背包(int 扩展次数, int 当前消耗 = 0, int 当前位置 = 1, int 累计消耗 = 0)
	{
		if (当前位置 > 扩展次数)
		{
			return 累计消耗;
		}
		if (当前位置 <= 1)
		{
			int num = 累计消耗;
			当前消耗 = 10000;
			累计消耗 = num + 10000;
		}
		else
		{
			累计消耗 += (当前消耗 += 10000);
		}
		return 扩展资源背包(扩展次数, 当前消耗, 当前位置 + 1, 累计消耗);
	}

	public static int 数值限制(int 下限, int 数值, int 上限)
	{
		if (数值 > 上限)
		{
			return 上限;
		}
		if (数值 < 下限)
		{
			return 下限;
		}
		return 数值;
	}

	public static int 网格距离(Point 原点, Point 终点)
	{
		int val = Math.Abs(终点.X - 原点.X);
		int val2 = Math.Abs(终点.Y - 原点.Y);
		return Math.Max(val, val2);
	}

	public static int 时间转换(DateTime 时间)
	{
		return (int)(时间 - 系统相对时间).TotalSeconds;
	}

	public static bool 日期同周(DateTime 日期一, DateTime 日期二)
	{
		DateTime dateTime = ((!(日期二 > 日期一)) ? 日期一 : 日期二);
		DateTime dateTime2 = ((!(日期二 > 日期一)) ? 日期二 : 日期一);
		if ((dateTime - dateTime2).TotalDays > 7.0)
		{
			return false;
		}
		int num = Convert.ToInt32(dateTime.DayOfWeek);
		if (num == 0)
		{
			num = 7;
		}
		int num2 = Convert.ToInt32(dateTime2.DayOfWeek);
		if (num2 == 0)
		{
			num2 = 7;
		}
		if (num2 > num)
		{
			return false;
		}
		return true;
	}

	public static float 收益衰减(int 玩家等级, int 怪物等级)
	{
		decimal val = (decimal)Math.Max(0, 玩家等级 - 怪物等级 - 自定义类.减收益等级差) * 自定义类.收益减少比率;
		return (float)Math.Max(0m, val);
	}

	public static bool 计算概率(float 概率)
	{
		if (!(概率 >= 1f))
		{
			if (!(概率 <= 0f))
			{
				return 概率 * 100000000f > (float)主程.随机数.Next(100000000);
			}
			return false;
		}
		return true;
	}

	public static Point 螺旋坐标(Point 原点, int 步数)
	{
		if (--步数 >= 0)
		{
			int num = (int)Math.Floor(Math.Sqrt((double)步数 + 0.25) - 0.5);
			int num2 = num * (num + 1);
			int num3 = num2 + num + 1;
			int num4 = ((num & 1) << 1) - 1;
			int num5 = num4 * (num + 1 >> 1);
			int num6 = num5;
			if (步数 < num3)
			{
				num5 -= num4 * (步数 - num2 + 1);
			}
			else
			{
				num5 -= num4 * (num + 1);
				num6 -= num4 * (步数 - num3 + 1);
			}
			return new Point(原点.X + num5, 原点.Y + num6);
		}
		return 原点;
	}

	public static Point 前方坐标(Point 原点, Point 终点, int 步数)
	{
		if (原点 == 终点)
		{
			return 原点;
		}
		float num = (float)步数 / (float)网格距离(原点, 终点);
		int num2 = (int)Math.Round((float)(终点.X - 原点.X) * num);
		int num3 = (int)Math.Round((float)(终点.Y - 原点.Y) * num);
		return new Point(原点.X + num2, 原点.Y + num3);
	}

	public static Point 前方坐标(Point 原点, 游戏方向 方向, int 步数)
	{
		return 方向 switch
		{
			游戏方向.下方 => new Point(原点.X, 原点.Y - 步数), 
			游戏方向.右下 => new Point(原点.X - 步数, 原点.Y - 步数), 
			游戏方向.右方 => new Point(原点.X - 步数, 原点.Y), 
			游戏方向.右上 => new Point(原点.X - 步数, 原点.Y + 步数), 
			游戏方向.上方 => new Point(原点.X, 原点.Y + 步数), 
			游戏方向.左上 => new Point(原点.X + 步数, 原点.Y + 步数), 
			游戏方向.左方 => new Point(原点.X + 步数, 原点.Y), 
			_ => new Point(原点.X + 步数, 原点.Y - 步数), 
		};
	}

	public static 游戏方向 随机方向()
	{
		return (游戏方向)(主程.随机数.Next(8) * 1024);
	}

	public static 游戏方向 计算方向(Point 原点, Point 终点)
	{
		int num = 终点.X - 原点.X;
		return (游戏方向)((int)(Math.Round((Math.Atan2(终点.Y - 原点.Y, num) * 180.0 / Math.PI + 360.0) % 360.0 / 45.0) * 1024.0) % 8192);
	}

	public static 游戏方向 正向方向(Point 原点, Point 终点)
	{
		if (原点 == 终点)
		{
			return 游戏方向.左方;
		}
		游戏方向 方向 = 计算方向(终点, 原点);
		int 步数 = Math.Max(Math.Abs(终点.X - 原点.X), Math.Abs(终点.Y - 原点.Y)) - 1;
		Point 终点2 = 前方坐标(终点, 方向, 步数);
		return 计算方向(原点, 终点2);
	}

	public static 游戏方向 旋转方向(游戏方向 当前方向, int 旋转向量)
	{
		return (游戏方向)((int)(当前方向 + 旋转向量 % 8 * 1024 + 8192) % 8192);
	}

	public static Point 点阵坐标转协议坐标(Point 点阵坐标)
	{
		return new Point(点阵坐标.X * 32 - 16, 点阵坐标.Y * 32 - 16);
	}

	public static Point 协议坐标转点阵坐标(Point 协议坐标)
	{
		return new Point((int)Math.Round(((float)协议坐标.X + 16f) / 32f), (int)Math.Round(((float)协议坐标.Y + 16f) / 32f));
	}

	public static Point 游戏坐标转点阵坐标(PointF 游戏坐标)
	{
		PointF pointF = default(PointF);
		pointF.Y = (游戏坐标.X + 游戏坐标.Y) / 0.707107f / 0.000976562f / 2f / 4096f;
		pointF.X = (游戏坐标.X / 0.707107f / 0.000976562f + 134217730f) / 4096f - pointF.Y;
		return new Point((int)((double)(pointF.X / 32f) + 0.5), (int)((double)(pointF.Y / 32f) + 0.5));
	}

	public static PointF 点阵坐标转游戏坐标(Point 点阵坐标)
	{
		PointF pointF = new PointF(((float)点阵坐标.X - 0.5f) * 32f, ((float)点阵坐标.Y - 0.5f) * 32f);
		PointF result = default(PointF);
		result.X = ((pointF.Y + pointF.X) * 4096f - 134217730f) * 0.707107f * 0.000976562f;
		result.Y = ((pointF.Y - pointF.X) * 4096f + 134217730f) * 0.707107f * 0.000976562f;
		return result;
	}

	public static int 计算攻速(int 攻速)
	{
		return 数值限制(-5, 攻速, 5) * 50;
	}

	public static float 计算幸运(int 幸运)
	{
		switch (幸运)
		{
		default:
			if (幸运 >= 9)
			{
				return 1f;
			}
			return 0f;
		case 0:
			return 0.1f;
		case 1:
			return 0.11f;
		case 2:
			return 0.13f;
		case 3:
			return 0.14f;
		case 4:
			return 0.17f;
		case 5:
			return 0.2f;
		case 6:
			return 0.25f;
		case 7:
			return 0.33f;
		case 8:
			return 0.5f;
		}
	}

	public static int 计算攻击(int 下限, int 上限, int 幸运)
	{
		int result = ((幸运 >= 0) ? 上限 : 下限);
		if (计算概率(计算幸运(Math.Abs(幸运))))
		{
			return result;
		}
		return 主程.随机数.Next(Math.Min(下限, 上限), Math.Max(下限, 上限) + 1);
	}

	public static int 计算防御(int 下限, int 上限)
	{
		if (上限 < 下限)
		{
			return 主程.随机数.Next(上限, 下限 + 1);
		}
		return 主程.随机数.Next(下限, 上限 + 1);
	}

	public static bool 直线方向(Point 原点, Point 锚点)
	{
		int num = 原点.X - 锚点.X;
		int num2 = 原点.Y - 锚点.Y;
		if (num == 0 || num2 == 0)
		{
			return true;
		}
		return Math.Abs(num) == Math.Abs(num2);
	}

	public static bool 计算命中(float 命中基数, float 闪避基数, float 命中系数, float 闪避系数)
	{
		float 概率 = ((闪避基数 == 0f) ? 1f : (命中基数 / 闪避基数));
		float num = 命中系数 - 闪避系数;
		if (num == 0f)
		{
			return 计算概率(概率);
		}
		if (!(num >= 0f))
		{
			if (计算概率(概率))
			{
				return !计算概率(0f - num);
			}
			return false;
		}
		if (!计算概率(概率))
		{
			return 计算概率(num);
		}
		return true;
	}

	public static bool 计算位移(地图实例 地图, 地图对象 来源, 游戏方向 方向, int 力度, out List<地图对象> 目标)
	{
		目标 = new List<地图对象>();
		return false;
	}

	public static Point[] 技能范围(Point 锚点, 游戏方向 方向, 技能范围类型 范围)
	{
		return 范围 switch
		{
			技能范围类型.单体1x1 => new Point[1] { 锚点 }, 
			技能范围类型.半月3x1 => 方向 switch
			{
				游戏方向.左方 => new Point[5]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1)
				}, 
				游戏方向.上方 => new Point[5]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 1)
				}, 
				游戏方向.左上 => new Point[5]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X - 2, 锚点.Y)
				}, 
				游戏方向.左下 => new Point[5]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 2)
				}, 
				游戏方向.下方 => new Point[5]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1)
				}, 
				游戏方向.右方 => new Point[5]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1)
				}, 
				游戏方向.右上 => new Point[5]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 2)
				}, 
				_ => new Point[5]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y)
				}, 
			}, 
			技能范围类型.半月3x2 => 方向 switch
			{
				游戏方向.右方 => new Point[8]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1)
				}, 
				游戏方向.右上 => new Point[8]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1)
				}, 
				游戏方向.左下 => new Point[8]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1)
				}, 
				游戏方向.下方 => new Point[8]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y - 1)
				}, 
				游戏方向.左方 => new Point[8]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1)
				}, 
				游戏方向.上方 => new Point[8]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y + 1)
				}, 
				游戏方向.左上 => new Point[8]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1)
				}, 
				_ => new Point[8]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1)
				}, 
			}, 
			技能范围类型.半月3x3 => 方向 switch
			{
				游戏方向.左下 => new Point[12]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 2, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 2)
				}, 
				游戏方向.下方 => new Point[12]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X - 2, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y + 1)
				}, 
				游戏方向.右方 => new Point[12]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X + 1, 锚点.Y + 2),
					new Point(锚点.X + 1, 锚点.Y - 2)
				}, 
				游戏方向.右上 => new Point[12]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 2, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 2)
				}, 
				游戏方向.上方 => new Point[12]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X + 2, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y - 1)
				}, 
				游戏方向.左上 => new Point[12]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 2),
					new Point(锚点.X - 2, 锚点.Y + 1)
				}, 
				游戏方向.左方 => new Point[12]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X - 1, 锚点.Y - 2),
					new Point(锚点.X - 1, 锚点.Y + 2)
				}, 
				_ => new Point[12]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y - 1)
				}, 
			}, 
			技能范围类型.空心3x3 => new Point[8]
			{
				前方坐标(锚点, 游戏方向.上方, 1),
				前方坐标(锚点, 游戏方向.下方, 1),
				前方坐标(锚点, 游戏方向.左方, 1),
				前方坐标(锚点, 游戏方向.右方, 1),
				前方坐标(锚点, 游戏方向.左上, 1),
				前方坐标(锚点, 游戏方向.左下, 1),
				前方坐标(锚点, 游戏方向.右上, 1),
				前方坐标(锚点, 游戏方向.右下, 1)
			}, 
			技能范围类型.实心3x3 => new Point[9]
			{
				锚点,
				前方坐标(锚点, 游戏方向.上方, 1),
				前方坐标(锚点, 游戏方向.下方, 1),
				前方坐标(锚点, 游戏方向.左方, 1),
				前方坐标(锚点, 游戏方向.右方, 1),
				前方坐标(锚点, 游戏方向.左上, 1),
				前方坐标(锚点, 游戏方向.左下, 1),
				前方坐标(锚点, 游戏方向.右上, 1),
				前方坐标(锚点, 游戏方向.右下, 1)
			}, 
			技能范围类型.实心5x5 => new Point[25]
			{
				锚点,
				new Point(锚点.X + 1, 锚点.Y + 1),
				new Point(锚点.X, 锚点.Y + 1),
				new Point(锚点.X - 1, 锚点.Y + 1),
				new Point(锚点.X + 1, 锚点.Y),
				new Point(锚点.X - 1, 锚点.Y),
				new Point(锚点.X + 1, 锚点.Y - 1),
				new Point(锚点.X, 锚点.Y - 1),
				new Point(锚点.X - 1, 锚点.Y - 1),
				new Point(锚点.X + 2, 锚点.Y),
				new Point(锚点.X + 2, 锚点.Y + 1),
				new Point(锚点.X + 2, 锚点.Y + 2),
				new Point(锚点.X + 1, 锚点.Y + 2),
				new Point(锚点.X, 锚点.Y + 2),
				new Point(锚点.X - 1, 锚点.Y + 2),
				new Point(锚点.X - 2, 锚点.Y + 2),
				new Point(锚点.X - 2, 锚点.Y + 1),
				new Point(锚点.X - 2, 锚点.Y),
				new Point(锚点.X - 2, 锚点.Y - 1),
				new Point(锚点.X - 2, 锚点.Y - 2),
				new Point(锚点.X - 1, 锚点.Y - 2),
				new Point(锚点.X, 锚点.Y - 2),
				new Point(锚点.X + 1, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 1)
			}, 
			技能范围类型.斩月1x3 => new Point[3]
			{
				锚点,
				前方坐标(锚点, 方向, 1),
				前方坐标(锚点, 方向, 2)
			}, 
			技能范围类型.斩月3x3 => 方向 switch
			{
				游戏方向.左下 => new Point[9]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X + 2, 锚点.Y - 2),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 2),
					new Point(锚点.X + 2, 锚点.Y - 1)
				}, 
				游戏方向.下方 => new Point[9]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 2),
					new Point(锚点.X + 1, 锚点.Y - 2)
				}, 
				游戏方向.右方 => new Point[9]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y + 1),
					new Point(锚点.X - 2, 锚点.Y - 1)
				}, 
				游戏方向.右上 => new Point[9]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X - 2, 锚点.Y + 2),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 2),
					new Point(锚点.X - 2, 锚点.Y + 1)
				}, 
				游戏方向.左方 => new Point[9]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y - 1),
					new Point(锚点.X + 2, 锚点.Y + 1)
				}, 
				游戏方向.上方 => new Point[9]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 2),
					new Point(锚点.X - 1, 锚点.Y + 2)
				}, 
				游戏方向.左上 => new Point[9]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y + 2),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y + 1)
				}, 
				_ => new Point[9]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y - 2),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 2)
				}, 
			}, 
			技能范围类型.线型1x5 => new Point[5]
			{
				锚点,
				前方坐标(锚点, 方向, 1),
				前方坐标(锚点, 方向, 2),
				前方坐标(锚点, 方向, 3),
				前方坐标(锚点, 方向, 4)
			}, 
			技能范围类型.线型1x8 => new Point[8]
			{
				锚点,
				前方坐标(锚点, 方向, 1),
				前方坐标(锚点, 方向, 2),
				前方坐标(锚点, 方向, 3),
				前方坐标(锚点, 方向, 4),
				前方坐标(锚点, 方向, 5),
				前方坐标(锚点, 方向, 6),
				前方坐标(锚点, 方向, 7)
			}, 
			技能范围类型.线型3x8 => 方向 switch
			{
				游戏方向.上方 => new Point[24]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X + 1, 锚点.Y + 2),
					new Point(锚点.X - 1, 锚点.Y + 2),
					new Point(锚点.X, 锚点.Y + 3),
					new Point(锚点.X + 1, 锚点.Y + 3),
					new Point(锚点.X - 1, 锚点.Y + 3),
					new Point(锚点.X, 锚点.Y + 4),
					new Point(锚点.X + 1, 锚点.Y + 4),
					new Point(锚点.X - 1, 锚点.Y + 4),
					new Point(锚点.X, 锚点.Y + 5),
					new Point(锚点.X + 1, 锚点.Y + 5),
					new Point(锚点.X - 1, 锚点.Y + 5),
					new Point(锚点.X, 锚点.Y + 6),
					new Point(锚点.X + 1, 锚点.Y + 6),
					new Point(锚点.X - 1, 锚点.Y + 6),
					new Point(锚点.X, 锚点.Y + 7),
					new Point(锚点.X + 1, 锚点.Y + 7),
					new Point(锚点.X - 1, 锚点.Y + 7)
				}, 
				游戏方向.左上 => new Point[24]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 2),
					new Point(锚点.X + 3, 锚点.Y + 3),
					new Point(锚点.X + 3, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y + 3),
					new Point(锚点.X + 4, 锚点.Y + 4),
					new Point(锚点.X + 4, 锚点.Y + 3),
					new Point(锚点.X + 3, 锚点.Y + 4),
					new Point(锚点.X + 5, 锚点.Y + 5),
					new Point(锚点.X + 5, 锚点.Y + 4),
					new Point(锚点.X + 4, 锚点.Y + 5),
					new Point(锚点.X + 6, 锚点.Y + 6),
					new Point(锚点.X + 6, 锚点.Y + 5),
					new Point(锚点.X + 5, 锚点.Y + 6),
					new Point(锚点.X + 7, 锚点.Y + 7),
					new Point(锚点.X + 7, 锚点.Y + 6),
					new Point(锚点.X + 6, 锚点.Y + 7)
				}, 
				游戏方向.左方 => new Point[24]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X + 2, 锚点.Y - 1),
					new Point(锚点.X + 2, 锚点.Y + 1),
					new Point(锚点.X + 3, 锚点.Y),
					new Point(锚点.X + 3, 锚点.Y - 1),
					new Point(锚点.X + 3, 锚点.Y + 1),
					new Point(锚点.X + 4, 锚点.Y),
					new Point(锚点.X + 4, 锚点.Y - 1),
					new Point(锚点.X + 4, 锚点.Y + 1),
					new Point(锚点.X + 5, 锚点.Y),
					new Point(锚点.X + 5, 锚点.Y - 1),
					new Point(锚点.X + 5, 锚点.Y + 1),
					new Point(锚点.X + 6, 锚点.Y),
					new Point(锚点.X + 6, 锚点.Y - 1),
					new Point(锚点.X + 6, 锚点.Y + 1),
					new Point(锚点.X + 7, 锚点.Y),
					new Point(锚点.X + 7, 锚点.Y - 1),
					new Point(锚点.X + 7, 锚点.Y + 1)
				}, 
				游戏方向.右方 => new Point[24]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X - 2, 锚点.Y + 1),
					new Point(锚点.X - 2, 锚点.Y - 1),
					new Point(锚点.X - 3, 锚点.Y),
					new Point(锚点.X - 3, 锚点.Y + 1),
					new Point(锚点.X - 3, 锚点.Y - 1),
					new Point(锚点.X - 4, 锚点.Y),
					new Point(锚点.X - 4, 锚点.Y + 1),
					new Point(锚点.X - 4, 锚点.Y - 1),
					new Point(锚点.X - 5, 锚点.Y),
					new Point(锚点.X - 5, 锚点.Y + 1),
					new Point(锚点.X - 5, 锚点.Y - 1),
					new Point(锚点.X - 6, 锚点.Y),
					new Point(锚点.X - 6, 锚点.Y + 1),
					new Point(锚点.X - 6, 锚点.Y - 1),
					new Point(锚点.X - 7, 锚点.Y),
					new Point(锚点.X - 7, 锚点.Y + 1),
					new Point(锚点.X - 7, 锚点.Y - 1)
				}, 
				游戏方向.右上 => new Point[24]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 2, 锚点.Y + 2),
					new Point(锚点.X - 1, 锚点.Y + 2),
					new Point(锚点.X - 2, 锚点.Y + 1),
					new Point(锚点.X - 3, 锚点.Y + 3),
					new Point(锚点.X - 2, 锚点.Y + 3),
					new Point(锚点.X - 3, 锚点.Y + 2),
					new Point(锚点.X - 4, 锚点.Y + 4),
					new Point(锚点.X - 3, 锚点.Y + 4),
					new Point(锚点.X - 4, 锚点.Y + 3),
					new Point(锚点.X - 5, 锚点.Y + 5),
					new Point(锚点.X - 4, 锚点.Y + 5),
					new Point(锚点.X - 5, 锚点.Y + 4),
					new Point(锚点.X - 6, 锚点.Y + 6),
					new Point(锚点.X - 5, 锚点.Y + 6),
					new Point(锚点.X - 6, 锚点.Y + 5),
					new Point(锚点.X - 7, 锚点.Y + 7),
					new Point(锚点.X - 6, 锚点.Y + 7),
					new Point(锚点.X - 7, 锚点.Y + 6)
				}, 
				游戏方向.左下 => new Point[24]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 2, 锚点.Y - 2),
					new Point(锚点.X + 1, 锚点.Y - 2),
					new Point(锚点.X + 2, 锚点.Y - 1),
					new Point(锚点.X + 3, 锚点.Y - 3),
					new Point(锚点.X + 2, 锚点.Y - 3),
					new Point(锚点.X + 3, 锚点.Y - 2),
					new Point(锚点.X + 4, 锚点.Y - 4),
					new Point(锚点.X + 3, 锚点.Y - 4),
					new Point(锚点.X + 4, 锚点.Y - 3),
					new Point(锚点.X + 5, 锚点.Y - 5),
					new Point(锚点.X + 4, 锚点.Y - 5),
					new Point(锚点.X + 5, 锚点.Y - 4),
					new Point(锚点.X + 6, 锚点.Y - 6),
					new Point(锚点.X + 5, 锚点.Y - 6),
					new Point(锚点.X + 6, 锚点.Y - 5),
					new Point(锚点.X + 7, 锚点.Y - 7),
					new Point(锚点.X + 6, 锚点.Y - 7),
					new Point(锚点.X + 7, 锚点.Y - 6)
				}, 
				游戏方向.下方 => new Point[24]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X - 1, 锚点.Y - 2),
					new Point(锚点.X + 1, 锚点.Y - 2),
					new Point(锚点.X, 锚点.Y - 3),
					new Point(锚点.X - 1, 锚点.Y - 3),
					new Point(锚点.X + 1, 锚点.Y - 3),
					new Point(锚点.X, 锚点.Y - 4),
					new Point(锚点.X - 1, 锚点.Y - 4),
					new Point(锚点.X + 1, 锚点.Y - 4),
					new Point(锚点.X, 锚点.Y - 5),
					new Point(锚点.X - 1, 锚点.Y - 5),
					new Point(锚点.X + 1, 锚点.Y - 5),
					new Point(锚点.X, 锚点.Y - 6),
					new Point(锚点.X - 1, 锚点.Y - 6),
					new Point(锚点.X + 1, 锚点.Y - 6),
					new Point(锚点.X, 锚点.Y - 7),
					new Point(锚点.X - 1, 锚点.Y - 7),
					new Point(锚点.X + 1, 锚点.Y - 7)
				}, 
				_ => new Point[24]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y - 2),
					new Point(锚点.X - 2, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 2),
					new Point(锚点.X - 3, 锚点.Y - 3),
					new Point(锚点.X - 3, 锚点.Y - 2),
					new Point(锚点.X - 2, 锚点.Y - 3),
					new Point(锚点.X - 4, 锚点.Y - 4),
					new Point(锚点.X - 4, 锚点.Y - 3),
					new Point(锚点.X - 3, 锚点.Y - 4),
					new Point(锚点.X - 5, 锚点.Y - 5),
					new Point(锚点.X - 5, 锚点.Y - 4),
					new Point(锚点.X - 4, 锚点.Y - 5),
					new Point(锚点.X - 6, 锚点.Y - 6),
					new Point(锚点.X - 6, 锚点.Y - 5),
					new Point(锚点.X - 5, 锚点.Y - 6),
					new Point(锚点.X - 7, 锚点.Y - 7),
					new Point(锚点.X - 7, 锚点.Y - 6),
					new Point(锚点.X - 6, 锚点.Y - 7)
				}, 
			}, 
			技能范围类型.菱形3x3 => new Point[5]
			{
				锚点,
				new Point(锚点.X, 锚点.Y + 1),
				new Point(锚点.X, 锚点.Y - 1),
				new Point(锚点.X + 1, 锚点.Y),
				new Point(锚点.X - 1, 锚点.Y)
			}, 
			技能范围类型.线型3x7 => 方向 switch
			{
				游戏方向.右方 => new Point[21]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y),
					new Point(锚点.X - 2, 锚点.Y + 1),
					new Point(锚点.X - 2, 锚点.Y - 1),
					new Point(锚点.X - 3, 锚点.Y),
					new Point(锚点.X - 3, 锚点.Y + 1),
					new Point(锚点.X - 3, 锚点.Y - 1),
					new Point(锚点.X - 4, 锚点.Y),
					new Point(锚点.X - 4, 锚点.Y + 1),
					new Point(锚点.X - 4, 锚点.Y - 1),
					new Point(锚点.X - 5, 锚点.Y),
					new Point(锚点.X - 5, 锚点.Y + 1),
					new Point(锚点.X - 5, 锚点.Y - 1),
					new Point(锚点.X - 6, 锚点.Y),
					new Point(锚点.X - 6, 锚点.Y + 1),
					new Point(锚点.X - 6, 锚点.Y - 1)
				}, 
				游戏方向.右上 => new Point[21]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X - 2, 锚点.Y + 2),
					new Point(锚点.X - 1, 锚点.Y + 2),
					new Point(锚点.X - 2, 锚点.Y + 1),
					new Point(锚点.X - 3, 锚点.Y + 3),
					new Point(锚点.X - 2, 锚点.Y + 3),
					new Point(锚点.X - 3, 锚点.Y + 2),
					new Point(锚点.X - 4, 锚点.Y + 4),
					new Point(锚点.X - 3, 锚点.Y + 4),
					new Point(锚点.X - 4, 锚点.Y + 3),
					new Point(锚点.X - 5, 锚点.Y + 5),
					new Point(锚点.X - 4, 锚点.Y + 5),
					new Point(锚点.X - 5, 锚点.Y + 4),
					new Point(锚点.X - 6, 锚点.Y + 6),
					new Point(锚点.X - 5, 锚点.Y + 6),
					new Point(锚点.X - 6, 锚点.Y + 5)
				}, 
				游戏方向.左下 => new Point[21]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 2, 锚点.Y - 2),
					new Point(锚点.X + 1, 锚点.Y - 2),
					new Point(锚点.X + 2, 锚点.Y - 1),
					new Point(锚点.X + 3, 锚点.Y - 3),
					new Point(锚点.X + 2, 锚点.Y - 3),
					new Point(锚点.X + 3, 锚点.Y - 2),
					new Point(锚点.X + 4, 锚点.Y - 4),
					new Point(锚点.X + 3, 锚点.Y - 4),
					new Point(锚点.X + 4, 锚点.Y - 3),
					new Point(锚点.X + 5, 锚点.Y - 5),
					new Point(锚点.X + 4, 锚点.Y - 5),
					new Point(锚点.X + 5, 锚点.Y - 4),
					new Point(锚点.X + 6, 锚点.Y - 6),
					new Point(锚点.X + 5, 锚点.Y - 6),
					new Point(锚点.X + 6, 锚点.Y - 5)
				}, 
				游戏方向.下方 => new Point[21]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 2),
					new Point(锚点.X - 1, 锚点.Y - 2),
					new Point(锚点.X + 1, 锚点.Y - 2),
					new Point(锚点.X, 锚点.Y - 3),
					new Point(锚点.X - 1, 锚点.Y - 3),
					new Point(锚点.X + 1, 锚点.Y - 3),
					new Point(锚点.X, 锚点.Y - 4),
					new Point(锚点.X - 1, 锚点.Y - 4),
					new Point(锚点.X + 1, 锚点.Y - 4),
					new Point(锚点.X, 锚点.Y - 5),
					new Point(锚点.X - 1, 锚点.Y - 5),
					new Point(锚点.X + 1, 锚点.Y - 5),
					new Point(锚点.X, 锚点.Y - 6),
					new Point(锚点.X - 1, 锚点.Y - 6),
					new Point(锚点.X + 1, 锚点.Y - 6)
				}, 
				游戏方向.左方 => new Point[21]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y),
					new Point(锚点.X + 2, 锚点.Y - 1),
					new Point(锚点.X + 2, 锚点.Y + 1),
					new Point(锚点.X + 3, 锚点.Y),
					new Point(锚点.X + 3, 锚点.Y - 1),
					new Point(锚点.X + 3, 锚点.Y + 1),
					new Point(锚点.X + 4, 锚点.Y),
					new Point(锚点.X + 4, 锚点.Y - 1),
					new Point(锚点.X + 4, 锚点.Y + 1),
					new Point(锚点.X + 5, 锚点.Y),
					new Point(锚点.X + 5, 锚点.Y - 1),
					new Point(锚点.X + 5, 锚点.Y + 1),
					new Point(锚点.X + 6, 锚点.Y),
					new Point(锚点.X + 6, 锚点.Y - 1),
					new Point(锚点.X + 6, 锚点.Y + 1)
				}, 
				游戏方向.上方 => new Point[21]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 2),
					new Point(锚点.X + 1, 锚点.Y + 2),
					new Point(锚点.X - 1, 锚点.Y + 2),
					new Point(锚点.X, 锚点.Y + 3),
					new Point(锚点.X + 1, 锚点.Y + 3),
					new Point(锚点.X - 1, 锚点.Y + 3),
					new Point(锚点.X, 锚点.Y + 4),
					new Point(锚点.X + 1, 锚点.Y + 4),
					new Point(锚点.X - 1, 锚点.Y + 4),
					new Point(锚点.X, 锚点.Y + 5),
					new Point(锚点.X + 1, 锚点.Y + 5),
					new Point(锚点.X - 1, 锚点.Y + 5),
					new Point(锚点.X, 锚点.Y + 6),
					new Point(锚点.X + 1, 锚点.Y + 6),
					new Point(锚点.X - 1, 锚点.Y + 6)
				}, 
				游戏方向.左上 => new Point[21]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 2, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 2),
					new Point(锚点.X + 3, 锚点.Y + 3),
					new Point(锚点.X + 3, 锚点.Y + 2),
					new Point(锚点.X + 2, 锚点.Y + 3),
					new Point(锚点.X + 4, 锚点.Y + 4),
					new Point(锚点.X + 4, 锚点.Y + 3),
					new Point(锚点.X + 3, 锚点.Y + 4),
					new Point(锚点.X + 5, 锚点.Y + 5),
					new Point(锚点.X + 5, 锚点.Y + 4),
					new Point(锚点.X + 4, 锚点.Y + 5),
					new Point(锚点.X + 6, 锚点.Y + 6),
					new Point(锚点.X + 6, 锚点.Y + 5),
					new Point(锚点.X + 5, 锚点.Y + 6)
				}, 
				_ => new Point[21]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 2, 锚点.Y - 2),
					new Point(锚点.X - 2, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y - 2),
					new Point(锚点.X - 3, 锚点.Y - 3),
					new Point(锚点.X - 3, 锚点.Y - 2),
					new Point(锚点.X - 2, 锚点.Y - 3),
					new Point(锚点.X - 4, 锚点.Y - 4),
					new Point(锚点.X - 4, 锚点.Y - 3),
					new Point(锚点.X - 3, 锚点.Y - 4),
					new Point(锚点.X - 5, 锚点.Y - 5),
					new Point(锚点.X - 5, 锚点.Y - 4),
					new Point(锚点.X - 4, 锚点.Y - 5),
					new Point(锚点.X - 6, 锚点.Y - 6),
					new Point(锚点.X - 6, 锚点.Y - 5),
					new Point(锚点.X - 5, 锚点.Y - 6)
				}, 
			}, 
			技能范围类型.叉型3x3 => new Point[5]
			{
				锚点,
				new Point(锚点.X + 1, 锚点.Y + 1),
				new Point(锚点.X - 1, 锚点.Y + 1),
				new Point(锚点.X + 1, 锚点.Y - 1),
				new Point(锚点.X - 1, 锚点.Y - 1)
			}, 
			技能范围类型.空心5x5 => new Point[24]
			{
				new Point(锚点.X + 1, 锚点.Y + 1),
				new Point(锚点.X, 锚点.Y + 1),
				new Point(锚点.X - 1, 锚点.Y + 1),
				new Point(锚点.X + 1, 锚点.Y),
				new Point(锚点.X - 1, 锚点.Y),
				new Point(锚点.X + 1, 锚点.Y - 1),
				new Point(锚点.X, 锚点.Y - 1),
				new Point(锚点.X - 1, 锚点.Y - 1),
				new Point(锚点.X + 2, 锚点.Y),
				new Point(锚点.X + 2, 锚点.Y + 1),
				new Point(锚点.X + 2, 锚点.Y + 2),
				new Point(锚点.X + 1, 锚点.Y + 2),
				new Point(锚点.X, 锚点.Y + 2),
				new Point(锚点.X - 1, 锚点.Y + 2),
				new Point(锚点.X - 2, 锚点.Y + 2),
				new Point(锚点.X - 2, 锚点.Y + 1),
				new Point(锚点.X - 2, 锚点.Y),
				new Point(锚点.X - 2, 锚点.Y - 1),
				new Point(锚点.X - 2, 锚点.Y - 2),
				new Point(锚点.X - 1, 锚点.Y - 2),
				new Point(锚点.X, 锚点.Y - 2),
				new Point(锚点.X + 1, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 1)
			}, 
			技能范围类型.线型1x2 => new Point[2]
			{
				锚点,
				前方坐标(锚点, 方向, 1)
			}, 
			技能范围类型.前方3x1 => 方向 switch
			{
				游戏方向.右方 => new Point[3]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1)
				}, 
				游戏方向.右上 => new Point[3]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1)
				}, 
				游戏方向.左下 => new Point[3]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1)
				}, 
				游戏方向.下方 => new Point[3]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y)
				}, 
				游戏方向.上方 => new Point[3]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y)
				}, 
				游戏方向.左上 => new Point[3]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y)
				}, 
				游戏方向.左方 => new Point[3]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y + 1)
				}, 
				_ => new Point[3]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y)
				}, 
			}, 
			技能范围类型.螺旋7x7 => new Point[49]
			{
				锚点,
				new Point(锚点.X - 1, 锚点.Y),
				new Point(锚点.X - 1, 锚点.Y - 1),
				new Point(锚点.X, 锚点.Y - 1),
				new Point(锚点.X + 1, 锚点.Y - 1),
				new Point(锚点.X + 1, 锚点.Y),
				new Point(锚点.X + 1, 锚点.Y + 1),
				new Point(锚点.X, 锚点.Y + 1),
				new Point(锚点.X - 1, 锚点.Y + 1),
				new Point(锚点.X - 2, 锚点.Y + 1),
				new Point(锚点.X - 2, 锚点.Y),
				new Point(锚点.X - 2, 锚点.Y - 1),
				new Point(锚点.X - 2, 锚点.Y - 2),
				new Point(锚点.X - 1, 锚点.Y - 2),
				new Point(锚点.X, 锚点.Y - 2),
				new Point(锚点.X + 1, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 1),
				new Point(锚点.X + 2, 锚点.Y),
				new Point(锚点.X + 2, 锚点.Y + 1),
				new Point(锚点.X + 2, 锚点.Y + 2),
				new Point(锚点.X + 1, 锚点.Y + 2),
				new Point(锚点.X, 锚点.Y + 2),
				new Point(锚点.X - 1, 锚点.Y + 2),
				new Point(锚点.X - 2, 锚点.Y + 2),
				new Point(锚点.X - 3, 锚点.Y + 2),
				new Point(锚点.X - 3, 锚点.Y + 1),
				new Point(锚点.X - 3, 锚点.Y),
				new Point(锚点.X - 3, 锚点.Y - 1),
				new Point(锚点.X - 3, 锚点.Y - 2),
				new Point(锚点.X - 3, 锚点.Y - 3),
				new Point(锚点.X - 2, 锚点.Y - 3),
				new Point(锚点.X - 1, 锚点.Y - 3),
				new Point(锚点.X, 锚点.Y - 3),
				new Point(锚点.X + 1, 锚点.Y - 3),
				new Point(锚点.X + 2, 锚点.Y - 3),
				new Point(锚点.X + 3, 锚点.Y - 3),
				new Point(锚点.X + 3, 锚点.Y - 2),
				new Point(锚点.X + 3, 锚点.Y - 1),
				new Point(锚点.X + 3, 锚点.Y),
				new Point(锚点.X + 3, 锚点.Y + 1),
				new Point(锚点.X + 3, 锚点.Y + 2),
				new Point(锚点.X + 3, 锚点.Y + 3),
				new Point(锚点.X + 2, 锚点.Y + 3),
				new Point(锚点.X + 1, 锚点.Y + 3),
				new Point(锚点.X, 锚点.Y + 3),
				new Point(锚点.X - 1, 锚点.Y + 3),
				new Point(锚点.X - 2, 锚点.Y + 3),
				new Point(锚点.X - 3, 锚点.Y + 3)
			}, 
			技能范围类型.炎龙1x2 => 方向 switch
			{
				游戏方向.左下 => new Point[4]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y - 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y)
				}, 
				游戏方向.下方 => new Point[6]
				{
					锚点,
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y - 1)
				}, 
				游戏方向.右方 => new Point[6]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y - 1)
				}, 
				游戏方向.右上 => new Point[4]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y)
				}, 
				游戏方向.上方 => new Point[6]
				{
					锚点,
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X - 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y + 1)
				}, 
				游戏方向.左上 => new Point[4]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1)
				}, 
				游戏方向.左方 => new Point[6]
				{
					锚点,
					new Point(锚点.X + 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y + 1),
					new Point(锚点.X, 锚点.Y - 1),
					new Point(锚点.X + 1, 锚点.Y + 1),
					new Point(锚点.X + 1, 锚点.Y - 1)
				}, 
				_ => new Point[4]
				{
					锚点,
					new Point(锚点.X - 1, 锚点.Y - 1),
					new Point(锚点.X - 1, 锚点.Y),
					new Point(锚点.X, 锚点.Y - 1)
				}, 
			}, 
			技能范围类型.线型1x7 => new Point[7]
			{
				锚点,
				前方坐标(锚点, 方向, 1),
				前方坐标(锚点, 方向, 2),
				前方坐标(锚点, 方向, 3),
				前方坐标(锚点, 方向, 4),
				前方坐标(锚点, 方向, 5),
				前方坐标(锚点, 方向, 6)
			}, 
			技能范围类型.螺旋15x15 => new Point[288]
			{
				new Point(锚点.X - 1, 锚点.Y),
				new Point(锚点.X - 1, 锚点.Y - 1),
				new Point(锚点.X, 锚点.Y - 1),
				new Point(锚点.X + 1, 锚点.Y - 1),
				new Point(锚点.X + 1, 锚点.Y),
				new Point(锚点.X + 1, 锚点.Y + 1),
				new Point(锚点.X, 锚点.Y + 1),
				new Point(锚点.X - 1, 锚点.Y + 1),
				new Point(锚点.X - 2, 锚点.Y + 1),
				new Point(锚点.X - 2, 锚点.Y),
				new Point(锚点.X - 2, 锚点.Y - 1),
				new Point(锚点.X - 2, 锚点.Y - 2),
				new Point(锚点.X - 1, 锚点.Y - 2),
				new Point(锚点.X, 锚点.Y - 2),
				new Point(锚点.X + 1, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 2),
				new Point(锚点.X + 2, 锚点.Y - 1),
				new Point(锚点.X + 2, 锚点.Y),
				new Point(锚点.X + 2, 锚点.Y + 1),
				new Point(锚点.X + 2, 锚点.Y + 2),
				new Point(锚点.X + 1, 锚点.Y + 2),
				new Point(锚点.X, 锚点.Y + 2),
				new Point(锚点.X - 1, 锚点.Y + 2),
				new Point(锚点.X - 2, 锚点.Y + 2),
				new Point(锚点.X - 3, 锚点.Y + 2),
				new Point(锚点.X - 3, 锚点.Y + 1),
				new Point(锚点.X - 3, 锚点.Y),
				new Point(锚点.X - 3, 锚点.Y - 1),
				new Point(锚点.X - 3, 锚点.Y - 2),
				new Point(锚点.X - 3, 锚点.Y - 3),
				new Point(锚点.X - 2, 锚点.Y - 3),
				new Point(锚点.X - 1, 锚点.Y - 3),
				new Point(锚点.X, 锚点.Y - 3),
				new Point(锚点.X + 1, 锚点.Y - 3),
				new Point(锚点.X + 2, 锚点.Y - 3),
				new Point(锚点.X + 3, 锚点.Y - 3),
				new Point(锚点.X + 3, 锚点.Y - 2),
				new Point(锚点.X + 3, 锚点.Y - 1),
				new Point(锚点.X + 3, 锚点.Y),
				new Point(锚点.X + 3, 锚点.Y + 1),
				new Point(锚点.X + 3, 锚点.Y + 2),
				new Point(锚点.X + 3, 锚点.Y + 3),
				new Point(锚点.X + 2, 锚点.Y + 3),
				new Point(锚点.X + 1, 锚点.Y + 3),
				new Point(锚点.X, 锚点.Y + 3),
				new Point(锚点.X - 1, 锚点.Y + 3),
				new Point(锚点.X - 2, 锚点.Y + 3),
				new Point(锚点.X - 3, 锚点.Y + 3),
				new Point(锚点.X - 4, 锚点.Y + 3),
				new Point(锚点.X - 4, 锚点.Y + 2),
				new Point(锚点.X - 4, 锚点.Y + 1),
				new Point(锚点.X - 4, 锚点.Y),
				new Point(锚点.X - 4, 锚点.Y - 1),
				new Point(锚点.X - 4, 锚点.Y - 2),
				new Point(锚点.X - 4, 锚点.Y - 3),
				new Point(锚点.X - 4, 锚点.Y - 4),
				new Point(锚点.X - 3, 锚点.Y - 4),
				new Point(锚点.X - 2, 锚点.Y - 4),
				new Point(锚点.X - 1, 锚点.Y - 4),
				new Point(锚点.X, 锚点.Y - 4),
				new Point(锚点.X + 1, 锚点.Y - 4),
				new Point(锚点.X + 2, 锚点.Y - 4),
				new Point(锚点.X + 3, 锚点.Y - 4),
				new Point(锚点.X + 4, 锚点.Y - 4),
				new Point(锚点.X + 4, 锚点.Y - 3),
				new Point(锚点.X + 4, 锚点.Y - 2),
				new Point(锚点.X + 4, 锚点.Y - 1),
				new Point(锚点.X + 4, 锚点.Y),
				new Point(锚点.X + 4, 锚点.Y + 1),
				new Point(锚点.X + 4, 锚点.Y + 2),
				new Point(锚点.X + 4, 锚点.Y + 3),
				new Point(锚点.X + 4, 锚点.Y + 4),
				new Point(锚点.X + 3, 锚点.Y + 4),
				new Point(锚点.X + 2, 锚点.Y + 4),
				new Point(锚点.X + 1, 锚点.Y + 4),
				new Point(锚点.X, 锚点.Y + 4),
				new Point(锚点.X - 1, 锚点.Y + 4),
				new Point(锚点.X - 2, 锚点.Y + 4),
				new Point(锚点.X - 3, 锚点.Y + 4),
				new Point(锚点.X - 4, 锚点.Y + 4),
				new Point(锚点.X - 5, 锚点.Y + 4),
				new Point(锚点.X - 5, 锚点.Y + 3),
				new Point(锚点.X - 5, 锚点.Y + 2),
				new Point(锚点.X - 5, 锚点.Y + 1),
				new Point(锚点.X - 5, 锚点.Y),
				new Point(锚点.X - 5, 锚点.Y - 1),
				new Point(锚点.X - 5, 锚点.Y - 2),
				new Point(锚点.X - 5, 锚点.Y - 3),
				new Point(锚点.X - 5, 锚点.Y - 4),
				new Point(锚点.X - 5, 锚点.Y - 5),
				new Point(锚点.X - 4, 锚点.Y - 5),
				new Point(锚点.X - 3, 锚点.Y - 5),
				new Point(锚点.X - 2, 锚点.Y - 5),
				new Point(锚点.X - 1, 锚点.Y - 5),
				new Point(锚点.X, 锚点.Y - 5),
				new Point(锚点.X + 1, 锚点.Y - 5),
				new Point(锚点.X + 2, 锚点.Y - 5),
				new Point(锚点.X + 3, 锚点.Y - 5),
				new Point(锚点.X + 4, 锚点.Y - 5),
				new Point(锚点.X + 5, 锚点.Y - 5),
				new Point(锚点.X + 5, 锚点.Y - 4),
				new Point(锚点.X + 5, 锚点.Y - 3),
				new Point(锚点.X + 5, 锚点.Y - 2),
				new Point(锚点.X + 5, 锚点.Y - 1),
				new Point(锚点.X + 5, 锚点.Y),
				new Point(锚点.X + 5, 锚点.Y + 1),
				new Point(锚点.X + 5, 锚点.Y + 2),
				new Point(锚点.X + 5, 锚点.Y + 3),
				new Point(锚点.X + 5, 锚点.Y + 4),
				new Point(锚点.X + 5, 锚点.Y + 5),
				new Point(锚点.X + 4, 锚点.Y + 5),
				new Point(锚点.X + 3, 锚点.Y + 5),
				new Point(锚点.X + 2, 锚点.Y + 5),
				new Point(锚点.X + 1, 锚点.Y + 5),
				new Point(锚点.X, 锚点.Y + 5),
				new Point(锚点.X - 1, 锚点.Y + 5),
				new Point(锚点.X - 2, 锚点.Y + 5),
				new Point(锚点.X - 3, 锚点.Y + 5),
				new Point(锚点.X - 4, 锚点.Y + 5),
				new Point(锚点.X - 5, 锚点.Y + 5),
				new Point(锚点.X - 6, 锚点.Y + 5),
				new Point(锚点.X - 6, 锚点.Y + 4),
				new Point(锚点.X - 6, 锚点.Y + 3),
				new Point(锚点.X - 6, 锚点.Y + 2),
				new Point(锚点.X - 6, 锚点.Y + 1),
				new Point(锚点.X - 6, 锚点.Y),
				new Point(锚点.X - 6, 锚点.Y - 1),
				new Point(锚点.X - 6, 锚点.Y - 2),
				new Point(锚点.X - 6, 锚点.Y - 3),
				new Point(锚点.X - 6, 锚点.Y - 4),
				new Point(锚点.X - 6, 锚点.Y - 5),
				new Point(锚点.X - 6, 锚点.Y - 6),
				new Point(锚点.X - 5, 锚点.Y - 6),
				new Point(锚点.X - 4, 锚点.Y - 6),
				new Point(锚点.X - 3, 锚点.Y - 6),
				new Point(锚点.X - 2, 锚点.Y - 6),
				new Point(锚点.X - 1, 锚点.Y - 6),
				new Point(锚点.X, 锚点.Y - 6),
				new Point(锚点.X + 1, 锚点.Y - 6),
				new Point(锚点.X + 2, 锚点.Y - 6),
				new Point(锚点.X + 3, 锚点.Y - 6),
				new Point(锚点.X + 4, 锚点.Y - 6),
				new Point(锚点.X + 5, 锚点.Y - 6),
				new Point(锚点.X + 6, 锚点.Y - 6),
				new Point(锚点.X + 6, 锚点.Y - 5),
				new Point(锚点.X + 6, 锚点.Y - 4),
				new Point(锚点.X + 6, 锚点.Y - 3),
				new Point(锚点.X + 6, 锚点.Y - 2),
				new Point(锚点.X + 6, 锚点.Y - 1),
				new Point(锚点.X + 6, 锚点.Y),
				new Point(锚点.X + 6, 锚点.Y + 1),
				new Point(锚点.X + 6, 锚点.Y + 2),
				new Point(锚点.X + 6, 锚点.Y + 3),
				new Point(锚点.X + 6, 锚点.Y + 4),
				new Point(锚点.X + 6, 锚点.Y + 5),
				new Point(锚点.X + 6, 锚点.Y + 6),
				new Point(锚点.X + 5, 锚点.Y + 6),
				new Point(锚点.X + 4, 锚点.Y + 6),
				new Point(锚点.X + 3, 锚点.Y + 6),
				new Point(锚点.X + 2, 锚点.Y + 6),
				new Point(锚点.X + 1, 锚点.Y + 6),
				new Point(锚点.X, 锚点.Y + 6),
				new Point(锚点.X - 1, 锚点.Y + 6),
				new Point(锚点.X - 2, 锚点.Y + 6),
				new Point(锚点.X - 3, 锚点.Y + 6),
				new Point(锚点.X - 4, 锚点.Y + 6),
				new Point(锚点.X - 5, 锚点.Y + 6),
				new Point(锚点.X - 6, 锚点.Y + 6),
				new Point(锚点.X - 7, 锚点.Y + 6),
				new Point(锚点.X - 7, 锚点.Y + 5),
				new Point(锚点.X - 7, 锚点.Y + 4),
				new Point(锚点.X - 7, 锚点.Y + 3),
				new Point(锚点.X - 7, 锚点.Y + 2),
				new Point(锚点.X - 7, 锚点.Y + 1),
				new Point(锚点.X - 7, 锚点.Y),
				new Point(锚点.X - 7, 锚点.Y - 1),
				new Point(锚点.X - 7, 锚点.Y - 2),
				new Point(锚点.X - 7, 锚点.Y - 3),
				new Point(锚点.X - 7, 锚点.Y - 4),
				new Point(锚点.X - 7, 锚点.Y - 5),
				new Point(锚点.X - 7, 锚点.Y - 6),
				new Point(锚点.X - 7, 锚点.Y - 7),
				new Point(锚点.X - 6, 锚点.Y - 7),
				new Point(锚点.X - 5, 锚点.Y - 7),
				new Point(锚点.X - 4, 锚点.Y - 7),
				new Point(锚点.X - 3, 锚点.Y - 7),
				new Point(锚点.X - 2, 锚点.Y - 7),
				new Point(锚点.X - 1, 锚点.Y - 7),
				new Point(锚点.X, 锚点.Y - 7),
				new Point(锚点.X + 1, 锚点.Y - 7),
				new Point(锚点.X + 2, 锚点.Y - 7),
				new Point(锚点.X + 3, 锚点.Y - 7),
				new Point(锚点.X + 4, 锚点.Y - 7),
				new Point(锚点.X + 5, 锚点.Y - 7),
				new Point(锚点.X + 6, 锚点.Y - 7),
				new Point(锚点.X + 7, 锚点.Y - 7),
				new Point(锚点.X + 7, 锚点.Y - 6),
				new Point(锚点.X + 7, 锚点.Y - 5),
				new Point(锚点.X + 7, 锚点.Y - 4),
				new Point(锚点.X + 7, 锚点.Y - 3),
				new Point(锚点.X + 7, 锚点.Y - 2),
				new Point(锚点.X + 7, 锚点.Y - 1),
				new Point(锚点.X + 7, 锚点.Y),
				new Point(锚点.X + 7, 锚点.Y + 1),
				new Point(锚点.X + 7, 锚点.Y + 2),
				new Point(锚点.X + 7, 锚点.Y + 3),
				new Point(锚点.X + 7, 锚点.Y + 4),
				new Point(锚点.X + 7, 锚点.Y + 5),
				new Point(锚点.X + 7, 锚点.Y + 6),
				new Point(锚点.X + 7, 锚点.Y + 7),
				new Point(锚点.X + 6, 锚点.Y + 7),
				new Point(锚点.X + 5, 锚点.Y + 7),
				new Point(锚点.X + 4, 锚点.Y + 7),
				new Point(锚点.X + 3, 锚点.Y + 7),
				new Point(锚点.X + 2, 锚点.Y + 7),
				new Point(锚点.X + 1, 锚点.Y + 7),
				new Point(锚点.X, 锚点.Y + 7),
				new Point(锚点.X - 1, 锚点.Y + 7),
				new Point(锚点.X - 2, 锚点.Y + 7),
				new Point(锚点.X - 3, 锚点.Y + 7),
				new Point(锚点.X - 4, 锚点.Y + 7),
				new Point(锚点.X - 5, 锚点.Y + 7),
				new Point(锚点.X - 6, 锚点.Y + 7),
				new Point(锚点.X - 7, 锚点.Y + 7),
				new Point(锚点.X - 8, 锚点.Y + 7),
				new Point(锚点.X - 8, 锚点.Y + 6),
				new Point(锚点.X - 8, 锚点.Y + 5),
				new Point(锚点.X - 8, 锚点.Y + 4),
				new Point(锚点.X - 8, 锚点.Y + 3),
				new Point(锚点.X - 8, 锚点.Y + 2),
				new Point(锚点.X - 8, 锚点.Y + 1),
				new Point(锚点.X - 8, 锚点.Y),
				new Point(锚点.X - 8, 锚点.Y - 1),
				new Point(锚点.X - 8, 锚点.Y - 2),
				new Point(锚点.X - 8, 锚点.Y - 3),
				new Point(锚点.X - 8, 锚点.Y - 4),
				new Point(锚点.X - 8, 锚点.Y - 5),
				new Point(锚点.X - 8, 锚点.Y - 6),
				new Point(锚点.X - 8, 锚点.Y - 7),
				new Point(锚点.X - 8, 锚点.Y - 8),
				new Point(锚点.X - 7, 锚点.Y - 8),
				new Point(锚点.X - 6, 锚点.Y - 8),
				new Point(锚点.X - 5, 锚点.Y - 8),
				new Point(锚点.X - 4, 锚点.Y - 8),
				new Point(锚点.X - 3, 锚点.Y - 8),
				new Point(锚点.X - 2, 锚点.Y - 8),
				new Point(锚点.X - 1, 锚点.Y - 8),
				new Point(锚点.X, 锚点.Y - 8),
				new Point(锚点.X + 1, 锚点.Y - 8),
				new Point(锚点.X + 2, 锚点.Y - 8),
				new Point(锚点.X + 3, 锚点.Y - 8),
				new Point(锚点.X + 4, 锚点.Y - 8),
				new Point(锚点.X + 5, 锚点.Y - 8),
				new Point(锚点.X + 6, 锚点.Y - 8),
				new Point(锚点.X + 7, 锚点.Y - 8),
				new Point(锚点.X + 8, 锚点.Y - 8),
				new Point(锚点.X + 8, 锚点.Y - 7),
				new Point(锚点.X + 8, 锚点.Y - 6),
				new Point(锚点.X + 8, 锚点.Y - 5),
				new Point(锚点.X + 8, 锚点.Y - 4),
				new Point(锚点.X + 8, 锚点.Y - 3),
				new Point(锚点.X + 8, 锚点.Y - 2),
				new Point(锚点.X + 8, 锚点.Y - 1),
				new Point(锚点.X + 8, 锚点.Y),
				new Point(锚点.X + 8, 锚点.Y + 1),
				new Point(锚点.X + 8, 锚点.Y + 2),
				new Point(锚点.X + 8, 锚点.Y + 3),
				new Point(锚点.X + 8, 锚点.Y + 4),
				new Point(锚点.X + 8, 锚点.Y + 5),
				new Point(锚点.X + 8, 锚点.Y + 6),
				new Point(锚点.X + 8, 锚点.Y + 7),
				new Point(锚点.X + 8, 锚点.Y + 8),
				new Point(锚点.X + 7, 锚点.Y + 8),
				new Point(锚点.X + 6, 锚点.Y + 8),
				new Point(锚点.X + 5, 锚点.Y + 8),
				new Point(锚点.X + 4, 锚点.Y + 8),
				new Point(锚点.X + 3, 锚点.Y + 8),
				new Point(锚点.X + 2, 锚点.Y + 8),
				new Point(锚点.X + 1, 锚点.Y + 8),
				new Point(锚点.X, 锚点.Y + 8),
				new Point(锚点.X - 1, 锚点.Y + 8),
				new Point(锚点.X - 2, 锚点.Y + 8),
				new Point(锚点.X - 3, 锚点.Y + 8),
				new Point(锚点.X - 4, 锚点.Y + 8),
				new Point(锚点.X - 5, 锚点.Y + 8),
				new Point(锚点.X - 6, 锚点.Y + 8),
				new Point(锚点.X - 7, 锚点.Y + 8),
				new Point(锚点.X - 8, 锚点.Y + 8)
			}, 
			技能范围类型.线型1x6 => new Point[6]
			{
				锚点,
				前方坐标(锚点, 方向, 1),
				前方坐标(锚点, 方向, 2),
				前方坐标(锚点, 方向, 3),
				前方坐标(锚点, 方向, 4),
				前方坐标(锚点, 方向, 5)
			}, 
			_ => new Point[0], 
		};
	}

	static 计算类()
	{
		系统相对时间 = Convert.ToDateTime("1970-01-01 08:00:00");
	}
}
