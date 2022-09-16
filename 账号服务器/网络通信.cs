using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace 账号服务器
{
	public sealed class 网络通信
	{
		public struct 数据封包
		{
			public IPEndPoint 客户地址;

			public byte[] 接收数据;
		}

		public static UdpClient 本地网络服务;

		public static ConcurrentQueue<数据封包> 数据处理队列;

		public static bool 启动服务()
		{
			try
			{
				本地网络服务 = new UdpClient(new IPEndPoint(IPAddress.Any, (ushort)主窗口.主界面.本地监听端口.Value));
				数据处理队列 = new ConcurrentQueue<数据封包>();
				Task.Run(delegate
				{
					while (本地网络服务 != null)
					{
						try
						{
							UdpClient udpClient = 本地网络服务;
							if (udpClient != null && udpClient.Available == 0)
							{
								Thread.Sleep(1);
							}
							else
							{
								数据封包 item = default(数据封包);
								item.接收数据 = 本地网络服务.Receive(ref item.客户地址);
								if (item.接收数据.Length > 1024)
								{
									主窗口.添加日志($"收到过长的封包  地址:{item.客户地址}, 长度:{item.接收数据.Length}");
								}
								else
								{
									数据处理队列.Enqueue(item);
									主窗口.已接收字节数 += item.接收数据.Length;
									主窗口.更新已接收字节数();
								}
							}
						}
						catch (Exception ex3)
						{
							主窗口.添加日志("数据接收错误: " + ex3.Message);
						}
					}
				});
				Task.Run(delegate
				{
					主窗口.添加日志("启动服务成功，等待用户连接...");
					while (本地网络服务 != null)
					{
						try
						{
							if (数据处理队列.IsEmpty || !数据处理队列.TryDequeue(out var result))
							{
								Thread.Sleep(1);
							}
							else
							{
								string[] array = Encoding.UTF8.GetString(result.接收数据, 0, result.接收数据.Length).Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								if (array.Length <= 3 || !int.TryParse(array[0], out var _))
								{
									主窗口.添加日志($"收到错误的封包  地址: {result.客户地址}, 长度: {result.接收数据.Length}");
								}
								else
								{
									switch (array[1])
									{
										case "0":
											if (array.Length == 4)
											{
												if (!主窗口.账号数据.TryGetValue(array[2], out var value3) || array[3] != value3.账号密码)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 1 用户名或密码错误"));
												}
												else
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 0 " + array[2] + " " + array[3] + " " + 主窗口.游戏区服));
													主窗口.添加日志("账号登录成功!  账号: " + array[2]);
												}
											}
											break;
										case "1":
											if (array.Length == 6)
											{
												if (array[2].Length <= 5 || array[2].Length > 12)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 3 用户名长度错误"));
												}
												else if (array[3].Length <= 5 || array[3].Length > 18)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 3 密码长度错误"));
												}
												else if (array[4].Length <= 1 || array[4].Length > 18)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 3 密保问题长度错误"));
												}
												else if (array[5].Length <= 1 || array[5].Length > 18)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 3 密保答案长度错误"));
												}
												else if (!Regex.IsMatch(array[2], "^[a-zA-Z]+.*$"))
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 3 用户名格式错误"));
												}
												else if (!Regex.IsMatch(array[2], "^[a-zA-Z_][A-Za-z0-9_]*$"))
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 3 用户名格式错误"));
												}
												else if (主窗口.账号数据.ContainsKey(array[2]))
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes("3 用户名已经存在"));
												}
												else
												{
													主窗口.添加账号(new 账号数据(array[2], array[3], array[4], array[5]));
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 2 " + array[2] + " " + array[3]));
													主窗口.添加日志("账号注册成功!  账号: " + array[2]);
													主窗口.新注册账号数++;
													主窗口.更新已注册账号数();
												}
											}
											break;
										case "2":
											if (array.Length == 6)
											{
												账号数据 value4;
												if (array[3].Length <= 1 || array[3].Length > 18)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 5 密码长度错误"));
												}
												else if (!主窗口.账号数据.TryGetValue(array[2], out value4))
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 5 账号不存在"));
												}
												else if (array[4] != value4.密保问题)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 5 密保问题错误"));
												}
												else if (array[5] != value4.密保答案)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 5 密保答案错误"));
												}
												else
												{
													value4.账号密码 = array[3];
													主窗口.保存账号(value4);
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 4 " + array[1] + " " + array[2]));
													主窗口.添加日志("密码修改成功!  账号: " + array[1]);
												}
											}
											break;
										case "3":
											if (array.Length == 6)
											{
												IPEndPoint value2;
												if (!主窗口.账号数据.TryGetValue(array[2], out var value) || array[3] != value.账号密码)
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 7 用户名或密码错误"));
												}
												else if (!主窗口.区服数据.TryGetValue(array[4], out value2))
												{
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 7 没有找到服务器"));
												}
												else
												{
													string text = 账号数据.生成门票();
													发送门票(value2, text, array[2]);
													发送数据(result.客户地址, Encoding.UTF8.GetBytes(array[0] + " 6 " + array[2] + " " + array[3] + " " + text));
													主窗口.添加日志("成功生成门票!  账号: " + array[2] + " - " + text);
													主窗口.生成门票总数++;
													主窗口.更新已生成门票数();
												}
											}
											break;
										default:
											主窗口.添加日志($"收到未定义的封包  地址: {result.客户地址}, 长度: {result.接收数据.Length}");
											break;
									}
								}
							}
						}
						catch (Exception ex2)
						{
							主窗口.添加日志("封包处理错误: " + ex2.Message);
						}
					}
					主窗口.添加日志("停止服务成功.");
				});
				return true;
			}
			catch (Exception ex)
			{
				主窗口.添加日志(ex.Message);
				本地网络服务?.Close();
				本地网络服务 = null;
				return false;
			}
		}

		public static void 结束服务()
		{
			本地网络服务?.Close();
			本地网络服务 = null;
		}

		public static void 发送数据(IPEndPoint 地址, byte[] 数据)
		{
			主窗口.已发送字节数 += 数据.Length;
			主窗口.更新已发送字节数();
			if (本地网络服务 != null)
			{
				try
				{
					本地网络服务.Send(数据, 数据.Length, 地址);
				}
				catch (Exception ex)
				{
					主窗口.添加日志("数据发送错误: " + ex.Message);
				}
			}
		}

		public static void 发送门票(IPEndPoint 地址, string 门票, string 账号)
		{
			主窗口.生成门票总数++;
			byte[] bytes = Encoding.UTF8.GetBytes(门票 + ";" + 账号);
			if (本地网络服务 != null)
			{
				try
				{
					本地网络服务.Send(bytes, bytes.Length, new IPEndPoint(地址.Address, (ushort)主窗口.主界面.门票发送端口.Value));
				}
				catch (Exception ex)
				{
					主窗口.添加日志("门票发送失败: " + ex.Message);
				}
			}
		}
	}

}
