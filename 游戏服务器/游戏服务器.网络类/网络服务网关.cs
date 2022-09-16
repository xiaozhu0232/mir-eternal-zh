using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using 游戏服务器.数据类;

namespace 游戏服务器.网络类;

public static class 网络服务网关
{
	private static IPEndPoint 门票发送端;

	private static UdpClient 门票接收器;

	private static TcpListener 网络监听器;

	public static bool 网络服务停止;

	public static bool 未登录连接数;

	public static uint 已登录连接数;

	public static uint 已上线连接数;

	public static long 已发送字节数;

	public static long 已接收字节数;

	public static HashSet<客户网络> 网络连接表;

	public static ConcurrentQueue<客户网络> 等待移除表;

	public static ConcurrentQueue<客户网络> 等待添加表;

	public static ConcurrentQueue<游戏封包> 全服公告表;

	public static Dictionary<string, 门票信息> 门票数据表;

	public static void 启动服务()
	{
		网络服务停止 = false;
		网络连接表 = new HashSet<客户网络>();
		等待添加表 = new ConcurrentQueue<客户网络>();
		等待移除表 = new ConcurrentQueue<客户网络>();
		全服公告表 = new ConcurrentQueue<游戏封包>();
		网络监听器 = new TcpListener(IPAddress.Any, 自定义类.客户连接端口);
		网络监听器.Start();
		网络监听器.BeginAcceptTcpClient(异步连接, null);
		门票数据表 = new Dictionary<string, 门票信息>();
		门票接收器 = new UdpClient(new IPEndPoint(IPAddress.Any, 自定义类.门票接收端口));
	}

	public static void 结束服务()
	{
		网络服务停止 = true;
		网络监听器?.Stop();
		网络监听器 = null;
		门票接收器?.Close();
		门票接收器 = null;
	}

	public static void 处理数据()
	{
		try
		{
			try
			{
				while (true)
				{
					UdpClient udpClient = 门票接收器;
					if (udpClient != null && udpClient.Available != 0)
					{
						byte[] bytes = 门票接收器.Receive(ref 门票发送端);
						string[] array = Encoding.UTF8.GetString(bytes).Split(';');
						if (array.Length == 2)
						{
							门票数据表[array[0]] = new 门票信息
							{
								登录账号 = array[1],
								有效时间 = 主程.当前时间.AddMinutes(5.0)
							};
						}
						continue;
					}
					break;
				}
			}
			catch (Exception ex)
			{
				主程.添加系统日志("接收登录门票时发生错误. " + ex.Message);
			}
			foreach (客户网络 item in 网络连接表)
			{
				if (item.正在断开 || item.绑定账号 != null || !(主程.当前时间.Subtract(item.接入时间).TotalSeconds > 30.0))
				{
					item.处理数据();
				}
				else
				{
					item.尝试断开连接(new Exception("登录超时, 断开连接!"));
				}
			}
			while (!等待移除表.IsEmpty)
			{
				if (等待移除表.TryDequeue(out var result))
				{
					网络连接表.Remove(result);
				}
			}
			while (!等待添加表.IsEmpty)
			{
				if (等待添加表.TryDequeue(out var result2))
				{
					网络连接表.Add(result2);
				}
			}
			while (!全服公告表.IsEmpty)
			{
				if (!全服公告表.TryDequeue(out var result3))
				{
					continue;
				}
				foreach (客户网络 item2 in 网络连接表)
				{
					if (item2.绑定角色 != null)
					{
						item2.发送封包(result3);
					}
				}
			}
		}
		catch
		{
			主窗口.添加系统日志("发生致命错误, 服务器即将停止1");
		}
	}

	public static void 异步连接(IAsyncResult 异步参数)
	{
		try
		{
			if (网络服务停止)
			{
				return;
			}
			TcpClient tcpClient = 网络监听器.EndAcceptTcpClient(异步参数);
			string text = tcpClient.Client.RemoteEndPoint.ToString().Split(':')[0];
			if (!系统数据.数据.网络封禁.ContainsKey(text) || 系统数据.数据.网络封禁[text] < 主程.当前时间)
			{
				if (网络连接表.Count < 65535)
				{
					等待添加表?.Enqueue(new 客户网络(tcpClient));
				}
			}
			else
			{
				tcpClient.Client.Close();
			}
		}
		catch (Exception ex)
		{
			主程.添加系统日志("异步连接异常: " + ex.ToString());
		}
		while (!网络服务停止 && 网络连接表.Count > 100)
		{
			Thread.Sleep(1);
		}
		if (!网络服务停止)
		{
			网络监听器.BeginAcceptTcpClient(异步连接, null);
		}
	}

	public static void 断网回调(object sender, Exception e)
	{
		客户网络 客户网络2 = sender as 客户网络;
		string text = "IP: " + 客户网络2.网络地址;
		if (客户网络2.绑定账号 != null)
		{
			text = text + " 账号: " + 客户网络2.绑定账号.账号名字.V;
		}
		if (客户网络2.绑定角色 != null)
		{
			text = text + " 角色: " + 客户网络2.绑定角色.对象名字;
		}
		text = text + " 信息: " + e.Message;
		主程.添加系统日志(text);
	}

	public static void 屏蔽网络(string 地址)
	{
		系统数据.数据.封禁网络(地址, 主程.当前时间.AddMinutes((int)自定义类.异常屏蔽时间));
	}

	public static void 发送公告(string 内容, bool 滚动播报 = false)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(0);
			binaryWriter.Write(滚动播报 ? 2415919106u : 2415919107u);
			binaryWriter.Write((!滚动播报) ? 3 : 2);
			binaryWriter.Write(0);
			binaryWriter.Write(Encoding.UTF8.GetBytes(内容 + "\0"));
			发送封包(new 接收聊天消息
			{
				字节描述 = memoryStream.ToArray()
			});
		}
		主窗口.添加系统日志(内容);
	}

	public static void 发送封包(游戏封包 封包)
	{
		if (封包 != null)
		{
			全服公告表?.Enqueue(封包);
		}
	}

	public static void 添加网络(客户网络 网络)
	{
		if (网络 != null)
		{
			等待添加表.Enqueue(网络);
		}
	}

	public static void 移除网络(客户网络 网络)
	{
		if (网络 != null)
		{
			等待移除表.Enqueue(网络);
		}
	}

	static 网络服务网关()
	{
	}
}
