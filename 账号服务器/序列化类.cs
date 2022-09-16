using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace 账号服务器
{
	public static class 序列化类
	{
		private static readonly JsonSerializerSettings 全局设置;

		static 序列化类()
		{
			全局设置 = new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Auto,
				Formatting = Formatting.Indented
			};
		}

		public static string 序列化(object O)
		{
			return JsonConvert.SerializeObject(O, 全局设置);
		}

		public static object[] 反序列化(string 文件夹, Type 类型)
		{
			List<object> list = new List<object>();
			FileInfo[] files = new DirectoryInfo(文件夹).GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				object obj = JsonConvert.DeserializeObject(File.ReadAllText(files[i].FullName), 类型, 全局设置);
				if (obj != null)
				{
					list.Add(obj);
				}
			}
			return list.ToArray();
		}
	}
}
