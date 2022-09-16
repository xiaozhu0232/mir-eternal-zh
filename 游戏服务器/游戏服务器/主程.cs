using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using 游戏服务器.地图类;
using 游戏服务器.数据类;
using 游戏服务器.网络类;

namespace 游戏服务器;

public static class 主程
{
	public static DateTime 当前时间;

	public static DateTime 每秒计时;

	public static ConcurrentQueue<GM命令> 外部命令;

	public static uint 循环计数;

	public static bool 已经启动;

	public static bool 正在保存;

	public static Thread 主线程;

	public static Random 随机数;

	public static void 启动服务()
	{
		if (!已经启动)
		{
			Thread obj = new Thread(服务循环)
			{
				IsBackground = true
			};
			主线程 = obj;
			obj.Start();
		}
	}

	public static void 停止服务()
	{
		已经启动 = false;
		网络服务网关.结束服务();
	}

	public static void 添加系统日志(string 文本)
	{
		主窗口.添加系统日志(文本);
	}

	public static void 添加聊天日志(string 前缀, byte[] 文本)
	{
		主窗口.添加聊天日志(前缀, 文本);
	}

	private static void 服务循环()
	{
		int num = 0;
		try
		{
			外部命令 = new ConcurrentQueue<GM命令>();
			主窗口.添加系统日志("正在生成地图元素...");
			地图处理网关.开启地图();
			主窗口.添加系统日志("正在启动网络服务...");
			网络服务网关.启动服务();
			主窗口.添加系统日志("服务器已成功开启");
			已经启动 = true;
			主窗口.服务启动回调();
			while (已经启动 || 网络服务网关.网络连接表.Count != 0)
			{
				Thread.Sleep(1);
				当前时间 = DateTime.Now;
				try
				{
					if (当前时间 > 每秒计时)
					{
						游戏数据网关.保存数据();
						主窗口.更新连接总数((uint)网络服务网关.网络连接表.Count);
						主窗口.更新已经登录(网络服务网关.已登录连接数);
						主窗口.更新已经上线(网络服务网关.已上线连接数);
						主窗口.更新发送字节(网络服务网关.已发送字节数);
						主窗口.更新接收字节(网络服务网关.已接收字节数);
						主窗口.更新对象统计(地图处理网关.激活对象表.Count, 地图处理网关.次要对象表.Count, 地图处理网关.地图对象表.Count);
						主窗口.更新后台帧数(循环计数);
						循环计数 = 0u;
						num++;
						每秒计时 = 当前时间.AddSeconds(1.0);
						if (num > 600)
						{
							游戏数据网关.保存数据();
							游戏数据网关.导出数据();
							num = 0;
						}
					}
					else
					{
						循环计数++;
					}
					GM命令 result;
					while (外部命令.TryDequeue(out result))
					{
						result.执行命令();
					}
					网络服务网关.处理数据();
					地图处理网关.处理数据();
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			主窗口.添加系统日志("发生致命错误, 服务器即将停止(主程.启动错误）");
			if (!Directory.Exists(".\\Log\\Error"))
			{
				Directory.CreateDirectory(".\\Log\\Error");
			}
			File.WriteAllText($".\\Log\\Error\\{DateTime.Now:yyyy-MM-dd--HH-mm-ss}.txt", "错误信息:\r\n" + ex.Message + "\r\n堆栈信息:\r\n" + ex.StackTrace);
			主窗口.添加系统日志("错误信息已保存到日志, 请注意查看");
		}
	}

	static 主程()
	{
		当前时间 = DateTime.Now;
		每秒计时 = DateTime.Now.AddSeconds(1.0);
		随机数 = new Random();
	}
}
