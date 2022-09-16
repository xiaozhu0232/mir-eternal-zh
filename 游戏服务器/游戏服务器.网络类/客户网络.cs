using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using 游戏服务器.地图类;
using 游戏服务器.数据类;

namespace 游戏服务器.网络类;

public sealed class 客户网络
{
	private DateTime 断开时间;

	private bool 正在发送;

	private byte[] 剩余数据;

	private readonly EventHandler<Exception> 断网事件;

	private ConcurrentQueue<游戏封包> 接收列表;

	private ConcurrentQueue<游戏封包> 发送列表;

	public bool 正在断开;

	public readonly DateTime 接入时间;

	public readonly TcpClient 当前连接;

	public 游戏阶段 当前阶段;

	public 账号数据 绑定账号;

	public 玩家实例 绑定角色;

	public string 网络地址;

	public string 物理地址;

	public int 发送总数;

	public int 接收总数;

	public 客户网络(TcpClient 客户端)
	{
		剩余数据 = new byte[0];
		接收列表 = new ConcurrentQueue<游戏封包>();
		发送列表 = new ConcurrentQueue<游戏封包>();
		当前连接 = 客户端;
		当前连接.NoDelay = true;
		接入时间 = 主程.当前时间;
		断开时间 = 主程.当前时间.AddMinutes((int)自定义类.掉线判定时间);
		断网事件 = (EventHandler<Exception>)Delegate.Combine(断网事件, new EventHandler<Exception>(网络服务网关.断网回调));
		网络地址 = 当前连接.Client.RemoteEndPoint.ToString().Split(':')[0];
		开始异步接收();
	}

	public void 处理数据()
	{
		try
		{
			if (!正在断开 && !网络服务网关.网络服务停止)
			{
				if (!(主程.当前时间 > 断开时间))
				{
					处理已收封包();
					发送全部封包();
				}
				else
				{
					尝试断开连接(new Exception("网络长时间无回应, 断开连接."));
				}
				return;
			}
			if (正在发送 || 接收列表.Count != 0 || 发送列表.Count != 0)
			{
				处理已收封包();
				发送全部封包();
				return;
			}
			绑定角色?.玩家角色下线();
			绑定账号?.账号下线();
			网络服务网关.移除网络(this);
			当前连接.Client.Shutdown(SocketShutdown.Both);
			当前连接.Close();
			接收列表 = null;
			发送列表 = null;
			当前阶段 = 游戏阶段.正在登录;
		}
		catch (Exception ex)
		{
			string[] array;
			object obj;
			if (绑定角色 != null)
			{
				array = new string[10] { "处理网络数据时出现异常, 已断开对应连接\r\n账号:[", null, null, null, null, null, null, null, null, null };
				账号数据 账号数据 = 绑定账号;
				if (账号数据 == null)
				{
					obj = null;
				}
				else
				{
					obj = 账号数据.账号名字.V;
					if (obj != null)
					{
						goto IL_0117;
					}
				}
				obj = "无";
				goto IL_0117;
			}
			goto IL_0193;
			IL_0117:
			array[1] = (string)obj;
			array[2] = "]\r\n角色:[";
			玩家实例 玩家实例 = 绑定角色;
			object obj2;
			if (玩家实例 != null)
			{
				obj2 = 玩家实例.对象名字;
				if (obj2 != null)
				{
					goto IL_014a;
				}
			}
			else
			{
				obj2 = null;
			}
			obj2 = "无";
			goto IL_014a;
			IL_014a:
			array[3] = (string)obj2;
			array[4] = "]\r\n网络地址:[";
			array[5] = 网络地址;
			array[6] = "]\r\n物理地址:[";
			array[7] = 物理地址;
			array[8] = "]\r\n错误提示:";
			array[9] = ex.Message;
			主程.添加系统日志(string.Concat(array));
			goto IL_0193;
			IL_0193:
			绑定角色?.玩家角色下线();
			绑定账号?.账号下线();
			网络服务网关.移除网络(this);
			当前连接.Client?.Shutdown(SocketShutdown.Both);
			当前连接?.Close();
			接收列表 = null;
			发送列表 = null;
			当前阶段 = 游戏阶段.正在登录;
		}
	}

	public void 发送封包(游戏封包 封包)
	{
		if (!正在断开 && !网络服务网关.网络服务停止 && 封包 != null)
		{
			发送列表.Enqueue(封包);
		}
	}

	public void 尝试断开连接(Exception e)
	{
		if (!正在断开)
		{
			正在断开 = true;
			断网事件?.Invoke(this, e);
		}
	}

	private void 处理已收封包()
	{
		游戏封包 result;
		while (true)
		{
			if (接收列表.IsEmpty)
			{
				return;
			}
			if (接收列表.Count <= 自定义类.封包限定数量)
			{
				if (接收列表.TryDequeue(out result))
				{
					if (!游戏封包.封包处理方法表.TryGetValue(result.封包类型, out var value))
					{
						break;
					}
					value.Invoke(this, new object[1] { result });
				}
				continue;
			}
			接收列表 = new ConcurrentQueue<游戏封包>();
			网络服务网关.屏蔽网络(网络地址);
			尝试断开连接(new Exception("封包过多, 断开连接并限制登录."));
			return;
		}
		尝试断开连接(new Exception("没有找到封包处理方法, 断开连接. 封包类型: " + result.封包类型.FullName));
	}

	private void 发送全部封包()
	{
		List<byte> list = new List<byte>();
		while (!发送列表.IsEmpty)
		{
			if (发送列表.TryDequeue(out var result))
			{
				list.AddRange(result.取字节());
			}
		}
		if (list.Count != 0)
		{
			开始异步发送(list);
		}
	}

	private void 延迟掉线时间()
	{
		断开时间 = 主程.当前时间.AddMinutes((int)自定义类.掉线判定时间);
	}

	private void 开始异步接收()
	{
		try
		{
			if (!正在断开 && !网络服务网关.网络服务停止)
			{
				byte[] array = new byte[8192];
				当前连接.Client.BeginReceive(array, 0, array.Length, SocketFlags.None, 接收完成回调, array);
			}
		}
		catch (Exception ex)
		{
			尝试断开连接(new Exception("异步接收错误 : " + ex.Message));
		}
	}

	private void 接收完成回调(IAsyncResult 异步参数)
	{
		try
		{
			if (正在断开 || 网络服务网关.网络服务停止 || 当前连接.Client == null)
			{
				return;
			}
			int num = 当前连接.Client?.EndReceive(异步参数) ?? 0;
			if (num <= 0)
			{
				尝试断开连接(new Exception("客户端断开连接."));
				return;
			}
			接收总数 += num;
			网络服务网关.已接收字节数 += num;
			byte[] src = 异步参数.AsyncState as byte[];
			byte[] dst = new byte[剩余数据.Length + num];
			Buffer.BlockCopy(剩余数据, 0, dst, 0, 剩余数据.Length);
			Buffer.BlockCopy(src, 0, dst, 剩余数据.Length, num);
			剩余数据 = dst;
			while (true)
			{
				游戏封包 游戏封包2 = 游戏封包.取封包(this, 剩余数据, out 剩余数据);
				if (游戏封包2 == null)
				{
					break;
				}
				接收列表.Enqueue(游戏封包2);
			}
			延迟掉线时间();
			开始异步接收();
		}
		catch (Exception ex)
		{
			尝试断开连接(new Exception("封包构建错误, 错误提示: " + ex.Message));
		}
	}

	private void 开始异步发送(List<byte> 数据)
	{
		try
		{
			正在发送 = true;
			当前连接.Client.BeginSend(数据.ToArray(), 0, 数据.Count, SocketFlags.None, 发送完成回调, null);
		}
		catch (Exception ex)
		{
			正在发送 = false;
			发送列表 = new ConcurrentQueue<游戏封包>();
			尝试断开连接(new Exception("异步发送错误 : " + ex.Message));
		}
	}

	private void 发送完成回调(IAsyncResult 异步参数)
	{
		try
		{
			int num = 当前连接.Client.EndSend(异步参数);
			发送总数 += num;
			网络服务网关.已发送字节数 += num;
			if (num == 0)
			{
				发送列表 = new ConcurrentQueue<游戏封包>();
				尝试断开连接(new Exception("发送回调错误!"));
			}
			正在发送 = false;
		}
		catch (Exception ex)
		{
			正在发送 = false;
			发送列表 = new ConcurrentQueue<游戏封包>();
			尝试断开连接(new Exception("发送回调错误 : " + ex.Message));
		}
	}

	public void 处理封包(预留封包零一 P)
	{
	}

	public void 处理封包(预留封包零二 P)
	{
	}

	public void 处理封包(预留封包零三 P)
	{
	}

	public void 处理封包(上传游戏设置 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家更改设置(P.字节描述);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户碰触法阵 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户进入法阵 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家进入法阵(P.法阵编号);
		}
	}

	public void 处理封包(点击Npcc对话 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(请求对象数据 P)
	{
		if (当前阶段 == 游戏阶段.场景加载 || 当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.请求对象外观(P.对象编号, P.状态编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户网速测试 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			发送封包(new 网速测试应答
			{
				当前时间 = P.客户时间
			});
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(测试网关网速 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
			return;
		}
		发送封包(new 登陆查询应答
		{
			当前时间 = P.客户时间
		});
	}

	public void 处理封包(客户请求复活 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家请求复活();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(切换攻击模式 P)
	{
		攻击模式 result;
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else if (!Enum.IsDefined(typeof(攻击模式), (int)P.攻击模式) || !Enum.TryParse<攻击模式>(P.攻击模式.ToString(), out result))
		{
			尝试断开连接(new Exception("更改攻击模式时提供错误的枚举参数.即将断开连接."));
		}
		else
		{
			绑定角色.更改攻击模式(result);
		}
	}

	public void 处理封包(更改宠物模式 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			if (!Enum.IsDefined(typeof(宠物模式), (int)P.宠物模式) || !Enum.TryParse<宠物模式>(P.宠物模式.ToString(), out var result))
			{
				尝试断开连接(new Exception($"更改宠物模式时提供错误的枚举参数.即将断开连接. 参数 - {P.宠物模式}"));
			}
			else
			{
				绑定角色.更改宠物模式(result);
			}
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(上传角色位置 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家同步位置();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户角色转动 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			if (Enum.IsDefined(typeof(游戏方向), (int)P.转动方向) && Enum.TryParse<游戏方向>(P.转动方向.ToString(), out var result))
			{
				绑定角色.玩家角色转动(result);
			}
			else
			{
				尝试断开连接(new Exception("玩家角色转动时提供错误的枚举参数.即将断开连接."));
			}
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户角色走动 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家角色走动(P.坐标);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户角色跑动 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家角色跑动(P.坐标);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色开关技能 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家开关技能(P.技能编号);
		}
	}

	public void 处理封包(角色装备技能 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else if (P.技能栏位 < 32)
		{
			绑定角色.玩家拖动技能(P.技能栏位, P.技能编号);
		}
		else
		{
			尝试断开连接(new Exception("玩家装配技能时提供错误的封包参数.即将断开连接."));
		}
	}

	public void 处理封包(角色释放技能 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家释放技能(P.技能编号, P.动作编号, P.目标编号, P.锚点坐标);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(战斗姿态切换 P)
	{
		if (当前阶段 == 游戏阶段.场景加载 || 当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家切换姿态();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户更换角色 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定账号.更换角色(this);
			当前阶段 = 游戏阶段.选择角色;
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(场景加载完成 P)
	{
		if (当前阶段 == 游戏阶段.场景加载 || 当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家进入场景();
			当前阶段 = 游戏阶段.正在游戏;
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(退出当前副本 P)
	{
		if (当前阶段 == 游戏阶段.场景加载 || 当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家退出副本();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家退出登录 P)
	{
		if (当前阶段 == 游戏阶段.正在登录)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定账号.返回登录(this);
		}
	}

	public void 处理封包(打开角色背包 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色拾取物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色丢弃物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家丢弃物品(P.背包类型, P.物品位置, P.丢弃数量);
		}
	}

	public void 处理封包(角色转移物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家转移物品(P.当前背包, P.原有位置, P.目标背包, P.目标位置);
		}
	}

	public void 处理封包(角色使用物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家使用物品(P.背包类型, P.物品位置);
		}
	}

	public void 处理封包(玩家喝修复油 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家喝修复油(P.背包类型, P.物品位置);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家扩展背包 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家扩展背包(P.背包类型, P.扩展大小);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(请求商店数据 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.请求商店数据(P.版本编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色购买物品 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家购买物品(P.商店编号, P.物品位置, P.购入数量);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色卖出物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家出售物品(P.背包类型, P.物品位置, P.卖出数量);
		}
	}

	public void 处理封包(查询回购列表 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.请求回购清单();
		}
	}

	public void 处理封包(角色回购物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else if (P.物品位置 >= 100)
		{
			尝试断开连接(new Exception("玩家回购物品时提供错误的位置参数.即将断开连接."));
		}
		else
		{
			绑定角色.玩家回购物品(P.物品位置);
		}
	}

	public void 处理封包(商店修理单件 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.商店修理单件(P.背包类型, P.物品位置);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(商店修理全部 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.商店修理全部();
		}
	}

	public void 处理封包(商店特修单件 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.商店特修单件(P.物品容器, P.物品位置);
		}
	}

	public void 处理封包(随身修理单件 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.随身修理单件(P.物品容器, P.物品位置, P.物品编号);
		}
	}

	public void 处理封包(随身特修全部 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.随身修理全部();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色整理背包 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家整理背包(P.背包类型);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色拆分物品 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家拆分物品(P.当前背包, P.物品位置, P.拆分数量, P.目标背包, P.目标位置);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色分解物品 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			if (!Enum.TryParse<物品背包分类>(P.背包类型.ToString(), out var result) || !Enum.IsDefined(typeof(物品背包分类), result))
			{
				尝试断开连接(new Exception("玩家分解物品时提供错误的枚举参数.即将断开连接."));
			}
			else
			{
				绑定角色.玩家分解物品(P.背包类型, P.物品位置, P.分解数量);
			}
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(角色合成物品 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家合成物品();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家镶嵌灵石 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家镶嵌灵石(P.装备类型, P.装备位置, P.装备孔位, P.灵石类型, P.灵石位置);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家拆除灵石 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家拆除灵石(P.装备类型, P.装备位置, P.装备孔位);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(普通铭文洗练 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.普通铭文洗练(P.装备类型, P.装备位置, P.物品编号);
		}
	}

	public void 处理封包(高级铭文洗练 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.高级铭文洗练(P.装备类型, P.装备位置, P.物品编号);
		}
	}

	public void 处理封包(替换铭文洗练 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.替换铭文洗练(P.装备类型, P.装备位置, P.物品编号);
		}
	}

	public void 处理封包(替换高级铭文 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.高级洗练确认(P.装备类型, P.装备位置);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(替换低级铭文 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.替换洗练确认(P.装备类型, P.装备位置);
		}
	}

	public void 处理封包(放弃铭文替换 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.放弃替换铭文();
		}
	}

	public void 处理封包(解锁双铭文位 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.解锁双铭文位(P.装备类型, P.装备位置, P.操作参数);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(切换双铭文位 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.切换双铭文位(P.装备类型, P.装备位置, P.操作参数);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(传承武器铭文 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.传承武器铭文(P.来源类型, P.来源位置, P.目标类型, P.目标位置);
		}
	}

	public void 处理封包(升级武器普通 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.升级武器普通(P.首饰组, P.材料组);
		}
	}

	public void 处理封包(角色选中目标 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家选中对象(P.对象编号);
		}
	}

	public void 处理封包(开始Npcc对话 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.开始Npcc对话(P.对象编号);
		}
	}

	public void 处理封包(继续Npcc对话 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.继续Npcc对话(P.对话编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查看玩家装备 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查看对象装备(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(请求龙卫数据 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(请求魂石数据 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询奖励找回 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(同步角色战力 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.查询玩家战力(P.对象编号);
		}
	}

	public void 处理封包(查询问卷调查 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家申请交易 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家申请交易(P.对象编号);
		}
	}

	public void 处理封包(玩家同意交易 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家同意交易(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家结束交易 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家结束交易();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家放入金币 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家放入金币(P.金币数量);
		}
	}

	public void 处理封包(玩家放入物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家放入物品(P.放入位置, P.放入物品, P.物品容器, P.物品位置);
		}
	}

	public void 处理封包(玩家装备锁定 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			主程.添加系统日志("玩家点击装备锁|" + 绑定角色.角色数据.角色名字);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家仓库锁定 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			主程.添加系统日志("玩家点击仓库锁|" + 绑定角色.角色数据.角色名字);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家锁定交易 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家锁定交易();
		}
	}

	public void 处理封包(玩家解锁交易 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家解锁交易();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家确认交易 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家确认交易();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家准备摆摊 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家准备摆摊();
		}
	}

	public void 处理封包(玩家重整摊位 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家重整摊位();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家开始摆摊 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家开始摆摊();
		}
	}

	public void 处理封包(玩家收起摊位 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家收起摊位();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(放入摊位物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.放入摊位物品(P.放入位置, P.物品容器, P.物品位置, P.物品数量, P.物品价格);
		}
	}

	public void 处理封包(取回摊位物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.取回摊位物品(P.取回位置);
		}
	}

	public void 处理封包(更改摊位名字 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.更改摊位名字(P.摊位名字);
		}
	}

	public void 处理封包(更改摊位外观 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.升级摊位外观(P.外观编号);
		}
	}

	public void 处理封包(打开角色摊位 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家打开摊位(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(购买摊位物品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.购买摊位物品(P.对象编号, P.物品位置, P.购买数量);
		}
	}

	public void 处理封包(添加好友关注 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家添加关注(P.对象编号, P.对象名字);
		}
	}

	public void 处理封包(取消好友关注 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家取消关注(P.对象编号);
		}
	}

	public void 处理封包(新建好友分组 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(移动好友分组 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(发送好友聊天 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			if (P.字节数据.Length >= 7)
			{
				if (P.字节数据.Last() == 0)
				{
					绑定角色.玩家好友聊天(P.字节数据);
				}
				else
				{
					尝试断开连接(new Exception($"数据错误,断开连接.  处理封包: {P.GetType()},  无结束符."));
				}
			}
			else
			{
				尝试断开连接(new Exception($"数据太短,断开连接.  处理封包: {P.GetType()},  数据长度:{P.字节数据.Length}"));
			}
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家添加仇人 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家添加仇人(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家删除仇人 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家删除仇人(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家屏蔽对象 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家屏蔽目标(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家解除屏蔽 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家解除屏蔽(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家比较成就 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(发送聊天信息 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			if (P.字节数据.Length >= 7)
			{
				if (P.字节数据.Last() != 0)
				{
					尝试断开连接(new Exception($"数据错误,断开连接.  处理封包: {P.GetType()},  无结束符."));
				}
				else
				{
					绑定角色.玩家发送广播(P.字节数据);
				}
			}
			else
			{
				尝试断开连接(new Exception($"数据太短,断开连接.  处理封包: {P.GetType()},  数据长度:{P.字节数据.Length}"));
			}
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(发送社交消息 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			if (P.字节数据.Length < 6)
			{
				尝试断开连接(new Exception($"数据太短,断开连接.  处理封包: {P.GetType()},  数据长度:{P.字节数据.Length}"));
			}
			else if (P.字节数据.Last() != 0)
			{
				尝试断开连接(new Exception($"数据错误,断开连接.  处理封包: {P.GetType()},  无结束符."));
			}
			else
			{
				绑定角色.玩家发送消息(P.字节数据);
			}
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(请求角色数据 P)
	{
		if (当前阶段 == 游戏阶段.场景加载 || 当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.请求角色资料(P.角色编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(上传社交信息 P)
	{
		if (当前阶段 != 游戏阶段.场景加载 && 当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询附近队伍 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询附近队伍();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询队伍信息 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询队伍信息(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(申请创建队伍 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.申请创建队伍(P.对象编号, P.分配方式);
		}
	}

	public void 处理封包(发送组队请求 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.发送组队请求(P.对象编号);
		}
	}

	public void 处理封包(申请离开队伍 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.申请队员离队(P.对象编号);
		}
	}

	public void 处理封包(申请更改队伍 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.申请移交队长(P.队长编号);
		}
	}

	public void 处理封包(回应组队请求 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.回应组队请求(P.对象编号, P.组队方式, P.回应方式);
		}
	}

	public void 处理封包(玩家装配称号 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家使用称号(P.称号编号);
		}
	}

	public void 处理封包(玩家卸下称号 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家卸下称号();
		}
	}

	public void 处理封包(申请发送邮件 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.申请发送邮件(P.字节数据);
		}
	}

	public void 处理封包(查询邮箱内容 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询邮箱内容();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查看邮件内容 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.查看邮件内容(P.邮件编号);
		}
	}

	public void 处理封包(删除指定邮件 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.删除指定邮件(P.邮件编号);
		}
	}

	public void 处理封包(提取邮件附件 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.提取邮件附件(P.邮件编号);
		}
	}

	public void 处理封包(查询行会名字 P)
	{
		if (当前阶段 != 游戏阶段.场景加载 && 当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.查询行会信息(P.行会编号);
		}
	}

	public void 处理封包(更多行会信息 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.更多行会信息();
		}
	}

	public void 处理封包(查看行会列表 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查看行会列表(P.行会编号, P.查看方式);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查找对应行会 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查找对应行会(P.行会编号, P.行会名字);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(申请加入行会 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.申请加入行会(P.行会编号, P.行会名字);
		}
	}

	public void 处理封包(查看申请列表 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.查看申请列表();
		}
	}

	public void 处理封包(处理入会申请 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.处理入会申请(P.对象编号, P.处理类型);
		}
	}

	public void 处理封包(处理入会邀请 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.处理入会邀请(P.对象编号, P.处理类型);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(邀请加入行会 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.邀请加入行会(P.对象名字);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(申请创建行会 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.申请创建行会(P.字节数据);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(申请解散行会 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.申请解散行会();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(捐献行会资金 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.捐献行会资金(P.金币数量);
		}
	}

	public void 处理封包(发放行会福利 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.发放行会福利();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(申请离开行会 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.申请离开行会();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(更改行会公告 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.更改行会公告(P.行会公告);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(更改行会宣言 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.更改行会宣言(P.行会宣言);
		}
	}

	public void 处理封包(设置行会禁言 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.设置行会禁言(P.对象编号, P.禁言状态);
		}
	}

	public void 处理封包(变更会员职位 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.变更会员职位(P.对象编号, P.对象职位);
		}
	}

	public void 处理封包(逐出行会成员 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.逐出行会成员(P.对象编号);
		}
	}

	public void 处理封包(转移会长职位 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.转移会长职位(P.对象编号);
		}
	}

	public void 处理封包(申请行会外交 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.申请行会外交(P.外交类型, P.外交时间, P.行会名字);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(申请行会敌对 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.申请行会敌对(P.敌对时间, P.行会名字);
		}
	}

	public void 处理封包(处理结盟申请 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.处理结盟申请(P.处理类型, P.行会编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(申请解除结盟 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.申请解除结盟(P.行会编号);
		}
	}

	public void 处理封包(申请解除敌对 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.申请解除敌对(P.行会编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(处理解敌申请 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.处理解除申请(P.行会编号, P.回应类型);
		}
	}

	public void 处理封包(更改存储权限 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查看结盟申请 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.查看结盟申请();
		}
	}

	public void 处理封包(更多行会事记 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.更多行会事记();
		}
	}

	public void 处理封包(查询行会成就 P)
	{
		if (当前阶段 != 游戏阶段.场景加载 && 当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(开启行会活动 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(发布通缉榜单 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(同步通缉榜单 P)
	{
		if (当前阶段 != 游戏阶段.场景加载 && 当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(发起行会战争 P)
	{
		if (当前阶段 != 游戏阶段.场景加载 && 当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询地图路线 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询地图路线();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(切换地图路线 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.切换地图路线();
		}
	}

	public void 处理封包(跳过剧情动画 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(更改收徒推送 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.更改收徒推送(P.收徒推送);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询师门成员 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询师门成员();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询师门奖励 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询师门奖励();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询拜师名册 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询拜师名册();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询收徒名册 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询收徒名册();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(祝贺徒弟升级 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家申请拜师 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.玩家申请拜师(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(同意拜师申请 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.同意拜师申请(P.对象编号);
		}
	}

	public void 处理封包(拒绝拜师申请 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.拒绝拜师申请(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(玩家申请收徒 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.玩家申请收徒(P.对象编号);
		}
	}

	public void 处理封包(同意收徒申请 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.同意收徒申请(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(拒绝收徒申请 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.拒绝收徒申请(P.对象编号);
		}
	}

	public void 处理封包(逐出师门申请 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.逐出师门申请(P.对象编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(离开师门申请 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.离开师门申请();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(提交出师申请 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.提交出师申请();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询排名榜单 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.查询排名榜单(P.榜单类型, P.起始位置);
		}
	}

	public void 处理封包(查看演武排名 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(刷新演武挑战 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(开始战场演武 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(进入演武战场 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(跨服武道排名 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(登录寄售平台 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
			return;
		}
		发送封包(new 社交错误提示
		{
			错误编号 = 12804
		});
	}

	public void 处理封包(查询平台商品 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			发送封包(new 社交错误提示
			{
				错误编号 = 12804
			});
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询指定商品 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			发送封包(new 社交错误提示
			{
				错误编号 = 12804
			});
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(上架平台商品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
			return;
		}
		发送封包(new 社交错误提示
		{
			错误编号 = 12804
		});
	}

	public void 处理封包(请求珍宝数据 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询珍宝商店(P.数据版本);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(查询出售信息 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.查询出售信息();
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(购买珍宝商品 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.购买珍宝商品(P.物品编号, P.购买数量);
		}
	}

	public void 处理封包(购买每周特惠 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.购买每周特惠(P.礼包编号);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(购买玛法特权 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.购买玛法特权(P.特权类型, P.购买数量);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(预定玛法特权 P)
	{
		if (当前阶段 == 游戏阶段.正在游戏)
		{
			绑定角色.预定玛法特权(P.特权类型);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(领取特权礼包 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定角色.领取特权礼包(P.特权类型, P.礼包位置);
		}
	}

	public void 处理封包(玩家每日签到 P)
	{
		if (当前阶段 != 游戏阶段.正在游戏)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户账号登录 P)
	{
		if (当前阶段 == 游戏阶段.正在登录)
		{
			门票信息 value;
			if (系统数据.数据.网卡封禁.TryGetValue(P.物理地址, out var v) && v > 主程.当前时间)
			{
				尝试断开连接(new Exception("网卡封禁, 限制登录"));
			}
			else if (网络服务网关.门票数据表.TryGetValue(P.登录门票, out value))
			{
				if (!(主程.当前时间 > value.有效时间))
				{
					游戏数据 value2;
					账号数据 账号数据2 = ((!游戏数据网关.账号数据表.检索表.TryGetValue(value.登录账号, out value2) || !(value2 is 账号数据 账号数据)) ? new 账号数据(value.登录账号) : 账号数据);
					if (账号数据2.网络连接 == null)
					{
						账号数据2.账号登录(this, P.物理地址);
					}
					else
					{
						账号数据2.网络连接.发送封包(new 登陆错误提示
						{
							错误代码 = 260u
						});
						账号数据2.网络连接.尝试断开连接(new Exception("账号重复登录, 被踢下线."));
						尝试断开连接(new Exception("账号已经在线, 无法登录."));
					}
				}
				else
				{
					尝试断开连接(new Exception("登录门票已经过期."));
				}
			}
			else
			{
				尝试断开连接(new Exception("登录的门票不存在."));
			}
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		网络服务网关.门票数据表.Remove(P.登录门票);
	}

	public void 处理封包(客户创建角色 P)
	{
		if (当前阶段 != 游戏阶段.选择角色)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定账号.创建角色(this, P);
		}
	}

	public void 处理封包(客户删除角色 P)
	{
		if (当前阶段 == 游戏阶段.选择角色)
		{
			绑定账号.删除角色(this, P);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(彻底删除角色 P)
	{
		if (当前阶段 == 游戏阶段.选择角色)
		{
			绑定账号.永久删除(this, P);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	public void 处理封包(客户进入游戏 P)
	{
		if (当前阶段 != 游戏阶段.选择角色)
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
		else
		{
			绑定账号.进入游戏(this, P);
		}
	}

	public void 处理封包(客户找回角色 P)
	{
		if (当前阶段 == 游戏阶段.选择角色)
		{
			绑定账号.找回角色(this, P);
		}
		else
		{
			尝试断开连接(new Exception($"阶段异常,断开连接.  处理封包: {P.GetType()},  当前阶段:{当前阶段}"));
		}
	}

	static 客户网络()
	{
	}
}
