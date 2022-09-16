using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 游戏登录器
{
	
	public sealed class 网络通信
	{
		public static UdpClient 通信实例;
		public static IPEndPoint 连接地址;
		public static ConcurrentQueue<byte[]> 接收队列;

		public static void 开始通信()
		{
			通信实例 = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
			接收队列 = new ConcurrentQueue<byte[]>();
			Task.Run(delegate
			{
				while (通信实例 != null)
				{
					try
					{
						IPEndPoint remoteEP = null;
						byte[] item = 通信实例.Receive(ref remoteEP);
						接收队列.Enqueue(item);
					}
					catch (Exception ex)
					{
						if (ex is SocketException ex2 && ex2.ErrorCode == 10054)
						{
							MessageBox.Show("服务器连接失败");
						}
						Environment.Exit(Environment.ExitCode);
					}
				}
			});
		}

		public static void 停止通信()
		{
			通信实例.Close();
			通信实例 = null;
		}

		public static bool 发送数据(byte[] 数据)
		{
			if (通信实例 != null)
			{
				try
				{
					通信实例.Send(数据, 数据.Length, 连接地址);
					return true;
				}
				catch
				{
					MessageBox.Show("连接服务器失败");
					return false;
				}
			}
			return false;
		}
	
	}
}
